using System.Collections.Generic;
using Shababeek.Interactions.Shababeek.Interactions;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Multi-cell Tetris-style inventory socket. Packs rectangular item footprints
    /// into a 2D grid using <see cref="GridPacker"/>. Shows a hover preview anchor
    /// while a held item hovers the grid, scales items down to fit their footprint
    /// area on insert (via <see cref="SocketScaleFitter"/>), and restores scale on
    /// remove. Designed for backpack-style inventories.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Inventory Grid Socket")]
    public class InventoryGridSocket : AbstractSocket
    {
        // ================================================================
        // Serialized Config
        // ================================================================

        [Header("Grid Layout")]
        [Tooltip("Number of columns (X) and rows (Y).")]
        [SerializeField] private Vector2Int gridSize = new Vector2Int(6, 6);

        [Tooltip("World-space size of one cell, in meters (X,Y).")]
        [SerializeField] private Vector2 cellWorldSize = new Vector2(0.05f, 0.05f);

        [Tooltip("Plane the grid lays on.")]
        [SerializeField] private LocalDirection gridPlane = LocalDirection.Forward;

        [Tooltip("Local offset of the grid center.")]
        [SerializeField] private Vector3 localOffset = Vector3.zero;

        [Tooltip("Center the grid at the offset.")]
        [SerializeField] private bool centerGrid = true;

        [Tooltip("Rotation offset for all cell pivots.")]
        [SerializeField] private Vector3 pivotRotationOffset = Vector3.zero;

        [Header("Placement")]
        [Tooltip("Local pos offset (relative to anchor cell) applied to occupants.")]
        [SerializeField] private Vector3 placementPositionOffset = Vector3.zero;

        [Tooltip("Local rot offset applied to occupants.")]
        [SerializeField] private Vector3 placementRotationOffset = Vector3.zero;

        [Header("Scale")]
        [Tooltip("Scale items down to fit their footprint cells on insert.")]
        [SerializeField] private bool scaleToFit = true;

        [Tooltip("Fit items to this fraction of cell size (0-1) so they don't touch edges.")]
        [SerializeField, Range(0f, 1f)] private float scalePadding = 0.9f;

        [Header("Events")]
        [Tooltip("Invoked after any insert/remove. Canvas redraws occupancy colors.")]
        [SerializeField] private UnityEvent onGridChanged = new();

        // Hover-changed event uses Nullable<Vector2Int> which Unity cannot serialize,
        // so this is runtime-only (no [SerializeField]).
        private readonly UnityEvent<Vector2Int?> _onHoverChanged = new UnityEvent<Vector2Int?>();

        // ================================================================
        // Runtime State
        // ================================================================

        private GridPacker _packer;
        private Transform[,] _cellPivots;
        private readonly Dictionary<Socketable, (Vector2Int anchor, Vector2Int footprint)> _placements = new();
        private readonly Dictionary<Socketable, Transform> _carriers = new();
        private readonly Dictionary<Socketable, bool> _wasKinematic = new();
        private readonly Dictionary<Socketable, bool> _wasColliderEnabled = new();
        // Scale to restore on remove, captured BEFORE scale-to-fit shrinks the item.
        // Reading current localScale on remove would return the shrunk value for
        // items without a GridItem, leaving them permanently small.
        private readonly Dictionary<Socketable, Vector3> _restoreScale = new();
        private Transform _gridRoot;

        private Socketable _hovering;
        private Vector2Int? _hoverAnchor;

        // ================================================================
        // Public State (for T5 CanvasController)
        // ================================================================

        public Vector2Int GridSize => gridSize;
        public Socketable Hovering => _hovering;
        public Vector2Int? HoverAnchor => _hoverAnchor;

        /// <summary>The footprint of the hovering item (GridItem.Footprint or 1x1 fallback). 0x0 if not hovering.</summary>
        public Vector2Int HoverFootprint => _hovering != null ? GetFootprint(_hovering) : Vector2Int.zero;

        public bool IsCellOccupied(int x, int y) => _packer != null && _packer.IsCellOccupied(x, y);

        /// <summary>
        /// Returns the stored item whose footprint covers the grid cell under <paramref name="worldPos"/>,
        /// or null if that cell is empty. Multi-cell items match when any of their cells is under the point.
        /// Used by the backpack grab to retrieve the item the hand is hovering over.
        /// </summary>
        public Socketable GetItemAtWorldPosition(Vector3 worldPos)
        {
            if (_packer == null) return null;
            var cell = WorldToCell(worldPos);
            foreach (var kvp in _placements)
            {
                var anchor = kvp.Value.anchor;
                var fp = kvp.Value.footprint;
                if (cell.x >= anchor.x && cell.x < anchor.x + fp.x &&
                    cell.y >= anchor.y && cell.y < anchor.y + fp.y)
                    return kvp.Key;
            }
            return null;
        }

        /// <summary>Returns world position of a cell's center (for Canvas highlights).</summary>
        public Vector3 GetCellWorldCenter(int x, int y)
        {
            if (_cellPivots != null &&
                x >= 0 && x < gridSize.x &&
                y >= 0 && y < gridSize.y &&
                _cellPivots[x, y] != null)
            {
                return _cellPivots[x, y].position;
            }

            // Fallback: compute via gridRoot transform.
            if (_gridRoot != null)
                return _gridRoot.TransformPoint(CalculateCellLocalPosition(x, y));

            return transform.position;
        }

        public Vector2 CellWorldSize => cellWorldSize;
        public LocalDirection GridPlane => gridPlane;

        /// <summary>Local offset of the grid center (for the Canvas overlay to match).</summary>
        public Vector3 GridLocalOffset => localOffset;

        /// <summary>
        /// Pivot-local rotation that maps the grid's in-plane X/Y axes onto the
        /// configured <see cref="gridPlane"/>. The Canvas overlay uses this so its
        /// flat XY quad lies in the same plane as the cell pivots and gizmos.
        /// </summary>
        public Quaternion GridPlaneRotation
        {
            get
            {
                Vector3 xAxis, yAxis;
                switch (gridPlane)
                {
                    case LocalDirection.Back:  xAxis = new Vector3(-1, 0, 0); yAxis = new Vector3(0, 1, 0);  break;
                    case LocalDirection.Right: xAxis = new Vector3(0, 0, -1); yAxis = new Vector3(0, 1, 0);  break;
                    case LocalDirection.Left:  xAxis = new Vector3(0, 0, 1);  yAxis = new Vector3(0, 1, 0);  break;
                    case LocalDirection.Up:    xAxis = new Vector3(1, 0, 0);  yAxis = new Vector3(0, 0, -1); break;
                    case LocalDirection.Down:  xAxis = new Vector3(1, 0, 0);  yAxis = new Vector3(0, 0, 1);  break;
                    default:                   xAxis = new Vector3(1, 0, 0);  yAxis = new Vector3(0, 1, 0);  break;
                }
                var forward = Vector3.Cross(xAxis, yAxis);
                return Quaternion.LookRotation(forward, yAxis);
            }
        }
        public UnityEvent OnGridChanged => onGridChanged;
        public UnityEvent<Vector2Int?> OnHoverChanged => _onHoverChanged;

        // ================================================================
        // Lifecycle
        // ================================================================

        private void Awake()
        {
            BuildGridRoot();
            BuildCellPivots();
            _packer = new GridPacker(gridSize.x, gridSize.y);
        }

        private void Update()
        {
            SyncGridRootScale();
            if (_hovering != null) RefreshHoverAnchor(_hovering);
        }

        private void OnValidate()
        {
            gridSize.x = Mathf.Max(1, gridSize.x);
            gridSize.y = Mathf.Max(1, gridSize.y);
            cellWorldSize.x = Mathf.Max(0.001f, cellWorldSize.x);
            cellWorldSize.y = Mathf.Max(0.001f, cellWorldSize.y);
        }

        // ================================================================
        // Grid Construction
        // ================================================================

        private void BuildGridRoot()
        {
            var go = new GameObject("GridRoot");
            _gridRoot = go.transform;
            _gridRoot.SetParent(Pivot, false);
            _gridRoot.localPosition = localOffset;
            _gridRoot.localRotation = Quaternion.identity;
            SyncGridRootScale();
        }

        /// <summary>
        /// Compensates for the socket transform's lossy scale so cell positions
        /// remain at the authored cellWorldSize regardless of parent scaling.
        /// Copied from ShiftingRackSocket.SyncRackRootScale.
        /// </summary>
        private void SyncGridRootScale()
        {
            if (_gridRoot == null) return;
            var s = Pivot.lossyScale;
            _gridRoot.localScale = new Vector3(
                Mathf.Approximately(s.x, 0f) ? 1f : 1f / s.x,
                Mathf.Approximately(s.y, 0f) ? 1f : 1f / s.y,
                Mathf.Approximately(s.z, 0f) ? 1f : 1f / s.z);
        }

        private void BuildCellPivots()
        {
            _cellPivots = new Transform[gridSize.x, gridSize.y];
            var rot = Quaternion.Euler(pivotRotationOffset);

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    var pivot = new GameObject($"Cell_{x}_{y}").transform;
                    pivot.SetParent(_gridRoot, false);
                    pivot.localPosition = CalculateCellLocalPosition(x, y);
                    pivot.localRotation = rot;
                    _cellPivots[x, y] = pivot;
                }
            }
        }

        /// <summary>
        /// Local-space position of a cell center within the grid root.
        /// Geometry adapted from GridMultiSocket.CalculateGridPosition.
        /// </summary>
        private Vector3 CalculateCellLocalPosition(int x, int y)
        {
            return MapGridToLocal(x, y);
        }

        /// <summary>Maps grid coordinates (can be fractional) to local-space Vector3.</summary>
        private Vector3 MapGridToLocal(float gridX, float gridY)
        {
            var centerOffset = centerGrid
                ? new Vector2(
                    (gridSize.x - 1) * cellWorldSize.x * 0.5f,
                    (gridSize.y - 1) * cellWorldSize.y * 0.5f)
                : Vector2.zero;

            var gridPos = new Vector2(
                gridX * cellWorldSize.x - centerOffset.x,
                gridY * cellWorldSize.y - centerOffset.y);

            return MapPlaneVector(gridPos);
        }

        /// <summary>
        /// Projects a 2D grid-plane position onto the appropriate local axes.
        /// Copied from GridMultiSocket plane switch.
        /// </summary>
        private Vector3 MapPlaneVector(Vector2 gridPos)
        {
            return gridPlane switch
            {
                LocalDirection.Forward => new Vector3(gridPos.x, gridPos.y, 0),
                LocalDirection.Back => new Vector3(-gridPos.x, gridPos.y, 0),
                LocalDirection.Right => new Vector3(0, gridPos.y, -gridPos.x),
                LocalDirection.Left => new Vector3(0, gridPos.y, gridPos.x),
                LocalDirection.Up => new Vector3(gridPos.x, 0, -gridPos.y),
                LocalDirection.Down => new Vector3(gridPos.x, 0, gridPos.y),
                _ => new Vector3(gridPos.x, gridPos.y, 0)
            };
        }

        // ================================================================
        // AbstractSocket Overrides
        // ================================================================

        public override bool CanSocket()
        {
            return _packer != null && !_packer.IsFull;
        }

        public override bool CanSocket(Socketable socketable)
        {
            if (!base.CanSocket(socketable)) return false;
            if (socketable == null) return false;

            // Prevent self-socketing — a backpack with both Socketable and
            // InventoryGridSocket would otherwise insert into its own grid.
            if (socketable.gameObject == gameObject) return false;

            var fp = GetFootprint(socketable);
            return _packer.FitsAnywhere(fp);
        }

        public override void StartHovering(Socketable socketable)
        {
            base.StartHovering(socketable);
            if (!CanSocket()) return;
            _hovering = socketable;
            RefreshHoverAnchor(socketable, silent: true);
            _onHoverChanged.Invoke(_hoverAnchor);
        }

        public override void EndHovering(Socketable socketable)
        {
            base.EndHovering(socketable);
            if (_hovering != socketable) return;
            ClearHover();
        }

        public override (Vector3 position, Quaternion rotation) GetPivotForSocketable(Socketable socketable)
        {
            AdoptAsHoverIfNeeded(socketable);

            if (_hovering == socketable && _hoverAnchor.HasValue)
            {
                RefreshHoverAnchor(socketable);
                return GetPlacementPose(_hoverAnchor.Value, GetFootprint(socketable), GetFootprintData(socketable));
            }

            // Fallback: find nearest valid anchor.
            if (_packer != null)
            {
                var fp = GetFootprint(socketable);
                var cell = WorldToCell(socketable.transform.position);
                var anchor = _packer.FindNearestValidAnchor(fp, cell);
                if (anchor.HasValue)
                    return GetPlacementPose(anchor.Value, fp, GetFootprintData(socketable));
            }

            return (Pivot.position, Pivot.rotation);
        }

        internal override Transform Insert(Socketable socketable)
        {
            AdoptAsHoverIfNeeded(socketable);

            var fp = GetFootprint(socketable);

            // Determine anchor: use hover anchor if hovering, else nearest valid.
            Vector2Int anchor;
            if (_hovering == socketable && _hoverAnchor.HasValue)
            {
                anchor = _hoverAnchor.Value;
            }
            else
            {
                var found = _packer.FindNearestValidAnchor(fp, WorldToCell(socketable.transform.position));
                if (!found.HasValue) return null;
                anchor = found.Value;
            }

            // Reserve cells.
            if (!_packer.TryReserve(anchor, fp)) return null;

            // Create carrier under gridRoot (scale-compensated).
            var carrier = new GameObject($"Carrier_{socketable.name}").transform;
            carrier.SetParent(_gridRoot, false);
            var placement = GetPlacementPose(anchor, fp, GetFootprintData(socketable));
            carrier.SetPositionAndRotation(placement.position, placement.rotation);

            // Track placement.
            _placements[socketable] = (anchor, fp);
            _carriers[socketable] = carrier;

            // Scale to fit. Capture the pre-scale value so Remove can restore it —
            // for items without a GridItem this is the only record of the original
            // scale (reading localScale on remove would return the shrunk value).
            if (scaleToFit)
            {
                var gridItem = socketable.GetComponent<GridItem>();
                var defaultScale = gridItem != null
                    ? gridItem.DefaultScale
                    : socketable.transform.localScale;
                _restoreScale[socketable] = defaultScale;

                var targetSize = cellWorldSize * fp;
                var fitScale = SocketScaleFitter.ComputeFitScale(
                    socketable.transform, targetSize, defaultScale, scalePadding);
                socketable.transform.localScale = fitScale;
            }

            // Rigidbody kinematic + collider disabled.
            var rb = socketable.GetComponent<Rigidbody>();
            if (rb != null)
            {
                _wasKinematic[socketable] = rb.isKinematic;
                rb.isKinematic = true;
            }

            var col = socketable.GetComponent<Collider>();
            if (col != null)
            {
                _wasColliderEnabled[socketable] = col.enabled;
                col.enabled = false;
            }

            ClearHover();
            onGridChanged.Invoke();
            base.Insert(socketable);
            return carrier;
        }

        public override void Remove(Socketable socketable)
        {
            // Restore scale — only if scale-to-fit actually changed it on insert.
            if (_restoreScale.TryGetValue(socketable, out var restoreScale))
            {
                socketable.transform.localScale = restoreScale;
                _restoreScale.Remove(socketable);
            }

            // A hand may be grabbing the item OUT of the grid (Remove fires from the grab's
            // OnSelected, so IsSelected is already true here). In that case the Grabable owns
            // the Rigidbody — it forced kinematic for the hold and restores physics on release —
            // so don't fight it: keep it kinematic and hand the grabber the correct pre-socket
            // baseline so its release thaws to the real (dynamic) state instead of the frozen one.
            var grabbable = socketable.GetComponent<Grabable>();
            bool grabbedOut = grabbable != null && grabbable.IsSelected;

            // Restore Rigidbody.
            if (_wasKinematic.TryGetValue(socketable, out var wasKinematic))
            {
                var rb = socketable.GetComponent<Rigidbody>();
                if (grabbedOut)
                {
                    if (rb != null) rb.isKinematic = true; // stay kinematic while on the hand
                    grabbable.SetReleaseKinematic(wasKinematic);
                }
                else if (rb != null)
                {
                    rb.isKinematic = wasKinematic;
                }
                _wasKinematic.Remove(socketable);
            }

            // Restore Collider (a held item still needs to collide/interact).
            if (_wasColliderEnabled.TryGetValue(socketable, out var wasColliderEnabled))
            {
                var col = socketable.GetComponent<Collider>();
                if (col != null) col.enabled = wasColliderEnabled;
                _wasColliderEnabled.Remove(socketable);
            }

            // Detach from carrier.
            if (_carriers.TryGetValue(socketable, out var carrier))
            {
                if (socketable.transform.parent == carrier)
                    socketable.transform.SetParent(null, true);
                if (carrier != null) Destroy(carrier.gameObject);
                _carriers.Remove(socketable);
            }

            // Release cells.
            if (_placements.TryGetValue(socketable, out var placement))
            {
                _packer.Release(placement.anchor, placement.footprint);
                _placements.Remove(socketable);
            }

            if (_hovering == socketable) ClearHover();
            onGridChanged.Invoke();
            base.Remove(socketable);
        }

        // ================================================================
        // Hover Logic
        // ================================================================

        /// <summary>
        /// When an item grabbed FROM the grid is still this socket's CurrentSocket,
        /// StartHovering won't re-fire (DetectSockets only fires on closest-socket
        /// change). Adopt the hovering state on demand so preview + insert work.
        /// Copied from ShiftingRackSocket.AdoptAsHoverIfNeeded pattern.
        /// </summary>
        private void AdoptAsHoverIfNeeded(Socketable socketable)
        {
            if (socketable == null || _hovering == socketable) return;
            if (_hovering != null) return;
            if (socketable.IsSocketed) return;
            if (socketable.CurrentSocket != this) return;
            if (!CanSocket() || !CanSocket(socketable)) return;

            _hovering = socketable;
            RefreshHoverAnchor(socketable, silent: true);
            _onHoverChanged.Invoke(_hoverAnchor);
        }

        private void RefreshHoverAnchor(Socketable socketable, bool silent = false)
        {
            if (_packer == null) return;

            var cell = WorldToCell(socketable.transform.position);
            var fp = GetFootprint(socketable);
            var newAnchor = _packer.FindNearestValidAnchor(fp, cell);

            if (newAnchor != _hoverAnchor)
            {
                _hoverAnchor = newAnchor;
                if (!silent) _onHoverChanged.Invoke(_hoverAnchor);
            }
        }

        private void ClearHover()
        {
            _hovering = null;
            _hoverAnchor = null;
            _onHoverChanged.Invoke(null);
        }

        // ================================================================
        // Coordinate Conversion
        // ================================================================

        /// <summary>
        /// Converts a world-space position to grid cell coordinates by projecting
        /// onto the grid plane and reversing the center offset.
        /// </summary>
        private Vector2Int WorldToCell(Vector3 worldPos)
        {
            if (_gridRoot == null) return Vector2Int.zero;

            var localPos = _gridRoot.InverseTransformPoint(worldPos);

            float inPlaneX, inPlaneY;
            switch (gridPlane)
            {
                case LocalDirection.Back:
                    inPlaneX = -localPos.x;
                    inPlaneY = localPos.y;
                    break;
                case LocalDirection.Right:
                    inPlaneX = -localPos.z;
                    inPlaneY = localPos.y;
                    break;
                case LocalDirection.Left:
                    inPlaneX = localPos.z;
                    inPlaneY = localPos.y;
                    break;
                case LocalDirection.Up:
                    inPlaneX = localPos.x;
                    inPlaneY = -localPos.z;
                    break;
                case LocalDirection.Down:
                    inPlaneX = localPos.x;
                    inPlaneY = localPos.z;
                    break;
                default: // Forward
                    inPlaneX = localPos.x;
                    inPlaneY = localPos.y;
                    break;
            }

            float centerOffsetX = centerGrid ? (gridSize.x - 1) * 0.5f : 0f;
            float centerOffsetY = centerGrid ? (gridSize.y - 1) * 0.5f : 0f;

            float cellX = inPlaneX / cellWorldSize.x + centerOffsetX;
            float cellY = inPlaneY / cellWorldSize.y + centerOffsetY;

            int cx = Mathf.Clamp(Mathf.RoundToInt(cellX), 0, gridSize.x - 1);
            int cy = Mathf.Clamp(Mathf.RoundToInt(cellY), 0, gridSize.y - 1);

            return new Vector2Int(cx, cy);
        }

        /// <summary>
        /// Computes the world-space placement pose (center of the footprint region)
        /// including both the socket's global offsets and the item's per-footprint
        /// position/rotation offsets (<see cref="ItemFootprint"/>).
        /// </summary>
        private (Vector3 position, Quaternion rotation) GetPlacementPose(
            Vector2Int anchor, Vector2Int footprint, ItemFootprint data = null)
        {
            // Center of the footprint region in grid-local space.
            float centerX = anchor.x + (footprint.x - 1) * 0.5f;
            float centerY = anchor.y + (footprint.y - 1) * 0.5f;
            var centerLocal = MapGridToLocal(centerX, centerY);

            // Per-item offsets from the footprint asset (default to none).
            var itemPosOffset = data != null ? data.PlacementOffset : Vector3.zero;
            var itemRotOffset = data != null ? Quaternion.Euler(data.PlacementRotation) : Quaternion.identity;

            // Apply placement offsets relative to the cell pivot rotation.
            // Order: global offset first, then per-item, so per-item rotation is
            // applied in the item's own local frame (innermost).
            var cellRotLocal = Quaternion.Euler(pivotRotationOffset);
            var placementPosLocal = centerLocal + cellRotLocal * (placementPositionOffset + itemPosOffset);
            var placementRotLocal = cellRotLocal * Quaternion.Euler(placementRotationOffset) * itemRotOffset;

            // Transform grid-local to world.
            var worldPos = _gridRoot.TransformPoint(placementPosLocal);
            var worldRot = _gridRoot.rotation * placementRotLocal;

            return (worldPos, worldRot);
        }

        /// <summary>Gets the ItemFootprint asset for a socketable (null when no GridItem/asset).</summary>
        private ItemFootprint GetFootprintData(Socketable s)
        {
            if (s == null) return null;
            return s.GetComponent<GridItem>()?.Data;
        }

        // ================================================================
        // Helpers
        // ================================================================

        /// <summary>Gets the grid footprint of a socketable (1x1 fallback when no GridItem).</summary>
        private Vector2Int GetFootprint(Socketable s)
        {
            if (s == null) return Vector2Int.one;
            return s.GetComponent<GridItem>()?.Footprint ?? Vector2Int.one;
        }

        // ================================================================
        // Gizmos
        // ================================================================

        private void OnDrawGizmos()
        {
            DrawGrid(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGrid(true);
        }

        private void DrawGrid(bool selected)
        {
            var oldMatrix = Gizmos.matrix;
            // Match runtime: grid root compensates for transform scale, so gizmos
            // use identity scale and apply localOffset manually.
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            var centerOffset = centerGrid
                ? new Vector2(
                    (gridSize.x - 1) * cellWorldSize.x * 0.5f,
                    (gridSize.y - 1) * cellWorldSize.y * 0.5f)
                : Vector2.zero;

            float sphereRadius = Mathf.Min(cellWorldSize.x, cellWorldSize.y) * 0.25f;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    var gridPos = new Vector2(
                        x * cellWorldSize.x - centerOffset.x,
                        y * cellWorldSize.y - centerOffset.y);

                    var localPos = localOffset + MapPlaneVector(gridPos);

                    bool occupied = false;
                    if (Application.isPlaying && _packer != null)
                        occupied = _packer.IsCellOccupied(x, y);

                    if (occupied)
                        Gizmos.color = new Color(1f, 0.2f, 0.2f, selected ? 0.9f : 0.5f);
                    else
                        Gizmos.color = selected ? Color.cyan : new Color(0f, 1f, 1f, 0.5f);

                    Gizmos.DrawWireSphere(localPos, sphereRadius);
                }
            }

            // Hover footprint highlight (green).
            if (Application.isPlaying && _hovering != null && _hoverAnchor.HasValue)
            {
                var fp = GetFootprint(_hovering);
                var anchor = _hoverAnchor.Value;
                Gizmos.color = new Color(0f, 1f, 0f, 0.4f);

                for (int dx = 0; dx < fp.x; dx++)
                {
                    for (int dy = 0; dy < fp.y; dy++)
                    {
                        int cx = anchor.x + dx;
                        int cy = anchor.y + dy;
                        if (cx < 0 || cx >= gridSize.x || cy < 0 || cy >= gridSize.y) continue;

                        var gridPos = new Vector2(
                            cx * cellWorldSize.x - centerOffset.x,
                            cy * cellWorldSize.y - centerOffset.y);
                        var localPos = localOffset + MapPlaneVector(gridPos);
                        Gizmos.DrawCube(localPos,
                            new Vector3(cellWorldSize.x, cellWorldSize.y, 0.01f));
                    }
                }
            }

            Gizmos.matrix = oldMatrix;
        }
    }
}

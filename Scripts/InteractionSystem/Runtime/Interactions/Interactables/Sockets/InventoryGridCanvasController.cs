using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Drives a world-space Canvas overlay on top of an <see cref="InventoryGridSocket"/>.
    /// Draws one colored square per grid cell: free cells are dim cyan, occupied cells
    /// are tinted red, and the current hover footprint is highlighted green (valid
    /// placement) or the whole grid turns faint red when the item cannot fit anywhere
    /// (blocked / no valid anchor). The Canvas is pure visual — non-interactable,
    /// Ignore-Raycast layer, no GraphicRaycaster — so it never interferes with
    /// <see cref="Socketable"/> socket detection (G4 compliance).
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Inventory Grid Canvas Controller")]
    [RequireComponent(typeof(InventoryGridSocket))]
    public class InventoryGridCanvasController : MonoBehaviour
    {
        // ================================================================
        // Serialized Config
        // ================================================================

        [Header("Sprite")]
        [Tooltip("Sprite used for every cell Image. Assign a white square (e.g. UISprite). Falls back to runtime white sprite if unassigned.")]
        [SerializeField] private Sprite cellSprite;

        [Header("Colors")]
        [Tooltip("Color for free (empty) cells.")]
        [SerializeField] private Color freeColor = new Color(0f, 1f, 1f, 0.15f);

        [Tooltip("Color for occupied cells.")]
        [SerializeField] private Color occupiedColor = new Color(1f, 0.3f, 0.3f, 0.3f);

        [Tooltip("Color for valid hover footprint cells.")]
        [SerializeField] private Color hoverValidColor = new Color(0f, 1f, 0f, 0.5f);

        [Tooltip("Color tint when item doesn't fit anywhere (blocked).")]
        [SerializeField] private Color hoverBlockedColor = new Color(1f, 0f, 0f, 0.2f);

        // ================================================================
        // Runtime State
        // ================================================================

        private InventoryGridSocket _socket;
        private Canvas _canvas;
        private Image[,] _cells; // [x, y] grid of cell images
        private bool _canvasCreatedHere;

        // Lazily-created fallback white sprite (used when cellSprite is unassigned).
        private static Sprite _fallbackSprite;
        private static Sprite FallbackSprite => _fallbackSprite ??= Sprite.Create(
            Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 100);

        private Sprite ResolvedSprite => cellSprite != null ? cellSprite : FallbackSprite;

        /// <summary>The Canvas driven by this controller (test access).</summary>
        internal Canvas Canvas => _canvas;

        /// <summary>The cell Image at grid coordinate (x, y) (test access).</summary>
        internal Image GetCellImage(int x, int y) => _cells[x, y];

        // ================================================================
        // Lifecycle
        // ================================================================

        private void Awake()
        {
            _socket = GetComponent<InventoryGridSocket>();
            BuildCanvas();
            BuildCells();
        }

        private void OnEnable()
        {
            if (_socket == null) return;
            _socket.OnGridChanged.AddListener(RefreshAll);
            _socket.OnHoverChanged.AddListener(OnHoverChanged);
        }

        private void OnDisable()
        {
            if (_socket == null) return;
            _socket.OnGridChanged.RemoveListener(RefreshAll);
            _socket.OnHoverChanged.RemoveListener(OnHoverChanged);
        }

        private void OnDestroy()
        {
            if (_socket != null)
            {
                _socket.OnGridChanged.RemoveListener(RefreshAll);
                _socket.OnHoverChanged.RemoveListener(OnHoverChanged);
            }

            if (_canvasCreatedHere && _canvas != null)
                Destroy(_canvas.gameObject);
        }

        // ================================================================
        // Canvas Construction (G4 compliant — non-interactable)
        // ================================================================

        private void BuildCanvas()
        {
            var size = _socket.GridSize;
            var cellSize = _socket.CellWorldSize;
            float canvasWidth = size.x * cellSize.x;
            float canvasHeight = size.y * cellSize.y;

            var canvasGo = new GameObject("GridCanvas");
            canvasGo.transform.SetParent(_socket.Pivot, false);
            // Match the grid's plane + offset so the overlay lies on the same plane
            // as the cell pivots/gizmos (not flat on the Pivot's local XY plane).
            canvasGo.transform.localPosition = _socket.GridLocalOffset;
            canvasGo.transform.localRotation = _socket.GridPlaneRotation;

            // Compensate for parent lossy scale so canvas units = world meters,
            // mirroring InventoryGridSocket.SyncGridRootScale.
            var ps = _socket.Pivot.lossyScale;
            canvasGo.transform.localScale = new Vector3(
                Mathf.Approximately(ps.x, 0f) ? 1f : 1f / ps.x,
                Mathf.Approximately(ps.y, 0f) ? 1f : 1f / ps.y,
                Mathf.Approximately(ps.z, 0f) ? 1f : 1f / ps.z);

            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;

            var rt = (RectTransform)canvasGo.transform;
            rt.sizeDelta = new Vector2(canvasWidth, canvasHeight);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // G4: Canvas must be non-interactable. No GraphicRaycaster.
            var group = canvasGo.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            // G4: Ignore Raycast layer on the Canvas and ALL its children.
            NeutralizeLayer(canvasGo.transform);

            _canvasCreatedHere = true;
        }

        private void BuildCells()
        {
            var size = _socket.GridSize;
            var cellSize = _socket.CellWorldSize;
            float canvasWidth = size.x * cellSize.x;
            float canvasHeight = size.y * cellSize.y;

            _cells = new Image[size.x, size.y];

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var go = new GameObject($"Cell_{x}_{y}");
                    go.transform.SetParent(_canvas.transform, false);

                    var img = go.AddComponent<Image>();
                    img.sprite = ResolvedSprite;
                    img.raycastTarget = false; // G4: never intercept input.

                    var rt = img.rectTransform;
                    rt.sizeDelta = cellSize;

                    // Center each cell within the canvas local space.
                    float localX = (x + 0.5f) * cellSize.x - canvasWidth * 0.5f;
                    float localY = (y + 0.5f) * cellSize.y - canvasHeight * 0.5f;
                    rt.anchoredPosition = new Vector2(localX, localY);

                    img.color = freeColor;
                    _cells[x, y] = img;
                }
            }

            RefreshAll();
        }

        /// <summary>
        /// Recursively sets every transform under <paramref name="root"/> to the
        /// Ignore Raycast layer (2) so the overlay never blocks socket detection.
        /// </summary>
        private static void NeutralizeLayer(Transform root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = 2; // Ignore Raycast
        }

        // ================================================================
        // Refresh Logic
        // ================================================================

        private void OnHoverChanged(Vector2Int? anchor)
        {
            RefreshHover();
        }

        /// <summary>Redraws occupancy colors then reapplies the hover highlight.</summary>
        private void RefreshAll()
        {
            if (_cells == null || _socket == null) return;

            var size = _socket.GridSize;
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    _cells[x, y].color =
                        _socket.IsCellOccupied(x, y) ? occupiedColor : freeColor;

            RefreshHover();
        }

        /// <summary>
        /// Overlays the hover footprint highlight: green for valid, faint red for
        /// blocked. Free cells are reset to freeColor first so stale highlights clear.
        /// </summary>
        private void RefreshHover()
        {
            if (_cells == null || _socket == null) return;

            var size = _socket.GridSize;

            // Reset all free cells to freeColor first.
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    if (!_socket.IsCellOccupied(x, y))
                        _cells[x, y].color = freeColor;

            if (_socket.Hovering == null) return;

            if (_socket.HoverAnchor.HasValue)
            {
                // Valid placement — paint the footprint green.
                var anchor = _socket.HoverAnchor.Value;
                var fp = _socket.HoverFootprint;
                for (int dx = 0; dx < fp.x; dx++)
                {
                    for (int dy = 0; dy < fp.y; dy++)
                    {
                        int cx = anchor.x + dx;
                        int cy = anchor.y + dy;
                        if (cx >= 0 && cx < size.x && cy >= 0 && cy < size.y)
                            _cells[cx, cy].color = hoverValidColor;
                    }
                }
            }
            else
            {
                // Blocked — item doesn't fit anywhere. Tint all free cells red.
                for (int x = 0; x < size.x; x++)
                    for (int y = 0; y < size.y; y++)
                        if (!_socket.IsCellOccupied(x, y))
                            _cells[x, y].color = hoverBlockedColor;
            }
        }
    }
}

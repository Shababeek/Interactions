using System.Collections.Generic;
using System.Linq;
using Shababeek.Interactions.Shababeek.Interactions;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Linear multi-slot socket. Occupants live in slot-indexed storage.
    ///
    /// Default (<see cref="allowGaps"/> off) the rack packs from the start: hover is
    /// clamped to the first empty slot and removal re-packs, so no gaps ever exist.
    ///
    /// With <see cref="allowGaps"/> on, every slot is an independent parking spot.
    /// Releasing over an empty slot parks there and nothing else moves; releasing over
    /// an occupied slot ripples the run of occupants toward the nearest gap (either
    /// direction, ties go right). Removing an occupant just empties its slot.
    /// </summary>
    public class ShiftingRackSocket : AbstractSocket
    {
        [Header("Category")]
        [Tooltip("Single category this rack accepts. Empty (Nothing) accepts any Socketable. Socketables can belong to multiple categories; the rack matches if the socketable's mask contains this bit.")]
        [SerializeField, SingleSocketCategory] private SocketMask requiredCategory;

        [Header("Rack Layout")]
        [Tooltip("Total number of slots in the rack.")]
        [SerializeField, Min(1)] private int slotCount = 6;

        [Tooltip("Distance between slot centers along the rack axis.")]
        [SerializeField, Min(0.01f)] private float spacing = 0.15f;

        [Tooltip("Direction the rack extends along, in local space.")]
        [SerializeField] private LocalDirection axis = LocalDirection.Right;

        [Tooltip("Local offset applied to the whole rack.")]
        [SerializeField] private Vector3 localOffset = Vector3.zero;

        [Tooltip("Center the rack on the local offset instead of growing from it.")]
        [SerializeField] private bool centerLine = true;

        [Tooltip("Rotation offset applied to every slot.")]
        [SerializeField] private Vector3 pivotRotationOffset = Vector3.zero;

        [Header("Placement Rules")]
        [Tooltip("Off (default): occupants pack from the start of the rack and re-pack on removal. On: any slot can be filled independently, leaving gaps; dropping onto an occupied slot ripples occupants toward the nearest gap.")]
        [SerializeField] private bool allowGaps = false;

        [Header("Placement Offset")]
        [Tooltip("Local position offset (relative to slot) applied to occupants. Use to lift items out of the slot's geometric center so they rest on the surface.")]
        [SerializeField] private Vector3 placementPositionOffset = Vector3.zero;

        [Tooltip("Local rotation offset (relative to slot) applied to occupants when placed.")]
        [SerializeField] private Vector3 placementRotationOffset = Vector3.zero;

        [Header("Motion")]
        [Tooltip("Higher values snap faster, lower values feel heavier. 10-16 feels light.")]
        [SerializeField, Min(0.1f)] private float shiftSpeed = 14f;

        [Header("Highlight")]
        [Tooltip("Optional visual instantiated as a child, moved to the active preview slot.")]
        [SerializeField] private Transform highlightPrefab;

        [Tooltip("Fired when the preview slot changes. Payload = slot index.")]
        [SerializeField] private UnityEvent<int> onSlotHighlighted = new();

        [Tooltip("Fired when the rack stops previewing any slot.")]
        [SerializeField] private UnityEvent onSlotUnhighlighted = new();

        [Header("Haptics")]
        [Tooltip("Haptic amplitude (0-1) when the hover preview moves to another slot.")]
        [SerializeField, Range(0f, 1f)] private float slotChangeAmplitude = 0.25f;
        [Tooltip("Haptic duration in seconds when the hover preview moves to another slot.")]
        [SerializeField, Min(0f)] private float slotChangeDuration = 0.04f;
        [Tooltip("Haptic amplitude (0-1) when an item is inserted.")]
        [SerializeField, Range(0f, 1f)] private float insertAmplitude = 0.55f;
        [Tooltip("Haptic duration in seconds when an item is inserted.")]
        [SerializeField, Min(0f)] private float insertDuration = 0.08f;
        [Tooltip("Haptic amplitude (0-1) when an insert is rejected (rack full).")]
        [SerializeField, Range(0f, 1f)] private float rejectAmplitude = 0.12f;
        [Tooltip("Haptic duration in seconds when an insert is rejected.")]
        [SerializeField, Min(0f)] private float rejectDuration = 0.03f;

        [Tooltip("Optional pattern for slot-change; overrides amplitude/duration when assigned.")]
        [SerializeField] private HapticPattern slotChangePattern;
        [Tooltip("Optional pattern for insert; overrides amplitude/duration when assigned.")]
        [SerializeField] private HapticPattern insertPattern;
        [Tooltip("Optional pattern for reject; overrides amplitude/duration when assigned.")]
        [SerializeField] private HapticPattern rejectPattern;

        private Transform[] _slots;
        private Socketable[] _slotOccupants;
        private readonly Dictionary<Socketable, Transform> _carriers = new();
        private readonly List<Socketable> _occupantsCache = new();
        private bool _occupantsDirty = true;
        private int _filledCount;
        private Socketable _hovering;
        private int _hoverIndex = -1;
        private Transform _highlightInstance;
        private Transform _rackRoot;

        public int SlotCount => slotCount;
        public int FilledCount => _filledCount;

        /// <summary>Lowest usable slot index that is empty, or <see cref="SlotCount"/> when the rack is full.</summary>
        public int FirstEmptyIndex
        {
            get
            {
                if (_slotOccupants == null) return 0;
                for (int i = 0; i < slotCount; i++)
                    if (IsSlotUsable(i) && _slotOccupants[i] == null) return i;
                return slotCount;
            }
        }

        /// <summary>Occupants in slot order, gaps skipped. Index here is NOT the slot index when gaps exist — use <see cref="GetOccupant"/> for that.</summary>
        public IReadOnlyList<Socketable> Occupants
        {
            get
            {
                if (_occupantsDirty) RebuildOccupantsCache();
                return _occupantsCache;
            }
        }

        /// <summary>Occupant parked in a specific slot, or null when that slot is empty.</summary>
        public Socketable GetOccupant(int slotIndex) =>
            _slotOccupants != null && slotIndex >= 0 && slotIndex < _slotOccupants.Length ? _slotOccupants[slotIndex] : null;

        /// <summary>Highest slot index holding an occupant, or -1 when the rack is empty.</summary>
        public int HighestOccupiedSlot
        {
            get
            {
                if (_slotOccupants == null) return -1;
                for (int i = slotCount - 1; i >= 0; i--)
                    if (_slotOccupants[i] != null) return i;
                return -1;
            }
        }

        public UnityEvent<int> OnSlotHighlighted => onSlotHighlighted;
        public UnityEvent OnSlotUnhighlighted => onSlotUnhighlighted;

        protected virtual void Awake()
        {
            BuildRackRoot();
            BuildSlots();
            if (highlightPrefab != null)
            {
                _highlightInstance = Instantiate(highlightPrefab, _rackRoot);
                _highlightInstance.localRotation = Quaternion.identity;
                NeutralizeHighlight(_highlightInstance);
                _highlightInstance.gameObject.SetActive(false);
            }
        }

        private void BuildRackRoot()
        {
            var go = new GameObject("RackRoot");
            _rackRoot = go.transform;
            _rackRoot.SetParent(Pivot, false);
            _rackRoot.localPosition = Vector3.zero;
            _rackRoot.localRotation = Quaternion.identity;
            SyncRackRootScale();
        }

        /// <summary>
        /// Rebuilds the rack with a new slot count. Only safe when the rack is empty
        /// (<see cref="FilledCount"/> == 0). Intended for runtime capacity changes.
        /// </summary>
        public void Reconfigure(int newSlotCount)
        {
            if (newSlotCount < 1) newSlotCount = 1;
            if (_filledCount > 0)
            {
                Debug.LogWarning($"[ShiftingRackSocket] Reconfigure called with {_filledCount} occupants; aborting.");
                return;
            }
            slotCount = newSlotCount;
            if (_rackRoot != null)
            {
                Destroy(_rackRoot.gameObject);
                _rackRoot = null;
            }
            BuildRackRoot();
            BuildSlots();
            if (highlightPrefab != null)
            {
                if (_highlightInstance != null) Destroy(_highlightInstance.gameObject);
                _highlightInstance = Instantiate(highlightPrefab, _rackRoot);
                _highlightInstance.localRotation = Quaternion.identity;
                NeutralizeHighlight(_highlightInstance);
                _highlightInstance.gameObject.SetActive(false);
            }
        }

        private void SyncRackRootScale()
        {
            if (_rackRoot == null) return;
            var s = Pivot.lossyScale;
            _rackRoot.localScale = new Vector3(
                Mathf.Approximately(s.x, 0f) ? 1f : 1f / s.x,
                Mathf.Approximately(s.y, 0f) ? 1f : 1f / s.y,
                Mathf.Approximately(s.z, 0f) ? 1f : 1f / s.z);
        }

        /// <summary>
        /// Whether a slot may hold an occupant. Base rack uses every slot; override to
        /// gate slots off at runtime (e.g. a rack that opens capacity progressively).
        /// </summary>
        protected virtual bool IsSlotUsable(int index) => index >= 0 && index < slotCount;

        /// <summary>
        /// Effective gap policy. Reads the serialized flag by default; override in a
        /// subclass whose placement model always allows gaps.
        /// </summary>
        protected virtual bool AllowGaps => allowGaps;

        public override bool CanSocket()
        {
            if (_slotOccupants == null) return false;
            for (int i = 0; i < slotCount; i++)
                if (IsSlotUsable(i) && _slotOccupants[i] == null) return true;
            return false;
        }

        public override bool CanSocket(Socketable socketable)
        {
            if (requiredCategory.IsEmpty) return true;
            if (socketable == null) return false;
            return requiredCategory.Overlaps(socketable.SocketableMask);
        }

        public override void StartHovering(Socketable socketable)
        {
            base.StartHovering(socketable);
            if (!CanSocket())
            {
                Debug.Log($"[Rack:{name}] HOVER START rejected (full) socketable='{(socketable!=null?socketable.name:"null")}' filled={_filledCount}/{slotCount}");
                TriggerHaptic(socketable, rejectAmplitude, rejectDuration, rejectPattern);
                return;
            }
            _hovering = socketable;
            RefreshHoverIndex(socketable, silent: true);
            ShowHighlight();
            Debug.Log($"[Rack:{name}] HOVER START socketable='{(socketable!=null?socketable.name:"null")}' hoverIndex={_hoverIndex} filled={_filledCount}/{slotCount} canAccept={CanSocket(socketable)} mask=0x{(socketable!=null?(int)socketable.SocketableMask.Value:0):X8}");
            onSlotHighlighted.Invoke(_hoverIndex);
        }

        public override void EndHovering(Socketable socketable)
        {
            base.EndHovering(socketable);
            if (_hovering != socketable) return;
            Debug.Log($"[Rack:{name}] HOVER END socketable='{(socketable!=null?socketable.name:"null")}' wasHoverIndex={_hoverIndex}");
            ClearHover();
        }

        public override (Vector3 position, Quaternion rotation) GetPivotForSocketable(Socketable socketable)
        {
            AdoptAsHoverIfNeeded(socketable);
            if (_hovering == socketable && _hoverIndex >= 0)
            {
                RefreshHoverIndex(socketable);
                return GetSlotPlacement(_slots[_hoverIndex]);
            }
            if (_slots != null && _slots.Length > 0) return GetSlotPlacement(_slots[0]);
            return (Pivot.position, Pivot.rotation);
        }

        /// <summary>
        /// World pose where an occupant should rest in the given slot, accounting for
        /// the placement position/rotation offsets configured on the rack.
        /// </summary>
        protected (Vector3 position, Quaternion rotation) GetSlotPlacement(Transform slot)
        {
            var rot = slot.rotation * Quaternion.Euler(placementRotationOffset);
            var pos = slot.position + slot.rotation * placementPositionOffset;
            return (pos, rot);
        }

        // Socketable.DetectSockets only fires StartHovering when the closest socket changes.
        // A tape grabbed from inside this rack keeps us as its CurrentSocket, so StartHovering
        // never re-fires and _hovering stays null. Adopt it on demand so highlight, shift,
        // and insert-index logic all work during rearrange.
        private void AdoptAsHoverIfNeeded(Socketable socketable)
        {
            if (socketable == null || _hovering == socketable) return;
            if (_hovering != null) return;
            if (socketable.IsSocketed) return;
            if (socketable.CurrentSocket != this) return;
            if (!CanSocket() || !CanSocket(socketable)) return;
            _hovering = socketable;
            RefreshHoverIndex(socketable, silent: true);
            ShowHighlight();
            Debug.Log($"[Rack:{name}] HOVER ADOPTED socketable='{socketable.name}' hoverIndex={_hoverIndex} filled={_filledCount}/{slotCount}");
            onSlotHighlighted.Invoke(_hoverIndex);
        }

        internal override Transform Insert(Socketable socketable)
        {
            if (!CanSocket())
            {
                Debug.LogWarning($"[Rack:{name}] INSERT REJECTED (full) socketable='{(socketable!=null?socketable.name:"null")}' filled={_filledCount}/{slotCount}");
                return null;
            }

            AdoptAsHoverIfNeeded(socketable);
            int target;
            string source;
            if (_hovering == socketable && _hoverIndex >= 0)
            {
                target = _hoverIndex;
                source = "hoverIndex";
            }
            else
            {
                target = ComputeInsertIndex(socketable.transform.position);
                source = "nearestSlot";
            }
            int rawIndex = target;
            target = Mathf.Clamp(target, 0, slotCount - 1);

            if (!IsSlotUsable(target))
            {
                // Target is gated off — park in the closest usable gap instead.
                target = FindNearestGap(target);
            }
            else if (_slotOccupants[target] != null)
            {
                int gap = FindNearestGap(target);
                if (gap < 0)
                {
                    Debug.LogWarning($"[Rack:{name}] INSERT REJECTED (no gap) socketable='{socketable.name}' target={target}");
                    return null;
                }
                ShiftTowardGap(target, gap);
            }
            if (target < 0)
            {
                Debug.LogWarning($"[Rack:{name}] INSERT REJECTED (no usable slot) socketable='{socketable.name}' raw={rawIndex}");
                return null;
            }

            float distToSlot = Vector3.Distance(socketable.transform.position, _slots[target].position);
            Debug.Log($"[Rack:{name}] INSERT socketable='{socketable.name}' slot={target} (raw={rawIndex} via {source}) filled-before={_filledCount}/{slotCount} dist={distToSlot:F3} canAccept={CanSocket(socketable)} mask=0x{(int)socketable.SocketableMask.Value:X8} releasePos={socketable.transform.position}");

            var carrier = new GameObject($"Carrier_{socketable.name}").transform;
            carrier.SetParent(_rackRoot, false);
            var placement = GetSlotPlacement(_slots[target]);
            carrier.SetPositionAndRotation(placement.position, placement.rotation);

            _slotOccupants[target] = socketable;
            _filledCount++;
            _occupantsDirty = true;
            _carriers[socketable] = carrier;

            TriggerHaptic(socketable, insertAmplitude, insertDuration, insertPattern);
            ClearHover();

            base.Insert(socketable);
            Debug.Log($"[Rack:{name}] INSERT DONE socketable='{socketable.name}' slot={target} filled-after={_filledCount}/{slotCount} order=[{string.Join(",", Occupants.Select(o => o != null ? o.name : "null"))}]");
            return carrier;
        }

        public override void Remove(Socketable socketable)
        {
            var idx = IndexOfOccupant(socketable);
            Debug.Log($"[Rack:{name}] REMOVE socketable='{(socketable!=null?socketable.name:"null")}' fromSlot={idx} filled-before={_filledCount}/{slotCount}");
            if (idx >= 0)
            {
                _slotOccupants[idx] = null;
                _filledCount--;
                _occupantsDirty = true;
                if (!AllowGaps) CompactTowardStart();

                if (_carriers.TryGetValue(socketable, out var carrier))
                {
                    _carriers.Remove(socketable);
                    if (carrier != null)
                    {
                        // Detach socketable so grab system can reparent without side effects.
                        if (socketable != null && socketable.transform.parent == carrier)
                        {
                            socketable.transform.SetParent(null, true);
                        }
                        Destroy(carrier.gameObject);
                    }
                }
            }
            if (_hovering == socketable) ClearHover();
            base.Remove(socketable);
        }

        private void Update()
        {
            if (_slots == null || _slots.Length == 0 || _slotOccupants == null) return;

            SyncRackRootScale();

            if (_hovering != null) RefreshHoverIndex(_hovering);

            var t = 1f - Mathf.Exp(-shiftSpeed * Time.deltaTime);
            for (int i = 0; i < slotCount; i++)
            {
                var occ = _slotOccupants[i];
                if (occ == null) continue;
                if (!_carriers.TryGetValue(occ, out var carrier) || carrier == null) continue;

                var targetIdx = PreviewSlotIndex(i);
                if (targetIdx < 0 || targetIdx >= slotCount) continue;

                var target = GetSlotPlacement(_slots[targetIdx]);
                carrier.position = Vector3.Lerp(carrier.position, target.position, t);
                carrier.rotation = Quaternion.Slerp(carrier.rotation, target.rotation, t);
            }

            if (_hovering != null && _highlightInstance != null && _hoverIndex >= 0)
            {
                // Position only — keep rack-aligned orientation.
                _highlightInstance.position = _slots[_hoverIndex].position;
            }
        }

        /// <summary>
        /// Slot an occupant should currently rest at, accounting for the live hover preview.
        /// Hovering an empty slot moves nothing; hovering an occupied slot ripples the run
        /// between it and the nearest gap by one.
        /// </summary>
        private int PreviewSlotIndex(int slotIndex)
        {
            if (_hovering == null || _hoverIndex < 0) return slotIndex;
            int target = _hoverIndex;
            if (!IsSlotUsable(target) || _slotOccupants[target] == null) return slotIndex;
            int gap = FindNearestGap(target);
            if (gap < 0) return slotIndex;
            if (gap > target) return slotIndex >= target && slotIndex < gap ? slotIndex + 1 : slotIndex;
            return slotIndex > gap && slotIndex <= target ? slotIndex - 1 : slotIndex;
        }

        /// <summary>Nearest empty usable slot to <paramref name="from"/>. Ties resolve toward the end of the rack. -1 when the rack is full.</summary>
        private int FindNearestGap(int from)
        {
            if (from >= 0 && from < slotCount && IsSlotUsable(from) && _slotOccupants[from] == null) return from;
            for (int d = 1; d < slotCount; d++)
            {
                int right = from + d;
                if (right < slotCount && IsSlotUsable(right) && _slotOccupants[right] == null) return right;
                int left = from - d;
                if (left >= 0 && IsSlotUsable(left) && _slotOccupants[left] == null) return left;
            }
            return -1;
        }

        /// <summary>Moves the run of occupants between <paramref name="target"/> and <paramref name="gap"/> one step toward the gap, leaving target empty.</summary>
        private void ShiftTowardGap(int target, int gap)
        {
            if (gap > target)
                for (int i = gap; i > target; i--) _slotOccupants[i] = _slotOccupants[i - 1];
            else
                for (int i = gap; i < target; i++) _slotOccupants[i] = _slotOccupants[i + 1];
            _slotOccupants[target] = null;
            _occupantsDirty = true;
        }

        private void CompactTowardStart()
        {
            int write = 0;
            for (int read = 0; read < slotCount; read++)
            {
                var occ = _slotOccupants[read];
                if (occ == null) continue;
                _slotOccupants[read] = null;
                while (write < slotCount && !IsSlotUsable(write)) write++;
                if (write >= slotCount) break;
                _slotOccupants[write++] = occ;
            }
            _occupantsDirty = true;
        }

        private int IndexOfOccupant(Socketable socketable)
        {
            if (_slotOccupants == null || socketable == null) return -1;
            for (int i = 0; i < slotCount; i++)
                if (_slotOccupants[i] == socketable) return i;
            return -1;
        }

        private void RebuildOccupantsCache()
        {
            _occupantsCache.Clear();
            if (_slotOccupants != null)
            {
                for (int i = 0; i < slotCount; i++)
                    if (_slotOccupants[i] != null) _occupantsCache.Add(_slotOccupants[i]);
            }
            _occupantsDirty = false;
        }

        private void RefreshHoverIndex(Socketable socketable, bool silent = false)
        {
            var newIndex = ComputeInsertIndex(socketable.transform.position);
            if (newIndex == _hoverIndex) return;
            int prev = _hoverIndex;
            _hoverIndex = newIndex;
            Debug.Log($"[Rack:{name}] HOVER SLOT CHANGED socketable='{socketable.name}' {prev}→{newIndex} filled={_filledCount}/{slotCount} pos={socketable.transform.position}");
            if (silent) return;
            TriggerHaptic(socketable, slotChangeAmplitude, slotChangeDuration, slotChangePattern);
            ShowHighlight();
            onSlotHighlighted.Invoke(_hoverIndex);
        }

        private int ComputeInsertIndex(Vector3 worldPosition)
        {
            // Packed mode keeps the legacy rule: never preview past the packed edge.
            int maxIndex = AllowGaps ? slotCount - 1 : Mathf.Min(FirstEmptyIndex, slotCount - 1);
            int closest = -1;
            float closestDist = float.MaxValue;
            for (int i = 0; i <= maxIndex; i++)
            {
                if (!IsSlotUsable(i)) continue;
                var d = (worldPosition - _slots[i].position).sqrMagnitude;
                if (d < closestDist)
                {
                    closestDist = d;
                    closest = i;
                }
            }
            return closest < 0 ? 0 : closest;
        }

        private void ShowHighlight()
        {
            if (_highlightInstance == null || _hoverIndex < 0) return;
            var slot = _slots[_hoverIndex];
            // Position only — rotation stays aligned with the rack, never the held socketable.
            _highlightInstance.position = slot.position;
            _highlightInstance.gameObject.SetActive(true);
        }

        private void HideHighlight()
        {
            if (_highlightInstance != null) _highlightInstance.gameObject.SetActive(false);
        }

        private void ClearHover()
        {
            if (_hoverIndex >= 0) onSlotUnhighlighted.Invoke();
            _hovering = null;
            _hoverIndex = -1;
            HideHighlight();
        }

        protected virtual void BuildSlots()
        {
            _slots = new Transform[slotCount];
            _slotOccupants = new Socketable[slotCount];
            _filledCount = 0;
            _occupantsDirty = true;
            var dir = AxisVector(axis);
            var centerOffset = centerLine ? dir * ((slotCount - 1) * spacing * 0.5f) : Vector3.zero;
            for (int i = 0; i < slotCount; i++)
            {
                var localPos = localOffset + dir * (i * spacing) - centerOffset;
                var localRot = Quaternion.Euler(pivotRotationOffset);
                _slots[i] = CreateSlot(i, localPos, localRot, _rackRoot);
            }
        }

        /// <summary>
        /// Builds a single slot transform under <paramref name="parent"/> at the given
        /// local pose. Override to swap in a custom prefab (e.g. labeled slot).
        /// </summary>
        protected virtual Transform CreateSlot(int index, Vector3 localPosition, Quaternion localRotation, Transform parent)
        {
            var slot = new GameObject($"RackSlot_{index}").transform;
            slot.SetParent(parent, false);
            slot.localPosition = localPosition;
            slot.localRotation = localRotation;
            return slot;
        }

        private static void NeutralizeHighlight(Transform root)
        {
            // Highlight is pure visual — strip anything that could interact with sockets or physics.
            var ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (ignoreLayer >= 0) t.gameObject.layer = ignoreLayer;
            }
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }
            foreach (var s in root.GetComponentsInChildren<Socketable>(true)) s.enabled = false;
            foreach (var i in root.GetComponentsInChildren<InteractableBase>(true)) i.enabled = false;
        }

        private void TriggerHaptic(Socketable socketable, float amplitude, float duration, HapticPattern pattern = null)
        {
            if (socketable == null) return;
            var interactor = socketable.GetComponent<InteractableBase>()?.CurrentInteractor;
            if (interactor == null) return;

            if (pattern != null)
                interactor.PlayHapticPattern(pattern);
            else if (amplitude > 0f && duration > 0f)
                interactor.SendHapticImpulse(amplitude, duration);
        }

        private static Vector3 AxisVector(LocalDirection d) => d switch
        {
            LocalDirection.Forward => Vector3.forward,
            LocalDirection.Back => Vector3.back,
            LocalDirection.Right => Vector3.right,
            LocalDirection.Left => Vector3.left,
            LocalDirection.Up => Vector3.up,
            LocalDirection.Down => Vector3.down,
            _ => Vector3.right
        };

        private void OnDrawGizmos()
        {
            var old = Gizmos.matrix;
            // Match runtime: slots live under a scale-compensated root, so gizmos ignore lossy scale too.
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            var dir = AxisVector(axis);
            var count = Mathf.Max(1, slotCount);
            var centerOffset = centerLine ? dir * ((count - 1) * spacing * 0.5f) : Vector3.zero;
            var slotRotLocal = Quaternion.Euler(pivotRotationOffset);
            var placementRotLocal = slotRotLocal * Quaternion.Euler(placementRotationOffset);
            var placementPosLocal = slotRotLocal * placementPositionOffset;
            float axisLen = spacing * 0.4f;
            float slotRadius = spacing * 0.25f;

            for (int i = 0; i < count; i++)
            {
                var slotPos = localOffset + dir * (i * spacing) - centerOffset;
                var placePos = slotPos + placementPosLocal;

                // Slot anchor (cyan wire sphere)
                Gizmos.color = new Color(0f, 1f, 1f, 0.7f);
                Gizmos.DrawWireSphere(slotPos, slotRadius);

                // Connector from slot anchor to placement point
                if (placementPosLocal.sqrMagnitude > 1e-8f)
                {
                    Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.6f);
                    Gizmos.DrawLine(slotPos, placePos);
                }

                // Placement pose: solid yellow cube + RGB axis arrows for orientation
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
                Gizmos.DrawCube(placePos, Vector3.one * (slotRadius * 0.6f));

                Gizmos.color = Color.red;
                Gizmos.DrawLine(placePos, placePos + placementRotLocal * Vector3.right * axisLen);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(placePos, placePos + placementRotLocal * Vector3.up * axisLen);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(placePos, placePos + placementRotLocal * Vector3.forward * axisLen);
            }

            // Rack axis line through slot anchors
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            var first = localOffset - centerOffset;
            var last = localOffset + dir * ((count - 1) * spacing) - centerOffset;
            Gizmos.DrawLine(first, last);
            Gizmos.matrix = old;
        }
    }
}

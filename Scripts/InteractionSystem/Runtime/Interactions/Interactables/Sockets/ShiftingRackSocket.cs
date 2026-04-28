using System.Collections.Generic;
using System.Linq;
using Shababeek.Interactions.Shababeek.Interactions;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Linear multi-slot socket that packs occupants from the start of the rack.
    /// While a held socketable hovers the rack, existing occupants shift aside
    /// to preview the insertion slot. Hover is clamped to the first empty slot
    /// so the user can never preview past the packed edge. Commits on release.
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
        [SerializeField, Range(0f, 1f)] private float slotChangeAmplitude = 0.25f;
        [SerializeField, Min(0f)] private float slotChangeDuration = 0.04f;
        [SerializeField, Range(0f, 1f)] private float insertAmplitude = 0.55f;
        [SerializeField, Min(0f)] private float insertDuration = 0.08f;
        [SerializeField, Range(0f, 1f)] private float rejectAmplitude = 0.12f;
        [SerializeField, Min(0f)] private float rejectDuration = 0.03f;

        private Transform[] _slots;
        private readonly List<Socketable> _occupants = new();
        private readonly Dictionary<Socketable, Transform> _carriers = new();
        private Socketable _hovering;
        private int _hoverIndex = -1;
        private Transform _highlightInstance;
        private Transform _rackRoot;

        public int SlotCount => slotCount;
        public int FilledCount => _occupants.Count;
        public int FirstEmptyIndex => _occupants.Count;
        public IReadOnlyList<Socketable> Occupants => _occupants;

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
            if (_occupants.Count > 0)
            {
                Debug.LogWarning($"[ShiftingRackSocket] Reconfigure called with {_occupants.Count} occupants; aborting.");
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

        public override bool CanSocket() => _occupants.Count < slotCount;

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
                Debug.Log($"[Rack:{name}] HOVER START rejected (full) socketable='{(socketable!=null?socketable.name:"null")}' filled={_occupants.Count}/{slotCount}");
                TriggerHaptic(socketable, rejectAmplitude, rejectDuration);
                return;
            }
            _hovering = socketable;
            RefreshHoverIndex(socketable, silent: true);
            ShowHighlight();
            Debug.Log($"[Rack:{name}] HOVER START socketable='{(socketable!=null?socketable.name:"null")}' hoverIndex={_hoverIndex} filled={_occupants.Count}/{slotCount} canAccept={CanSocket(socketable)} mask=0x{(socketable!=null?(int)socketable.SocketableMask.Value:0):X8}");
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
            Debug.Log($"[Rack:{name}] HOVER ADOPTED socketable='{socketable.name}' hoverIndex={_hoverIndex} filled={_occupants.Count}/{slotCount}");
            onSlotHighlighted.Invoke(_hoverIndex);
        }

        internal override Transform Insert(Socketable socketable)
        {
            if (!CanSocket())
            {
                Debug.LogWarning($"[Rack:{name}] INSERT REJECTED (full) socketable='{(socketable!=null?socketable.name:"null")}' filled={_occupants.Count}/{slotCount}");
                return null;
            }

            AdoptAsHoverIfNeeded(socketable);
            int insertAt;
            string source;
            if (_hovering == socketable && _hoverIndex >= 0)
            {
                insertAt = _hoverIndex;
                source = "hoverIndex";
            }
            else
            {
                insertAt = ComputeInsertIndex(socketable.transform.position);
                source = "nearestSlot";
            }
            int rawIndex = insertAt;
            insertAt = Mathf.Clamp(insertAt, 0, _occupants.Count);

            float distToSlot = Vector3.Distance(socketable.transform.position, _slots[insertAt].position);
            Debug.Log($"[Rack:{name}] INSERT socketable='{socketable.name}' slot={insertAt} (raw={rawIndex} via {source}) filled-before={_occupants.Count}/{slotCount} dist={distToSlot:F3} canAccept={CanSocket(socketable)} mask=0x{(int)socketable.SocketableMask.Value:X8} releasePos={socketable.transform.position}");

            var carrier = new GameObject($"Carrier_{socketable.name}").transform;
            carrier.SetParent(_rackRoot, false);
            var placement = GetSlotPlacement(_slots[insertAt]);
            carrier.SetPositionAndRotation(placement.position, placement.rotation);

            _occupants.Insert(insertAt, socketable);
            _carriers[socketable] = carrier;

            TriggerHaptic(socketable, insertAmplitude, insertDuration);
            ClearHover();

            base.Insert(socketable);
            Debug.Log($"[Rack:{name}] INSERT DONE socketable='{socketable.name}' slot={insertAt} filled-after={_occupants.Count}/{slotCount} order=[{string.Join(",", System.Linq.Enumerable.Range(0,_occupants.Count).Select(i => _occupants[i]!=null?_occupants[i].name:"null"))}]");
            return carrier;
        }

        public override void Remove(Socketable socketable)
        {
            var idx = _occupants.IndexOf(socketable);
            Debug.Log($"[Rack:{name}] REMOVE socketable='{(socketable!=null?socketable.name:"null")}' fromSlot={idx} filled-before={_occupants.Count}/{slotCount}");
            if (idx >= 0)
            {
                _occupants.RemoveAt(idx);
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
            if (_slots == null || _slots.Length == 0) return;

            SyncRackRootScale();

            if (_hovering != null) RefreshHoverIndex(_hovering);

            var t = 1f - Mathf.Exp(-shiftSpeed * Time.deltaTime);
            for (int i = 0; i < _occupants.Count; i++)
            {
                var occ = _occupants[i];
                if (occ == null) continue;
                if (!_carriers.TryGetValue(occ, out var carrier) || carrier == null) continue;

                var targetIdx = EffectiveSlotIndex(i);
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

        private int EffectiveSlotIndex(int occupantIndex)
        {
            if (_hovering != null && _hoverIndex >= 0 && occupantIndex >= _hoverIndex)
                return occupantIndex + 1;
            return occupantIndex;
        }

        private void RefreshHoverIndex(Socketable socketable, bool silent = false)
        {
            var newIndex = ComputeInsertIndex(socketable.transform.position);
            if (newIndex == _hoverIndex) return;
            int prev = _hoverIndex;
            _hoverIndex = newIndex;
            Debug.Log($"[Rack:{name}] HOVER SLOT CHANGED socketable='{socketable.name}' {prev}→{newIndex} filled={_occupants.Count}/{slotCount} pos={socketable.transform.position}");
            if (silent) return;
            TriggerHaptic(socketable, slotChangeAmplitude, slotChangeDuration);
            ShowHighlight();
            onSlotHighlighted.Invoke(_hoverIndex);
        }

        private int ComputeInsertIndex(Vector3 worldPosition)
        {
            // Insertable range is [0, filledCount], clamped inside rack bounds.
            var maxInsertable = Mathf.Min(_occupants.Count, slotCount - 1);
            int closest = 0;
            float closestDist = float.MaxValue;
            for (int i = 0; i <= maxInsertable; i++)
            {
                var d = (worldPosition - _slots[i].position).sqrMagnitude;
                if (d < closestDist)
                {
                    closestDist = d;
                    closest = i;
                }
            }
            return closest;
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

        private void TriggerHaptic(Socketable socketable, float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f || socketable == null) return;
            var interactable = socketable.GetComponent<InteractableBase>();
            interactable?.CurrentInteractor?.SendHapticImpulse(amplitude, duration);
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

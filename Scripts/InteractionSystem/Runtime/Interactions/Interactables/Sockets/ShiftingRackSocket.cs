using System.Collections.Generic;
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

        private void Awake()
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

        public override void StartHovering(Socketable socketable)
        {
            base.StartHovering(socketable);
            if (!CanSocket())
            {
                TriggerHaptic(socketable, rejectAmplitude, rejectDuration);
                return;
            }
            _hovering = socketable;
            RefreshHoverIndex(socketable, silent: true);
            ShowHighlight();
            onSlotHighlighted.Invoke(_hoverIndex);
        }

        public override void EndHovering(Socketable socketable)
        {
            base.EndHovering(socketable);
            if (_hovering != socketable) return;
            ClearHover();
        }

        public override (Vector3 position, Quaternion rotation) GetPivotForSocketable(Socketable socketable)
        {
            if (_hovering == socketable && _hoverIndex >= 0)
            {
                RefreshHoverIndex(socketable);
                var slot = _slots[_hoverIndex];
                return (slot.position, slot.rotation);
            }
            var fallback = _slots != null && _slots.Length > 0 ? _slots[0] : Pivot;
            return (fallback.position, fallback.rotation);
        }

        internal override Transform Insert(Socketable socketable)
        {
            if (!CanSocket()) return null;

            var insertAt = _hovering == socketable && _hoverIndex >= 0 ? _hoverIndex : _occupants.Count;
            insertAt = Mathf.Clamp(insertAt, 0, _occupants.Count);

            var carrier = new GameObject($"Carrier_{socketable.name}").transform;
            carrier.SetParent(_rackRoot, false);
            var slot = _slots[insertAt];
            carrier.SetPositionAndRotation(slot.position, slot.rotation);

            _occupants.Insert(insertAt, socketable);
            _carriers[socketable] = carrier;

            TriggerHaptic(socketable, insertAmplitude, insertDuration);
            ClearHover();

            base.Insert(socketable);
            return carrier;
        }

        public override void Remove(Socketable socketable)
        {
            var idx = _occupants.IndexOf(socketable);
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

                var target = _slots[targetIdx];
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
            _hoverIndex = newIndex;
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

        private void BuildSlots()
        {
            _slots = new Transform[slotCount];
            var dir = AxisVector(axis);
            var centerOffset = centerLine ? dir * ((slotCount - 1) * spacing * 0.5f) : Vector3.zero;
            for (int i = 0; i < slotCount; i++)
            {
                var slot = new GameObject($"RackSlot_{i}").transform;
                slot.SetParent(_rackRoot, false);
                slot.localPosition = localOffset + dir * (i * spacing) - centerOffset;
                slot.localRotation = Quaternion.Euler(pivotRotationOffset);
                _slots[i] = slot;
            }
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
            var centerOffset = centerLine ? dir * ((Mathf.Max(1, slotCount) - 1) * spacing * 0.5f) : Vector3.zero;
            Gizmos.color = new Color(0f, 1f, 1f, 0.7f);
            for (int i = 0; i < Mathf.Max(1, slotCount); i++)
            {
                var p = localOffset + dir * (i * spacing) - centerOffset;
                Gizmos.DrawWireSphere(p, spacing * 0.25f);
            }
            // Axis line
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            var first = localOffset + dir * 0f - centerOffset;
            var last = localOffset + dir * ((Mathf.Max(1, slotCount) - 1) * spacing) - centerOffset;
            Gizmos.DrawLine(first, last);
            Gizmos.matrix = old;
        }
    }
}

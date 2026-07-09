using System;
using UnityEngine;
using UnityEngine.Events;
using Shababeek.ReactiveVars;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Hinged door or cabinet door that swings around a single axis between a closed and an open
    /// limit. The door starts closed and can start <see cref="startLocked">locked</see>: while
    /// locked it stays shut and refuses to swing. Release the latch (typically via a
    /// <see cref="DoorHandleInteractable"/>) to allow it to open, then push/pull to swing it.
    /// Set <c>returnWhenDeselected</c> for a self-closing door; leave it off for one that stays
    /// where it is released.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Door")]
    public class DoorInteractable : RotaryLeverBase
    {
        [Header("Hinge")]
        [Tooltip("Local offset from the door's origin to the hinge it swings around, in the " +
                 "door object's parent space. Zero swings about the origin; move it to the door's " +
                 "edge for a realistic hinge.")]
        [SerializeField] private Vector3 hingeOffset = Vector3.zero;

        [Header("Lock")]
        [Tooltip("Whether the door starts latched shut. A locked door will not swing until Unlock() is called.")]
        [SerializeField] private bool startLocked = true;

        [Header("Events")]
        [Tooltip("Event invoked when the door reaches the open position.")]
        [SerializeField] private UnityEvent onOpened;

        [Tooltip("Event invoked when the door reaches the closed position.")]
        [SerializeField] private UnityEvent onClosed;

        [Tooltip("Event invoked continuously as the door swings, passing normalized angle (0 = closed, 1 = open).")]
        [SerializeField] private FloatUnityEvent onMoved;

        [Tooltip("Event invoked when the latch is released and the door becomes free to swing.")]
        [SerializeField] private UnityEvent onUnlocked;

        [Tooltip("Event invoked when the latch engages and the door is held shut.")]
        [SerializeField] private UnityEvent onLocked;

        private const float LimitEpsilon = 0.02f;

        [Header("Debug")]
        [ReadOnly, SerializeField] private bool isLocked;

        // Latches the last extreme reported so onOpened/onClosed fire once per transition
        // rather than every frame the door is held against a limit.
        private DoorState _lastState = DoorState.Between;

        // Whether the closed rest rotation has been captured yet (in Start at runtime, or lazily
        // the first time the editor preview poses the door in edit mode).
        private bool _restCaptured;

        // Relative-swing reference, captured on grab so the door moves by the hand's delta rather
        // than snapping to an absolute bearing.
        private bool _swinging;
        private float _swingRefHandAngle;
        private float _swingRefDoorAngle;

        private enum DoorState { Between, Open, Closed }

        /// <summary>Observable fired continuously as the door's normalized angle changes.</summary>
        public IObservable<float> OnMoved => onMoved.AsObservable();

        /// <summary>Observable fired when the door reaches the open extreme.</summary>
        public IObservable<Unit> OnOpened => onOpened.AsObservable();

        /// <summary>Observable fired when the door reaches the closed extreme.</summary>
        public IObservable<Unit> OnClosed => onClosed.AsObservable();

        /// <summary>Observable fired when the latch is released.</summary>
        public IObservable<Unit> OnUnlocked => onUnlocked.AsObservable();

        /// <summary>Observable fired when the latch engages.</summary>
        public IObservable<Unit> OnLocked => onLocked.AsObservable();

        /// <summary>True while the door is latched shut and cannot swing.</summary>
        public bool IsLocked => isLocked;

        /// <summary>Local offset from the door origin to the hinge, in the door object's parent space.</summary>
        public Vector3 HingeOffset
        {
            get => hingeOffset;
            set => hingeOffset = value;
        }

        /// <inheritdoc/>
        protected override Vector3 PivotLocalPosition => _originalPosition + hingeOffset;

        /// <summary>True when the door is at (or past) its open limit.</summary>
        public bool IsOpen => currentNormalizedAngle >= 1f - LimitEpsilon;

        /// <summary>True when the door is at (or past) its closed limit.</summary>
        public bool IsClosed => currentNormalizedAngle <= LimitEpsilon;

        protected override void Start()
        {
            base.Start();
            _restCaptured = true;
            isLocked = startLocked;
        }

        /// <summary>
        /// Poses the door to a normalized open amount (0 = closed rest, 1 = fully open) without
        /// requiring play mode. Bypasses the latch — intended for editor "simulate" buttons and
        /// scripted previews. Captures the closed rest rotation the first time it runs in edit mode.
        /// </summary>
        public void SetOpenAmount(float normalized)
        {
            if (interactableObject == null) return;
            if (!_restCaptured)
            {
                _originalRotation = interactableObject.transform.localRotation;
                _originalPosition = interactableObject.transform.localPosition;
                _restCaptured = true;
            }

            currentAngle = Mathf.Lerp(angleRange.x, angleRange.y, Mathf.Clamp01(normalized));
            ApplyRotationToTransform();
            UpdateDebugValues();

            // Only fire gameplay events during play; posing in edit mode must not run listeners.
            if (Application.isPlaying) OnAngleChanged();
        }

        /// <summary>Releases the latch so the door is free to swing.</summary>
        public void Unlock()
        {
            if (!isLocked) return;
            isLocked = false;
            onUnlocked?.Invoke();
        }

        /// <summary>Engages the latch, holding the door shut. Only takes effect while the door is closed.</summary>
        public void Lock()
        {
            if (isLocked || !IsClosed) return;
            isLocked = true;
            onLocked?.Invoke();
        }

        /// <summary>
        /// Captures the hand/door reference for a relative swing. Call once when a grip starts
        /// driving the door so the following <see cref="SwingByHand"/> calls move by the delta from
        /// this point — otherwise grabbing far from the hinge would snap the door open.
        /// </summary>
        public void BeginSwing(Vector3 handWorldPosition)
        {
            _swingRefHandAngle = HandAngleAroundPivot(handWorldPosition);
            _swingRefDoorAngle = currentAngle;
        }

        /// <summary>
        /// Swings the door to follow a hand, relative to the last <see cref="BeginSwing"/>. Used by a
        /// <see cref="DoorHandleInteractable"/> so the door follows the grip that released the latch,
        /// and by the door's own direct grab. No-op while locked.
        /// </summary>
        public void SwingByHand(Vector3 handWorldPosition)
        {
            if (isLocked || IsReturning) return;
            float delta = Mathf.DeltaAngle(_swingRefHandAngle, HandAngleAroundPivot(handWorldPosition));
            currentAngle = ProcessAngle(_swingRefDoorAngle + delta);
            ApplyRotationToTransform();
            UpdateDebugValues();
            OnAngleChanged();
        }

        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (isLocked || !IsSelected || IsReturning) return;

            if (!_swinging)
            {
                BeginSwing(handWorldPosition);
                _swinging = true;
            }
            SwingByHand(handWorldPosition);
        }

        protected override void HandleObjectDeselection()
        {
            base.HandleObjectDeselection();
            _swinging = false;
        }

        /// <summary>Holds the door shut at its closed rest pose while latched; otherwise clamps to the range.</summary>
        protected override float ProcessAngle(float requestedAngle)
        {
            if (isLocked) return 0f;
            return base.ProcessAngle(requestedAngle);
        }

        protected override void OnAngleChanged()
        {
            onMoved?.Invoke(currentNormalizedAngle);

            if (IsOpen)
            {
                if (_lastState != DoorState.Open)
                {
                    _lastState = DoorState.Open;
                    onOpened?.Invoke();
                }
            }
            else if (IsClosed)
            {
                if (_lastState != DoorState.Closed)
                {
                    _lastState = DoorState.Closed;
                    onClosed?.Invoke();
                }
            }
            else
            {
                _lastState = DoorState.Between;
            }
        }
    }
}

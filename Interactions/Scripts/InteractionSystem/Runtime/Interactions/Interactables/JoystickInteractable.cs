using System;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Core;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Turret-style interactable that allows constrained rotation around X (pitch) and Z (roll) axes.
    /// Provides smooth rotation control with configurable limits and return-to-original behavior.
    /// </summary>
    /// <remarks>
    /// This component is ideal for turret-style objects like security cameras, gun turrets,
    /// or any object that needs constrained multi-axis rotation. It supports independent limits 
    /// for X and Z axes, smooth movement with damping, and provides both UnityEvents and UniRx 
    /// observables for rotation changes.
    /// 
    /// Hand movement mapping:
    /// - Hand left/right (relative to turret) controls Z-axis rotation (roll)
    /// - Hand up/down (relative to turret) controls X-axis rotation (pitch)
    /// </remarks>
    [Serializable]
    public class JoystickInteractable : ConstrainedInteractableBase
    {
        [Header("Rotation Limits")]
        [Tooltip("Whether to limit rotation around the X axis (pitch up/down).")]
        [SerializeField] private bool limitXRotation = true;
        [Tooltip("Minimum allowed X rotation angle in degrees (pitch down).")]
        [SerializeField, Range(-90f, 90f)] private float minXAngle = -45f;
        [Tooltip("Maximum allowed X rotation angle in degrees (pitch up).")]
        [SerializeField, Range(-90f, 90f)] private float maxXAngle = 45f;
        
        [Tooltip("Whether to limit rotation around the Z axis (roll left/right).")]
        [SerializeField] private bool limitZRotation = true;
        [Tooltip("Minimum allowed Z rotation angle in degrees (roll left).")]
        [SerializeField, Range(-90f, 90f)] private float minZAngle = -45f;
        [Tooltip("Maximum allowed Z rotation angle in degrees (roll right).")]
        [SerializeField, Range(-90f, 90f)] private float maxZAngle = 45f;

        

        [Header("Return Behavior")]
        [Tooltip("Whether the turret should return to its original rotation when deselected.")]
        [SerializeField] private bool returnToOriginal = false;
        [Tooltip("Speed at which the turret returns to its original rotation.")]
        [SerializeField, Range(1f, 20f)] private float returnSpeed = 5f;

        [Header("Events")]
        [Tooltip("Event raised when the turret's rotation changes (provides current X,Z rotation in degrees).")]
        [SerializeField] private Vector2UnityEvent onRotationChanged = new();
        [Tooltip("Event raised when the turret's X rotation changes (pitch in degrees).")]
        [SerializeField] private FloatUnityEvent onXRotationChanged = new();
        [Tooltip("Event raised when the turret's Z rotation changes (roll in degrees).")]
        [SerializeField] private FloatUnityEvent onZRotationChanged = new();

        [Header("Debug")]
        [Tooltip("Current rotation of the turret in degrees (X=pitch, Z=roll) (read-only).")]
        [ReadOnly, SerializeField] private Vector2 currentRotation = Vector2.zero;
        [Tooltip("Normalized rotation values between 0-1 based on min/max limits (read-only).")]
        [ReadOnly, SerializeField] private Vector2 normalizedRotation = Vector2.zero;

        // Private fields
        private Quaternion _originalRotation;
        private Vector3 _handStartPosition;
        private bool _isReturning = false;

        /// <summary>
        /// Observable that fires when the turret's rotation changes.
        /// </summary>
        /// <value>An observable that emits the current rotation (X=pitch, Z=roll) in degrees.</value>
        public IObservable<Vector2> OnRotationChanged => onRotationChanged.AsObservable();
        
        /// <summary>
        /// Observable that fires when the turret's X rotation (pitch) changes.
        /// </summary>
        /// <value>An observable that emits the current X rotation in degrees.</value>
        public IObservable<float> OnXRotationChanged => onXRotationChanged.AsObservable();
        
        /// <summary>
        /// Observable that fires when the turret's Z rotation (roll) changes.
        /// </summary>
        /// <value>An observable that emits the current Z rotation in degrees.</value>
        public IObservable<float> OnZRotationChanged => onZRotationChanged.AsObservable();

        /// <summary>
        /// Current rotation of the turret in degrees (X=pitch, Z=roll).
        /// </summary>
        /// <value>The current rotation as a Vector2.</value>
        public Vector2 CurrentRotation => currentRotation;
        
        /// <summary>
        /// Normalized rotation values between 0-1 based on min/max limits.
        /// </summary>
        /// <value>Normalized rotation values where 0 = min angle, 1 = max angle.</value>
        public Vector2 NormalizedRotation => normalizedRotation;
        
        // Public properties for editor access
        public bool LimitXRotation => limitXRotation;
        public float MinXAngle
        {
            get => minXAngle;
            set => minXAngle = Mathf.Clamp(value, -90f, maxXAngle - 1f);
        }
        public float MaxXAngle
        {
            get => maxXAngle;
            set => maxXAngle = Mathf.Clamp(value, minXAngle + 1f, 90f);
        }
        public bool LimitZRotation => limitZRotation;
        public float MinZAngle
        {
            get => minZAngle;
            set => minZAngle = Mathf.Clamp(value, -90f, maxZAngle - 1f);
        }
        public float MaxZAngle
        {
            get => maxZAngle;
            set => maxZAngle = Mathf.Clamp(value, minZAngle + 1f, 90f);
        }
        
        public bool ReturnToOriginal => returnToOriginal;
        public float ReturnSpeed => returnSpeed;

        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            UpdateCurrentRotationFromTransform();
        }

        protected override void Activate()
        {
        }

        protected override void StartHover()
        {
        }

        protected override void EndHover()
        {
        }

        protected override void HandleObjectMovement()
        {
            if (!IsSelected) return;
            
            Rotate(CalculateAngle(transform.right), CalculateAngle(transform.forward));
            InvokeEvents();
        }

        protected override void HandleObjectDeselection()
        {
            Rotate(0, 0);
            InvokeEvents();
        }

        private void Update()
        {
            if (IsSelected)
            {
                HandleObjectMovement();
            }
        }
        private void Rotate(float x, float z)
        {
            var angleX = LimitAngle(x, limits.x);
            var angleZ = LimitAngle(z, limits.y);
            interactableObject.transform.localRotation = Quaternion.Euler(angleX, 0, angleZ);
            _currentNormalizedAngle.x = angleX / (limits.x / 2);
            _currentNormalizedAngle.y = angleZ / (limits.y / 2);
        }
        private void InvokeEvents()
        {
            var differenceX = _currentNormalizedAngle.x - _oldNormalizedAngle.x;
            var differenceZ = _currentNormalizedAngle.y - _oldNormalizedAngle.y;
            var absDifference = Mathf.Max(Mathf.Abs(differenceX), Mathf.Abs(differenceZ));
            if (!(Math.Abs(absDifference) > .1f)) return;
            onLeverChanged.Invoke(_currentNormalizedAngle);
            _oldNormalizedAngle = _currentNormalizedAngle;
        }

        private float CalculateAngle(Vector3 plane)
        {
            //-transform.right
            var direction = CurrentInteractor.transform.position - transform.position;
            direction = Vector3.ProjectOnPlane(direction, -plane).normalized;
            var angle = -Vector3.SignedAngle(direction, transform.up, plane);
            return angle;
        }

        private float LimitAngle(float angle, float limit)
        {
            if (angle > limit / 2)
            {
                angle = limit / 2;
            }

            if (angle < -limit / 2)
            {
                angle = -limit / 2;
            }

            return angle;
        }

        #region private classes
        

        [System.Serializable]
        private class Vector2Event : UnityEvent<Vector2>
        {
        }

        #endregion
    }
}
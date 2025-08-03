using System;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Core;
using UniRx;
using UnityEngine.Serialization;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Turret-style interactable that allows constrained rotation around multiple axes.
    /// Provides smooth rotation control with configurable limits and return-to-original behavior.
    /// </summary>
    /// <remarks>
    /// This component is ideal for turret-style objects like security cameras, gun turrets,
    /// or any object that needs constrained rotation. It supports independent limits for X, Y, and Z axes,
    /// smooth return-to-original behavior, and provides both UnityEvents and UniRx observables for rotation changes.
    /// </remarks>
    [Serializable]
    public class TurretInteractable : ConstrainedInteractableBase
    {
        [Header("Rotation Limits")]
        [Tooltip("Whether to limit rotation around the X axis (pitch).")]
        [SerializeField] private bool limitXRotation = true;
        [Tooltip("Minimum allowed X rotation angle in degrees.")]
        [SerializeField, MinMax(-180, 180)] private float minXAngle = -90f;
        [Tooltip("Maximum allowed X rotation angle in degrees.")]
        [SerializeField, MinMax(-180, 180)] private float maxXAngle = 90f;
        
        [Tooltip("Whether to limit rotation around the Y axis (yaw).")]
        [SerializeField] private bool limitYRotation = true;
        [Tooltip("Minimum allowed Y rotation angle in degrees.")]
        [SerializeField, MinMax(-180, 180)] private float minYAngle = -180f;
        [Tooltip("Maximum allowed Y rotation angle in degrees.")]
        [SerializeField, MinMax(-180, 180)] private float maxYAngle = 180f;
        
        [Tooltip("Whether to limit rotation around the Z axis (roll).")]
        [SerializeField] private bool limitZRotation = false;
        [Tooltip("Minimum allowed Z rotation angle in degrees.")]
        [SerializeField, MinMax(-180, 180)] private float minZAngle = -45f;
        [Tooltip("Maximum allowed Z rotation angle in degrees.")]
        [SerializeField, MinMax(-180, 180)] private float maxZAngle = 45f;

        [Header("Return Behavior")]
        [Tooltip("Whether the turret should return to its original rotation when deselected.")]
        [SerializeField] private bool returnToOriginal = false;
        [Tooltip("Speed at which the turret returns to its original rotation (degrees per second).")]
        [SerializeField] private float returnSpeed = 5f;

        [Header("Events")]
        [Tooltip("Event raised when the turret's rotation changes (provides current rotation in degrees).")]
        [SerializeField] private Vector3UnityEvent onRotationChanged = new();
        [Tooltip("Event raised when the turret's X rotation changes (pitch in degrees).")]
        [SerializeField] private FloatUnityEvent onXRotationChanged = new();
        [Tooltip("Event raised when the turret's Y rotation changes (yaw in degrees).")]
        [SerializeField] private FloatUnityEvent onYRotationChanged = new();
        [Tooltip("Event raised when the turret's Z rotation changes (roll in degrees).")]
        [SerializeField] private FloatUnityEvent onZRotationChanged = new();

        [Header("Debug")]
        [Tooltip("Current rotation of the turret in degrees (read-only).")]
        [ReadOnly, SerializeField] private Vector3 currentRotation = Vector3.zero;
        [Tooltip("Normalized rotation values between 0-1 based on min/max limits (read-only).")]
        [ReadOnly, SerializeField] private Vector3 normalizedRotation = Vector3.zero;

        // Private fields
        private Quaternion _originalRotation;
        private Vector3 _oldRotation = Vector3.zero;
        private bool _isReturning = false;

        /// <summary>
        /// Observable that fires when the turret's rotation changes.
        /// </summary>
        /// <value>An observable that emits the current rotation in degrees.</value>
        public IObservable<Vector3> OnRotationChanged => onRotationChanged.AsObservable();
        
        /// <summary>
        /// Observable that fires when the turret's X rotation (pitch) changes.
        /// </summary>
        /// <value>An observable that emits the current X rotation in degrees.</value>
        public IObservable<float> OnXRotationChanged => onXRotationChanged.AsObservable();
        
        /// <summary>
        /// Observable that fires when the turret's Y rotation (yaw) changes.
        /// </summary>
        /// <value>An observable that emits the current Y rotation in degrees.</value>
        public IObservable<float> OnYRotationChanged => onYRotationChanged.AsObservable();
        
        /// <summary>
        /// Observable that fires when the turret's Z rotation (roll) changes.
        /// </summary>
        /// <value>An observable that emits the current Z rotation in degrees.</value>
        public IObservable<float> OnZRotationChanged => onZRotationChanged.AsObservable();

        /// <summary>
        /// Current rotation of the turret in degrees.
        /// </summary>
        /// <value>The current rotation as a Vector3 (X=pitch, Y=yaw, Z=roll).</value>
        public Vector3 CurrentRotation => currentRotation;
        
        /// <summary>
        /// Normalized rotation values between 0-1 based on min/max limits.
        /// </summary>
        /// <value>Normalized rotation values where 0 = min angle, 1 = max angle.</value>
        public Vector3 NormalizedRotation => normalizedRotation;
        
        // Public properties for editor access
        public bool LimitXRotation => limitXRotation;
        public float MinXAngle => minXAngle;
        public float MaxXAngle => maxXAngle;
        public bool LimitYRotation => limitYRotation;
        public float MinYAngle => minYAngle;
        public float MaxYAngle => maxYAngle;
        public bool LimitZRotation => limitZRotation;
        public float MinZAngle => minZAngle;
        public float MaxZAngle => maxZAngle;
        
        // Public setters for editor
        public void SetMinXAngle(float value) => minXAngle = value;
        public void SetMaxXAngle(float value) => maxXAngle = value;
        public void SetMinYAngle(float value) => minYAngle = value;
        public void SetMaxYAngle(float value) => maxYAngle = value;
        public void SetMinZAngle(float value) => minZAngle = value;
        public void SetMaxZAngle(float value) => maxZAngle = value;

        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            
            OnDeselected
                .Where(_ => returnToOriginal)
                .Do(_ => ReturnToOriginal())
                .Do(_ => InvokeEvents())
                .Subscribe().AddTo(this);
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
            
            UpdateRotation();
            InvokeEvents();
            _isReturning = false;
        }

        protected override void HandleObjectDeselection()
        {
            if (returnToOriginal)
            {
                _isReturning = true;
            }
        }

        private void Update()
        {
            if (IsSelected)
            {
                HandleObjectMovement();
            }
            else if (returnToOriginal && _isReturning)
            {
                ReturnToOriginalSmooth();
            }
        }

        private void UpdateRotation()
        {
            if (CurrentInteractor == null) return;

            // Get the direction from turret center to hand
            Vector3 direction = CurrentInteractor.transform.position - transform.position;
            
            // Calculate rotation based on direction
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);
            
            // Extract Euler angles
            Vector3 targetEuler = targetRotation.eulerAngles;
            
            // Normalize angles to -180 to 180 range
            targetEuler.x = NormalizeAngle(targetEuler.x);
            targetEuler.y = NormalizeAngle(targetEuler.y);
            targetEuler.z = NormalizeAngle(targetEuler.z);
            
            // Apply limits
            if (limitXRotation)
                targetEuler.x = Mathf.Clamp(targetEuler.x, minXAngle, maxXAngle);
            if (limitYRotation)
                targetEuler.y = Mathf.Clamp(targetEuler.y, minYAngle, maxYAngle);
            if (limitZRotation)
                targetEuler.z = Mathf.Clamp(targetEuler.z, minZAngle, maxZAngle);
            
            // Apply rotation
            interactableObject.transform.localRotation = Quaternion.Euler(targetEuler);
            
            // Update current rotation
            currentRotation = targetEuler;
            
            // Calculate normalized values
            normalizedRotation = new Vector3(
                limitXRotation ? (targetEuler.x - minXAngle) / (maxXAngle - minXAngle) : 0.5f,
                limitYRotation ? (targetEuler.y - minYAngle) / (maxYAngle - minYAngle) : 0.5f,
                limitZRotation ? (targetEuler.z - minZAngle) / (maxZAngle - minZAngle) : 0.5f
            );
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        private void ReturnToOriginal()
        {
            interactableObject.transform.localRotation = _originalRotation;
            currentRotation = _originalRotation.eulerAngles;
            normalizedRotation = Vector3.zero;
            _oldRotation = Vector3.zero;
        }

        private void ReturnToOriginalSmooth()
        {
            interactableObject.transform.localRotation = Quaternion.Lerp(
                interactableObject.transform.localRotation,
                _originalRotation,
                returnSpeed * Time.deltaTime
            );

            // Update current rotation
            currentRotation = interactableObject.transform.localRotation.eulerAngles;
            
            // Stop returning when close enough
            if (Quaternion.Angle(interactableObject.transform.localRotation, _originalRotation) < 1f)
            {
                _isReturning = false;
                ReturnToOriginal();
            }
        }

        private void InvokeEvents()
        {
            Vector3 difference = currentRotation - _oldRotation;
            float absDifference = difference.magnitude;
            
            if (absDifference < 0.1f) return;
            
            _oldRotation = currentRotation;
            
            // Invoke events
            onRotationChanged.Invoke(currentRotation);
            onXRotationChanged.Invoke(normalizedRotation.x);
            onYRotationChanged.Invoke(normalizedRotation.y);
            onZRotationChanged.Invoke(normalizedRotation.z);
        }

        protected override void DeSelected()
        {
            base.DeSelected();
            if (returnToOriginal)
            {
                _isReturning = true;
            }
        }

        /// <summary>
        /// Sets the turret's rotation to the specified euler angles, respecting rotation limits.
        /// </summary>
        /// <param name="eulerAngles">The target rotation in euler angles (degrees).</param>
        /// <remarks>
        /// This method clamps the rotation values to the configured min/max limits for each axis.
        /// If an axis is not limited, the value is applied directly without clamping.
        /// </remarks>
        public void SetRotation(Vector3 eulerAngles)
        {
            Vector3 clampedAngles = new Vector3(
                limitXRotation ? Mathf.Clamp(eulerAngles.x, minXAngle, maxXAngle) : eulerAngles.x,
                limitYRotation ? Mathf.Clamp(eulerAngles.y, minYAngle, maxYAngle) : eulerAngles.y,
                limitZRotation ? Mathf.Clamp(eulerAngles.z, minZAngle, maxZAngle) : eulerAngles.z
            );
            
            interactableObject.transform.localRotation = Quaternion.Euler(clampedAngles);
            currentRotation = clampedAngles;
        }

        /// <summary>
        /// Sets the turret's rotation using normalized values (0-1) that are mapped to the min/max limits.
        /// </summary>
        /// <param name="normalized">Normalized rotation values where 0 = min angle, 1 = max angle for each axis.</param>
        /// <remarks>
        /// This method is useful for UI sliders or other normalized input systems.
        /// The normalized values are mapped to the configured min/max limits for each axis.
        /// </remarks>
        public void SetNormalizedRotation(Vector3 normalized)
        {
            Vector3 eulerAngles = new Vector3(
                limitXRotation ? Mathf.Lerp(minXAngle, maxXAngle, normalized.x) : 0f,
                limitYRotation ? Mathf.Lerp(minYAngle, maxYAngle, normalized.y) : 0f,
                limitZRotation ? Mathf.Lerp(minZAngle, maxZAngle, normalized.z) : 0f
            );
            
            SetRotation(eulerAngles);
        }
    }
} 
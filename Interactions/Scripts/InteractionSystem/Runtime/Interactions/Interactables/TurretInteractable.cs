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
    public class TurretInteractable : ConstrainedInteractableBase
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
            if (!IsSelected || _isReturning) return;
            
            CalculateAndApplyRotationImmediate();
            UpdateDebugValues();
            InvokeEvents();
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
            if (IsSelected && !_isReturning)
            {
                HandleObjectMovement();
            }
            else if (_isReturning && returnToOriginal)
            {
                HandleReturnToOriginal();
            }
        }

        /// <summary>
        /// Calculates the target rotation based on hand position relative to turret.
        /// </summary>
        private void CalculateAndApplyRotationImmediate()
        {
            if (CurrentInteractor == null) return;

            // Compute hand vector in the coordinate space of the pivot's parent (stable basis)
            Transform pivot = interactableObject.transform;
            Transform basis = pivot.parent != null ? pivot.parent : pivot;

            Vector3 toHandWorld = CurrentInteractor.transform.position - pivot.position;
            Vector3 toHandLocal = basis.InverseTransformDirection(toHandWorld);

            // Calculate angles using atan2 against depth (Z) to get intuitive joystick-like mapping
            // Pitch around local X (right): up/down vs forward depth
            float targetX = -Mathf.Atan2(toHandLocal.y, Mathf.Max(1e-4f, toHandLocal.z)) * Mathf.Rad2Deg;
            // Roll around local Z (forward): left/right vs forward depth (negative to match conventional right-handed roll)
            float targetZ = -Mathf.Atan2(toHandLocal.x, Mathf.Max(1e-4f, toHandLocal.z)) * Mathf.Rad2Deg;

            // Apply limits
            if (limitXRotation)
                targetX = Mathf.Clamp(targetX, minXAngle, maxXAngle);
            if (limitZRotation)
                targetZ = Mathf.Clamp(targetZ, minZAngle, maxZAngle);

            currentRotation = new Vector2(targetX, targetZ);
            ApplyRotationToTransform();
        }

        /// <summary>
        /// Applies the current rotation values to the turret transform.
        /// </summary>
        private void ApplyRotationToTransform()
        {
            // Compose rotation relative to original around local X then local Z
            var xRot = Quaternion.AngleAxis(currentRotation.x, Vector3.right);
            var zRot = Quaternion.AngleAxis(currentRotation.y, Vector3.forward);
            interactableObject.transform.localRotation = _originalRotation * xRot * zRot;
        }

        /// <summary>
        /// Updates current rotation from the transform (used during initialization and return).
        /// </summary>
        private void UpdateCurrentRotationFromTransform()
        {
            Vector3 eulerAngles = interactableObject.transform.localRotation.eulerAngles;
            
            // Normalize angles to -180 to 180 range
            float x = NormalizeAngle(eulerAngles.x);
            float z = NormalizeAngle(eulerAngles.z);
            
            currentRotation = new Vector2(x, z);
        }

        /// <summary>
        /// Updates debug values and normalized rotation.
        /// </summary>
        private void UpdateDebugValues()
        {
            // Calculate normalized values
            normalizedRotation = new Vector2(
                limitXRotation ? Mathf.InverseLerp(minXAngle, maxXAngle, currentRotation.x) : 0.5f,
                limitZRotation ? Mathf.InverseLerp(minZAngle, maxZAngle, currentRotation.y) : 0.5f
            );
        }

        /// <summary>
        /// Handles smooth return to original position.
        /// </summary>
        private void HandleReturnToOriginal()
        {
            // Smoothly return to original rotation
            interactableObject.transform.localRotation = Quaternion.Slerp(
                interactableObject.transform.localRotation,
                _originalRotation,
                returnSpeed * Time.deltaTime
            );

            // Update current rotation from transform
            UpdateCurrentRotationFromTransform();
            UpdateDebugValues();
            InvokeEvents();

            // Stop returning when close enough
            if (Quaternion.Angle(interactableObject.transform.localRotation, _originalRotation) < 1f)
            {
                _isReturning = false;
                interactableObject.transform.localRotation = _originalRotation;
                UpdateCurrentRotationFromTransform();
                // Keep currentRotation synchronized
            }
        }

        /// <summary>
        /// Normalizes an angle to the -180 to 180 degree range.
        /// </summary>
        /// <param name="angle">The angle to normalize.</param>
        /// <returns>The normalized angle.</returns>
        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        /// <summary>
        /// Invokes rotation change events.
        /// </summary>
        private void InvokeEvents()
        {
            onRotationChanged?.Invoke(currentRotation);
            onXRotationChanged?.Invoke(currentRotation.x);
            onZRotationChanged?.Invoke(currentRotation.y);
        }

        /// <summary>
        /// Sets the turret's rotation to the specified angles, respecting rotation limits.
        /// </summary>
        /// <param name="xAngle">The target X rotation (pitch) in degrees.</param>
        /// <param name="zAngle">The target Z rotation (roll) in degrees.</param>
        public void SetRotation(float xAngle, float zAngle)
        {
            Vector2 clampedAngles = new Vector2(
                limitXRotation ? Mathf.Clamp(xAngle, minXAngle, maxXAngle) : xAngle,
                limitZRotation ? Mathf.Clamp(zAngle, minZAngle, maxZAngle) : zAngle
            );
            
            currentRotation = clampedAngles;
            ApplyRotationToTransform();
            UpdateDebugValues();
            InvokeEvents();
        }

        /// <summary>
        /// Sets the turret's rotation using normalized values (0-1) that are mapped to the min/max limits.
        /// </summary>
        /// <param name="normalizedX">Normalized X rotation value where 0 = min angle, 1 = max angle.</param>
        /// <param name="normalizedZ">Normalized Z rotation value where 0 = min angle, 1 = max angle.</param>
        public void SetNormalizedRotation(float normalizedX, float normalizedZ)
        {
            float xAngle = limitXRotation ? Mathf.Lerp(minXAngle, maxXAngle, normalizedX) : 0f;
            float zAngle = limitZRotation ? Mathf.Lerp(minZAngle, maxZAngle, normalizedZ) : 0f;
            
            SetRotation(xAngle, zAngle);
        }

        /// <summary>
        /// Resets the turret to its original rotation.
        /// </summary>
        public void ResetToOriginal()
        {
            interactableObject.transform.localRotation = _originalRotation;
            UpdateCurrentRotationFromTransform();
            _isReturning = false;
            UpdateDebugValues();
            InvokeEvents();
        }

        /// <summary>
        /// Validates the turret configuration in the editor.
        /// </summary>
        private void OnValidate()
        {
            // Ensure min is less than max for rotation limits
            if (limitXRotation && minXAngle >= maxXAngle)
            {
                maxXAngle = minXAngle + 1f;
            }
            
            if (limitZRotation && minZAngle >= maxZAngle)
            {
                maxZAngle = minZAngle + 1f;
            }
            
            // Clamp values to reasonable ranges
            minXAngle = Mathf.Clamp(minXAngle, -90f, 90f);
            maxXAngle = Mathf.Clamp(maxXAngle, -90f, 90f);
            minZAngle = Mathf.Clamp(minZAngle, -90f, 90f);
            maxZAngle = Mathf.Clamp(maxZAngle, -90f, 90f);
            
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Draws gizmos in the scene view to visualize turret configuration.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (interactableObject == null) return;
            
            DrawTurretVisualization();
        }
        
        /// <summary>
        /// Draws selected gizmos with more detail when the object is selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (interactableObject == null) return;
            
            DrawTurretVisualization(true);
            DrawRotationLimits();
        }
        
        /// <summary>
        /// Draws the turret visualization gizmos.
        /// </summary>
        /// <param name="selected">Whether the object is selected (for more detailed visualization).</param>
        private void DrawTurretVisualization(bool selected = false)
        {
            var position = interactableObject.transform.position;
            
            // Draw center point
            Gizmos.color = selected ? Color.yellow : new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(position, 0.02f);
            
            // Dead zone visualization removed
            
            // Draw current rotation indicator
            if (Application.isPlaying)
            {
                var forward = interactableObject.transform.forward;
                Gizmos.color = selected ? Color.green : new Color(0f, 1f, 0f, 0.7f);
                Gizmos.DrawRay(position, forward * 0.3f);
            }
        }
        
        /// <summary>
        /// Draws the rotation limits visualization.
        /// </summary>
        private void DrawRotationLimits()
        {
            var position = interactableObject.transform.position;
            float radius = 0.5f;
            
            // Draw X rotation limits (pitch)
            if (limitXRotation)
            {
                Gizmos.color = Color.red;
                var minRotX = Quaternion.AngleAxis(minXAngle, transform.right);
                var maxRotX = Quaternion.AngleAxis(maxXAngle, transform.right);
                var minDirX = minRotX * transform.forward;
                var maxDirX = maxRotX * transform.forward;
                Gizmos.DrawRay(position, minDirX * radius);
                Gizmos.DrawRay(position, maxDirX * radius);
            }
            
            // Draw Z rotation limits (roll)
            if (limitZRotation)
            {
                Gizmos.color = Color.blue;
                var minRotZ = Quaternion.AngleAxis(minZAngle, transform.forward);
                var maxRotZ = Quaternion.AngleAxis(maxZAngle, transform.forward);
                var minDirZ = minRotZ * transform.up;
                var maxDirZ = maxRotZ * transform.up;
                Gizmos.DrawRay(position, minDirZ * radius);
                Gizmos.DrawRay(position, maxDirZ * radius);
            }
        }
        #endif
    }
}
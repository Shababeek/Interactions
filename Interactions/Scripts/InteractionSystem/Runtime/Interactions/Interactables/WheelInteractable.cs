using System;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Core;
using UniRx;
using UnityEngine.Events;

namespace Shababeek.Interactions
{

    /// <summary>
    /// Wheel-style interactable that tracks rotation around a single axis.
    /// Provides smooth wheel rotation tracking with full rotation counting.
    /// </summary>
    /// <remarks>
    /// This component is ideal for steering wheels, control knobs, or any circular
    /// object that needs to track rotation. It calculates the angle of the hand relative
    /// to the wheel's forward axis and tracks both visual rotation and full rotation counts.
    /// The wheel can be rotated continuously and tracks the total number of rotations.
    /// </remarks>
    public class WheelInteractable : ConstrainedInteractableBase
    {
        [Header("Wheel Settings")]
        [Tooltip("The axis around which the wheel rotates.")]
        [SerializeField] private Axis rotationAxis = Axis.Z;
        

        
        [Header("Wheel Behavior")]
        [Tooltip("Return to starting position when deselected.")]
        [SerializeField] private bool returnToStart = false;
        [Tooltip("Speed of return animation (degrees per second).")]
        [SerializeField] private float returnSpeed = 90f;
        
        [Header("Events")]
        [Tooltip("Event raised when the wheel rotates (provides current angle).")]
        [SerializeField] private FloatUnityEvent onWheelAngleChanged;
        [Tooltip("Event raised when the wheel completes a full rotation (provides rotation count).")]
        [SerializeField] private FloatUnityEvent onWheelRotated;
        
        [Header("Debug")]
        [Tooltip("Current rotation angle in degrees (read-only).")]
        [ReadOnly] [SerializeField] private float currentAngle = 0f;
        [Tooltip("Total number of full rotations completed (read-only).")]
        [ReadOnly] [SerializeField] private float currentRotation = 0f;

        private float _lastAngle = 0f;
        private float _accumulatedAngle = 0f;
        private float _totalRotationAngle = 0f;
        private float _startingAngle = 0f;
        private bool _isReturning = false;

        /// <summary>
        /// Observable that fires when the wheel angle changes.
        /// </summary>
        /// <value>An observable that emits the current angle in degrees.</value>
        public IObservable<float> OnWheelAngleChanged => onWheelAngleChanged.AsObservable();
        
        /// <summary>
        /// Observable that fires when the wheel completes a full rotation.
        /// </summary>
        /// <value>An observable that emits the total rotation count (can be negative for counter-clockwise).</value>
        public IObservable<float> OnWheelRotated => onWheelRotated.AsObservable();
        


        // Public properties for editor access
        public Axis RotationAxis => rotationAxis;
        public bool ReturnToStart => returnToStart;
        public float ReturnSpeed => returnSpeed;
        public float CurrentAngle => currentAngle;
        public float CurrentRotation => currentRotation;

        protected override void HandleObjectMovement()
        {
            if (!IsSelected || _isReturning) return;

            // Calculate the angle of the hand relative to the wheel's rotation axis
            float targetAngle = CalculateHandAngle();

            // Calculate the change in angle since last frame
            float deltaAngle = Mathf.DeltaAngle(_lastAngle, targetAngle);
            _totalRotationAngle += deltaAngle;
            _lastAngle = targetAngle;
            currentAngle = _totalRotationAngle;

            // Apply visual rotation based on configured axis
            ApplyVisualRotation();

            // Track full rotations
            TrackFullRotations();

            // Fire angle changed event
            onWheelAngleChanged?.Invoke(currentAngle);
        }

        protected override void HandleObjectDeselection()
        {
            if (returnToStart && !_isReturning)
            {
                _isReturning = true;
            }
        }

        private void Start()
        {
            _startingAngle = _totalRotationAngle;
        }

        private void Update()
        {
            if (IsSelected && !_isReturning)
            {
                HandleObjectMovement();
            }
            else if (_isReturning && returnToStart)
            {
                HandleReturnToStart();
            }
        }

        /// <summary>
        /// Calculates the angle of the hand relative to the wheel's rotation axis.
        /// </summary>
        /// <returns>The angle in degrees between the hand position and the reference vector.</returns>
        /// <remarks>
        /// This method projects the hand position onto a plane perpendicular to the wheel's rotation axis,
        /// then calculates the signed angle between the projected direction and the reference vector.
        /// The result is used to determine the wheel's rotation and track full rotations.
        /// </remarks>
        private float CalculateHandAngle()
        {
            // Get direction from wheel center to hand
            Vector3 handDirection = CurrentInteractor.transform.position - transform.position;
            
            // Get rotation axis and reference vectors based on configured axis
            Vector3 rotationAxisVector = GetRotationAxisVector();
            Vector3 referenceVector = GetReferenceVector();
            
            // Project the hand direction onto the plane perpendicular to the rotation axis
            Vector3 projectedDirection = Vector3.ProjectOnPlane(handDirection, rotationAxisVector);
            
            // Calculate the angle between the projected direction and the reference vector
            float angle = Vector3.SignedAngle(referenceVector, projectedDirection, rotationAxisVector);
            
            return angle;
        }
        
        /// <summary>
        /// Gets the rotation axis vector based on the configured rotation axis.
        /// </summary>
        /// <returns>The world space vector for the rotation axis.</returns>
        private Vector3 GetRotationAxisVector()
        {
            return rotationAxis switch
            {
                Axis.X => transform.right,
                Axis.Y => transform.up,
                Axis.Z => transform.forward,
                _ => transform.forward
            };
        }
        
        /// <summary>
        /// Gets the reference vector for angle calculation based on the rotation axis.
        /// </summary>
        /// <returns>The world space reference vector.</returns>
        private Vector3 GetReferenceVector()
        {
            return rotationAxis switch
            {
                Axis.X => transform.forward,  // For X rotation, use forward as reference
                Axis.Y => transform.forward,  // For Y rotation, use forward as reference
                Axis.Z => transform.up,       // For Z rotation, use up as reference
                _ => transform.up
            };
        }
        
        /// <summary>
        /// Applies visual rotation to the wheel based on the configured rotation axis.
        /// </summary>
        private void ApplyVisualRotation()
        {
            var currentRotation = interactableObject.transform.localRotation.eulerAngles;
            var newRotation = rotationAxis switch
            {
                Axis.X => new Vector3(_totalRotationAngle, currentRotation.y, currentRotation.z),
                Axis.Y => new Vector3(currentRotation.x, _totalRotationAngle, currentRotation.z),
                Axis.Z => new Vector3(currentRotation.x, currentRotation.y, _totalRotationAngle),
                _ => new Vector3(currentRotation.x, currentRotation.y, _totalRotationAngle)
            };
            
            interactableObject.transform.localRotation = Quaternion.Euler(newRotation);
        }
        
        /// <summary>
        /// Tracks full rotations and fires events when complete rotations occur.
        /// </summary>
        private void TrackFullRotations()
        {
            _accumulatedAngle += Mathf.DeltaAngle(_accumulatedAngle, _totalRotationAngle);
            
            // Check if we've completed a full rotation
            float fullRotations = Mathf.Floor(_accumulatedAngle / 360f);
            if (Mathf.Abs(fullRotations) >= 1f)
            {
                currentRotation += fullRotations;
                _accumulatedAngle -= fullRotations * 360f;
                onWheelRotated?.Invoke(currentRotation);
            }
        }
        
        /// <summary>
        /// Handles the return to start animation when the wheel is deselected.
        /// </summary>
        private void HandleReturnToStart()
        {
            if (!_isReturning) return;
            
            float targetAngle = _startingAngle;
            float currentAngleValue = _totalRotationAngle;
            
            // Move towards the starting angle
            float step = returnSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(currentAngleValue, targetAngle, step);
            
            _totalRotationAngle = newAngle;
            currentAngle = _totalRotationAngle;
            
            // Apply visual rotation
            ApplyVisualRotation();
            
            // Check if we've reached the starting position
            if (Mathf.Approximately(newAngle, targetAngle))
            {
                _isReturning = false;
                _totalRotationAngle = targetAngle;
                currentAngle = _totalRotationAngle;
                ApplyVisualRotation();
            }
            
            // Fire angle changed event
            onWheelAngleChanged?.Invoke(currentAngle);
        }
        
        /// <summary>
        /// Resets the wheel to its starting position.
        /// </summary>
        public void ResetWheel()
        {
            _totalRotationAngle = _startingAngle;
            _accumulatedAngle = 0f;
            currentRotation = 0f;
            currentAngle = _totalRotationAngle;
            _isReturning = false;
            ApplyVisualRotation();
        }
        
        /// <summary>
        /// Sets the wheel to a specific angle.
        /// </summary>
        /// <param name="angle">The target angle in degrees.</param>
        public void SetWheelAngle(float angle)
        {
            _totalRotationAngle = angle;
            currentAngle = _totalRotationAngle;
            ApplyVisualRotation();
            onWheelAngleChanged?.Invoke(currentAngle);
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
        
        /// <summary>
        /// Validates the wheel configuration in the editor.
        /// </summary>
        private void OnValidate()
        {
            // Ensure return speed is positive
            if (returnSpeed <= 0)
            {
                returnSpeed = 90f;
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Draws gizmos in the scene view to visualize wheel configuration.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (interactableObject == null) return;
            
            DrawWheelVisualization();
        }
        
        /// <summary>
        /// Draws selected gizmos with more detail when the object is selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (interactableObject == null) return;
            
            DrawWheelVisualization(true);
        }
        
        /// <summary>
        /// Draws the wheel visualization gizmos.
        /// </summary>
        /// <param name="selected">Whether the object is selected (for more detailed visualization).</param>
        private void DrawWheelVisualization(bool selected = false)
        {
            var position = interactableObject.transform.position;
            var rotationAxisVector = GetRotationAxisVector();
            var referenceVector = GetReferenceVector();
            
            // Draw rotation axis
            Gizmos.color = selected ? Color.yellow : new Color(1f, 1f, 0f, 0.5f);
            float axisLength = selected ? 1f : 0.5f;
            Gizmos.DrawRay(position, rotationAxisVector * axisLength);
            Gizmos.DrawRay(position, -rotationAxisVector * axisLength);
            
            // Draw reference direction
            Gizmos.color = selected ? Color.cyan : new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawRay(position, referenceVector * 0.5f);
            
            // Draw current rotation indicator
            if (Application.isPlaying)
            {
                var currentRotationQuat = Quaternion.AngleAxis(currentAngle, rotationAxisVector);
                var currentDir = currentRotationQuat * referenceVector;
                Gizmos.color = selected ? Color.green : new Color(0f, 1f, 0f, 0.7f);
                Gizmos.DrawRay(position, currentDir * 0.7f);
            }
        }
        
        // Rotation limits visualization intentionally removed (limits are disabled for now)
        #endif
    }
} 
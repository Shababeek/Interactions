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
        [Tooltip("Event raised when the wheel completes a full rotation (provides rotation count).")]
        [SerializeField] private FloatUnityEvent onWheelRotated;
        
        [Header("Debug")]
        [Tooltip("Total number of full rotations completed (read-only).")]
        [ReadOnly] [SerializeField] private float currentRotation = 0f;
        [Tooltip("Current angle of the hand relative to the wheel in degrees (read-only).")]
        [ReadOnly] [SerializeField] private float currentAngle = 0f;

        private float _lastAngle = 0f;
        private float _accumulatedAngle = 0f;
        private float _totalRotationAngle = 0f;

        /// <summary>
        /// Observable that fires when the wheel completes a full rotation.
        /// </summary>
        /// <value>An observable that emits the total rotation count (can be negative for counter-clockwise).</value>
        public IObservable<float> OnWheelRotated => onWheelRotated.AsObservable();

        protected override void HandleObjectMovement()
        {
            if (!IsSelected) return;
            
            // Calculate the angle of the hand relative to the wheel's forward axis
            float angle = CalculateHandAngle();
            
            // Calculate the change in angle since last frame
            float deltaAngle = Mathf.DeltaAngle(_lastAngle, angle);
            _accumulatedAngle += deltaAngle;
            _totalRotationAngle += deltaAngle;
            _lastAngle = angle;

            // Rotate the wheel visually
            interactableObject.transform.localRotation = Quaternion.AngleAxis(_totalRotationAngle, Vector3.forward);

            // Check if we've completed a full rotation
            float fullRotations = Mathf.Floor(_accumulatedAngle / 360f);
            if (Mathf.Abs(fullRotations) >= 1f)
            {
                currentRotation += fullRotations;
                _accumulatedAngle -= fullRotations * 360f;
                onWheelRotated?.Invoke(currentRotation);
            }
            
            currentAngle = angle;
        }

        protected override void HandleObjectDeselection()
        {
            // Wheel doesn't need any special deselection logic
        }

        private void Update()
        {
            if (IsSelected)
            {
                HandleObjectMovement();
            }
        }

        /// <summary>
        /// Calculates the angle of the hand relative to the wheel's forward axis.
        /// </summary>
        /// <returns>The angle in degrees between the hand position and the wheel's up vector.</returns>
        /// <remarks>
        /// This method projects the hand position onto a plane perpendicular to the wheel's forward axis,
        /// then calculates the signed angle between the projected direction and the wheel's up vector.
        /// The result is used to determine the wheel's rotation and track full rotations.
        /// </remarks>
        private float CalculateHandAngle()
        {
            // Get direction from wheel center to hand
            Vector3 handDirection = CurrentInteractor.transform.position - transform.position;
            
            // Project the hand direction onto the plane perpendicular to the wheel's forward axis
            Vector3 projectedDirection = Vector3.ProjectOnPlane(handDirection, transform.forward);
            
            // Calculate the angle between the projected direction and the wheel's up vector
            float angle = Vector3.SignedAngle(transform.up, projectedDirection, transform.forward);
            
            return angle;
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


    }
} 
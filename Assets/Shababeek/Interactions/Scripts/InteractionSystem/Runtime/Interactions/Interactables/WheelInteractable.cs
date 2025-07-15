using System;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Core;
using UniRx;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    [Serializable]
    public class FloatUnityEvent : UnityEvent<float>
    {
    }

    public class WheelInteractable : ConstrainedInteractableBase
    {
        [Header("Wheel Settings")]
        [SerializeField] private FloatUnityEvent onWheelRotated;
        
        [Header("Debug")]
        [ReadOnly] [SerializeField] private float currentRotation = 0f;
        [ReadOnly] [SerializeField] private float currentAngle = 0f;

        private float _lastAngle = 0f;
        private float _accumulatedAngle = 0f;
        private float _totalRotationAngle = 0f;

        public IObservable<float> OnWheelRotated => onWheelRotated.AsObservable();

        private void Update()
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
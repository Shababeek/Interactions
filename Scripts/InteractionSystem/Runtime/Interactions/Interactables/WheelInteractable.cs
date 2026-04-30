using System;
using UnityEngine;
using Shababeek.ReactiveVars;
using Shababeek.Interactions.Core;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Wheel interactable for steering wheels and similar rotating objects.
    /// Continuously rotatable up to a configured number of full rotations in each direction,
    /// returning smoothly to zero on release.
    /// </summary>
    public class WheelInteractable : RotaryInteractableBase
    {
        [Header("Rotation Limits")]
        [Tooltip("Maximum rotations in each direction (0.5 = half turn, 1 = full turn).")]
        [SerializeField] private float maxRotations = 1f;

        [Header("Events")]
        [Tooltip("Event invoked when wheel rotation angle changes, passing the angle in degrees.")]
        [SerializeField] private FloatUnityEvent onAngleChanged = new();

        [Tooltip("Event invoked when normalized rotation value changes (-1 to 1).")]
        [SerializeField] private FloatUnityEvent onNormalizedChanged = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private float normalizedValue;

        private float _returnTimer;

        /// <summary>Observable fired when the wheel angle changes.</summary>
        public IObservable<float> OnAngleChanged => onAngleChanged.AsObservable();

        /// <summary>Observable fired when the normalized rotation value changes.</summary>
        public IObservable<float> OnNormalizedChanged => onNormalizedChanged.AsObservable();

        /// <summary>Normalized rotation value (-1 to 1) based on current angle and max rotations.</summary>
        public float NormalizedValue => normalizedValue;

        /// <summary>Maximum number of rotations in each direction.</summary>
        public float MaxRotations => maxRotations;

        private float MaxAngle => maxRotations * 360f;
        private float MinAngle => -maxRotations * 360f;

        protected override float ProcessAngleDelta(float currentAngle, float delta)
        {
            return Mathf.Clamp(currentAngle + delta, MinAngle, MaxAngle);
        }

        protected override void OnAngleApplied(float newAngle)
        {
            UpdateNormalizedValue();
            InvokeEvents();
        }

        protected override void OnGrabAlignment(HandIdentifier handIdentifier)
        {
            if (grabMode != WheelGrabMode.ObjectFollowsHand) return;

            float grabHandAngle = GetHandAngle(CurrentInteractor.transform.position);
            float constraintAngle = GetConstraintAngle(handIdentifier);
            float targetWheelAngle = grabHandAngle - constraintAngle;

            currentAngle = Mathf.Clamp(targetWheelAngle, MinAngle, MaxAngle);
            ApplyRotation();
            UpdateNormalizedValue();
            InvokeEvents();
        }

        protected override void HandleObjectDeselection()
        {
            base.HandleObjectDeselection();
            _returnTimer = 0f;
        }

        protected override void HandleReturnToOriginalPosition()
        {
            _returnTimer += Time.deltaTime * returnSpeed;
            currentAngle = Mathf.Lerp(currentAngle, 0f, _returnTimer);

            ApplyRotation();
            UpdateNormalizedValue();
            InvokeEvents();

            if (Mathf.Abs(currentAngle) < 0.5f)
            {
                currentAngle = 0f;
                normalizedValue = 0f;
                IsReturning = false;
                interactableObject.transform.localRotation = _originalRotation;
            }
        }

        /// <summary>Sets the wheel to a specific angle in degrees, clamped to the rotation limits.</summary>
        public void SetAngle(float angle)
        {
            currentAngle = Mathf.Clamp(angle, MinAngle, MaxAngle);
            ApplyRotation();
            UpdateNormalizedValue();
            InvokeEvents();
        }

        /// <summary>Sets the wheel using a normalized value (-1 to 1).</summary>
        public void SetNormalized(float value)
        {
            SetAngle(value * MaxAngle);
        }

        /// <summary>Resets the wheel to its original (zero) rotation.</summary>
        public void ResetWheel()
        {
            currentAngle = 0f;
            normalizedValue = 0f;
            IsReturning = false;
            interactableObject.transform.localRotation = _originalRotation;
            InvokeEvents();
        }

        private void UpdateNormalizedValue()
        {
            normalizedValue = MaxAngle > 0 ? currentAngle / MaxAngle : 0f;
        }

        private void InvokeEvents()
        {
            onAngleChanged?.Invoke(currentAngle);
            onNormalizedChanged?.Invoke(normalizedValue);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            maxRotations = Mathf.Max(0.1f, maxRotations);
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        private void OnDrawGizmosSelected()
        {
            if (interactableObject == null) return;

            var pos = interactableObject.transform.position;
            var axis = GetWorldAxis();

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pos, axis * 0.2f);
            Gizmos.DrawRay(pos, -axis * 0.2f);

            var t = interactableObject.transform;
            Vector3 reference = rotationAxis switch
            {
                RotationAxis.Right => t.forward,
                RotationAxis.Up => t.right,
                _ => t.right
            };

            Gizmos.color = Color.red;
            var minRot = Quaternion.AngleAxis(-maxRotations * 360f, axis);
            Gizmos.DrawRay(pos, minRot * reference * 0.3f);

            Gizmos.color = Color.blue;
            var maxRot = Quaternion.AngleAxis(maxRotations * 360f, axis);
            Gizmos.DrawRay(pos, maxRot * reference * 0.3f);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                var curRot = Quaternion.AngleAxis(currentAngle, axis);
                Gizmos.DrawRay(pos, curRot * reference * 0.25f);
            }
        }
    }
}

using System;
using UnityEngine;
using Shababeek.Utilities;
using Shababeek.Interactions.Core;
using UniRx;

namespace Shababeek.Interactions
{
    public enum WheelGrabMode
    {
        ObjectFollowsHand,
        HandFollowsObject
    }

    /// <summary>
    /// Wheel interactable for steering wheels and similar rotating objects.
    /// Both modes rotate the wheel when moving the hand - the difference is initial grab behavior.
    /// </summary>
    public class WheelInteractable : ConstrainedInteractableBase
    {
        [Header("Wheel Settings")]
        [SerializeField] private WheelGrabMode grabMode = WheelGrabMode.ObjectFollowsHand;
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Forward;

        [Header("Rotation Limits")]
        [Tooltip("Maximum rotations in each direction (0.5 = half turn, 1 = full turn)")]
        [SerializeField] private float maxRotations = 1f;
        
        [Header("Events")]
        [SerializeField] private FloatUnityEvent onAngleChanged = new();
        [SerializeField] private FloatUnityEvent onNormalizedChanged = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private float currentAngle;
        [ReadOnly, SerializeField] private float normalizedValue;

        private Quaternion _originalRotation;
        private float _previousHandAngle;
        private float _returnTimer;

        // For HandFollowsObject mode
        private Transform _fakeHand;
        private float _fakeHandOrbitAngle;

        public IObservable<float> OnAngleChanged => onAngleChanged.AsObservable();
        public IObservable<float> OnNormalizedChanged => onNormalizedChanged.AsObservable();

        public float CurrentAngle => currentAngle;
        public float NormalizedValue => normalizedValue;
        public float MaxRotations => maxRotations;
        public WheelGrabMode GrabMode => grabMode;
        public RotationAxis RotationAxis => rotationAxis;

        private float MaxAngle => maxRotations * 360f;
        private float MinAngle => -maxRotations * 360f;

        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            PoseConstrainer = GetComponent<PoseConstrainter>();
        }

        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!IsSelected || IsReturning) return;

            var handAngle = GetHandAngle(handWorldPosition);

            var delta = Mathf.DeltaAngle(_previousHandAngle, handAngle);
            _previousHandAngle = handAngle;

            var newAngle = currentAngle + delta;
            currentAngle = Mathf.Clamp(newAngle, MinAngle, MaxAngle);

            if (grabMode == WheelGrabMode.HandFollowsObject && _fakeHand != null)
            {
                _fakeHandOrbitAngle += delta;
                UpdateFakeHandOrbit();
            }

            ApplyWheelRotation();
            UpdateNormalizedValue();
            InvokeEvents();
        }

        /// <summary>
        /// Gets the angle of hand position on the rotation plane using the interactable's parent space.
        /// </summary>
        private float GetHandAngle(Vector3 handWorldPosition)
        {
            // Use parent transform space (not interactableObject which rotates)
            Vector3 localPos = transform.InverseTransformPoint(handWorldPosition);

            float x, y;
            switch (rotationAxis)
            {
                case RotationAxis.Right:  x = localPos.z; y = localPos.y; break;
                case RotationAxis.Up:     x = localPos.x; y = localPos.z; break;
                case RotationAxis.Forward:
                default:                  x = localPos.x; y = localPos.y; break;
            }

            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        private void ApplyWheelRotation()
        {
            Vector3 axis = GetAxis();
            Quaternion rot = Quaternion.AngleAxis(currentAngle, axis);
            interactableObject.transform.localRotation = _originalRotation * rot;
        }

        private Vector3 GetAxis()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => Vector3.right,
                RotationAxis.Up => Vector3.up,
                _ => Vector3.forward
            };
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

        #region Grab Handling

        protected override void PositionFakeHand(Transform fakeHand, HandIdentifier handIdentifier)
        {
            _fakeHand = fakeHand;

            // Get hand angle at grab moment
            float grabHandAngle = GetHandAngle(CurrentInteractor.transform.position);
            _previousHandAngle = grabHandAngle;

            if (grabMode == WheelGrabMode.ObjectFollowsHand)
            {
                float constraintAngle = GetConstraintAngle(handIdentifier);
                float targetWheelAngle = grabHandAngle - constraintAngle;

                currentAngle = Mathf.Clamp(targetWheelAngle, MinAngle, MaxAngle);
                ApplyWheelRotation();
                UpdateNormalizedValue();

                base.PositionFakeHand(fakeHand, handIdentifier);
            }
            else
            {
                _fakeHandOrbitAngle = grabHandAngle;
                UpdateFakeHandOrbit();
            }

            InvokeEvents();
        }

        /// <summary>
        /// Gets the angle of the constraint position on the rotation plane.
        /// </summary>
        private float GetConstraintAngle(HandIdentifier handIdentifier)
        {
            if (PoseConstrainer == null) return 0f;

            var positioning = PoseConstrainer.GetTargetHandTransform(handIdentifier);
            Vector3 constraintLocal = positioning.position;

            float x, y;
            switch (rotationAxis)
            {
                case RotationAxis.Right:  x = constraintLocal.z; y = constraintLocal.y; break;
                case RotationAxis.Up:     x = constraintLocal.x; y = constraintLocal.z; break;
                case RotationAxis.Forward:
                default:                  x = constraintLocal.x; y = constraintLocal.y; break;
            }

            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        private void UpdateFakeHandOrbit()
        {
            if (_fakeHand == null || PoseConstrainer == null) return;

            var basePose = PoseConstrainer.GetTargetHandTransform(CurrentInteractor.HandIdentifier);
            var orbitRadius = basePose.position.magnitude;
            float rad = _fakeHandOrbitAngle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad) * orbitRadius;
            float sin = Mathf.Sin(rad) * orbitRadius;

            Vector3 offset = rotationAxis switch
            {
                RotationAxis.Right => new Vector3(0, sin, cos),
                RotationAxis.Up => new Vector3(cos, 0, sin),
                _ => new Vector3(cos, sin, 0)
            };

            _fakeHand.position = transform.TransformPoint(offset);

            // Rotate hand to match orbit position
            Quaternion orbitRot = Quaternion.AngleAxis(_fakeHandOrbitAngle, GetAxis());
            _fakeHand.localRotation = orbitRot * basePose.rotation;
        }

        #endregion

        #region Deselection / Return

        protected override void HandleObjectDeselection()
        {
            _returnTimer = 0f;
            _fakeHand = null;
        }

        protected override void HandleReturnToOriginalPosition()
        {
            _returnTimer += Time.deltaTime * returnSpeed;
            currentAngle = Mathf.Lerp(currentAngle, 0f, _returnTimer);

            ApplyWheelRotation();
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

        #endregion

        #region Public API

        public void SetAngle(float angle)
        {
            currentAngle = Mathf.Clamp(angle, MinAngle, MaxAngle);
            ApplyWheelRotation();
            UpdateNormalizedValue();
            InvokeEvents();
        }

        public void SetNormalized(float value)
        {
            SetAngle(value * MaxAngle);
        }

        public void ResetWheel()
        {
            currentAngle = 0f;
            normalizedValue = 0f;
            IsReturning = false;
            interactableObject.transform.localRotation = _originalRotation;
            InvokeEvents();
        }

        #endregion

        #region Editor

        public Vector3 GetWorldAxis()
        {
            var t = interactableObject.transform;
            return rotationAxis switch
            {
                RotationAxis.Right => t.right,
                RotationAxis.Up => t.up,
                _ => t.forward
            };
        }

        private void OnValidate()
        {
            maxRotations = Mathf.Max(0.1f, maxRotations);
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        private void OnDrawGizmosSelected()
        {
            if (interactableObject == null) return;

            var pos = interactableObject.transform.position;
            var axis = GetWorldAxis();

            // Draw axis
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pos, axis * 0.2f);
            Gizmos.DrawRay(pos, -axis * 0.2f);

            // Draw rotation limits
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

        #endregion
    }
}

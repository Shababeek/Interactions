using System;
using UnityEngine;
using Shababeek.Utilities;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Wheel-style interactable that tracks rotation around a single axis with optional limits.
    /// Provides smooth wheel rotation tracking with full rotation counting.
    /// </summary>
    public class WheelInteractable : ConstrainedInteractableBase
    {
        [Header("Wheel Settings")]
        [Tooltip("The axis around which the wheel rotates")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Forward;

        [Header("Rotation Limits")]
        [Tooltip("Enable rotation limits")]
        [SerializeField] private bool limitRotations = false;

        [Tooltip("Rotation limit range (min rotations, max rotations)")]
        [SerializeField] private Vector2 rotationLimits = new Vector2(-2f, 2f);

        [Header("Events")]
        [SerializeField] private FloatUnityEvent onWheelAngleChanged = new();
        [SerializeField] private FloatUnityEvent onWheelRotated = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private float currentAngle = 0f;
        [ReadOnly, SerializeField] private float totalRotations = 0f;

        private Quaternion _originalRotation;
        private float _lastAngle = 0f;
        private float _accumulatedAngle = 0f;
        private float _returnTimer;

        /// <summary>
        /// Observable that fires when the wheel angle changes.
        /// </summary>
        public IObservable<float> OnWheelAngleChanged => onWheelAngleChanged.AsObservable();

        /// <summary>
        /// Observable that fires when the wheel completes a full rotation.
        /// </summary>
        public IObservable<float> OnWheelRotated => onWheelRotated.AsObservable();

        public RotationAxis RotationAxis => rotationAxis;
        public float CurrentAngle => currentAngle;
        public float TotalRotations => totalRotations;
        public bool LimitRotations => limitRotations;

        public Vector2 RotationLimits
        {
            get => rotationLimits;
            set
            {
                rotationLimits = new Vector2(
                    Mathf.Min(value.x, value.y - 0.1f),
                    Mathf.Max(value.y, value.x + 0.1f)
                );
            }
        }

        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            _lastAngle = 0f;
            _accumulatedAngle = 0f;
        }

        protected override void HandleObjectMovement(Vector3 target)
        {
            if (!IsSelected || IsReturning) return;

            CalculateAndApplyRotation(target);
            InvokeEvents();
        }

        protected override void HandleObjectDeselection()
        {
            _returnTimer = 0f;
        }

        protected override void UseStarted()
        {
        }

        protected override void StartHover()
        {
        }

        protected override void EndHover()
        {
        }

        private void CalculateAndApplyRotation(Vector3 handWorldPosition)
        {
            if (CurrentInteractor == null) return;

            Transform pivot = interactableObject.transform;

            Vector3 direction = handWorldPosition - pivot.position;
            direction = transform.InverseTransformDirection(direction);

            Vector3 axisNormal = GetLocalAxis();
            Vector3 referenceVector = GetReferenceVector();

            Vector3 projected = Vector3.ProjectOnPlane(direction, axisNormal);

            if (projected.sqrMagnitude < 0.0001f)
            {
                return;
            }

            // Calculate angle from reference vector
            float targetAngle = Vector3.SignedAngle(referenceVector, projected.normalized, axisNormal);

            float deltaAngle = Mathf.DeltaAngle(_lastAngle, targetAngle);
            _accumulatedAngle += deltaAngle;
            _lastAngle = targetAngle;

            TrackFullRotations();

            if (limitRotations)
            {
                float maxAngle = rotationLimits.y * 360f;
                float minAngle = rotationLimits.x * 360f;
                _accumulatedAngle = Mathf.Clamp(_accumulatedAngle, minAngle, maxAngle);
            }

            currentAngle = _accumulatedAngle;
            ApplyRotationToTransform();
        }

        private Vector3 GetLocalAxis()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => Vector3.right,
                RotationAxis.Up => Vector3.up,
                RotationAxis.Forward => Vector3.forward,
                _ => Vector3.forward
            };
        }

        private Vector3 GetReferenceVector()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => Vector3.forward,
                RotationAxis.Up => Vector3.forward,
                RotationAxis.Forward => Vector3.up,
                _ => Vector3.up
            };
        }

        private void ApplyRotationToTransform()
        {
            var localAxis = GetLocalAxis();
            var relative = Quaternion.AngleAxis(_accumulatedAngle, localAxis);
            interactableObject.transform.localRotation = _originalRotation * relative;
        }

        private void TrackFullRotations()
        {
            // Calculate total rotations (positive = clockwise, negative = counter-clockwise)
            float newTotalRotations = _accumulatedAngle / 360f;
            
            // Check if we crossed a full rotation threshold
            if (Mathf.Floor(Mathf.Abs(newTotalRotations)) != Mathf.Floor(Mathf.Abs(totalRotations)))
            {
                onWheelRotated?.Invoke(newTotalRotations);
            }
            
            totalRotations = newTotalRotations;
        }

        protected override void HandleReturnToOriginalPosition()
        {
            _returnTimer += Time.deltaTime * returnSpeed;

            // Lerp accumulated angle back to 0
            _accumulatedAngle = Mathf.Lerp(_accumulatedAngle, 0f, _returnTimer);
            currentAngle = _accumulatedAngle;

            ApplyRotationToTransform();
            TrackFullRotations();
            InvokeEvents();

            if (Mathf.Abs(_accumulatedAngle) < 0.1f)
            {
                IsReturning = false;
                _accumulatedAngle = 0f;
                _lastAngle = 0f;
                currentAngle = 0f;
                totalRotations = 0f;
                interactableObject.transform.localRotation = _originalRotation;
            }
        }

        private void InvokeEvents()
        {
            onWheelAngleChanged?.Invoke(currentAngle);
        }


        public void ResetWheel()
        {
            _accumulatedAngle = 0f;
            _lastAngle = 0f;
            currentAngle = 0f;
            totalRotations = 0f;
            IsReturning = false;
            interactableObject.transform.localRotation = _originalRotation;
            InvokeEvents();
        }


        public void SetWheelAngle(float angle)
        {
            _accumulatedAngle = angle;
            currentAngle = angle;
            TrackFullRotations();
            ApplyRotationToTransform();
            InvokeEvents();
        }

 
        public void SetWheelRotations(float rotations)
        {
            SetWheelAngle(rotations * 360f);
        }


        public Vector3 GetRotationAxisVector()
        {
            var t = interactableObject.transform;
            return rotationAxis switch
            {
                RotationAxis.Right => t.right,
                RotationAxis.Up => t.up,
                RotationAxis.Forward => t.forward,
                _ => t.forward
            };
        }


        public Vector3 GetWorldReferenceVector()
        {
            var t = interactableObject.transform;
            return rotationAxis switch
            {
                RotationAxis.Right => t.forward,
                RotationAxis.Up => t.forward,
                RotationAxis.Forward => t.up,
                _ => t.up
            };
        }

        private void OnValidate()
        {
            if (limitRotations)
            {
                // Ensure min < max
                if (rotationLimits.x >= rotationLimits.y)
                {
                    rotationLimits.y = rotationLimits.x + 0.1f;
                }
            }

            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        private void OnDrawGizmos()
        {
            if (interactableObject == null) return;
            DrawWheelVisualization();
        }

        private void OnDrawGizmosSelected()
        {
            if (interactableObject == null) return;
            DrawWheelVisualization(true);
            if (limitRotations) DrawRotationLimits();
        }

        private void DrawWheelVisualization(bool selected = false)
        {
            var position = interactableObject.transform.position;
            var axis = GetRotationAxisVector();
            var reference = GetWorldReferenceVector();

            // Draw rotation axis
            Gizmos.color = selected ? Color.yellow : new Color(1f, 1f, 0f, 0.5f);
            float axisLength = selected ? 1f : 0.5f;
            Gizmos.DrawRay(position, axis * axisLength);
            Gizmos.DrawRay(position, -axis * axisLength);

            // Draw reference direction
            Gizmos.color = selected ? Color.cyan : new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawRay(position, reference * 0.5f);

            // Draw current rotation indicator
            if (Application.isPlaying)
            {
                var currentRotationQuat = Quaternion.AngleAxis(currentAngle, axis);
                var currentDir = currentRotationQuat * reference;
                Gizmos.color = selected ? Color.green : new Color(0f, 1f, 0f, 0.7f);
                Gizmos.DrawRay(position, currentDir * 0.7f);
            }
        }

        private void DrawRotationLimits()
        {
            var position = interactableObject.transform.position;
            var axis = GetRotationAxisVector();
            var reference = GetWorldReferenceVector();
            float radius = 0.6f;

            // Draw min rotation limit
            Gizmos.color = Color.red;
            var minRot = Quaternion.AngleAxis(rotationLimits.x * 360f, axis);
            var minDir = minRot * reference;
            Gizmos.DrawRay(position, minDir * radius);

            // Draw max rotation limit
            Gizmos.color = Color.blue;
            var maxRot = Quaternion.AngleAxis(rotationLimits.y * 360f, axis);
            var maxDir = maxRot * reference;
            Gizmos.DrawRay(position, maxDir * radius);
        }
    }
}
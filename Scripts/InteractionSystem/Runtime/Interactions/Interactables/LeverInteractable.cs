using System;
using UnityEngine;
using Shababeek.Utilities;
using UniRx;

namespace Shababeek.Interactions
{
    public enum RotationAxis
    {
        Right,
        Up,
        Forward
    }
    /// <summary>
    /// Lever-style interactable that rotates around a single axis with configurable limits.
    /// Provides smooth rotation control and normalized output values.
    /// </summary>
    public class LeverInteractable : ConstrainedInteractableBase
    {
        [SerializeField] public RotationAxis rotationAxis = RotationAxis.Right;
        [Tooltip("Rotation angle range in degrees (min, max)")]
        [SerializeField] private Vector2 angleRange = new Vector2(-40f, 40f);
        
        [Tooltip("Reference distance for angle calculation (affects sensitivity) should be around the same height as the grab point")]
        [SerializeField] private float projectionDistance = 0.3f;
        [SerializeField] private FloatUnityEvent onLeverChanged = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private float currentAngle = 0f;
        [ReadOnly, SerializeField] private float currentNormalizedAngle = 0f;

        private Quaternion _originalRotation;
        private float _returnTimer;

        /// <summary>
        /// Observable that fires when the lever's normalized position changes.
        /// </summary>
        public IObservable<float> OnLeverChanged => onLeverChanged.AsObservable();
        public float CurrentAngle => currentAngle;
        public float CurrentNormalizedAngle => currentNormalizedAngle;
        public Vector2 AngleRange
        {
            get => angleRange;
            set =>
                angleRange = new Vector2(
                    Mathf.Clamp(value.x, -180f, value.y - 1f),
                    Mathf.Clamp(value.y, value.x + 1f, 180f)
                );
        }

        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            UpdateCurrentAngleFromTransform();
        }

        protected override void HandleObjectMovement(Vector3 target)
        {
            if (!IsSelected || IsReturning) return;

            CalculateAndApplyRotation(target);
            UpdateDebugValues();
            InvokeEvents();
        }
        protected override void HandleObjectDeselection()
        {
            _returnTimer = 0f; //TODO: Snap to nearest step if stepped movement is implemented
        }
        private void CalculateAndApplyRotation(Vector3 handWorldPosition)
        {
            Transform pivot = interactableObject.transform;
            
            Vector3 direction = handWorldPosition - pivot.position;
            direction = transform.InverseTransformDirection(direction);

            var (axisNormal, tangent) = GetAxisVectors();

            Vector3 projected = Vector3.ProjectOnPlane(direction, axisNormal);

            var v = Vector3.Dot(projected, tangent);
            var targetAngle = Mathf.Atan2(v, projectionDistance) * Mathf.Rad2Deg;

            targetAngle = Mathf.Clamp(targetAngle, angleRange.x, angleRange.y);

            currentAngle = targetAngle;
            ApplyRotationToTransform();
        }

        private (Vector3 axis, Vector3 tangentV) GetAxisVectors()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => (Vector3.right, Vector3.forward),
                RotationAxis.Up => (Vector3.up, Vector3.right),
                RotationAxis.Forward => (Vector3.forward, Vector3.left),
                _ => (Vector3.forward, Vector3.right)
            };
        }

        private Vector3 GetLocalAxis()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => Vector3.right,
                RotationAxis.Up => Vector3.up,
                RotationAxis.Forward => Vector3.forward,
                _ => Vector3.right
            };
        }

        private void ApplyRotationToTransform()
        {
            var localAxis = GetLocalAxis();
            var relative = Quaternion.AngleAxis(currentAngle, localAxis);
            interactableObject.transform.localRotation = _originalRotation * relative;
        }

        private void UpdateCurrentAngleFromTransform()
        {
            // Extract current angle from transform
            Vector3 eulerAngles = interactableObject.transform.localRotation.eulerAngles;
            
            float angle = rotationAxis switch
            {
                RotationAxis.Right => eulerAngles.x,
                RotationAxis.Up => eulerAngles.y,
                RotationAxis.Forward => eulerAngles.z,
                _ => eulerAngles.x
            };

            currentAngle = NormalizeAngle(angle);
        }

        private void UpdateDebugValues()
        {
            currentNormalizedAngle = Mathf.InverseLerp(angleRange.x, angleRange.y, currentAngle);
        }

        protected override void HandleReturnToOriginalPosition()
        {
            _returnTimer += Time.deltaTime * returnSpeed;
            var localRotation = interactableObject.transform.localRotation;
            localRotation = Quaternion.Lerp(localRotation, _originalRotation, _returnTimer);
            interactableObject.transform.localRotation = localRotation;

            UpdateCurrentAngleFromTransform();
            UpdateDebugValues();
            InvokeEvents();

            if (Quaternion.Angle(interactableObject.transform.localRotation, _originalRotation) < 1f)
            {
                IsReturning = false;
                interactableObject.transform.localRotation = _originalRotation;
                UpdateCurrentAngleFromTransform();
            }
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        private void InvokeEvents()
        {
            onLeverChanged?.Invoke(currentNormalizedAngle);
        }

        /// <summary>
        /// Sets the lever rotation to a specific angle in degrees.
        /// </summary>
        public void SetAngle(float angle)
        {
            currentAngle = Mathf.Clamp(angle, angleRange.x, angleRange.y);
            ApplyRotationToTransform();
            UpdateDebugValues();
            InvokeEvents();
        }

        /// <summary>
        /// Sets the lever rotation using a normalized value (0-1).
        /// </summary>
        public void SetNormalizedAngle(float normalizedAngle)
        {
            float angle = Mathf.Lerp(angleRange.x, angleRange.y, normalizedAngle);
            SetAngle(angle);
        }

        /// <summary>
        /// Resets the lever to its original rotation.
        /// </summary>
        public void ResetToOriginal()
        {
            interactableObject.transform.localRotation = _originalRotation;
            UpdateCurrentAngleFromTransform();
            IsReturning = false;
            UpdateDebugValues();
            InvokeEvents();
        }

        /// <summary>
        /// Gets the rotation axis and normal vector for the lever (used by editor).
        /// </summary>
        public (Vector3 plane, Vector3 normal) GetRotationAxis()
        {
            var t = interactableObject.transform;
            return rotationAxis switch
            {
                RotationAxis.Right => (t.right, t.up),
                RotationAxis.Up => (t.up, t.forward),
                RotationAxis.Forward => (t.forward, t.up),
                _ => (t.right, t.up)
            };
        }

        private void OnValidate()
        {
            // Ensure min < max
            if (angleRange.x >= angleRange.y)
            {
                angleRange.y = angleRange.x + 1f;
            }

            angleRange.x = Mathf.Clamp(angleRange.x, -180f, angleRange.y - 1f);
            angleRange.y = Mathf.Clamp(angleRange.y, angleRange.x + 1f, 180f);

            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        private void OnDrawGizmos()
        {
            if (interactableObject == null) return;
            DrawLeverVisualization();
        }

        private void OnDrawGizmosSelected()
        {
            if (interactableObject == null) return;
            DrawLeverVisualization(true);
            DrawRotationLimits();
        }

        private void DrawLeverVisualization(bool selected = false)
        {
            var position = interactableObject.transform.position;

            Gizmos.color = selected ? Color.yellow : new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(position, 0.02f);

            if (Application.isPlaying)
            {
                var axis = GetRotationAxis().normal;
                Gizmos.color = selected ? Color.green : new Color(0f, 1f, 0f, 0.7f);
                Gizmos.DrawRay(position, axis * 0.3f);
            }
        }

        private void DrawRotationLimits()
        {
            var position = transform.position;
            float radius = 0.5f;

            var (axis, normal) = GetRotationAxis();

            Gizmos.color = Color.cyan;
            var minRot = Quaternion.AngleAxis(angleRange.x, axis);
            var maxRot = Quaternion.AngleAxis(angleRange.y, axis);
            var minDir = minRot * normal;
            var maxDir = maxRot * normal;
            Gizmos.DrawRay(position, minDir * radius);
            Gizmos.DrawRay(position, maxDir * radius);
        }
    }
}
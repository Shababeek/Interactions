using UnityEngine;
using Shababeek.ReactiveVars;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Base class for lever-style interactables that rotate around a single axis between angle limits.
    /// Handles projecting hand input to an angle, applying rotation, and the snap/return pipeline,
    /// while subclasses decide how the raw angle is processed (free-running lever vs stepped toggle).
    /// </summary>
    public abstract class RotaryLeverBase : ConstrainedInteractableBase
    {
        [Tooltip("The axis around which the lever rotates.")]
        [SerializeField] protected RotationAxis rotationAxis = RotationAxis.Forward;

        [Tooltip("Rotation angle range in degrees (x = minimum, y = maximum).")]
        [SerializeField] protected Vector2 angleRange = new Vector2(-40f, 40f);

        [Tooltip("Reference distance for angle calculation (affects sensitivity). Should be around the same height as the grab point.")]
        [SerializeField] protected float projectionDistance = 0.3f;

        [Header("Debug")]
        [ReadOnly, SerializeField] protected float currentAngle = 0f;
        [ReadOnly, SerializeField] protected float currentNormalizedAngle = 0f;

        protected Quaternion _originalRotation;
        protected Vector3 _originalPosition;
        protected float _returnTargetAngle = 0f;

        /// <summary>
        /// The point the body rotates about, in the interactable object's parent-local space.
        /// Defaults to the object's own rest origin; override to hinge about an offset point such
        /// as a door's edge. When this equals the rest origin the motion is a pure spin in place.
        /// </summary>
        protected virtual Vector3 PivotLocalPosition => _originalPosition;

        /// <summary>Current rotation angle in degrees, relative to the rest pose.</summary>
        public float CurrentAngle => currentAngle;

        /// <summary>Current angle remapped to [0, 1] across the angle range.</summary>
        public float CurrentNormalizedAngle => currentNormalizedAngle;

        /// <summary>Rotation limits in degrees (x = minimum, y = maximum).</summary>
        public Vector2 AngleRange
        {
            get => angleRange;
            set => angleRange = new Vector2(
                Mathf.Clamp(value.x, -180f, value.y - 1f),
                Mathf.Clamp(value.y, value.x + 1f, 180f));
        }

        protected virtual void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            _originalPosition = interactableObject.transform.localPosition;
            UpdateCurrentAngleFromTransform();
            UpdateDebugValues();
        }

        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!IsSelected || IsReturning) return;

            float requested = CalculateAngle(handWorldPosition);
            currentAngle = ProcessAngle(requested);
            ApplyRotationToTransform();
            UpdateDebugValues();
            OnAngleChanged();
        }

        protected override void HandleObjectDeselection()
        {
            _returnTargetAngle = 0f;
            OnDeselected();
        }

        protected override void HandleReturnToOriginalPosition()
        {
            currentAngle = Mathf.Lerp(currentAngle, _returnTargetAngle, Time.deltaTime * returnSpeed);
            ApplyRotationToTransform();
            UpdateDebugValues();
            OnAngleChanged();

            if (Mathf.Abs(currentAngle - _returnTargetAngle) < 0.1f)
            {
                currentAngle = _returnTargetAngle;
                ApplyRotationToTransform();
                UpdateDebugValues();
                IsReturning = false;
                OnReturnComplete();
            }
        }

        /// <summary>
        /// Processes the raw angle requested by the hand before it is applied.
        /// Default clamps to the angle range; subclasses can add snapping or stepping.
        /// </summary>
        protected virtual float ProcessAngle(float requestedAngle)
        {
            return Mathf.Clamp(requestedAngle, angleRange.x, angleRange.y);
        }

        /// <summary>Called after the angle is applied. Subclasses raise events or fire haptics here.</summary>
        protected virtual void OnAngleChanged() { }

        /// <summary>Called when the lever is released, after the return target is reset to rest.</summary>
        protected virtual void OnDeselected() { }

        /// <summary>Called once the lever finishes returning to its target angle.</summary>
        protected virtual void OnReturnComplete() { }

        protected float CalculateAngle(Vector3 handWorldPosition)
        {
            Transform t = interactableObject.transform;
            Vector3 pivotWorld = t.parent != null
                ? t.parent.TransformPoint(PivotLocalPosition)
                : PivotLocalPosition;

            Vector3 direction = handWorldPosition - pivotWorld;
            direction = transform.InverseTransformDirection(direction);

            var (axisNormal, tangent) = GetAxisVectors();
            Vector3 projected = Vector3.ProjectOnPlane(direction, axisNormal);
            float v = Vector3.Dot(projected, tangent);
            float angle = Mathf.Atan2(v, projectionDistance) * Mathf.Rad2Deg;

            return Mathf.Clamp(angle, angleRange.x, angleRange.y);
        }

        /// <summary>
        /// The hand's true angular position (degrees) around the pivot within the rotation plane,
        /// measured in the interactable's fixed local frame. Unlike <see cref="CalculateAngle"/>
        /// this is an absolute bearing meant for relative, delta-based dragging: track it between
        /// frames and apply the difference, so grabbing far from the pivot does not snap the body.
        /// </summary>
        protected float HandAngleAroundPivot(Vector3 handWorldPosition)
        {
            Transform t = interactableObject.transform;
            Vector3 pivotWorld = t.parent != null
                ? t.parent.TransformPoint(PivotLocalPosition)
                : PivotLocalPosition;

            Vector3 dir = transform.InverseTransformDirection(handWorldPosition - pivotWorld);
            var (axisNormal, tangent) = GetAxisVectors();
            Vector3 binormal = Vector3.Cross(axisNormal, tangent);
            Vector3 planar = Vector3.ProjectOnPlane(dir, axisNormal);
            return Mathf.Atan2(Vector3.Dot(planar, binormal), Vector3.Dot(planar, tangent)) * Mathf.Rad2Deg;
        }

        protected void ApplyRotationToTransform()
        {
            Vector3 localAxis = GetLocalAxis();
            Quaternion newRotation = _originalRotation * Quaternion.AngleAxis(currentAngle, localAxis);

            Vector3 pivot = PivotLocalPosition;
            // The same rotation expressed in parent space, so the body can swing about an
            // off-origin hinge. When pivot == rest origin, position is unchanged and this reduces
            // to the previous spin-in-place behaviour.
            Quaternion parentSpin = Quaternion.AngleAxis(currentAngle, _originalRotation * localAxis);
            Vector3 newPosition = pivot + parentSpin * (_originalPosition - pivot);

            var t = interactableObject.transform;
            t.localRotation = newRotation;
            t.localPosition = newPosition;
        }

        protected void UpdateDebugValues()
        {
            currentNormalizedAngle = Mathf.InverseLerp(angleRange.x, angleRange.y, currentAngle);
        }

        protected void UpdateCurrentAngleFromTransform()
        {
            Vector3 euler = interactableObject.transform.localRotation.eulerAngles;
            float angle = rotationAxis switch
            {
                RotationAxis.Right => euler.x,
                RotationAxis.Up => euler.y,
                RotationAxis.Forward => euler.z,
                _ => euler.x
            };
            currentAngle = NormalizeAngle(angle);
        }

        protected float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        private (Vector3 axis, Vector3 tangent) GetAxisVectors()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => (Vector3.right, Vector3.forward),
                RotationAxis.Up => (Vector3.up, Vector3.right),
                RotationAxis.Forward => (Vector3.forward, Vector3.left),
                _ => (Vector3.forward, Vector3.right)
            };
        }

        protected Vector3 GetLocalAxis()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => Vector3.right,
                RotationAxis.Up => Vector3.up,
                RotationAxis.Forward => Vector3.forward,
                _ => Vector3.right
            };
        }

        /// <summary>Rotation plane axis and rest-direction vector in world space (used by editors and gizmos).</summary>
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

        /// <summary>Sets the rotation to a specific angle in degrees, raising change events.</summary>
        public void SetAngle(float angle)
        {
            currentAngle = Mathf.Clamp(angle, angleRange.x, angleRange.y);
            ApplyRotationToTransform();
            UpdateDebugValues();
            OnAngleChanged();
        }

        /// <summary>Sets the rotation using a normalized value (0-1) across the angle range.</summary>
        public void SetNormalizedAngle(float normalizedAngle)
        {
            SetAngle(Mathf.Lerp(angleRange.x, angleRange.y, normalizedAngle));
        }

        /// <summary>Resets the lever to its rest pose.</summary>
        public void ResetToOriginal()
        {
            currentAngle = 0f;
            ApplyRotationToTransform();
            UpdateDebugValues();
            IsReturning = false;
            OnAngleChanged();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (angleRange.x >= angleRange.y) angleRange.y = angleRange.x + 1f;
            angleRange.x = Mathf.Clamp(angleRange.x, -180f, angleRange.y - 1f);
            angleRange.y = Mathf.Clamp(angleRange.y, angleRange.x + 1f, 180f);
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        protected virtual void OnDrawGizmos()
        {
            if (interactableObject == null) return;

            var position = interactableObject.transform.position;
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawWireSphere(position, 0.02f);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (interactableObject == null) return;

            var position = transform.position;
            float radius = 0.5f;
            var (axis, normal) = GetRotationAxis();

            Gizmos.color = Color.cyan;
            var minDir = Quaternion.AngleAxis(angleRange.x, axis) * normal;
            var maxDir = Quaternion.AngleAxis(angleRange.y, axis) * normal;
            Gizmos.DrawRay(position, minDir * radius);
            Gizmos.DrawRay(position, maxDir * radius);
        }
    }
}

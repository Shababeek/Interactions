using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.ReactiveVars;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Base class for rotary interactables (wheels, dials) that rotate around a single local axis.
    /// Handles input gathering for both control schemes (hand position around pivot, or wrist twist),
    /// fake-hand orbit positioning, and dispatches angle changes to subclass-specific clamp/wrap/step logic.
    /// </summary>
    public abstract class RotaryInteractableBase : ConstrainedInteractableBase
    {
        [Tooltip("How rotation input is gathered: hand position around the pivot, or wrist twist around the axis.")]
        [SerializeField] protected RotaryControlScheme controlScheme = RotaryControlScheme.HandPosition;

        [Tooltip("Hand-positioning behavior at grab. Only used when control scheme is Hand Position.")]
        [SerializeField] protected WheelGrabMode grabMode = WheelGrabMode.ObjectFollowsHand;

        [Tooltip("The local axis around which the object rotates.")]
        [SerializeField] protected RotationAxis rotationAxis = RotationAxis.Forward;

        [Header("Debug")]
        [ReadOnly, SerializeField] protected float currentAngle;

        protected Quaternion _originalRotation;

        // HandPosition state
        private float _previousHandAngle;

        // HandRotation state
        private Quaternion _grabInteractorRotation;
        private Vector3 _grabReferenceLocal;
        private float _previousTwist;

        // HandFollowsObject state
        protected Transform _fakeHand;
        protected float _fakeHandOrbitAngle;
        // Constant grab-time rotation offset between hand and constraint, around the rotation axis.
        // The wheel's parented rotation already carries currentAngle visually; this offset only
        // accounts for the static gap between where the user grabbed and where the constraint sits.
        private float _grabRotationOffset;

        /// <summary>Current rotation angle in degrees, relative to the original rotation.</summary>
        public float CurrentAngle => currentAngle;

        /// <summary>How rotation input is gathered (hand position vs wrist twist).</summary>
        public RotaryControlScheme ControlScheme => controlScheme;

        /// <summary>Hand-positioning behavior at grab. Only meaningful for the HandPosition control scheme.</summary>
        public WheelGrabMode GrabMode => grabMode;

        /// <summary>The local axis around which the object rotates.</summary>
        public RotationAxis RotationAxis => rotationAxis;

        /// <summary>Returns the rotation axis in world space, taken from the interactable object's transform.</summary>
        public Vector3 GetWorldAxis()
        {
            var t = interactableObject != null ? interactableObject.transform : transform;
            return rotationAxis switch
            {
                RotationAxis.Right => t.right,
                RotationAxis.Up => t.up,
                _ => t.forward
            };
        }

        protected virtual void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            PoseConstrainer = GetComponent<PoseConstrainter>();
        }

        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!IsSelected || IsReturning) return;

            float delta = controlScheme == RotaryControlScheme.HandPosition
                ? GatherDeltaFromHandPosition(handWorldPosition)
                : GatherDeltaFromHandRotation();

            currentAngle = ProcessAngleDelta(currentAngle, delta);

            if (controlScheme == RotaryControlScheme.HandPosition
                && grabMode == WheelGrabMode.HandFollowsObject
                && _fakeHand != null)
            {
                _fakeHandOrbitAngle += delta;
                UpdateFakeHandOrbit();
            }

            ApplyRotation();
            OnAngleApplied(currentAngle);
        }

        protected override void PositionFakeHand(Transform fakeHand, HandIdentifier handIdentifier)
        {
            _fakeHand = fakeHand;

            if (controlScheme == RotaryControlScheme.HandPosition)
            {
                _previousHandAngle = GetHandAngle(CurrentInteractor.transform.position);
                OnGrabAlignment(handIdentifier);

                if (grabMode == WheelGrabMode.ObjectFollowsHand)
                {
                    base.PositionFakeHand(fakeHand, handIdentifier);
                }
                else
                {
                    _fakeHandOrbitAngle = _previousHandAngle;
                    _grabRotationOffset = _previousHandAngle - GetConstraintAngle(handIdentifier);
                    UpdateFakeHandOrbit();
                }
            }
            else
            {
                CacheGrabRotationReference();
                _previousTwist = 0f;
                base.PositionFakeHand(fakeHand, handIdentifier);
            }
        }

        protected override void HandleObjectDeselection()
        {
            _fakeHand = null;
        }

        /// <summary>
        /// Subclass implements clamping, wrapping, or step-snapping logic.
        /// Returns the angle that should be applied this frame.
        /// </summary>
        protected abstract float ProcessAngleDelta(float currentAngle, float delta);

        /// <summary>
        /// Hook fired after a new angle is applied. Subclasses use this for events, step detection, or haptics.
        /// </summary>
        protected virtual void OnAngleApplied(float newAngle) { }

        /// <summary>
        /// Hook called at grab time before the fake hand is positioned (HandPosition scheme only).
        /// Subclasses can override to align rotation state to the grab point.
        /// </summary>
        protected virtual void OnGrabAlignment(HandIdentifier handIdentifier) { }

        protected void ApplyRotation()
        {
            Vector3 axis = GetLocalAxis();
            Quaternion rot = Quaternion.AngleAxis(currentAngle, axis);
            interactableObject.transform.localRotation = _originalRotation * rot;
        }

        protected float GetHandAngle(Vector3 handWorldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(handWorldPosition);
            float x, y;
            switch (rotationAxis)
            {
                case RotationAxis.Right:   x = localPos.z; y = localPos.y; break;
                case RotationAxis.Up:      x = localPos.x; y = localPos.z; break;
                case RotationAxis.Forward:
                default:                   x = localPos.x; y = localPos.y; break;
            }
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        protected Vector3 GetLocalAxis()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => -Vector3.right,
                RotationAxis.Up => -Vector3.up,
                _ => Vector3.forward
            };
        }

        protected void UpdateFakeHandOrbit()
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

            // The interactable object's rotation (currentAngle around axis) already rotates the
            // fake hand visually via parenting. Only the constant grab-time offset needs to be
            // applied as local rotation — applying _fakeHandOrbitAngle here double-counts.
            Quaternion orbitRot = Quaternion.AngleAxis(_grabRotationOffset, GetLocalAxis());
            _fakeHand.localRotation = orbitRot * basePose.rotation;
        }

        protected float GetConstraintAngle(HandIdentifier handIdentifier)
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

        private float GatherDeltaFromHandPosition(Vector3 handWorldPosition)
        {
            float handAngle = GetHandAngle(handWorldPosition);
            float delta = Mathf.DeltaAngle(_previousHandAngle, handAngle);
            _previousHandAngle = handAngle;
            return delta;
        }

        private float GatherDeltaFromHandRotation()
        {
            float twist = GetTwistSinceGrab();
            float delta = Mathf.DeltaAngle(_previousTwist, twist);
            _previousTwist = twist;
            return delta;
        }

        private void CacheGrabRotationReference()
        {
            _grabInteractorRotation = CurrentInteractor.transform.rotation;

            Vector3 worldAxis = transform.TransformDirection(GetLocalAxis());
            Vector3 worldRef = Vector3.Cross(worldAxis, Vector3.up);
            if (worldRef.sqrMagnitude < 0.0001f) worldRef = Vector3.Cross(worldAxis, Vector3.forward);
            worldRef.Normalize();

            _grabReferenceLocal = Quaternion.Inverse(_grabInteractorRotation) * worldRef;
        }

        private float GetTwistSinceGrab()
        {
            Vector3 worldAxis = transform.TransformDirection(GetLocalAxis());
            Vector3 grabRefWorld = _grabInteractorRotation * _grabReferenceLocal;
            Vector3 currentRefWorld = CurrentInteractor.transform.rotation * _grabReferenceLocal;

            Vector3 grabProjected = Vector3.ProjectOnPlane(grabRefWorld, worldAxis);
            Vector3 currentProjected = Vector3.ProjectOnPlane(currentRefWorld, worldAxis);

            return Vector3.SignedAngle(grabProjected, currentProjected, worldAxis);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            // HandFollowsObject orbit is only meaningful when the hand actually moves around the pivot.
            if (controlScheme == RotaryControlScheme.HandRotation)
                grabMode = WheelGrabMode.ObjectFollowsHand;
        }
    }
}

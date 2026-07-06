using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Constrained grabable that supports one or two hands. Both hands are represented by fake
    /// hands welded to the object's grips (real hands hidden), so grips always look perfect.
    /// One hand: the object follows the hidden primary hand. Two hands: the object's grip axis
    /// aims from the primary to the secondary hand (rifle-style look-rotation). When the primary
    /// releases, the secondary hand is promoted and keeps the object.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Two Handed Grabable")]
    public class TwoHandedGrabable : ConstrainedInteractableBase
    {
        [Header("Two-Handed")]
        [Tooltip("When enabled, the object stays anchored in place and inert (use/activation disabled) " +
                 "when held by only one hand. It only lifts and follows the hands once a second hand " +
                 "grabs it. Releasing one hand keeps the object with the remaining hand, freezing it in " +
                 "place and reverting it to inert.")]
        [SerializeField] private bool requireTwoHands = false;

        [Tooltip("Smoothing speed of the follow/solve motion. Higher snaps faster.")]
        [SerializeField] private float followSmoothing = 25f;

        [Tooltip("Whether this object tracks velocity while held and applies a throw on release. Requires a Rigidbody.")]
        [SerializeField] private bool canBeThrown = true;

        [Tooltip("Throw tracking settings. Only used when Can Be Thrown is enabled and a Rigidbody is present.")]
        [SerializeField] private Throwable throwable = new();

        [Tooltip("Fired when a second hand grabs this object.")]
        [SerializeField] private InteractorUnityEvent onSecondaryGrabStart = new();

        [Tooltip("Fired when the second hand releases or is promoted to primary.")]
        [SerializeField] private InteractorUnityEvent onSecondaryGrabEnd = new();

        private Rigidbody _body;
        private bool _wasKinematic;

        /// <summary>True while both hands are holding this object.</summary>
        public bool IsTwoHanded => IsSelected && SecondaryInteractor != null;

        /// <summary>
        /// True when the object's two-handed function is active: either two hands are holding it,
        /// or two hands aren't required. When false the object is held but inert.
        /// </summary>
        public bool IsFunctional => !requireTwoHands || IsTwoHanded;

        /// <inheritdoc/>
        protected override bool SupportsSecondaryGrab => true;

        /// <inheritdoc/>
        protected override bool CanBeUsed => IsFunctional;

        protected override void InitializeInteractable()
        {
            base.InitializeInteractable();
            _body = GetComponent<Rigidbody>();
        }

        /// <inheritdoc/>
        protected override bool Select()
        {
            // Freeze physics for the duration of the grab; prior state restored on release.
            if (_body != null)
            {
                _wasKinematic = _body.isKinematic;
                _body.isKinematic = true;
            }

            var abort = base.Select(); // fake hand + constraints + hidden real hand

            if (canBeThrown && _body != null)
            {
                throwable.StartTracking(_body, transform);
            }

            return abort;
        }

        /// <inheritdoc/>
        protected override void DeSelected()
        {
            // Read before base.DeSelected() — it consumes SecondaryInteractor for promotion.
            bool handingOver = SecondaryInteractor != null;

            base.DeSelected();

            if (_body != null) _body.isKinematic = _wasKinematic;

            // Order matters: ApplyThrow is a no-op on kinematic bodies, so kinematic state is
            // restored first. No throw during a hand-over — the other hand keeps the object.
            if (canBeThrown && _body != null && !handingOver)
            {
                throwable.ApplyThrow();
            }
        }

        /// <inheritdoc/>
        protected override void OnSecondarySelected(InteractorBase interactor)
        {
            onSecondaryGrabStart.Invoke(interactor);
        }

        /// <inheritdoc/>
        protected override void OnSecondaryDeselected(InteractorBase interactor)
        {
            onSecondaryGrabEnd.Invoke(interactor);
        }

        /// <inheritdoc/>
        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!IsSelected || CurrentInteractor == null) return;

            // Losing the second hand makes a two-hands-required object inert; stop any active use.
            if (IsUsing && !IsFunctional)
            {
                StopUsing(CurrentInteractor);
            }

            // Two-hands-required and only one hand holding: keep the object anchored in place — it
            // doesn't lift or follow the single hand until the second hand grabs it.
            if (!IsFunctional) return;

            Vector3 targetPosition;
            Quaternion targetRotation;

            if (SecondaryInteractor != null)
            {
                (targetPosition, targetRotation) = TwoHandSolver.Solve(
                    CurrentInteractor.transform.position,
                    SecondaryInteractor.transform.position,
                    CurrentInteractor.transform.up,
                    GetGripAnchorLocal(CurrentInteractor.HandIdentifier),
                    GetGripAnchorLocal(SecondaryInteractor.HandIdentifier),
                    ConstraintTransform.lossyScale);
            }
            else
            {
                (targetPosition, targetRotation) = SolveOneHanded();
            }

            // Exponential decay smoothing — frame-rate independent.
            float t = 1f - Mathf.Exp(-followSmoothing * Time.deltaTime);
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, targetPosition, t),
                Quaternion.Slerp(transform.rotation, targetRotation, t));
        }

        private (Vector3 position, Quaternion rotation) SolveOneHanded()
        {
            // Place the object so its authored grip pose coincides with the (hidden) real hand:
            // hand pose in constraint space is (anchor, anchorRotation), so
            // root = hand * inverse(localGripPose). Assumes ConstraintTransform is rotationally
            // aligned with the root (the scale compensator is created with identity rotation).
            var handIdentifier = CurrentInteractor.HandIdentifier;
            var (anchorLocal, anchorLocalRotation) = GetGripPoseLocal(handIdentifier);

            Quaternion handRotation = CurrentInteractor.transform.rotation;
            Quaternion rotation = handRotation * Quaternion.Inverse(anchorLocalRotation);
            Vector3 scaledAnchor = Vector3.Scale(anchorLocal, ConstraintTransform.lossyScale);
            Vector3 position = CurrentInteractor.transform.position - rotation * scaledAnchor;

            return (position, rotation);
        }

        private Vector3 GetGripAnchorLocal(HandIdentifier handIdentifier)
        {
            if (PoseConstrainer.ConstraintType == HandConstrainType.MultiPoint)
            {
                int index = PoseConstrainer.GetActiveGrabPointIndex(handIdentifier);
                if (index >= 0 && index < PoseConstrainer.GrabPoints.Count)
                {
                    return PoseConstrainer.GrabPoints[index].localPosition;
                }
            }

            var (position, _) = PoseConstrainer.GetTargetHandTransform(handIdentifier);
            return position;
        }

        private (Vector3 position, Quaternion rotation) GetGripPoseLocal(HandIdentifier handIdentifier)
        {
            var (position, rotation) = PoseConstrainer.GetTargetHandTransform(handIdentifier);

            if (PoseConstrainer.ConstraintType == HandConstrainType.MultiPoint)
            {
                int index = PoseConstrainer.GetActiveGrabPointIndex(handIdentifier);
                if (index >= 0 && index < PoseConstrainer.GrabPoints.Count)
                {
                    var point = PoseConstrainer.GrabPoints[index];
                    // Grab point pose combined with the per-hand offset authored on it.
                    position = point.localPosition + Quaternion.Euler(point.localRotation) * position;
                    rotation = Quaternion.Euler(point.localRotation) * rotation;
                }
            }

            return (position, rotation);
        }

        /// <inheritdoc/>
        protected override void HandleObjectDeselection() { }

        /// <inheritdoc/>
        protected override void HandleReturnToOriginalPosition()
        {
            // Free objects don't return to a rest pose.
            IsReturning = false;
        }

        private void FixedUpdate()
        {
            if (IsSelected && canBeThrown && _body != null)
            {
                throwable.Sample();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !IsTwoHanded) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(CurrentInteractor.transform.position, SecondaryInteractor.transform.position);
        }
    }
}

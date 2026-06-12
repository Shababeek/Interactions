using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Grabable that accepts a second hand while held. The primary hand owns position; the
    /// object's grip axis aims from the primary to the secondary hand (rifle-style look-rotation).
    /// When the primary releases, the secondary hand is promoted to primary and keeps the object.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Two Handed Grabable")]
    public class TwoHandedGrabable : Grabable
    {
        [Header("Two-Handed")]
        [Tooltip("Smoothing speed of the two-handed pose solve. Higher snaps faster.")]
        [SerializeField] private float solveSmoothing = 25f;

        [Tooltip("Fired when a second hand grabs this object.")]
        [SerializeField] private InteractorUnityEvent onSecondaryGrabStart = new();

        [Tooltip("Fired when the second hand releases or is promoted to primary.")]
        [SerializeField] private InteractorUnityEvent onSecondaryGrabEnd = new();

        private InteractorBase _secondary;

        /// <summary>The interactor holding the secondary grip (null when one-handed).</summary>
        public InteractorBase SecondaryInteractor => _secondary;

        /// <summary>True while both hands are holding this object.</summary>
        public bool IsTwoHanded => IsSelected && _secondary != null;

        /// <inheritdoc/>
        public override bool CanAcceptSecondaryInteractor(InteractorBase interactor)
        {
            return IsSelected
                   && _secondary == null
                   && interactor != null
                   && interactor != CurrentInteractor
                   && CanInteract(interactor.Hand);
        }

        /// <inheritdoc/>
        public override bool TrySecondarySelect(InteractorBase interactor)
        {
            if (!CanAcceptSecondaryInteractor(interactor)) return false;

            _secondary = interactor;
            Constrainter.ApplyConstraints(interactor.Hand, interactor.GetInteractionPoint());

            // Two hands can't both parent the object — switch to the per-frame solve.
            CancelGrabTween();
            transform.SetParent(null, true);

            onSecondaryGrabStart.Invoke(interactor);
            return true;
        }

        /// <inheritdoc/>
        public override void SecondaryDeselect(InteractorBase interactor)
        {
            if (_secondary == null || _secondary != interactor) return;

            Constrainter.RemoveConstraints(interactor.Hand);
            var released = _secondary;
            _secondary = null;

            if (IsSelected)
            {
                // Back to the ordinary one-hand attachment on the primary.
                InitializeAttachmentPointTransform();
                AttachToHand();
            }

            onSecondaryGrabEnd.Invoke(released);
        }

        /// <inheritdoc/>
        protected override bool SuppressThrow => _secondary != null;

        /// <inheritdoc/>
        protected override void DeSelected()
        {
            base.DeSelected();

            if (_secondary == null) return;

            // Primary released while the second hand still holds: promote it. The promotion is
            // a normal Select() and must run after the state machine settles to None, so defer
            // one frame.
            var toPromote = _secondary;
            _secondary = null;
            onSecondaryGrabEnd.Invoke(toPromote);
            PromoteNextFrame(toPromote);
        }

        private async void PromoteNextFrame(InteractorBase interactor)
        {
            await Awaitable.NextFrameAsync();
            if (this == null || interactor == null) return;
            if (IsSelected) return; // something else claimed the object meanwhile
            interactor.PromoteSecondaryToPrimary();
        }

        private void LateUpdate()
        {
            if (!IsTwoHanded) return;
            if (CurrentInteractor == null || _secondary == null) return;

            Vector3 primaryHand = CurrentInteractor.Hand.transform.position;
            Vector3 secondaryHand = _secondary.Hand.transform.position;
            Vector3 upHint = CurrentInteractor.Hand.transform.up;

            var (targetPosition, targetRotation) = TwoHandSolver.Solve(
                primaryHand, secondaryHand, upHint,
                GetGripAnchorLocal(CurrentInteractor),
                GetGripAnchorLocal(_secondary),
                Constrainter.ConstraintTransform.lossyScale);

            // Exponential decay smoothing — frame-rate independent.
            float t = 1f - Mathf.Exp(-solveSmoothing * Time.deltaTime);
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, targetPosition, t),
                Quaternion.Slerp(transform.rotation, targetRotation, t));
        }

        private Vector3 GetGripAnchorLocal(InteractorBase interactor)
        {
            var handIdentifier = interactor.Hand.HandIdentifier;

            if (Constrainter.ConstraintType == HandConstrainType.MultiPoint)
            {
                int index = Constrainter.GetActiveGrabPointIndex(handIdentifier);
                if (index >= 0 && index < Constrainter.GrabPoints.Count)
                {
                    return Constrainter.GrabPoints[index].localPosition;
                }
            }

            var (position, _) = Constrainter.GetTargetHandTransform(handIdentifier);
            return position;
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !IsTwoHanded) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(CurrentInteractor.Hand.transform.position, _secondary.Hand.transform.position);
        }
    }
}

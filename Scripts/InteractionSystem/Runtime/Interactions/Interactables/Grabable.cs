using System;
using Shababeek.ReactiveVars;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Component that enables objects to be grabbed and held by VR hands.
    /// Manages the grabbing process, hand positioning, and smooth transitions
    /// between grab states using pose constraints and tweening.
    /// </summary>
    /// <remarks>
    /// This component requires a UnifiedPoseConstraintSystem for proper hand positioning.
    /// It automatically handles the grab/ungrab process and manages the attachment
    /// of objects to hand attachment points with smooth animations.
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Grabable")]
    [RequireComponent(typeof(PoseConstrainer))]
    public class Grabable : InteractableBase
    {
        [Tooltip("The tweener component used for smooth grab animations. Auto-added if not present., only add if you want to the same tweener across multiple tweenables")]
        [SerializeField] private VariableTweener tweener;

        [Tooltip("Whether this grabable should track velocity while held and apply a throw on release. Requires a Rigidbody.")]
        [SerializeField] private bool canBeThrown = true;

        [Tooltip("Throw tracking settings. Only used when Can Be Thrown is enabled and a Rigidbody is present.")]
        [SerializeField] private Throwable throwable = new();

        private readonly TransformTweenable _transformTweenable = new();
        private Rigidbody _body;
        private bool _wasKinematic;
        private Action _tweenCompleteCallback;


        private (Vector3 position, Quaternion rotation) RightHandTarget => Constrainter.GetTargetHandTransform(HandIdentifier.Right);

        private (Vector3 position, Quaternion rotation) LeftHandTarget => Constrainter.GetTargetHandTransform(HandIdentifier.Left);

        /// <summary>Throw tracker for this grabable. Active only when Can Be Thrown is enabled and a Rigidbody is present.</summary>
        public Throwable Throwable => throwable;

        /// <summary>Whether this grabable applies a throw velocity on release.</summary>
        public bool CanBeThrown
        {
            get => canBeThrown;
            set => canBeThrown = value;
        }


        /// <inheritdoc/>
        protected override void UseStarted() { }
        
        /// <inheritdoc/>
        protected override void StartHover() { }
        
        /// <inheritdoc/>
        protected override void EndHover() { }

        /// <inheritdoc/>
        /// <returns>False to allow the grab to proceed normally</returns>
        protected override bool Select()
        {
            // Apply pose constraints with interaction point for nearest grab point selection
            Vector3 interactionPoint = CurrentInteractor.GetInteractionPoint();
            Constrainter.ApplyConstraints(CurrentInteractor.Hand, interactionPoint);

            // Force the rigidbody kinematic for the duration of the grab so the tween can
            // animate position cleanly. The prior state is restored on release.
            if (_body != null)
            {
                _wasKinematic = _body.isKinematic;
                _body.isKinematic = true;
            }

            InitializeAttachmentPointTransform();
            MoveObjectToPosition(AttachToHand);

            if (canBeThrown && _body != null)
            {
                throwable.StartTracking(_body, transform);
            }

            return false;
        }

        /// <inheritdoc/>
        protected override void DeSelected()
        {
            // Remove pose constraints and restore hand visibility
            Constrainter.RemoveConstraints(CurrentInteractor.Hand);

            // Clean up tween subscription before removing tweenable
            UnsubscribeTweenComplete();

            tweener.RemoveTweenable(_transformTweenable);

            // Detach + restore the rigidbody's prior kinematic state, then apply the throw.
            // Order matters: ApplyThrow is a no-op on kinematic bodies, so kinematic state must
            // be restored first.
            transform.SetParent(null, true);
            if (_body != null) _body.isKinematic = _wasKinematic;

            if (canBeThrown && _body != null)
            {
                throwable.ApplyThrow();
            }
        }

        private void AttachToHand()
        {
            transform.SetParent(CurrentInteractor.AttachmentPoint, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void FixedUpdate()
        {
            if (IsSelected && canBeThrown && _body != null)
            {
                throwable.Sample();
            }
        }

        protected override void InitializeInteractable()
        {
            base.InitializeInteractable();

            tweener ??= GetComponent<VariableTweener>();
            if (!tweener)
            {
                tweener = gameObject.AddComponent<VariableTweener>();
                tweener.TweenScale = 15;
            }

            _body = GetComponent<Rigidbody>();
        }
        
        private void InitializeAttachmentPointTransform()
        {
            var (handLocalPosition, handLocalRotation) = CurrentInteractor.Hand.HandIdentifier == HandIdentifier.Left ?
                LeftHandTarget : RightHandTarget;

            // Hand offsets are in ConstraintTransform local space which is scaled by the
            // interactable's own scale. Convert to world scale for the attachment point.
            Vector3 constraintScale = Constrainter.ConstraintTransform.lossyScale;
            Vector3 scaledHandPosition = Vector3.Scale(handLocalPosition, constraintScale);

            Quaternion objectRotation = Quaternion.Inverse(handLocalRotation);
            Vector3 objectPosition = objectRotation * (-scaledHandPosition);

            CurrentInteractor.AttachmentPoint.localPosition = objectPosition;
            CurrentInteractor.AttachmentPoint.localRotation = objectRotation;
        }

        private void MoveObjectToPosition(Action callBack)
        {
            UnsubscribeTweenComplete();
            
            _transformTweenable.Initialize(transform, CurrentInteractor.AttachmentPoint);
            tweener.AddTweenable(_transformTweenable);
            
            // Store callback reference so we can unsubscribe later
            _tweenCompleteCallback = callBack;
            _transformTweenable.OnTweenComplete += _tweenCompleteCallback;
        }

        private void UnsubscribeTweenComplete()
        {
            if (_tweenCompleteCallback != null)
            {
                _transformTweenable.OnTweenComplete -= _tweenCompleteCallback;
                _tweenCompleteCallback = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeTweenComplete();
        }
    }
}
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
    [RequireComponent(typeof(PoseConstrainter))]
    public class Grabable : InteractableBase
    {
        [Tooltip("The tweener component used for smooth grab animations. Auto-added if not present., only add if you want to the same tweener across multiple tweenables")]
        [SerializeField] private VariableTweener tweener;
        
        private readonly TransformTweenable _transformTweenable = new();
        private GrabStrategy _grabStrategy;
        private PoseConstrainter _poseConstrainter;
        private Action _tweenCompleteCallback;
        
   
        
        /// <summary>
        /// Gets the target position and rotation for the right hand during grabbing.
        /// </summary>
        /// <returns>The target position and rotation for the right hand.</returns>
        public (Vector3 position, Quaternion rotation) GetRightHandTarget() => _poseConstrainter.GetTargetHandTransform(HandIdentifier.Right);
        
        /// <summary>
        /// Gets the target position and rotation for the left hand during grabbing.
        /// </summary>
        /// <returns>The target position and rotation for the left hand.</returns>
        public (Vector3 position, Quaternion rotation) GetLeftHandTarget() => _poseConstrainter.GetTargetHandTransform(HandIdentifier.Left);


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
            _poseConstrainter.ApplyConstraints(CurrentInteractor.Hand, interactionPoint);

            _grabStrategy.Initialize(CurrentInteractor);
            InitializeAttachmentPointTransform();
            MoveObjectToPosition(() => _grabStrategy.Grab(this, CurrentInteractor));
            return false;
        }
        
        /// <inheritdoc/>
        protected override void DeSelected()
        {
            // Remove pose constraints and restore hand visibility
            _poseConstrainter.RemoveConstraints(CurrentInteractor.Hand);
            
            // Clean up tween subscription before removing tweenable
            UnsubscribeTweenComplete();
            
            tweener.RemoveTweenable(_transformTweenable);
            _grabStrategy.UnGrab(this, CurrentInteractor);
        }
        
        public override void InitializeInteractable()
        {
            base.InitializeInteractable();
            
            _poseConstrainter ??= GetComponent<PoseConstrainter>();
            tweener ??= GetComponent<VariableTweener>();
            if (!tweener)
            {
                tweener = gameObject.AddComponent<VariableTweener>();
                tweener.TweenScale = 15;
            }
            
            Rigidbody body = GetComponent<Rigidbody>();
            if (body)
            {
                _grabStrategy = new RigidBodyGrabStrategy(body);
            }
            else
            {
                _grabStrategy = new TransformGrabStrategy(transform);
            }
        }
        
        private void InitializeAttachmentPointTransform()
        {
            var (handLocalPosition, handLocalRotation) = CurrentInteractor.Hand.HandIdentifier == HandIdentifier.Left ?
                GetLeftHandTarget() : GetRightHandTarget();

            // Hand offsets are in ConstraintTransform local space which is scaled by the
            // interactable's own scale. Convert to world scale for the attachment point.
            Vector3 constraintScale = _poseConstrainter.ConstraintTransform.lossyScale;
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
using System;
using Shababeek.Utilities;
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
        [Tooltip("Whether to hide the hand model when this object is grabbed.")]
        [SerializeField] protected bool hideHand;//TODO: remove and rely on PoseConstrainter instead
        
        [Tooltip("The tweener component used for smooth grab animations. Auto-added if not present., only add if you want to the same tweener across multiple tweenables")]
        [SerializeField] private VariableTweener tweener;
        
        private readonly TransformTweenable _transformTweenable = new();
        private GrabStrategy _grabStrategy;
        private PoseConstrainter _poseConstrainter;
        
        /// <summary>
        /// Gets the transform for the right hand's relative position during grabbing.
        /// </summary>
        /// <value>The transform representing the right hand's grab position.</value>
        public Transform RightHandRelativePosition => _poseConstrainter.RightHandTransform;
        
        /// <summary>
        /// Gets the transform for the left hand's relative position during grabbing.
        /// </summary>
        /// <value>The transform representing the left hand's grab position.</value>
        public Transform LeftHandRelativePosition => _poseConstrainter.LeftHandTransform;
        
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
            // Apply pose constraints and visibility control
            _poseConstrainter.ApplyConstraints(CurrentInteractor.Hand);
            
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
            
            tweener.RemoveTweenable(_transformTweenable);
            _grabStrategy.UnGrab(this, CurrentInteractor);
        }
        
        protected virtual void Awake()
        {
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
            // Get target position and rotation for the current hand (in local coordinates)
            var (localPosition, localRotation) = CurrentInteractor.Hand.HandIdentifier == HandIdentifier.Left ? 
                GetLeftHandTarget() : GetRightHandTarget();
            
            // Convert local coordinates to world coordinates
            var worldPosition = _poseConstrainter.transform.TransformPoint(localPosition);
            var worldRotation = _poseConstrainter.transform.rotation * localRotation;
            
            // Calculate the attachment point offset using pure math
            // The attachment point needs to represent where the object currently is
            // relative to where the hand target is
            Quaternion inverseTargetRotation = Quaternion.Inverse(worldRotation);
            Vector3 localPositionOffset = inverseTargetRotation * (transform.position - worldPosition);
            Quaternion localRotationOffset = inverseTargetRotation * transform.rotation;
            
            CurrentInteractor.AttachmentPoint.localPosition = localPositionOffset;
            CurrentInteractor.AttachmentPoint.localRotation = localRotationOffset;
        }

        private void MoveObjectToPosition(Action callBack)
        {
            _transformTweenable.Initialize(transform, CurrentInteractor.AttachmentPoint);
            tweener.AddTweenable(_transformTweenable);
            _transformTweenable.OnTweenComplete += callBack;
        }
    }
}
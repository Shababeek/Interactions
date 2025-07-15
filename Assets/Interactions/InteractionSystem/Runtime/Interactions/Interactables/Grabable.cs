using System;
using Kandooz.InteractionSystem.Core;
using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    [RequireComponent(typeof(InteractionPoseConstrainer))]
    public class Grabable : InteractableBase
    {
        [SerializeField] protected bool hideHand;
        [SerializeField] private VariableTweener tweener;

        private TransformTweenable transformTweenable= new();
        private GrabStrategy grabStrategy;
        private InteractionPoseConstrainer poseConstrainer;
        public Transform RightHandRelativePosition => poseConstrainer.RightHandTransform;
        public Transform LeftHandRelativePosition => poseConstrainer.LeftHandTransform;

        protected override void Activate(){}
        protected override void StartHover(){}
        protected override void EndHover(){}

        protected override void Select()
        {
            if (hideHand) CurrentInteractor.ToggleHandModel(false);
            grabStrategy.Initialize(CurrentInteractor);
            InitializeAttachmentPointTransform();
            LerpObjectToPosition(() => grabStrategy.Grab(this, CurrentInteractor));
        }
        protected override void DeSelected()
        {
            if (hideHand) CurrentInteractor.ToggleHandModel(true);
            tweener.RemoveTweenable(transformTweenable);
            grabStrategy.UnGrab(this, CurrentInteractor);
        }
        
        private void Awake()
        {
            poseConstrainer ??= GetComponent<InteractionPoseConstrainer>();
            tweener ??= GetComponent<VariableTweener>();
            if (!tweener)
            {
                tweener = gameObject.AddComponent<VariableTweener>();
                tweener.tweenScale = 10;
            }
            
            Rigidbody body = GetComponent<Rigidbody>();
            if (body)
            {
                grabStrategy = new RigidBodyGrabStrategy(body);
            }
            else
            {
                grabStrategy = new TransformGrabStrategy(transform);
            }
        }
        
        private void InitializeAttachmentPointTransform()
        {
            var relativeTransform = CurrentInteractor.HandIdentifier == HandIdentifier.Left ? LeftHandRelativePosition : RightHandRelativePosition;
            relativeTransform.parent = null;
            transform.parent = relativeTransform;
            CurrentInteractor.AttachmentPoint.localPosition = transform.localPosition;
            CurrentInteractor.AttachmentPoint.localRotation = transform.localRotation;
            transform.parent = null;
            relativeTransform.parent = poseConstrainer.PivotParent;
        }

        private void LerpObjectToPosition(Action callBack)
        {
            transformTweenable.Initialize(transform, CurrentInteractor.AttachmentPoint);
            tweener.AddTweenable(transformTweenable);
            transformTweenable.OnTweenComplete += callBack;
        }
        
    }

}
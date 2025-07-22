using System;
using Shababeek.Core;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Interactables/Grabable")]
    [RequireComponent(typeof(InteractionPoseConstrainer))]
    public class Grabable : InteractableBase
    {
        [SerializeField] protected bool hideHand;
        [SerializeField] private VariableTweener tweener;
        private readonly TransformTweenable _transformTweenable= new();
        private GrabStrategy _grabStrategy;
        private InteractionPoseConstrainer _poseConstrainer;
        public Transform RightHandRelativePosition => _poseConstrainer.RightHandTransform;
        public Transform LeftHandRelativePosition => _poseConstrainer.LeftHandTransform;

        protected override void Activate(){}
        protected override void StartHover(){}
        protected override void EndHover(){}

        protected override bool Select()
        {
            if (hideHand) CurrentInteractor.ToggleHandModel(false);
            _grabStrategy.Initialize(CurrentInteractor);
            InitializeAttachmentPointTransform();
            MoveObjectToPosition(() => _grabStrategy.Grab(this, CurrentInteractor));
            return false;
        }
        protected override void DeSelected()
        {
            if (hideHand) CurrentInteractor.ToggleHandModel(true);
            tweener.RemoveTweenable(_transformTweenable);
            _grabStrategy.UnGrab(this, CurrentInteractor);
        }
        
        protected virtual void Awake()
        {
            _poseConstrainer ??= GetComponent<InteractionPoseConstrainer>();
            tweener ??= GetComponent<VariableTweener>();
            if (!tweener)
            {
                tweener = gameObject.AddComponent<VariableTweener>();
                tweener.tweenScale = 15;
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
            var relativeTransform = CurrentInteractor.HandIdentifier == HandIdentifier.Left ? LeftHandRelativePosition : RightHandRelativePosition;
            relativeTransform.parent = null;
            transform.parent = relativeTransform;
            CurrentInteractor.AttachmentPoint.localPosition = transform.localPosition;
            CurrentInteractor.AttachmentPoint.localRotation = transform.localRotation;
            transform.parent = null;
            relativeTransform.parent = _poseConstrainer.PivotParent;
        }

        private void MoveObjectToPosition(Action callBack)
        {
            _transformTweenable.Initialize(transform, CurrentInteractor.AttachmentPoint);
            tweener.AddTweenable(_transformTweenable);
            _transformTweenable.OnTweenComplete += callBack;
        }
        
    }

}
using System;
using Kandooz.Interactions;
using Kandooz.InteractionSystem.Core;
using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    [RequireComponent(typeof(Hand))]
    public abstract class InteractorBase : MonoBehaviour
    {
        private Hand hand;
        [SerializeField] [ReadOnly] protected InteractableBase currentInteractable;
        [SerializeField][ReadOnly] protected bool isInteracting;
        private Transform attachmentPoint;
        private XRButtonObserver onInteractionStateChanged;
        private XRButtonObserver onActivate;
        private IDisposable interactionSubscriber, activationSubscriber;
        private Joint attachmentJoint;
        public Transform AttachmentPoint => attachmentPoint;
        public HandIdentifier HandIdentifier => hand.HandIdentifier;
        public Joint InteractorAttachmentJoint => attachmentJoint;
        public Hand Hand => hand;
        public bool IsInteracting => isInteracting;
        public event Action onHoverEnd;

        public void ToggleHandModel(bool enable) => hand.ToggleRenderer(enable);

        private void Awake()
        {
            GetDependencies();
            InitializeAttachmentPoint();
            InitializeJoint();

            onInteractionStateChanged = new XRButtonObserver((state) =>
            {
                if (currentInteractable is null) return;
                switch (state)
                {
                    case ButtonState.Up:
                        if (currentInteractable.CurrentState == InteractionState.Selected && currentInteractable.CurrentInteractor == this)
                        {
                            OnDeSelect();
                        }

                        break;
                    case ButtonState.Down:
                        if (currentInteractable.CurrentState == InteractionState.Hovering)
                        {
                            OnSelect();
                        }

                        break;
                }
            }, null, null);
            onActivate = new XRButtonObserver((state) =>
            {
                if (currentInteractable is null) return;
                switch (state)
                {
                    case ButtonState.Down:
                        OnActivate();
                        break;
                }
            }, null, null);
        }

        private void InitializeJoint()
        {
            var jointObject = new GameObject("Joint Object");
            jointObject.transform.parent = attachmentPoint;
            jointObject.AddComponent<Rigidbody>().isKinematic = true;
            attachmentJoint = jointObject.AddComponent<FixedJoint>();
            attachmentJoint.enableCollision = false;
            jointObject.SetActive(false);
        }

        private void GetDependencies()
        {
            hand = GetComponent<Hand>();
        }

        private void InitializeAttachmentPoint()
        {
            var attachmentObject = new GameObject("AttachmentPoint");
            attachmentObject.transform.parent = transform;
            attachmentPoint = attachmentObject.transform;
            attachmentPoint.localPosition = Vector3.zero;
            attachmentPoint.localRotation = Quaternion.identity;
        }

        protected void OnHoverStart()
        {
            if (currentInteractable == null || currentInteractable.IsSelected) return;
            if (!currentInteractable.IsValidHand(this.Hand)) return ;
            
            currentInteractable.OnStateChanged(InteractionState.Hovering, this);
            interactionSubscriber = currentInteractable.SelectionButton switch
            {
                XRButton.Grip => hand.OnGripButtonStateChange.Subscribe(onInteractionStateChanged),
                XRButton.Trigger => hand.OnTriggerTriggerButtonStateChange.Subscribe(onInteractionStateChanged),
                _ => interactionSubscriber
            };
        }

        protected virtual void OnHoverEnd()
        {
            if (currentInteractable.CurrentState != InteractionState.Hovering) return;
            currentInteractable.OnStateChanged(InteractionState.None, this);
            interactionSubscriber?.Dispose();
            currentInteractable = null;
        }

        protected void OnSelect()
        {
            if (currentInteractable == null ||  currentInteractable.IsSelected) return;
            isInteracting = true;
            currentInteractable.OnStateChanged(InteractionState.Selected, this);
            activationSubscriber = currentInteractable.SelectionButton switch
            {
                XRButton.Trigger => hand.OnGripButtonStateChange.Subscribe(onActivate),
                XRButton.Grip => hand.OnTriggerTriggerButtonStateChange.Subscribe(onActivate),
                _ => activationSubscriber
            };
        }

        private void OnDeSelect()
        {
            isInteracting = false;
            activationSubscriber?.Dispose();
            interactionSubscriber?.Dispose();
            currentInteractable.OnStateChanged(InteractionState.None,this);
            OnHoverStart(); 
        }

        private void OnActivate()
        {
            if (!currentInteractable) return;

            currentInteractable.OnStateChanged(InteractionState.Activated, this);
        }

        public void ToggleJointObject(bool enable)
        {
            attachmentJoint.gameObject.SetActive(enable);
        }
    }
}
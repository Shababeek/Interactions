using System;
using Shababeek.Core;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Base class for interactors in the interaction system.
    /// This class provides common functionality for interactors, such as handling interaction states,
    /// managing the current interactable, and providing an attachment point for held objects.
    /// </summary>
    [RequireComponent(typeof(Hand))]
    public abstract class InteractorBase : MonoBehaviour
    {
        [SerializeField] [ReadOnly] public InteractableBase currentInteractable;
        [SerializeField] [ReadOnly] protected bool isInteracting;
        private Hand _hand;
        private Transform _attachmentPoint;
        private readonly Subject<VRButtonState> _onInteractionStateChanged = new();
        private readonly Subject<VRButtonState> _onActivate = new();
        private IDisposable _hoverSubscriber, _activationSubscriber;
        private Joint _attachmentJoint;
        /// <summary>
        /// The attachment point transform for objects held by this interactor.
        /// </summary>
        public Transform AttachmentPoint => _attachmentPoint;
        /// <summary>
        /// The hand identifier (left or right) for this interactor.
        /// </summary>
        public HandIdentifier HandIdentifier => _hand.HandIdentifier;
        /// <summary>
        /// The Hand component associated with this interactor.
        /// </summary>
        public Hand Hand => _hand;
        protected bool IsInteracting => isInteracting;

        /// <summary>
        /// Toggles the visibility of the hand model.
        /// </summary>
        /// <param name="enable">If true, the hand model is shown; otherwise, it is hidden.</param>
        public void ToggleHandModel(bool enable)
        {
            _hand.ToggleRenderer(enable);
        }

        private void Awake()
        {
            GetDependencies();
            InitializeAttachmentPoint();
            _onInteractionStateChanged
                .Do((state) =>
                {
                    if (currentInteractable is null) return;
                    switch (state)
                    {
                        case VRButtonState.Up:
                            if (currentInteractable.CurrentState == InteractionState.Selected && currentInteractable.CurrentInteractor == this) OnDeSelect();

                            break;
                        case VRButtonState.Down:
                            if (currentInteractable.CurrentState == InteractionState.Hovering) OnSelect();

                            break;
                    }
                })
                .Subscribe().AddTo(this);
            _onActivate
                .Do((state) =>
                {
                    if (currentInteractable is null) return;
                    switch (state)
                    {
                        case VRButtonState.Down:
                            OnActivate();
                            break;
                    }
                })
                .Subscribe().AddTo(this);
        }


        private void GetDependencies()
        {
            _hand = GetComponent<Hand>();
        }

        private void InitializeAttachmentPoint()
        {
            var attachmentObject = new GameObject("AttachmentPoint");
            attachmentObject.transform.parent = transform;
            _attachmentPoint = attachmentObject.transform;
            _attachmentPoint.localPosition = Vector3.zero;
            _attachmentPoint.localRotation = Quaternion.identity;
        }

        protected void OnHoverStart()
        {
            if (currentInteractable == null || currentInteractable.IsSelected) return;
            if (!currentInteractable.IsValidHand(Hand)) return;

            currentInteractable.OnStateChanged(InteractionState.Hovering, this);
            var onInteractButtonPressed = currentInteractable.SelectionButton switch
            {
                XRButton.Grip => _hand.OnGripButtonStateChange,
                XRButton.Trigger => _hand.OnTriggerTriggerButtonStateChange,
                _ => null
            };
            _hoverSubscriber = onInteractButtonPressed.Do(_onInteractionStateChanged).Subscribe();
        }

        protected virtual void OnHoverEnd()
        {
            if (currentInteractable.CurrentState != InteractionState.Hovering) return;
            currentInteractable.OnStateChanged(InteractionState.None, this);
            _hoverSubscriber?.Dispose();
            currentInteractable = null;
        }

        /// <summary>
        /// Called when the interactor selects an interactable object.
        /// </summary>
        public void OnSelect()
        {
            if (currentInteractable == null || currentInteractable.IsSelected) return;
            isInteracting = true;
            currentInteractable.OnStateChanged(InteractionState.Selected, this);

            var onInteractButtonPressed = currentInteractable.SelectionButton switch
            {
                XRButton.Trigger => _hand.OnGripButtonStateChange,
                XRButton.Grip => _hand.OnTriggerTriggerButtonStateChange,
                _ => null
            };
            _activationSubscriber = onInteractButtonPressed.Do(_onActivate).Subscribe();
        }

        /// <summary>
        /// Called when the interactor deselects an interactable object.
        /// </summary>
        public void OnDeSelect()
        {
            isInteracting = false;
            _activationSubscriber?.Dispose();
            _hoverSubscriber?.Dispose();
            currentInteractable.OnStateChanged(InteractionState.None, this);
            OnHoverStart();
        }

        private void OnActivate()
        {
            if (!currentInteractable) return;

            currentInteractable.OnStateChanged(InteractionState.Activated, this);
        }
        
    }
}
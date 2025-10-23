using System;
using Shababeek.Utilities;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Base class for interactors providing common interaction functionality.
    /// </summary>
    [RequireComponent(typeof(Hand))]
    public abstract class InteractorBase : MonoBehaviour
    {
        [SerializeField] [ReadOnly] [Tooltip("The currently hovered or selected interactable object")]
        private InteractableBase currentInteractable;

        [SerializeField] [ReadOnly] [Tooltip("Whether this interactor is currently interacting with an object")]
        protected bool isInteracting;

        private Hand _hand;
        private Transform _attachmentPoint;
        private readonly Subject<VRButtonState> _onInteractionStateChanged = new();
        private readonly Subject<VRButtonState> _onActivate = new();
        private readonly CompositeDisposable _disposables = new();
        private IDisposable _hoverSubscriber, _activationSubscriber;

        /// <summary>
        /// Attachment point transform for held objects.
        /// </summary>
        public Transform AttachmentPoint => _attachmentPoint;

        /// <summary>
        /// Hand identifier (left or right).
        /// </summary>
        public HandIdentifier HandIdentifier => _hand.HandIdentifier;

        /// <summary>
        /// Hand component associated with this interactor.
        /// </summary>
        public Hand Hand => _hand;

        /// <summary>
        /// Whether currently interacting with an object.
        /// </summary>
        protected bool IsInteracting => isInteracting;

        /// <summary>
        /// Currently hovered or selected interactable object.
        /// </summary>
        public InteractableBase CurrentInteractable
        {
            get => currentInteractable;
            set => currentInteractable = value;
        }

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
                .Do(HandleInteractionStateChanged)
                .Subscribe()
                .AddTo(_disposables);

            _onActivate
                .Do(HandleActivation)
                .Subscribe()
                .AddTo(_disposables);
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
            if (!currentInteractable || currentInteractable.IsSelected) return;
            if (!currentInteractable.IsValidHand(Hand)) return;

            currentInteractable.OnStateChanged(InteractionState.Hovering, this);
            var buttonObservable =
                GetButtonObservable(currentInteractable.SelectionButton, ButtonMappingType.Selection);
            _hoverSubscriber = buttonObservable?.Do(_onInteractionStateChanged).Subscribe();
        }

        protected virtual void OnHoverEnd()
        {
            if (currentInteractable && currentInteractable.CurrentState != InteractionState.Hovering) return;
            currentInteractable.OnStateChanged(InteractionState.None, this);
            DisposeHoverSubscription();
            currentInteractable = null;
        }

        /// <summary>
        /// Selects an interactable object.
        /// </summary>
        public void Select()
        {
            if (!currentInteractable || currentInteractable.IsSelected) return;
            isInteracting = true;
            currentInteractable.OnStateChanged(InteractionState.Selected, this);

            var buttonObservable =
                GetButtonObservable(currentInteractable.SelectionButton, ButtonMappingType.Activation);
            _activationSubscriber = buttonObservable?.Do(_onActivate).Subscribe();
        }

        /// <summary>
        /// Deselects an interactable object.
        /// </summary>
        public void DeSelect()
        {
            isInteracting = false;
            DisposeActivationSubscription();
            DisposeHoverSubscription();
            currentInteractable.OnStateChanged(InteractionState.None, this);
            OnHoverStart();
        }


        private void Use()
        {
            if (!currentInteractable) return;
            if (currentInteractable.CurrentState != InteractionState.Selected) return;

            // Call directly on the interactable without state change
            currentInteractable.StartUsing(this);
        }

        private void UnUse()
        {
            if (!currentInteractable) return;
            if (currentInteractable.CurrentState != InteractionState.Selected) return;

            currentInteractable.StopUsing(this);
        }

        private void HandleInteractionStateChanged(VRButtonState state)
        {
            if (currentInteractable == null) return;
            switch (state)
            {
                case VRButtonState.Up:
                    if (currentInteractable.CurrentState == InteractionState.Selected &&
                        currentInteractable.CurrentInteractor == this)
                        DeSelect();
                    break;
                case VRButtonState.Down:
                    if (currentInteractable.CurrentState == InteractionState.Hovering)
                        Select();
                    break;
            }
        }

        private void HandleActivation(VRButtonState state)
        {
            if (currentInteractable == null) return;
            switch (state)
            {
                case VRButtonState.Down:
                    Use();
                    break;
                case VRButtonState.Up:
                    UnUse();
                    break;
            }
        }

        private void DisposeHoverSubscription()
        {
            _hoverSubscriber?.Dispose();
            _hoverSubscriber = null;
        }

        private void DisposeActivationSubscription()
        {
            _activationSubscriber?.Dispose();
            _activationSubscriber = null;
        }

        private IObservable<VRButtonState> GetButtonObservable(XRButton button, ButtonMappingType mappingType)
        {
            return (button, mappingType) switch
            {
                (XRButton.Grip, ButtonMappingType.Selection) => _hand.OnGripButtonStateChange,
                (XRButton.Trigger, ButtonMappingType.Selection) => _hand.OnTriggerTriggerButtonStateChange,
                (XRButton.Trigger, ButtonMappingType.Activation) => _hand.OnGripButtonStateChange,
                (XRButton.Grip, ButtonMappingType.Activation) => _hand.OnTriggerTriggerButtonStateChange,
                _ => null
            };
        }


        private enum ButtonMappingType
        {
            Selection,
            Activation
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            DisposeHoverSubscription();
            DisposeActivationSubscription();
        }
        
    }
    [System.Serializable]
    public class InteractorUnityEvent : UnityEvent<InteractorBase> { }
}
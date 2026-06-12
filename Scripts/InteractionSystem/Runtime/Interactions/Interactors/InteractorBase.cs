using System;
using Shababeek.ReactiveVars;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using Hand = Shababeek.Interactions.Core.Hand;

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
        private readonly Subject<VRButtonState> _onThumb = new();
        private readonly CompositeDisposable _disposables = new();
        private IDisposable _hoverSubscriber, _activationSubscriber, _thumbSubscriber;
        private XRButton _actualSelectionButton;
        private bool _isSecondaryHold;

        /// <summary>True while this interactor holds a secondary grip on a two-handed interactable.</summary>
        public bool IsSecondaryHold => _isSecondaryHold;

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

        /// <summary>
        /// Disables every Collider on the hand hierarchy. Use to suppress physics or hover detection.
        /// </summary>
        public void DisableColliders() => _hand.ToggleColliders(false);

        /// <summary>
        /// Re-enables every Collider on the hand hierarchy.
        /// </summary>
        public void EnableColliders() => _hand.ToggleColliders(true);

        /// <summary>
        /// World position where this interactor contacts or targets interactables.
        /// Override in subclasses to provide interactor-specific contact points.
        /// </summary>
        public virtual Vector3 GetInteractionPoint()
        {
            return _hand.transform.position;
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

            _onThumb
                .Do(HandleThumbButton)
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

        protected void StartHover()
        {
            if (!currentInteractable) return;
            if (currentInteractable.IsSelected)
            {
                // Already held by another hand: if the interactable accepts a second grip,
                // subscribe the selection button only — never OnStateChanged(Hovering), which
                // would tear down the primary's Selected state.
                if (!currentInteractable.CanAcceptSecondaryInteractor(this)) return;
                DisposeHoverSubscription();
                var secondaryObservable =
                    GetButtonObservable(currentInteractable.SelectionButton, ButtonMappingType.Selection);
                _hoverSubscriber = secondaryObservable?.Do(_onInteractionStateChanged).Subscribe();
                return;
            }
            if (!currentInteractable.CanInteract(Hand)) return;
            try
            {
                currentInteractable.OnStateChanged(InteractionState.Hovering, this);                
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during hover start on {currentInteractable.name}: {e.Message}", currentInteractable);
                currentInteractable = null;
                return;
            }
            var buttonObservable =
                GetButtonObservable(currentInteractable.SelectionButton, ButtonMappingType.Selection);
            DisposeHoverSubscription();
            _hoverSubscriber = buttonObservable?.Do(_onInteractionStateChanged).Subscribe();
        }

        protected virtual void EndHover()
        {
            // Always dispose, even on the early returns below — a leaked subscription
            // lets a stale button stream select a different interactable later.
            DisposeHoverSubscription();
            if (currentInteractable==null)
            {
                return;
            }
            if (currentInteractable && currentInteractable.CurrentState != InteractionState.Hovering) return;
            try
            {
                currentInteractable.OnStateChanged(InteractionState.None, this);                
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during hover end on {currentInteractable.name}: {e.Message}", currentInteractable);
            }
            DisposeHoverSubscription();
            currentInteractable = null;
        }

        /// <summary>
        /// Selects an interactable object.
        /// </summary>
        public void Select()
        {
            if (!currentInteractable || currentInteractable.IsSelected ) return;
            if(!currentInteractable.CanInteract(Hand)) return;
            
            try{
                currentInteractable.OnStateChanged(InteractionState.Selected, this);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during selection on {currentInteractable.name}: {e.Message}", currentInteractable);

            }
            // The interactable can abort the selection (Select() returning true) or force a
            // deselect mid-selection; committing isInteracting without a selected state would
            // permanently block this interactor.
            if (!currentInteractable || currentInteractable.CurrentState != InteractionState.Selected ||
                currentInteractable.CurrentInteractor != this)
                return;
            isInteracting = true;

            // Programmatic selections (SpawningInteractable re-target, DistanceGrabber catch)
            // bypass StartHover, which is where the selection-button stream is normally
            // subscribed — without it, button-up can never reach DeSelect and the object
            // would be stuck in the hand.
            if (_hoverSubscriber == null)
            {
                var selectionObservable =
                    GetButtonObservable(currentInteractable.SelectionButton, ButtonMappingType.Selection);
                _hoverSubscriber = selectionObservable?.Do(_onInteractionStateChanged).Subscribe();
            }

            // Dispose before subscribing — SpawningInteractable re-enters Select() from inside
            // OnStateChanged, so the inner call's subscriptions would otherwise be overwritten
            // here without disposal (one leaked subscription per spawn).
            DisposeActivationSubscription();
            var buttonObservable =
                GetButtonObservable(currentInteractable.SelectionButton, ButtonMappingType.Activation);
            _activationSubscriber = buttonObservable?.Do(_onActivate).Subscribe();

            DisposeThumbSubscription();
            var thumbObservable = _hand.OnThumbButtonStateChange;
            _thumbSubscriber = thumbObservable?.Do(_onThumb).Subscribe();
        }

        /// <summary>
        /// Deselects an interactable object.
        /// </summary>
        public void DeSelect()
        {
            if (_isSecondaryHold)
            {
                // Secondary grips live outside the interactable's state machine — releasing
                // one must never send OnStateChanged(None), which would deselect the primary.
                _isSecondaryHold = false;
                isInteracting = false;
                DisposeActivationSubscription();
                DisposeHoverSubscription();
                DisposeThumbSubscription();
                if (currentInteractable != null) currentInteractable.SecondaryDeselect(this);
                currentInteractable = null;
                return;
            }

            isInteracting = false;
            DisposeActivationSubscription();
            DisposeHoverSubscription();
            DisposeThumbSubscription();
            if (currentInteractable == null) return;
            currentInteractable.OnStateChanged(InteractionState.None, this);
            StartHover();
            EndHover();
            currentInteractable = null;
        }

        /// <summary>
        /// Re-selects the current interactable as a normal (primary) grab. Called by two-handed
        /// interactables when the primary hand released while this hand held the secondary grip.
        /// </summary>
        public void PromoteSecondaryToPrimary()
        {
            if (currentInteractable == null) return;
            _isSecondaryHold = false;
            isInteracting = false;
            Select();
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
            if (currentInteractable == null)
            {
                if (isInteracting) ForceReleaseDestroyedInteractable();
                return;
            }
            switch (state)
            {
                case VRButtonState.Up:
                    if (_isSecondaryHold)
                    {
                        DeSelect();
                    }
                    else if (currentInteractable.CurrentState == InteractionState.Selected &&
                             currentInteractable.CurrentInteractor == this)
                    {
                        DeSelect();
                    }
                    break;
                case VRButtonState.Down:
                    if (currentInteractable.CurrentState == InteractionState.Hovering)
                    {
                        if (currentInteractable.SelectionButton == XRButton.Any)
                            DetectSelectionButton();
                        Select();
                    }
                    else if (!_isSecondaryHold && !isInteracting &&
                             currentInteractable.IsSelected &&
                             currentInteractable.CurrentInteractor != this &&
                             currentInteractable.CanAcceptSecondaryInteractor(this) &&
                             currentInteractable.TrySecondarySelect(this))
                    {
                        _isSecondaryHold = true;
                        isInteracting = true;
                    }
                    break;
            }
        }

        private void DetectSelectionButton()
        {
            var xrNode = _hand.HandIdentifier == HandIdentifier.Left ? XRNode.LeftHand : XRNode.RightHand;
            var device = InputDevices.GetDeviceAtXRNode(xrNode);
            if (!device.isValid) return;
            device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed);
            _actualSelectionButton = gripPressed ? XRButton.Grip : XRButton.Trigger;
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

        private void ForceReleaseDestroyedInteractable()
        {
            isInteracting = false;
            DisposeActivationSubscription();
            DisposeHoverSubscription();
            DisposeThumbSubscription();
            currentInteractable = null;
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

        private void DisposeThumbSubscription()
        {
            _thumbSubscriber?.Dispose();
            _thumbSubscriber = null;
        }

        private void HandleThumbButton(VRButtonState state)
        {
            if (currentInteractable == null) return;
            switch (state)
            {
                case VRButtonState.Down:
                    currentInteractable.ThumbPress(this);
                    break;
                case VRButtonState.Up:
                    currentInteractable.ThumbRelease(this);
                    break;
            }
        }

        private IObservable<VRButtonState> GetButtonObservable(XRButton button, ButtonMappingType mappingType)
        {
            return (button, mappingType) switch
            {
                (XRButton.Grip, ButtonMappingType.Selection) => _hand.OnGripButtonStateChange,
                (XRButton.Trigger, ButtonMappingType.Selection) => _hand.OnTriggerTriggerButtonStateChange,
                (XRButton.Any, ButtonMappingType.Selection) => _hand.OnAnyButtonStateChange,
                (XRButton.Trigger, ButtonMappingType.Activation) => _hand.OnGripButtonStateChange,
                (XRButton.Grip, ButtonMappingType.Activation) => _hand.OnTriggerTriggerButtonStateChange,
                (XRButton.Any, ButtonMappingType.Activation) => _actualSelectionButton == XRButton.Grip
                    ? _hand.OnTriggerTriggerButtonStateChange
                    : _hand.OnGripButtonStateChange,
                _ => null
            };
        }


        private enum ButtonMappingType
        {
            Selection,
            Activation
        }

        private void OnDisable()
        {
            if (currentInteractable != null)
            {
                Release(currentInteractable);
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            DisposeHoverSubscription();
            DisposeActivationSubscription();
            DisposeThumbSubscription();
        }

        public void Release(InteractableBase interactableBase)
        {
            if(currentInteractable!=interactableBase) return;
            if (isInteracting)
            {
                DeSelect();
            }
            else
            {
                EndHover();
            }
        }

        /// <summary>
        /// Plays a curve-based haptic pattern on this interactor's controller.
        /// </summary>
        /// <param name="pattern">The pattern asset to play; null is ignored.</param>
        public async void PlayHapticPattern(HapticPattern pattern)
        {
            if (pattern == null) return;
            try
            {
                await HapticPatternPlayer.Play(pattern, SendHapticImpulse, destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Interactor destroyed mid-pattern — nothing to clean up.
            }
        }

        /// <summary>
        /// Sends a haptic impulse to the VR controller associated with this interactor's hand.
        /// </summary>
        /// <param name="hapticAmplitude">Vibration intensity from 0 (none) to 1 (max).</param>
        /// <param name="hapticDuration">Duration of the haptic pulse in seconds.</param>
        public void SendHapticImpulse(float hapticAmplitude, float hapticDuration)
        {
            var xrNode = _hand.HandIdentifier == HandIdentifier.Left ? XRNode.LeftHand : XRNode.RightHand;
            var inputDevice = InputDevices.GetDeviceAtXRNode(xrNode);
            if (inputDevice.isValid)
            {
                inputDevice.SendHapticImpulse(0, hapticAmplitude, hapticDuration);
            }
        }
    }
    [System.Serializable]
    public class InteractorUnityEvent : UnityEvent<InteractorBase> { }
}
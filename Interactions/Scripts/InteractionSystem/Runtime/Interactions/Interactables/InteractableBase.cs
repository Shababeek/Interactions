using System;
using Shababeek.Core;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Core;
using Shababeek.Interactions;
using UniRx;
using UniRx.Triggers;


namespace Shababeek.Interactions
{
    [Flags]
    public enum InteractionHand
    {
        Left = 1,
        Right = 2,
    }

    /// <summary>
    /// Base class for all interactable objects in the interaction system.
    /// Provides the foundation for hover, selection, and activation interactions.
    /// Derived classes must implement the abstract interaction methods.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour
    {
        [Tooltip("Specifies which hands can interact with this object (Left, Right, or Both).")]
        [SerializeField] private InteractionHand interactionHand = (InteractionHand.Left | InteractionHand.Right);
        
        [Tooltip("The button that triggers selection of this interactable (Grip or Trigger).")]
        [SerializeField] private XRButton selectionButton = XRButton.Grip;
        
        [Tooltip("Event raised when this interactable is selected by an interactor.")]
        [SerializeField] public InteractorUnityEvent onSelected;
        
        [Tooltip("Event raised when this interactable is deselected by an interactor.")]
        [SerializeField] private InteractorUnityEvent onDeselected;
        
        [Tooltip("Event raised when an interactor starts hovering over this interactable.")]
        [SerializeField] private InteractorUnityEvent onHoverStart;
        
        [Tooltip("Event raised when an interactor stops hovering over this interactable.")]
        [SerializeField] private InteractorUnityEvent onHoverEnd;
        
        [Tooltip("Event raised when this the alterntive button is pressed while the object is being interacted with by an interactor.")]
        [SerializeField] private InteractorUnityEvent onActivated;
        
        [Tooltip("Indicates whether this interactable is currently selected.")]
        [SerializeField][ReadOnly] private bool isSelected;
        
        [Tooltip("The interactor that is currently being interacted/Hovered/Selected with by this interactor.")]
        [SerializeField][ReadOnly] private InteractorBase currentInteractor;
        
        [Tooltip("The current interaction state of this interactable.")]
        [SerializeField][ReadOnly] private InteractionState currentState;
        
        /// <summary>
        /// The interaction point transform for this interactable.
        /// </summary>
        /// <Todo>
        /// Add a custom interaction point defnision strategy.
        /// </Todo>
        /// <value>The transform representing the interaction point, or null if not specified.</value>
        public virtual Transform InteractionPoint => null;
        
        /// <summary>
        /// Indicates whether this interactable is currently selected by an interactor.
        /// </summary>
        /// <value>True if the interactable is selected, false otherwise.</value>
        public bool IsSelected => isSelected;
        
        /// <summary>
        /// Observable that fires when this interactable is selected by an interactor.
        /// </summary>
        /// <value>An observable that emits the interactor that selected this object.</value>
        public IObservable<InteractorBase> OnSelected => onSelected.AsObservable();
        
        /// <summary>
        /// Observable that fires when this interactable is deselected by an interactor.
        /// </summary>
        /// <value>An observable that emits the interactor that deselected this object.</value>
        public IObservable<InteractorBase> OnDeselected => onDeselected.AsObservable();
        
        /// <summary>
        /// Observable that fires when an interactor starts hovering over this interactable.
        /// </summary>
        /// <value>An observable that emits the interactor that started hovering.</value>
        public IObservable<InteractorBase> OnHoverStarted => onHoverStart.AsObservable();
        
        /// <summary>
        /// Observable that fires when an interactor stops hovering over this interactable.
        /// </summary>
        /// <value>An observable that emits the interactor that stopped hovering.</value>
        public IObservable<InteractorBase> OnHoverEnded => onHoverEnd.AsObservable();
        
        /// <summary>
        /// Observable that fires when this interactable is activated by an interactor.
        /// </summary>
        /// <value>An observable that emits the interactor that activated this object.</value>
        public IObservable<InteractorBase> OnActivated => onActivated.AsObservable();
        
        /// <summary>
        /// The button that triggers selection of this interactable.
        /// </summary>
        /// <value>The XR button used for selection (Grip or Trigger).</value>
        public XRButton SelectionButton => selectionButton;
        
        /// <summary>
        /// The current interaction state of this interactable.
        /// </summary>
        /// <value>The current interaction state (None, Hovering, Selected, or Activated).</value>
        public InteractionState CurrentState => currentState;
        
        /// <summary>
        /// The interactor that is currently interacting with this object.
        /// </summary>
        /// <value>The current interactor, or null if no interaction is active.</value>
        public InteractorBase CurrentInteractor => currentInteractor;
        

        /// <summary>
        /// Handles state changes for this interactable.
        /// This method manages the transition between different interaction states
        /// and invokes the appropriate events in the interactable .
        /// </summary>
        /// <param name="state">The new interaction state to transition to.</param>
        /// <param name="interactor">The interactor that triggered the state change.</param>
 
        public void OnStateChanged(InteractionState state, InteractorBase interactor)
        {
            if (this.currentState == state) return;
            currentInteractor = interactor;
            if (state == InteractionState.None)
                HandleNoneState();
            else if (state == InteractionState.Hovering)
                HandleHoverState();
            else if (state == InteractionState.Selected)
                HandleSelectionState();
            else if (state == InteractionState.Activated)
                HandleActiveState();
        }

        private void HandleNoneState()
        {
            if (currentState == InteractionState.Selected)
            {
                DeSelected();
                isSelected = false;
                onDeselected.Invoke(currentInteractor);
                currentInteractor = null;
            }

            else if (currentState == InteractionState.Hovering)
            {
                EndHover();
                onHoverEnd.Invoke(currentInteractor);
            }

            currentState = InteractionState.None;
            currentInteractor = null;
        }

        private void HandleHoverState()
        {
            if (currentState == InteractionState.Selected)
            {
                isSelected = false;
                onDeselected.Invoke(currentInteractor);
                DeSelected();
            }

            currentState = InteractionState.Hovering;
            StartHover();
            onHoverStart.Invoke(currentInteractor);
        }

        private void HandleSelectionState()
        {
            if (currentState == InteractionState.Hovering)
            {
                onHoverEnd.Invoke(currentInteractor);
                EndHover();
            }

            try
            {
                if(Select())return;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            isSelected = true;
            onSelected.Invoke(currentInteractor);
            currentState = InteractionState.Selected;
        }
        
        private void HandleActiveState()
        {
            if (this.currentState == InteractionState.Selected)
            {
                onActivated.Invoke(currentInteractor);
                Activate();
            }
        }

        protected abstract void Activate();
        protected abstract void StartHover();
        protected abstract void EndHover();
        protected abstract bool Select();
        protected abstract void DeSelected();

        public bool IsValidHand(HandIdentifier hand)
        {
            var handID = (int)hand;
            var valid = (int)interactionHand;
            return (valid & handID) != 0;
        }
    }
}
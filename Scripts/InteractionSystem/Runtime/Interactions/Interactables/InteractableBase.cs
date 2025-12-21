using System;
using Shababeek.Utilities;
using UnityEngine;
using Shababeek.Interactions.Core;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Defines which hands can interact with an interactable object.
    /// </summary>
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
    /// <remarks>
    /// This class handles the core interaction logic including state management,
    /// event dispatching, and hand validation. Inherit from this class to create
    /// custom interactable objects with specific interaction behaviors.
    /// </remarks>
    public abstract class InteractableBase : MonoBehaviour
    {
        //TODO: Replace abstract methods with event subscriptions
        [Header("Interaction Settings")]
        [Tooltip("Specifies which hands can interact with this object (Left, Right, or Both).")]
        [SerializeField] private InteractionHand interactionHand = (InteractionHand.Left | InteractionHand.Right);
        
        [Tooltip("The button that triggers selection of this interactable (Grip or Trigger).")]
        [SerializeField] private XRButton selectionButton = XRButton.Grip;
        
        [Header("Interaction Events")]
        [Tooltip("Event raised when this interactable is selected by an interactor.")]
        [SerializeField] public InteractorUnityEvent onSelected;
        
        [Tooltip("Event raised when this interactable is deselected by an interactor.")]
        [SerializeField] private InteractorUnityEvent onDeselected;
        
        [Tooltip("Event raised when an interactor starts hovering over this interactable.")]
        [SerializeField] private InteractorUnityEvent onHoverStart;
        
        [Tooltip("Event raised when an interactor stops hovering over this interactable.")]
        [SerializeField] private InteractorUnityEvent onHoverEnd;
        
        [Tooltip("Event raised when the secondary button is pressed while selected.")]
        [SerializeField] private InteractorUnityEvent onUseStarted;

        [Tooltip("Event raised when the secondary button is released while selected.")]
        [SerializeField] private InteractorUnityEvent onUseEnded;
        
        [Header("Runtime State")]
        [Tooltip("Indicates whether this interactable is currently selected.")]
        [SerializeField][ReadOnly] private bool isSelected;
        
        [Tooltip("The interactor that is currently interacting with this object.")]
        [SerializeField][ReadOnly] private InteractorBase currentInteractor;
        
        [Tooltip("The current interaction state of this interactable.")]
        [SerializeField][ReadOnly] private InteractionState currentState;
        
        [Tooltip("Indicates whether this interactable is currently being used (secondary button pressed).")]
        [SerializeField][ReadOnly] private bool isUsing;
        
        // Scale compensation - prevents shearing during editor pose setup and runtime manipulation
        protected Transform _scaleCompensator;
        
        /// <summary>
        /// The interaction point transform for this interactable.
        /// </summary>
        /// <Todo>
        /// Add a custom interaction point defnision strategy.
        /// </Todo>
        /// <value>The transform representing the interaction point, or null if not specified.</value>
        public virtual Transform InteractionPoint => null;
        
        /// <summary>
        /// Transform used for pose constraints and hand positioning.
        /// Returns the scale compensator if it exists, otherwise returns this transform.
        /// </summary>
        public Transform ConstraintTransform => _scaleCompensator ? _scaleCompensator : transform;
        
        /// <summary>
        /// Gets the interactableObject child transform if it exists.
        /// This is the standard location for models/visuals in all interactables.
        /// </summary>
        public Transform InteractableObject => !_scaleCompensator ? null : _scaleCompensator.Find("interactableObject");

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
        /// Observable that fires when the secondary button is pressed while this interactable is selected.
        /// </summary>
        /// <value>An observable that emits the interactor that activated this object.</value>
        public IObservable<InteractorBase> OnUseStarted => onUseStarted.AsObservable();

        /// <summary>
        /// Observable that fires when the secondary button is released while this interactable is selected.
        /// </summary>
        /// <value>An observable that emits the interactor that deactivated this object.</value>
        public IObservable<InteractorBase> OnUseEnded => onUseEnded.AsObservable();        
        
        /// <summary>
        /// The button that triggers selection of this interactable.
        /// </summary>
        /// <value>The XR button used for selection (Grip or Trigger).</value>
        public XRButton SelectionButton => selectionButton;
        
        /// <summary>
        /// The current interaction state of this interactable.
        /// </summary>
        /// <value>The current interaction state (None, Hovering, Selected).</value>
        public InteractionState CurrentState => currentState;
        
        /// <summary>
        /// The interactor that is currently interacting with this object.
        /// </summary>
        /// <value>The current interactor, or null if no interaction is active.</value>
        public InteractorBase CurrentInteractor => currentInteractor;
        
        /// <summary>
        /// Indicates whether this interactable is currently being used (secondary button pressed).
        /// </summary>
        /// <value>True if the interactable is being used, false otherwise.</value>
        public bool IsUsing => isUsing;
        
        
        public InteractionHand InteractionHand
        {
            get => interactionHand; 
            set => interactionHand = value;
        }

        /// <summary>
        /// Handles state changes for this interactable.
        /// This method manages the transition between different interaction states
        /// and invokes the appropriate events in the interactable.
        /// </summary>
        /// <param name="state">The new interaction state to transition to.</param>
        /// <param name="interactor">The interactor that triggered the state change.</param>
        public void OnStateChanged(InteractionState state, InteractorBase interactor)
        {
            if (this.currentState == state) return;
            currentInteractor = interactor;
            switch (state)
            {
                case InteractionState.None:
                    HandleNoneState();
                    break;
                case InteractionState.Hovering:
                    HandleHoverState();
                    break;
                case InteractionState.Selected:
                    HandleSelectionState();
                    break;
            }
        }

        private void HandleNoneState()
        {
            if (currentState == InteractionState.Selected)
            {
                isUsing = false;
                DeSelected();
                isSelected = false;
                onDeselected.Invoke(currentInteractor);
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
                isUsing = false;
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
                if (Select()) return;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            isSelected = true;
            onSelected.Invoke(currentInteractor);
            currentState = InteractionState.Selected;
        }
        
        /// <summary>
        /// Called when the secondary button is pressed while this interactable is selected.
        /// Override this method to implement custom use behavior.
        /// </summary>
        protected abstract void UseStarted();
        
        /// <summary>
        /// Called when an interactor starts hovering over this interactable.
        /// Override this method to implement custom hover start behavior.
        /// </summary>
        protected abstract void StartHover();
        
        /// <summary>
        /// Called when an interactor stops hovering over this interactable.
        /// Override this method to implement custom hover end behavior.
        /// </summary>
        protected abstract void EndHover();
        
        /// <summary>
        /// Attempts to select this interactable.
        /// </summary>
        /// <returns>True if selection should be cancelled/aborted, false to proceed normally.</returns>
        protected abstract bool Select();
        
        /// <summary>
        /// Called when this interactable is deselected.
        /// Override this method to implement custom deselection behavior.
        /// </summary>
        protected abstract void DeSelected();
        
        /// <summary>
        /// Called when the secondary button is released while this interactable is selected.
        /// Override this method to implement custom use end behavior.
        /// </summary>
        protected virtual void UseEnded() { }

        /// <summary>
        /// Checks if the specified hand is valid for interacting with this object.
        /// </summary>
        /// <param name="hand">The hand identifier to check</param>
        /// <returns>True if the hand can interact with this object, false otherwise</returns>
        public bool CanInteract(HandIdentifier hand)
        {
            var handID = (int)hand;
            var valid = (int)interactionHand;
            var useableHand= (valid & handID ) != 0;
            return useableHand && enabled;
        }

        /// <summary>
        /// Starts the use action for this interactable.
        /// Called when the secondary button is pressed while selected.
        /// </summary>
        /// <param name="interactorBase">The interactor that started using this object</param>
        public void StartUsing(InteractorBase interactorBase)
        {
            if (this.currentState == InteractionState.Selected)
            {
                isUsing = true;
                onUseStarted.Invoke(currentInteractor);
                UseStarted();
            }
        }

        /// <summary>
        /// Stops the use action for this interactable.
        /// Called when the secondary button is released while selected.
        /// </summary>
        /// <param name="interactorBase">The interactor that stopped using this object</param>
        public void StopUsing(InteractorBase interactorBase)
        {
            if (this.currentState == InteractionState.Selected)
            {
                isUsing = false;
                onUseEnded.Invoke(currentInteractor);
                UseEnded();
            }
        }
        
        /// <summary>
        /// Called when component is first added or reset in editor.
        /// Creates the scale compensator and interactable object structure.
        /// </summary>
        protected virtual void Reset()
        {
            ValidateAndCreateHierarchy();
        }
        
        /// <summary>
        /// Called when values change in the inspector.
        /// Ensures the hierarchy is always correct.
        /// </summary>
        protected virtual void OnValidate()
        {
            ValidateAndCreateHierarchy();
        }
        
        /// <summary>
        /// Initializes the scale compensator to prevent shearing during pose editing and manipulation.
        /// Called automatically - override InitializeInteractable() in derived classes for custom initialization.
        /// </summary>
        protected virtual void Awake()
        {
            ValidateAndCreateHierarchy();
            InitializeInteractable();
        }
        
        /// <summary>
        /// Override this method in derived classes for custom initialization logic.
        /// </summary>
        public virtual void InitializeInteractable()
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Validates and creates the proper hierarchy: ScaleCompensator with interactableObject child.
        /// This ensures the structure exists in edit mode and runtime.
        /// Public to allow editor scripts to refresh the hierarchy when needed.
        /// </summary>
        public void ValidateAndCreateHierarchy()
        {
            // Step 1: Validate/create ScaleCompensator
            ValidateScaleCompensator();
            
            // Step 2: Validate/create interactableObject as child of ScaleCompensator
            ValidateInteractableObject();
        }
        
        /// <summary>
        /// Validates that the scale compensator exists and is correctly configured.
        /// Creates it if missing or recreates if incorrectly structured.
        /// </summary>
        protected void ValidateScaleCompensator()
        {
            // Check if we already have a valid reference
            if (_scaleCompensator != null && _scaleCompensator.parent == transform)
            {
                return; // Already valid
            }
            
            // Try to find existing ScaleCompensator
            var existing = transform.Find("ScaleCompensator");
            
            if (existing != null)
            {
                // Found existing - validate it's set up correctly
                if (existing.parent == transform)
                {
                    _scaleCompensator = existing;
                    UpdateScaleCompensatorScale();
                    return;
                }
            }
            
            // Need to create new ScaleCompensator
            CreateScaleCompensator();
        }
        
        /// <summary>
        /// Creates the scale compensator transform.
        /// </summary>
        protected void CreateScaleCompensator()
        {
            _scaleCompensator = new GameObject("ScaleCompensator").transform;
            _scaleCompensator.SetParent(transform, false);
            _scaleCompensator.localPosition = Vector3.zero;
            _scaleCompensator.localRotation = Quaternion.identity;
            
            UpdateScaleCompensatorScale();
        }
        
        /// <summary>
        /// Updates the scale compensator's scale based on parent scale.
        /// </summary>
        protected void UpdateScaleCompensatorScale()
        {
            if (!_scaleCompensator) return;
            
            // Apply scale compensation if parent exists and has non-uniform scale
            if (transform.parent)
            {
                var parentScale = transform.parent.lossyScale;
                
                // Check for zero or near-zero scale to prevent division by zero
                if (Mathf.Approximately(parentScale.x, 0f) || 
                    Mathf.Approximately(parentScale.y, 0f) || 
                    Mathf.Approximately(parentScale.z, 0f))
                {
                    Debug.LogWarning($"Parent of {gameObject.name} has zero scale. Scale compensator created but not compensating.", this);
                    _scaleCompensator.localScale = Vector3.one;
                    return;
                }
                
                // Set inverse scale to compensate for parent's scale
                _scaleCompensator.localScale = new Vector3(
                    1f / parentScale.x,
                    1f / parentScale.y,
                    1f / parentScale.z
                );
            }
            else
            {
                // No parent, no need to compensate
                _scaleCompensator.localScale = Vector3.one;
            }
        }
        
        /// <summary>
        /// Validates that interactableObject exists as a child of ScaleCompensator.
        /// Creates it if missing. Override this for custom interactableObject setup.
        /// </summary>
        protected virtual void ValidateInteractableObject()
        {
            if (!_scaleCompensator) return;
            
            // Check if interactableObject exists and is in the right place
            var existing = _scaleCompensator.Find("interactableObject");
            
            if (existing != null)
            {
                return; // Already exists in correct location
            }
            
            // Check if it exists elsewhere (wrong location)
            var wrongLocation = transform.Find("interactableObject");
            if (wrongLocation != null)
            {
                // Move it to correct location
                Debug.Log($"Moving interactableObject into ScaleCompensator for {gameObject.name}", this);
                wrongLocation.SetParent(_scaleCompensator, true);
                return;
            }
            
            // Create new interactableObject
            CreateInteractableObject();
        }
        
        /// <summary>
        /// Creates the interactableObject as a child of ScaleCompensator.
        /// Override this in derived classes for custom setup.
        /// </summary>
        protected virtual void CreateInteractableObject()
        {
            var interactableObject = new GameObject("interactableObject").transform;
            interactableObject.SetParent(_scaleCompensator, false);
            interactableObject.localPosition = Vector3.zero;
            interactableObject.localRotation = Quaternion.identity;
            interactableObject.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Cleans up the scale compensator when the object is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_scaleCompensator)
            {
                // Only destroy if we're not in the middle of a scene unload
                if (_scaleCompensator.gameObject)
                {
                    DestroyImmediate(_scaleCompensator.gameObject);
                }
            }
        }
    }
}
using UniRx;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds interactable states (Hovered, Selected, Used) to BoolVariables.
    /// </summary>
    /// <remarks>
    /// Allows decoupled state tracking of interactables through the scriptable variable system.
    /// Each state can be bound to a different BoolVariable.
    ///
    /// Common use cases include:
    /// - UI indicators showing interaction state
    /// - Enabling/disabling features based on selection
    /// - Triggering effects when objects are hovered
    /// - Analytics tracking of interactions
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Interactable To Bool Binder")]
    [RequireComponent(typeof(InteractableBase))]
    public class InteractableToBoolBinder : MonoBehaviour
    {
        [Header("Hover State")]
        [Tooltip("BoolVariable set to true when hovered, false when not.")]
        [SerializeField] private BoolVariable hoveredVariable;

        [Header("Selected State")]
        [Tooltip("BoolVariable set to true when selected (grabbed), false when deselected.")]
        [SerializeField] private BoolVariable selectedVariable;

        [Header("Use State")]
        [Tooltip("BoolVariable set to true when use starts, false when use ends.")]
        [SerializeField] private BoolVariable usedVariable;

        [Header("Options")]
        [Tooltip("Reset all variables to false on disable.")]
        [SerializeField] private bool resetOnDisable = true;

        private InteractableBase _interactable;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _interactable = GetComponent<InteractableBase>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            // Hover events
            if (hoveredVariable != null)
            {
                _interactable.OnHoverStarted
                    .Subscribe(_ => hoveredVariable.Value = true)
                    .AddTo(_disposable);

                _interactable.OnHoverEnded
                    .Subscribe(_ => hoveredVariable.Value = false)
                    .AddTo(_disposable);
            }

            // Selection events
            if (selectedVariable != null)
            {
                _interactable.OnSelected
                    .Subscribe(_ => selectedVariable.Value = true)
                    .AddTo(_disposable);

                _interactable.OnDeselected
                    .Subscribe(_ => selectedVariable.Value = false)
                    .AddTo(_disposable);
            }

            // Use events
            if (usedVariable != null)
            {
                _interactable.OnUseStarted
                    .Subscribe(_ => usedVariable.Value = true)
                    .AddTo(_disposable);

                _interactable.OnUseEnded
                    .Subscribe(_ => usedVariable.Value = false)
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();

            if (resetOnDisable)
            {
                if (hoveredVariable != null) hoveredVariable.Value = false;
                if (selectedVariable != null) selectedVariable.Value = false;
                if (usedVariable != null) usedVariable.Value = false;
            }
        }

        /// <summary>
        /// Gets whether the interactable is currently hovered.
        /// </summary>
        public bool IsHovered => hoveredVariable != null && hoveredVariable.Value;

        /// <summary>
        /// Gets whether the interactable is currently selected.
        /// </summary>
        public bool IsSelected => selectedVariable != null && selectedVariable.Value;

        /// <summary>
        /// Gets whether the interactable is currently being used.
        /// </summary>
        public bool IsUsed => usedVariable != null && usedVariable.Value;
    }
}

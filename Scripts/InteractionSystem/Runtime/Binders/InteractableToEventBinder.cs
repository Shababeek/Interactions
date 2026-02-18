using UniRx;
using UnityEngine;
using Shababeek.Interactions;
using Shababeek.ReactiveVars;

namespace Shababeek.Interactions.Binders
{
    /// <summary>
    /// Binds interactable events to GameEvents.
    /// </summary>
    /// <remarks>
    /// Fires scriptable GameEvents when interactable state changes.
    /// Allows decoupled event handling through the scriptable event system.
    ///
    /// Common use cases include:
    /// - Triggering game logic when objects are grabbed
    /// - Playing effects through event listeners
    /// - Updating UI/analytics when interactions occur
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Interactable To Event Binder")]
    [RequireComponent(typeof(InteractableBase))]
    public class InteractableToEventBinder : MonoBehaviour
    {
        [Header("Hover Events")]
        [Tooltip("GameEvent raised when hover starts.")]
        [SerializeField] private GameEvent onHoverStartEvent;

        [Tooltip("GameEvent raised when hover ends.")]
        [SerializeField] private GameEvent onHoverEndEvent;

        [Header("Selection Events")]
        [Tooltip("GameEvent raised when selected (grabbed).")]
        [SerializeField] private GameEvent onSelectedEvent;

        [Tooltip("GameEvent raised when deselected (released).")]
        [SerializeField] private GameEvent onDeselectedEvent;

        [Header("Use Events")]
        [Tooltip("GameEvent raised when use starts.")]
        [SerializeField] private GameEvent onUseStartEvent;

        [Tooltip("GameEvent raised when use ends.")]
        [SerializeField] private GameEvent onUseEndEvent;

        [Header("Thumb Events")]
        [Tooltip("GameEvent raised when thumb button (A/B) is pressed while selected.")]
        [SerializeField] private GameEvent onThumbPressedEvent;

        [Tooltip("GameEvent raised when thumb button (A/B) is released while selected.")]
        [SerializeField] private GameEvent onThumbReleasedEvent;

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
            if (onHoverStartEvent != null)
            {
                _interactable.OnHoverStarted
                    .Subscribe(_ => onHoverStartEvent.Raise())
                    .AddTo(_disposable);
            }

            if (onHoverEndEvent != null)
            {
                _interactable.OnHoverEnded
                    .Subscribe(_ => onHoverEndEvent.Raise())
                    .AddTo(_disposable);
            }

            // Selection events
            if (onSelectedEvent != null)
            {
                _interactable.OnSelected
                    .Subscribe(_ => onSelectedEvent.Raise())
                    .AddTo(_disposable);
            }

            if (onDeselectedEvent != null)
            {
                _interactable.OnDeselected
                    .Subscribe(_ => onDeselectedEvent.Raise())
                    .AddTo(_disposable);
            }

            // Use events
            if (onUseStartEvent != null)
            {
                _interactable.OnUseStarted
                    .Subscribe(_ => onUseStartEvent.Raise())
                    .AddTo(_disposable);
            }

            if (onUseEndEvent != null)
            {
                _interactable.OnUseEnded
                    .Subscribe(_ => onUseEndEvent.Raise())
                    .AddTo(_disposable);
            }

            // Thumb events
            if (onThumbPressedEvent != null)
            {
                _interactable.OnThumbPressed
                    .Subscribe(_ => onThumbPressedEvent.Raise())
                    .AddTo(_disposable);
            }

            if (onThumbReleasedEvent != null)
            {
                _interactable.OnThumbReleased
                    .Subscribe(_ => onThumbReleasedEvent.Raise())
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }
    }
}

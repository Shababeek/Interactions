using System;
using UniRx;
using UnityEngine;
using Shababeek.Interactions;
using Shababeek.ReactiveVars;

namespace Shababeek.Interactions.Drivers
{
    /// <summary>Raises GameEvents in response to interactable events.</summary>
    /// <remarks>
    /// Fires scriptable GameEvents when interactable state changes.
    /// Allows decoupled event handling through the scriptable event system.
    ///
    /// Common use cases include:
    /// - Triggering game logic when objects are grabbed
    /// - Playing effects through event listeners
    /// - Updating UI/analytics when interactions occur
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Interactable To Event Driver")]
    [RequireComponent(typeof(InteractableBase))]
    public class InteractableToEventDriver : MonoBehaviour
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

            Raise(_interactable.OnHoverStarted,   onHoverStartEvent);
            Raise(_interactable.OnHoverEnded,     onHoverEndEvent);
            Raise(_interactable.OnSelected,       onSelectedEvent);
            Raise(_interactable.OnDeselected,     onDeselectedEvent);
            Raise(_interactable.OnUseStarted,     onUseStartEvent);
            Raise(_interactable.OnUseEnded,       onUseEndEvent);
            Raise(_interactable.OnThumbPressed,   onThumbPressedEvent);
            Raise(_interactable.OnThumbReleased,  onThumbReleasedEvent);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void Raise<T>(IObservable<T> source, GameEvent evt)
        {
            if (evt == null) return;
            source.Subscribe(_ => evt.Raise()).AddTo(_disposable);
        }
    }
}

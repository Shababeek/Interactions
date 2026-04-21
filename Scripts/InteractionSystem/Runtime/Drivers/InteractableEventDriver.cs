using System;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Writes interactable interaction events into GameEvents and variables.</summary>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Interactable Event Driver")]
    public class InteractableEventDriver : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("Source interactable.")]
        [SerializeField] private InteractableBase interactable;

        [Header("Selection Events")]
        [Tooltip("GameEvent raised when the interactable is selected.")]
        [SerializeField] private GameEvent onSelectedEvent;
        [Tooltip("GameEvent raised when the interactable is deselected.")]
        [SerializeField] private GameEvent onDeselectedEvent;

        [Header("Hover Events")]
        [Tooltip("GameEvent raised when hover starts.")]
        [SerializeField] private GameEvent onHoverStartEvent;
        [Tooltip("GameEvent raised when hover ends.")]
        [SerializeField] private GameEvent onHoverEndEvent;

        [Header("Use Events")]
        [Tooltip("GameEvent raised when use starts.")]
        [SerializeField] private GameEvent onUseStartEvent;
        [Tooltip("GameEvent raised when use ends.")]
        [SerializeField] private GameEvent onUseEndEvent;

        [Header("State Variables")]
        [Tooltip("Bool variable tracking if the interactable is selected.")]
        [SerializeField] private BoolVariable isSelectedVariable;
        [Tooltip("Bool variable tracking if the interactable is hovered.")]
        [SerializeField] private BoolVariable isHoveredVariable;
        [Tooltip("Bool variable tracking if the interactable is being used.")]
        [SerializeField] private BoolVariable isUsingVariable;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (interactable == null) interactable = GetComponent<InteractableBase>();
            if (interactable == null) return;

            _disposable = new CompositeDisposable();

            Bind(interactable.OnSelected,      onSelectedEvent,    isSelectedVariable, true);
            Bind(interactable.OnDeselected,    onDeselectedEvent,  isSelectedVariable, false);
            Bind(interactable.OnHoverStarted,  onHoverStartEvent,  isHoveredVariable,  true);
            Bind(interactable.OnHoverEnded,    onHoverEndEvent,    isHoveredVariable,  false);
            Bind(interactable.OnUseStarted,    onUseStartEvent,    isUsingVariable,    true);
            Bind(interactable.OnUseEnded,      onUseEndEvent,      isUsingVariable,    false);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void Bind<T>(IObservable<T> source, GameEvent evt, BoolVariable state, bool value)
        {
            source.Subscribe(_ =>
            {
                evt?.Raise();
                if (state != null) state.Value = value;
            }).AddTo(_disposable);
        }
    }
}

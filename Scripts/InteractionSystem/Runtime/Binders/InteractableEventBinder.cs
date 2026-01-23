using Shababeek.Utilities;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds interactable interaction events to GameEvents and variables.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Interactable Event Binder")]
    public class InteractableEventBinder : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private InteractableBase interactable;

        [Header("Selection Events")]
        [SerializeField] private GameEvent onSelectedEvent;
        [SerializeField] private GameEvent onDeselectedEvent;

        [Header("Hover Events")]
        [SerializeField] private GameEvent onHoverStartEvent;
        [SerializeField] private GameEvent onHoverEndEvent;

        [Header("Use Events")]
        [SerializeField] private GameEvent onUseStartEvent;
        [SerializeField] private GameEvent onUseEndEvent;

        [Header("State Variables")]
        [SerializeField] private BoolVariable isSelectedVariable;
        [SerializeField] private BoolVariable isHoveredVariable;
        [SerializeField] private BoolVariable isUsingVariable;

        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (interactable == null) interactable = GetComponent<InteractableBase>();
            if (interactable == null) return;

            _disposable = new CompositeDisposable();

            // Selection
            interactable.OnSelected
                .Subscribe(_ =>
                {
                    onSelectedEvent?.Raise();
                    if (isSelectedVariable != null) isSelectedVariable.Value = true;
                })
                .AddTo(_disposable);

            interactable.OnDeselected
                .Subscribe(_ =>
                {
                    onDeselectedEvent?.Raise();
                    if (isSelectedVariable != null) isSelectedVariable.Value = false;
                })
                .AddTo(_disposable);

            // Hover
            interactable.OnHoverStarted
                .Subscribe(_ =>
                {
                    onHoverStartEvent?.Raise();
                    if (isHoveredVariable != null) isHoveredVariable.Value = true;
                })
                .AddTo(_disposable);

            interactable.OnHoverEnded
                .Subscribe(_ =>
                {
                    onHoverEndEvent?.Raise();
                    if (isHoveredVariable != null) isHoveredVariable.Value = false;
                })
                .AddTo(_disposable);

            // Use
            interactable.OnUseStarted
                .Subscribe(_ =>
                {
                    onUseStartEvent?.Raise();
                    if (isUsingVariable != null) isUsingVariable.Value = true;
                })
                .AddTo(_disposable);

            interactable.OnUseEnded
                .Subscribe(_ =>
                {
                    onUseEndEvent?.Raise();
                    if (isUsingVariable != null) isUsingVariable.Value = false;
                })
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();
    }
}

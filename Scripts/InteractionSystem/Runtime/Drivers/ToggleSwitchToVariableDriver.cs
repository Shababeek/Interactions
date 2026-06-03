using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Writes a ToggleSwitchInteractable's step output to scriptable variables and events.</summary>
    [AddComponentMenu("Shababeek/Interactions/Drivers/Toggle Switch To Variable Driver")]
    [RequireComponent(typeof(ToggleSwitchInteractable))]
    public class ToggleSwitchToVariableDriver : MonoBehaviour
    {
        [Header("Variable Outputs")]
        [Tooltip("IntVariable that receives the current step index (0-based).")]
        [SerializeField] private NumericalVariable<int> stepVariable;

        [Header("Event Outputs")]
        [Tooltip("GameEvent raised when the step changes during interaction.")]
        [SerializeField] private GameEvent onStepChangedEvent;

        [Tooltip("GameEvent raised when a step is committed (snap complete).")]
        [SerializeField] private GameEvent onStepConfirmedEvent;

        [Header("Per-Step Events")]
        [Tooltip("Optional GameEvents, one per step. Array index matches the step number.")]
        [SerializeField] private GameEvent[] stepEvents;

        private ToggleSwitchInteractable _toggle;
        private CompositeDisposable _disposable;

        private void Awake() => _toggle = GetComponent<ToggleSwitchInteractable>();

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            _toggle.OnStepChanged
                .Subscribe(OnStepChangedHandler)
                .AddTo(_disposable);

            _toggle.OnStepConfirmed
                .Subscribe(OnStepConfirmedHandler)
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnStepChangedHandler(int step)
        {
            if (stepVariable) stepVariable.Value = step;
            onStepChangedEvent?.Raise();
        }

        private void OnStepConfirmedHandler(int step)
        {
            onStepConfirmedEvent?.Raise();

            if (stepEvents != null && step >= 0 && step < stepEvents.Length && stepEvents[step] != null)
            {
                stepEvents[step].Raise();
            }
        }
    }
}

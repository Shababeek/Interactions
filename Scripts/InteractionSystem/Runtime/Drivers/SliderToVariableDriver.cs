using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    [AddComponentMenu("Shababeek/Interactions/Drivers/Slider To Variable Driver")]
    [RequireComponent(typeof(SliderInteractable))]
    public class SliderToVariableDriver : MonoBehaviour
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

        private SliderInteractable _slider;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _slider = GetComponent<SliderInteractable>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            _slider.OnStepChanged
                .Subscribe(OnStepChangedHandler)
                .AddTo(_disposable);

            _slider.OnStepConfirmed
                .Subscribe(OnStepConfirmedHandler)
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnStepChangedHandler(int step)
        {
            if (stepVariable != null) stepVariable.Value = step;
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

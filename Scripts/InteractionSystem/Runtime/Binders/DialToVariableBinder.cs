using UniRx;
using UnityEngine;
using Shababeek.ReactiveVars;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds a DialInteractable's step output to scriptable variables and events.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Dial To Variable Binder")]
    [RequireComponent(typeof(DialInteractable))]
    public class DialToVariableBinder : MonoBehaviour
    {
        [Header("Variable Outputs")]
        [Tooltip("IntVariable that receives the current step index (0-based).")]
        [SerializeField] private IntVariable stepVariable;

        [Tooltip("FloatVariable that receives the normalized step value (0-1).")]
        [SerializeField] private FloatVariable normalizedVariable;

        [Header("Event Outputs")]
        [Tooltip("GameEvent raised when the step changes during interaction.")]
        [SerializeField] private GameEvent onStepChangedEvent;

        [Tooltip("GameEvent raised when a step is committed (snap complete).")]
        [SerializeField] private GameEvent onStepConfirmedEvent;

        [Header("Per-Step Events")]
        [Tooltip("Optional GameEvents, one per step. Array index matches the step number.")]
        [SerializeField] private GameEvent[] stepEvents;

        private DialInteractable _dial;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _dial = GetComponent<DialInteractable>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            WriteStepVariables(_dial.CurrentStep);

            _dial.OnStepChanged
                .Subscribe(OnStepChanged)
                .AddTo(_disposable);

            _dial.OnStepConfirmed
                .Subscribe(OnStepConfirmed)
                .AddTo(_disposable);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnStepChanged(int step)
        {
            WriteStepVariables(step);
            onStepChangedEvent?.Raise();
        }

        private void OnStepConfirmed(int step)
        {
            onStepConfirmedEvent?.Raise();

            if (stepEvents != null && step >= 0 && step < stepEvents.Length && stepEvents[step] != null)
            {
                stepEvents[step].Raise();
            }
        }

        private void WriteStepVariables(int step)
        {
            if (stepVariable != null) stepVariable.Value = step;
            if (normalizedVariable != null) normalizedVariable.Value = _dial.NormalizedValue;
        }

        /// <summary>Forces the dial to match the current value of the bound step variable.</summary>
        public void SyncDialToVariable()
        {
            if (stepVariable != null) _dial.SetStep(stepVariable.Value);
        }
    }
}

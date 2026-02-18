using UniRx;
using UnityEngine;
using Shababeek.ReactiveVars;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Binds a DialInteractable to Scriptable Variables and Events.
    /// Outputs the current step as IntVariable and normalized value as FloatVariable.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Binders/Dial To Variable Binder")]
    [RequireComponent(typeof(DialInteractable))]
    public class DialToVariableBinder : MonoBehaviour
    {
        [Header("Variable Outputs")]
        [Tooltip("IntVariable to receive the current step index (0-based).")]
        [SerializeField] private IntVariable stepVariable;

        [Tooltip("FloatVariable to receive the normalized value (0-1).")]
        [SerializeField] private FloatVariable normalizedVariable;

        [Tooltip("FloatVariable to receive the current angle.")]
        [SerializeField] private FloatVariable angleVariable;

        [Header("Event Outputs")]
        [Tooltip("GameEvent raised when the step changes.")]
        [SerializeField] private GameEvent onStepChangedEvent;

        [Tooltip("GameEvent raised when a step is confirmed (on release).")]
        [SerializeField] private GameEvent onStepConfirmedEvent;

        [Header("Per-Step Events")]
        [Tooltip("Optional: Specific GameEvents for each step. Array index = step number.")]
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

            // Initial values
            UpdateVariables(_dial.CurrentStep, _dial.CurrentAngle);

            // Subscribe to step changes
            _dial.OnStepChanged
                .Subscribe(OnStepChanged)
                .AddTo(_disposable);

            // Subscribe to angle changes
            _dial.OnAngleChanged
                .Subscribe(OnAngleChanged)
                .AddTo(_disposable);

            // Subscribe to step confirmed
            _dial.OnStepConfirmed
                .Subscribe(OnStepConfirmed)
                .AddTo(_disposable);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void OnStepChanged(int step)
        {
            if (stepVariable != null)
            {
                stepVariable.Value = step;
            }

            if (normalizedVariable != null)
            {
                normalizedVariable.Value = _dial.NormalizedValue;
            }

            onStepChangedEvent?.Raise();

            // Fire per-step event if configured
            if (stepEvents != null && step >= 0 && step < stepEvents.Length && stepEvents[step] != null)
            {
                stepEvents[step].Raise();
            }
        }

        private void OnAngleChanged(float angle)
        {
            if (angleVariable != null)
            {
                angleVariable.Value = angle;
            }
        }

        private void OnStepConfirmed(int step)
        {
            onStepConfirmedEvent?.Raise();
        }

        private void UpdateVariables(int step, float angle)
        {
            if (stepVariable != null)
            {
                stepVariable.Value = step;
            }

            if (normalizedVariable != null)
            {
                normalizedVariable.Value = _dial.NormalizedValue;
            }

            if (angleVariable != null)
            {
                angleVariable.Value = angle;
            }
        }

        /// <summary>
        /// Sets the dial to match the current step variable value.
        /// </summary>
        public void SyncDialToVariable()
        {
            if (stepVariable != null)
            {
                _dial.SetStep(stepVariable.Value);
            }
        }
    }
}

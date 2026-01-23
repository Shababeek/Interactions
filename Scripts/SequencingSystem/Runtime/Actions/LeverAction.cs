using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    public enum LeverCondition
    {
        /// <summary>Complete when lever reaches or exceeds target value.</summary>
        ReachTarget,
        /// <summary>Complete when lever is held at target for duration.</summary>
        HoldAtTarget,
        /// <summary>Complete when lever passes through target value.</summary>
        PassThrough,
    }

    /// <summary>
    /// Completes a step based on lever position.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/LeverAction")]
    public class LeverAction : AbstractSequenceAction
    {
        [Tooltip("The lever to monitor.")]
        [SerializeField] private LeverInteractable lever;

        [Tooltip("Condition for step completion.")]
        [SerializeField] private LeverCondition condition = LeverCondition.ReachTarget;

        [Tooltip("Target normalized value (0-1).")]
        [Range(0f, 1f)]
        [SerializeField] private float targetValue = 1f;

        [Tooltip("How close to target counts as reached.")]
        [SerializeField] private float tolerance = 0.05f;

        [Tooltip("Duration to hold at target (HoldAtTarget only).")]
        [SerializeField] private float holdDuration = 1f;

        private float _holdTime;
        private bool _wasAtTarget;
        private float _lastValue;

        private void Subscribe()
        {
            if (lever == null) return;

            lever.OnLeverChanged
                .Do(OnValueChanged)
                .Subscribe()
                .AddTo(StepDisposable);

            _lastValue = lever.CurrentNormalizedAngle;
            _holdTime = 0f;
            _wasAtTarget = false;
        }

        private void OnValueChanged(float normalizedValue)
        {
            bool atTarget = Mathf.Abs(normalizedValue - targetValue) <= tolerance;

            switch (condition)
            {
                case LeverCondition.ReachTarget:
                    if (atTarget) CompleteStep();
                    break;

                case LeverCondition.PassThrough:
                    // Check if we crossed the target
                    bool crossed = (_lastValue < targetValue && normalizedValue >= targetValue) ||
                                   (_lastValue > targetValue && normalizedValue <= targetValue);
                    if (crossed) CompleteStep();
                    break;

                case LeverCondition.HoldAtTarget:
                    // Handled in Update
                    break;
            }

            _wasAtTarget = atTarget;
            _lastValue = normalizedValue;
        }

        private void Update()
        {
            if (!Started || condition != LeverCondition.HoldAtTarget || lever == null) return;

            bool atTarget = Mathf.Abs(lever.CurrentNormalizedAngle - targetValue) <= tolerance;

            if (atTarget)
            {
                _holdTime += Time.deltaTime;
                if (_holdTime >= holdDuration)
                {
                    CompleteStep();
                }
            }
            else
            {
                _holdTime = 0f;
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                _holdTime = 0f;
                _wasAtTarget = false;
                Subscribe();
            }
        }

        public float HoldProgress => holdDuration > 0 ? Mathf.Clamp01(_holdTime / holdDuration) : 0f;
    }
}

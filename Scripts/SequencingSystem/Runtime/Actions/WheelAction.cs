using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    public enum WheelCondition
    {
        /// <summary>Complete when wheel reaches target rotation.</summary>
        ReachTarget,
        /// <summary>Complete when wheel is held at target for duration.</summary>
        HoldAtTarget,
        /// <summary>Complete when wheel completes N full rotations.</summary>
        CompleteRotations,
    }

    /// <summary>
    /// Completes a step based on wheel rotation.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/WheelAction")]
    public class WheelAction : AbstractSequenceAction
    {
        [Tooltip("The wheel to monitor.")]
        [SerializeField] private WheelInteractable wheel;

        [Tooltip("Condition for step completion.")]
        [SerializeField] private WheelCondition condition = WheelCondition.ReachTarget;

        [Tooltip("Target normalized value (-1 to 1).")]
        [Range(-1f, 1f)]
        [SerializeField] private float targetValue = 1f;

        [Tooltip("How close to target counts as reached.")]
        [SerializeField] private float tolerance = 0.05f;

        [Tooltip("Duration to hold at target (HoldAtTarget only).")]
        [SerializeField] private float holdDuration = 1f;

        [Tooltip("Number of full rotations to complete (CompleteRotations only).")]
        [SerializeField] private int targetRotations = 1;

        [Tooltip("Direction for rotations (CompleteRotations only).")]
        [SerializeField] private bool clockwise = true;

        private float _holdTime;
        private float _startRotations;

        private void Subscribe()
        {
            if (wheel == null) return;

            wheel.OnNormalizedChanged
                .Do(OnValueChanged)
                .Subscribe()
                .AddTo(StepDisposable);

            _holdTime = 0f;
            _startRotations = wheel.CurrentAngle / 360f;
        }

        private void OnValueChanged(float normalizedValue)
        {
            switch (condition)
            {
                case WheelCondition.ReachTarget:
                    if (Mathf.Abs(normalizedValue - targetValue) <= tolerance)
                    {
                        CompleteStep();
                    }
                    break;

                case WheelCondition.CompleteRotations:
                    float currentRotations = wheel.CurrentAngle / 360f;
                    float delta = currentRotations - _startRotations;

                    if (clockwise && delta >= targetRotations)
                    {
                        CompleteStep();
                    }
                    else if (!clockwise && delta <= -targetRotations)
                    {
                        CompleteStep();
                    }
                    break;

                case WheelCondition.HoldAtTarget:
                    // Handled in Update
                    break;
            }
        }

        private void Update()
        {
            if (!Started || condition != WheelCondition.HoldAtTarget || wheel == null) return;

            bool atTarget = Mathf.Abs(wheel.NormalizedValue - targetValue) <= tolerance;

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
                Subscribe();
            }
        }

        public float HoldProgress => holdDuration > 0 ? Mathf.Clamp01(_holdTime / holdDuration) : 0f;
    }
}

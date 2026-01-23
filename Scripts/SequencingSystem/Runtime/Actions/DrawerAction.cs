using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    public enum DrawerCondition
    {
        /// <summary>Complete when drawer is fully opened.</summary>
        Opened,
        /// <summary>Complete when drawer is fully closed.</summary>
        Closed,
        /// <summary>Complete when drawer reaches target position.</summary>
        ReachTarget,
        /// <summary>Complete when drawer is held at target for duration.</summary>
        HoldAtTarget,
    }

    /// <summary>
    /// Completes a step based on drawer/slider position.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/DrawerAction")]
    public class DrawerAction : AbstractSequenceAction
    {
        [Tooltip("The drawer to monitor.")]
        [SerializeField] private DrawerInteractable drawer;

        [Tooltip("Condition for step completion.")]
        [SerializeField] private DrawerCondition condition = DrawerCondition.Opened;

        [Tooltip("Target normalized value (0-1). Used with ReachTarget/HoldAtTarget.")]
        [Range(0f, 1f)]
        [SerializeField] private float targetValue = 1f;

        [Tooltip("How close to target counts as reached.")]
        [SerializeField] private float tolerance = 0.05f;

        [Tooltip("Duration to hold at target (HoldAtTarget only).")]
        [SerializeField] private float holdDuration = 1f;

        private float _holdTime;
        private float _currentValue;

        private void Subscribe()
        {
            if (drawer == null) return;

            drawer.OnMoved
                .Do(OnValueChanged)
                .Subscribe()
                .AddTo(StepDisposable);

            // Also subscribe to open/close events
            drawer.OnOpened
                .Do(_ => OnOpened())
                .Subscribe()
                .AddTo(StepDisposable);

            drawer.OnClosed
                .Do(_ => OnClosed())
                .Subscribe()
                .AddTo(StepDisposable);

            _holdTime = 0f;
        }

        private void OnValueChanged(float normalizedValue)
        {
            _currentValue = normalizedValue;

            if (condition == DrawerCondition.ReachTarget)
            {
                if (Mathf.Abs(normalizedValue - targetValue) <= tolerance)
                {
                    CompleteStep();
                }
            }
        }

        private void OnOpened()
        {
            if (condition == DrawerCondition.Opened)
            {
                CompleteStep();
            }
        }

        private void OnClosed()
        {
            if (condition == DrawerCondition.Closed)
            {
                CompleteStep();
            }
        }

        private void Update()
        {
            if (!Started || condition != DrawerCondition.HoldAtTarget || drawer == null) return;

            bool atTarget = Mathf.Abs(_currentValue - targetValue) <= tolerance;

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
                _currentValue = 0f;
                Subscribe();
            }
        }

        public float HoldProgress => holdDuration > 0 ? Mathf.Clamp01(_holdTime / holdDuration) : 0f;
    }
}

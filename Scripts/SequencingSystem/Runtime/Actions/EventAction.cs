using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// A simple action that invokes Unity events on step start and can be completed externally.
    /// Useful for connecting sequences to custom game logic without writing code.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/EventAction")]
    public class EventAction : AbstractSequenceAction
    {
        [Tooltip("Event raised when the step starts.")]
        [SerializeField] private UnityEvent onStepStarted;

        [Tooltip("Event raised when the step is completed.")]
        [SerializeField] private UnityEvent onStepCompleted;

        [Tooltip("If true, the step completes automatically after invoking onStepStarted.")]
        [SerializeField] private bool autoComplete = false;

        [Tooltip("Delay before auto-completing (only used when autoComplete is true).")]
        [SerializeField] private float autoCompleteDelay = 0f;

        private float _elapsedTime = 0f;
        private bool _waitingForAutoComplete = false;

        /// <summary>
        /// Completes this action's step. Call this from external scripts or UnityEvents.
        /// </summary>
        private void Complete()
        {
            if (Started)
            {
                onStepCompleted?.Invoke();
                CompleteStep();
            }
        }

        private void Update()
        {
            if (!_waitingForAutoComplete) return;

            _elapsedTime += Time.deltaTime;
            if (_elapsedTime >= autoCompleteDelay)
            {
                _waitingForAutoComplete = false;
                Complete();
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                onStepStarted?.Invoke();

                if (autoComplete)
                {
                    if (autoCompleteDelay <= 0f)
                    {
                        Complete();
                    }
                    else
                    {
                        _elapsedTime = 0f;
                        _waitingForAutoComplete = true;
                    }
                }
            }
            else
            {
                _waitingForAutoComplete = false;
            }
        }
    }
}

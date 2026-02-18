using Shababeek.ReactiveVars;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Waits for a specified duration before completing the step.
    /// </summary>
    [AddComponentMenu(menuName : "Shababeek/Sequencing/Actions/TimerAction")]
    public class TimerAction : AbstractSequenceAction
    {
        [Tooltip("Event raised when the timer completes.")]
        [SerializeField] private UnityEvent onComplete;
        
        [Tooltip("Start the timer automatically when this component is enabled.")]
        [SerializeField] private bool startOnEnable;
        
        [Tooltip("Duration of the timer in seconds.")]
        [SerializeField] private float time;
        
        [Tooltip("Elapsed time in seconds.")]
        [SerializeField][ReadOnly] private float elapsed = 0;
        
        [Tooltip("Whether the timer is currently active.")]
        [ReadOnly][SerializeField] private bool active;

        private void OnEnable()
        {
            if (startOnEnable) StartTimer();
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if(status== SequenceStatus.Started)StartTimer();
        }

        /// <summary>
        /// Starts the timer countdown.
        /// </summary>
        public void StartTimer()
        {
            elapsed = 0;
            active = true;
        }

        private void Update()
        {
            if (!active) return;
            elapsed += Time.deltaTime;
            if (elapsed >= time)
            {
                active = false;
                onComplete.Invoke();
                CompleteStep();
            }
        }
    }
}
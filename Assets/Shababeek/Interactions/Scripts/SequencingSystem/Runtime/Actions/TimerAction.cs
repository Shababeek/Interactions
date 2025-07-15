using Shababeek.
    Core;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    [AddComponentMenu(menuName : "Shababeek/Sequencing/Actions/TimerAction")]
    public class TimerAction : AbstractSequenceAction
    {
        [SerializeField] private UnityEvent onComplete;
        [SerializeField] private bool startOnEnable;
        [SerializeField] private float time;
        [SerializeField][ReadOnly] private float elapsed = 0;
        [ReadOnly][SerializeField] private bool active;

        private void OnEnable()
        {
            if (startOnEnable) StartTimer();
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if(status== SequenceStatus.Started)StartTimer();
        }

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

            }
        }
    }
}
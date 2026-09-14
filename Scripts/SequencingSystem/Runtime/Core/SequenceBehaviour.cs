using System;
using Shababeek.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Manages the execution of sequences in the sequencing system.
    /// Handles sequence lifecycle and timing.
    /// </summary>
    public class SequenceBehaviour : MonoBehaviour
    {
        [Tooltip("The sequence to be executed by this behaviour.")]
        [SerializeField] public Sequence sequence;

        [Tooltip("Start the sequence when this component is enabled.")]
        [SerializeField] private bool starOnAwake = false;

        [Tooltip("Seconds to wait after enabling before the first step begins.")]
        [SerializeField] private float delay = 1;

        [Tooltip("Allow Space to start the sequence and to force-complete the current step. Editor/desktop testing aid.")]
        [SerializeField] private bool startOnSpace;

        [ReadOnly] [SerializeField] private bool started;

        [SerializeField] private UnityEvent onSequenceStarted;
        [SerializeField] private UnityEvent onSequenceCompleted;

        public bool StarOnAwake => starOnAwake;

        private void OnEnable()
        {
            sequence.OnRaisedData
                .Where(status => status == SequenceStatus.Started)
                .Do(_ => onSequenceStarted.Invoke())
                .Subscribe().AddTo(this);

            sequence.OnRaisedData
                .Where(status => status == SequenceStatus.Completed)
                .Do(_ => onSequenceCompleted.Invoke())
                .Subscribe().AddTo(this);

            if (starOnAwake) StartQuest();
        }

        /// <summary>
        /// Starts the sequence after the configured delay.
        /// </summary>
        public async void StartQuest()
        {
            await Awaitable.NextFrameAsync();
            if (delay > 0) await Awaitable.WaitForSecondsAsync(delay);
            if (!this) return;
            sequence.Begin();
            started = true;
        }

        private void Update()
        {
            if (!startOnSpace) return;
            if (!started && Input.GetKeyDown(KeyCode.Space)) StartQuest();
            else if (started && Input.GetKeyDown(KeyCode.Space)) sequence.CurrentStep.CompleteStep();
        }

        [Serializable]
        public class StepEventPair
        {
            public UnityEvent listeners;
            public Step step;

            public StepEventPair(Step step)
            {
                this.step = step;
            }
        }
    }
}

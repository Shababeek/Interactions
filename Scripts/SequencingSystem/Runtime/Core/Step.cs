using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Represents a step in a sequence of actions, which can be started and completed.
    /// This class handles audio playback, step completion, and events for starting and completing the step.
    /// </summary>
    /// <remarks>
    /// this ScriptableObject can only be created in a sequence, it should not be created manually.
    /// </remarks>
    [Serializable]
    public class Step : SequenceNode
    {
        [Tooltip("The audio clip to play when this step starts.")]
        [SerializeField] private AudioClip audioClip;
        
        [Tooltip("Enable to allow the step to be completed before it starts.")]
        [SerializeField] private bool canBeFinishedBeforeStarted;
        
        [Tooltip("When enabled, the step automatically completes when the audio finishes playing.")]
        [SerializeField] private bool audioOnly;
        
        [Tooltip("Delay in seconds before starting the audio playback.")]
        [SerializeField] private float audioDelay = .1f;
        
        [Tooltip("Unity event raised when the step starts.")]
        [SerializeField] private UnityEvent onStarted;
        
        [Tooltip("Unity event raised when the step completes.")]
        [SerializeField] private UnityEvent onCompleted;
        
        [Tooltip("When enabled, overrides the sequence's default pitch with a custom value.")]
        [SerializeField] private bool overridePitch = false;
        
        [Tooltip("Custom pitch for this step's audio (0.1 to 2.0).")]
        [SerializeField] [Range(0.1f, 2)] private float pitch;
        private Sequence _parentSequence;
        private bool _finished = false;

        /// <summary>
        /// Gets or sets the current status of the step.
        /// </summary>
        public SequenceStatus StepStatus
        {
            get => status;
            protected set
            {
                if (value == status) return;
                status = value;
                if (value != SequenceStatus.Inactive) Raise(value);
            }
        }

        /// <summary>
        /// Begins the step execution by starting audio and raising events.
        /// </summary>
        public override void Begin()
        {
            if (overridePitch) audioObject.pitch = pitch;
            StepStatus = SequenceStatus.Started;
            onStarted.Invoke();
            CheckAudioCompletion();
            if (_finished) CompleteStep();
        }

        /// <summary>
        /// Completes the step and moves to the next step in the sequence.
        /// </summary>
        public void CompleteStep()
        {
            if (status == SequenceStatus.Started)
            {
                onCompleted.Invoke();

                Complete();
            }
            else if (canBeFinishedBeforeStarted)
            {
                _finished = true;
            }
        }

        /// <summary>
        /// Initializes the step with its parent sequence.
        /// </summary>
        public void Initialize(Sequence sequence)
        {
            _finished = false;
            status = SequenceStatus.Inactive;
            _parentSequence = sequence;
        }

        private async void CheckAudioCompletion()
        {
            audioObject.Stop();
            if (audioClip is null) return;
            await Task.Delay((int)(audioDelay * 1000));
            audioObject.clip = audioClip;
            audioObject.Play();
            if (!audioOnly) return;
            await Task.Delay(100);
            while (audioObject.isPlaying) await Task.Yield();
            CompleteStep();
        }

        protected override SequenceStatus DefaultValue => status;

        private void Complete()
        {
            audioObject.pitch = _parentSequence.pitch;
            StepStatus = SequenceStatus.Completed;
            _parentSequence.CompleteStep(this);
        }
    }
}
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
        [Tooltip("The audio source to play the audio clip from.")]
        [SerializeField] private AudioClip audioClip;
        [Tooltip("set to true if you want to give the user the ability to finish the step before it is started")]
        [SerializeField] private bool canBeFinshedBeforeStarted;
        [Tooltip("set true if the step will complete when the audio clip is finished playing")]
        [SerializeField] private bool audioOnly;
        [SerializeField] private float audioDelay = .1f;
        [SerializeField] private UnityEvent onStarted;
        [SerializeField] private UnityEvent onCompleted;
        [SerializeField] private bool overridePitch = false;
        [SerializeField] [Range(0.1f, 2)] private float pitch;
        private Sequence _parentSequence;
        private bool _finished = false;


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

        public override void Begin()
        {
            if (overridePitch) audioObject.pitch = pitch;
            StepStatus = SequenceStatus.Started;
            onStarted.Invoke();
            CheckAudioCompletion();
            if (_finished) CompleteStep();
        }

        public void CompleteStep()
        {
            if (status == SequenceStatus.Started)
            {
                onCompleted.Invoke();

                Complete();
            }
            else if (canBeFinshedBeforeStarted)
            {
                _finished = true;
            }
        }

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
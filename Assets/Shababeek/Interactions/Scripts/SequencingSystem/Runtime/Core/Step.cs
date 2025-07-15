using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    [Serializable]
    public class Step : SequenceNode
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private bool canBeFinshedBeforeStarted;
        [SerializeField] private bool audioOnly;
        [SerializeField] private float audioDelay = .5f;
        [SerializeField] private UnityEvent onStarted;
        [SerializeField] private UnityEvent onCompleted;
        [SerializeField] private bool overridePitch = false;
        [SerializeField] [Range(0.1f, 2)] private float pitch;
        private Sequence parentSequence;
        private bool finished = false;


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
            if (finished) CompleteStep();
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
            audioObject.pitch = parentSequence.pitch;
            StepStatus = SequenceStatus.Completed;
            parentSequence.CompleteStep(this);
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
                finished = true;
            }
        }

        public void Initialize(Sequence sequence)
        {
            finished = false;
            status = SequenceStatus.Inactive;
            parentSequence = sequence;
        }
    }
}
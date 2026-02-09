using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Shababeek.Utilities;
using UnityEngine;

[assembly: InternalsVisibleTo("Shababeek.Kuest.Editor")]

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Represents a sequence of steps that can be executed in order.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Sequencing/Sequence")]
    public class Sequence : SequenceNode
    {
        [Tooltip("Audio pitch multiplier for the sequence (0.1 to 2.0).")]
        [SerializeField, Range(0.1f, 2)] internal float pitch = 1;

        internal override float SequencePitch => pitch;
        
        [Tooltip("Audio volume level for the sequence (0 to 1).")]
        [SerializeField, Range(0, 1)] private float volume = .5f;

        [HideInInspector] [SerializeField] private List<Step> steps;

        [SerializeField, ReadOnly] private int currentStepIndex;
        private bool initialized;

        /// <summary>
        /// Gets whether the sequence has been started.
        /// </summary>
        public bool Started => status == SequenceStatus.Started;
        
        /// <summary>
        /// Gets the current step being executed in the sequence.
        /// </summary>
        public Step CurrentStep => currentStepIndex < steps.Count ? steps[currentStepIndex] : null;
        
        /// <summary>
        /// Gets the list of all steps in the sequence.
        /// </summary>
        public List<Step> Steps => steps;

        private void Awake()
        {
            initialized = false;
        }

        private void OnEnable()
        {
            Awake();
        }

        /// <summary>
        /// Begins the sequence execution by starting the first step.
        /// </summary>
        public override void Begin()
        {
            Debug.Log($"starting sequence{name}");
            currentStepIndex = 0;

            status = SequenceStatus.Started;
            if (!initialized)
            {
                initialized = true;
                audioObject = new GameObject($"{name}_AudioObject").AddComponent<AudioSource>();
                audioObject.loop = false;
                audioObject.playOnAwake = false;
                audioObject.pitch = pitch;
                audioObject.volume = volume;
            }

            foreach (var step in steps)
            {
                step.audioObject = audioObject;
                step.Initialize(this);
            }

            steps[currentStepIndex].Begin();
            Raise(SequenceStatus.Started);
        }

        internal override void CompleteStep(Step step)
        {
            if (steps[currentStepIndex] != step) return;
            currentStepIndex++;
            if (currentStepIndex < steps.Count)
            {
                steps[currentStepIndex].Begin();
                return;
            }

            status = SequenceStatus.Completed;
            Raise(SequenceStatus.Completed);
        }

        /// <summary>
        /// Plays an audio clip using the sequence's audio source.
        /// </summary>
        public void PlayClip(AudioClip clip)
        {
            audioObject.Stop();
            audioObject.clip = clip;
            audioObject.Play();
        }

        /// <summary>
        /// Goes back to the previous step in the sequence.
        /// </summary>
        public void GoToPreviousStep()
        {
            if (currentStepIndex <= 0) return;
            steps[currentStepIndex].Raise(SequenceStatus.Inactive);
            currentStepIndex--;
            steps[currentStepIndex].Begin();
        }

        /// <summary>
        /// Resets the sequence to its initial state so it can be started again.
        /// </summary>
        public void Reset()
        {
            foreach (var step in steps)
            {
                step.Raise(SequenceStatus.Inactive);
            }
            currentStepIndex = 0;
            status = SequenceStatus.Inactive;
            initialized = false;
        }

        /// <summary>
        /// Initializes the sequence by creating an empty steps list.
        /// </summary>
        public void Init() => steps = new List<Step>();
    }
}
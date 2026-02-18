using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Shababeek.ReactiveVars;
using UnityEngine;

[assembly: InternalsVisibleTo("Shababeek.Sequencing.Editor")]

namespace Shababeek.Sequencing
{
    [Serializable]
    internal class StepNodePosition
    {
        public Step step;
        public Vector2 position;
    }

    /// <summary>
    /// A sequence that supports conditional branching between steps.
    /// Steps are connected via transitions that evaluate ScriptableVariable conditions.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Sequencing/BranchingSequence")]
    public class BranchingSequence : SequenceNode
    {
        [HideInInspector]
        [SerializeField] internal List<StepNodePosition> nodePositions = new();

        [Tooltip("Audio pitch multiplier for the sequence (0.1 to 2.0).")]
        [SerializeField, Range(0.1f, 2)] internal float pitch = 1;

        [Tooltip("Audio volume level for the sequence (0 to 1).")]
        [SerializeField, Range(0, 1)] private float volume = .5f;

        [Tooltip("The first step to execute when the sequence begins.")]
        [SerializeField] private Step entryStep;

        [HideInInspector] [SerializeField] private List<Step> allSteps = new();
        [HideInInspector] [SerializeField] private List<StepTransitionGroup> transitionGroups = new();

        [SerializeField, ReadOnly] private Step currentStep;
        private bool initialized;
        private Dictionary<Step, List<StepTransition>> _transitionCache;

        internal override float SequencePitch => pitch;

        /// <summary>
        /// Gets whether the sequence has been started.
        /// </summary>
        public bool Started => status == SequenceStatus.Started;

        /// <summary>
        /// Gets the currently active step.
        /// </summary>
        public Step CurrentStep => currentStep;

        /// <summary>
        /// Gets all steps in the branching sequence.
        /// </summary>
        public List<Step> AllSteps => allSteps;

        /// <summary>
        /// Gets all transition groups in the branching sequence.
        /// </summary>
        public List<StepTransitionGroup> TransitionGroups => transitionGroups;

        /// <summary>
        /// Gets the entry step for this branching sequence.
        /// </summary>
        public Step EntryStep => entryStep;

        private void Awake()
        {
            initialized = false;
        }

        private void OnEnable()
        {
            Awake();
        }

        /// <summary>
        /// Begins the branching sequence at the entry step.
        /// </summary>
        public override void Begin()
        {
            Debug.Log($"Starting branching sequence '{name}'");

            currentStep = null;
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

            BuildTransitionCache();

            foreach (var step in allSteps)
            {
                if (step == null) continue;
                step.audioObject = audioObject;
                step.Initialize(this);
            }

            if (entryStep == null)
            {
                Debug.LogError($"[BranchingSequence] '{name}' has no entry step assigned.");
                EndSequence();
                return;
            }

            TransitionToStep(entryStep);
            Raise(SequenceStatus.Started);
        }

        /// <summary>
        /// Called when a child step completes. Evaluates transitions to determine the next step.
        /// </summary>
        internal override void CompleteStep(Step step)
        {
            if (currentStep != step)
            {
                Debug.LogWarning(
                    $"[BranchingSequence] Step '{step.name}' completed but is not the current step '{currentStep?.name}'.");
                return;
            }

            if (!_transitionCache.TryGetValue(currentStep, out var transitions) || transitions.Count == 0)
            {
                EndSequence();
                return;
            }

            foreach (var transition in transitions)
            {
                if (!transition.Evaluate()) continue;

                if (transition.TargetStep == null)
                {
                    Debug.LogWarning(
                        $"[BranchingSequence] Transition from '{currentStep.name}' matched but has no target step.");
                    EndSequence();
                    return;
                }

                transition.TransitionEvent?.Raise();
                TransitionToStep(transition.TargetStep);
                return;
            }

            // No transition matched
            Debug.LogWarning(
                $"[BranchingSequence] No matching transition from step '{currentStep.name}'. Ending sequence.");
            EndSequence();
        }

        private void TransitionToStep(Step nextStep)
        {
            currentStep = nextStep;
            currentStep.Begin();
        }

        private void EndSequence()
        {
            currentStep = null;
            status = SequenceStatus.Completed;
            Raise(SequenceStatus.Completed);
        }

        private void BuildTransitionCache()
        {
            _transitionCache = new Dictionary<Step, List<StepTransition>>();

            foreach (var group in transitionGroups)
            {
                if (group?.fromStep == null) continue;

                if (!_transitionCache.ContainsKey(group.fromStep))
                    _transitionCache[group.fromStep] = new List<StepTransition>();

                foreach (var transition in group.transitions)
                {
                    if (transition != null)
                        _transitionCache[group.fromStep].Add(transition);
                }
            }
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
        /// Resets the sequence to its initial state.
        /// </summary>
        public void Reset()
        {
            foreach (var step in allSteps)
            {
                if (step != null)
                    step.Raise(SequenceStatus.Inactive);
            }

            currentStep = null;
            status = SequenceStatus.Inactive;
            initialized = false;
        }

        /// <summary>
        /// Initializes empty lists for a new branching sequence asset.
        /// </summary>
        public void Init()
        {
            allSteps = new List<Step>();
            transitionGroups = new List<StepTransitionGroup>();
        }

        internal Vector2 GetStepPosition(Step step)
        {
            if (nodePositions == null) return new Vector2(float.NaN, float.NaN);
            foreach (var np in nodePositions)
                if (np.step == step) return np.position;
            return new Vector2(float.NaN, float.NaN);
        }

        internal void SetStepPosition(Step step, Vector2 position)
        {
            nodePositions ??= new List<StepNodePosition>();
            foreach (var np in nodePositions)
            {
                if (np.step != step) continue;
                np.position = position;
                return;
            }

            nodePositions.Add(new StepNodePosition { step = step, position = position });
        }
    }
}

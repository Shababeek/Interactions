using System;
using System.Collections.Generic;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Manages the execution of sequences in the sequencing system.
    /// Handles sequence lifecycle, timing, debug controls, and analytics tracking.
    /// </summary>
    public class SequenceBehaviour : MonoBehaviour
    {
        [Tooltip("The sequence to be executed by this behaviour.")]
        [SerializeField] public Sequence sequence;

        [Tooltip("Whether the sequence starts automatically when the object is enabled.")]
        [SerializeField] private bool startOnAwake = false;

        [Tooltip("Delay in seconds before the sequence begins after being triggered.")]
        [SerializeField] private float delay = 0;

        [Header("Events")]
        [Tooltip("Event raised when the sequence starts.")]
        [SerializeField] private UnityEvent onSequenceStarted;

        [Tooltip("Event raised when the sequence completes all steps.")]
        [SerializeField] private UnityEvent onSequenceCompleted;

        [Header("Debug Controls")]
        [Tooltip("Enable keyboard controls for stepping through the sequence during development.")]
        [SerializeField] private bool enableDebugControls = false;

        [Tooltip("Key to advance to the next step (default: N).")]
        [SerializeField] private Key nextStepKey = Key.N;

        [Tooltip("Key to go back to the previous step (default: P).")]
        [SerializeField] private Key previousStepKey = Key.P;

        [Header("Analytics")]
        [Tooltip("Enable analytics tracking for sequence start and completion times.")]
        [SerializeField] private bool enableAnalytics = false;

        [Header("Runtime State")]
        [ReadOnly] [SerializeField] private bool started;

        [HideInInspector] [SerializeField] internal List<StepEventPair> steps;
        [HideInInspector] [SerializeField] internal List<StepEventListener> stepListeners;

        /// <summary>
        /// Whether the sequence starts automatically on Awake.
        /// </summary>
        public bool StartOnAwake => startOnAwake;

        /// <summary>
        /// Whether the sequence has been started.
        /// </summary>
        public bool Started => started;

        private float _startTime;

        private void Awake()
        {
            if (!enableAnalytics || sequence == null || sequence.Steps == null || sequence.Steps.Count == 0) return;
            SubscribeAnalytics();
        }

        private void OnEnable()
        {
            if (sequence == null) return;

            sequence.OnRaisedData
                .Where(status => status == SequenceStatus.Started)
                .Do(_ => onSequenceStarted.Invoke())
                .Subscribe()
                .AddTo(this);

            sequence.OnRaisedData
                .Where(status => status == SequenceStatus.Completed)
                .Do(_ => onSequenceCompleted.Invoke())
                .Subscribe()
                .AddTo(this);

            if (StartOnAwake)
                BeginSequence();
        }

        private async void BeginSequence()
        {
            await Awaitable.NextFrameAsync();
            if (delay > 0)
            {
                await Awaitable.WaitForSecondsAsync(delay);
            }
            sequence.Begin();
            started = true;
        }

        /// <summary>
        /// Starts the sequence manually. Use this when StartOnAwake is disabled.
        /// </summary>
        public void StartSequence()
        {
            if (started) return;
            BeginSequence();
        }

        /// <summary>
        /// Restarts the sequence from the beginning.
        /// </summary>
        public void RestartSequence()
        {
            started = false;
            sequence.Reset();
            BeginSequence();
        }

        /// <summary>
        /// Skips the current step and advances to the next one.
        /// </summary>
        public void SkipCurrentStep()
        {
            if (!started || sequence.CurrentStep == null) return;
            sequence.CurrentStep.CompleteStep();
        }

        /// <summary>
        /// Goes back to the previous step in the sequence.
        /// </summary>
        public void GoToPreviousStep()
        {
            if (!started || sequence == null) return;
            sequence.GoToPreviousStep();
        }

        private void Update()
        {
            if (!enableDebugControls || !started) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard[nextStepKey].wasPressedThisFrame)
            {
                SkipCurrentStep();
            }
            else if (keyboard[previousStepKey].wasPressedThisFrame)
            {
                GoToPreviousStep();
            }
        }

        private void SubscribeAnalytics()
        {
            sequence.Steps[0].OnRaisedData
                .Where(status => status == SequenceStatus.Started)
                .Do(_ => _startTime = Time.realtimeSinceStartup)
                .Subscribe()
                .AddTo(this);

            sequence.Steps[^1].OnRaisedData
                .Where(status => status == SequenceStatus.Completed)
                .Do(_ =>
                {
                    var elapsed = Time.realtimeSinceStartup - _startTime;
                    Debug.Log($"[Sequence Analytics] '{sequence.name}' completed in {elapsed:F2}s");
                })
                .Subscribe()
                .AddTo(this);
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

using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Manages the lifecycle of a BranchingSequence in the scene.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/BranchingSequenceBehaviour")]
    public class BranchingSequenceBehaviour : MonoBehaviour
    {
        [Tooltip("The branching sequence to execute.")]
        [SerializeField] public BranchingSequence sequence;

        [Tooltip("Whether the sequence starts automatically when the object is enabled.")]
        [SerializeField] private bool startOnAwake;

        [Tooltip("Delay in seconds before the sequence begins.")]
        [SerializeField] private float delay;

        [Header("Events")]
        [Tooltip("Event raised when the sequence starts.")]
        [SerializeField] private UnityEvent onSequenceStarted;

        [Tooltip("Event raised when the sequence completes.")]
        [SerializeField] private UnityEvent onSequenceCompleted;

        [Header("Debug Controls")]
        [Tooltip("Enable keyboard controls for debugging during development.")]
        [SerializeField] private bool enableDebugControls;

        [Tooltip("Key to skip the current step (default: N).")]
        [SerializeField] private Key skipStepKey = Key.N;

        [Header("Analytics")]
        [Tooltip("Enable analytics tracking for sequence timing.")]
        [SerializeField] private bool enableAnalytics;

        [Header("Runtime State")]
        [ReadOnly] [SerializeField] private bool started;

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
            if (!enableAnalytics || sequence == null) return;
            SubscribeAnalytics();
        }

        private void OnEnable()
        {
            if (sequence == null) return;

            sequence.OnRaisedData
                .Where(s => s == SequenceStatus.Started)
                .Do(_ => onSequenceStarted.Invoke())
                .Subscribe()
                .AddTo(this);

            sequence.OnRaisedData
                .Where(s => s == SequenceStatus.Completed)
                .Do(_ =>
                {
                    onSequenceCompleted.Invoke();
                    started = false;
                })
                .Subscribe()
                .AddTo(this);

            if (startOnAwake)
                BeginSequence();
        }

        private async void BeginSequence()
        {
            await Awaitable.NextFrameAsync();
            if (delay > 0)
                await Awaitable.WaitForSecondsAsync(delay);

            sequence.Begin();
            started = true;
        }

        /// <summary>
        /// Starts the branching sequence manually.
        /// </summary>
        public void StartSequence()
        {
            if (started) return;
            BeginSequence();
        }

        /// <summary>
        /// Restarts the branching sequence from the beginning.
        /// </summary>
        public void RestartSequence()
        {
            started = false;
            sequence.Reset();
            BeginSequence();
        }

        /// <summary>
        /// Skips the current step by forcing completion.
        /// </summary>
        public void SkipCurrentStep()
        {
            if (!started || sequence.CurrentStep == null) return;
            sequence.CurrentStep.CompleteStep();
        }

        private void Update()
        {
            if (!enableDebugControls || !started) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard[skipStepKey].wasPressedThisFrame)
                SkipCurrentStep();
        }

        private void SubscribeAnalytics()
        {
            sequence.OnRaisedData
                .Where(s => s == SequenceStatus.Started)
                .Do(_ => _startTime = Time.realtimeSinceStartup)
                .Subscribe()
                .AddTo(this);

            sequence.OnRaisedData
                .Where(s => s == SequenceStatus.Completed)
                .Do(_ =>
                {
                    var elapsed = Time.realtimeSinceStartup - _startTime;
                    Debug.Log($"[BranchingSequence Analytics] '{sequence.name}' completed in {elapsed:F2}s");
                })
                .Subscribe()
                .AddTo(this);
        }
    }
}

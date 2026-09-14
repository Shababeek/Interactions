using System;
using System.Threading.Tasks;
using UniRx;
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

        [Tooltip("Sound effect played alongside the voice line when this step starts. Optional.")]
        [SerializeField] private AudioClip sfxClip;

        [Tooltip("Voice line for the hint, played after a first mistake. Optional.")]
        [SerializeField] private AudioClip hintClip;

        [Tooltip("Voice line for the explicit correction, played after a repeated mistake. Optional.")]
        [SerializeField] private AudioClip fixClip;

        [Tooltip("What the trainee is asked to do. Shown on the instruction panel while the step runs.")]
        [SerializeField] [TextArea(2, 4)] private string instructionText;

        [Tooltip("Nudge shown after a first mistake, without giving the answer away.")]
        [SerializeField] [TextArea(2, 4)] private string hintText;

        [Tooltip("Explicit corrective instruction shown after a repeated mistake.")]
        [SerializeField] [TextArea(2, 4)] private string fixText;

        [Tooltip("Enable to allow the step to be completed before it starts.")]
        [SerializeField] private bool canBeFinshedBeforeStarted;

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
        private Func<bool> _completionGuard;
        private readonly Subject<Unit> _completionRejected = new();

        /// <summary>Text asked of the trainee while this step runs.</summary>
        public string InstructionText => instructionText;

        /// <summary>Nudge shown after a first mistake.</summary>
        public string HintText => hintText;

        /// <summary>Explicit corrective instruction shown after a repeated mistake.</summary>
        public string FixText => fixText;

        /// <summary>Voice line matching <see cref="HintText"/>.</summary>
        public AudioClip HintClip => hintClip;

        /// <summary>Voice line matching <see cref="FixText"/>.</summary>
        public AudioClip FixClip => fixClip;

        /// <summary>
        /// Fires when a completion attempt was refused by the completion guard.
        /// </summary>
        /// <remarks>
        /// Subscribe to this to drive corrective feedback: a veto with no feedback is
        /// indistinguishable from a step that simply never fired.
        /// </remarks>
        public IObservable<Unit> OnCompletionRejected => _completionRejected;

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
        /// Installs a predicate that must return true before this step is allowed to complete.
        /// </summary>
        /// <param name="guard">The predicate, or null to remove the current guard.</param>
        /// <remarks>
        /// Lets a scene-side validator veto completion without subclassing this ScriptableObject.
        /// Only one guard is held at a time; the last caller wins.
        /// </remarks>
        public void SetCompletionGuard(Func<bool> guard) => _completionGuard = guard;

        /// <summary>
        /// Whether this step is currently allowed to complete.
        /// </summary>
        protected virtual bool CanComplete() => _completionGuard == null || _completionGuard();

        /// <summary>
        /// Begins the step execution by starting audio and raising events.
        /// </summary>
        public override void Begin()
        {
            if (overridePitch && voiceSource) voiceSource.pitch = pitch;
            StepStatus = SequenceStatus.Started;
            onStarted.Invoke();

            // A step banked before it started has already been performed, so its instruction line
            // is stale. Skipping it also avoids the line being cut off mid-word by the next step.
            if (_finished)
            {
                CompleteStep();
                return;
            }

            PlayStepAudio();
        }

        /// <summary>
        /// Completes the step and moves to the next step in the sequence.
        /// </summary>
        public void CompleteStep()
        {
            if (status == SequenceStatus.Started)
            {
                if (!CanComplete())
                {
                    _completionRejected.OnNext(Unit.Default);
                    return;
                }

                onCompleted.Invoke();
                Complete();
            }
            else if (canBeFinshedBeforeStarted)
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

        private async void PlayStepAudio()
        {
            if (!voiceSource) return;
            voiceSource.Stop();
            if (sfxClip && sfxSource) sfxSource.PlayOneShot(sfxClip);
            if (audioClip is null) return;

            await Task.Delay((int)(audioDelay * 1000));
            if (status != SequenceStatus.Started || !voiceSource) return;

            voiceSource.clip = audioClip;
            voiceSource.Play();
            if (!audioOnly) return;

            await Task.Delay(100);
            while (voiceSource && voiceSource.isPlaying) await Task.Yield();
            if (status == SequenceStatus.Started) CompleteStep();
        }

        protected override SequenceStatus DefaultValue => status;

        private void Complete()
        {
            if (voiceSource && _parentSequence) voiceSource.pitch = _parentSequence.pitch;
            StepStatus = SequenceStatus.Completed;
            _parentSequence.CompleteStep(this);
        }
    }
}

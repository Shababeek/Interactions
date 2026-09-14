using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Shababeek.Utilities;
using UnityEngine;
using UnityEngine.Audio;

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

        [Tooltip("Voice-over volume level (0 to 1).")]
        [SerializeField, Range(0, 1)] private float volume = .5f;

        [Tooltip("Sound-effect volume level (0 to 1).")]
        [SerializeField, Range(0, 1)] private float sfxVolume = .8f;

        [Tooltip("Mixer group for the voice-over channel. Leave empty to play straight to master.")]
        [SerializeField] private AudioMixerGroup voiceMixerGroup;

        [Tooltip("Mixer group for the sound-effect channel. Leave empty to play straight to master.")]
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        [HideInInspector] [SerializeField] private List<Step> steps;

        /// <summary>
        /// Index of the step currently running. Runtime state only - see <see cref="SequenceNode.status"/>.
        /// </summary>
        [NonSerialized] private int currentStepIndex;

        /// <summary>
        /// Gets whether the sequence has been started.
        /// </summary>
        public bool Started => status == SequenceStatus.Started;

        /// <summary>
        /// Gets the current step being executed in the sequence.
        /// </summary>
        public Step CurrentStep => currentStepIndex < steps.Count ? steps[currentStepIndex] : null;

        /// <summary>
        /// Gets the zero-based index of the step currently running.
        /// </summary>
        public int CurrentStepIndex => currentStepIndex;

        /// <summary>
        /// Gets the list of all steps in the sequence.
        /// </summary>
        public List<Step> Steps => steps;

        /// <summary>
        /// Whether a voice line is currently playing.
        /// </summary>
        /// <remarks>
        /// Exposed so external systems (ducking, subtitles) can react to narration without
        /// being handed the AudioSource itself.
        /// </remarks>
        public bool IsVoicePlaying => voiceSource && voiceSource.isPlaying;

        private void OnEnable()
        {
            status = SequenceStatus.Inactive;
            currentStepIndex = 0;
        }

        /// <summary>
        /// Begins the sequence execution by starting the first step.
        /// </summary>
        public override void Begin()
        {
            Debug.Log($"starting sequence{name}");
            currentStepIndex = 0;

            status = SequenceStatus.Started;
            EnsureAudioSources();

            foreach (var step in steps)
            {
                step.voiceSource = voiceSource;
                step.sfxSource = sfxSource;
                step.Initialize(this);
            }

            steps[currentStepIndex].Begin();
            Raise(SequenceStatus.Started);
        }

        /// <summary>
        /// Creates the voice and SFX sources if they are missing.
        /// </summary>
        /// <remarks>
        /// Checked by reference rather than by an "initialized" flag: this ScriptableObject outlives
        /// scene loads, but the AudioSource GameObjects do not, which previously left a destroyed
        /// reference behind on the second play-through.
        /// </remarks>
        private void EnsureAudioSources()
        {
            if (!voiceSource)
            {
                voiceSource = CreateSource($"{name}_VoiceSource", volume, voiceMixerGroup);
                voiceSource.pitch = pitch;
            }

            if (!sfxSource) sfxSource = CreateSource($"{name}_SFXSource", sfxVolume, sfxMixerGroup);
        }

        private static AudioSource CreateSource(string sourceName, float sourceVolume, AudioMixerGroup group)
        {
            var source = new GameObject(sourceName).AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.volume = sourceVolume;
            if (group) source.outputAudioMixerGroup = group;
            return source;
        }

        internal void CompleteStep(Step step)
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
        /// Plays an audio clip on the voice channel, replacing whatever was playing.
        /// </summary>
        public void PlayClip(AudioClip clip)
        {
            EnsureAudioSources();
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        /// <summary>
        /// Plays a sound effect on the SFX channel without disturbing the voice channel.
        /// </summary>
        /// <remarks>
        /// Uses PlayOneShot, so overlapping effects layer rather than cutting each other off.
        /// </remarks>
        public void PlaySfx(AudioClip clip)
        {
            if (!clip) return;
            EnsureAudioSources();
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// Initializes the sequence by creating an empty steps list.
        /// </summary>
        public void Init() => steps = new List<Step>();
    }
}

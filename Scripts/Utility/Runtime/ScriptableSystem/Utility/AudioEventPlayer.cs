using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Plays audio events with spatial positioning support.
    /// Listens to multiple AudioVariables and plays them at specified positions.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu(menuName: "Shababeek/Scriptable System/Audio Event Player")]
    public class AudioEventPlayer : MonoBehaviour
    {
        [Header("Event Audio Variables")]
        [Tooltip("AudioVariables that will play automatically when raised")]
        [SerializeField] private List<AudioVariable> audioVariables = new List<AudioVariable>();

        [Header("Settings")]
        [Tooltip("Move AudioSource to event position for spatial audio")]
        [SerializeField] private bool useSpatialAudio = true;

        private AudioSource _audioSource;
        private CompositeDisposable _disposable;
        private AudioVariable _currentLoopingAudio;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                Debug.LogError($"AudioSource component not found on {gameObject.name}");
            }
        }

        private void OnEnable()
        {
            if (_audioSource == null) return;

            _disposable = new CompositeDisposable();

            foreach (var audioVariable in audioVariables)
            {
                if (audioVariable != null)
                {
                    // Subscribe to normal raise (without position)
                    audioVariable.OnAudioRaised
                        .Subscribe(raisedAudio => PlayAudio(raisedAudio, transform.position))
                        .AddTo(_disposable);

                    // Subscribe to raise with position
                    audioVariable.OnAudioRaisedWithPosition
                        .Subscribe(data => PlayAudio(data.audioVariable, data.position))
                        .AddTo(_disposable);

                    audioVariable.OnAudioStopped
                        .Subscribe(stoppedAudio => StopAudio(stoppedAudio))
                        .AddTo(_disposable);

                    audioVariable.OnPitchChanged
                        .Subscribe(newPitch => UpdatePitch(audioVariable, newPitch))
                        .AddTo(_disposable);
                }
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
            _currentLoopingAudio = null;
        }

        private void PlayAudio(AudioVariable audioVariable, Vector3 position)
        {
            if (audioVariable == null || audioVariable.Clip == null) return;

            // Move AudioSource to position if spatial audio is enabled
            if (useSpatialAudio)
            {
                transform.position = position;
            }

            if (audioVariable.Loop)
            {
                if (_currentLoopingAudio == audioVariable && _audioSource.isPlaying)
                    return;

                if (_currentLoopingAudio != null && _currentLoopingAudio != audioVariable)
                {
                    _audioSource.Stop();
                }

                _currentLoopingAudio = audioVariable;
                _audioSource.clip = audioVariable.Clip;
                _audioSource.volume = audioVariable.Volume;
                _audioSource.pitch = audioVariable.Pitch;
                _audioSource.loop = true;
                _audioSource.Play();
            }
            else
            {
                _audioSource.PlayOneShot(audioVariable.Clip, audioVariable.Volume);
            }
        }

        private void StopAudio(AudioVariable audioVariable)
        {
            if (audioVariable == null) return;

            // Stop if it's the current looping audio
            if (_currentLoopingAudio == audioVariable && _audioSource.isPlaying)
            {
                _audioSource.Stop();
                _currentLoopingAudio = null;
            }
            // Also stop AudioSource completely (stops PlayOneShot too)
            //else if (_audioSource.isPlaying)
            //{
            //    _audioSource.Stop();
            //}
        }

        private void UpdatePitch(AudioVariable audioVariable, float newPitch)
        {
            if (_currentLoopingAudio == audioVariable && _audioSource.isPlaying)
            {
                _audioSource.pitch = newPitch;
            }
        }

        public void AddAudioVariable(AudioVariable audioVariable)
        {
            if (audioVariable == null || audioVariables.Contains(audioVariable)) return;

            audioVariables.Add(audioVariable);

            if (_disposable != null && !_disposable.IsDisposed)
            {
                audioVariable.OnAudioRaised
                    .Subscribe(raisedAudio => PlayAudio(raisedAudio, transform.position))
                    .AddTo(_disposable);

                audioVariable.OnAudioRaisedWithPosition
                    .Subscribe(data => PlayAudio(data.audioVariable, data.position))
                    .AddTo(_disposable);

                audioVariable.OnAudioStopped
                    .Subscribe(stoppedAudio => StopAudio(stoppedAudio))
                    .AddTo(_disposable);

                audioVariable.OnPitchChanged
                    .Subscribe(newPitch => UpdatePitch(audioVariable, newPitch))
                    .AddTo(_disposable);
            }
        }

        public void RemoveAudioVariable(AudioVariable audioVariable)
        {
            audioVariables.Remove(audioVariable);
        }
    }
}
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu(menuName: "Shababeek/Scriptable System/Audio Source Binder")]
    public class AudioSourceBinder : MonoBehaviour
    {
        [SerializeField] private List<AudioVariable> audioVariables = new List<AudioVariable>();

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
                    audioVariable.OnAudioRaised
                        .Subscribe(raisedAudio => PlayAudio(raisedAudio))
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

        private void PlayAudio(AudioVariable audioVariable)
        {
            if (audioVariable == null || audioVariable.Clip == null) return;

            if (audioVariable.Loop)
            {
                // If already playing this audio, don't restart
                if (_currentLoopingAudio == audioVariable && _audioSource.isPlaying)
                    return;

                // Stop different loop if playing
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

            if (_currentLoopingAudio == audioVariable && _audioSource.isPlaying)
            {
                _audioSource.Stop();
                _currentLoopingAudio = null;
            }
        }

        private void UpdatePitch(AudioVariable audioVariable, float newPitch)
        {
            // Only update pitch if this is the currently playing loop
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
                    .Subscribe(raisedAudio => PlayAudio(raisedAudio))
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
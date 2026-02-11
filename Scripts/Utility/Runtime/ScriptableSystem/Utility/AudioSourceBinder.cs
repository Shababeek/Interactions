using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu(menuName: "Shababeek/Scriptable System/Audio Source Binder")]
    public class AudioSourceBinder : MonoBehaviour
    {
        [Header("Binding Mode (Legacy)")]
        [Tooltip("Binds this AudioVariable's settings to the AudioSource. Play manually via audioSource.Play()")]
        [SerializeField] private AudioVariable audioVariable;

        [Header("Event Mode (Auto-Play)")]
        [Tooltip("These AudioVariables play automatically when raised")]
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
                return;
            }

            // Binding mode - configure AudioSource with audioVariable settings
            if (audioVariable != null)
            {
                BindAudioSettings();
            }
        }

        private void OnEnable()
        {
            if (_audioSource == null) return;

            _disposable = new CompositeDisposable();

            // Event mode - subscribe to event AudioVariables
            foreach (var eventAudioVariable in audioVariables)
            {
                if (eventAudioVariable != null)
                {
                    eventAudioVariable.OnAudioRaised
                        .Subscribe(raisedAudio => PlayAudio(raisedAudio))
                        .AddTo(_disposable);

                    eventAudioVariable.OnAudioStopped
                        .Subscribe(stoppedAudio => StopAudio(stoppedAudio))
                        .AddTo(_disposable);

                    eventAudioVariable.OnPitchChanged
                        .Subscribe(newPitch => UpdatePitch(eventAudioVariable, newPitch))
                        .AddTo(_disposable);
                }
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
            _currentLoopingAudio = null;
        }

        /// <summary>
        /// Applies the bound AudioVariable settings to the AudioSource (Binding mode).
        /// </summary>
        private void BindAudioSettings()
        {
            if (audioVariable == null)
            {
                Debug.LogWarning($"AudioVariable is not assigned on {gameObject.name}");
                return;
            }

            _audioSource.clip = audioVariable.Clip;
            _audioSource.volume = audioVariable.Volume;
            _audioSource.pitch = audioVariable.Pitch;
            _audioSource.loop = audioVariable.Loop;
        }

        /// <summary>
        /// Rebinds the audio settings. Useful if you change the AudioVariable at runtime.
        /// </summary>
        public void RefreshBinding()
        {
            BindAudioSettings();
        }

        private void PlayAudio(AudioVariable eventAudioVariable)
        {
            if (eventAudioVariable == null || eventAudioVariable.Clip == null) return;

            if (eventAudioVariable.Loop)
            {
                if (_currentLoopingAudio == eventAudioVariable && _audioSource.isPlaying)
                    return;

                if (_currentLoopingAudio != null && _currentLoopingAudio != eventAudioVariable)
                {
                    _audioSource.Stop();
                }

                _currentLoopingAudio = eventAudioVariable;
                _audioSource.clip = eventAudioVariable.Clip;
                _audioSource.volume = eventAudioVariable.Volume;
                _audioSource.pitch = eventAudioVariable.Pitch;
                _audioSource.loop = true;
                _audioSource.Play();
            }
            else
            {
                _audioSource.PlayOneShot(eventAudioVariable.Clip, eventAudioVariable.Volume);
            }
        }

        private void StopAudio(AudioVariable eventAudioVariable)
        {
            if (eventAudioVariable == null) return;

            if (_currentLoopingAudio == eventAudioVariable && _audioSource.isPlaying)
            {
                _audioSource.Stop();
                _currentLoopingAudio = null;
            }
        }

        private void UpdatePitch(AudioVariable eventAudioVariable, float newPitch)
        {
            if (_currentLoopingAudio == eventAudioVariable && _audioSource.isPlaying)
            {
                _audioSource.pitch = newPitch;
            }
        }

        public void AddAudioVariable(AudioVariable eventAudioVariable)
        {
            if (eventAudioVariable == null || audioVariables.Contains(eventAudioVariable)) return;

            audioVariables.Add(eventAudioVariable);

            if (_disposable != null && !_disposable.IsDisposed)
            {
                eventAudioVariable.OnAudioRaised
                    .Subscribe(raisedAudio => PlayAudio(raisedAudio))
                    .AddTo(_disposable);

                eventAudioVariable.OnAudioStopped
                    .Subscribe(stoppedAudio => StopAudio(stoppedAudio))
                    .AddTo(_disposable);

                eventAudioVariable.OnPitchChanged
                    .Subscribe(newPitch => UpdatePitch(eventAudioVariable, newPitch))
                    .AddTo(_disposable);
            }
        }

        public void RemoveAudioVariable(AudioVariable eventAudioVariable)
        {
            audioVariables.Remove(eventAudioVariable);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && _audioSource != null && audioVariable != null)
            {
                BindAudioSettings();
            }
        }
#endif
    }
}
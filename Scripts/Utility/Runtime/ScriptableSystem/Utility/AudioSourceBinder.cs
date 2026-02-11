using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds an AudioVariable to an AudioSource component for configuration.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu(menuName: "Shababeek/Scriptable System/Audio Source Binder")]
    public class AudioSourceBinder : MonoBehaviour
    {
        [Tooltip("The AudioVariable to bind to the AudioSource.")]
        [SerializeField] private AudioVariable audioVariable;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                Debug.LogError($"AudioSource component not found on {gameObject.name}");
                return;
            }

            BindAudioSettings();
        }

        /// <summary>
        /// Applies the AudioVariable settings to the AudioSource.
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && _audioSource != null)
            {
                BindAudioSettings();
            }
        }
#endif
    }
}
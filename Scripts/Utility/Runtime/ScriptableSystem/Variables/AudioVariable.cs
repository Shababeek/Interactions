using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores audio configuration data.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/AudioVariable")]
    public class AudioVariable : ScriptableObject
    {
        [Tooltip("The audio clip to play.")]
        [SerializeField] private AudioClip clip;

        [Tooltip("The volume of the audio (0 to 1).")]
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;

        [Tooltip("The pitch of the audio.")]
        [SerializeField] [Range(-3f, 3f)] private float pitch = 1f;

        [Tooltip("Should the audio loop?")]
        [SerializeField] private bool loop = false;

        /// <summary>
        /// Gets the audio clip.
        /// </summary>
        public AudioClip Clip => clip;

        /// <summary>
        /// Gets the volume value.
        /// </summary>
        public float Volume => volume;

        /// <summary>
        /// Gets the pitch value.
        /// </summary>
        public float Pitch => pitch;

        /// <summary>
        /// Gets whether the audio should loop.
        /// </summary>
        public bool Loop => loop;
    }
}

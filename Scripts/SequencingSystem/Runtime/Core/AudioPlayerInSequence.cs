using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Plays audio clips using a sequence's audio source.
    /// </summary>
    public class AudioPlayerInSequence : MonoBehaviour
    {
        [Tooltip("The sequence whose audio source will be used for playback.")]
        [SerializeField] private Sequence sequence;
        
        [Tooltip("The audio clip to play.")]
        [SerializeField] private AudioClip clip;

        /// <summary>
        /// Plays the assigned audio clip using the sequence's audio source.
        /// </summary>
        public void Play()
        {
            sequence.PlayClip(clip);
        }
    }
}
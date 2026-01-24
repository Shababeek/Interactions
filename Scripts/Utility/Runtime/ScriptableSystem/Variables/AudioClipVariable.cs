using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores an AudioClip with playback helper methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/AudioClipVariable")]
    public class AudioClipVariable : ScriptableVariable<AudioClip>
    {
        /// <summary>
        /// Plays the audio clip on the specified AudioSource.
        /// </summary>
        public void Play(AudioSource audioSource)
        {
            if (Value != null && audioSource != null)
            {
                audioSource.clip = Value;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Plays the audio clip once on the specified AudioSource.
        /// </summary>
        public void PlayOneShot(AudioSource audioSource)
        {
            if (Value != null && audioSource != null)
            {
                audioSource.PlayOneShot(Value);
            }
        }

        /// <summary>
        /// Plays the audio clip once with a volume scale on the specified AudioSource.
        /// </summary>
        public void PlayOneShot(AudioSource audioSource, float volumeScale)
        {
            if (Value != null && audioSource != null)
            {
                audioSource.PlayOneShot(Value, volumeScale);
            }
        }

        /// <summary>
        /// Gets the length of the audio clip in seconds.
        /// </summary>
        public float Length => Value != null ? Value.length : 0f;
        
        /// <summary>
        /// Gets the sample frequency of the audio clip.
        /// </summary>
        public int Frequency => Value != null ? Value.frequency : 0;
        
        /// <summary>
        /// Gets the number of audio channels in the clip.
        /// </summary>
        public int Channels => Value != null ? Value.channels : 0;
        
        /// <summary>
        /// Gets whether the audio clip is valid (not null).
        /// </summary>
        public bool IsValid => Value != null;

        // Equality operators
        public static bool operator ==(AudioClipVariable a, AudioClipVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(AudioClipVariable a, AudioClipVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(AudioClipVariable a, AudioClip b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(AudioClipVariable a, AudioClip b)
        {
            return !(a == b);
        }

        public static bool operator ==(AudioClip a, AudioClipVariable b)
        {
            return b == a;
        }

        public static bool operator !=(AudioClip a, AudioClipVariable b)
        {
            return !(b == a);
        }

        // Use reference equality for Equals (standard object behavior)
        // Use == operator for value comparison in code
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}
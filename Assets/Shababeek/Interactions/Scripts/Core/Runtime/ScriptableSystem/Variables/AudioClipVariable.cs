using UnityEngine;

namespace Shababeek.Core
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/AudioClipVariable")]
    public class AudioClipVariable : ScriptableVariable<AudioClip>
    {
        public void Play(AudioSource audioSource)
        {
            if (Value != null && audioSource != null)
            {
                audioSource.clip = Value;
                audioSource.Play();
            }
        }

        public void PlayOneShot(AudioSource audioSource)
        {
            if (Value != null && audioSource != null)
            {
                audioSource.PlayOneShot(Value);
            }
        }

        public void PlayOneShot(AudioSource audioSource, float volumeScale)
        {
            if (Value != null && audioSource != null)
            {
                audioSource.PlayOneShot(Value, volumeScale);
            }
        }

        public float Length => Value != null ? Value.length : 0f;
        public int Frequency => Value != null ? Value.frequency : 0;
        public int Channels => Value != null ? Value.channels : 0;
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

        public override bool Equals(object obj)
        {
            if (obj is AudioClipVariable other) return this == other;
            if (obj is AudioClip audioClipValue) return this == audioClipValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
} 
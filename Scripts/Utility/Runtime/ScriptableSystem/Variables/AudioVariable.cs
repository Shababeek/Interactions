using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/AudioVariable")]
    public class AudioVariable : GameEvent
    {
        [SerializeField] private AudioClip clip;
        [SerializeField][Range(0f, 1f)] private float volume = 1f;
        [SerializeField][Range(-3f, 3f)] private float pitch = 1f;
        [SerializeField] private bool loop = false;

        private Subject<AudioVariable> _onAudioRaised;
        private Subject<AudioVariable> _onAudioStopped;
        private Subject<float> _onPitchChanged;
        private Subject<(AudioVariable, Vector3)> _onAudioRaisedWithPosition;

        public IObservable<AudioVariable> OnAudioRaised
        {
            get
            {
                if (_onAudioRaised == null)
                    _onAudioRaised = new Subject<AudioVariable>();
                return _onAudioRaised;
            }
        }

        public IObservable<AudioVariable> OnAudioStopped
        {
            get
            {
                if (_onAudioStopped == null)
                    _onAudioStopped = new Subject<AudioVariable>();
                return _onAudioStopped;
            }
        }

        public IObservable<float> OnPitchChanged
        {
            get
            {
                if (_onPitchChanged == null)
                    _onPitchChanged = new Subject<float>();
                return _onPitchChanged;
            }
        }

        public IObservable<(AudioVariable audioVariable, Vector3 position)> OnAudioRaisedWithPosition
        {
            get
            {
                if (_onAudioRaisedWithPosition == null)
                    _onAudioRaisedWithPosition = new Subject<(AudioVariable, Vector3)>();
                return _onAudioRaisedWithPosition;
            }
        }

        public AudioClip Clip => clip;
        public float Volume => volume;
        public float Pitch => pitch;
        public bool Loop => loop;

        public override void Raise()
        {
            base.Raise();
            _onAudioRaised?.OnNext(this);
        }

        /// <summary>
        /// Raises the audio event with a specific position for spatial audio.
        /// </summary>
        public void Raise(Vector3 position)
        {
            base.Raise();
            _onAudioRaisedWithPosition?.OnNext((this, position));
        }

        public void Stop()
        {
            _onAudioStopped?.OnNext(this);
        }

        public void SetPitch(float newPitch)
        {
            pitch = Mathf.Clamp(newPitch, -3f, 3f);
            _onPitchChanged?.OnNext(pitch);
        }
    }
}
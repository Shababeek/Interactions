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

        public AudioClip Clip => clip;
        public float Volume => volume;
        public float Pitch => pitch;
        public bool Loop => loop;

        public override void Raise()
        {
            base.Raise();
            _onAudioRaised?.OnNext(this);
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
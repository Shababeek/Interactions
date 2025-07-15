using Shababeek.Core;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Shababeek.Interactions.Animations
{

    /// <summary>
    /// Mixes between two different states of a finger
    /// </summary>
    [System.Serializable]
    internal class FingerAnimationMixer
    {
        [Range(0, 1)] [SerializeField] private float weight;
        private AnimationLayerMixerPlayable _mixer;
        private TweenableFloat _crossFadingWeight;
        public float Weight
        {
            set
            {
                if (Mathf.Abs(value - weight) > .01f)
                {
                    weight = value;
                    _crossFadingWeight.Value = value;
                }
            }
            get => weight;
        }
        public FingerAnimationMixer(PlayableGraph graph, AnimationClip closed, AnimationClip opened, AvatarMask mask, VariableTweener lerper)
        {
            var openPlayable = AnimationClipPlayable.Create(graph, opened);
            var closedPlayable = AnimationClipPlayable.Create(graph, closed);
            InitializeMixer(graph, mask);
            ConnectPlayablesToGraph(graph, openPlayable, closedPlayable);
            SetMixerWeight(0);
            _crossFadingWeight = new TweenableFloat(lerper);
            _crossFadingWeight.OnChange += SetMixerWeight;
        }
        private void InitializeMixer(PlayableGraph graph, AvatarMask mask)
        {
            _mixer = AnimationLayerMixerPlayable.Create(graph, 2);
            _mixer.SetLayerAdditive(0, false);
            _mixer.SetLayerMaskFromAvatarMask(0, mask);
        }
        private void ConnectPlayablesToGraph(PlayableGraph graph, AnimationClipPlayable openPlayable, AnimationClipPlayable closedPlayable)
        {
            graph.Connect(openPlayable, 0, _mixer, 0);
            graph.Connect(closedPlayable, 0, _mixer, 1);
        }
        private void SetMixerWeight(float value)
        {
            _mixer.SetInputWeight(0, 1 - value);
            _mixer.SetInputWeight(1, value);
        }
        public AnimationLayerMixerPlayable Mixer => _mixer;
    }
}
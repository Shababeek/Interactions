using Kandooz.InteractionSystem.Core;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Kandooz.InteractionSystem.Animations
{

    /// <summary>
    /// Mixes between two different states of a finger
    /// </summary>
    [System.Serializable]
    public class FingerAnimationMixer
    {
        [Range(0, 1)] [SerializeField] private float weight;
        private AnimationLayerMixerPlayable mixer;
        private TweenableFloat crossFadingWeight;
        public float Weight
        {
            set
            {
                if (Mathf.Abs(value - weight) > .01f)
                {
                    weight = value;
                    crossFadingWeight.Value = value;
                }
            }
            get
            {
                return weight;
            }
        }
        public FingerAnimationMixer(PlayableGraph graph, AnimationClip closed, AnimationClip opened, AvatarMask mask, VariableTweener lerper)
        {
            var openPlayable = AnimationClipPlayable.Create(graph, opened);
            var closedPlayable = AnimationClipPlayable.Create(graph, closed);
            InitializeMixer(graph, mask);
            ConnectPlayablesToGraph(graph, openPlayable, closedPlayable);
            SetMixerWeight(0);
            crossFadingWeight = new TweenableFloat(lerper);
            crossFadingWeight.onChange += SetMixerWeight;
        }
        private void InitializeMixer(PlayableGraph graph, AvatarMask mask)
        {
            mixer = AnimationLayerMixerPlayable.Create(graph, 2);
            mixer.SetLayerAdditive(0, false);
            mixer.SetLayerMaskFromAvatarMask(0, mask);
        }
        private void ConnectPlayablesToGraph(PlayableGraph graph, AnimationClipPlayable openPlayable, AnimationClipPlayable closedPlayable)
        {
            graph.Connect(openPlayable, 0, mixer, 0);
            graph.Connect(closedPlayable, 0, mixer, 1);
        }
        private void SetMixerWeight(float value)
        {
            mixer.SetInputWeight(0, 1 - value);
            mixer.SetInputWeight(1, value);
        }
        public AnimationLayerMixerPlayable Mixer
        {
            get
            {
                return mixer;
            }
        }
    }
}
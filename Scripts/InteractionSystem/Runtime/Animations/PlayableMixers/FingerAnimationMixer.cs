using Shababeek.ReactiveVars;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Mixes between two different states of a finger (open and closed).
    /// Provides smooth transitions between finger positions using Unity's Playable system.
    /// </summary>
    /// <remarks>
    /// This class creates a layer mixer that blends between open and closed finger animations.
    /// It uses TweenableFloat for smooth weight transitions and applies avatar masks for
    /// finger-specific animation control.
    /// </remarks>
    [System.Serializable]
    internal class FingerAnimationMixer
    {
        [Header("Finger Animation")]
        [Range(0, 1)] [SerializeField] [Tooltip("The current weight between open (0) and closed (1) finger states.")]
        private float weight;
        
        private AnimationLayerMixerPlayable _mixer;
        private TweenableFloat _crossFadingWeight;
        
        /// <summary>
        /// Gets or sets the weight between open and closed finger states.
        /// </summary>
        /// <value>The weight value (0 = open, 1 = closed)</value>
        /// <returns>The current weight value</returns>
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
        
        /// <summary>
        /// Initializes a new finger animation mixer with open and closed animation clips.
        /// </summary>
        /// <param name="graph">The playable graph to create the animation in</param>
        /// <param name="closed">The animation clip for the closed finger state</param>
        /// <param name="opened">The animation clip for the open finger state</param>
        /// <param name="mask">The avatar mask for this finger</param>
        /// <param name="lerper">The variable tweener for smooth transitions</param>
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
        
        /// <summary>
        /// Gets the layer mixer playable that manages the finger animation blending.
        /// </summary>
        /// <returns>The animation layer mixer playable</returns>
        public AnimationLayerMixerPlayable Mixer => _mixer;
    }
}
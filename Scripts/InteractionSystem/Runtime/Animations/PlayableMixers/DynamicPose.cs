
using Shababeek.ReactiveVars;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Represents a dynamic pose that can be used in the interaction system.
    /// Allows real-time control of individual finger positions between open and closed states.
    /// </summary>
    /// <remarks>
    /// DynamicPose creates a layer mixer for each finger, allowing individual control
    /// of finger curl values. Each finger can be positioned anywhere between fully open
    /// and fully closed, enabling natural hand interactions and gestures.
    /// </remarks>
    internal class DynamicPose : IPose
    {
        private AnimationLayerMixerPlayable _poseMixer;
        private readonly FingerAnimationMixer[] _fingers;
        private readonly IAvatarMaskIndexer _handFingerMask;
        private readonly string _name;
        
        /// <summary>
        /// Sets the value of a finger in the dynamic pose.
        /// </summary>
        /// <param name="indexer">The index of the finger to set the value for:
        /// 0 for thumb, 1 for index, 2 for middle, 3 for ring, and 4 for pinky.</param>
        /// <value>The finger curl value (0 = extended, 1 = curled)</value>
        public float this[int indexer]
        {
            set => _fingers[indexer].Weight = value;
        }

        /// <summary>
        /// Gets the pose mixer playable that manages all finger layers.
        /// </summary>
        /// <returns>The animation layer mixer playable</returns>
        internal AnimationLayerMixerPlayable PoseMixer => _poseMixer;
        
        /// <summary>
        /// Gets the name of this dynamic pose.
        /// </summary>
        /// <returns>The pose name</returns>
        public string Name => _name;

        /// <summary>
        /// Initializes a new dynamic pose with the given configuration.
        /// </summary>
        /// <param name="graph">The playable graph to create the animation in</param>
        /// <param name="poseData">The pose data containing open and closed animation clips</param>
        /// <param name="fingerMask">The avatar mask indexer for finger-specific masking</param>
        /// <param name="tweener">The variable tweener for smooth transitions</param>
        internal DynamicPose(PlayableGraph graph, PoseData poseData, IAvatarMaskIndexer fingerMask, VariableTweener tweener)
        {
            _handFingerMask = fingerMask;
            _name = poseData.Name;
            _fingers = new FingerAnimationMixer[5];
            _poseMixer = AnimationLayerMixerPlayable.Create(graph, _fingers.Length);

            for (uint i = 0; i < _fingers.Length; i++)
            {
                CreateFingerLayer(i);
                CreateAndConnectFinger(graph, poseData, tweener, i);
            }
        }

        private void CreateFingerLayer(uint i)
        {
            _poseMixer.SetLayerAdditive(i, false);
            _poseMixer.SetLayerMaskFromAvatarMask(i, _handFingerMask[(int)i]);
            _poseMixer.SetInputWeight((int)i, 1);
        }

        private void CreateAndConnectFinger(PlayableGraph graph, PoseData poseData, VariableTweener tweener, uint i)
        {
            _fingers[i] = new FingerAnimationMixer(graph, poseData.ClosedAnimationClip, poseData.OpenAnimationClip, _handFingerMask[(int)i], tweener);
            graph.Connect(_fingers[i].Mixer, 0, _poseMixer, (int)i);
        }
    }
}

using Shababeek.Core;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Represents a dynamic pose that can be used in the interaction system.
    /// </summary>
    internal class DynamicPose : IPose
    {
        private AnimationLayerMixerPlayable _poseMixer;
        private readonly FingerAnimationMixer[] _fingers;
        private readonly IAvatarMaskIndexer _handFingerMask;
        private readonly string _name;
        /// <summary>
        /// This is used to set the value of a finger in the dynamic pose.
        /// </summary>
        /// <param name="indexer">The index of the finger to set the value for
        /// 0 for thumb, 1 for index, 2 for middle, 3 for ring, and 4 for pinky.
        /// </param>

        public float this[int indexer]
        {
            set => _fingers[indexer].Weight = value;
        }

        internal AnimationLayerMixerPlayable PoseMixer => _poseMixer;
        public string Name => _name;

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
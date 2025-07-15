
using Shababeek.Core;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// A pose that have every finger controller individually between two poses
    /// </summary>
    internal class DynamicPose : IPose
    {
        private AnimationLayerMixerPlayable _poseMixer;
        private readonly FingerAnimationMixer[] _fingers;
        private readonly IAvatarMaskIndexer _handFingerMask;
        private readonly string _name;

        public float this[int indexer]
        {
            set => _fingers[indexer].Weight = value;
        }

        internal AnimationLayerMixerPlayable PoseMixer => _poseMixer;
        public string Name => _name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">The graph to be conect the pose to</param>
        /// <param name="poseData">the pose Data object</param>
        /// <param name="fingerMask">Avatar Mask of the finger</param>
        /// <param name="tweener">The Variable tweener to be used by the editor</param>
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
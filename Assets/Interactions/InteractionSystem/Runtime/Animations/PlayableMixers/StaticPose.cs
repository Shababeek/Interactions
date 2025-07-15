
using Kandooz.InteractionSystem.Core;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kandooz.InteractionSystem.Animations
{
    [System.Serializable]
    public class StaticPose : IPose
    {
        public float this[FingerName index] { set  { } }
        public float this[int index] { set { } }

        private AnimationClipPlayable playable;
        private string name;

        public AnimationClipPlayable Mixer { get => playable;  }
        public string Name => name;

        public StaticPose(PlayableGraph graph, PoseData poseData)
        {
            playable = AnimationClipPlayable.Create(graph, poseData.OpenAnimationClip);
            name = poseData.Name;
        }

    }
}

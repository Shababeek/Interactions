using Shababeek.Interactions.Core;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// A pose that does not allow acess to fingers indvidually
    /// </summary>
    [System.Serializable]
    public class StaticPose : IPose
    {
        public float this[int index] { set { } }

        private AnimationClipPlayable _playable;
        private string _name;

        public AnimationClipPlayable Mixer => _playable;
        public string Name => _name;

        public StaticPose(PlayableGraph graph, PoseData poseData)
        {
            _playable = AnimationClipPlayable.Create(graph, poseData.OpenAnimationClip);
            _name = poseData.Name;
        }

    }
}

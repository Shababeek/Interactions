using Shababeek.Interactions.Core;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// A pose that does not allow access to fingers individually.
    /// Plays a predefined animation clip without the ability to modify finger positions.
    /// </summary>
    /// <remarks>
    /// StaticPose is used for poses that have a fixed animation, such as gestures or
    /// specific hand positions that cannot be modified during runtime. It implements
    /// the IPose interface but ignores finger value changes.
    /// </remarks>
    [System.Serializable]
    public class StaticPose : IPose
    {
        /// <summary>
        /// Sets the finger value (ignored for static poses).
        /// </summary>
        /// <param name="index">The finger index (ignored)</param>
        /// <value>The finger value to set (ignored)</value>
        public float this[int index] { set { } }

        private AnimationClipPlayable _playable;
        private string _name;

        /// <summary>
        /// Gets the animation clip playable for this static pose.
        /// </summary>
        /// <returns>The animation clip playable</returns>
        public AnimationClipPlayable Mixer => _playable;
        
        /// <summary>
        /// Gets the name of this static pose.
        /// </summary>
        /// <returns>The pose name</returns>
        public string Name => _name;

        /// <summary>
        /// Initializes a new static pose with the given pose data.
        /// </summary>
        /// <param name="graph">The playable graph to create the animation in</param>
        /// <param name="poseData">The pose data containing the animation clip and name</param>
        public StaticPose(PlayableGraph graph, PoseData poseData)
        {
            _playable = AnimationClipPlayable.Create(graph, poseData.OpenAnimationClip);
            _name = poseData.Name;
        }
    }
}

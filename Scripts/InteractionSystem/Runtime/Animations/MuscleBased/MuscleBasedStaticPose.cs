using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Static pose driven by a single humanoid muscle snapshot. Captures the open clip at t=0
    /// and writes those finger muscle values every frame; finger weights are ignored.
    /// Side-agnostic: both hands' finger muscles are written from the same snapshot.
    /// </summary>
    /// <remarks>
    /// Static poses under the muscle-based system are single-frame snapshots, unlike the legacy
    /// StaticPose which plays an AnimationClip through a PlayableGraph over time. If a static pose
    /// needs to animate over time, the legacy pose system should be used for that HandData.
    /// </remarks>
    internal class MuscleBasedStaticPose : IPose
    {
        private readonly string _name;
        private readonly float[] _muscles;
        private readonly int[] _fingerMuscleIndices;

        /// <summary>Ignored for static poses.</summary>
        /// <param name="finger">Finger index (ignored).</param>
        /// <value>Curl value (ignored).</value>
        public float this[int finger] { set { } }

        /// <summary>Name of this pose.</summary>
        public string Name => _name;

        /// <summary>
        /// Builds a muscle-based static pose by sampling its open clip once at t=0.
        /// </summary>
        /// <param name="poseData">Pose definition; only the open clip is used.</param>
        /// <param name="animator">Humanoid animator the clip is sampled against.</param>
        public MuscleBasedStaticPose(PoseData poseData, Animator animator)
        {
            _name = poseData.Name;
            _muscles = HumanPoseSampler.SampleClipMuscles(animator, poseData.OpenAnimationClip);
            _fingerMuscleIndices = HumanPoseSampler.GetBothHandsFingerMuscleIndices();
        }

        /// <summary>
        /// Writes this pose's finger muscle values into the given HumanPose.
        /// Both hands' 40 finger muscles are written; all other muscles are preserved.
        /// </summary>
        /// <param name="pose">HumanPose whose muscles array will be mutated in place.</param>
        public void WriteTo(ref HumanPose pose)
        {
            if (pose.muscles == null || pose.muscles.Length < HumanTrait.MuscleCount) return;
            for (int i = 0; i < _fingerMuscleIndices.Length; i++)
            {
                int idx = _fingerMuscleIndices[i];
                if (idx < 0) continue;
                pose.muscles[idx] = _muscles[idx];
            }
        }
    }
}

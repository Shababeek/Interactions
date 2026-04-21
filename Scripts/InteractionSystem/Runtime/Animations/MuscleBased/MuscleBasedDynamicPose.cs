using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Dynamic pose driven by humanoid muscle values. Caches open and closed muscle snapshots at
    /// construction time and blends per finger using weights set through this[int]. Side-agnostic:
    /// the same weight drives the corresponding muscles on both hands so a single controller can
    /// constrain either or both hands on the rig.
    /// </summary>
    /// <remarks>
    /// Unlike DynamicPose, this class creates no PlayableGraph nodes. HandPoseController renders
    /// the hand by calling WriteTo once per frame and then HumanPoseHandler.SetHumanPose.
    /// Because snapshots are humanoid muscle values, clips authored on one humanoid rig work on any other.
    /// </remarks>
    internal class MuscleBasedDynamicPose : IPose
    {
        private const int FingersPerHand = 5;
        private const int MusclesPerFinger = 4;
        private const int MusclesPerFingerBothHands = MusclesPerFinger * 2;

        private readonly string _name;
        private readonly float[] _openMuscles;
        private readonly float[] _closedMuscles;
        private readonly int[] _fingerMuscleIndices;
        private readonly float[] _fingerWeights = new float[FingersPerHand];

        /// <summary>Sets the blend weight for a finger (0 = open, 1 = closed).</summary>
        /// <param name="finger">Finger index: 0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky.</param>
        /// <value>Curl value to apply to this finger on both hands.</value>
        public float this[int finger]
        {
            set
            {
                if (finger < 0 || finger >= _fingerWeights.Length) return;
                _fingerWeights[finger] = Mathf.Clamp01(value);
            }
        }

        /// <summary>Name of this pose.</summary>
        public string Name => _name;

        /// <summary>
        /// Builds a muscle-based dynamic pose by sampling its open and closed clips once.
        /// </summary>
        /// <param name="poseData">Pose definition holding the open and closed humanoid clips.</param>
        /// <param name="animator">Humanoid animator the clips are sampled against.</param>
        public MuscleBasedDynamicPose(PoseData poseData, Animator animator)
        {
            _name = poseData.Name;
            _openMuscles = HumanPoseSampler.SampleClipMuscles(animator, poseData.OpenAnimationClip);
            _closedMuscles = HumanPoseSampler.SampleClipMuscles(animator, poseData.ClosedAnimationClip);
            _fingerMuscleIndices = HumanPoseSampler.GetBothHandsFingerMuscleIndices();
        }

        /// <summary>
        /// Writes this pose's blended finger muscle values into the given HumanPose.
        /// Both hands' 40 finger muscles are driven; all other muscles are preserved.
        /// </summary>
        /// <param name="pose">HumanPose whose muscles array will be mutated in place.</param>
        public void WriteTo(ref HumanPose pose)
        {
            if (pose.muscles == null || pose.muscles.Length < HumanTrait.MuscleCount) return;

            for (int finger = 0; finger < _fingerWeights.Length; finger++)
            {
                // Finger weight convention: 0 = open, 1 = closed. The sampled muscle signs on
                // this rig come out inverted, so we blend from closed->open with the weight
                // directly instead of open->closed.
                float w = _fingerWeights[finger];
                int baseIdx = finger * MusclesPerFingerBothHands;
                for (int m = 0; m < MusclesPerFingerBothHands; m++)
                {
                    int idx = _fingerMuscleIndices[baseIdx + m];
                    if (idx < 0) continue;
                    pose.muscles[idx] = Mathf.Lerp(_closedMuscles[idx], _openMuscles[idx], w);
                }
            }
        }
    }
}

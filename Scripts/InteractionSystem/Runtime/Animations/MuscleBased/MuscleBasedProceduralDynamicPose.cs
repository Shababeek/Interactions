using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Dynamic pose driven by hardcoded open and closed muscle values. Used when a pose is
    /// configured without open/closed clips in muscle-based mode - gives every humanoid hand
    /// a reasonable neutral-to-fist gesture without requiring artist-authored clips.
    /// Side-agnostic: the same weight drives corresponding muscles on both hands.
    /// </summary>
    /// <remarks>
    /// "Open" is the T-pose neutral (all finger muscles at 0). "Closed" curls the three
    /// Stretched muscles per finger and leaves Spread at 0. The sign of the curl constant
    /// depends on Unity's humanoid convention - flip ClosedCurlValue if your rig curls backward.
    /// </remarks>
    internal class MuscleBasedProceduralDynamicPose : IPose
    {
        private const int FingersPerHand = 5;
        private const int MusclesPerFinger = 4;
        private const int MusclesPerFingerBothHands = MusclesPerFinger * 2;

        // Unity emits finger muscles as: [0]=N1 Stretched, [1]=Spread, [2]=N2 Stretched, [3]=N3 Stretched.
        // Open is the zero vector; closed curls the Stretched muscles with a negative value so
        // a finger weight of 1 collapses the digit into the palm on the Shababeek rigs.
        private const float OpenValue = 0f;
        private const float ClosedCurlValue = -1f;
        private const float ClosedSpreadValue = 0f;
        private const int SpreadLocalIndex = 1;

        private readonly string _name;
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
        /// Builds a procedural dynamic pose that needs no AnimationClips.
        /// </summary>
        /// <param name="name">Display name for this pose (usually the PoseData's name).</param>
        public MuscleBasedProceduralDynamicPose(string name)
        {
            _name = name;
            _fingerMuscleIndices = HumanPoseSampler.GetBothHandsFingerMuscleIndices();
        }

        /// <summary>
        /// Writes blended finger muscle values into the given HumanPose.
        /// Both hands' 40 finger muscles are driven; other muscles are preserved.
        /// </summary>
        /// <param name="pose">HumanPose whose muscles array will be mutated in place.</param>
        public void WriteTo(ref HumanPose pose)
        {
            if (pose.muscles == null || pose.muscles.Length < HumanTrait.MuscleCount) return;

            for (int finger = 0; finger < _fingerWeights.Length; finger++)
            {
                float w = _fingerWeights[finger];
                int baseIdx = finger * MusclesPerFingerBothHands;
                for (int m = 0; m < MusclesPerFingerBothHands; m++)
                {
                    int idx = _fingerMuscleIndices[baseIdx + m];
                    if (idx < 0) continue;
                    // Muscle-local index within a hand: 0..3. The Spread slot is shared between
                    // the two hands so we mod by MusclesPerFinger before checking it.
                    int localMuscle = m % MusclesPerFinger;
                    float closed = (localMuscle == SpreadLocalIndex) ? ClosedSpreadValue : ClosedCurlValue;
                    pose.muscles[idx] = Mathf.Lerp(OpenValue, closed, w);
                }
            }
        }
    }
}

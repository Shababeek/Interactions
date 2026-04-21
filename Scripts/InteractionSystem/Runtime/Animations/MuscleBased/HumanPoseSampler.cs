using Shababeek.Interactions.Core;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Utilities for sampling humanoid muscle values from AnimationClips and mapping both hands' finger muscles.
    /// </summary>
    /// <remarks>
    /// Used by the muscle-based pose system to convert humanoid AnimationClips into frozen muscle snapshots at load time.
    /// The snapshots are then interpolated per frame without keeping a PlayableGraph alive for finger animation.
    /// Sampling is side-agnostic: both LeftFingers and RightFingers body parts are unmasked so the clip's finger
    /// muscle values for either or both hands are captured in one pass.
    /// </remarks>
    internal static class HumanPoseSampler
    {
        private static readonly string[] UnityFingerNames = { "Thumb", "Index", "Middle", "Ring", "Little" };

        private const int FingersPerHand = 5;
        private const int MusclesPerFinger = 4;

        /// <summary>Finger muscle indices per hand (5 fingers * 4 muscles).</summary>
        public const int FingerMuscleCount = FingersPerHand * MusclesPerFinger;

        /// <summary>Finger muscle indices across both hands (10 fingers * 4 muscles).</summary>
        public const int BothHandsFingerMuscleCount = FingerMuscleCount * 2;

        /// <summary>
        /// Samples a humanoid AnimationClip at t=0 through a both-hands fingers-only AvatarMask and returns a copy of the full HumanPose.muscles array.
        /// </summary>
        /// <param name="animator">A humanoid Animator the clip will be sampled against. Its avatar must be Humanoid.</param>
        /// <param name="clip">The humanoid clip to sample. If null, a zero-filled array is returned.</param>
        /// <returns>A copy of HumanPose.muscles (HumanTrait.MuscleCount entries) after evaluating the clip at time 0.</returns>
        public static float[] SampleClipMuscles(Animator animator, AnimationClip clip)
        {
            if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
            {
                Debug.LogWarning("[HumanPoseSampler] Animator is null or not humanoid; returning zero muscle snapshot.");
                return new float[HumanTrait.MuscleCount];
            }
            if (clip == null)
            {
                return new float[HumanTrait.MuscleCount];
            }

            var graph = PlayableGraph.Create("HumanPoseSampler");
            HumanPoseHandler handler = null;
            AvatarMask mask = null;
            try
            {
                var clipPlayable = AnimationClipPlayable.Create(graph, clip);
                var mixer = AnimationLayerMixerPlayable.Create(graph, 1);
                mixer.ConnectInput(0, clipPlayable, 0);
                mixer.SetInputWeight(0, 1f);

                mask = BuildBothHandsFingersOnlyMask();
                mixer.SetLayerMaskFromAvatarMask(0, mask);

                var output = AnimationPlayableOutput.Create(graph, "Sample", animator);
                output.SetSourcePlayable(mixer);
                clipPlayable.SetTime(0);
                graph.Evaluate();

                handler = new HumanPoseHandler(animator.avatar, animator.transform);
                var pose = new HumanPose();
                handler.GetHumanPose(ref pose);

                var copy = new float[pose.muscles.Length];
                System.Array.Copy(pose.muscles, copy, pose.muscles.Length);
                return copy;
            }
            finally
            {
                handler?.Dispose();
                if (graph.IsValid()) graph.Destroy();
                if (mask != null) Object.DestroyImmediate(mask);
            }
        }

        private static AvatarMask BuildBothHandsFingersOnlyMask()
        {
            var mask = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
            {
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            }
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            return mask;
        }

        /// <summary>
        /// Returns muscle indices for one hand's finger muscles, ordered Thumb, Index, Middle, Ring, Pinky,
        /// with 4 muscles per finger in the order Unity emits them.
        /// </summary>
        /// <param name="side">Which hand to map. HandIdentifier.None returns an empty array.</param>
        /// <returns>A flat array of 20 indices into HumanPose.muscles. Missing muscles are filled with -1 and logged.</returns>
        public static int[] GetHandFingerMuscleIndices(HandIdentifier side)
        {
            if (side == HandIdentifier.None)
                return new int[0];

            var indices = new int[FingerMuscleCount];
            string sidePrefix = side == HandIdentifier.Left ? "Left" : "Right";

            int outIdx = 0;
            for (int f = 0; f < FingersPerHand; f++)
            {
                string fingerPrefix = sidePrefix + " " + UnityFingerNames[f] + " ";
                int found = 0;
                for (int m = 0; m < HumanTrait.MuscleCount && found < MusclesPerFinger; m++)
                {
                    if (HumanTrait.MuscleName[m].StartsWith(fingerPrefix))
                    {
                        indices[outIdx++] = m;
                        found++;
                    }
                }
                if (found != MusclesPerFinger)
                {
                    Debug.LogWarning($"[HumanPoseSampler] Expected {MusclesPerFinger} muscles for '{fingerPrefix.Trim()}', found {found}.");
                    while (found < MusclesPerFinger)
                    {
                        indices[outIdx++] = -1;
                        found++;
                    }
                }
            }
            return indices;
        }

        /// <summary>
        /// Returns finger muscle indices for both hands, grouped per-finger: for each finger f (0..4),
        /// 4 left-hand muscles followed by 4 right-hand muscles starting at offset f * 8.
        /// </summary>
        /// <returns>A flat array of 40 indices into HumanPose.muscles. Missing muscles are filled with -1.</returns>
        public static int[] GetBothHandsFingerMuscleIndices()
        {
            var left = GetHandFingerMuscleIndices(HandIdentifier.Left);
            var right = GetHandFingerMuscleIndices(HandIdentifier.Right);
            var combined = new int[BothHandsFingerMuscleCount];
            for (int f = 0; f < FingersPerHand; f++)
            {
                for (int m = 0; m < MusclesPerFinger; m++)
                {
                    combined[f * MusclesPerFinger * 2 + m] = left[f * MusclesPerFinger + m];
                    combined[f * MusclesPerFinger * 2 + MusclesPerFinger + m] = right[f * MusclesPerFinger + m];
                }
            }
            return combined;
        }
    }
}

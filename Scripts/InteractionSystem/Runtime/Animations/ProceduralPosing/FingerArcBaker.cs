using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Baked fingertip trajectories for one hand pose: hand-local tip and mid positions sampled
    /// across the 0-1 curl range. Bake once per hand/pose, solve many times.
    /// </summary>
    public class FingerArcs
    {
        /// <summary>A single curl sample along one finger's arc.</summary>
        public struct Sample
        {
            /// <summary>Hand-local fingertip position at this curl value.</summary>
            public Vector3 tip;

            /// <summary>Hand-local mid-phalanx position at this curl value (equals tip when no mid bone).</summary>
            public Vector3 mid;
        }

        private readonly Sample[][] _samples;
        private readonly float[] _radii;

        /// <summary>Number of curl samples per finger.</summary>
        public int SampleCount { get; }

        /// <summary>Creates baked arcs from per-finger sample arrays and cast radii.</summary>
        public FingerArcs(Sample[][] samples, float[] radii, int sampleCount)
        {
            _samples = samples;
            _radii = radii;
            SampleCount = sampleCount;
        }

        /// <summary>Returns the sample for a finger (0=Thumb..4=Pinky) at sample index k.</summary>
        public Sample GetSample(int finger, int k) => _samples[finger][k];

        /// <summary>Cast radius for the given finger.</summary>
        public float GetRadius(int finger) => _radii[finger];

        /// <summary>Curl value (0-1) corresponding to sample index k.</summary>
        public float CurlAt(int k) => SampleCount <= 1 ? 0f : (float)k / (SampleCount - 1);
    }

    /// <summary>
    /// Samples a HandPoseController across the curl range and records hand-local fingertip arcs.
    /// </summary>
    public static class FingerArcBaker
    {
        /// <summary>
        /// Bakes finger arcs for the controller's given pose. Temporarily drives finger weights
        /// and evaluates the pose out-of-band; restores the original state before returning.
        /// </summary>
        /// <param name="controller">Hand pose controller with an initialized graph.</param>
        /// <param name="rig">Finger bone references on the same hand.</param>
        /// <param name="poseIndex">Pose to bake (the constraint's targetPoseIndex).</param>
        /// <param name="sampleCount">Curl samples per finger; 12 is a good default.</param>
        public static FingerArcs Bake(HandPoseController controller, HandFingerRig rig, int poseIndex, int sampleCount = 12)
        {
            if (controller == null || rig == null || !rig.IsValid)
            {
                Debug.LogError("[FingerArcBaker] Controller or finger rig missing/invalid.");
                return null;
            }

            var savedWeights = new float[5];
            for (int i = 0; i < 5; i++) savedWeights[i] = controller[i];
            int savedPose = controller.CurrentPoseIndex;

            controller.CurrentPoseIndex = poseIndex;

            var root = controller.transform;
            var samples = new FingerArcs.Sample[5][];
            var radii = new float[5];
            for (int f = 0; f < 5; f++)
            {
                samples[f] = new FingerArcs.Sample[sampleCount];
                radii[f] = rig[f].radius;
            }

            for (int k = 0; k < sampleCount; k++)
            {
                float t = sampleCount <= 1 ? 0f : (float)k / (sampleCount - 1);
                for (int f = 0; f < 5; f++) controller[f] = t;
                controller.EvaluatePoseImmediate();

                for (int f = 0; f < 5; f++)
                {
                    var bones = rig[f];
                    Vector3 tip = root.InverseTransformPoint(bones.tip.position);
                    Vector3 mid = bones.mid ? root.InverseTransformPoint(bones.mid.position) : tip;
                    samples[f][k] = new FingerArcs.Sample { tip = tip, mid = mid };
                }
            }

            for (int i = 0; i < 5; i++) controller[i] = savedWeights[i];
            controller.CurrentPoseIndex = savedPose;
            controller.EvaluatePoseImmediate();

            return new FingerArcs(samples, radii, sampleCount);
        }
    }
}

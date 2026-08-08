using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Caches baked finger arcs per hand controller and pose so grab-time fitting only pays the
    /// bake cost once per hand/pose combination.
    /// </summary>
    public static class FingerArcCache
    {
        private static readonly Dictionary<(int controllerId, int poseIndex, int sampleCount), FingerArcs> Cache = new();

        /// <summary>Returns the cached arcs for the controller and pose, baking them on first request.</summary>
        public static FingerArcs Get(HandPoseController controller, HandFingerRig rig, int poseIndex, int sampleCount = 12)
        {
            if (controller == null || rig == null) return null;

            var key = (controller.GetInstanceID(), poseIndex, sampleCount);
            if (Cache.TryGetValue(key, out var arcs)) return arcs;

            arcs = FingerArcBaker.Bake(controller, rig, poseIndex, sampleCount);
            if (arcs != null) Cache[key] = arcs;
            return arcs;
        }

        /// <summary>Clears every cached arc. Call when hand rigs or pose data change at runtime.</summary>
        public static void Clear() => Cache.Clear();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayMode() => Clear();
#endif
    }
}

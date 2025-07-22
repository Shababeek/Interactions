
using UnityEngine;
using UnityEngine.Serialization;

namespace Shababeek.Interactions.Core
{
    [System.Serializable]
    /// <summary>
    /// Represents constraints for hand poses in the interaction system.
    /// </summary>
    public struct HandConstraints
    {
        public PoseConstrains poseConstrains;
        public Transform relativeTransform;
    }

}


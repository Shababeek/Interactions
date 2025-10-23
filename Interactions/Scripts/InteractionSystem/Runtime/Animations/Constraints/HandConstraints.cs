
using UnityEngine;
using UnityEngine.Serialization;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Represents constraints for hand poses in the interaction system.
    /// </summary>
    [System.Serializable]
    public struct HandConstraints
    {
        public PoseConstrains poseConstrains;
        public Transform relativeTransform;
    }

}


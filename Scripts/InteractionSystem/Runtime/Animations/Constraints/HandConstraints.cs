
using UnityEngine;
using UnityEngine.Serialization;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Constraints for hand poses.
    /// </summary>
    [System.Serializable]
    public struct HandConstraints
    {
        public PoseConstrains poseConstrains;
        public Transform relativeTransform;
    }

}


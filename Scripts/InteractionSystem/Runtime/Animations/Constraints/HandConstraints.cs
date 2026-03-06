
using UnityEngine;
using UnityEngine.Serialization;

namespace Shababeek.Interactions.Core
{
    /// <summary>Constraints for a hand pose including finger and transform constraints.</summary>
    [System.Serializable]
    public struct HandConstraints
    {
        /// <summary>Pose constraints for fingers.</summary>
        public PoseConstrains poseConstrains;
        /// <summary>Relative transform for hand positioning.</summary>
        public Transform relativeTransform;
    }

}


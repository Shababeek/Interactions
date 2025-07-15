
using UnityEngine;
using UnityEngine.Serialization;

namespace Shababeek.Interactions.Core
{
    [System.Serializable]
    public struct HandConstraints
    {
        public PoseConstrains poseConstrains;
        public Transform relativeTransform;
    }

}


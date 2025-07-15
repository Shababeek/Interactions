
using UnityEngine;
using UnityEngine.Serialization;

namespace Kandooz.InteractionSystem.Core
{
    [System.Serializable]
    public struct HandConstraints
    {
        public PoseConstrains poseConstrains;
        public Transform relativeTransform;
    }

}


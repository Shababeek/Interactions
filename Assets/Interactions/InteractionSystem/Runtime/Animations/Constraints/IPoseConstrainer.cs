using Kandooz.InteractionSystem.Core;
using UnityEngine;

namespace Kandooz.InteractionSystem.Animations.Constraints
{
    public interface IPoseConstrainer
    {
        PoseConstrains LeftPoseConstrains { get; }
        PoseConstrains RightPoseConstrains { get; }
        Transform LeftHandTransform { get; set; }
        Transform RightHandTransform { get; set; }
        Transform PivotParent { get;  }
        bool HasChanged { get;  }
        void UpdatePivots();
        void Initialize();
    }
}
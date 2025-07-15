using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Animations.Constraints
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
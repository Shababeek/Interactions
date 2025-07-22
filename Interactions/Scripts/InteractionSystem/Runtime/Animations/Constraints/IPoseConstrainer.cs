using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Animations.Constraints
{
    /// <summary>
    /// Interface for constraining hand poses in the interaction system.
    /// </summary>
    /// <remarks>
    /// This interface defines the properties and methods required for constraining hand poses,
    /// including left and right pose constraints, hand transforms, and pivot parent.
    /// It is used by the InteractionPoseConstrainer to manage hand poses during interactions.
    /// </remarks>
    /// <seealso cref="InteractionPoseConstrainer"/>
    /// <seealso cref="HandConstraints"/>
    /// <seealso cref="PoseConstrains"/>
    public interface IPoseConstrainer
    {
        PoseConstrains LeftPoseConstrains { get; }
        PoseConstrains RightPoseConstrains { get; }
        Transform LeftHandTransform { get; set; }
        Transform RightHandTransform { get; set; }
        Transform PivotParent { get; }
        bool HasChanged { get; }
        void UpdatePivots();
        void Initialize();
    }
}
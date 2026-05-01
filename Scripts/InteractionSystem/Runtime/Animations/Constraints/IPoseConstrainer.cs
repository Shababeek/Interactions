using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Animations.Constraints
{
    /// <summary>Interface for constraining hand poses during interactions.</summary>
    public interface IPoseConstrainer
    {
        /// <summary>Gets the pose constraints for the left hand.</summary>
        PoseConstrains LeftPoseConstrains { get; }
        /// <summary>Gets the pose constraints for the right hand.</summary>
        PoseConstrains RightPoseConstrains { get; }
        /// <summary>Gets whether the constraints have changed.</summary>
        bool HasChanged { get; }

        /// <summary>Applies pose constraints to the hand.</summary>
        void ApplyConstraints(Hand interactor);

        /// <summary>Removes pose constraints from the hand.</summary>
        void RemoveConstraints(Hand interactor);

        /// <summary>Gets the type of constraint to apply during interaction.</summary>
        HandConstrainType ConstraintType { get; }

        /// <summary>Gets whether to use smooth transitions when applying constraints.</summary>
        bool UseSmoothTransitions { get; }

        /// <summary>Gets the speed of smooth transitions.</summary>
        float TransitionSpeed { get; }

    }
}
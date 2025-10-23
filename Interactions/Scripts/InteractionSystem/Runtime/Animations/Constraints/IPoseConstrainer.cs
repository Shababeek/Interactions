using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Animations.Constraints
{
    /// <summary>
    /// Interface for constraining hand poses.
    /// It is used by the InteractionPoseConstrainer to manage hand poses during interactions.
    /// </summary>
    public interface IPoseConstrainer
    {
        PoseConstrains LeftPoseConstrains { get; }
        PoseConstrains RightPoseConstrains { get; }
        Transform LeftHandTransform { get; set; }
        Transform RightHandTransform { get; set; }
        Transform PivotParent { get; }
        bool HasChanged { get; }
        
        /// <summary>
        /// Applies pose constraints to the hand.
        /// </summary>
        void ApplyConstraints(Hand interactor);
        
        /// <summary>
        /// Removes pose constraints from the hand.
        /// </summary>
        void RemoveConstraints(Hand interactor);
        
        /// <summary>
        /// Type of constraint to apply during interaction.
        /// </summary>
        HandConstrainType ConstraintType { get; }
        
        /// <summary>
        /// Whether to use smooth transitions when applying constraints.
        /// </summary>
        bool UseSmoothTransitions { get; }
        
        /// <summary>
        /// Speed of smooth transitions.
        /// </summary>
        float TransitionSpeed { get; }
        
        void UpdatePivots();
        void Initialize();
    }
}
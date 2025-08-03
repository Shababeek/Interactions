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
        
        /// <summary>
        /// Applies pose constraints to the specified interactor's hand.
        /// This method handles all constraint types (Hide, Free, Constrained) and smooth transitions.
        /// </summary>
        /// <param name="interactor">The interactor whose hand should be constrained.</param>
        void ApplyConstraints(Hand interactor);
        
        /// <summary>
        /// Removes all pose constraints from the specified interactor's hand.
        /// This method restores the hand to its default state.
        /// </summary>
        /// <param name="interactor">The interactor whose hand should be unconstrained.</param>
        void RemoveConstraints(Hand interactor);
        
        /// <summary>
        /// The type of constraint to apply to hands during interaction.
        /// </summary>
        HandConstrainType ConstraintType { get; }
        
        /// <summary>
        /// Whether to use smooth transitions when applying transform constraints.
        /// </summary>
        bool UseSmoothTransitions { get; }
        
        /// <summary>
        /// The speed of smooth transitions when applying transform constraints.
        /// </summary>
        float TransitionSpeed { get; }
        
        void UpdatePivots();
        void Initialize();
    }
}
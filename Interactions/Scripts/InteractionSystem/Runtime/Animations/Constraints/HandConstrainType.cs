namespace Shababeek.Interactions.Animations.Constraints
{
    /// <summary>
    /// Defines the different types of hand constraints that can be applied during interactions.
    /// </summary>
    public enum HandConstrainType
    {
        /// <summary>
        /// Hides the hand model during interaction.
        /// Useful when the hand should not be visible, such as when holding small objects.
        /// </summary>
        HideHand,
        
        /// <summary>
        /// Allows the hand to move freely without any pose constraints.
        /// The hand maintains its natural movement and pose during interaction.
        /// </summary>
        FreeHand,
        
        /// <summary>
        /// Applies specific pose constraints to the hand during interaction.
        /// The hand is positioned and posed according to the configured constraints.
        /// </summary>
        Constrained
    }
} 
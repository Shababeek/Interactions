namespace Shababeek.Interactions.Animations.Constraints
{
    /// <summary>
    /// Types of hand constraints during interactions.
    /// </summary>
    public enum HandConstrainType
    {
        /// <summary>
        /// Hides the hand model.
        /// </summary>
        HideHand,
        
        /// <summary>
        /// Allows free hand movement without constraints.
        /// </summary>
        FreeHand,
        
        /// <summary>
        /// Applies specific pose constraints to the hand.
        /// </summary>
        Constrained
    }
} 
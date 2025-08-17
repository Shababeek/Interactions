namespace Shababeek.Interactions
{
    /// <summary>
    /// Represents the different states of interaction in the interaction system.
    /// </summary>
    public enum InteractionState
    {
        /// <summary>
        /// No interaction is currently active with this object.
        /// </summary>
        None,
        
        /// <summary>
        /// An interactor is hovering over this object but has not selected it yet.
        /// </summary>
        Hovering,
        
        /// <summary>
        /// An interactor has selected this object and can perform actions on it.
        /// </summary>
        Selected,
    }
}
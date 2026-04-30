namespace Shababeek.Interactions
{
    /// <summary>
    /// Local axis around which a rotary interactable rotates.
    /// </summary>
    public enum RotationAxis
    {
        Right,
        Up,
        Forward
    }

    /// <summary>
    /// How the hand and rotary object are positioned at grab time when using the HandPosition control scheme.
    /// </summary>
    public enum WheelGrabMode
    {
        ObjectFollowsHand,
        HandFollowsObject
    }

    /// <summary>
    /// How rotation input is gathered for a rotary interactable.
    /// </summary>
    public enum RotaryControlScheme
    {
        /// <summary>Rotation is driven by the hand's position around the pivot (large wheels, big dials).</summary>
        HandPosition,
        /// <summary>Rotation is driven by the wrist's twist around the rotation axis (small dials, knobs).</summary>
        HandRotation
    }
}

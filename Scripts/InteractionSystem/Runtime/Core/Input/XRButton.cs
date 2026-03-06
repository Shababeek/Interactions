
namespace Shababeek.Interactions.Core
{
    /// <summary>VR button types supported by the interaction system.</summary>
    public enum XRButton
    {
        /// <summary>Trigger button.</summary>
        Trigger,
        /// <summary>Grip button.</summary>
        Grip,
        /// <summary>Any button state.</summary>
        Any,
    }

    /// <summary>VR button state (up or down).</summary>
    public enum VRButtonState
    {
        /// <summary>Button is not pressed.</summary>
        Up,
        /// <summary>Button is pressed.</summary>
        Down,
    }
}
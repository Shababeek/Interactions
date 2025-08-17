using System.Collections.Generic;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Interface for pose objects that can control finger animations.
    /// Defines the contract for both static and dynamic poses used by the HandPoseController.
    /// </summary>
    /// <remarks>
    /// This interface is implemented by both StaticPose and DynamicPose classes.
    /// It provides a unified way to access and control finger values regardless of the pose type.
    /// </remarks>
    public interface IPose
    {
        /// <summary>
        /// Access the finger of the pose by its ID.
        /// </summary>
        /// <param name="finger">Finger ID: 0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky</param>
        /// <value>The finger curl value to set (0 = extended, 1 = curled)</value>
        float this[int finger] { set; }
        
        /// <summary>
        /// Gets the name of this pose.
        /// </summary>
        /// <returns>The pose name</returns>
        string Name { get; }
    }
}

using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Grab strategy implementation for objects without Rigidbody components.
    /// Handles transform-based grabbing for static or kinematic objects.
    /// </summary>
    /// <remarks>
    /// This strategy is ideal for objects that don't need physics behavior,
    /// such as UI elements, static objects, or objects that should maintain
    /// their exact position when released.
    /// </remarks>
    internal class TransformGrabStrategy : GrabStrategy
    {
        private readonly Transform transform;

        /// <summary>
        /// Initializes a new instance of the TransformGrabStrategy class.
        /// </summary>
        /// <param name="transform">The Transform component of the object to be grabbed</param>
        public TransformGrabStrategy(Transform transform) : base(transform.gameObject)
        {
            this.transform = transform;
        }
        
        /// <inheritdoc/>
        public override void UnGrab(Grabable interactable, InteractorBase interactor)
        {
            base.UnGrab(interactable, interactor);
            transform.parent = null;
        }
    }
}
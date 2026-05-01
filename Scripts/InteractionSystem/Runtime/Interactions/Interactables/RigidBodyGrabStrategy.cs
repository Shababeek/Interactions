using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Grab strategy for objects with a Rigidbody. Makes the body kinematic while held
    /// and restores its prior kinematic state on release so designer-set kinematic bodies
    /// keep their behavior.
    /// </summary>
    public class RigidBodyGrabStrategy : GrabStrategy
    {
        private readonly Rigidbody body;
        private bool _wasKinematic;

        /// <summary>
        /// Initializes a new instance of the RigidBodyGrabStrategy class.
        /// </summary>
        /// <param name="body">The Rigidbody component of the object to be grabbed.</param>
        public RigidBodyGrabStrategy(Rigidbody body) : base(body.gameObject)
        {
            this.body = body;
        }

        /// <inheritdoc/>
        protected override void InitializeStep()
        {
            _wasKinematic = body.isKinematic;
            body.isKinematic = true;
        }

        /// <inheritdoc/>
        public override void UnGrab(Grabable interactable, InteractorBase interactor)
        {
            base.UnGrab(interactable, interactor);
            body.isKinematic = _wasKinematic;
        }
    }
}

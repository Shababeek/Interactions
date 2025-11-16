using System.Threading.Tasks;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Grab strategy implementation for objects with Rigidbody components.
    /// Handles physics-based grabbing by temporarily making the rigidbody kinematic.
    /// </summary>
    /// <remarks>
    /// This strategy is ideal for objects that need to maintain physics behavior
    /// when released, such as throwable objects or objects that should fall naturally.
    /// </remarks>
    public class RigidBodyGrabStrategy : GrabStrategy
    {
        private readonly Rigidbody body;
        private bool grabbed = false;
        
        /// <summary>
        /// Initializes a new instance of the RigidBodyGrabStrategy class.
        /// </summary>
        /// <param name="body">The Rigidbody component of the object to be grabbed</param>
        public RigidBodyGrabStrategy(Rigidbody body) : base(body.gameObject)
        {
            this.body = body;
        }

        /// <summary>
        /// Initializes the rigidbody for grabbing by making it kinematic.
        /// This prevents physics interactions while the object is being held.
        /// </summary>
        protected override void InitializeStep() => body.isKinematic = true;
        /// <inheritdoc/>
        public override async void UnGrab(Grabable interactable, InteractorBase interactor)
        {
            base.UnGrab(interactable, interactor);
            body.isKinematic = false;
            await Task.Delay(10);
            grabbed = false;
        }
    }
}
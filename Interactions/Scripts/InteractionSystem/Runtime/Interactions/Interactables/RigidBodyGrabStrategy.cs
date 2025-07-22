using System.Threading.Tasks;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions;

namespace Shababeek.Interactions
{
    public class RigidBodyGrabStrategy : GrabStrategy
    {
        private readonly Rigidbody body;
        private bool grabbed = false;
        public RigidBodyGrabStrategy(Rigidbody body) :base(body.gameObject)
        {
            this.body = body;
        }

        protected override void InitializeStep() => body.isKinematic = true;
        
        public override async void UnGrab(Grabable interactable, InteractorBase interactor)
        {
            base.UnGrab(interactable, interactor);
            body.isKinematic = false;
            await Task.Delay(10);
            grabbed = false;
        }
    }
}
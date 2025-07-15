using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{

    internal class TransformGrabStrategy : GrabStrategy
    {
        private readonly Transform transform;

        public TransformGrabStrategy(Transform transform): base(transform.gameObject)
        {
            this.transform = transform;
        }
        

        public override void UnGrab(Grabable interactable, InteractorBase interactor)
        {
            base.UnGrab(interactable, interactor);
            transform.parent = null;
        }
    }
}
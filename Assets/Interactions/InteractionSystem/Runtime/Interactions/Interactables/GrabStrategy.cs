using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    public abstract class GrabStrategy
    {
        protected GameObject gameObject;
        private readonly Collider[] colliders;
        private readonly int[] collisionLayers;
        private int layer;

        protected GrabStrategy(GameObject gameObject)
        {
            this.gameObject = gameObject;
            layer = gameObject.layer;
            colliders = gameObject.GetComponentsInChildren<Collider>();
            collisionLayers = new int [colliders.Length];
            for (int i = 0; i < collisionLayers.Length; i++)
            {
                collisionLayers[i] = colliders[i].gameObject.layer;
            }
        }

        public void Initialize(InteractorBase interactor)
        {
            foreach (var collider in colliders)
            {
                collider.gameObject.layer = interactor.gameObject.layer;
            }

            gameObject.layer = interactor.gameObject.layer;
            InitializeStep();
        }

        protected virtual void InitializeStep()
        {
            
        }

        public virtual void Grab(Grabable interactable, InteractorBase interactor)
        {
            var transform = interactable.transform;

            transform.parent = interactor.AttachmentPoint;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public virtual void UnGrab(Grabable interactable, InteractorBase interactor)
        {
            interactable.transform.parent = null;
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].gameObject.layer = collisionLayers[i];
            }
            gameObject.layer = layer;
        }
    }
}
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Abstract base class for implementing different grab strategies.
    /// Defines how objects are grabbed, positioned, and released by interactors.
    /// </summary>
    /// <remarks>
    /// This class handles the core grab mechanics including layer management,
    /// transform parenting, and collision handling. Different strategies can be
    /// implemented for various object types (rigidbody, transform-based, etc.).
    /// </remarks>
    public abstract class GrabStrategy
    {
        protected GameObject gameObject;
        private readonly Collider[] colliders;
        private readonly int[] collisionLayers;
        private int layer;

        /// <summary>
        /// Initializes a new instance of the GrabStrategy class.
        /// </summary>
        /// <param name="gameObject">The GameObject that will be grabbed</param>
        protected GrabStrategy(GameObject gameObject)
        {
            this.gameObject = gameObject;
            colliders = gameObject.GetComponentsInChildren<Collider>();
            collisionLayers = new int[colliders.Length];
           
        }
        
        /// <summary>
        /// Initializes the GrabStrategy for a specific interactor.
        /// Sets up layer management and calls the strategy-specific initialization.
        /// </summary>
        /// <param name="interactor">The interactor that will grab this object</param>
        public void Initialize(InteractorBase interactor)
        {
            layer = gameObject.layer;
            for (int i = 0; i < collisionLayers.Length; i++)
            {
                collisionLayers[i] = colliders[i].gameObject.layer;
            }
            foreach (var collider in colliders)
            {
                collider.gameObject.layer = interactor.gameObject.layer;
            }

            gameObject.layer = interactor.gameObject.layer;
            InitializeStep();
        }

        /// <summary>
        /// Strategy-specific initialization step.
        /// Override this method to implement custom initialization logic.
        /// </summary>
        protected virtual void InitializeStep()
        {
            
        }

        /// <summary>
        /// Performs the grab action, attaching the object to the interactor.
        /// </summary>
        /// <param name="interactable">The grabable object being grabbed</param>
        /// <param name="interactor">The interactor performing the grab</param>
        public virtual void Grab(Grabable interactable, InteractorBase interactor)
        {
            var transform = interactable.transform;

            transform.parent = interactor.AttachmentPoint;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Performs the ungrab action, detaching the object from the interactor.
        /// Restores the original layer settings and transform hierarchy.
        /// </summary>
        /// <param name="interactable">The grabable object being released</param>
        /// <param name="interactor">The interactor releasing the object</param>
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
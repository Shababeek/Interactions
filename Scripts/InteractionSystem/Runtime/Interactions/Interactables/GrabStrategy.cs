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
    /// This class handles core grab mechanics such as transform parenting and
    /// release behavior. Layer management is handled by InteractableBase so
    /// strategies can focus only on attachment semantics.
    /// </remarks>
    public abstract class GrabStrategy
    {
        protected GameObject gameObject;

        /// <summary>
        /// Initializes a new instance of the GrabStrategy class.
        /// </summary>
        /// <param name="gameObject">The GameObject that will be grabbed</param>
        protected GrabStrategy(GameObject gameObject)
        {
            this.gameObject = gameObject;
           
        }
        
        /// <summary>
        /// Initializes the GrabStrategy for a specific interactor.
        /// Calls the strategy-specific initialization step.
        /// </summary>
        /// <param name="interactor">The interactor that will grab this object</param>
        public void Initialize(InteractorBase interactor)
        {
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
        }
    }
}
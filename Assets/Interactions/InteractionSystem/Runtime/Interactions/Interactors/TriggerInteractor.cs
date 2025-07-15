
using Kandooz.Interactions;
using Kandooz.InteractionSystem.Core;
using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    public class TriggerInteractor : InteractorBase
    {
        //TODO: rewrite to make it support pairs of colliders/interactables
        [ReadOnly][SerializeField] private Collider currentCollider;
        private int frameCounter;
        private void OnTriggerEnter(Collider other)
        {
            if (isInteracting) return;
            var interactable = other.GetComponentInParent<InteractableBase>();
            if (!interactable || interactable == currentInteractable) return;
            if (!ShouldChangeInteractable( interactable)) return;
            ChangeInteractable(interactable);
            currentCollider = other;
        }

        private void ChangeInteractable( InteractableBase interactable)
        {
            try { if (currentInteractable) OnHoverEnd(); } catch { }
            currentInteractable = interactable;
            try { if (currentInteractable) OnHoverStart(); } catch { };
        }

        private bool ShouldChangeInteractable(InteractableBase interactable )
        {
            return true;
            if(currentInteractable == null) return true;
            var interactableDistance = (transform.position - interactable.transform.position).sqrMagnitude;
            var currentInteractableDistance = (transform.position - currentInteractable.transform.position).sqrMagnitude;
            return currentInteractableDistance > interactableDistance;
        }

        private void OnTriggerExit(Collider other)
        {
            if(IsInteracting) { return; }
            if (other == currentCollider)
            {
                ChangeInteractable(null);
                return;
            }
            var interactable = other.GetComponent<InteractableBase>();
            
            if (currentInteractable != null && currentInteractable.CurrentState == InteractionState.Selected) return;
            if (interactable == currentInteractable)
            {
                    ChangeInteractable(null);
            }
        }
        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();
            currentCollider = null;
        }
    }
}
using Shababeek.Core;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// This class is used to handle the trigger input for the interactable.
    /// </summary>
    
    public class TriggerInteractor : InteractorBase
    {
        //TODO: rewrite to make it support pairs of colliders/interactables
        [ReadOnly][SerializeField] private Collider currentCollider;
        
        // Cache for performance and to avoid hierarchy issues
        private Vector3 lastInteractionPoint;
        private float lastDistanceCheck;
        private const float DISTANCE_CHECK_INTERVAL = 0.05f; // Check distance every 0.1 seconds
        
        private void OnTriggerEnter(Collider other)
        {
            if (isInteracting) return;
            var interactable = other.GetComponentInParent<InteractableBase>();
            if (!interactable || interactable == currentInteractable) return;
            if (!ShouldChangeInteractable(interactable)) return;
            ChangeInteractable(interactable);
            currentCollider = other;
        }

        private void ChangeInteractable(InteractableBase interactable)
        {
            try { if (currentInteractable) OnHoverEnd(); }
            catch
            {
                // ignored
            }

            currentInteractable = interactable;
            try { if (currentInteractable) OnHoverStart(); }
            catch
            {
                // ignored
            }
        }

        private bool ShouldChangeInteractable(InteractableBase interactable)
        {
            if (currentInteractable == null) return true;
            
            // Performance optimization: only check distance periodically
            if (Time.time - lastDistanceCheck < DISTANCE_CHECK_INTERVAL)
                return false;
                
            lastDistanceCheck = Time.time;
            
            // Use interaction points instead of transform positions to avoid hierarchy issues
            Vector3 newInteractionPoint = GetInteractionPoint(interactable);
            Vector3 currentInteractionPoint = GetInteractionPoint(currentInteractable);
            Vector3 interactorPosition = transform.position;
            
            float newDistance = Vector3.SqrMagnitude(interactorPosition - newInteractionPoint);
            float currentDistance = Vector3.SqrMagnitude(interactorPosition - currentInteractionPoint);
            
            return newDistance < currentDistance;
        }
        
        private Vector3 GetInteractionPoint(InteractableBase interactable)
        {
            if (interactable == null) return Vector3.zero;
            
            // Try to find a dedicated interaction point first
            var interactionPoint = interactable.InteractionPoint;
            if (interactionPoint != null)
                return interactionPoint.position;
            
            // Fallback: use the closest point on the collider bounds
            var collider = interactable.GetComponent<Collider>();
            if (collider != null)
                return collider.ClosestPoint(transform.position);
            
            // Last resort: use transform position
            return interactable.transform.position;
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsInteracting) { return; }
            if (other == currentCollider)
            {
                ChangeInteractable(null);
                return;
            }
            var interactable = other.GetComponentInParent<InteractableBase>();

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
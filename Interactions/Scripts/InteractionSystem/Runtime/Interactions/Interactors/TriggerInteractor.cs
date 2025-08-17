using Shababeek.Core;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// TriggerInteractor is a type of Interactor that uses Unity's trigger collider system to detect interactions with interactable objects.
    /// It allows for hover and selection interactions based on trigger events.
    /// This interactor is designed to work with colliders and does not require direct input from the user.
    /// </summary>
    public class TriggerInteractor : InteractorBase
    {
        //TODO: rewrite to make it support pairs of colliders/interactables
        [ReadOnly][SerializeField] private Collider currentCollider;
        
        // Cache for performance and to avoid hierarchy issues
        private Vector3 _lastInteractionPoint;
        private float _lastDistanceCheck;
        private const float DistanceCheckInterval = 0.08f; 
        private void OnTriggerEnter(Collider other)
        {
            if (isInteracting) return;
            var interactable = other.GetComponentInParent<InteractableBase>();
            if (!interactable || interactable == CurrentInteractable) return;
            if (!ShouldChangeInteractable(interactable)) return;
            ChangeInteractable(interactable);
            currentCollider = other;
        }

        private void ChangeInteractable(InteractableBase interactable)
        {
            try { if (CurrentInteractable) OnHoverEnd(); }
            catch
            {
                // ignored
            }

            CurrentInteractable = interactable;
            try { if (CurrentInteractable) OnHoverStart(); }
            catch
            {
                // ignored
            }
        }

        private bool ShouldChangeInteractable(InteractableBase interactable)
        {
            if (CurrentInteractable == null) return true;
            
            // Performance optimization: only check distance periodically
            if (Time.time - _lastDistanceCheck < DistanceCheckInterval)
                return false;
                
            _lastDistanceCheck = Time.time;
            
            // Use interaction points instead of transform positions to avoid hierarchy issues
            Vector3 newInteractionPoint = GetInteractionPoint(interactable);
            Vector3 currentInteractionPoint = GetInteractionPoint(CurrentInteractable);
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

            if (CurrentInteractable != null && CurrentInteractable.CurrentState == InteractionState.Selected) return;
            if (interactable == CurrentInteractable)
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
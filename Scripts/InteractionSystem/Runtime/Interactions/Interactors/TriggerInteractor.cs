using Shababeek.Utilities;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Interactor using trigger colliders to detect interactions.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactors/Trigger Interactor")]
    public class TriggerInteractor : InteractorBase
    {
        [Header("Runtime State")]
        [ReadOnly][SerializeField] [Tooltip("The collider currently being interacted with.")]
        private Collider currentCollider;

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
            if (CurrentInteractable)
            {
                try 
                { 
                    EndHover(); 
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error ending hover on {CurrentInteractable.name}: {e.Message}", CurrentInteractable);
                }
            }

            CurrentInteractable = interactable;

            if (!CurrentInteractable) return;
            {
                try 
                { 
                    StartHover(); 
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error starting hover on {CurrentInteractable.name}: {e.Message}", CurrentInteractable);
                    CurrentInteractable = null;
                }
            }
        }

        private bool ShouldChangeInteractable(InteractableBase interactable)
        {
            if (CurrentInteractable == null) return true;
            
            if (Time.time - _lastDistanceCheck < DistanceCheckInterval)
                return false;
            if (!CurrentInteractable.CanInteract(Hand))
            {
                return false;
            }
            _lastDistanceCheck = Time.time;
            
            Vector3 newInteractionPoint = GetInteractionPoint(interactable);
            Vector3 currentInteractionPoint = GetInteractionPoint(CurrentInteractable);
            Vector3 interactorPosition = transform.position;
            
            float newDistance = Vector3.SqrMagnitude(interactorPosition - newInteractionPoint);
            float currentDistance = Vector3.SqrMagnitude(interactorPosition - currentInteractionPoint);
            
            return newDistance < currentDistance;
        }
        
        private Vector3 GetInteractionPoint(InteractableBase interactable)
        {
            if (!interactable) return Vector3.zero;
            
            // Try to find a dedicated interaction point first
            var interactionPoint = interactable.InteractionPoint;
            if (interactionPoint)
                return interactionPoint.position;
            
            // Fallback: use the closest point on the collider bounds
            var firstCollider = interactable.GetComponentInChildren<Collider>();
            return firstCollider ? firstCollider.ClosestPoint(transform.position) :
                // Last resort: use transform position
                interactable.transform.position;
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
        
        /// <inheritdoc/>
        protected override void EndHover()
        {
            base.EndHover();
            currentCollider = null;
        }
    }
}
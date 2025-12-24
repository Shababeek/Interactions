using Shababeek.Utilities;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Interactor using sphere-based detection to find interactable objects.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactors/Trigger Interactor")]
    public class TriggerInteractor : InteractorBase
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 0.1f;
        [SerializeField] private Vector3 detectionOffset = Vector3.zero;
        [SerializeField] private LayerMask interactableLayerMask = -1;
        [SerializeField] private float distanceCheckInterval = 0.1f;
        
        [ReadOnly][SerializeField]private Collider currentCollider;

        private Collider[] _overlapResults = new Collider[10];
        private float _lastDistanceCheck;
        private InteractableBase _previousDetectedInteractable;
        
        private void Update()
        {
            if (IsInteracting) return;
            DetectInteractables();
        }
        
        private void DetectInteractables()
        {
            Vector3 detectionCenter = transform.TransformPoint(detectionOffset);
            var hitCount = Physics.OverlapSphereNonAlloc(detectionCenter, detectionRadius, _overlapResults, interactableLayerMask);
            if (hitCount == 0)
            {
                ChangeInteractable(null);
            }
            InteractableBase closestInteractable = CurrentInteractable;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < hitCount; i++)
            {
                var col = _overlapResults[i];
                
                var interactable = col.GetComponentInParent<InteractableBase>();
                
                if (interactable == null || !interactable.CanInteract(Hand)) continue;
                if (interactable == CurrentInteractable) continue;

                var distance = Vector3.SqrMagnitude(detectionCenter - GetInteractionPoint(interactable));
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
            // Handle interactable enter/exit
            if (closestInteractable != null && closestInteractable != CurrentInteractable)
            {
                if (ShouldChangeInteractable(closestInteractable))
                {
                    ChangeInteractable(closestInteractable);
                }
            }
            else if (closestInteractable == null && CurrentInteractable != null)
            {
                // Only exit if not currently selected
                if (CurrentInteractable.CurrentState != InteractionState.Selected)
                {
                    ChangeInteractable(null);
                }
            }
        }

        private void ChangeInteractable(InteractableBase interactable)
        {
            EndHoverToCurrentInteractor();

            CurrentInteractable = interactable;

            if (CurrentInteractable == null)
            {
                currentCollider = null;
                return;
            }

            try 
            { 
                StartHover(); 
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error starting hover on {CurrentInteractable.name}: {e.Message}", CurrentInteractable);
                CurrentInteractable = null;
                currentCollider = null;
            }
        }

        private void EndHoverToCurrentInteractor()
        {
            if (CurrentInteractable != null)
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
        }

        private bool ShouldChangeInteractable(InteractableBase interactable)
        {
            if (CurrentInteractable == null) return true;
            
            if (!interactable.CanInteract(Hand) || !CurrentInteractable.CanInteract(Hand))
            {
                return false;
            }

            if (Time.time - _lastDistanceCheck < distanceCheckInterval)
            {
                return false;
            }
            _lastDistanceCheck = Time.time;

            Vector3 detectionCenter = transform.TransformPoint(detectionOffset);
            
            Vector3 newInteractionPoint = GetInteractionPoint(interactable);
            Vector3 currentInteractionPoint = GetInteractionPoint(CurrentInteractable);
            
            float newDistance = Vector3.SqrMagnitude(detectionCenter - newInteractionPoint);
            float currentDistance = Vector3.SqrMagnitude(detectionCenter - currentInteractionPoint);
            
            return newDistance < currentDistance;
        }
        private Vector3 GetInteractionPoint(InteractableBase interactable)
        {
            if (!interactable) return Vector3.zero;
            
            var interactionPoint = interactable[HandIdentifier];
            //todo use the interaction point instead;
            
            var firstCollider = interactable.GetComponentInChildren<Collider>();
            return firstCollider ? firstCollider.ClosestPoint(transform.position) : interactable.transform.position;
        }

        /// <inheritdoc/>
        protected override void EndHover()
        {
            base.EndHover();
            currentCollider = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (detectionRadius <= 0) return;
            Vector3 detectionCenter = transform.TransformPoint(detectionOffset);
            Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
            Gizmos.DrawWireSphere(detectionCenter, detectionRadius);

            Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
            Gizmos.DrawLine(transform.position, detectionCenter);
            Gizmos.color = new Color(0f, 0f, 1f, 0.8f);
            Gizmos.DrawSphere(detectionCenter, 0.01f);
        }
    }
}
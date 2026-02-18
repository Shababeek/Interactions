using Shababeek.ReactiveVars;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Interactor using sphere-based detection to find interactable objects.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactors/Trigger Interactor")]
    public class TriggerInteractor : InteractorBase
    {
        [SerializeField] private float detectionRadius = 0.1f;
        [SerializeField] private Vector3 detectionOffset = Vector3.zero;
        [SerializeField] private LayerMask interactableLayerMask = -1;
        [SerializeField] private float distanceCheckInterval = 0.05f;

        [ReadOnly] [SerializeField] private Collider[] overlapResults = new Collider[10];
        private float _timeSinceLastColliderUpdate = 0;                                    

        private void Update()
        {
            _timeSinceLastColliderUpdate += Time.deltaTime;
            if (_timeSinceLastColliderUpdate < distanceCheckInterval) return;
            if (IsInteracting) return;
            DetectInteractables();
        }

        private void DetectInteractables()
        {
            Vector3 detectionCenter = transform.TransformPoint(detectionOffset);
            var hitCount = Physics.OverlapSphereNonAlloc(detectionCenter, detectionRadius, overlapResults,
                interactableLayerMask);
            if (hitCount == 0 && CurrentInteractable)
            {
                ChangeInteractable(null);
                return;
            }

            var closestInteractable = FindNearestInteractable(hitCount, detectionCenter);
            if (CurrentInteractable == closestInteractable) return;
            ChangeInteractable(closestInteractable);
        }

        private InteractableBase FindNearestInteractable(int hitCount, Vector3 detectionCenter)
        {
            InteractableBase closestInteractable = null;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < hitCount; i++)
            {
                var col = overlapResults[i];
                var interactable = col.GetComponentInParent<InteractableBase>();
                if (interactable == null || !interactable.CanInteract(Hand)) continue;
                var distance = Vector3.SqrMagnitude(detectionCenter - GetInteractionPoint(interactable));
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }

            return closestInteractable;
        }

        private void ChangeInteractable(InteractableBase interactable)
        {
            EndHover();
            CurrentInteractable = interactable;
            StartHover();
        }

        private Vector3 GetInteractionPoint(InteractableBase interactable)
        {
            var firstCollider = interactable.GetComponentInChildren<Collider>();
            return firstCollider ? firstCollider.ClosestPoint(transform.position) : interactable.transform.position;
        }

        /// <summary>
        /// Returns the trigger detection sphere center as the interaction point.
        /// </summary>
        public override Vector3 GetInteractionPoint()
        {
            return transform.TransformPoint(detectionOffset);
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
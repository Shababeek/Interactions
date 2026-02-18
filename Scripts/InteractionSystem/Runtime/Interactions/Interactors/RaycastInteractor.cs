using Shababeek.ReactiveVars;
using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Interactor using raycasting to detect interactions at a distance.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactors/Raycast Interactor")]
    public class RaycastInteractor : InteractorBase
    {
        [Header("Raycast Settings")]
        [Tooltip("Maximum distance the raycast can reach in world units.")]
        [SerializeField] private float maxRaycastDistance = 10f;
        
        [Tooltip("Layer mask used for raycasting. Only objects on these layers will be detected.")]
        [SerializeField] private LayerMask raycastLayerMask = -1;
        
        [Tooltip("Transform that serves as the origin point for the raycast. If null, uses this transform.")]
        [SerializeField] private Transform raycastOrigin;
        
        [Header("Line Renderer Settings")]
        [Tooltip("LineRenderer component used to visualize the raycast. Auto-added if not present.")]
        [SerializeField] private LineRenderer lineRenderer;
        
        [Tooltip("Material used for the line renderer visualization.")]
        [SerializeField] private Material lineMaterial;
        
        [Tooltip("Color of the line renderer when not hitting anything.")]
        [SerializeField] private Color lineColor = Color.white;
        
        [Tooltip("Width of the line renderer in world units.")]
        [SerializeField] private float lineWidth = 0.01f;
        
        [Tooltip("Whether to show the line renderer visualization.")]
        [SerializeField] private bool showLineRenderer = true;
        
        [Header("Debug")]
        [ReadOnly][SerializeField] [Tooltip("World position where the raycast hit an object.")]
        private Vector3 hitPoint;
        
        [ReadOnly][SerializeField] [Tooltip("Whether the raycast is currently hitting an object.")]
        private bool isHitting;
        
        private RaycastHit[] raycastHits = new RaycastHit[10];
        private int hitCount;

        private void Start()
        {
            InitializeLineRenderer();
            if (!raycastOrigin)
                raycastOrigin = transform;
        }
        

        private void InitializeLineRenderer()
        {
            if (!lineRenderer)
                lineRenderer = GetComponent<LineRenderer>();
                
            if (!lineRenderer)
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = showLineRenderer;
        }
        

        private void Update()
        {
            PerformRaycast();
            UpdateLineRenderer();
        }

        private void PerformRaycast()
        {
            Vector3 origin = raycastOrigin.position;
            Vector3 direction = raycastOrigin.forward;
            
            hitCount = Physics.RaycastNonAlloc(origin, direction, raycastHits, maxRaycastDistance, raycastLayerMask);
            
            InteractableBase closestInteractable = null;
            float closestDistance = float.MaxValue;
            isHitting = false;
            
            for (int i = 0; i < hitCount; i++)
            {
                var hit = raycastHits[i];
                var interactable = hit.collider.GetComponentInParent<InteractableBase>();
                
                if (interactable && hit.distance < closestDistance)
                {
                    closestInteractable = interactable;
                    closestDistance = hit.distance;
                    hitPoint = hit.point;
                    isHitting = true;
                }
            }
            
            if (closestInteractable != CurrentInteractable)
            {
                if (CurrentInteractable)
                {
                    EndHover();
                }

                CurrentInteractable = closestInteractable;
                
                if (CurrentInteractable)
                {
                    StartHover();
                }
            }
        }
        

        private void UpdateLineRenderer()
        {
            if (!showLineRenderer || !lineRenderer)
                return;
                
            Vector3 origin = raycastOrigin.position;
            Vector3 endPoint;
            
            if (isHitting)
            {
                endPoint = hitPoint;
                lineRenderer.startColor = Color.green; // Hit color
                lineRenderer.endColor = Color.green;
            }
            else
            {
                endPoint = origin + raycastOrigin.forward * maxRaycastDistance;
                lineRenderer.startColor = lineColor; // Default color
                lineRenderer.endColor = lineColor;
            }
            
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, endPoint);
        }
        
        /// <summary>
        /// Sets the visibility of the line renderer.
        /// </summary>
        public void SetLineRendererVisibility(bool visible)
        {
            showLineRenderer = visible;
            if (lineRenderer)
                lineRenderer.enabled = visible;
        }
        
        /// <summary>
        /// Sets the color of the line renderer.
        /// </summary>
        public void SetLineColor(Color color)
        {
            lineColor = color;
            if (lineRenderer)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }
        
        /// <summary>
        /// Sets the maximum distance for the raycast.
        /// </summary>
        public void SetMaxDistance(float distance)
        {
            maxRaycastDistance = distance;
        }
        
        /// <summary>
        /// Sets the layer mask used for raycasting.
        /// </summary>
        public void SetRaycastLayerMask(LayerMask layerMask)
        {
            raycastLayerMask = layerMask;
        }

        /// <summary>
        /// Returns the raycast hit point, or ray origin if not hitting.
        /// </summary>
        public override Vector3 GetInteractionPoint()
        {
            return isHitting ? hitPoint : (raycastOrigin ? raycastOrigin.position : transform.position);
        }
        

        private void OnDrawGizmosSelected()
        {
            if (!raycastOrigin) return;
            
            Vector3 origin = raycastOrigin.position;
            Vector3 direction = raycastOrigin.forward;
            
            Gizmos.color = isHitting ? Color.green : Color.red;
            Gizmos.DrawRay(origin, direction * maxRaycastDistance);
            
            if (isHitting)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hitPoint, 0.05f);
            }
        }
    }
}
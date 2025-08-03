using Shababeek.Interactions;
using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Core;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Shababeek.Interactions.Animations;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Represents the positioning data for a hand relative to an interactable.
    /// </summary>
    [System.Serializable]
    public struct HandPositioning
    {
        [Tooltip("Position offset for the hand relative to the interactable.")]
        public Vector3 positionOffset;
        
        [Tooltip("Rotation offset for the hand relative to the interactable.")]
        public Vector3 rotationOffset;
        
        public HandPositioning(Vector3 position, Vector3 rotation)
        {
            positionOffset = position;
            rotationOffset = rotation;
        }
        
        public static HandPositioning Zero => new HandPositioning(Vector3.zero, Vector3.zero);
    }
    
    /// <summary>
    /// Unified system for constraining hand poses during interactions.
    /// This component provides pose constraints, transform positioning, and hand visibility control.
    /// Movement strategy (object to hand vs hand to object) is handled by individual interactables.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Unified Pose Constraint System")]
    public class UnifiedPoseConstraintSystem : MonoBehaviour, IPoseConstrainer
    {
        [Header("Constraint Configuration")]
        [Tooltip("The type of constraint to apply to hands during interaction.")]
        [SerializeField] private HandConstrainType constraintType = HandConstrainType.Constrained;
        
        [Header("Pose Constraints")]
        [Tooltip("Constraints for the left hand's pose during interactions.")]
        [SerializeField] private PoseConstrains leftPoseConstraints;
        
        [Tooltip("Constraints for the right hand's pose during interactions.")]
        [SerializeField] private PoseConstrains rightPoseConstraints;
        
        [Header("Hand Positioning")]
        [Tooltip("Positioning data for the left hand relative to the interactable.")]
        [SerializeField] private HandPositioning leftHandPositioning = HandPositioning.Zero;
        
        [Tooltip("Positioning data for the right hand relative to the interactable.")]
        [SerializeField] private HandPositioning rightHandPositioning = HandPositioning.Zero;
        
        // IPoseConstrainer Implementation (for backward compatibility)
        public PoseConstrains LeftPoseConstrains => leftPoseConstraints;
        public PoseConstrains RightPoseConstrains => rightPoseConstraints;
        
        public Transform LeftHandTransform
        {
            get => null;
            set => throw new System.NotImplementedException();
        }

        public Transform RightHandTransform
        {
            get => null;
            set => throw new System.NotImplementedException();
        }

        public Transform PivotParent => null;
        public bool HasChanged => transform.hasChanged;
        
        // Interface properties
        public HandConstrainType ConstraintType => constraintType;
        public bool UseSmoothTransitions => false;
        public float TransitionSpeed => 10f;
        
        /// <summary>
        /// Applies pose constraints and visibility control to the specified interactor's hand.
        /// </summary>
        /// <param name="interactor">The interactor whose hand should be constrained.</param>
        public void ApplyConstraints(Hand hand)
        {
            
            switch (constraintType)
            {
                case HandConstrainType.HideHand:
                    hand.ToggleRenderer(false);
                    break;
                    
                case HandConstrainType.FreeHand:
                    // No pose constraints applied - hand moves freely
                    break;
                    
                case HandConstrainType.Constrained:
                    // Apply pose constraints
                    hand.Constrain(this);
                    break;
            }
        }
        
        /// <summary>
        /// Removes all pose constraints and restores hand visibility.
        /// </summary>
        /// <param name="interactor">The interactor whose hand should be unconstrained.</param>
        public void RemoveConstraints(InteractorBase interactor)
        {
            var hand = interactor.Hand;
            
            // Remove pose constraints
            hand.Unconstrain(this);
            
            // Restore hand visibility
            hand.ToggleRenderer(true);
        }
        
        /// <summary>
        /// Gets the target position and rotation for the specified hand identifier.
        /// </summary>
        /// <param name="handIdentifier">The hand identifier (Left or Right).</param>
        /// <returns>The target position and rotation for the specified hand.</returns>
        public (Vector3 position, Quaternion rotation) GetTargetHandTransform(HandIdentifier handIdentifier)
        {
            if (handIdentifier == HandIdentifier.Left)
            {
                var position = transform.position + transform.rotation * leftHandPositioning.positionOffset;
                var rotation = transform.rotation * Quaternion.Euler(leftHandPositioning.rotationOffset);
                return (position, rotation);
            }
            else
            {
                var position = transform.position + transform.rotation * rightHandPositioning.positionOffset;
                var rotation = transform.rotation * Quaternion.Euler(rightHandPositioning.rotationOffset);
                return (position, rotation);
            }
        }
        
        /// <summary>
        /// Gets the pose constraints for the specified hand identifier.
        /// </summary>
        /// <param name="handIdentifier">The hand identifier (Left or Right).</param>
        /// <returns>The pose constraints for the specified hand.</returns>
        public PoseConstrains GetPoseConstraints(HandIdentifier handIdentifier)
        {
            return handIdentifier == HandIdentifier.Left ? leftPoseConstraints : rightPoseConstraints;
        }
        
        /// <summary>
        /// Updates the pivot parent and hand transforms.
        /// </summary>
        public void UpdatePivots()
        {
            // This method is no longer needed as pivotParent is removed
        }
        
        /// <summary>
        /// Initializes the constraint system.
        /// </summary>
        public void Initialize()
        {
            UpdatePivots();
        }
        
        private void OnEnable()
        {
            // Ensure proper initialization
            // This part is no longer needed as pivotParent is removed
        }
    }
} 
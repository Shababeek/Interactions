using Shababeek.Utilities;
using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Core;

using UnityEngine;

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
        
        /// <summary>
        /// Initializes a new hand positioning with the specified position and rotation offsets.
        /// </summary>
        /// <param name="position">The position offset</param>
        /// <param name="rotation">The rotation offset</param>
        public HandPositioning(Vector3 position, Vector3 rotation)
        {
            positionOffset = position;
            rotationOffset = rotation;
        }
        
        /// <summary>
        /// Gets a zero hand positioning (no offset).
        /// </summary>
        /// <returns>A hand positioning with zero offsets</returns>
        public static HandPositioning Zero => new HandPositioning(Vector3.zero, Vector3.zero);
    }
    
    /// <summary>
    /// Constrains hand poses during interactions.
    /// This component provides pose constraints, transform positioning, and hand visibility control.
    /// Movement strategy (object to hand vs hand to object) is handled by individual interactables.
    /// </summary>
    /// <remarks>
    /// The PoseConstrainter manages how hands are positioned and constrained during interactions.
    /// It can hide hands, allow free movement, or apply specific pose constraints based on the
    /// configured constraint type.
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Pose Constrainer")]
    public class PoseConstrainter : MonoBehaviour, IPoseConstrainer
    {
        [Header("Constraint Configuration")]
        [Tooltip("The type of constraint to apply to hands during interaction.")]
        [SerializeField] private HandConstrainType constraintType = HandConstrainType.Constrained;
        
        [Tooltip("Whether to use smooth transitions when positioning hands.")]
        [SerializeField] private bool useSmoothTransitions = false;
        
        [Tooltip("Speed of smooth transitions (units per second).")]
        [SerializeField] private float transitionSpeed = 10f;
        
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

        [Header("Runtime State")]
        [SerializeField, ReadOnly] [Tooltip("The parent transform for this constraint system.")]
        private Transform parent;

        /// <summary>
        /// Gets or sets the parent transform for this constraint system.
        /// </summary>
        /// <value>The parent transform</value>
        /// <returns>The parent transform, or this transform if no parent is set</returns>
        public Transform Parent
        {
            get => parent == null ? transform : parent;
            set => parent = value;
        }

        /// <summary>
        /// Gets the pose constraints for the left hand.
        /// </summary>
        /// <returns>The left hand pose constraints</returns>
        public PoseConstrains LeftPoseConstrains => leftPoseConstraints;
        
        /// <summary>
        /// Gets the pose constraints for the right hand.
        /// </summary>
        /// <returns>The right hand pose constraints</returns>
        public PoseConstrains RightPoseConstrains => rightPoseConstraints;
        
        /// <summary>
        /// Gets or sets the left hand transform (not implemented).
        /// </summary>
        /// <value>The left hand transform</value>
        /// <returns>Always returns null</returns>
        public Transform LeftHandTransform
        {
            get => null;
            set => throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the right hand transform (not implemented).
        /// </summary>
        /// <value>The right hand transform</value>
        /// <returns>Always returns null</returns>
        public Transform RightHandTransform
        {
            get => null;
            set => throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the pivot parent transform (not implemented).
        /// </summary>
        /// <returns>Always returns null</returns>
        public Transform PivotParent => null;
        
        /// <summary>
        /// Gets whether this transform has changed since the last frame.
        /// </summary>
        /// <returns>True if the transform has changed</returns>
        public bool HasChanged => transform.hasChanged;
        
        // Interface properties
        /// <summary>
        /// Gets the type of constraint to apply to hands during interaction.
        /// </summary>
        /// <returns>The hand constraint type</returns>
        public HandConstrainType ConstraintType => constraintType;
        
        /// <summary>
        /// Gets whether to use smooth transitions when positioning hands.
        /// </summary>
        /// <returns>True if smooth transitions are enabled</returns>
        public bool UseSmoothTransitions => useSmoothTransitions;
        
        /// <summary>
        /// Gets the speed of smooth transitions when positioning hands.
        /// </summary>
        /// <returns>The transition speed in units per second</returns>
        public float TransitionSpeed => transitionSpeed;
        
        /// <summary>
        /// Applies pose constraints and visibility control to the specified hand.
        /// </summary>
        /// <param name="hand">The hand that should be constrained</param>
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
        /// <param name="hand">The hand that should be unconstrained</param>
        public void RemoveConstraints(Hand hand)
        {
            hand.Unconstrain(this);
            hand.ToggleRenderer(true);
        }
        
        /// <summary>
        /// Gets the target position and rotation for the specified hand identifier.
        /// </summary>
        /// <param name="handIdentifier">The hand identifier (Left or Right)</param>
        /// <returns>The target position and rotation for the specified hand in LOCAL coordinates relative to this transform</returns>
        public (Vector3 position, Quaternion rotation) GetTargetHandTransform(HandIdentifier handIdentifier)
        {
            if (handIdentifier == HandIdentifier.Left)
            {
                var position = leftHandPositioning.positionOffset;
                var rotation = Quaternion.Euler(leftHandPositioning.rotationOffset);
                return (position, rotation);
            }
            else
            {
                var position = rightHandPositioning.positionOffset;
                var rotation = Quaternion.Euler(rightHandPositioning.rotationOffset);
                return (position, rotation);
            }
        }
        
        /// <summary>
        /// Gets the target hand transform relative to a specified parent transform.
        /// </summary>
        /// <param name="parent">The parent transform to calculate relative coordinates for</param>
        /// <param name="handIdentifier">The hand identifier (Left or Right)</param>
        /// <returns>The target position and rotation in the parent's local coordinates</returns>
        public (Vector3 position, Quaternion rotation) GetRelativeTargetHandTransform(Transform parent, HandIdentifier handIdentifier)
        {
            var transfom = GetTargetHandTransform(handIdentifier);
            Vector3 worldPosition = transform.TransformPoint(transfom.position);
            Quaternion worldRotation = transform.rotation * transfom.rotation;
            
            //  convert from world space to interactableObject's local space
            transfom.position = parent.InverseTransformPoint(worldPosition);
            transfom.rotation = Quaternion.Inverse(parent.rotation) * worldRotation;
            return transfom;
        }
        
        /// <summary>
        /// Gets the pose constraints for the specified hand identifier.
        /// </summary>
        /// <param name="handIdentifier">The hand identifier (Left or Right)</param>
        /// <returns>The pose constraints for the specified hand</returns>
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
    }
} 
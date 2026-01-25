using Shababeek.Utilities;
using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Core;

using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Positioning data for a hand relative to an interactable.
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
        /// Zero hand positioning (no offset).
        /// </summary>
        public static HandPositioning Zero => new HandPositioning(Vector3.zero, Vector3.zero);
    }
    
    /// <summary>
    /// Constrains hand poses during interactions.
    /// </summary>
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
        
        private InteractableBase _interactableBase;

        /// <summary>
        /// Parent transform for this constraint system.
        /// </summary>
        public Transform Parent
        {
            get => parent == null ? transform : parent;
            set => parent = value;
        }
        
        /// <summary>
        /// Transform used for pose calculations. References the scale compensator if available.
        /// </summary>
        public Transform ConstraintTransform
        {
            get
            {
                if (!_interactableBase)
                    _interactableBase = GetComponent<InteractableBase>();
                return _interactableBase ? _interactableBase.ConstraintTransform : transform;
            }
        }

        /// <summary>
        /// Pose constraints for the left hand.
        /// </summary>
        public PoseConstrains LeftPoseConstrains => leftPoseConstraints;
        
        /// <summary>
        /// Pose constraints for the right hand.
        /// </summary>
        public PoseConstrains RightPoseConstrains => rightPoseConstraints;
        
        /// <summary>
        /// Whether this transform has changed since the last frame.
        /// </summary>
        public bool HasChanged => transform.hasChanged;
        
        /// <summary>
        /// Type of constraint to apply during interaction.
        /// </summary>
        public HandConstrainType ConstraintType => constraintType;
        
        /// <summary>
        /// Whether to use smooth transitions when positioning hands.
        /// </summary>
        public bool UseSmoothTransitions => useSmoothTransitions;
        
        /// <summary>
        /// Speed of smooth transitions.
        /// </summary>
        public float TransitionSpeed => transitionSpeed;
        
        /// <summary>
        /// Applies pose constraints to the hand.
        /// </summary>
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
        /// Removes pose constraints and restores hand visibility.
        /// </summary>
        public void RemoveConstraints(Hand hand)
        {
            hand.Unconstrain(this);
            hand.ToggleRenderer(true);
        }
        
        /// <summary>
        /// Target position and rotation for the specified hand in local coordinates.
        /// </summary>
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
        public void UpdatePivots()
        {
            // This method is no longer needed as pivotParent is removed
        }
        
        public void Initialize()
        {
            UpdatePivots();
        }
    }
}
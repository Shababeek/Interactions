using System.Collections.Generic;
using Shababeek.ReactiveVars;
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
    /// A single grab point with per-hand positioning and pose constraints.
    /// </summary>
    [System.Serializable]
    public class GrabPoint
    {
        [Tooltip("Descriptive name for this grab point.")]
        public string pointName = "Grab Point";

        [Tooltip("Local position of this grab point on the interactable.")]
        public Vector3 localPosition = Vector3.zero;

        [Tooltip("Local rotation of this grab point on the interactable (Euler angles).")]
        public Vector3 localRotation = Vector3.zero;

        [Tooltip("Constraints for the left hand at this grab point.")]
        public PoseConstrains leftPoseConstraints;

        [Tooltip("Constraints for the right hand at this grab point.")]
        public PoseConstrains rightPoseConstraints;

        [Tooltip("Left hand positioning at this grab point.")]
        public HandPositioning leftHandPositioning = HandPositioning.Zero;

        [Tooltip("Right hand positioning at this grab point.")]
        public HandPositioning rightHandPositioning = HandPositioning.Zero;
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

        [Header("Multi-Point Grab")]
        [Tooltip("List of grab points for MultiPoint constraint mode. Each point has its own hand positioning and pose constraints.")]
        [SerializeField] private List<GrabPoint> grabPoints = new();

        [Header("Runtime State")]
        [SerializeField, ReadOnly] [Tooltip("The parent transform for this constraint system.")]
        private Transform parent;
        
        private InteractableBase _interactableBase;
        private int _activeGrabPointIndex = -1;

        /// <summary>
        /// Parent transform for this constraint system.
        /// </summary>
        public Transform Parent
        {
            get => parent == null ? transform : parent;
            set => parent = value;
        }

        /// <summary>
        /// List of grab points for MultiPoint mode.
        /// </summary>
        public IReadOnlyList<GrabPoint> GrabPoints => grabPoints;

        /// <summary>
        /// Index of the currently active grab point (-1 if none).
        /// </summary>
        public int ActiveGrabPointIndex => _activeGrabPointIndex;
        
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
        /// Pose constraints for the left hand. Returns active grab point constraints when in MultiPoint mode.
        /// </summary>
        public PoseConstrains LeftPoseConstrains
        {
            get
            {
                if (constraintType == HandConstrainType.MultiPoint && _activeGrabPointIndex >= 0 && _activeGrabPointIndex < grabPoints.Count)
                    return grabPoints[_activeGrabPointIndex].leftPoseConstraints;
                return leftPoseConstraints;
            }
        }

        /// <summary>
        /// Pose constraints for the right hand. Returns active grab point constraints when in MultiPoint mode.
        /// </summary>
        public PoseConstrains RightPoseConstrains
        {
            get
            {
                if (constraintType == HandConstrainType.MultiPoint && _activeGrabPointIndex >= 0 && _activeGrabPointIndex < grabPoints.Count)
                    return grabPoints[_activeGrabPointIndex].rightPoseConstraints;
                return rightPoseConstraints;
            }
        }
        
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
        /// In MultiPoint mode, uses the interaction point to find the nearest grab point.
        /// </summary>
        /// <param name="hand">The hand to constrain.</param>
        /// <param name="interactionPoint">World position where the interactor contacted the object. Used in MultiPoint mode to select nearest grab point. Falls back to hand position if null.</param>
        public void ApplyConstraints(Hand hand, Vector3? interactionPoint )
        {
            switch (constraintType)
            {
                case HandConstrainType.HideHand:
                    hand.ToggleRenderer(false);
                    break;

                case HandConstrainType.FreeHand:
                    break;

                case HandConstrainType.Constrained:
                    hand.Constrain(this);
                    break;

                case HandConstrainType.MultiPoint:
                    Vector3 searchPoint = interactionPoint ?? hand.transform.position;
                    _activeGrabPointIndex = FindNearestGrabPoint(searchPoint);
                    hand.Constrain(this);
                    break;
            }
        }

        public void ApplyConstraints(Hand interactor)
        {
            ApplyConstraints(interactor, null);
        }

        /// <summary>
        /// Removes pose constraints and restores hand visibility.
        /// </summary>
        public void RemoveConstraints(Hand hand)
        {
            hand.Unconstrain(this);
            hand.ToggleRenderer(true);
            _activeGrabPointIndex = -1;
        }
        
        /// <summary>
        /// Target position and rotation for the specified hand in local coordinates.
        /// </summary>
        public (Vector3 position, Quaternion rotation) GetTargetHandTransform(HandIdentifier handIdentifier)
        {
            var positioning = GetActiveHandPositioning(handIdentifier);
            return (positioning.positionOffset, Quaternion.Euler(positioning.rotationOffset));
        }

        private HandPositioning GetActiveHandPositioning(HandIdentifier handIdentifier)
        {
            if (constraintType == HandConstrainType.MultiPoint && _activeGrabPointIndex >= 0 && _activeGrabPointIndex < grabPoints.Count)
            {
                var point = grabPoints[_activeGrabPointIndex];
                return handIdentifier == HandIdentifier.Left ? point.leftHandPositioning : point.rightHandPositioning;
            }
            return handIdentifier == HandIdentifier.Left ? leftHandPositioning : rightHandPositioning;
        }

        private int FindNearestGrabPoint(Vector3 worldPosition)
        {
            if (grabPoints == null || grabPoints.Count == 0) return -1;

            int nearest = 0;
            float nearestDist = float.MaxValue;
            for (int i = 0; i < grabPoints.Count; i++)
            {
                Vector3 grabPointWorld = ConstraintTransform.TransformPoint(grabPoints[i].localPosition);
                float dist = Vector3.SqrMagnitude(grabPointWorld - worldPosition);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = i;
                }
            }
            return nearest;
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
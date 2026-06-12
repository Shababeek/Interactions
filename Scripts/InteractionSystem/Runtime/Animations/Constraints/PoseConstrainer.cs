using System.Collections.Generic;
using Shababeek.ReactiveVars;
using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Core;

using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Positioning data for a hand relative to an interactable.</summary>
    [System.Serializable]
    public struct HandPositioning
    {
        [Tooltip("Position offset for the hand relative to the interactable.")]
        public Vector3 positionOffset;

        [Tooltip("Rotation offset for the hand relative to the interactable.")]
        public Vector3 rotationOffset;

        /// <summary>Initializes a new hand positioning with the specified position and rotation offsets.</summary>
        public HandPositioning(Vector3 position, Vector3 rotation)
        {
            positionOffset = position;
            rotationOffset = rotation;
        }

        /// <summary>Zero hand positioning (no offset).</summary>
        public static HandPositioning Zero => new HandPositioning(Vector3.zero, Vector3.zero);
    }

    /// <summary>A single grab point with per-hand positioning and pose constraints.</summary>
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
    
    /// <summary>Constrains hand poses during interactions.</summary>
    [AddComponentMenu("Shababeek/Interactions/Pose Constrainer")]
    public class PoseConstrainer : MonoBehaviour, IPoseConstrainer
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
        // Per-hand active grab point: two hands holding a MultiPoint interactable must not
        // overwrite each other's resolved point (each hand poses from its own).
        private int _leftActiveGrabPointIndex = -1;
        private int _rightActiveGrabPointIndex = -1;

        /// <summary>Gets or sets the parent transform for this constraint system.</summary>
        public Transform Parent
        {
            get => parent == null ? transform : parent;
            set => parent = value;
        }

        /// <summary>Gets the list of grab points for MultiPoint mode.</summary>
        public IReadOnlyList<GrabPoint> GrabPoints => grabPoints;

        /// <summary>Gets the active grab point index of whichever hand holds one (-1 if none).</summary>
        public int ActiveGrabPointIndex =>
            _leftActiveGrabPointIndex >= 0 ? _leftActiveGrabPointIndex : _rightActiveGrabPointIndex;

        /// <summary>Gets the active grab point index for a specific hand (-1 if none).</summary>
        public int GetActiveGrabPointIndex(HandIdentifier hand)
        {
            return hand == HandIdentifier.Left ? _leftActiveGrabPointIndex : _rightActiveGrabPointIndex;
        }

        /// <summary>Gets the transform used for pose calculations.</summary>
        public Transform ConstraintTransform
        {
            get
            {
                if (!_interactableBase)
                    _interactableBase = GetComponent<InteractableBase>();
                return _interactableBase ? _interactableBase.ConstraintTransform : transform;
            }
        }

        /// <summary>Gets pose constraints for the left hand.</summary>
        public PoseConstrains LeftPoseConstrains
        {
            get
            {
                int index = _leftActiveGrabPointIndex;
                if (constraintType == HandConstrainType.MultiPoint && index >= 0 && index < grabPoints.Count)
                    return grabPoints[index].leftPoseConstraints;
                return leftPoseConstraints;
            }
        }

        /// <summary>Gets pose constraints for the right hand.</summary>
        public PoseConstrains RightPoseConstrains
        {
            get
            {
                int index = _rightActiveGrabPointIndex;
                if (constraintType == HandConstrainType.MultiPoint && index >= 0 && index < grabPoints.Count)
                    return grabPoints[index].rightPoseConstraints;
                return rightPoseConstraints;
            }
        }
        
        /// <summary>Gets whether this transform has changed since the last frame.</summary>
        public bool HasChanged => transform.hasChanged;

        /// <summary>Gets the type of constraint to apply during interaction.</summary>
        public HandConstrainType ConstraintType => constraintType;

        /// <summary>Gets whether to use smooth transitions when positioning hands.</summary>
        public bool UseSmoothTransitions => useSmoothTransitions;

        /// <summary>Gets the speed of smooth transitions.</summary>
        public float TransitionSpeed => transitionSpeed;
        
        /// <summary>Applies pose constraints to a hand using an interaction point for MultiPoint mode.</summary>
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
                    // A point already claimed by the other hand is excluded so two hands
                    // never resolve to the same grip.
                    int otherHandIndex = GetActiveGrabPointIndex(OtherHand(hand.HandIdentifier));
                    SetActiveGrabPointIndex(hand.HandIdentifier, FindNearestGrabPoint(searchPoint, otherHandIndex));
                    hand.Constrain(this);
                    break;
            }
        }

        private static HandIdentifier OtherHand(HandIdentifier hand)
        {
            return hand == HandIdentifier.Left ? HandIdentifier.Right : HandIdentifier.Left;
        }

        private void SetActiveGrabPointIndex(HandIdentifier hand, int index)
        {
            if (hand == HandIdentifier.Left) _leftActiveGrabPointIndex = index;
            else _rightActiveGrabPointIndex = index;
        }

        public void ApplyConstraints(Hand interactor)
        {
            ApplyConstraints(interactor, null);
        }

        /// <summary>Removes pose constraints and restores hand visibility. Only clears the
        /// releasing hand's grab point — the other hand may still be holding one.</summary>
        public void RemoveConstraints(Hand hand)
        {
            hand.Unconstrain(this);
            hand.ToggleRenderer(true);
            SetActiveGrabPointIndex(hand.HandIdentifier, -1);
        }
        
        /// <summary>Gets target position and rotation for the specified hand in local coordinates.</summary>
        public (Vector3 position, Quaternion rotation) GetTargetHandTransform(HandIdentifier handIdentifier)
        {
            var positioning = GetActiveHandPositioning(handIdentifier);
            return (positioning.positionOffset, Quaternion.Euler(positioning.rotationOffset));
        }

        private HandPositioning GetActiveHandPositioning(HandIdentifier handIdentifier)
        {
            int index = GetActiveGrabPointIndex(handIdentifier);
            if (constraintType == HandConstrainType.MultiPoint && index >= 0 && index < grabPoints.Count)
            {
                var point = grabPoints[index];
                return handIdentifier == HandIdentifier.Left ? point.leftHandPositioning : point.rightHandPositioning;
            }
            return handIdentifier == HandIdentifier.Left ? leftHandPositioning : rightHandPositioning;
        }

        private int FindNearestGrabPoint(Vector3 worldPosition, int excludeIndex = -1)
        {
            if (grabPoints == null || grabPoints.Count == 0) return -1;

            int nearest = -1;
            float nearestDist = float.MaxValue;
            for (int i = 0; i < grabPoints.Count; i++)
            {
                if (i == excludeIndex) continue;
                Vector3 grabPointWorld = ConstraintTransform.TransformPoint(grabPoints[i].localPosition);
                float dist = Vector3.SqrMagnitude(grabPointWorld - worldPosition);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = i;
                }
            }

            // All points excluded (single point already held by the other hand) — share it
            // rather than fail; pose data still resolves correctly per hand.
            return nearest >= 0 ? nearest : excludeIndex;
        }
  
        
    }
}
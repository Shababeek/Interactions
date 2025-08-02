using Shababeek.Interactions;
using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Constrains the interaction pose of hands position and pose when interaction starts.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interaction Pose Constrainer")]
    public class InteractionPoseConstrainer : MonoBehaviour, IPoseConstrainer
    {
        [Tooltip("Constraints for the left hand's position and pose during interactions.")]
        [HideInInspector, SerializeField] private HandConstraints leftConstraints;
        [Tooltip("Constraints for the right hand's position and pose during interactions.")]
        [HideInInspector, SerializeField] private HandConstraints rightConstraints;
        [Tooltip("Parent transform for interaction pivot points that define hand positions.")]
        [HideInInspector, SerializeField] private Transform pivotParent;
        private InteractableBase interactable;

        public Transform LeftHandTransform
        {
            set => leftConstraints.relativeTransform = value;
            get => leftConstraints.relativeTransform;
        }
        
        public Transform RightHandTransform
        {
            set => rightConstraints.relativeTransform = value;
            get => rightConstraints.relativeTransform;
        }

        public Transform PivotParent => pivotParent;
        public bool HasChanged => transform.hasChanged;
        public PoseConstrains LeftPoseConstrains => leftConstraints.poseConstrains;
        public PoseConstrains RightPoseConstrains => rightConstraints.poseConstrains;

        public void UpdatePivots()
        {
            if (!pivotParent)
            {
                pivotParent = new GameObject("Interaction Pivots").transform;
            }
            pivotParent.parent = null;
            pivotParent.localScale = Vector3.one;
            pivotParent.parent = transform;
            pivotParent.transform.localPosition = Vector3.zero;
            pivotParent.transform.localRotation = Quaternion.identity;
            ;
        }

        public void Initialize()
        {
            UpdatePivots();
        }

        private void OnEnable()
        {
            interactable = GetComponent<InteractableBase>();
            interactable.OnSelected.Do(Constrain).Subscribe().AddTo(this);
            interactable.OnDeselected.Do(Unconstrain).Subscribe().AddTo(this);
        }

        private void Unconstrain(InteractorBase interactor)
        {
            interactor.Hand.Unconstrain(this);
        }

        private void Constrain(InteractorBase interactor)
        {
            interactor.Hand.Constrain(this);
        }
    }
}
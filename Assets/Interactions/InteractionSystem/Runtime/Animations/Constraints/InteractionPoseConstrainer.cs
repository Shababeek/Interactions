
using Kandooz.InteractionSystem.Animations.Constraints;
using Kandooz.InteractionSystem.Core;
using UniRx;
using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    [RequireComponent(typeof(InteractableBase))]
    public class InteractionPoseConstrainer : MonoBehaviour,IPoseConstrainer
    {
        [HideInInspector, SerializeField] private HandConstraints leftConstraints;
        [HideInInspector, SerializeField] private HandConstraints rightConstraints;
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
            pivotParent.transform.localRotation=Quaternion.identity;
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
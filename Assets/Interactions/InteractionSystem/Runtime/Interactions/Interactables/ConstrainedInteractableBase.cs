using Kandooz.InteractionSystem.Animations.Constraints;
using Kandooz.InteractionSystem.Core;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Kandooz.InteractionSystem.Interactions
{
    public abstract class ConstrainedInteractableBase : InteractableBase, IPoseConstrainer
    {
        [SerializeField] protected Transform interactableObject;
        [HideInInspector, SerializeField] private Transform leftHand;
        [HideInInspector, SerializeField] private Transform rightHand;
        [HideInInspector, SerializeField] private PoseConstrains leftConstraints;
        [HideInInspector, SerializeField] private PoseConstrains rightConstraints;
        [HideInInspector, SerializeField] private Transform pivotParent;

        [SerializeField] private float snapDistance = .5f;
        [SerializeField] private HandConstrainType constraintsType = HandConstrainType.Constrained;
        [SerializeField] private bool smoothHandTransition = true;
        
        private (Vector3 position, Quaternion rotation) leftHandPivot;
        private (Vector3 position, Quaternion rotation) rightHandPivot;
        private Transform fakeHandTransform;

        private (Vector3 position, Quaternion rotation) currentPivot;
        private float positionLerper;
        public Transform InteractableObject
        {
            get => interactableObject;
            set => interactableObject = value;
        }

        protected override void Select()
        {
            if (constraintsType == HandConstrainType.FreeHand)
                return;
            CurrentInteractor.ToggleHandModel(false);
            if (constraintsType == HandConstrainType.HideHand)
                return;

            fakeHandTransform = (CurrentInteractor.HandIdentifier == HandIdentifier.Left ? leftHand : rightHand).transform;
            fakeHandTransform.gameObject.SetActive(true);
            currentPivot = CurrentInteractor.HandIdentifier == HandIdentifier.Left ? leftHandPivot : rightHandPivot;
            var interactableTransform = CurrentInteractor.transform;
            if (smoothHandTransition)
            {
                fakeHandTransform.position = interactableTransform.position;
                fakeHandTransform.rotation = interactableTransform.rotation;
                positionLerper = 0;
            }
            else
            {
                fakeHandTransform.localRotation = currentPivot.rotation;
                fakeHandTransform.position = currentPivot.position;
            }
        }

        protected override void DeSelected()
        {
            CurrentInteractor.ToggleHandModel(true);
            leftHand.gameObject.SetActive(false);
            rightHand.gameObject.SetActive(false);
        }

        private void Awake()
        {
            //interactableObject.transform.position = GetPositionBetweenTwoPoints(interactableObject.transform.position, point1.position, point2.position);   
            leftHandPivot = new()
            {
                position = leftHand.transform.localPosition,
                rotation = leftHand.transform.localRotation
            };
            rightHandPivot = new()
            {
                position = rightHand.transform.localPosition,
                rotation = rightHand.transform.localRotation
            };
            this.UpdateAsObservable()
                .Where(_ => IsSelected)
                .Where(_ => constraintsType == HandConstrainType.Constrained && smoothHandTransition)
                .Do(_ => SetInteractionHandPosition()).Subscribe().AddTo(this);
        }

        private void SetInteractionHandPosition()
        {
            positionLerper += Time.deltaTime * 10;
            fakeHandTransform.localPosition = Vector3.Lerp(fakeHandTransform.localPosition, currentPivot.position, positionLerper);
            fakeHandTransform.localRotation = Quaternion.Lerp(fakeHandTransform.localRotation, currentPivot.rotation, positionLerper);
        }

        private enum HandConstrainType
        {
            HideHand,
            FreeHand,
            Constrained
        }
        public PoseConstrains LeftPoseConstrains => leftConstraints;
        public PoseConstrains RightPoseConstrains => rightConstraints;
        public Transform LeftHandTransform
        {
            get => leftHand;
            set => leftHand = value;
        }
        public Transform RightHandTransform
        {
            get => rightHand;
            set => rightHand = value;
        }

        public Transform PivotParent => pivotParent;
        public bool HasChanged => transform.hasChanged;
        public void UpdatePivots()
        {
            if (!pivotParent)
            {
                pivotParent = new GameObject("pivotParent").transform;
                pivotParent.position = interactableObject.transform.position;
                pivotParent.rotation = interactableObject.rotation;
            }

            pivotParent.parent = null;
            pivotParent.localScale = Vector3.one;
            pivotParent.parent = interactableObject;
            string handName = "leftHand";
            var hand = leftHand;
            if (!leftHand)
            {
                hand = InitializeHandPivot(handName);
            }

            leftHand = hand;

        }

        public virtual void Initialize()
        {
            if (interactableObject) return;
            interactableObject = new GameObject("interactableObject").transform;
            interactableObject.parent = transform;
            interactableObject.localPosition = Vector3.zero;
        }

        private Transform InitializeHandPivot(string handName)
        {
            Transform hand;
            hand = new GameObject(handName).transform;
            hand.parent = pivotParent;
            hand.localPosition = Vector3.zero;
            return hand;
        }
    }
}
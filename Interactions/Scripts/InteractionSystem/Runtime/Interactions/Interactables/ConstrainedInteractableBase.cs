using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    public abstract class ConstrainedInteractableBase : InteractableBase,IPoseConstrainer
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

        private (Vector3 position, Quaternion rotation) _leftHandPivot;
        private (Vector3 position, Quaternion rotation) _rightHandPivot;
        private Transform _fakeHandTransform;

        private (Vector3 position, Quaternion rotation) _currentPivot;
        private float _positionLerper;

        public Transform InteractableObject
        {
            get => interactableObject;
            set => interactableObject = value;
        }

        protected override bool Select()
        {
            if (constraintsType == HandConstrainType.FreeHand) return false;
            CurrentInteractor.ToggleHandModel(false);
            if (constraintsType == HandConstrainType.HideHand)
                return false;

            _fakeHandTransform = (CurrentInteractor.HandIdentifier == HandIdentifier.Left ? leftHand : rightHand);
            _fakeHandTransform.gameObject.SetActive(true);
            _currentPivot = CurrentInteractor.HandIdentifier == HandIdentifier.Left ? _leftHandPivot : _rightHandPivot;
            var interactableTransform = CurrentInteractor.transform;
            if (smoothHandTransition)
            {
                _fakeHandTransform.position = interactableTransform.position;
                _fakeHandTransform.rotation = interactableTransform.rotation;
                _positionLerper = 0;
            }
            else
            {
                _fakeHandTransform.localRotation = _currentPivot.rotation;
                _fakeHandTransform.position = _currentPivot.position;
            }

            return false;
        }

        protected override void DeSelected()
        {
            CurrentInteractor.ToggleHandModel(true);
            leftHand.gameObject.SetActive(false);
            rightHand.gameObject.SetActive(false);
        }

        private void Awake()
        {
            _leftHandPivot = new()
            {
                position = leftHand.transform.localPosition,
                rotation = leftHand.transform.localRotation
            };
            _rightHandPivot = new()
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
            _positionLerper += Time.deltaTime * 10;
            _fakeHandTransform.localPosition =
                Vector3.Lerp(_fakeHandTransform.localPosition, _currentPivot.position, _positionLerper);
            _fakeHandTransform.localRotation =
                Quaternion.Lerp(_fakeHandTransform.localRotation, _currentPivot.rotation, _positionLerper);
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
        
        // New interface properties for backward compatibility
        public HandConstrainType ConstraintType => constraintsType;
        public bool UseSmoothTransitions => smoothHandTransition;
        public float TransitionSpeed => 10f;

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
            // Check if interactableObject already exists and is valid
            if (interactableObject != null ) return;
            
            // Check if there's already an interactableObject child
            var existingChild = transform.Find("interactableObject");
            if (existingChild != null)
            {
                interactableObject = existingChild;
                return;
            }
            
            // Create new interactableObject only if none exists
            interactableObject = new GameObject("interactableObject").transform;
            interactableObject.parent = transform;
            interactableObject.localPosition = Vector3.zero;
            
            #if UNITY_EDITOR
            Debug.Log($"Created new interactableObject for {gameObject.name} at {Time.time}");
            #endif
        }

        private Transform InitializeHandPivot(string handName)
        {
            Transform hand;
            hand = new GameObject(handName).transform;
            hand.parent = pivotParent;
            hand.localPosition = Vector3.zero;
            return hand;
        }
        
        /// <summary>
        /// Applies pose constraints to the specified interactor's hand.
        /// This method handles all constraint types (Hide, Free, Constrained) and smooth transitions.
        /// </summary>
        /// <param name="interactor">The interactor whose hand should be constrained.</param>
        public void ApplyConstraints(InteractorBase interactor)
        {
            // Use the existing Select logic for constraint application
            if (constraintsType == HandConstrainType.FreeHand) return;
            
            interactor.ToggleHandModel(false);
            if (constraintsType == HandConstrainType.HideHand) return;

            _fakeHandTransform = (interactor.Hand.HandIdentifier == HandIdentifier.Left ? leftHand : rightHand);
            _fakeHandTransform.gameObject.SetActive(true);
            _currentPivot = interactor.Hand.HandIdentifier == HandIdentifier.Left ? _leftHandPivot : _rightHandPivot;
            
            var interactableTransform = interactor.transform;
            if (smoothHandTransition)
            {
                _fakeHandTransform.position = interactableTransform.position;
                _fakeHandTransform.rotation = interactableTransform.rotation;
                _positionLerper = 0;
            }
            else
            {
                _fakeHandTransform.localRotation = _currentPivot.rotation;
                _fakeHandTransform.position = _currentPivot.position;
            }
        }
        
        /// <summary>
        /// Removes all pose constraints from the specified interactor's hand.
        /// This method restores the hand to its default state.
        /// </summary>
        /// <param name="interactor">The interactor whose hand should be unconstrained.</param>
        public void RemoveConstraints(InteractorBase interactor)
        {
            // Use the existing DeSelected logic for constraint removal
            interactor.ToggleHandModel(true);
            leftHand.gameObject.SetActive(false);
            rightHand.gameObject.SetActive(false);
        }
    }
}
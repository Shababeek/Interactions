using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Animations.Constraints;

namespace Shababeek.Interactions
{
    [RequireComponent(typeof(UnifiedPoseConstraintSystem))]
    public abstract class ConstrainedInteractableBase : InteractableBase
    {
        [SerializeField] protected Transform interactableObject;
        [SerializeField] private float snapDistance = .5f;

        // Cache for fake hands
        private HandPoseController _leftFakeHand;
        private HandPoseController _rightFakeHand;
        private HandPoseController _currentFakeHand;
        
        // Private reference to the required UnifiedPoseConstraintSystem
        private UnifiedPoseConstraintSystem _poseConstraintSystem;

        public Transform InteractableObject
        {
            get => interactableObject;
            set => interactableObject = value;
        }

        protected override bool Select()
        {
            if (!_poseConstraintSystem) _poseConstraintSystem = GetComponent<UnifiedPoseConstraintSystem>();
            var handIdentifier = CurrentInteractor.HandIdentifier;
            if (poseConstraintSystem.ConstraintType == HandConstrainType.Constrained)
            {
                _currentFakeHand = GetOrCreateFakeHand(handIdentifier);
                poseConstraintSystem.ApplyConstraints(_currentFakeHand);
                CurrentInteractor.ToggleHandModel(false);
                PositionFakeHand(_currentFakeHand.transform, handIdentifier);
            }
            else
            {
                poseConstraintSystem.ApplyConstraints(CurrentInteractor.Hand);
            }

            HandleObjectMovement();
            
            return false;
        }

        protected override void DeSelected()
        {
            CurrentInteractor.ToggleHandModel(true);
            
            if (_currentFakeHand != null)
            {
                _currentFakeHand.gameObject.SetActive(false);
                _currentFakeHand = null;
            }
            
            HandleObjectDeselection();
        }

        private Hand GetOrCreateFakeHand(HandIdentifier handIdentifier)
        {
            var cachedHand = handIdentifier == HandIdentifier.Left ? _leftFakeHand : _rightFakeHand;

            if (cachedHand != null)
            {
                cachedHand.gameObject.SetActive(true);
                return cachedHand;
            }

            var newFakeHand = CreateFakeHand(handIdentifier);

            if (handIdentifier == HandIdentifier.Left)
            {
                _leftFakeHand = newFakeHand;
            }
            else
            {
                _rightFakeHand = newFakeHand;
            }
            
            return newFakeHand;
        }

        private Hand CreateFakeHand(HandIdentifier handIdentifier)
        {
            var handData = CurrentInteractor.Hand.HandData;
            var handPrefab = (handIdentifier == HandIdentifier.Left
                ? handData.LeftHandPrefab
                : handData.RightHandPrefab).GetComponent<Hand>();

            var fakeHand = Instantiate(handPrefab);
            var fakeHandTransform = fakeHand.transform;
            fakeHandTransform.position = CurrentInteractor.transform.position;
            fakeHandTransform.rotation = CurrentInteractor.transform.rotation;
            fakeHandTransform.parent = interactableObject.transform;
            fakeHand.name = $"FakeHand_{handData.name}_{handIdentifier}";
            return fakeHand;
        }


        private void PositionFakeHand(Transform fakeHand, HandIdentifier handIdentifier)
        {
            if (fakeHand == null || _poseConstraintSystem == null) return;
            
            var positioning = _poseConstraintSystem.GetTargetHandTransform(handIdentifier);
            
            if (_poseConstraintSystem.UseSmoothTransitions)
            {
                // Lerp from current hand position to target position
                var currentInteractorTransform = CurrentInteractor.transform;
                fakeHand.position = Vector3.Lerp(currentInteractorTransform.position, positioning.position, Time.deltaTime * _poseConstraintSystem.TransitionSpeed);
                fakeHand.rotation = Quaternion.Lerp(currentInteractorTransform.rotation, positioning.rotation, Time.deltaTime * _poseConstraintSystem.TransitionSpeed);
            }
            else
            {
                // Instant positioning
                fakeHand.localPosition = positioning.position;
                fakeHand.localRotation = positioning.rotation;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            if (interactableObject != null) return;

            var existingChild = transform.Find("interactableObject");
            if (existingChild != null)
            {
                interactableObject = existingChild;
                return;
            }

            interactableObject = new GameObject("interactableObject").transform;
            interactableObject.parent = transform;
            interactableObject.localPosition = Vector3.zero;
            Debug.Log($"Created new interactableObject for {gameObject.name} at {Time.time}");
        }

        private void OnDestroy()
        {
            if (_leftFakeHand != null)
            {
                DestroyImmediate(_leftFakeHand.gameObject);
            }

            if (_rightFakeHand != null)
            {
                DestroyImmediate(_rightFakeHand.gameObject);
            }
        }

        protected abstract void HandleObjectMovement();
        protected abstract void HandleObjectDeselection();
    }
}
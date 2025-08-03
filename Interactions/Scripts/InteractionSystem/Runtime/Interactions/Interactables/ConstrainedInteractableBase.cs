using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Animations.Constraints;

namespace Shababeek.Interactions
{
    [RequireComponent(typeof(UnifiedPoseConstraintSystem))]
    public abstract class ConstrainedInteractableBase : InteractableBase
    {
        [SerializeField] protected Transform interactableObject;
        [SerializeField] private float _snapDistance = .5f;

        // Cache for fake hands
        private Hand _leftFakeHand;
        private Hand _rightFakeHand;
        private Hand _currentFakeHand;
        
        // Private reference to the required UnifiedPoseConstraintSystem
        private UnifiedPoseConstraintSystem _poseConstraintSystem;
        
        // Smooth transition tracking
        private float _transitionProgress = 0f;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isTransitioning = false;

        public Transform InteractableObject
        {
            get => interactableObject;
            set => interactableObject = value;
        }

        protected override bool Select()
        {
            if (!_poseConstraintSystem) _poseConstraintSystem = GetComponent<UnifiedPoseConstraintSystem>();
            var handIdentifier = CurrentInteractor.HandIdentifier;
            if (_poseConstraintSystem.ConstraintType == HandConstrainType.Constrained)
            {
                _currentFakeHand = GetOrCreateFakeHand(handIdentifier);
                _poseConstraintSystem.ApplyConstraints(_currentFakeHand);
                CurrentInteractor.ToggleHandModel(false);
                PositionFakeHand(_currentFakeHand.transform, handIdentifier);
            }
            else
            {
                _poseConstraintSystem.ApplyConstraints(CurrentInteractor.Hand);
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
                // Start smooth transition
                _startPosition = fakeHand.position;
                _startRotation = fakeHand.rotation;
                _targetPosition = positioning.position;
                _targetRotation = positioning.rotation;
                _transitionProgress = 0f;
                _isTransitioning = true;
            }
            else
            {
                // Instant positioning - use world space since GetTargetHandTransform returns world coordinates
                fakeHand.position = positioning.position;
                fakeHand.rotation = positioning.rotation;
                _isTransitioning = false;
            }
        }

        private void Update()
        {
            // Handle ongoing smooth transitions
            if (_isTransitioning && _currentFakeHand != null)
            {
                _transitionProgress += Time.deltaTime * _poseConstraintSystem.TransitionSpeed;
                var t = Mathf.Clamp01(_transitionProgress);
                
                _currentFakeHand.transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);
                _currentFakeHand.transform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, t);
                
                if (t >= 1f)
                {
                    _isTransitioning = false;
                }
            }
            
            // Handle object movement if selected
            if (IsSelected)
            {
                HandleObjectMovement();
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
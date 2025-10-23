using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Animations.Constraints;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Base class for interactables that use constrained hand poses during interaction.
    /// Manages fake hand creation, pose constraints, and smooth transitions.
    /// </summary>
    [RequireComponent(typeof(PoseConstrainter))]
    public abstract class ConstrainedInteractableBase : InteractableBase
    {
        [Tooltip("Transform representing the object that will be manipulated during interaction.")]
        [SerializeField] protected Transform interactableObject;
        [Tooltip("Distance threshold for snapping the fake hand to the target position.")]
        [SerializeField] private float _snapDistance = .5f;

        private Hand _leftFakeHand;
        private Hand _rightFakeHand;
        private Hand _currentFakeHand;
        private PoseConstrainter _poseConstrainter;
        private float _transitionProgress = 0f;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isTransitioning = false;

        /// <summary>
        /// Transform representing the object being manipulated during interaction.
        /// </summary>
        public Transform InteractableObject
        {
            get => interactableObject;
            set => interactableObject = value;
        }

        /// <summary>
        /// Gets the transform that should be used for manipulation.
        /// Uses the interactableObject for constrained interactions.
        /// </summary>
        protected Transform ManipulationTarget => interactableObject ? interactableObject : ConstraintTransform;
        
        protected override void ValidateInteractableObject()
        {
            if (!_scaleCompensator) return;
            
            // If we already have a valid reference, just validate its location
            if (interactableObject != null)
            {
                if (interactableObject.parent == _scaleCompensator)
                {
                    return; // Already valid
                }
                else
                {
                    // Reference exists but in wrong location - move it
                    Debug.Log($"Moving interactableObject into ScaleCompensator for {gameObject.name}", this);
                    interactableObject.SetParent(_scaleCompensator, true);
                    return;
                }
            }
            
            // No reference - check if object exists in correct location
            var existing = _scaleCompensator.Find("interactableObject");
            if (existing != null)
            {
                interactableObject = existing;
                return;
            }
            
            // Check if it exists elsewhere (wrong location)
            var wrongLocation = transform.Find("interactableObject");
            if (wrongLocation != null)
            {
                Debug.Log($"Moving interactableObject into ScaleCompensator for {gameObject.name}", this);
                wrongLocation.SetParent(_scaleCompensator, true);
                interactableObject = wrongLocation;
                return;
            }
            
            // Create new interactableObject
            CreateInteractableObject();
        }
        
        protected override void CreateInteractableObject()
        {
            interactableObject = new GameObject("interactableObject").transform;
            interactableObject.SetParent(_scaleCompensator, false);
            interactableObject.localPosition = Vector3.zero;
            interactableObject.localRotation = Quaternion.identity;
            interactableObject.localScale = Vector3.one;
        }


        protected override bool Select()
        {
            if (!_poseConstrainter) _poseConstrainter = GetComponent<PoseConstrainter>();
            var handIdentifier = CurrentInteractor.HandIdentifier;
            if (_poseConstrainter.ConstraintType == HandConstrainType.Constrained)
            {
                _currentFakeHand = GetOrCreateFakeHand(handIdentifier);
                _poseConstrainter.ApplyConstraints(_currentFakeHand);
                CurrentInteractor.ToggleHandModel(false);
                PositionFakeHand(_currentFakeHand.transform, handIdentifier);
            }
            else
            {
                _poseConstrainter.ApplyConstraints(CurrentInteractor.Hand);
            }

            HandleObjectMovement();

            return false;
        }

        protected override void DeSelected()
        {
            CurrentInteractor.ToggleHandModel(true);

            if (_currentFakeHand)
            {
                _currentFakeHand.gameObject.SetActive(false);
                _currentFakeHand = null;
            }

            HandleObjectDeselection();
        }

        private Hand GetOrCreateFakeHand(HandIdentifier handIdentifier)
        {
            var cachedHand = handIdentifier == HandIdentifier.Left ? _leftFakeHand : _rightFakeHand;

            if (cachedHand)
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
            fakeHandTransform.SetParent(ManipulationTarget, false);
            fakeHand.name = $"FakeHand_{handData.name}_{handIdentifier}";
            return fakeHand;
        }

        private void PositionFakeHand(Transform fakeHand, HandIdentifier handIdentifier)
        {
            if (!fakeHand || !_poseConstrainter) return;

            var positioning = _poseConstrainter.GetRelativeTargetHandTransform(ManipulationTarget, handIdentifier);
            if (_poseConstrainter.UseSmoothTransitions)
            {
                _startPosition = ManipulationTarget.InverseTransformPoint(CurrentInteractor.transform.position);
                _startRotation = Quaternion.Inverse(ManipulationTarget.rotation) * CurrentInteractor.transform.rotation;
                _targetPosition = positioning.position;
                _targetRotation = positioning.rotation;
                _transitionProgress = 0f;
                _isTransitioning = true;
            }
            else
            {
                fakeHand.localPosition = positioning.position;
                fakeHand.localRotation = positioning.rotation;
                _isTransitioning = false;
            }
        }

        private void Update()
        {
            if (_isTransitioning && _currentFakeHand)
            {
                _transitionProgress += Time.deltaTime * _poseConstrainter.TransitionSpeed;
                var t = Mathf.Clamp01(_transitionProgress);
                _currentFakeHand.transform.localPosition = Vector3.Lerp(_startPosition, _targetPosition, t);
                _currentFakeHand.transform.localRotation = Quaternion.Lerp(_startRotation, _targetRotation, t);

                if (t >= 1f)
                {
                    _isTransitioning = false;
                }
            }

            if (IsSelected)
            {
                HandleObjectMovement();
            }
        }

        public override void InitializeInteractable()
        {
            base.InitializeInteractable();
            DestroyFakeHands();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyFakeHands();
        }

        private void DestroyFakeHands()
        {
            if (_leftFakeHand)
            {
                DestroyImmediate(_leftFakeHand.gameObject);
            }

            if (_rightFakeHand)
            {
                DestroyImmediate(_rightFakeHand.gameObject);
            }
        }

        /// <summary>
        /// Called each frame while the interactable is selected to handle manipulation.
        /// </summary>
        protected abstract void HandleObjectMovement();
        
        /// <summary>
        /// Called when the interactable is deselected to handle cleanup.
        /// </summary>
        protected abstract void HandleObjectDeselection();
    }
}
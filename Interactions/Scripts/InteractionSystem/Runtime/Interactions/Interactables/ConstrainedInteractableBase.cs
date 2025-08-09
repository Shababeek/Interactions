using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Animations.Constraints;

namespace Shababeek.Interactions
{
    [RequireComponent(typeof(PoseConstrainter))]
    public abstract class ConstrainedInteractableBase : InteractableBase
    {
        [SerializeField] protected Transform interactableObject;
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

        public Transform InteractableObject
        {
            get => interactableObject;
            set => interactableObject = value;
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
            if (!fakeHand || !_poseConstrainter) return;

            var positioning = _poseConstrainter.GetRelativeTargetHandTransform(interactableObject, handIdentifier);
            if (_poseConstrainter.UseSmoothTransitions)
            {
                _startPosition = interactableObject.InverseTransformPoint(CurrentInteractor.transform.position);
                _startRotation = Quaternion.Inverse(interactableObject.rotation) * CurrentInteractor.transform.rotation;
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
            if (_isTransitioning && _currentFakeHand != null)
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

        private void Awake()
        {
            DestroyFakeHands();
            Initialize();
            CompnstateScale();
        }

        private void CompnstateScale()
        {
            if(!interactableObject.parent)return;
            var scaleCompensator = new GameObject("ScaleCompensator").transform;
            scaleCompensator.parent = interactableObject.parent;
            scaleCompensator.position = interactableObject.position;
            scaleCompensator.rotation = interactableObject.rotation;
            var x = 1 / interactableObject.parent.lossyScale.x;
            var y = 1 / interactableObject.parent.lossyScale.y;
            var z = 1 / interactableObject.parent.lossyScale.z;
            scaleCompensator.localScale =  new Vector3(x, y, z);
            interactableObject.parent = scaleCompensator;
            interactableObject = scaleCompensator;
        }

        public void Initialize()
        {
            if (interactableObject != null) return;

            var existingChild = transform.Find("interactableObject");
            if (existingChild != null)
            {
                interactableObject = existingChild;

                return;
            }
            interactableObject = new GameObject("interactableObject").transform;
            interactableObject.localPosition = Vector3.zero;
        }


        private void OnDestroy()
        {
            DestroyFakeHands();
        }

        private void DestroyFakeHands()
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
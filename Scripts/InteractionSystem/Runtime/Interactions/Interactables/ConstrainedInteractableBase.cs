using System;
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

        [SerializeField] protected bool returnWhenDeselected;
        [SerializeField] protected float returnSpeed;
        protected bool IsReturning = false;
        protected PoseConstrainter PoseConstrainer;

        private Hand _leftFakeHand;
        private Hand _rightFakeHand;
        private Hand _currentFakeHand;
        private Transform _leftFakeHandWrapper;
        private Transform _rightFakeHandWrapper;
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

        protected Transform ManipulationTarget => interactableObject ? interactableObject : ConstraintTransform;

        protected virtual void Update()
        {
            if (_isTransitioning && _currentFakeHand)
            {
                _transitionProgress += Time.deltaTime * PoseConstrainer.TransitionSpeed;
                var t = Mathf.Clamp01(_transitionProgress);
                // Position on wrapper (in InteractableObject's space), rotation on hand (in uniform wrapper space)
                _currentFakeHand.transform.parent.localPosition = Vector3.Lerp(_startPosition, _targetPosition, t);
                _currentFakeHand.transform.localRotation = Quaternion.Lerp(_startRotation, _targetRotation, t);

                if (t >= 1f)
                {
                    _isTransitioning = false;
                }
            }

            if (IsSelected)
            {
                HandleObjectMovement(CurrentInteractor.transform.position);
            }

            if (returnWhenDeselected && IsReturning)
            {
                HandleReturnToOriginalPosition();
            }
        }

        protected abstract void HandleReturnToOriginalPosition();

        protected override bool Select()
        {
            if (!PoseConstrainer) PoseConstrainer = GetComponent<PoseConstrainter>();
            var handIdentifier = CurrentInteractor.HandIdentifier;
            Vector3 interactionPoint = CurrentInteractor.GetInteractionPoint();

            if (PoseConstrainer.ConstraintType == HandConstrainType.Constrained ||
                PoseConstrainer.ConstraintType == HandConstrainType.MultiPoint)
            {
                _currentFakeHand = GetOrCreateFakeHand(handIdentifier);
                PoseConstrainer.ApplyConstraints(_currentFakeHand, interactionPoint);
                CurrentInteractor.ToggleHandModel(false);
                PositionFakeHand(_currentFakeHand.transform, handIdentifier);
            }
            else
            {
                PoseConstrainer.ApplyConstraints(CurrentInteractor.Hand, interactionPoint);
            }

            HandleObjectMovement(CurrentInteractor.transform.position);

            return false;
        }

        protected override void DeSelected()
        {
            IsReturning = returnWhenDeselected;
            CurrentInteractor.ToggleHandModel(true);
            if (_currentFakeHand)
            {
                _currentFakeHand.gameObject.SetActive(false);
                _currentFakeHand = null;
            }

            HandleObjectDeselection();
        }

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

            // Create a scale-compensation wrapper so the hand rotation isn't skewed
            // by non-uniform parent scale. Position goes on the wrapper (in the parent's
            // skewed space — fine for position), rotation goes on the hand (in the
            // wrapper's uniform space — no shearing).
            var wrapper = new GameObject($"FakeHandWrapper_{handIdentifier}").transform;
            wrapper.SetParent(InteractableObject, false);
            wrapper.localRotation = Quaternion.identity;

            var parentScale = InteractableObject.lossyScale;
            wrapper.localScale = new Vector3(
                1f / parentScale.x,
                1f / parentScale.y,
                1f / parentScale.z
            );

            var fakeHandTransform = fakeHand.transform;
            fakeHandTransform.SetParent(wrapper, false);
            fakeHandTransform.localScale = Vector3.one;
            fakeHandTransform.localPosition = Vector3.zero;
            fakeHandTransform.localRotation = Quaternion.identity;

            if (handIdentifier == HandIdentifier.Left)
                _leftFakeHandWrapper = wrapper;
            else
                _rightFakeHandWrapper = wrapper;

            fakeHand.name = $"FakeHand_{handData.name}_{handIdentifier}";
            return fakeHand;
        }

        protected virtual void PositionFakeHand(Transform fakeHand, HandIdentifier handIdentifier)
        {
            if (!fakeHand || !PoseConstrainer) return;

            var positioning = PoseConstrainer.GetTargetHandTransform(handIdentifier);
            var wrapper = fakeHand.parent;

            if (PoseConstrainer.UseSmoothTransitions)
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
                // Position on wrapper (in InteractableObject's space)
                wrapper.localPosition = positioning.position;
                // Rotation on hand (in wrapper's uniform space — no skewing)
                fakeHand.localRotation = positioning.rotation;
                _isTransitioning = false;
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
                if (_leftFakeHandWrapper) DestroyImmediate(_leftFakeHandWrapper.gameObject);
                else DestroyImmediate(_leftFakeHand.gameObject);
                _leftFakeHand = null;
                _leftFakeHandWrapper = null;
            }

            if (_rightFakeHand)
            {
                if (_rightFakeHandWrapper) DestroyImmediate(_rightFakeHandWrapper.gameObject);
                else DestroyImmediate(_rightFakeHand.gameObject);
                _rightFakeHand = null;
                _rightFakeHandWrapper = null;
            }
        }

        /// <summary>
        /// Called each frame while the interactable is selected to handle manipulation.
        /// </summary>
        protected abstract void HandleObjectMovement(Vector3 target);

        /// <summary>
        /// Called when the interactable is deselected to handle cleanup.
        /// </summary>
        protected abstract void HandleObjectDeselection();
        protected override void OnDisable()
        {
            base.OnDisable();
            if (returnWhenDeselected)
            {
                ReturnWhenDisabled();
            }
        }

        private async void ReturnWhenDisabled()
        {
            try
            {
                IsReturning = true;
                await Awaitable.NextFrameAsync();

                while (!enabled)
                {
                    await Awaitable.NextFrameAsync();
                    HandleReturnToOriginalPosition();
                }
            }
            catch (Exception e)
            { 
                // TODO handle exception
            }
        }
    }
}
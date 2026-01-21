using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Types of grab/hold conditions that can trigger step completion.
    /// </summary>
    public enum GrabHoldCondition
    {
        /// <summary>Triggered when the object is grabbed.</summary>
        OnGrab = 0,
        /// <summary>Triggered when the object is released.</summary>
        OnRelease = 1,
        /// <summary>Triggered when the object is held for a duration.</summary>
        HoldDuration = 2,
        /// <summary>Triggered when the object is grabbed with a specific hand.</summary>
        GrabWithHand = 3,
    }

    /// <summary>
    /// Completes a step based on grab/hold interactions with an interactable object.
    /// Supports various conditions like grab, release, hold duration, and specific hand requirements.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/GrabHoldAction")]
    public class GrabHoldAction : AbstractSequenceAction
    {
        [Tooltip("The interactable object to monitor.")]
        [SerializeField] private InteractableBase interactable;

        [Tooltip("The condition that triggers step completion.")]
        [SerializeField] private GrabHoldCondition condition = GrabHoldCondition.OnGrab;

        [Tooltip("Duration the object must be held (only used with HoldDuration condition).")]
        [SerializeField] private float holdDuration = 2f;

        [Tooltip("Required hand for GrabWithHand condition.")]
        [SerializeField] private HandIdentifier requiredHand = HandIdentifier.Right;

        private bool _isHeld = false;
        private float _holdTime = 0f;
        private InteractorBase _currentInteractor;

        private void Subscribe()
        {
            if (interactable == null) return;

            interactable.OnSelected
                .Do(OnGrabbed)
                .Subscribe()
                .AddTo(StepDisposable);

            interactable.OnDeselected
                .Do(OnReleased)
                .Subscribe()
                .AddTo(StepDisposable);
        }

        private void OnGrabbed(InteractorBase interactor)
        {
            _isHeld = true;
            _holdTime = 0f;
            _currentInteractor = interactor;

            switch (condition)
            {
                case GrabHoldCondition.OnGrab:
                    CompleteStep();
                    break;

                case GrabHoldCondition.GrabWithHand:
                    if (interactor != null && interactor.HandIdentifier == requiredHand)
                    {
                        CompleteStep();
                    }
                    break;
            }
        }

        private void OnReleased(InteractorBase interactor)
        {
            _isHeld = false;
            _currentInteractor = null;

            if (condition == GrabHoldCondition.OnRelease)
            {
                CompleteStep();
            }
        }

        private void Update()
        {
            if (!Started || condition != GrabHoldCondition.HoldDuration) return;

            if (_isHeld)
            {
                _holdTime += Time.deltaTime;
                if (_holdTime >= holdDuration)
                {
                    CompleteStep();
                }
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                _holdTime = 0f;
                _isHeld = false;
                Subscribe();
            }
            // StepDisposable cleanup is handled by base class
        }

        /// <summary>
        /// Gets the current hold progress as a value between 0 and 1 (only relevant for HoldDuration condition).
        /// </summary>
        public float HoldProgress => holdDuration > 0 ? Mathf.Clamp01(_holdTime / holdDuration) : 0f;

        /// <summary>
        /// Gets whether the object is currently being held.
        /// </summary>
        public bool IsHeld => _isHeld;
    }
}

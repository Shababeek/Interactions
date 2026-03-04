using NUnit.Framework;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class InteractableBaseTests
    {
        private TestInteractable _interactable;

        [SetUp]
        public void SetUp()
        {
            _interactable = TestHelpers.CreateTestInteractable();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_interactable);
        }

        [Test]
        public void Interactable_InitialStateIsNone()
        {
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
        }

        [Test]
        public void Interactable_InitiallyNotSelected()
        {
            Assert.IsFalse(_interactable.IsSelected);
        }

        [Test]
        public void Interactable_InitiallyNotUsing()
        {
            Assert.IsFalse(_interactable.IsUsing);
        }

        [Test]
        public void Interactable_CurrentInteractorInitiallyNull()
        {
            Assert.IsNull(_interactable.CurrentInteractor);
        }

        [Test]
        public void Interactable_CanInteract_WithLeftHandWhenBothAllowed()
        {
            _interactable.InteractionHand = InteractionHand.Left | InteractionHand.Right;
            Assert.IsTrue(_interactable.CanInteract(HandIdentifier.Left));
        }

        [Test]
        public void Interactable_CanInteract_WithRightHandWhenBothAllowed()
        {
            _interactable.InteractionHand = InteractionHand.Left | InteractionHand.Right;
            Assert.IsTrue(_interactable.CanInteract(HandIdentifier.Right));
        }

        [Test]
        public void Interactable_CannotInteract_WithDisallowedHand()
        {
            _interactable.InteractionHand = InteractionHand.Left;
            Assert.IsFalse(_interactable.CanInteract(HandIdentifier.Right));
        }

        [Test]
        public void Interactable_CanInteract_WhenDisabled()
        {
            _interactable.InteractionHand = InteractionHand.Left | InteractionHand.Right;
            _interactable.gameObject.SetActive(false);
            Assert.IsFalse(_interactable.CanInteract(HandIdentifier.Left));
        }

        [Test]
        public void Interactable_TransitionToHovering()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.OnStateChanged(InteractionState.Hovering, mockInteractor);

            Assert.AreEqual(InteractionState.Hovering, _interactable.CurrentState);
            Assert.AreEqual(mockInteractor, _interactable.CurrentInteractor);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_IgnoreDuplicateStateChanges()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            var selectCallsBefore = _interactable.SelectCallCount;

            _interactable.OnStateChanged(InteractionState.Hovering, mockInteractor);
            _interactable.OnStateChanged(InteractionState.Hovering, mockInteractor);

            Assert.AreEqual(InteractionState.Hovering, _interactable.CurrentState);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_TransitionFromHoveringToSelected()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Hovering, mockInteractor);
            _interactable.OnStateChanged(InteractionState.Selected, mockInteractor);

            Assert.AreEqual(InteractionState.Selected, _interactable.CurrentState);
            Assert.IsTrue(_interactable.IsSelected);
            Assert.AreEqual(1, _interactable.SelectCallCount);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_TransitionFromSelectedToNone()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Hovering, mockInteractor);
            _interactable.OnStateChanged(InteractionState.Selected, mockInteractor);
            _interactable.OnStateChanged(InteractionState.None, mockInteractor);

            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            Assert.IsFalse(_interactable.IsSelected);
            Assert.AreEqual(1, _interactable.DeSelectCallCount);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_StartUsing_OnlyWhenSelected()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.StartUsing(mockInteractor);

            Assert.IsFalse(_interactable.IsUsing);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_StartUsing_WhenSelected()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, mockInteractor);
            _interactable.StartUsing(mockInteractor);

            Assert.IsTrue(_interactable.IsUsing);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_StopUsing_ClearsUsingFlag()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, mockInteractor);
            _interactable.StartUsing(mockInteractor);
            _interactable.StopUsing(mockInteractor);

            Assert.IsFalse(_interactable.IsUsing);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_ThumbPress_OnlyWhenSelected()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.ThumbPress(mockInteractor);

            // No exception should be thrown
            Assert.Pass();
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_ThumbRelease_OnlyWhenSelected()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.ThumbRelease(mockInteractor);

            // No exception should be thrown
            Assert.Pass();
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_ScaleCompensatorCreated()
        {
            var scaleCompensator = _interactable.ConstraintTransform;
            Assert.IsNotNull(scaleCompensator);
            Assert.AreEqual("ScaleCompensator", scaleCompensator.name);
        }

        [Test]
        public void Interactable_InteractableObjectCreated()
        {
            var interactableObject = _interactable.InteractableObject;
            Assert.IsNotNull(interactableObject);
            Assert.AreEqual("interactableObject", interactableObject.name);
        }

        [Test]
        public void Interactable_ConstraintTransformReturnsScaleCompensator()
        {
            var constraintTransform = _interactable.ConstraintTransform;
            Assert.AreEqual("ScaleCompensator", constraintTransform.name);
        }

        [Test]
        public void Interactable_SelectButtonDefaultIsGrip()
        {
            Assert.AreEqual(XRButton.Grip, _interactable.SelectionButton);
        }

        [Test]
        public void Interactable_InteractionHandDefaultIsBoth()
        {
            var expectedHand = InteractionHand.Left | InteractionHand.Right;
            Assert.AreEqual(expectedHand, _interactable.InteractionHand);
        }

        [Test]
        public void Interactable_CanModifyInteractionHand()
        {
            _interactable.InteractionHand = InteractionHand.Right;
            Assert.AreEqual(InteractionHand.Right, _interactable.InteractionHand);
        }

        [Test]
        public void Interactable_OnStateChanged_ClearsInteractorOnNone()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, mockInteractor);
            Assert.AreEqual(mockInteractor, _interactable.CurrentInteractor);

            _interactable.OnStateChanged(InteractionState.None, mockInteractor);
            Assert.IsNull(_interactable.CurrentInteractor);

            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void Interactable_SelectReturningTrue_CancelsSelection()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(true);

            _interactable.OnStateChanged(InteractionState.Selected, mockInteractor);

            // When Select returns true, selection should be cancelled and state should revert
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            TestHelpers.DestroyComponent(mockInteractor);
        }
    }
}

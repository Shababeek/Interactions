using NUnit.Framework;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class InteractableStateFlowTests
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
        public void StateFlow_NoneToHoveringToSelected()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            // None -> Hovering
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            Assert.AreEqual(InteractionState.Hovering, _interactable.CurrentState);

            // Hovering -> Selected
            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            Assert.AreEqual(InteractionState.Selected, _interactable.CurrentState);
            Assert.IsTrue(_interactable.IsSelected);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_SelectedToHoveringToNone()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            Assert.AreEqual(InteractionState.Selected, _interactable.CurrentState);

            // Selected -> Hovering
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            Assert.AreEqual(InteractionState.Hovering, _interactable.CurrentState);
            Assert.IsFalse(_interactable.IsSelected);

            // Hovering -> None
            _interactable.OnStateChanged(InteractionState.None, interactor);
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_DirectSelectionSkipsHovering()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            // None -> Selected directly (skipping Hovering)
            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            Assert.AreEqual(InteractionState.Selected, _interactable.CurrentState);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_RapidStateChanges()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            _interactable.OnStateChanged(InteractionState.None, interactor);

            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            Assert.AreEqual(1, _interactable.SelectCallCount);
            Assert.AreEqual(1, _interactable.DeSelectCallCount);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_DuplicateStateChangesIgnored()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);

            Assert.AreEqual(InteractionState.Hovering, _interactable.CurrentState);
            Assert.AreEqual(0, _interactable.SelectCallCount);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_InteractorUpdatesOnStateChange()
        {
            var interactor1 = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            var interactor2 = TestHelpers.CreateMockInteractor(HandIdentifier.Right);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, interactor1);
            Assert.AreEqual(interactor1, _interactable.CurrentInteractor);

            _interactable.OnStateChanged(InteractionState.None, interactor1);
            _interactable.OnStateChanged(InteractionState.Selected, interactor2);
            Assert.AreEqual(interactor2, _interactable.CurrentInteractor);

            TestHelpers.DestroyComponent(interactor1);
            TestHelpers.DestroyComponent(interactor2);
        }

        [Test]
        public void StateFlow_SelectFailureReverts()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(true); // Select fails

            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_ClearsInteractorOnNoneState()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            Assert.IsNotNull(_interactable.CurrentInteractor);

            _interactable.OnStateChanged(InteractionState.None, interactor);
            Assert.IsNull(_interactable.CurrentInteractor);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_HoveringClearsOnDirectTransitionToNone()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);

            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            Assert.AreEqual(InteractionState.Hovering, _interactable.CurrentState);

            _interactable.OnStateChanged(InteractionState.None, interactor);
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            Assert.IsNull(_interactable.CurrentInteractor);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_MultipleSelectAttempts()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            // First select succeeds
            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            Assert.IsTrue(_interactable.IsSelected);
            var firstSelectCount = _interactable.SelectCallCount;

            // Reset to hovering
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            Assert.IsFalse(_interactable.IsSelected);

            // Second select attempt
            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            Assert.IsTrue(_interactable.IsSelected);
            Assert.Greater(_interactable.SelectCallCount, firstSelectCount);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void StateFlow_UseStateIndependentOfSelection()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            // Cannot use when not selected
            _interactable.StartUsing(interactor);
            Assert.IsFalse(_interactable.IsUsing);

            // Can use when selected
            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            _interactable.StartUsing(interactor);
            Assert.IsTrue(_interactable.IsUsing);

            TestHelpers.DestroyComponent(interactor);
        }
    }
}

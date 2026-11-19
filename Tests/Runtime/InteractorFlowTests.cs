using NUnit.Framework;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    /// <summary>
    /// Drives interactables through InteractorBase (hover, select, deselect) instead of calling
    /// OnStateChanged directly, so the interactor's own bookkeeping is covered.
    /// </summary>
    [TestFixture]
    public class InteractorFlowTests
    {
        private TestInteractable _interactable;
        private TestInteractor _left;
        private TestInteractor _right;

        [SetUp]
        public void SetUp()
        {
            _interactable = TestHelpers.CreateTestInteractable();
            _left = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _right = TestHelpers.CreateMockInteractor(HandIdentifier.Right);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_interactable);
            TestHelpers.DestroyComponent(_left);
            TestHelpers.DestroyComponent(_right);
        }

        [Test]
        public void Hover_StartsAndEndsThroughTheInteractor()
        {
            _left.Hover(_interactable);
            Assert.AreEqual(InteractionState.Hovering, _interactable.CurrentState);
            Assert.AreSame(_left, _interactable.CurrentInteractor);

            _left.Hover(null);
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            Assert.IsNull(_interactable.CurrentInteractor);
        }

        [Test]
        public void Select_CommitsTheInteractor_AndDeSelectReleasesBoth()
        {
            _left.Hover(_interactable);

            _left.Select();
            Assert.IsTrue(_interactable.IsSelected);
            Assert.IsTrue(_left.Interacting);
            Assert.AreSame(_left, _interactable.CurrentInteractor);

            _left.DeSelect();
            Assert.IsFalse(_interactable.IsSelected);
            Assert.IsFalse(_left.Interacting);
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            Assert.AreEqual(1, _interactable.DeSelectCallCount);
        }

        [Test]
        public void Select_Aborted_LeavesTheInteractorFree()
        {
            _interactable.SetSelectResult(true);
            _left.Hover(_interactable);

            _left.Select();

            Assert.IsFalse(_left.Interacting, "an aborted selection must not lock the interactor");
            Assert.IsFalse(_interactable.IsSelected);
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);

            var other = TestHelpers.CreateTestInteractable();
            _left.Hover(other);
            _left.Select();
            Assert.IsTrue(other.IsSelected);
            Assert.IsTrue(_left.Interacting);
            TestHelpers.DestroyComponent(other);
        }

        [Test]
        public void Select_HandNotAllowed_DoesNothing()
        {
            _interactable.InteractionHand = InteractionHand.Right;

            _left.Hover(_interactable);
            _left.Select();

            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            Assert.IsFalse(_left.Interacting);
        }

        [Test]
        public void SecondHand_CannotStealAHeldObject()
        {
            _left.Hover(_interactable);
            _left.Select();

            _right.Hover(_interactable);
            _right.Select();

            Assert.AreSame(_left, _interactable.CurrentInteractor);
            Assert.IsFalse(_right.Interacting);
            Assert.AreEqual(1, _interactable.SelectCallCount);
        }

        [Test]
        public void HandToHandPass_SecondHandOwnsTheObjectAfterTheFirstReleases()
        {
            _left.Hover(_interactable);
            _left.Select();
            _left.DeSelect();

            _right.Hover(_interactable);
            _right.Select();

            Assert.IsTrue(_interactable.IsSelected);
            Assert.AreSame(_right, _interactable.CurrentInteractor);
            Assert.IsTrue(_right.Interacting);
            Assert.IsFalse(_left.Interacting);
        }

        [Test]
        public void Release_ByTheInteractable_FreesTheInteractor()
        {
            _left.Hover(_interactable);
            _left.Select();

            _left.Release(_interactable);

            Assert.IsFalse(_interactable.IsSelected);
            Assert.IsFalse(_left.Interacting);
        }

        [Test]
        public void DestroyedWhileHovered_NextHoverStillWorks()
        {
            _left.Hover(_interactable);
            Object.DestroyImmediate(_interactable.gameObject);

            var other = TestHelpers.CreateTestInteractable();
            Assert.DoesNotThrow(() => _left.Hover(other));
            Assert.AreEqual(InteractionState.Hovering, other.CurrentState);
            TestHelpers.DestroyComponent(other);
        }
    }
}

using NUnit.Framework;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    /// <summary>
    /// InteractableBase defaults, hand rules, hierarchy, selection cancellation, and use/thumb gates.
    /// Multi-step state sequences live in <see cref="InteractableStateFlowTests"/>.
    /// </summary>
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
        public void InitialState_Defaults()
        {
                Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
                Assert.IsFalse(_interactable.IsSelected);
                Assert.IsFalse(_interactable.IsUsing);
                Assert.IsNull(_interactable.CurrentInteractor);
                Assert.AreEqual(XRButton.Grip, _interactable.SelectionButton);
                Assert.AreEqual(InteractionHand.Left | InteractionHand.Right, _interactable.InteractionHand);
            }

        [Test]
        public void CanInteract_RespectsHandFlagsAndActiveHierarchy()
        {
            _interactable.InteractionHand = InteractionHand.Left | InteractionHand.Right;
            Assert.IsTrue(_interactable.CanInteract(HandIdentifier.Left));
            Assert.IsTrue(_interactable.CanInteract(HandIdentifier.Right));

            _interactable.InteractionHand = InteractionHand.Left;
            Assert.IsFalse(_interactable.CanInteract(HandIdentifier.Right));

            _interactable.InteractionHand = InteractionHand.Left | InteractionHand.Right;
            _interactable.gameObject.SetActive(false);
            Assert.IsFalse(_interactable.CanInteract(HandIdentifier.Left));
        }

        [Test]
        public void Hierarchy_HasScaleCompensatorAndInteractableObjectChild()
        {
            var constraint = _interactable.ConstraintTransform;
            Assert.IsNotNull(constraint);
            Assert.AreEqual("ScaleCompensator", constraint.name);

            var interactableObject = _interactable.InteractableObject;
            Assert.IsNotNull(interactableObject);
            Assert.AreEqual("interactableObject", interactableObject.name);
        }

        [Test]
        public void StartUsing_BlockedUntilSelected()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.StartUsing(mockInteractor);
            Assert.IsFalse(_interactable.IsUsing);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void ThumbObservable_WhenNotSelected_DoesNotEmit()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            int presses = 0, releases = 0;
            using (_interactable.OnThumbPressed.Subscribe(_ => presses++))
            using (_interactable.OnThumbReleased.Subscribe(_ => releases++))
            {
                _interactable.ThumbPress(mockInteractor);
                _interactable.ThumbRelease(mockInteractor);
            }

            Assert.AreEqual(0, presses);
            Assert.AreEqual(0, releases);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void ThumbObservable_WhenSelected_EmitsCurrentInteractor()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);
            _interactable.OnStateChanged(InteractionState.Selected, mockInteractor);

            InteractorBase pressInteractor = null;
            InteractorBase releaseInteractor = null;
            using (_interactable.OnThumbPressed.Subscribe(i => pressInteractor = i))
            using (_interactable.OnThumbReleased.Subscribe(i => releaseInteractor = i))
            {
                _interactable.ThumbPress(mockInteractor);
                _interactable.ThumbRelease(mockInteractor);
            }

            Assert.AreEqual(mockInteractor, pressInteractor);
            Assert.AreEqual(mockInteractor, releaseInteractor);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void SelectReturningTrue_CancelsSelection()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(true);
            _interactable.OnStateChanged(InteractionState.Selected, mockInteractor);
            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            TestHelpers.DestroyComponent(mockInteractor);
        }
    }
}

using NUnit.Framework;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class ConstrainedInteractableTests
    {
        private TestConstrainedInteractable _constrainedInteractable;

        [SetUp]
        public void SetUp()
        {
            _constrainedInteractable = new GameObject("ConstrainedInteractable").AddComponent<TestConstrainedInteractable>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_constrainedInteractable);
        }

        [Test]
        public void AddingTheComponent_AlsoAddsAPoseConstrainer()
        {
            Assert.IsNotNull(_constrainedInteractable.GetComponent<PoseConstrainer>());
            Assert.AreSame(_constrainedInteractable.GetComponent<PoseConstrainer>(), _constrainedInteractable.Constrainer);
        }

        [Test]
        public void Defaults_MatchTheBaseInteractable()
        {
            Assert.AreEqual(InteractionState.None, _constrainedInteractable.CurrentState);
            Assert.IsFalse(_constrainedInteractable.IsSelected);
            Assert.IsNull(_constrainedInteractable.CurrentInteractor);
            Assert.AreEqual(XRButton.Grip, _constrainedInteractable.SelectionButton);
            Assert.IsTrue(_constrainedInteractable.CanInteract(HandIdentifier.Left));
            Assert.IsTrue(_constrainedInteractable.CanInteract(HandIdentifier.Right));
        }

        [Test]
        public void Hierarchy_IsRootThenScaleCompensatorThenInteractableObject()
        {
            var scaleCompensator = _constrainedInteractable.ConstraintTransform;
            var interactableObject = _constrainedInteractable.InteractableObject;

            Assert.AreEqual("ScaleCompensator", scaleCompensator.name);
            Assert.AreEqual("interactableObject", interactableObject.name);
            Assert.AreEqual(_constrainedInteractable.transform, scaleCompensator.parent);
            Assert.AreEqual(scaleCompensator, interactableObject.parent);
        }

        [Test]
        public void InteractableObject_CanBeReplacedWithACustomChild()
        {
            var customObject = new GameObject("CustomObject").transform;
            customObject.SetParent(_constrainedInteractable.ConstraintTransform);

            _constrainedInteractable.InteractableObject = customObject;

            Assert.AreEqual(customObject, _constrainedInteractable.InteractableObject);
        }

        [Test]
        public void Hover_ChangesStateAndRecordsTheInteractor()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);

            _constrainedInteractable.OnStateChanged(InteractionState.Hovering, interactor);

            Assert.AreEqual(InteractionState.Hovering, _constrainedInteractable.CurrentState);
            Assert.AreSame(interactor, _constrainedInteractable.CurrentInteractor);
            TestHelpers.DestroyComponent(interactor);
        }
    }

    /// <summary>
    /// Test implementation of ConstrainedInteractableBase for testing abstract methods.
    /// </summary>
    public class TestConstrainedInteractable : ConstrainedInteractableBase
    {
        protected override void HandleObjectMovement(Vector3 target)
        {
            // Test implementation - do nothing
        }

        protected override void HandleObjectDeselection()
        {
            // Test implementation - do nothing
        }

        protected override void HandleReturnToOriginalPosition()
        {
            // Test implementation - do nothing
        }
    }
}

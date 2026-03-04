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
            var go = new GameObject("ConstrainedInteractable");
            var poseConstrainter = go.AddComponent<TestPoseConstrainter>();
            _constrainedInteractable = go.AddComponent<TestConstrainedInteractable>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_constrainedInteractable);
        }

        [Test]
        public void ConstrainedInteractable_InitialStateIsNone()
        {
            Assert.AreEqual(InteractionState.None, _constrainedInteractable.CurrentState);
        }

        [Test]
        public void ConstrainedInteractable_InteractableObjectInitialized()
        {
            var interactableObject = _constrainedInteractable.InteractableObject;
            Assert.IsNotNull(interactableObject);
        }

        [Test]
        public void ConstrainedInteractable_ScaleCompensatorCreated()
        {
            var scaleCompensator = _constrainedInteractable.ConstraintTransform;
            Assert.IsNotNull(scaleCompensator);
            Assert.AreEqual("ScaleCompensator", scaleCompensator.name);
        }

        [Test]
        public void ConstrainedInteractable_InteractableObjectCreated()
        {
            var interactableObject = _constrainedInteractable.InteractableObject;
            Assert.IsNotNull(interactableObject);
            Assert.AreEqual("interactableObject", interactableObject.name);
        }

        [Test]
        public void ConstrainedInteractable_CanAssignCustomInteractableObject()
        {
            var customObject = new GameObject("CustomObject").transform;
            customObject.SetParent(_constrainedInteractable.ConstraintTransform);
            _constrainedInteractable.InteractableObject = customObject;

            Assert.AreEqual(customObject, _constrainedInteractable.InteractableObject);

            TestHelpers.DestroyGameObject(customObject.gameObject);
        }

        [Test]
        public void ConstrainedInteractable_InitiallyNotSelected()
        {
            Assert.IsFalse(_constrainedInteractable.IsSelected);
        }

        [Test]
        public void ConstrainedInteractable_CurrentInteractorInitiallyNull()
        {
            Assert.IsNull(_constrainedInteractable.CurrentInteractor);
        }

        [Test]
        public void ConstrainedInteractable_CanTransitionToHovering()
        {
            var mockInteractor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _constrainedInteractable.OnStateChanged(InteractionState.Hovering, mockInteractor);

            Assert.AreEqual(InteractionState.Hovering, _constrainedInteractable.CurrentState);
            TestHelpers.DestroyComponent(mockInteractor);
        }

        [Test]
        public void ConstrainedInteractable_InteractableObjectIsChild()
        {
            var interactableObject = _constrainedInteractable.InteractableObject;
            Assert.IsTrue(interactableObject.IsChildOf(_constrainedInteractable.ConstraintTransform));
        }

        [Test]
        public void ConstrainedInteractable_HierarchyStructure()
        {
            // Verify: InteractableBase -> ScaleCompensator -> interactableObject
            var scaleCompensator = _constrainedInteractable.ConstraintTransform;
            var interactableObject = _constrainedInteractable.InteractableObject;

            Assert.AreEqual(scaleCompensator, interactableObject.parent);
            Assert.AreEqual(_constrainedInteractable.transform, scaleCompensator.parent);
        }

        [Test]
        public void ConstrainedInteractable_CanInteractBothHands()
        {
            Assert.IsTrue(_constrainedInteractable.CanInteract(HandIdentifier.Left));
            Assert.IsTrue(_constrainedInteractable.CanInteract(HandIdentifier.Right));
        }

        [Test]
        public void ConstrainedInteractable_DefaultSelectButtonIsGrip()
        {
            Assert.AreEqual(XRButton.Grip, _constrainedInteractable.SelectionButton);
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

    /// <summary>
    /// Test implementation of PoseConstrainter for testing.
    /// </summary>
    public class TestPoseConstrainter : MonoBehaviour
    {
        public void ApplyConstraints(Hand hand, Vector3 interactionPoint)
        {
            // Test implementation
        }

        public void RemoveConstraints(Hand hand)
        {
            // Test implementation
        }

        public (Vector3 position, Quaternion rotation) GetTargetHandTransform(HandIdentifier handIdentifier)
        {
            return (Vector3.zero, Quaternion.identity);
        }
    }
}

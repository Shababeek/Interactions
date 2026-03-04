using NUnit.Framework;
using Shababeek.Interactions.Core;
using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class TransformGrabStrategyTests
    {
        private GameObject _grabbableGo;
        private Grabable _grabable;
        private TestInteractor _interactor;

        [SetUp]
        public void SetUp()
        {
            _grabbableGo = new GameObject("Grabbable");
            _grabbableGo.AddComponent<PoseConstrainter>();
            _grabbableGo.AddComponent<VariableTweener>();
            _grabable = _grabbableGo.AddComponent<Grabable>();

            _interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyGameObject(_grabbableGo);
            TestHelpers.DestroyComponent(_interactor);
        }

        // ── Initialize: Layer Management ──

        [Test]
        public void Initialize_ChangesGameObjectLayerToInteractorLayer()
        {
            _grabbableGo.layer = LayerMask.NameToLayer("Default");
            var strategy = new TransformGrabStrategy(_grabbableGo.transform);

            strategy.Initialize(_interactor);

            Assert.AreEqual(_interactor.gameObject.layer, _grabbableGo.layer);
        }

        [Test]
        public void Initialize_ChangesColliderLayersToInteractorLayer()
        {
            var collider = _grabbableGo.AddComponent<BoxCollider>();
            _grabbableGo.layer = LayerMask.NameToLayer("Default");
            var strategy = new TransformGrabStrategy(_grabbableGo.transform);

            strategy.Initialize(_interactor);

            Assert.AreEqual(_interactor.gameObject.layer, collider.gameObject.layer);
        }

        [Test]
        public void Initialize_ChangesAllChildColliderLayers()
        {
            var child1 = new GameObject("Child1");
            child1.transform.SetParent(_grabbableGo.transform);
            var col1 = child1.AddComponent<BoxCollider>();

            var child2 = new GameObject("Child2");
            child2.transform.SetParent(_grabbableGo.transform);
            var col2 = child2.AddComponent<SphereCollider>();

            var strategy = new TransformGrabStrategy(_grabbableGo.transform);
            strategy.Initialize(_interactor);

            Assert.AreEqual(_interactor.gameObject.layer, col1.gameObject.layer);
            Assert.AreEqual(_interactor.gameObject.layer, col2.gameObject.layer);
        }

        [Test]
        public void Initialize_NoColliders_DoesNotThrow()
        {
            var bareGo = new GameObject("NoColliders");
            var strategy = new TransformGrabStrategy(bareGo.transform);

            Assert.DoesNotThrow(() => strategy.Initialize(_interactor));

            TestHelpers.DestroyGameObject(bareGo);
        }

        // ── Grab: Transform Parenting ──

        [Test]
        public void Grab_ParentsObjectToAttachmentPoint()
        {
            var strategy = new TransformGrabStrategy(_grabbableGo.transform);
            strategy.Initialize(_interactor);

            strategy.Grab(_grabable, _interactor);

            Assert.AreEqual(_interactor.AttachmentPoint, _grabable.transform.parent);
        }

        [Test]
        public void Grab_SetsLocalPositionToZero()
        {
            _grabbableGo.transform.position = new Vector3(10f, 20f, 30f);
            var strategy = new TransformGrabStrategy(_grabbableGo.transform);
            strategy.Initialize(_interactor);

            strategy.Grab(_grabable, _interactor);

            Assert.AreEqual(Vector3.zero, _grabable.transform.localPosition);
        }

        [Test]
        public void Grab_SetsLocalRotationToIdentity()
        {
            _grabbableGo.transform.rotation = Quaternion.Euler(45f, 90f, 180f);
            var strategy = new TransformGrabStrategy(_grabbableGo.transform);
            strategy.Initialize(_interactor);

            strategy.Grab(_grabable, _interactor);

            Assert.AreEqual(Quaternion.identity, _grabable.transform.localRotation);
        }

        // ── UnGrab: Restore State ──

        [Test]
        public void UnGrab_DetachesFromParent()
        {
            var strategy = new TransformGrabStrategy(_grabbableGo.transform);
            strategy.Initialize(_interactor);
            strategy.Grab(_grabable, _interactor);

            strategy.UnGrab(_grabable, _interactor);

            Assert.IsNull(_grabable.transform.parent);
        }

        [Test]
        public void UnGrab_RestoresOriginalGameObjectLayer()
        {
            int originalLayer = LayerMask.NameToLayer("Default");
            _grabbableGo.layer = originalLayer;
            var strategy = new TransformGrabStrategy(_grabbableGo.transform);

            strategy.Initialize(_interactor);
            strategy.Grab(_grabable, _interactor);
            strategy.UnGrab(_grabable, _interactor);

            Assert.AreEqual(originalLayer, _grabbableGo.layer);
        }

        [Test]
        public void UnGrab_RestoresOriginalColliderLayers()
        {
            var child = new GameObject("Child");
            child.transform.SetParent(_grabbableGo.transform);
            var collider = child.AddComponent<BoxCollider>();
            int originalLayer = LayerMask.NameToLayer("Default");
            child.layer = originalLayer;

            var strategy = new TransformGrabStrategy(_grabbableGo.transform);
            strategy.Initialize(_interactor);
            strategy.Grab(_grabable, _interactor);
            strategy.UnGrab(_grabable, _interactor);

            Assert.AreEqual(originalLayer, collider.gameObject.layer);
        }

        // ── Full Grab Cycle ──

        [Test]
        public void FullCycle_Initialize_Grab_UnGrab_RestoresAllState()
        {
            var child = new GameObject("Child");
            child.transform.SetParent(_grabbableGo.transform);
            child.AddComponent<BoxCollider>();
            int originalGoLayer = _grabbableGo.layer;
            int originalChildLayer = child.layer;

            var strategy = new TransformGrabStrategy(_grabbableGo.transform);

            strategy.Initialize(_interactor);
            Assert.AreEqual(_interactor.gameObject.layer, _grabbableGo.layer);

            strategy.Grab(_grabable, _interactor);
            Assert.AreEqual(_interactor.AttachmentPoint, _grabable.transform.parent);

            strategy.UnGrab(_grabable, _interactor);
            Assert.IsNull(_grabable.transform.parent);
            Assert.AreEqual(originalGoLayer, _grabbableGo.layer);
            Assert.AreEqual(originalChildLayer, child.layer);
        }
    }

    [TestFixture]
    public class RigidBodyGrabStrategyTests
    {
        private GameObject _grabbableGo;
        private Grabable _grabable;
        private Rigidbody _rigidbody;
        private TestInteractor _interactor;

        [SetUp]
        public void SetUp()
        {
            _grabbableGo = new GameObject("RBGrabbable");
            _grabbableGo.AddComponent<PoseConstrainter>();
            _grabbableGo.AddComponent<VariableTweener>();
            _grabable = _grabbableGo.AddComponent<Grabable>();
            _rigidbody = _grabbableGo.AddComponent<Rigidbody>();

            _interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyGameObject(_grabbableGo);
            TestHelpers.DestroyComponent(_interactor);
        }

        // ── Initialize: Kinematic ──

        [Test]
        public void Initialize_SetsRigidbodyKinematic()
        {
            _rigidbody.isKinematic = false;
            var strategy = new RigidBodyGrabStrategy(_rigidbody);

            strategy.Initialize(_interactor);

            Assert.IsTrue(_rigidbody.isKinematic);
        }

        [Test]
        public void Initialize_AlreadyKinematic_StaysKinematic()
        {
            _rigidbody.isKinematic = true;
            var strategy = new RigidBodyGrabStrategy(_rigidbody);

            strategy.Initialize(_interactor);

            Assert.IsTrue(_rigidbody.isKinematic);
        }

        // ── Initialize: Layer Management (inherited) ──

        [Test]
        public void Initialize_ChangesGameObjectLayerToInteractorLayer()
        {
            _grabbableGo.layer = LayerMask.NameToLayer("Default");
            var strategy = new RigidBodyGrabStrategy(_rigidbody);

            strategy.Initialize(_interactor);

            Assert.AreEqual(_interactor.gameObject.layer, _grabbableGo.layer);
        }

        // ── Grab ──

        [Test]
        public void Grab_ParentsObjectToAttachmentPoint()
        {
            var strategy = new RigidBodyGrabStrategy(_rigidbody);
            strategy.Initialize(_interactor);

            strategy.Grab(_grabable, _interactor);

            Assert.AreEqual(_interactor.AttachmentPoint, _grabable.transform.parent);
        }

        // ── UnGrab ──

        [Test]
        public void UnGrab_SetsRigidbodyNonKinematic()
        {
            _rigidbody.isKinematic = false;
            var strategy = new RigidBodyGrabStrategy(_rigidbody);
            strategy.Initialize(_interactor);
            strategy.Grab(_grabable, _interactor);

            strategy.UnGrab(_grabable, _interactor);

            Assert.IsFalse(_rigidbody.isKinematic);
        }

        [Test]
        public void UnGrab_DetachesFromParent()
        {
            var strategy = new RigidBodyGrabStrategy(_rigidbody);
            strategy.Initialize(_interactor);
            strategy.Grab(_grabable, _interactor);

            strategy.UnGrab(_grabable, _interactor);

            Assert.IsNull(_grabable.transform.parent);
        }

        [Test]
        public void UnGrab_RestoresOriginalGameObjectLayer()
        {
            int originalLayer = LayerMask.NameToLayer("Default");
            _grabbableGo.layer = originalLayer;
            var strategy = new RigidBodyGrabStrategy(_rigidbody);

            strategy.Initialize(_interactor);
            strategy.Grab(_grabable, _interactor);
            strategy.UnGrab(_grabable, _interactor);

            Assert.AreEqual(originalLayer, _grabbableGo.layer);
        }
    }
}

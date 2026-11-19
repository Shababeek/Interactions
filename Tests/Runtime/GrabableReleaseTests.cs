using NUnit.Framework;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    /// <summary>
    /// The grab and release contract for the Rigidbody of a Grabable: kinematic while held, and
    /// dynamic after release even when it was authored kinematic-at-rest.
    /// </summary>
    [TestFixture]
    public class GrabableReleaseTests
    {
        private Grabable _grabable;
        private Rigidbody _body;
        private TestInteractor _hand;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("GrabableTest");
            _body = go.AddComponent<Rigidbody>();
            _body.useGravity = false;
            _grabable = go.AddComponent<Grabable>();
            _hand = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_grabable);
            TestHelpers.DestroyComponent(_hand);
        }

        [Test]
        public void HeldBody_IsKinematicWithoutInterpolation()
        {
            _body.isKinematic = false;
            _body.interpolation = RigidbodyInterpolation.Interpolate;

            Grab();

            Assert.IsTrue(_grabable.IsSelected);
            Assert.IsTrue(_body.isKinematic);
            Assert.AreEqual(RigidbodyInterpolation.None, _body.interpolation);
        }

        [Test]
        public void Release_RestoresInterpolationAndUnparents()
        {
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            Grab();

            _hand.DeSelect();

            Assert.IsFalse(_grabable.IsSelected);
            Assert.IsNull(_grabable.transform.parent);
            Assert.AreEqual(RigidbodyInterpolation.Interpolate, _body.interpolation);
        }

        [Test]
        public void KinematicAtRest_BecomesDynamicOnRelease()
        {
            _body.isKinematic = true;
            Grab();

            _hand.DeSelect();

            Assert.IsFalse(_body.isKinematic, "a released throwable must be dynamic, whatever it was before the grab");
        }

        [Test]
        public void KinematicAtRest_StaysKinematic_WhenItCannotBeThrown()
        {
            _body.isKinematic = true;
            _grabable.CanBeThrown = false;
            Grab();

            _hand.DeSelect();

            Assert.IsTrue(_body.isKinematic);
        }

        [Test]
        public void SetReleaseKinematic_OverridesThePreGrabState_WhenItCannotBeThrown()
        {
            _body.isKinematic = true;
            _grabable.CanBeThrown = false;
            Grab();
            _grabable.SetReleaseKinematic(false);

            _hand.DeSelect();

            Assert.IsFalse(_body.isKinematic);
        }

        private void Grab()
        {
            _hand.Hover(_grabable);
            _hand.Select();
        }
    }
}

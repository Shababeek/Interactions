using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class DrawerInteractableTests
    {
        // ── GetBiggestAxis (private static on LinearInteractableBase) ──

        private static int InvokeGetBiggestAxis(Vector3 direction)
        {
            var method = typeof(LinearInteractableBase).GetMethod("GetBiggestAxis",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (int)method.Invoke(null, new object[] { direction });
        }

        [Test]
        public void GetBiggestAxis_XDominant_Returns0()
        {
            Assert.AreEqual(0, InvokeGetBiggestAxis(new Vector3(10f, 1f, 1f)));
        }

        [Test]
        public void GetBiggestAxis_YDominant_Returns1()
        {
            Assert.AreEqual(1, InvokeGetBiggestAxis(new Vector3(1f, 10f, 1f)));
        }

        [Test]
        public void GetBiggestAxis_ZDominant_Returns2()
        {
            Assert.AreEqual(2, InvokeGetBiggestAxis(new Vector3(1f, 1f, 10f)));
        }

        [Test]
        public void GetBiggestAxis_NegativeXDominant_Returns0()
        {
            Assert.AreEqual(0, InvokeGetBiggestAxis(new Vector3(-10f, 1f, 1f)));
        }

        [Test]
        public void GetBiggestAxis_NegativeYDominant_Returns1()
        {
            Assert.AreEqual(1, InvokeGetBiggestAxis(new Vector3(1f, -10f, 1f)));
        }

        [Test]
        public void GetBiggestAxis_EqualXAndY_XGreaterThanZ_ReturnsX()
        {
            Assert.AreEqual(0, InvokeGetBiggestAxis(new Vector3(5f, 5f, 1f)));
        }

        // ── LocalStart / LocalEnd Properties ──

        [Test]
        public void LocalStart_DefaultIsZero()
        {
            var go = new GameObject("DrawerPropTest");
            var drawer = go.AddComponent<DrawerInteractable>();

            Assert.AreEqual(Vector3.zero, drawer.LocalStart);

            TestHelpers.DestroyGameObject(go);
        }

        [Test]
        public void LocalEnd_DefaultIsForward()
        {
            var go = new GameObject("DrawerPropTest");
            var drawer = go.AddComponent<DrawerInteractable>();

            Assert.AreEqual(Vector3.forward, drawer.LocalEnd);

            TestHelpers.DestroyGameObject(go);
        }

        [Test]
        public void LocalStart_SetAndGet_RoundTrips()
        {
            var go = new GameObject("DrawerPropTest");
            var drawer = go.AddComponent<DrawerInteractable>();

            drawer.LocalStart = new Vector3(1f, 2f, 3f);

            Assert.AreEqual(new Vector3(1f, 2f, 3f), drawer.LocalStart);

            TestHelpers.DestroyGameObject(go);
        }

        [Test]
        public void LocalEnd_SetAndGet_RoundTrips()
        {
            var go = new GameObject("DrawerPropTest");
            var drawer = go.AddComponent<DrawerInteractable>();

            drawer.LocalEnd = new Vector3(4f, 5f, 6f);

            Assert.AreEqual(new Vector3(4f, 5f, 6f), drawer.LocalEnd);

            TestHelpers.DestroyGameObject(go);
        }
    }
}

using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class DrawerInteractableTests
    {
        // ── GetBiggestAxe (private static) ──

        private static int InvokeGetBiggestAxe(Vector3 direction)
        {
            var method = typeof(DrawerInteractable).GetMethod("GetBiggestAxe",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (int)method.Invoke(null, new object[] { direction });
        }

        [Test]
        public void GetBiggestAxe_XDominant_Returns0()
        {
            Assert.AreEqual(0, InvokeGetBiggestAxe(new Vector3(10f, 1f, 1f)));
        }

        [Test]
        public void GetBiggestAxe_YDominant_Returns1()
        {
            Assert.AreEqual(1, InvokeGetBiggestAxe(new Vector3(1f, 10f, 1f)));
        }

        [Test]
        public void GetBiggestAxe_ZDominant_Returns2()
        {
            Assert.AreEqual(2, InvokeGetBiggestAxe(new Vector3(1f, 1f, 10f)));
        }

        [Test]
        public void GetBiggestAxe_NegativeXDominant_Returns0()
        {
            Assert.AreEqual(0, InvokeGetBiggestAxe(new Vector3(-10f, 1f, 1f)));
        }

        [Test]
        public void GetBiggestAxe_NegativeYDominant_Returns1()
        {
            Assert.AreEqual(1, InvokeGetBiggestAxe(new Vector3(1f, -10f, 1f)));
        }

        [Test]
        public void GetBiggestAxe_EqualXAndY_XGreaterThanZ_ReturnsX()
        {
            Assert.AreEqual(0, InvokeGetBiggestAxe(new Vector3(5f, 5f, 1f)));
        }

        // ── FindNormalizedDistanceAlongPath (private static) ──

        private static float InvokeFindNormalizedDistanceAlongPath(
            Vector3 direction, Vector3 projectedPoint, Vector3 position1)
        {
            var method = typeof(DrawerInteractable).GetMethod("FindNormalizedDistanceAlongPath",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (float)method.Invoke(null, new object[] { direction, projectedPoint, position1 });
        }

        [Test]
        public void FindNormalizedDistance_AtStart_ReturnsZero()
        {
            var start = Vector3.zero;
            var direction = Vector3.forward;

            var result = InvokeFindNormalizedDistanceAlongPath(direction, start, start);

            Assert.AreEqual(0f, result, 0.01f);
        }

        [Test]
        public void FindNormalizedDistance_AtEnd_ReturnsOne()
        {
            var start = Vector3.zero;
            var direction = Vector3.forward;

            var result = InvokeFindNormalizedDistanceAlongPath(direction, Vector3.forward, start);

            Assert.AreEqual(1f, result, 0.01f);
        }

        [Test]
        public void FindNormalizedDistance_AtMidpoint_ReturnsHalf()
        {
            var start = Vector3.zero;
            var direction = new Vector3(0, 0, 2f);
            var projectedPoint = new Vector3(0, 0, 1f);

            var result = InvokeFindNormalizedDistanceAlongPath(direction, projectedPoint, start);

            Assert.AreEqual(0.5f, result, 0.01f);
        }

        [Test]
        public void FindNormalizedDistance_BeyondEnd_ClampsToOne()
        {
            var start = Vector3.zero;
            var direction = Vector3.forward;

            var result = InvokeFindNormalizedDistanceAlongPath(direction, new Vector3(0, 0, 5f), start);

            Assert.AreEqual(1f, result, 0.01f);
        }

        [Test]
        public void FindNormalizedDistance_BeforeStart_ClampsToZero()
        {
            var start = Vector3.zero;
            var direction = Vector3.forward;

            var result = InvokeFindNormalizedDistanceAlongPath(direction, new Vector3(0, 0, -5f), start);

            Assert.AreEqual(0f, result, 0.01f);
        }

        // ── GetPositionBetweenTwoPoints (private static) ──

        private static Vector3 InvokeGetPositionBetweenTwoPoints(
            Vector3 point, Vector3 start, Vector3 end)
        {
            var method = typeof(DrawerInteractable).GetMethod("GetPositionBetweenTwoPoints",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (Vector3)method.Invoke(null, new object[] { point, start, end });
        }

        [Test]
        public void GetPositionBetweenTwoPoints_PointAtStart_ReturnsStart()
        {
            var start = Vector3.zero;
            var end = Vector3.forward;

            var result = InvokeGetPositionBetweenTwoPoints(start, start, end);

            Assert.AreEqual(start.x, result.x, 0.01f);
            Assert.AreEqual(start.y, result.y, 0.01f);
            Assert.AreEqual(start.z, result.z, 0.01f);
        }

        [Test]
        public void GetPositionBetweenTwoPoints_PointAtEnd_ReturnsEnd()
        {
            var start = Vector3.zero;
            var end = Vector3.forward;

            var result = InvokeGetPositionBetweenTwoPoints(end, start, end);

            Assert.AreEqual(end.x, result.x, 0.01f);
            Assert.AreEqual(end.y, result.y, 0.01f);
            Assert.AreEqual(end.z, result.z, 0.01f);
        }

        [Test]
        public void GetPositionBetweenTwoPoints_BeyondEnd_ClampsToEnd()
        {
            var start = Vector3.zero;
            var end = Vector3.forward;

            var result = InvokeGetPositionBetweenTwoPoints(new Vector3(0, 0, 10f), start, end);

            Assert.AreEqual(end.x, result.x, 0.01f);
            Assert.AreEqual(end.y, result.y, 0.01f);
            Assert.AreEqual(end.z, result.z, 0.01f);
        }

        [Test]
        public void GetPositionBetweenTwoPoints_BeforeStart_ClampsToStart()
        {
            var start = Vector3.zero;
            var end = Vector3.forward;

            var result = InvokeGetPositionBetweenTwoPoints(new Vector3(0, 0, -10f), start, end);

            Assert.AreEqual(start.x, result.x, 0.01f);
            Assert.AreEqual(start.y, result.y, 0.01f);
            Assert.AreEqual(start.z, result.z, 0.01f);
        }

        [Test]
        public void GetPositionBetweenTwoPoints_OffAxisPoint_ProjectsOntoPath()
        {
            var start = Vector3.zero;
            var end = new Vector3(0, 0, 2f);
            var point = new Vector3(5f, 5f, 1f);

            var result = InvokeGetPositionBetweenTwoPoints(point, start, end);

            Assert.AreEqual(0f, result.x, 0.01f);
            Assert.AreEqual(0f, result.y, 0.01f);
            Assert.AreEqual(1f, result.z, 0.01f);
        }

        [Test]
        public void GetPositionBetweenTwoPoints_XAxisPath_WorksCorrectly()
        {
            var start = new Vector3(-1f, 0, 0);
            var end = new Vector3(1f, 0, 0);

            var result = InvokeGetPositionBetweenTwoPoints(Vector3.zero, start, end);

            Assert.AreEqual(0f, result.x, 0.01f);
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

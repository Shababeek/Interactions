using NUnit.Framework;
using Shababeek.Interactions;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class DistanceGrabMathTests
    {
        private static Vector3 PositionAfter(Vector3 from, Vector3 v0, Vector3 gravity, float t)
        {
            return from + v0 * t + 0.5f * gravity * t * t;
        }

        [Test]
        public void LaunchVelocity_ReachesTargetUnderGravity()
        {
            var from = new Vector3(0f, 1f, 3f);
            var to = new Vector3(0.2f, 1.4f, 0.1f);
            const float flightTime = 0.6f;
            var gravity = Physics.gravity;

            var v0 = DistanceGrabMath.ComputeLaunchVelocity(from, to, flightTime, gravity);
            var landing = PositionAfter(from, v0, gravity, flightTime);

            Assert.Less(Vector3.Distance(landing, to), 1e-4f);
        }

        [Test]
        public void LaunchVelocity_ReachesTargetWithoutGravity()
        {
            var from = Vector3.zero;
            var to = new Vector3(1f, 2f, 3f);
            const float flightTime = 0.5f;

            var v0 = DistanceGrabMath.ComputeLaunchVelocity(from, to, flightTime, Vector3.zero);
            var landing = PositionAfter(from, v0, Vector3.zero, flightTime);

            Assert.Less(Vector3.Distance(landing, to), 1e-4f);
            Assert.AreEqual((to - from).magnitude / flightTime, v0.magnitude, 1e-4f);
        }

        [Test]
        public void LaunchVelocity_ArcsUpwardAgainstGravity()
        {
            // A level shot under gravity needs upward initial velocity to land level.
            var from = new Vector3(0f, 1f, 2f);
            var to = new Vector3(0f, 1f, 0f);

            var v0 = DistanceGrabMath.ComputeLaunchVelocity(from, to, 0.6f, Physics.gravity);

            Assert.Greater(v0.y, 0f);
        }

        [Test]
        public void FlightTime_ScalesWithDistanceAndClamps()
        {
            var range = new Vector2(0.35f, 0.9f);

            Assert.AreEqual(0.35f, DistanceGrabMath.ComputeFlightTime(1f, 0.12f, range), 1e-5f);   // clamped low
            Assert.AreEqual(0.48f, DistanceGrabMath.ComputeFlightTime(4f, 0.12f, range), 1e-5f);   // scaled
            Assert.AreEqual(0.9f, DistanceGrabMath.ComputeFlightTime(20f, 0.12f, range), 1e-5f);   // clamped high
        }

        [Test]
        public void LaunchVelocity_GuardsAgainstZeroFlightTime()
        {
            var v0 = DistanceGrabMath.ComputeLaunchVelocity(Vector3.zero, Vector3.one, 0f, Vector3.zero);

            Assert.IsFalse(float.IsInfinity(v0.x) || float.IsNaN(v0.x));
        }
    }
}

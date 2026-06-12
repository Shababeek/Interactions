using NUnit.Framework;
using Shababeek.Interactions;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class TwoHandSolverTests
    {
        private static Vector3 TransformAnchor(Vector3 position, Quaternion rotation, Vector3 anchorLocal, Vector3 scale)
        {
            return position + rotation * Vector3.Scale(anchorLocal, scale);
        }

        [Test]
        public void Solve_PrimaryAnchorLandsOnPrimaryHand()
        {
            var p1 = new Vector3(0f, 1.2f, 0.3f);
            var p2 = new Vector3(0.1f, 1.3f, 0.8f);
            var anchor1 = new Vector3(0f, 0f, -0.15f);
            var anchor2 = new Vector3(0f, 0.02f, 0.25f);

            var (pos, rot) = TwoHandSolver.Solve(p1, p2, Vector3.up, anchor1, anchor2, Vector3.one);

            var anchorWorld = TransformAnchor(pos, rot, anchor1, Vector3.one);
            Assert.Less(Vector3.Distance(anchorWorld, p1), 1e-4f);
        }

        [Test]
        public void Solve_GripAxisAimsAtSecondaryHand()
        {
            var p1 = new Vector3(0f, 1f, 0f);
            var p2 = new Vector3(0f, 1f, 1f);
            var anchor1 = new Vector3(0f, 0f, -0.1f);
            var anchor2 = new Vector3(0f, 0f, 0.3f);

            var (pos, rot) = TwoHandSolver.Solve(p1, p2, Vector3.up, anchor1, anchor2, Vector3.one);

            // The secondary anchor must sit exactly on the primary→secondary line.
            var secondaryWorld = TransformAnchor(pos, rot, anchor2, Vector3.one);
            var toSecondaryAnchor = (secondaryWorld - p1).normalized;
            var toSecondaryHand = (p2 - p1).normalized;
            Assert.Greater(Vector3.Dot(toSecondaryAnchor, toSecondaryHand), 0.9999f);
        }

        [Test]
        public void Solve_RespectsScale()
        {
            var p1 = Vector3.zero;
            var p2 = new Vector3(0f, 0f, 2f);
            var anchor1 = Vector3.zero;
            var anchor2 = new Vector3(0f, 0f, 0.5f);
            var scale = new Vector3(1f, 1f, 2f);

            var (pos, rot) = TwoHandSolver.Solve(p1, p2, Vector3.up, anchor1, anchor2, scale);

            // Scaled anchor span is 1m along the grip axis.
            var secondaryWorld = TransformAnchor(pos, rot, anchor2, scale);
            Assert.AreEqual(1f, Vector3.Distance(pos, secondaryWorld), 1e-4f);
        }

        [Test]
        public void Solve_DegenerateInput_DoesNotProduceNaN()
        {
            var (pos, rot) = TwoHandSolver.Solve(
                Vector3.one, Vector3.one, Vector3.up, Vector3.zero, Vector3.zero, Vector3.one);

            Assert.IsFalse(float.IsNaN(pos.x) || float.IsNaN(rot.x));
        }

        [Test]
        public void Solve_VerticalGripAxis_UsesFallbackUpHint()
        {
            // Anchors along local Y would make LookRotation's up hint parallel — must not throw
            // or produce NaN.
            var (pos, rot) = TwoHandSolver.Solve(
                new Vector3(0f, 1f, 0f), new Vector3(0f, 2f, 0f), Vector3.forward,
                Vector3.zero, new Vector3(0f, 0.4f, 0f), Vector3.one);

            Assert.IsFalse(float.IsNaN(rot.x) || float.IsNaN(rot.y) || float.IsNaN(rot.z) || float.IsNaN(rot.w));
            var anchorWorld = pos + rot * new Vector3(0f, 0.4f, 0f);
            var direction = (anchorWorld - new Vector3(0f, 1f, 0f)).normalized;
            Assert.Greater(Vector3.Dot(direction, Vector3.up), 0.9999f);
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using Shababeek.Interactions.Animations;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class PoseFitSolverTests
    {
        private GameObject _obstacle;
        private List<Collider> _colliders;

        [SetUp]
        public void SetUp()
        {
            // 1m cube centered at origin; its +Y face sits at y = 0.5.
            _obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _obstacle.transform.position = Vector3.zero;
            Physics.SyncTransforms();
            _colliders = new List<Collider> { _obstacle.GetComponent<Collider>() };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_obstacle);
        }

        private static FingerArcs MakeStraightArc(Vector3 from, Vector3 to, int samples, float radius)
        {
            var fingerSamples = new FingerArcs.Sample[5][];
            var radii = new float[5];
            for (int f = 0; f < 5; f++)
            {
                fingerSamples[f] = new FingerArcs.Sample[samples];
                radii[f] = radius;
                for (int k = 0; k < samples; k++)
                {
                    var p = Vector3.Lerp(from, to, (float)k / (samples - 1));
                    fingerSamples[f][k] = new FingerArcs.Sample { tip = p, mid = p };
                }
            }
            return new FingerArcs(fingerSamples, radii, samples);
        }

        [Test]
        public void FitFinger_ArcIntoSurface_ReportsContactCurl()
        {
            // Arc descends from y=1.5 to y=0.5 above the cube face at y=0.5;
            // contact (ignoring radius) happens at the very end of the sweep.
            var arcs = MakeStraightArc(new Vector3(0, 1.5f, 0), new Vector3(0, 0.4f, 0), 12, 0f);

            var result = PoseFitSolver.FitFinger(arcs, 0, Matrix4x4.identity, _colliders);

            Assert.IsTrue(result.hit);
            // Surface at y=0.5 → 1.0 of 1.1 total travel → curl ≈ 0.909
            Assert.AreEqual(1.0f / 1.1f, result.curl, 0.02f);
            Assert.AreEqual(0.5f, result.point.y, 0.01f);
            Assert.AreEqual(Vector3.up.y, result.normal.y, 0.01f);
        }

        [Test]
        public void FitFinger_ArcMissesSurface_ReportsNoHit()
        {
            var arcs = MakeStraightArc(new Vector3(5, 2f, 0), new Vector3(5, 1f, 0), 12, 0f);

            var result = PoseFitSolver.FitFinger(arcs, 0, Matrix4x4.identity, _colliders);

            Assert.IsFalse(result.hit);
            Assert.AreEqual(1f, result.curl);
        }

        [Test]
        public void FitFinger_RadiusBacksOffContact()
        {
            var thin = PoseFitSolver.FitFinger(
                MakeStraightArc(new Vector3(0, 1.5f, 0), new Vector3(0, 0.4f, 0), 12, 0f),
                0, Matrix4x4.identity, _colliders);
            var thick = PoseFitSolver.FitFinger(
                MakeStraightArc(new Vector3(0, 1.5f, 0), new Vector3(0, 0.4f, 0), 12, 0.05f),
                0, Matrix4x4.identity, _colliders);

            Assert.IsTrue(thin.hit);
            Assert.IsTrue(thick.hit);
            Assert.Less(thick.curl, thin.curl, "A thicker finger should stop earlier.");
        }

        [Test]
        public void FitFinger_HandLocalToWorld_TransformsArc()
        {
            // Arc authored in hand-local space; hand placed 5 units away on X so the
            // world-space arc misses the cube entirely.
            var arcs = MakeStraightArc(new Vector3(0, 1.5f, 0), new Vector3(0, 0.4f, 0), 12, 0f);
            var awayFromCube = Matrix4x4.Translate(new Vector3(5, 0, 0));

            var result = PoseFitSolver.FitFinger(arcs, 0, awayFromCube, _colliders);

            Assert.IsFalse(result.hit);
        }

        [Test]
        public void Fit_ReturnsResultForAllFiveFingers()
        {
            var arcs = MakeStraightArc(new Vector3(0, 1.5f, 0), new Vector3(0, 0.4f, 0), 12, 0f);

            var results = PoseFitSolver.Fit(arcs, Matrix4x4.identity, _colliders);

            Assert.AreEqual(5, results.Length);
            foreach (var r in results) Assert.IsTrue(r.hit);
        }
    }
}

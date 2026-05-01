using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class ThrowableTests
    {
        private GameObject _go;
        private Rigidbody _body;
        private Throwable _throwable;

        private static T GetField<T>(object instance, string fieldName)
        {
            var field = typeof(Throwable).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(instance);
        }

        private static T GetStaticField<T>(string fieldName)
        {
            var field = typeof(Throwable).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Static);
            return (T)field.GetValue(null);
        }

        private static Vector3 InvokeAngularVelocityFromDelta(Quaternion delta, float dt)
        {
            var method = typeof(Throwable).GetMethod("AngularVelocityFromDelta",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (Vector3)method.Invoke(null, new object[] { delta, dt });
        }

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("ThrowableHost");
            _body = _go.AddComponent<Rigidbody>();
            _throwable = new Throwable();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        // ── Defaults ──

        [Test]
        public void DefaultVelocitySampleCount_IsTen()
        {
            Assert.AreEqual(10, GetField<int>(_throwable, "velocitySampleCount"));
        }

        [Test]
        public void DefaultThrowMultiplier_IsOne()
        {
            Assert.AreEqual(1f, GetField<float>(_throwable, "throwMultiplier"), 0.001f);
        }

        [Test]
        public void DefaultEnableAngularVelocity_IsTrue()
        {
            Assert.IsTrue(GetField<bool>(_throwable, "enableAngularVelocity"));
        }

        // ── StartTracking ──

        [Test]
        public void StartTracking_InitializesSampleBuffer()
        {
            _throwable.StartTracking(_body, _go.transform);

            var samples = GetField<Vector3[]>(_throwable, "_velocitySamples");
            Assert.IsNotNull(samples);
            Assert.AreEqual(10, samples.Length);
        }

        [Test]
        public void StartTracking_ResetsSampleCount()
        {
            _throwable.StartTracking(_body, _go.transform);
            // Sample once to advance the count, then re-start; count should reset to 0.
            _throwable.Sample();
            _throwable.StartTracking(_body, _go.transform);

            Assert.AreEqual(0, GetField<int>(_throwable, "_count"));
        }

        [Test]
        public void StartTracking_ClearsPriorSamples()
        {
            _throwable.StartTracking(_body, _go.transform);
            var samples = GetField<Vector3[]>(_throwable, "_velocitySamples");
            for (int i = 0; i < samples.Length; i++) samples[i] = Vector3.one;

            _throwable.StartTracking(_body, _go.transform);

            samples = GetField<Vector3[]>(_throwable, "_velocitySamples");
            for (int i = 0; i < samples.Length; i++)
            {
                Assert.AreEqual(Vector3.zero, samples[i], $"sample {i} should be zero");
            }
        }

        // ── ApplyThrow ──

        [Test]
        public void ApplyThrow_WhenNotTracking_ReturnsZero()
        {
            Vector3 result = _throwable.ApplyThrow();
            Assert.AreEqual(Vector3.zero, result);
        }

        [Test]
        public void ApplyThrow_WhenKinematicBody_DoesNotSetVelocity()
        {
            _body.isKinematic = true;
            _throwable.StartTracking(_body, _go.transform);
            // No Sample calls — count is 0, but we still want to confirm the kinematic guard works.
            // Move the transform so a sample would have non-zero velocity, then sample once.
            _go.transform.position = new Vector3(1f, 0f, 0f);
            _throwable.Sample();

            _throwable.ApplyThrow();

            // linearVelocity setter is a no-op on kinematic bodies; just confirm we didn't crash
            // and that the velocity stayed zero.
            Assert.AreEqual(Vector3.zero, _body.linearVelocity);
        }

        [Test]
        public void CancelTracking_StopsApplyFromFiring()
        {
            _throwable.StartTracking(_body, _go.transform);
            _throwable.CancelTracking();

            Vector3 result = _throwable.ApplyThrow();
            Assert.AreEqual(Vector3.zero, result);
        }

        // ── AngularVelocityFromDelta ──

        [Test]
        public void AngularVelocityFromDelta_Identity_ReturnsZero()
        {
            Vector3 result = InvokeAngularVelocityFromDelta(Quaternion.identity, 0.02f);
            Assert.LessOrEqual(result.magnitude, 0.0001f);
        }

        [Test]
        public void AngularVelocityFromDelta_90Degrees_ReturnsNonZero()
        {
            Vector3 result = InvokeAngularVelocityFromDelta(Quaternion.AngleAxis(90f, Vector3.up), 0.02f);
            Assert.Greater(result.magnitude, 0f);
        }

        [Test]
        public void AngularVelocityFromDelta_DivideByDeltaTime_ScalesProperly()
        {
            // A 1-radian rotation over 1s = 1 rad/s. Over 0.5s = 2 rad/s.
            var delta = Quaternion.AngleAxis(Mathf.Rad2Deg, Vector3.up);
            Vector3 a = InvokeAngularVelocityFromDelta(delta, 1f);
            Vector3 b = InvokeAngularVelocityFromDelta(delta, 0.5f);

            // The magnitude of b should be roughly 2× the magnitude of a.
            Assert.AreEqual(a.magnitude * 2f, b.magnitude, 0.01f);
        }

        // ── OnThrowEnd event surface ──

        [Test]
        public void OnThrowEnd_IsNonNull()
        {
            Assert.IsNotNull(_throwable.OnThrowEnd);
        }
    }
}

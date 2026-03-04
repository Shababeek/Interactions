using System.Reflection;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class ThrowableTests
    {
        private Throwable _throwable;
        private Rigidbody _rigidbody;

        private void SetSerializedField(string fieldName, object value)
        {
            var field = typeof(Throwable).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_throwable, value);
        }

        private T GetSerializedField<T>(string fieldName)
        {
            var field = typeof(Throwable).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(_throwable);
        }

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("ThrowableObject");
            go.AddComponent<BoxCollider>();
            _rigidbody = go.AddComponent<Rigidbody>();
            go.AddComponent<Grabable>();
            _throwable = go.AddComponent<Throwable>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_throwable);
        }

        // ── OnValidate Clamping ──

        [Test]
        public void OnValidate_VelocitySampleCount_ClampsToMinimumOne()
        {
            SetSerializedField("velocitySampleCount", -5);

            // Invoke OnValidate via reflection
            var onValidate = typeof(Throwable).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidate?.Invoke(_throwable, null);

            var count = GetSerializedField<int>("velocitySampleCount");
            Assert.GreaterOrEqual(count, 1);
        }

        [Test]
        public void OnValidate_ThrowMultiplier_ClampsToMinimum01()
        {
            SetSerializedField("throwMultiplier", -2f);

            var onValidate = typeof(Throwable).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidate?.Invoke(_throwable, null);

            var multiplier = GetSerializedField<float>("throwMultiplier");
            Assert.GreaterOrEqual(multiplier, 0.1f);
        }

        [Test]
        public void OnValidate_AngularVelocityMultiplier_ClampsToMinimumZero()
        {
            SetSerializedField("angularVelocityMultiplier", -1f);

            var onValidate = typeof(Throwable).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidate?.Invoke(_throwable, null);

            var multiplier = GetSerializedField<float>("angularVelocityMultiplier");
            Assert.GreaterOrEqual(multiplier, 0f);
        }

        [Test]
        public void OnValidate_ValidValues_RemainUnchanged()
        {
            SetSerializedField("velocitySampleCount", 15);
            SetSerializedField("throwMultiplier", 2f);
            SetSerializedField("angularVelocityMultiplier", 1.5f);

            var onValidate = typeof(Throwable).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidate?.Invoke(_throwable, null);

            Assert.AreEqual(15, GetSerializedField<int>("velocitySampleCount"));
            Assert.AreEqual(2f, GetSerializedField<float>("throwMultiplier"), 0.01f);
            Assert.AreEqual(1.5f, GetSerializedField<float>("angularVelocityMultiplier"), 0.01f);
        }

        // ── Velocity Sample Array Initialization ──

        [Test]
        public void Awake_InitializesVelocitySampleArray()
        {
            var samples = GetSerializedField<Vector3[]>("_velocitySamples");

            Assert.IsNotNull(samples);
            Assert.AreEqual(10, samples.Length); // default velocitySampleCount is 10
        }

        [Test]
        public void Awake_InitializesAngularVelocitySampleArray()
        {
            var samples = GetSerializedField<Vector3[]>("_angularVelocitySamples");

            Assert.IsNotNull(samples);
            Assert.AreEqual(10, samples.Length);
        }

        // ── StartThrowing (private, via reflection) ──

        [Test]
        public void StartThrowing_SetsRigidbodyKinematic()
        {
            var startMethod = typeof(Throwable).GetMethod("StartThrowing",
                BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod?.Invoke(_throwable, null);

            Assert.IsTrue(_rigidbody.isKinematic);
        }

        [Test]
        public void StartThrowing_ResetsVelocitySamples()
        {
            // Fill with non-zero data first
            var samples = GetSerializedField<Vector3[]>("_velocitySamples");
            for (int i = 0; i < samples.Length; i++)
                samples[i] = Vector3.one;

            var startMethod = typeof(Throwable).GetMethod("StartThrowing",
                BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod?.Invoke(_throwable, null);

            samples = GetSerializedField<Vector3[]>("_velocitySamples");
            for (int i = 0; i < samples.Length; i++)
            {
                Assert.AreEqual(Vector3.zero, samples[i],
                    $"Sample {i} should be zero after StartThrowing");
            }
        }

        [Test]
        public void StartThrowing_ResetsSampleIndex()
        {
            var startMethod = typeof(Throwable).GetMethod("StartThrowing",
                BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod?.Invoke(_throwable, null);

            var sampleIndex = GetSerializedField<int>("_currentSampleIndex");
            Assert.AreEqual(0, sampleIndex);
        }

        [Test]
        public void StartThrowing_ResetsSampleCount()
        {
            var startMethod = typeof(Throwable).GetMethod("StartThrowing",
                BindingFlags.NonPublic | BindingFlags.Instance);
            startMethod?.Invoke(_throwable, null);

            var sampleCount = GetSerializedField<int>("_sampleCount");
            Assert.AreEqual(0, sampleCount);
        }

        // ── GetAngularVelocityFromDeltaRotation (private) ──

        [Test]
        public void GetAngularVelocity_IdentityDelta_ReturnsZero()
        {
            var method = typeof(Throwable).GetMethod("GetAngularVelocityFromDeltaRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = (Vector3)method.Invoke(_throwable,
                new object[] { Quaternion.identity, 0.02f });

            Assert.AreEqual(0f, result.magnitude, 0.1f);
        }

        [Test]
        public void GetAngularVelocity_90DegreeRotation_ReturnsNonZero()
        {
            var method = typeof(Throwable).GetMethod("GetAngularVelocityFromDeltaRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var deltaRot = Quaternion.AngleAxis(90f, Vector3.up);
            var result = (Vector3)method.Invoke(_throwable, new object[] { deltaRot, 0.02f });

            Assert.Greater(result.magnitude, 0f);
        }

        // ── Observable ──

        [Test]
        public void OnThrowEnd_ReturnsNonNullObservable()
        {
            Assert.IsNotNull(_throwable.OnThrowEnd);
        }

        [Test]
        public void OnThrowEnd_CanSubscribeWithoutError()
        {
            bool called = false;
            var disposable = _throwable.OnThrowEnd.Do(_ => called = true).Subscribe();

            // Not thrown yet, so should not fire
            Assert.IsFalse(called);
            disposable.Dispose();
        }
    }
}

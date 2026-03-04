using System.Reflection;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class WheelInteractableTests
    {
        private WheelInteractable _wheel;

        private void SetSerializedField(string fieldName, object value)
        {
            var field = typeof(WheelInteractable).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_wheel, value);
        }

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("WheelTest");
            _wheel = go.AddComponent<WheelInteractable>();

            var origRotField = typeof(WheelInteractable).GetField("_originalRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);
            origRotField?.SetValue(_wheel, Quaternion.identity);

            SetSerializedField("maxRotations", 1f);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_wheel);
        }

        // ── MaxRotations ──

        [Test]
        public void MaxRotations_Default_IsOne()
        {
            Assert.AreEqual(1f, _wheel.MaxRotations, 0.01f);
        }

        // ── SetAngle ──

        [Test]
        public void SetAngle_WithinRange_SetsCurrentAngle()
        {
            _wheel.SetAngle(90f);

            Assert.AreEqual(90f, _wheel.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_AboveMax_ClampsToMaxAngle()
        {
            _wheel.SetAngle(500f);

            Assert.AreEqual(360f, _wheel.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_BelowMin_ClampsToMinAngle()
        {
            _wheel.SetAngle(-500f);

            Assert.AreEqual(-360f, _wheel.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_Negative_SetsNegativeAngle()
        {
            _wheel.SetAngle(-180f);

            Assert.AreEqual(-180f, _wheel.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_Zero_SetsZero()
        {
            _wheel.SetAngle(45f);
            _wheel.SetAngle(0f);

            Assert.AreEqual(0f, _wheel.CurrentAngle, 0.01f);
        }

        // ── SetAngle with different MaxRotations ──

        [Test]
        public void SetAngle_HalfRotation_ClampsAt180()
        {
            SetSerializedField("maxRotations", 0.5f);
            _wheel.SetAngle(300f);

            Assert.AreEqual(180f, _wheel.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_TwoRotations_AllowsUp720()
        {
            SetSerializedField("maxRotations", 2f);
            _wheel.SetAngle(600f);

            Assert.AreEqual(600f, _wheel.CurrentAngle, 0.01f);
        }

        // ── NormalizedValue ──

        [Test]
        public void NormalizedValue_AtZero_IsZero()
        {
            _wheel.SetAngle(0f);

            Assert.AreEqual(0f, _wheel.NormalizedValue, 0.01f);
        }

        [Test]
        public void NormalizedValue_AtMaxAngle_IsOne()
        {
            _wheel.SetAngle(360f);

            Assert.AreEqual(1f, _wheel.NormalizedValue, 0.01f);
        }

        [Test]
        public void NormalizedValue_AtHalfMax_IsHalf()
        {
            _wheel.SetAngle(180f);

            Assert.AreEqual(0.5f, _wheel.NormalizedValue, 0.01f);
        }

        [Test]
        public void NormalizedValue_Negative_IsNegative()
        {
            _wheel.SetAngle(-180f);

            Assert.AreEqual(-0.5f, _wheel.NormalizedValue, 0.01f);
        }

        // ── SetNormalized ──

        [Test]
        public void SetNormalized_Half_Sets180Degrees()
        {
            _wheel.SetNormalized(0.5f);

            Assert.AreEqual(180f, _wheel.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetNormalized_One_SetsMaxAngle()
        {
            _wheel.SetNormalized(1f);

            Assert.AreEqual(360f, _wheel.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetNormalized_Zero_SetsZero()
        {
            _wheel.SetNormalized(0f);

            Assert.AreEqual(0f, _wheel.CurrentAngle, 0.01f);
        }

        // ── ResetWheel ──

        [Test]
        public void ResetWheel_ResetsAngleToZero()
        {
            _wheel.SetAngle(200f);
            _wheel.ResetWheel();

            Assert.AreEqual(0f, _wheel.CurrentAngle, 0.01f);
        }

        [Test]
        public void ResetWheel_ResetsNormalizedValueToZero()
        {
            _wheel.SetAngle(200f);
            _wheel.ResetWheel();

            Assert.AreEqual(0f, _wheel.NormalizedValue, 0.01f);
        }

        // ── Event Firing ──

        [Test]
        public void SetAngle_FiresOnAngleChanged()
        {
            float receivedAngle = -1f;
            var disposable = _wheel.OnAngleChanged.Do(angle => receivedAngle = angle).Subscribe();

            _wheel.SetAngle(45f);

            Assert.AreEqual(45f, receivedAngle, 0.01f);
            disposable.Dispose();
        }

        [Test]
        public void SetAngle_FiresOnNormalizedChanged()
        {
            float receivedNormalized = -1f;
            var disposable = _wheel.OnNormalizedChanged.Do(val => receivedNormalized = val).Subscribe();

            _wheel.SetAngle(180f);

            Assert.AreEqual(0.5f, receivedNormalized, 0.01f);
            disposable.Dispose();
        }

        [Test]
        public void ResetWheel_FiresAngleChangedWithZero()
        {
            float lastAngle = -999f;
            var disposable = _wheel.OnAngleChanged.Do(angle => lastAngle = angle).Subscribe();

            _wheel.SetAngle(100f);
            _wheel.ResetWheel();

            Assert.AreEqual(0f, lastAngle, 0.01f);
            disposable.Dispose();
        }

        // ── Properties ──

        [Test]
        public void GrabMode_DefaultIsObjectFollowsHand()
        {
            Assert.AreEqual(WheelGrabMode.ObjectFollowsHand, _wheel.GrabMode);
        }

        [Test]
        public void RotationAxis_DefaultIsForward()
        {
            Assert.AreEqual(RotationAxis.Forward, _wheel.RotationAxis);
        }

        // ── SetAngle then ResetWheel consistency ──

        [Test]
        public void SetAngle_ThenReset_NormalizedIsConsistentWithAngle()
        {
            _wheel.SetAngle(270f);

            float expectedNormalized = 270f / 360f;
            Assert.AreEqual(expectedNormalized, _wheel.NormalizedValue, 0.01f);

            _wheel.ResetWheel();
            Assert.AreEqual(0f, _wheel.NormalizedValue, 0.01f);
            Assert.AreEqual(0f, _wheel.CurrentAngle, 0.01f);
        }
    }
}

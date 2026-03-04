using System.Reflection;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class LeverInteractableTests
    {
        private LeverInteractable _lever;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("LeverTest");
            _lever = go.AddComponent<LeverInteractable>();

            // Initialize _originalRotation so SetAngle/ApplyRotationToTransform work
            var origRotField = typeof(LeverInteractable).GetField("_originalRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);
            origRotField?.SetValue(_lever, Quaternion.identity);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_lever);
        }

        // ── AngleRange Property Clamping ──

        [Test]
        public void AngleRange_DefaultRange_IsNegative40ToPositive40()
        {
            Assert.AreEqual(-40f, _lever.AngleRange.x);
            Assert.AreEqual(40f, _lever.AngleRange.y);
        }

        [Test]
        public void AngleRange_SetValid_AppliesDirectly()
        {
            _lever.AngleRange = new Vector2(-90f, 90f);

            Assert.AreEqual(-90f, _lever.AngleRange.x);
            Assert.AreEqual(90f, _lever.AngleRange.y);
        }

        [Test]
        public void AngleRange_MinClampsToNegative180()
        {
            _lever.AngleRange = new Vector2(-300f, 90f);

            Assert.GreaterOrEqual(_lever.AngleRange.x, -180f);
        }

        [Test]
        public void AngleRange_MaxClampsToPositive180()
        {
            _lever.AngleRange = new Vector2(-90f, 300f);

            Assert.LessOrEqual(_lever.AngleRange.y, 180f);
        }

        [Test]
        public void AngleRange_MinCannotExceedMax()
        {
            _lever.AngleRange = new Vector2(50f, 30f);

            Assert.Less(_lever.AngleRange.x, _lever.AngleRange.y);
        }

        [Test]
        public void AngleRange_MaxAlwaysAtLeastOneMoreThanMin()
        {
            _lever.AngleRange = new Vector2(45f, 45f);

            Assert.GreaterOrEqual(_lever.AngleRange.y, _lever.AngleRange.x + 1f);
        }

        // ── SetAngle ──

        [Test]
        public void SetAngle_WithinRange_SetsCurrentAngle()
        {
            _lever.AngleRange = new Vector2(-90f, 90f);
            _lever.SetAngle(30f);

            Assert.AreEqual(30f, _lever.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_AboveMax_ClampsToMax()
        {
            _lever.AngleRange = new Vector2(-40f, 40f);
            _lever.SetAngle(100f);

            Assert.AreEqual(40f, _lever.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_BelowMin_ClampsToMin()
        {
            _lever.AngleRange = new Vector2(-40f, 40f);
            _lever.SetAngle(-100f);

            Assert.AreEqual(-40f, _lever.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_AtZero_SetsToZero()
        {
            _lever.AngleRange = new Vector2(-90f, 90f);
            _lever.SetAngle(0f);

            Assert.AreEqual(0f, _lever.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_AtExactMin_SetsToMin()
        {
            _lever.AngleRange = new Vector2(-60f, 60f);
            _lever.SetAngle(-60f);

            Assert.AreEqual(-60f, _lever.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetAngle_AtExactMax_SetsToMax()
        {
            _lever.AngleRange = new Vector2(-60f, 60f);
            _lever.SetAngle(60f);

            Assert.AreEqual(60f, _lever.CurrentAngle, 0.01f);
        }

        // ── SetNormalizedAngle ──

        [Test]
        public void SetNormalizedAngle_Zero_SetsToMinAngle()
        {
            _lever.AngleRange = new Vector2(-45f, 45f);
            _lever.SetNormalizedAngle(0f);

            Assert.AreEqual(-45f, _lever.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetNormalizedAngle_One_SetsToMaxAngle()
        {
            _lever.AngleRange = new Vector2(-45f, 45f);
            _lever.SetNormalizedAngle(1f);

            Assert.AreEqual(45f, _lever.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetNormalizedAngle_Half_SetsToCenterAngle()
        {
            _lever.AngleRange = new Vector2(-60f, 60f);
            _lever.SetNormalizedAngle(0.5f);

            Assert.AreEqual(0f, _lever.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetNormalizedAngle_QuarterPoint_InterpolatesCorrectly()
        {
            _lever.AngleRange = new Vector2(0f, 100f);
            _lever.SetNormalizedAngle(0.25f);

            Assert.AreEqual(25f, _lever.CurrentAngle, 0.01f);
        }

        // ── CurrentNormalizedAngle ──

        [Test]
        public void CurrentNormalizedAngle_AtMin_IsZero()
        {
            _lever.AngleRange = new Vector2(-45f, 45f);
            _lever.SetAngle(-45f);

            Assert.AreEqual(0f, _lever.CurrentNormalizedAngle, 0.01f);
        }

        [Test]
        public void CurrentNormalizedAngle_AtMax_IsOne()
        {
            _lever.AngleRange = new Vector2(-45f, 45f);
            _lever.SetAngle(45f);

            Assert.AreEqual(1f, _lever.CurrentNormalizedAngle, 0.01f);
        }

        [Test]
        public void CurrentNormalizedAngle_AtCenter_IsHalf()
        {
            _lever.AngleRange = new Vector2(-50f, 50f);
            _lever.SetAngle(0f);

            Assert.AreEqual(0.5f, _lever.CurrentNormalizedAngle, 0.01f);
        }

        // ── NormalizeAngle (private, via reflection) ──

        [Test]
        public void NormalizeAngle_PositiveOver180_WrapsToNegative()
        {
            var method = typeof(LeverInteractable).GetMethod("NormalizeAngle",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = (float)method.Invoke(_lever, new object[] { 270f });

            Assert.AreEqual(-90f, result, 0.01f);
        }

        [Test]
        public void NormalizeAngle_NegativeUnder180_WrapsToPositive()
        {
            var method = typeof(LeverInteractable).GetMethod("NormalizeAngle",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = (float)method.Invoke(_lever, new object[] { -270f });

            Assert.AreEqual(90f, result, 0.01f);
        }

        [Test]
        public void NormalizeAngle_Within180_Unchanged()
        {
            var method = typeof(LeverInteractable).GetMethod("NormalizeAngle",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = (float)method.Invoke(_lever, new object[] { 45f });

            Assert.AreEqual(45f, result, 0.01f);
        }

        [Test]
        public void NormalizeAngle_Exactly360_WrapsToZero()
        {
            var method = typeof(LeverInteractable).GetMethod("NormalizeAngle",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = (float)method.Invoke(_lever, new object[] { 360f });

            Assert.AreEqual(0f, result, 0.01f);
        }

        // ── Event Firing ──

        [Test]
        public void SetAngle_FiresOnLeverChanged()
        {
            bool eventFired = false;
            var disposable = _lever.OnLeverChanged.Do(_ => eventFired = true).Subscribe();

            _lever.AngleRange = new Vector2(-90f, 90f);
            _lever.SetAngle(45f);

            Assert.IsTrue(eventFired);
            disposable.Dispose();
        }

        [Test]
        public void SetNormalizedAngle_FiresOnLeverChanged()
        {
            bool eventFired = false;
            var disposable = _lever.OnLeverChanged.Do(_ => eventFired = true).Subscribe();

            _lever.AngleRange = new Vector2(-90f, 90f);
            _lever.SetNormalizedAngle(0.75f);

            Assert.IsTrue(eventFired);
            disposable.Dispose();
        }

        // ── Axis Configuration ──

        [Test]
        public void GetRotationAxis_ReturnsNonZeroVectors()
        {
            var (plane, normal) = _lever.GetRotationAxis();

            Assert.AreNotEqual(Vector3.zero, plane);
            Assert.AreNotEqual(Vector3.zero, normal);
        }
    }
}

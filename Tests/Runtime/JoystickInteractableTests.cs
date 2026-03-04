using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class JoystickInteractableTests
    {
        private JoystickInteractable _joystick;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("JoystickTest");
            _joystick = go.AddComponent<JoystickInteractable>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_joystick);
        }

        // ── XRotationRange Property Clamping ──

        [Test]
        public void XRotationRange_DefaultRange_IsNeg45ToPos45()
        {
            Assert.AreEqual(-45f, _joystick.XRotationRange.x, 0.01f);
            Assert.AreEqual(45f, _joystick.XRotationRange.y, 0.01f);
        }

        [Test]
        public void XRotationRange_SetValid_AppliesDirectly()
        {
            _joystick.XRotationRange = new Vector2(-30f, 30f);

            Assert.AreEqual(-30f, _joystick.XRotationRange.x, 0.01f);
            Assert.AreEqual(30f, _joystick.XRotationRange.y, 0.01f);
        }

        [Test]
        public void XRotationRange_MinCannotExceedMaxMinus1()
        {
            _joystick.XRotationRange = new Vector2(50f, 30f);

            Assert.Less(_joystick.XRotationRange.x, _joystick.XRotationRange.y);
        }

        [Test]
        public void XRotationRange_ClampsToMax85Degrees()
        {
            _joystick.XRotationRange = new Vector2(-100f, 100f);

            Assert.GreaterOrEqual(_joystick.XRotationRange.x, -85f);
            Assert.LessOrEqual(_joystick.XRotationRange.y, 85f);
        }

        // ── ZRotationRange Property Clamping ──

        [Test]
        public void ZRotationRange_DefaultRange_IsNeg45ToPos45()
        {
            Assert.AreEqual(-45f, _joystick.ZRotationRange.x, 0.01f);
            Assert.AreEqual(45f, _joystick.ZRotationRange.y, 0.01f);
        }

        [Test]
        public void ZRotationRange_SetValid_AppliesDirectly()
        {
            _joystick.ZRotationRange = new Vector2(-20f, 20f);

            Assert.AreEqual(-20f, _joystick.ZRotationRange.x, 0.01f);
            Assert.AreEqual(20f, _joystick.ZRotationRange.y, 0.01f);
        }

        [Test]
        public void ZRotationRange_MinCannotExceedMaxMinus1()
        {
            _joystick.ZRotationRange = new Vector2(40f, 10f);

            Assert.Less(_joystick.ZRotationRange.x, _joystick.ZRotationRange.y);
        }

        [Test]
        public void ZRotationRange_ClampsToMax85Degrees()
        {
            _joystick.ZRotationRange = new Vector2(-100f, 100f);

            Assert.GreaterOrEqual(_joystick.ZRotationRange.x, -85f);
            Assert.LessOrEqual(_joystick.ZRotationRange.y, 85f);
        }

        // ── ProjectionPlaneHeight ──

        [Test]
        public void ProjectionPlaneHeight_Default_Is03()
        {
            Assert.AreEqual(0.3f, _joystick.ProjectionPlaneHeight, 0.01f);
        }

        [Test]
        public void ProjectionPlaneHeight_SetValid_Applies()
        {
            _joystick.ProjectionPlaneHeight = 0.5f;

            Assert.AreEqual(0.5f, _joystick.ProjectionPlaneHeight, 0.01f);
        }

        [Test]
        public void ProjectionPlaneHeight_SetNegative_ClampsToMinimum()
        {
            _joystick.ProjectionPlaneHeight = -1f;

            Assert.GreaterOrEqual(_joystick.ProjectionPlaneHeight, 0.01f);
        }

        [Test]
        public void ProjectionPlaneHeight_SetZero_ClampsToMinimum()
        {
            _joystick.ProjectionPlaneHeight = 0f;

            Assert.GreaterOrEqual(_joystick.ProjectionPlaneHeight, 0.01f);
        }

        // ── ProjectionMethod ──

        [Test]
        public void ProjectionMethod_DefaultIsDirectionProjection()
        {
            Assert.AreEqual(JoystickProjectionMethod.DirectionProjection, _joystick.ProjectionMethod);
        }

        [Test]
        public void ProjectionMethod_CanBeChanged()
        {
            _joystick.ProjectionMethod = JoystickProjectionMethod.PlaneIntersection;

            Assert.AreEqual(JoystickProjectionMethod.PlaneIntersection, _joystick.ProjectionMethod);
        }

        // ── ClampAngles (private, via reflection) ──

        [Test]
        public void ClampAngles_WithinRange_Unchanged()
        {
            var method = typeof(JoystickInteractable).GetMethod("ClampAngles",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var input = new Vector2(10f, -10f);
            var result = (Vector2)method.Invoke(_joystick, new object[] { input });

            Assert.AreEqual(10f, result.x, 0.01f);
            Assert.AreEqual(-10f, result.y, 0.01f);
        }

        [Test]
        public void ClampAngles_ExceedsXMax_ClampsToXMax()
        {
            _joystick.XRotationRange = new Vector2(-45f, 45f);
            var method = typeof(JoystickInteractable).GetMethod("ClampAngles",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var input = new Vector2(90f, 0f);
            var result = (Vector2)method.Invoke(_joystick, new object[] { input });

            Assert.AreEqual(45f, result.x, 0.01f);
        }

        [Test]
        public void ClampAngles_ExceedsZMax_ClampsToZMax()
        {
            _joystick.ZRotationRange = new Vector2(-30f, 30f);
            var method = typeof(JoystickInteractable).GetMethod("ClampAngles",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var input = new Vector2(0f, 60f);
            var result = (Vector2)method.Invoke(_joystick, new object[] { input });

            Assert.AreEqual(30f, result.y, 0.01f);
        }

        [Test]
        public void ClampAngles_BelowXMin_ClampsToXMin()
        {
            _joystick.XRotationRange = new Vector2(-45f, 45f);
            var method = typeof(JoystickInteractable).GetMethod("ClampAngles",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var input = new Vector2(-90f, 0f);
            var result = (Vector2)method.Invoke(_joystick, new object[] { input });

            Assert.AreEqual(-45f, result.x, 0.01f);
        }

        // ── NormalizeAngle (private, via reflection) ──

        [Test]
        public void NormalizeAngle_PositiveOver180_WrapsNegative()
        {
            var method = typeof(JoystickInteractable).GetMethod("NormalizeAngle",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = (float)method.Invoke(_joystick, new object[] { 270f });

            Assert.AreEqual(-90f, result, 0.01f);
        }

        [Test]
        public void NormalizeAngle_NegativeUnder180_WrapsPositive()
        {
            var method = typeof(JoystickInteractable).GetMethod("NormalizeAngle",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = (float)method.Invoke(_joystick, new object[] { -270f });

            Assert.AreEqual(90f, result, 0.01f);
        }

        // ── SetNormalizedRotation ──

        [Test]
        public void SetNormalizedRotation_CenterValues_SetsToMidRange()
        {
            var origRotField = typeof(JoystickInteractable).GetField("_originalRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);
            origRotField?.SetValue(_joystick, Quaternion.identity);

            _joystick.XRotationRange = new Vector2(-45f, 45f);
            _joystick.ZRotationRange = new Vector2(-45f, 45f);

            _joystick.SetNormalizedRotation(0.5f, 0.5f);

            Assert.AreEqual(0f, _joystick.CurrentRotation.x, 0.5f);
            Assert.AreEqual(0f, _joystick.CurrentRotation.y, 0.5f);
        }

        // ── Asymmetric ranges ──

        [Test]
        public void XRotationRange_AsymmetricRange_ClampsCorrectly()
        {
            _joystick.XRotationRange = new Vector2(-10f, 80f);

            Assert.AreEqual(-10f, _joystick.XRotationRange.x, 0.01f);
            Assert.AreEqual(80f, _joystick.XRotationRange.y, 0.01f);
        }

        [Test]
        public void ZRotationRange_AsymmetricRange_ClampsCorrectly()
        {
            _joystick.ZRotationRange = new Vector2(-80f, 10f);

            Assert.AreEqual(-80f, _joystick.ZRotationRange.x, 0.01f);
            Assert.AreEqual(10f, _joystick.ZRotationRange.y, 0.01f);
        }
    }
}

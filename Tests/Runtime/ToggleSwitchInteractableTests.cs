using System.Reflection;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class ToggleSwitchInteractableTests
    {
        private ToggleSwitchInteractable _toggle;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("ToggleSwitchTest");
            _toggle = go.AddComponent<ToggleSwitchInteractable>();

            // Initialize _originalRotation (inherited protected) so rotation math works without play mode.
            var origRotField = typeof(ToggleSwitchInteractable).GetField("_originalRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);
            origRotField?.SetValue(_toggle, Quaternion.identity);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_toggle);
        }

        private void SetNumberOfSteps(int steps)
        {
            var field = typeof(ToggleSwitchInteractable).GetField("numberOfSteps",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_toggle, steps);
        }

        [Test]
        public void NumberOfSteps_DefaultsToTwo()
        {
            Assert.AreEqual(2, _toggle.NumberOfSteps);
        }

        [Test]
        public void SetStep_WithinRange_SetsCurrentStep()
        {
            SetNumberOfSteps(4);
            _toggle.SetStep(2);
            Assert.AreEqual(2, _toggle.CurrentStep);
        }

        [Test]
        public void SetStep_AboveMax_ClampsToLastStep()
        {
            SetNumberOfSteps(4);
            _toggle.SetStep(10);
            Assert.AreEqual(3, _toggle.CurrentStep);
        }

        [Test]
        public void SetStep_BelowZero_ClampsToZero()
        {
            SetNumberOfSteps(4);
            _toggle.SetStep(-5);
            Assert.AreEqual(0, _toggle.CurrentStep);
        }

        [Test]
        public void IncrementStep_AdvancesByOne()
        {
            SetNumberOfSteps(4);
            _toggle.SetStep(1);
            _toggle.IncrementStep();
            Assert.AreEqual(2, _toggle.CurrentStep);
        }

        [Test]
        public void DecrementStep_GoesBackByOne()
        {
            SetNumberOfSteps(4);
            _toggle.SetStep(2);
            _toggle.DecrementStep();
            Assert.AreEqual(1, _toggle.CurrentStep);
        }

        [Test]
        public void IncrementStep_AtLastStep_StaysClamped()
        {
            SetNumberOfSteps(3);
            _toggle.SetStep(2);
            _toggle.IncrementStep();
            Assert.AreEqual(2, _toggle.CurrentStep);
        }

        [Test]
        public void NormalizedValue_AtFirstStep_IsZero()
        {
            SetNumberOfSteps(5);
            _toggle.SetStep(0);
            Assert.AreEqual(0f, _toggle.NormalizedValue, 0.001f);
        }

        [Test]
        public void NormalizedValue_AtLastStep_IsOne()
        {
            SetNumberOfSteps(5);
            _toggle.SetStep(4);
            Assert.AreEqual(1f, _toggle.NormalizedValue, 0.001f);
        }

        [Test]
        public void NormalizedValue_AtMiddleStep_IsHalf()
        {
            SetNumberOfSteps(5);
            _toggle.SetStep(2);
            Assert.AreEqual(0.5f, _toggle.NormalizedValue, 0.001f);
        }

        [Test]
        public void SetStep_MapsToExpectedAngle()
        {
            SetNumberOfSteps(2);
            _toggle.AngleRange = new Vector2(-40f, 40f);

            _toggle.SetStep(0);
            Assert.AreEqual(-40f, _toggle.CurrentAngle, 0.01f);

            _toggle.SetStep(1);
            Assert.AreEqual(40f, _toggle.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetNormalizedStep_MapsToNearestStep()
        {
            SetNumberOfSteps(5);
            _toggle.SetNormalizedStep(0.5f);
            Assert.AreEqual(2, _toggle.CurrentStep);
        }

        [Test]
        public void SetStep_FiresOnStepChanged()
        {
            SetNumberOfSteps(4);
            int received = -1;
            var disposable = _toggle.OnStepChanged.Subscribe(s => received = s);

            _toggle.SetStep(2);

            Assert.AreEqual(2, received);
            disposable.Dispose();
        }

        [Test]
        public void SetStep_FiresOnStepConfirmed()
        {
            SetNumberOfSteps(4);
            bool confirmed = false;
            var disposable = _toggle.OnStepConfirmed.Subscribe(_ => confirmed = true);

            _toggle.SetStep(1);

            Assert.IsTrue(confirmed);
            disposable.Dispose();
        }
    }
}

using System.Reflection;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class DialInteractableTests
    {
        private DialInteractable _dial;

        private void SetSerializedField(string fieldName, object value)
        {
            var field = typeof(DialInteractable).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_dial, value);
        }

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("DialTest");
            _dial = go.AddComponent<DialInteractable>();

            // Initialize _originalRotation so ApplyDialRotation works
            var origRotField = typeof(DialInteractable).GetField("_originalRotation",
                BindingFlags.NonPublic | BindingFlags.Instance);
            origRotField?.SetValue(_dial, Quaternion.identity);

            SetSerializedField("numberOfSteps", 8);
            SetSerializedField("totalAngle", 360f);
            SetSerializedField("startingStep", 0);
            SetSerializedField("wrapAround", false);

            var prevStepField = typeof(DialInteractable).GetField("_previousStep",
                BindingFlags.NonPublic | BindingFlags.Instance);
            prevStepField?.SetValue(_dial, 0);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_dial);
        }

        // ── AnglePerStep Calculation ──

        [Test]
        public void AnglePerStep_8Steps360Degrees_Is45()
        {
            Assert.AreEqual(45f, _dial.AnglePerStep, 0.01f);
        }

        [Test]
        public void AnglePerStep_4Steps360Degrees_Is90()
        {
            SetSerializedField("numberOfSteps", 4);

            Assert.AreEqual(90f, _dial.AnglePerStep, 0.01f);
        }

        [Test]
        public void AnglePerStep_10Steps180Degrees_Is18()
        {
            SetSerializedField("numberOfSteps", 10);
            SetSerializedField("totalAngle", 180f);

            Assert.AreEqual(18f, _dial.AnglePerStep, 0.01f);
        }

        // ── NormalizedValue ──

        [Test]
        public void NormalizedValue_AtStep0_IsZero()
        {
            _dial.SetStep(0);

            Assert.AreEqual(0f, _dial.NormalizedValue, 0.01f);
        }

        [Test]
        public void NormalizedValue_AtLastStep_IsOne()
        {
            _dial.SetStep(7);

            Assert.AreEqual(1f, _dial.NormalizedValue, 0.01f);
        }

        [Test]
        public void NormalizedValue_AtMiddleStep_IsHalf()
        {
            SetSerializedField("numberOfSteps", 5);
            _dial.SetStep(2);

            Assert.AreEqual(0.5f, _dial.NormalizedValue, 0.01f);
        }

        // ── SetStep ──

        [Test]
        public void SetStep_ValidStep_SetsCurrentStep()
        {
            _dial.SetStep(3);

            Assert.AreEqual(3, _dial.CurrentStep);
        }

        [Test]
        public void SetStep_ValidStep_SetsCorrectAngle()
        {
            _dial.SetStep(2);

            Assert.AreEqual(90f, _dial.CurrentAngle, 0.01f);
        }

        [Test]
        public void SetStep_AboveMax_ClampsToLastStep()
        {
            _dial.SetStep(100);

            Assert.AreEqual(7, _dial.CurrentStep);
        }

        [Test]
        public void SetStep_BelowZero_ClampsToZero()
        {
            _dial.SetStep(-5);

            Assert.AreEqual(0, _dial.CurrentStep);
        }

        // ── IncrementStep ──

        [Test]
        public void IncrementStep_FromZero_GoesToOne()
        {
            _dial.SetStep(0);
            _dial.IncrementStep();

            Assert.AreEqual(1, _dial.CurrentStep);
        }

        [Test]
        public void IncrementStep_NoWrap_ClampsAtMax()
        {
            SetSerializedField("wrapAround", false);
            _dial.SetStep(7);
            _dial.IncrementStep();

            Assert.AreEqual(7, _dial.CurrentStep);
        }

        [Test]
        public void IncrementStep_WithWrap_WrapsToZero()
        {
            SetSerializedField("wrapAround", true);
            _dial.SetStep(7);
            _dial.IncrementStep();

            Assert.AreEqual(0, _dial.CurrentStep);
        }

        [Test]
        public void IncrementStep_MultipleIncrements_AdvancesCorrectly()
        {
            _dial.SetStep(0);
            _dial.IncrementStep();
            _dial.IncrementStep();
            _dial.IncrementStep();

            Assert.AreEqual(3, _dial.CurrentStep);
        }

        // ── DecrementStep ──

        [Test]
        public void DecrementStep_FromThree_GoesToTwo()
        {
            _dial.SetStep(3);
            _dial.DecrementStep();

            Assert.AreEqual(2, _dial.CurrentStep);
        }

        [Test]
        public void DecrementStep_NoWrap_ClampsAtZero()
        {
            SetSerializedField("wrapAround", false);
            _dial.SetStep(0);
            _dial.DecrementStep();

            Assert.AreEqual(0, _dial.CurrentStep);
        }

        [Test]
        public void DecrementStep_WithWrap_WrapsToLastStep()
        {
            SetSerializedField("wrapAround", true);
            _dial.SetStep(0);
            _dial.DecrementStep();

            Assert.AreEqual(7, _dial.CurrentStep);
        }

        // ── SetNormalized ──

        [Test]
        public void SetNormalized_Zero_SetsStepToZero()
        {
            _dial.SetNormalized(0f);

            Assert.AreEqual(0, _dial.CurrentStep);
        }

        [Test]
        public void SetNormalized_One_SetsStepToMax()
        {
            _dial.SetNormalized(1f);

            Assert.AreEqual(7, _dial.CurrentStep);
        }

        [Test]
        public void SetNormalized_Half_SetsMiddleStep()
        {
            _dial.SetNormalized(0.5f);

            // RoundToInt(0.5 * 7) = RoundToInt(3.5) = 4
            Assert.AreEqual(4, _dial.CurrentStep);
        }

        // ── ResetDial ──

        [Test]
        public void ResetDial_ReturnsToStartingStep()
        {
            SetSerializedField("startingStep", 3);
            _dial.SetStep(7);
            _dial.ResetDial();

            Assert.AreEqual(3, _dial.CurrentStep);
        }

        [Test]
        public void ResetDial_DefaultStartingStep_ReturnsToZero()
        {
            _dial.SetStep(5);
            _dial.ResetDial();

            Assert.AreEqual(0, _dial.CurrentStep);
        }

        // ── Event Firing ──

        [Test]
        public void SetStep_FiresOnStepChanged()
        {
            int receivedStep = -1;
            var disposable = _dial.OnStepChanged.Do(step => receivedStep = step).Subscribe();

            _dial.SetStep(4);

            Assert.AreEqual(4, receivedStep);
            disposable.Dispose();
        }

        [Test]
        public void SetStep_FiresOnStepConfirmed()
        {
            int confirmedStep = -1;
            var disposable = _dial.OnStepConfirmed.Do(step => confirmedStep = step).Subscribe();

            _dial.SetStep(3);

            Assert.AreEqual(3, confirmedStep);
            disposable.Dispose();
        }

        [Test]
        public void IncrementStep_FiresOnStepChanged()
        {
            int receivedStep = -1;
            var disposable = _dial.OnStepChanged.Do(step => receivedStep = step).Subscribe();

            _dial.SetStep(0);
            _dial.IncrementStep();

            Assert.AreEqual(1, receivedStep);
            disposable.Dispose();
        }

        // ── NumberOfSteps ──

        [Test]
        public void NumberOfSteps_ReturnsConfiguredValue()
        {
            SetSerializedField("numberOfSteps", 12);

            Assert.AreEqual(12, _dial.NumberOfSteps);
        }

        // ── Angle-Step Consistency ──

        [Test]
        public void SetStep_AngleMatchesStepTimesAnglePerStep()
        {
            for (int i = 0; i < 8; i++)
            {
                _dial.SetStep(i);
                float expectedAngle = i * _dial.AnglePerStep;
                Assert.AreEqual(expectedAngle, _dial.CurrentAngle, 0.01f,
                    $"Step {i} should have angle {expectedAngle}");
            }
        }

        // ── WrapAround full cycle ──

        [Test]
        public void IncrementStep_WithWrap_FullCycleReturnsToStart()
        {
            SetSerializedField("wrapAround", true);
            _dial.SetStep(0);

            for (int i = 0; i < 8; i++)
                _dial.IncrementStep();

            Assert.AreEqual(0, _dial.CurrentStep);
        }

        [Test]
        public void DecrementStep_WithWrap_FullCycleReturnsToStart()
        {
            SetSerializedField("wrapAround", true);
            _dial.SetStep(0);

            for (int i = 0; i < 8; i++)
                _dial.DecrementStep();

            Assert.AreEqual(0, _dial.CurrentStep);
        }
    }
}

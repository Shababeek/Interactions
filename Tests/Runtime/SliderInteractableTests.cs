using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class SliderInteractableTests
    {
        private SliderInteractable _slider;
        private CompositeDisposable _subscriptions;

        [SetUp]
        public void SetUp()
        {
            _slider = new GameObject("SliderTest").AddComponent<SliderInteractable>();
            _subscriptions = new CompositeDisposable();
        }

        [TearDown]
        public void TearDown()
        {
            _subscriptions.Dispose();
            TestHelpers.DestroyComponent(_slider);
        }

        [Test]
        public void NumberOfSteps_DefaultsToFour()
        {
            Assert.AreEqual(4, _slider.NumberOfSteps);
        }

        [TestCase(0f, 0)]
        [TestCase(0.3f, 1)]
        [TestCase(0.7f, 2)]
        [TestCase(1f, 3)]
        [TestCase(-2f, 0)]
        [TestCase(5f, 3)]
        public void NormalizedToStep_PicksTheNearestStep(float normalized, int expectedStep)
        {
            Assert.AreEqual(expectedStep, _slider.NormalizedToStep(normalized));
        }

        [Test]
        public void SetStep_ClampsToTheStepRange()
        {
            _slider.SetStep(99);
            Assert.AreEqual(3, _slider.CurrentStep);

            _slider.SetStep(-5);
            Assert.AreEqual(0, _slider.CurrentStep);
        }

        [Test]
        public void SetStep_RaisesMovedChangedAndConfirmed()
        {
            var changed = new List<int>();
            var confirmed = new List<int>();
            var moved = new List<float>();
            _slider.OnStepChanged.Subscribe(changed.Add).AddTo(_subscriptions);
            _slider.OnStepConfirmed.Subscribe(confirmed.Add).AddTo(_subscriptions);
            _slider.OnMoved.Subscribe(moved.Add).AddTo(_subscriptions);

            _slider.SetStep(2);

            CollectionAssert.AreEqual(new[] { 2 }, changed);
            CollectionAssert.AreEqual(new[] { 2 }, confirmed);
            Assert.AreEqual(1, moved.Count);
            Assert.AreEqual(2f / 3f, moved[0], 0.0001f);
        }

        [Test]
        public void NormalizedValue_FollowsTheCurrentStep()
        {
            _slider.SetStep(0);
            Assert.AreEqual(0f, _slider.NormalizedValue, 0.0001f);

            _slider.SetStep(3);
            Assert.AreEqual(1f, _slider.NormalizedValue, 0.0001f);
        }

        [Test]
        public void IncrementAndDecrement_StopAtTheEnds()
        {
            _slider.SetStep(2);
            _slider.IncrementStep();
            _slider.IncrementStep();
            Assert.AreEqual(3, _slider.CurrentStep);

            _slider.SetStep(1);
            _slider.DecrementStep();
            _slider.DecrementStep();
            Assert.AreEqual(0, _slider.CurrentStep);
        }

        [Test]
        public void SetNormalized_SnapsToTheNearestStep()
        {
            _slider.SetNormalized(0.6f);
            Assert.AreEqual(2, _slider.CurrentStep);
        }

        [Test]
        public void ResetSlider_ReturnsToTheStartingStep()
        {
            _slider.StartingStep = 1;
            _slider.SetStep(3);

            _slider.ResetSlider();

            Assert.AreEqual(1, _slider.CurrentStep);
        }

        [UnityTest]
        public IEnumerator Start_AppliesTheStartingStep()
        {
            var slider = CreateSliderBeforeStart();
            slider.StartingStep = 3;

            yield return null;

            Assert.AreEqual(3, slider.CurrentStep);
            Assert.AreEqual(1f, slider.CurrentNormalized, 0.0001f);
            TestHelpers.DestroyComponent(slider);
        }

        [UnityTest]
        public IEnumerator StepSetBeforeStart_WinsOverTheStartingStep()
        {
            var slider = CreateSliderBeforeStart();
            slider.StartingStep = 1;
            slider.SetStep(2);

            yield return null;

            Assert.AreEqual(2, slider.CurrentStep);
            Assert.AreEqual(2f / 3f, slider.CurrentNormalized, 0.0001f);
            TestHelpers.DestroyComponent(slider);
        }

        [UnityTest]
        public IEnumerator GetStepWorldPosition_SpreadsStepsAlongTheTrack()
        {
            yield return null;

            var first = _slider.GetStepWorldPosition(0, 0f);
            var last = _slider.GetStepWorldPosition(3, 0f);
            var second = _slider.GetStepWorldPosition(1, 0f);

            Assert.AreEqual(Vector3.Distance(first, last) / 3f, Vector3.Distance(first, second), 0.0001f);
        }

        private static SliderInteractable CreateSliderBeforeStart()
        {
            // Created and configured inside the test body, so Start has not run yet.
            return new GameObject("SliderStartTest").AddComponent<SliderInteractable>();
        }
    }
}

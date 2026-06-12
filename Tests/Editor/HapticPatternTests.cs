using NUnit.Framework;
using Shababeek.Interactions;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class HapticPatternTests
    {
        private HapticPattern _pattern;

        [SetUp]
        public void SetUp()
        {
            _pattern = ScriptableObject.CreateInstance<HapticPattern>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_pattern);
        }

        [Test]
        public void Evaluate_AppliesStrengthMultiplier()
        {
            _pattern.SetShape(AnimationCurve.Constant(0f, 1f, 1f), 0.1f, 0.5f);

            Assert.AreEqual(0.5f, _pattern.Evaluate(0.5f), 1e-5f);
        }

        [Test]
        public void Evaluate_ClampsResultToUnitRange()
        {
            _pattern.SetShape(AnimationCurve.Constant(0f, 1f, 5f), 0.1f, 1f);
            Assert.AreEqual(1f, _pattern.Evaluate(0.5f), 1e-5f);

            _pattern.SetShape(AnimationCurve.Constant(0f, 1f, -2f), 0.1f, 1f);
            Assert.AreEqual(0f, _pattern.Evaluate(0.5f), 1e-5f);
        }

        [Test]
        public void Evaluate_ClampsNormalizedTime()
        {
            _pattern.SetShape(AnimationCurve.Linear(0f, 0f, 1f, 1f), 0.1f, 1f);

            Assert.AreEqual(0f, _pattern.Evaluate(-1f), 1e-5f);
            Assert.AreEqual(1f, _pattern.Evaluate(2f), 1e-5f);
        }

        [Test]
        public void SetShape_EnforcesMinimumDuration()
        {
            _pattern.SetShape(AnimationCurve.Constant(0f, 1f, 1f), 0f, 1f);

            Assert.GreaterOrEqual(_pattern.Duration, 0.01f);
        }
    }
}

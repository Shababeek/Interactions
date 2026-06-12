using NUnit.Framework;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class FingerConstraintModeTests
    {
        [Test]
        public void LegacyLockedData_ResolvesToFixed()
        {
            // Simulates deserialized pre-enum data: mode is Unset (0), locked flag set.
            var c = new FingerConstraints { locked = true, min = 0.4f, max = 0.9f };

            Assert.AreEqual(FingerConstraintMode.Fixed, c.Mode);
            Assert.AreEqual(0.4f, c.GetConstrainedValue(0f), 1e-5f);
            Assert.AreEqual(0.4f, c.GetConstrainedValue(1f), 1e-5f);
        }

        [Test]
        public void LegacyRangeData_ResolvesToRange()
        {
            var c = new FingerConstraints { locked = false, min = 0.2f, max = 0.8f };

            Assert.AreEqual(FingerConstraintMode.Range, c.Mode);
            Assert.AreEqual(0.5f, c.GetConstrainedValue(0.5f), 1e-5f);
            Assert.AreEqual(0.2f, c.GetConstrainedValue(0f), 1e-5f);
            Assert.AreEqual(0.8f, c.GetConstrainedValue(1f), 1e-5f);
        }

        [Test]
        public void LegacyFullRangeData_ResolvesToFree()
        {
            var c = new FingerConstraints { locked = false, min = 0f, max = 1f };

            Assert.AreEqual(FingerConstraintMode.Free, c.Mode);
            Assert.AreEqual(0.37f, c.GetConstrainedValue(0.37f), 1e-5f);
        }

        [Test]
        public void FixedMode_HoldsValueAndIgnoresInput()
        {
            var c = new FingerConstraints(FingerConstraintMode.Fixed, 0.6f, 1f);

            Assert.AreEqual(0.6f, c.GetConstrainedValue(0f), 1e-5f);
            Assert.AreEqual(0.6f, c.GetConstrainedValue(1f), 1e-5f);
            Assert.AreEqual(0.6f, c.FixedValue, 1e-5f);
        }

        [Test]
        public void ModeSetter_KeepsLegacyLockedFlagInSync()
        {
            var c = new FingerConstraints(FingerConstraintMode.Range, 0.1f, 0.9f);
            Assert.IsFalse(c.locked);

            c.Mode = FingerConstraintMode.Fixed;
            Assert.IsTrue(c.locked);

            c.Mode = FingerConstraintMode.Free;
            Assert.IsFalse(c.locked);
        }

        [Test]
        public void SwitchingFixedToRange_PreservesAuthoredMax()
        {
            var c = new FingerConstraints(FingerConstraintMode.Range, 0.2f, 0.8f);

            c.Mode = FingerConstraintMode.Fixed;
            c.FixedValue = 0.5f;
            c.Mode = FingerConstraintMode.Range;

            Assert.AreEqual(0.8f, c.max, 1e-5f, "Max should survive a round-trip through Fixed mode.");
        }

        [Test]
        public void LegacyConstructor_MigratesMode()
        {
            Assert.AreEqual(FingerConstraintMode.Fixed, new FingerConstraints(true, 0.3f, 1f).Mode);
            Assert.AreEqual(FingerConstraintMode.Range, new FingerConstraints(false, 0.3f, 1f).Mode);
            Assert.AreEqual(FingerConstraintMode.Free, new FingerConstraints(false, 0f, 1f).Mode);
        }
    }
}

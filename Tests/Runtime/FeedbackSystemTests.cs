using NUnit.Framework;
using Shababeek.Interactions.Feedback;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class FeedbackSystemTests
    {
        private FeedbackSystem _feedbackSystem;
        private TestInteractable _interactable;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("FeedbackTestObject");
            _interactable = go.AddComponent<TestInteractable>();
            _feedbackSystem = go.AddComponent<FeedbackSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_feedbackSystem);
            TestHelpers.DestroyComponent(_interactable);
        }

        [Test]
        public void FeedbackSystem_CanAddFeedback()
        {
            var feedback = new MaterialFeedback();
            _feedbackSystem.AddFeedback(feedback);

            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.Contains(feedback, feedbacks);
        }

        [Test]
        public void FeedbackSystem_CanRemoveFeedback()
        {
            var feedback = new MaterialFeedback();
            _feedbackSystem.AddFeedback(feedback);
            _feedbackSystem.RemoveFeedback(feedback);

            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.IsEmpty(feedbacks);
        }

        [Test]
        public void FeedbackSystem_CanClearAllFeedbacks()
        {
            var feedback1 = new MaterialFeedback();
            var feedback2 = new AnimationFeedback();
            _feedbackSystem.AddFeedback(feedback1);
            _feedbackSystem.AddFeedback(feedback2);

            _feedbackSystem.ClearFeedbacks();
            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.IsEmpty(feedbacks);
        }

        [Test]
        public void FeedbackSystem_GetFeedbacksReturnsValidList()
        {
            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.IsNotNull(feedbacks);
            Assert.IsInstanceOf<System.Collections.Generic.List<FeedbackData>>(feedbacks);
        }

        [Test]
        public void FeedbackSystem_CanAddMaterialFeedback()
        {
            _feedbackSystem.AddFeedback();
            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.Greater(feedbacks.Count, 0);
        }

        [Test]
        public void FeedbackSystem_NoDuplicateFeedbacks()
        {
            var feedback = new MaterialFeedback();
            _feedbackSystem.AddFeedback(feedback);
            _feedbackSystem.AddFeedback(feedback);

            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.AreEqual(1, feedbacks.Count);
        }

        [Test]
        public void FeedbackSystem_RemoveNonExistentFeedback()
        {
            var feedback = new MaterialFeedback();
            _feedbackSystem.RemoveFeedback(feedback);

            // Should not throw
            Assert.Pass();
        }

        [Test]
        public void FeedbackData_MaterialFeedback_HasValidName()
        {
            var feedback = new MaterialFeedback();
            Assert.AreEqual("Material Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackData_AnimationFeedback_HasValidName()
        {
            var feedback = new AnimationFeedback();
            Assert.AreEqual("Animation Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackData_HapticFeedback_HasValidName()
        {
            var feedback = new HapticFeedback();
            Assert.AreEqual("Haptic Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackData_AudioFeedback_HasValidName()
        {
            var feedback = new AudioFeedback();
            Assert.AreEqual("Audio Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackData_CanSetName()
        {
            var feedback = new MaterialFeedback();
            feedback.FeedbackName = "Custom Feedback";
            Assert.AreEqual("Custom Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackData_CanToggleEnabled()
        {
            var feedback = new MaterialFeedback();
            feedback.Enabled = false;
            Assert.IsFalse(feedback.Enabled);

            feedback.Enabled = true;
            Assert.IsTrue(feedback.Enabled);
        }

        [Test]
        public void FeedbackData_IsValid_ChecksEnabled()
        {
            var feedback = new MaterialFeedback();
            feedback.Enabled = false;
            Assert.IsFalse(feedback.IsValid());

            feedback.Enabled = true;
            // IsValid also checks for proper initialization, so result may vary
        }

        [Test]
        public void FeedbackSystem_CanAddMultipleTypes()
        {
            var material = new MaterialFeedback();
            var animation = new AnimationFeedback();
            var haptic = new HapticFeedback();

            _feedbackSystem.AddFeedback(material);
            _feedbackSystem.AddFeedback(animation);
            _feedbackSystem.AddFeedback(haptic);

            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.AreEqual(3, feedbacks.Count);
        }

        [Test]
        public void FeedbackData_ObjectToggleFeedback_HasValidName()
        {
            var feedback = new ObjectToggleFeedback();
            Assert.AreEqual("Object Toggle Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackData_ScaleFeedback_HasValidName()
        {
            var feedback = new ScaleFeedback();
            Assert.AreEqual("Scale Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackData_ParticleFeedback_HasValidName()
        {
            var feedback = new ParticleFeedback();
            Assert.AreEqual("Particle Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackData_UnityEventFeedback_HasValidName()
        {
            var feedback = new UnityEventFeedback();
            Assert.AreEqual("UnityEvent Feedback", feedback.FeedbackName);
        }

        [Test]
        public void FeedbackSystem_RemoveAndReaddFeedback()
        {
            var feedback = new MaterialFeedback();
            _feedbackSystem.AddFeedback(feedback);
            _feedbackSystem.RemoveFeedback(feedback);

            // Should be able to add again
            _feedbackSystem.AddFeedback(feedback);
            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.AreEqual(1, feedbacks.Count);
        }

        [Test]
        public void FeedbackSystem_ClearAndReaddFeedback()
        {
            var feedback1 = new MaterialFeedback();
            var feedback2 = new AnimationFeedback();
            _feedbackSystem.AddFeedback(feedback1);
            _feedbackSystem.AddFeedback(feedback2);

            _feedbackSystem.ClearFeedbacks();
            var feedback3 = new HapticFeedback();
            _feedbackSystem.AddFeedback(feedback3);

            var feedbacks = _feedbackSystem.GetFeedbacks();
            Assert.AreEqual(1, feedbacks.Count);
            Assert.Contains(feedback3, feedbacks);
        }
    }
}

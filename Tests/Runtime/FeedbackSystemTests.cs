using NUnit.Framework;
using Shababeek.Interactions.Core;
using Shababeek.Interactions.Feedback;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    /// <summary>
    /// Feedback pipeline: list lifecycle plus forwarding interaction observables to valid feedback entries.
    /// </summary>
    [TestFixture]
    public class FeedbackSystemTests
    {
        private GameObject _root;
        private TestInteractable _interactable;
        private FeedbackSystem _feedbackSystem;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("FeedbackTestRoot");
            _interactable = _root.AddComponent<TestInteractable>();
            _feedbackSystem = _root.AddComponent<FeedbackSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyGameObject(_root);
        }

        private sealed class RecordingFeedback : FeedbackData
        {
            public int HoverStartedCount;
            public int HoverEndedCount;
            public int SelectedCount;
            public int DeselectedCount;
            public int ActivatedCount;

            public override void OnHoverStarted(InteractorBase interactor)
            {
                HoverStartedCount++;
            }

            public override void OnHoverEnded(InteractorBase interactor)
            {
                HoverEndedCount++;
            }

            public override void OnSelected(InteractorBase interactor)
            {
                SelectedCount++;
            }

            public override void OnDeselected(InteractorBase interactor)
            {
                DeselectedCount++;
            }

            public override void OnActivated(InteractorBase interactor)
            {
                ActivatedCount++;
            }
        }

        [Test]
        public void FeedbackRoutesHoverSelectedUse_ToValidFeedback()
        {
            var spy = new RecordingFeedback();
            _feedbackSystem.AddFeedback(spy);

            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            Assert.AreEqual(1, spy.HoverStartedCount);

            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            Assert.AreEqual(1, spy.HoverEndedCount);
            Assert.AreEqual(1, spy.SelectedCount);

            _interactable.StartUsing(interactor);
            Assert.AreEqual(1, spy.ActivatedCount);

            _interactable.OnStateChanged(InteractionState.None, interactor);
            Assert.AreEqual(1, spy.DeselectedCount);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void DisabledFeedback_SkipsCallbacks()
        {
            var spy = new RecordingFeedback { Enabled = false };
            _feedbackSystem.AddFeedback(spy);

            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);

            Assert.AreEqual(0, spy.HoverStartedCount);
            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void LateAddedFeedback_StillReceivesSubsequentEvents()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);

            var spy = new RecordingFeedback();
            _feedbackSystem.AddFeedback(spy);

            _interactable.OnStateChanged(InteractionState.None, interactor);
            Assert.AreEqual(1, spy.HoverEndedCount);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void Add_Remove_Clear_ManageList()
        {
            var a = new RecordingFeedback();
            var b = new RecordingFeedback();

            _feedbackSystem.AddFeedback(a);
            _feedbackSystem.AddFeedback(b);
            Assert.AreEqual(2, _feedbackSystem.GetFeedbacks().Count);

            _feedbackSystem.RemoveFeedback(a);
            Assert.AreEqual(1, _feedbackSystem.GetFeedbacks().Count);

            _feedbackSystem.ClearFeedbacks();
            Assert.IsEmpty(_feedbackSystem.GetFeedbacks());
        }

        [Test]
        public void AddFeedback_IgnoresDuplicateInstance()
        {
            var fb = new RecordingFeedback();
            _feedbackSystem.AddFeedback(fb);
            _feedbackSystem.AddFeedback(fb);
            Assert.AreEqual(1, _feedbackSystem.GetFeedbacks().Count);
        }

        [Test]
        public void RemoveNonExistentFeedback_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                _feedbackSystem.RemoveFeedback(new RecordingFeedback()));
        }

        [Test]
        public void AddFeedback_Parameterless_AddsMaterialFeedbackEntry()
        {
            _feedbackSystem.AddFeedback();
            var list = _feedbackSystem.GetFeedbacks();
            Assert.AreEqual(1, list.Count);
            Assert.IsInstanceOf<MaterialFeedback>(list[0]);
        }
    }
}

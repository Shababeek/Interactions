using System;
using System.Collections;
using NUnit.Framework;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shababeek.Interactions.Tests
{
    /// <summary>
    /// Edge cases of the InteractableBase state machine: aborted selections, deselect ordering
    /// and the hand-layer hand-off. Plain state sequences live in <see cref="InteractableStateFlowTests"/>.
    /// </summary>
    [TestFixture]
    public class InteractableLifecycleTests
    {
        private const float LayerRestoreWait = 0.35f;

        private TestInteractable _interactable;
        private TestInteractor _left;
        private TestInteractor _right;
        private CompositeDisposable _subscriptions;

        [SetUp]
        public void SetUp()
        {
            _interactable = TestHelpers.CreateTestInteractable();
            _left = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _right = TestHelpers.CreateMockInteractor(HandIdentifier.Right);
            _subscriptions = new CompositeDisposable();
        }

        [TearDown]
        public void TearDown()
        {
            _subscriptions.Dispose();
            TestHelpers.DestroyComponent(_interactable);
            TestHelpers.DestroyComponent(_left);
            TestHelpers.DestroyComponent(_right);
        }

        [Test]
        public void AbortedSelection_FromHover_LandsInNoneWithoutInteractor()
        {
            _interactable.SetSelectResult(true);
            _interactable.OnStateChanged(InteractionState.Hovering, _left);

            _interactable.OnStateChanged(InteractionState.Selected, _left);

            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            Assert.IsFalse(_interactable.IsSelected);
            Assert.IsNull(_interactable.CurrentInteractor);
        }

        [Test]
        public void AbortedSelection_WithoutHover_ClearsInteractor()
        {
            _interactable.SetSelectResult(true);

            _interactable.OnStateChanged(InteractionState.Selected, _left);

            Assert.AreEqual(InteractionState.None, _interactable.CurrentState);
            Assert.IsNull(_interactable.CurrentInteractor);
        }

        [Test]
        public void AbortedSelection_HoverEventsStayBalanced()
        {
            int starts = 0, ends = 0;
            _interactable.OnHoverStarted.Subscribe(_ => starts++).AddTo(_subscriptions);
            _interactable.OnHoverEnded.Subscribe(_ => ends++).AddTo(_subscriptions);
            _interactable.SetSelectResult(true);

            _interactable.OnStateChanged(InteractionState.Hovering, _left);
            _interactable.OnStateChanged(InteractionState.Selected, _left);
            _interactable.OnStateChanged(InteractionState.Hovering, _left);
            _interactable.OnStateChanged(InteractionState.None, _left);

            Assert.AreEqual(2, starts, "the hover after an aborted selection must raise hover start again");
            Assert.AreEqual(2, ends);
        }

        [Test]
        public void Deselect_ToNone_RunsDeSelectedBeforeTheEvent()
        {
            int deselectCallsSeenByListener = -1;
            bool selectedSeenByListener = true;
            _interactable.OnDeselected.Subscribe(_ =>
            {
                deselectCallsSeenByListener = _interactable.DeSelectCallCount;
                selectedSeenByListener = _interactable.IsSelected;
            }).AddTo(_subscriptions);
            _interactable.OnStateChanged(InteractionState.Selected, _left);

            _interactable.OnStateChanged(InteractionState.None, _left);

            Assert.AreEqual(1, deselectCallsSeenByListener);
            Assert.IsFalse(selectedSeenByListener);
        }

        [Test]
        public void Deselect_ToHover_RunsDeSelectedBeforeTheEvent()
        {
            int deselectCallsSeenByListener = -1;
            bool selectedSeenByListener = true;
            _interactable.OnDeselected.Subscribe(_ =>
            {
                deselectCallsSeenByListener = _interactable.DeSelectCallCount;
                selectedSeenByListener = _interactable.IsSelected;
            }).AddTo(_subscriptions);
            _interactable.OnStateChanged(InteractionState.Selected, _left);

            _interactable.OnStateChanged(InteractionState.Hovering, _left);

            Assert.AreEqual(1, deselectCallsSeenByListener);
            Assert.IsFalse(selectedSeenByListener);
        }

        [UnityTest]
        public IEnumerator Layers_MatchHandWhileHeld_AndReturnAfterRelease()
        {
            var layered = CreateLayeredInteractable(out var collider);
            _left.gameObject.layer = 4;

            layered.OnStateChanged(InteractionState.Selected, _left);
            Assert.AreEqual(4, layered.gameObject.layer);
            Assert.AreEqual(4, collider.gameObject.layer);

            layered.OnStateChanged(InteractionState.None, _left);
            yield return new WaitForSeconds(LayerRestoreWait);

            Assert.AreEqual(0, layered.gameObject.layer);
            Assert.AreEqual(0, collider.gameObject.layer);
            TestHelpers.DestroyComponent(layered);
        }

        [UnityTest]
        public IEnumerator Layers_QuickHandToHandPass_StillReturnToTheOriginalLayer()
        {
            var layered = CreateLayeredInteractable(out var collider);
            _left.gameObject.layer = 4;
            _right.gameObject.layer = 5;

            layered.OnStateChanged(InteractionState.Selected, _left);
            layered.OnStateChanged(InteractionState.None, _left);
            layered.OnStateChanged(InteractionState.Selected, _right);
            yield return new WaitForSeconds(LayerRestoreWait);

            Assert.AreEqual(5, layered.gameObject.layer, "the first hand's pending restore must not fire while the second hand holds the object");

            layered.OnStateChanged(InteractionState.None, _right);
            yield return new WaitForSeconds(LayerRestoreWait);

            Assert.AreEqual(0, layered.gameObject.layer);
            Assert.AreEqual(0, collider.gameObject.layer);
            TestHelpers.DestroyComponent(layered);
        }

        [UnityTest]
        public IEnumerator Layers_DestroyedDuringRestoreDelay_DoesNotThrow()
        {
            var layered = CreateLayeredInteractable(out _);
            _left.gameObject.layer = 4;
            layered.OnStateChanged(InteractionState.Selected, _left);
            layered.OnStateChanged(InteractionState.None, _left);

            UnityEngine.Object.Destroy(layered.gameObject);
            yield return new WaitForSeconds(LayerRestoreWait);

            // The test runner fails this test if the pending restore logged an exception.
            Assert.IsTrue(layered == null);
        }

        private static TestInteractable CreateLayeredInteractable(out Collider collider)
        {
            var go = new GameObject("LayeredInteractable");
            go.SetActive(false);
            var child = new GameObject("Collider");
            child.transform.SetParent(go.transform, false);
            collider = child.AddComponent<BoxCollider>();
            var interactable = go.AddComponent<TestInteractable>();
            go.SetActive(true);
            return interactable;
        }
    }
}

using NUnit.Framework;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    /// <summary>
    /// Helper utilities for creating test objects and mocking VR components.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a simple test interactable for testing base functionality.
        /// </summary>
        public static TestInteractable CreateTestInteractable()
        {
            var go = new GameObject("TestInteractable");
            var interactable = go.AddComponent<TestInteractable>();
            return interactable;
        }

        /// <summary>
        /// Creates a mock Hand component for testing.
        /// </summary>
        public static Hand CreateMockHand(HandIdentifier identifier)
        {
            var go = new GameObject($"Hand_{identifier}");
            var hand = go.AddComponent<Hand>();
            hand.HandIdentifier = identifier;
            return hand;
        }

        /// <summary>
        /// Creates a mock interactor with dependencies.
        /// </summary>
        public static TestInteractor CreateMockInteractor(HandIdentifier identifier)
        {
            var go = new GameObject($"Interactor_{identifier}");
            var hand = go.AddComponent<Hand>();
            hand.HandIdentifier = identifier;
            var interactor = go.AddComponent<TestInteractor>();
            return interactor;
        }

        /// <summary>
        /// Creates a simple GameObject with a transform.
        /// </summary>
        public static GameObject CreateGameObject(string name = "TestObject")
        {
            return new GameObject(name);
        }

        /// <summary>
        /// Creates a GameObject with a Rigidbody.
        /// </summary>
        public static Rigidbody CreateRigidbody(string name = "RigidbodyObject")
        {
            var go = new GameObject(name);
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            return rb;
        }

        /// <summary>
        /// Safely destroys a GameObject and waits for cleanup.
        /// </summary>
        public static void DestroyGameObject(GameObject go)
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Safely destroys a component and its GameObject.
        /// </summary>
        public static void DestroyComponent<T>(T component) where T : Component
        {
            if (component != null)
            {
                DestroyGameObject(component.gameObject);
            }
        }
    }

    /// <summary>
    /// Concrete implementation of InteractableBase for testing abstract functionality.
    /// </summary>
    public class TestInteractable : InteractableBase
    {
        public int SelectCallCount { get; private set; }
        public int DeSelectCallCount { get; private set; }
        public bool LastSelectResult { get; private set; }

        protected override bool Select()
        {
            SelectCallCount++;
            return LastSelectResult;
        }

        protected override void DeSelected()
        {
            DeSelectCallCount++;
        }

        public void ResetCounts()
        {
            SelectCallCount = 0;
            DeSelectCallCount = 0;
        }

        public void SetSelectResult(bool result)
        {
            LastSelectResult = result;
        }
    }

    /// <summary>
    /// Concrete implementation of InteractorBase for testing.
    /// </summary>
    public class TestInteractor : InteractorBase
    {
        public Vector3 TestInteractionPoint { get; set; } = Vector3.zero;

        public override Vector3 GetInteractionPoint()
        {
            return TestInteractionPoint;
        }
    }
}

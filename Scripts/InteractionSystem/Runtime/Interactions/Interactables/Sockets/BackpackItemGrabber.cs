using System;
using System.Collections.Generic;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    [RequireComponent(typeof(InventoryGridSocket))]
    [AddComponentMenu("Shababeek/Interactions/Interactables/Backpack Item Grabber")]
    public class BackpackItemGrabber : MonoBehaviour
    {
        [Tooltip("Distance from this transform within which a grip press counts as a grid grab.")]
        [SerializeField] private float grabRadius = 0.3f;

        private InventoryGridSocket _grid;
        private readonly List<IDisposable> _subs = new();

        private void Awake() => _grid = GetComponent<InventoryGridSocket>();

        private void Start()
        {
            foreach (var interactor in FindObjectsOfType<TriggerInteractor>())
                SubscribeInteractor(interactor);
        }

        private void OnDestroy()
        {
            foreach (var sub in _subs) sub.Dispose();
            _subs.Clear();
        }

        private void SubscribeInteractor(TriggerInteractor interactor)
        {
            var sub = interactor.Hand.OnGripButtonStateChange
                .Where(s => s == VRButtonState.Down)
                .Subscribe(_ => OnGripDown(interactor));
            _subs.Add(sub);
        }

        private void OnGripDown(TriggerInteractor interactor)
        {
            // The hand is allowed to hover the backpack itself (same GameObject) —
            // that doesn't block a grid grab. But if it's holding/hovering something
            // else entirely, don't interfere.
            var current = interactor.CurrentInteractable;
            bool hoveringBackpack = current != null && current.gameObject == gameObject;
            if (current != null && !hoveringBackpack) return;

            var handPos = interactor.GetInteractionPoint();
            float dist = Vector3.Distance(handPos, transform.position);

            if (dist > grabRadius)
                return;

            var socketable = _grid.GetItemAtWorldPosition(handPos);
            if (socketable == null)
            {
                Debug.Log($"[BackpackGrab] {interactor.HandIdentifier} grip at dist={dist:F3} — no item at hand position");
                return;
            }

            var grabable = socketable.GetComponent<Grabable>();
            if (grabable == null)
            {
                Debug.LogWarning($"[BackpackGrab] Item '{socketable.name}' has no Grabable — cannot grab out");
                return;
            }
            if (grabable.IsSelected)
            {
                Debug.LogWarning($"[BackpackGrab] Item '{socketable.name}' already selected — skipping");
                return;
            }

            // If the hand was hovering the backpack, release that hover before grabbing the item.
            if (hoveringBackpack)
                interactor.Release(current);

            Debug.Log($"[BackpackGrab] {interactor.HandIdentifier} → grabbing '{socketable.name}' from grid (dist={dist:F3})");
            interactor.CurrentInteractable = grabable;
            interactor.Select();
        }
    }
}

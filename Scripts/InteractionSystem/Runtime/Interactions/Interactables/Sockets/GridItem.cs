using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Component bridging a grabbable item to its <see cref="ItemFootprint"/> data.
    /// Captures the item's authored localScale in Awake as the "default scale" so the
    /// InventoryGridSocket can restore it after scaling the item down to fit its cells.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Grid Item")]
    [RequireComponent(typeof(Socketable))]
    public class GridItem : MonoBehaviour
    {
        [Tooltip("Footprint data describing this item's grid occupancy, scale, and placement.")]
        [SerializeField] private ItemFootprint footprint;

        private Vector3 _defaultScale;

        private void Awake()
        {
            _defaultScale = transform.localScale;
        }

        /// <summary>The item's localScale as captured in Awake, before any socketing.</summary>
        public Vector3 DefaultScale => _defaultScale;

        /// <summary>The footprint dimensions in cells. Falls back to 1x1 when no SO is assigned.</summary>
        public Vector2Int Footprint => footprint != null ? footprint.dimensions : Vector2Int.one;

        /// <summary>The assigned footprint data asset (may be null).</summary>
        public ItemFootprint Data => footprint;

        /// <summary>Test-only setter so NUnit tests can assign a runtime-created footprint.</summary>
        internal void SetFootprintForTest(ItemFootprint f) => footprint = f;
    }
}

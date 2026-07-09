using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// ScriptableObject describing an item's grid footprint: cell occupancy dimensions,
    /// display metadata, and placement offsets. One asset per item type.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemFootprint", menuName = "Shababeek/Interactions/Item Footprint", order = 0)]
    public class ItemFootprint : ScriptableObject
    {
        [Tooltip("Width (X) and Height (Y) in grid cells this item occupies.")]
        [SerializeField]
        public Vector2Int dimensions = new Vector2Int(1, 1);

        [Tooltip("Human-readable item name.")]
        [SerializeField] private string displayName = "";

        [Tooltip("Optional icon for UI display.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Local position offset (relative to the footprint center) applied when socketed.")]
        [SerializeField] private Vector3 placementOffset = Vector3.zero;

        [Tooltip("Local Euler rotation offset applied to this item when socketed. Use to orient each item correctly in the grid.")]
        [SerializeField] private Vector3 placementRotation = Vector3.zero;

        public Vector2Int Dimensions => dimensions;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Vector3 PlacementOffset => placementOffset;
        public Vector3 PlacementRotation => placementRotation;

        private void OnValidate()
        {
            dimensions.x = Mathf.Max(1, dimensions.x);
            dimensions.y = Mathf.Max(1, dimensions.y);
        }

        /// <summary>Test-only setter so NUnit tests can set dimensions on a runtime-created asset.</summary>
        internal void SetDimensionsForTest(Vector2Int d) => dimensions = d;
    }
}

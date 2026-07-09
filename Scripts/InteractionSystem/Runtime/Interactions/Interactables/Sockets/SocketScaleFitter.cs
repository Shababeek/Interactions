using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Computes the uniform scale that fits an item's combined Renderer bounds
    /// into a target 2D rectangle (the grid footprint area). Pure computation —
    /// does NOT apply the scale. The caller (InventoryGridSocket) applies it.
    /// </summary>
    public static class SocketScaleFitter
    {
        /// <summary>
        /// Computes the final localScale that fits the item into the target XY size.
        /// Reads the item's current combined Renderer bounds (world space), computes
        /// a uniform multiplier so the bounds fit within targetSize on the X and Y axes,
        /// then multiplies by defaultScale.
        ///
        /// Assumption: items start at scale 1 so world-space bounds reflect the native
        /// mesh size. World-space bounds already incorporate the item's current scale,
        /// so the returned multiplier is applied on top of defaultScale.
        /// </summary>
        /// <param name="item">The item transform (reads Renderer bounds from it and children).</param>
        /// <param name="targetSizeXY">Target world-space size on X and Y axes (e.g. cellWorldSize * footprint).</param>
        /// <param name="defaultScale">The item's authored/default localScale to base the result on.</param>
        /// <param name="padding">Multiplier (0-1) to shrink the result so items don't touch cell edges. Default 0.9.</param>
        /// <returns>The final localScale vector (uniform). Returns defaultScale unchanged if no Renderer found.</returns>
        public static Vector3 ComputeFitScale(Transform item, Vector2 targetSizeXY, Vector3 defaultScale, float padding = 0.9f)
        {
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>();

            if (renderers == null || renderers.Length == 0)
            {
                return defaultScale;
            }

            // Temporarily reset to defaultScale so bounds measurement is
            // position/parent-independent — world-space bounds incorporate
            // lossy scale, which varies by cell position in a scaled grid.
            var savedScale = item.localScale;
            item.localScale = defaultScale;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            item.localScale = savedScale;

            float boundsX = bounds.size.x;
            float boundsY = bounds.size.y;

            // Guard against zero / degenerate bounds to avoid divide-by-zero or NaN.
            if (boundsX <= 0f || boundsY <= 0f)
            {
                return defaultScale;
            }

            // Uniform multiplier: use the min so the bounds fit on the tighter axis.
            float scaleX = targetSizeXY.x / boundsX;
            float scaleY = targetSizeXY.y / boundsY;
            float s = Mathf.Min(scaleX, scaleY);

            // Apply padding so items don't visually touch cell edges.
            s *= padding;

            // Multiply each axis by s so a non-uniform defaultScale is preserved proportionally.
            return new Vector3(defaultScale.x * s, defaultScale.y * s, defaultScale.z * s);
        }
    }
}

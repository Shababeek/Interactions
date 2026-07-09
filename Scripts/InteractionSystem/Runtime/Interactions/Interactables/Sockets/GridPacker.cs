using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Pure-C# multi-cell placement algorithm for a 2D inventory grid.
    /// Tracks cell occupancy and finds valid anchors for rectangular footprints.
    /// Not a MonoBehaviour — owned by InventoryGridSocket.
    /// </summary>
    public class GridPacker
    {
        private readonly bool[,] _occupied;

        public GridPacker(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);

            _occupied = new bool[Width, Height];
        }

        public int Width { get; }
        public int Height { get; }

        public int TotalCells => Width * Height;

        public int OccupiedCellCount
        {
            get
            {
                int count = 0;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (_occupied[x, y]) count++;
                    }
                }

                return count;
            }
        }

        public int FreeCellCount => TotalCells - OccupiedCellCount;

        public bool IsFull => FreeCellCount == 0;

        /// <summary>
        /// Does the footprint fit anywhere in the grid (bounds + free cells)?
        /// </summary>
        public bool FitsAnywhere(Vector2Int footprint)
        {
            if (!IsFootprintValid(footprint)) return false;

            // Early-out: footprint larger than grid in either dimension cannot fit.
            if (footprint.x > Width || footprint.y > Height) return false;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (CanFit(new Vector2Int(x, y), footprint)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the footprint fit at this specific anchor (top-left cell)?
        /// Checks: anchor in bounds, footprint extends within bounds, all cells free.
        /// Allocation-free — called per-cell every frame from the hover path.
        /// </summary>
        public bool CanFit(Vector2Int anchor, Vector2Int footprint)
        {
            if (!IsFootprintValid(footprint)) return false;

            // Bounds: anchor must be in-grid and the footprint must not overhang.
            if (anchor.x < 0 || anchor.y < 0) return false;
            if (anchor.x + footprint.x > Width || anchor.y + footprint.y > Height) return false;

            for (int dx = 0; dx < footprint.x; dx++)
            {
                for (int dy = 0; dy < footprint.y; dy++)
                {
                    if (_occupied[anchor.x + dx, anchor.y + dy]) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the list of cell coords the footprint covers at this anchor.
        /// Returns an empty list if any cell falls out of bounds.
        /// </summary>
        public List<Vector2Int> CellsFor(Vector2Int anchor, Vector2Int footprint)
        {
            var cells = new List<Vector2Int>();

            if (!IsFootprintValid(footprint)) return cells;

            for (int dx = 0; dx < footprint.x; dx++)
            {
                for (int dy = 0; dy < footprint.y; dy++)
                {
                    int cx = anchor.x + dx;
                    int cy = anchor.y + dy;

                    if (!IsInBounds(cx, cy)) return new List<Vector2Int>();

                    cells.Add(new Vector2Int(cx, cy));
                }
            }

            return cells;
        }

        /// <summary>
        /// Finds the nearest valid anchor to preferredCell (squared cell-distance).
        /// Returns null if no valid placement exists. preferredCell is clamped to
        /// grid bounds before the distance is computed.
        /// </summary>
        public Vector2Int? FindNearestValidAnchor(Vector2Int footprint, Vector2Int preferredCell)
        {
            if (!IsFootprintValid(footprint)) return null;
            if (footprint.x > Width || footprint.y > Height) return null;

            var clampedPreferred = new Vector2Int(
                Mathf.Clamp(preferredCell.x, 0, Width - 1),
                Mathf.Clamp(preferredCell.y, 0, Height - 1));

            Vector2Int? best = null;
            int bestDist = int.MaxValue;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var anchor = new Vector2Int(x, y);
                    if (!CanFit(anchor, footprint)) continue;

                    int dx = x - clampedPreferred.x;
                    int dy = y - clampedPreferred.y;
                    int dist = dx * dx + dy * dy;

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = anchor;
                    }
                }
            }

            return best;
        }

        /// <summary>
        /// Reserve cells for a footprint at an anchor.
        /// Returns false if overlap/out of bounds (non-throwing).
        /// </summary>
        public bool TryReserve(Vector2Int anchor, Vector2Int footprint)
        {
            if (!CanFit(anchor, footprint)) return false;

            for (int dx = 0; dx < footprint.x; dx++)
            {
                for (int dy = 0; dy < footprint.y; dy++)
                {
                    _occupied[anchor.x + dx, anchor.y + dy] = true;
                }
            }

            return true;
        }

        /// <summary>
        /// Release cells for a footprint at an anchor.
        /// Idempotent — releasing already-free cells is a no-op.
        /// </summary>
        public void Release(Vector2Int anchor, Vector2Int footprint)
        {
            if (!IsFootprintValid(footprint)) return;

            for (int dx = 0; dx < footprint.x; dx++)
            {
                for (int dy = 0; dy < footprint.y; dy++)
                {
                    int cx = anchor.x + dx;
                    int cy = anchor.y + dy;

                    if (IsInBounds(cx, cy))
                    {
                        _occupied[cx, cy] = false;
                    }
                }
            }
        }

        /// <summary>
        /// Is this specific cell occupied?
        /// </summary>
        public bool IsCellOccupied(int x, int y)
        {
            if (!IsInBounds(x, y)) return false;

            return _occupied[x, y];
        }

        /// <summary>
        /// Clear all occupancy.
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _occupied[x, y] = false;
                }
            }
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private static bool IsFootprintValid(Vector2Int footprint)
        {
            return footprint.x >= 1 && footprint.y >= 1;
        }
    }
}

using System.Collections.Generic;
using Shababeek.Interactions.Shababeek.Interactions;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// A multi-socket that arranges objects in a grid pattern.
    /// Perfect for organizing objects in rows and columns like inventory grids, shelves, or display cases.
    /// </summary>
    public class GridMultiSocket : AbstractSocket
    {
        [Header("Grid Settings")]
        [SerializeField] private Vector2Int gridSize = new Vector2Int(3, 3);
        [SerializeField] private Vector2 spacing = new Vector2(1f, 1f);
        [SerializeField] private LocalDirection gridPlane = LocalDirection.Forward;
        [Header("Positioning")]
        [SerializeField] private Vector3 localOffset = Vector3.zero;
        [SerializeField] private bool centerGrid = true;
        [Header("Pivot Settings")]
        [SerializeField] private Vector3 pivotRotationOffset = Vector3.zero;

        private readonly List<Transform> _gridPivots = new();
        private readonly Dictionary<Transform, int> _occupiedSlots = new();

        private void Awake()
        {
            CreateGridPivots();
        }

        private void CreateGridPivots()
        {
            _gridPivots.Clear();
            _occupiedSlots.Clear();

            for (int row = 0; row < gridSize.y; row++)
            {
                for (int col = 0; col < gridSize.x; col++)
                {
                    var gridIndex = row * gridSize.x + col;
                    var pivot = new GameObject($"GridPivot_{col}_{row}").transform;
                    pivot.SetParent(Pivot);
                    
                    var localPos = CalculateGridPosition(col, row);
                    pivot.localPosition = localPos + localOffset;
                    pivot.localRotation = Quaternion.Euler(pivotRotationOffset);
                    
                    _gridPivots.Add(pivot);
                }
            }
        }

        private Vector3 CalculateGridPosition(int col, int row)
        {
            // Calculate offset for centering
            var centerOffset = centerGrid 
                ? new Vector2((gridSize.x - 1) * spacing.x * 0.5f, (gridSize.y - 1) * spacing.y * 0.5f)
                : Vector2.zero;

            var gridPos = new Vector2(col * spacing.x - centerOffset.x, row * spacing.y - centerOffset.y);

            // Map to 3D space based on grid plane
            return gridPlane switch
            {
                LocalDirection.Forward => new Vector3(gridPos.x, gridPos.y, 0),
                LocalDirection.Back => new Vector3(-gridPos.x, gridPos.y, 0),
                LocalDirection.Right => new Vector3(0, gridPos.y, -gridPos.x),
                LocalDirection.Left => new Vector3(0, gridPos.y, gridPos.x),
                LocalDirection.Up => new Vector3(gridPos.x, 0, -gridPos.y),
                LocalDirection.Down => new Vector3(gridPos.x, 0, gridPos.y),
                _ => new Vector3(gridPos.x, gridPos.y, 0)
            };
        }

        public override (Vector3 position, Quaternion rotation) GetPivotForSocketable(Socketable socketable)
        {
            var closestPivot = FindClosestAvailableSlot(socketable.transform.position);
            if (closestPivot != null)
            {
                return (closestPivot.position, closestPivot.rotation);
            }
            
            // Fallback to center if no slots available
            return (transform.TransformPoint(localOffset), transform.rotation * Quaternion.Euler(pivotRotationOffset));
        }

        public override Transform Insert(Socketable socketable)
        {
            var closestPivot = FindClosestAvailableSlot(socketable.transform.position);
            if (closestPivot != null)
            {
                var slotIndex = _gridPivots.IndexOf(closestPivot);
                _occupiedSlots[closestPivot] = slotIndex;
                base.Insert(socketable);
                return closestPivot;
            }
            
            return null; // No available slots
        }

        public override void Remove(Socketable socketable)
        {
            var pivotTransform = socketable.transform.parent;
            if (pivotTransform != null && _occupiedSlots.ContainsKey(pivotTransform))
            {
                _occupiedSlots.Remove(pivotTransform);
            }
            
            base.Remove(socketable);
        }

        public override bool CanSocket()
        {
            return _occupiedSlots.Count < _gridPivots.Count;
        }

        private Transform FindClosestAvailableSlot(Vector3 worldPosition)
        {
            Transform closestPivot = null;
            float closestDistance = float.MaxValue;

            foreach (var pivot in _gridPivots)
            {
                if (_occupiedSlots.ContainsKey(pivot)) continue;

                var distance = Vector3.Distance(worldPosition, pivot.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPivot = pivot;
                }
            }

            return closestPivot;
        }

        private Vector3 GetPlaneNormal()
        {
            return gridPlane switch
            {
                LocalDirection.Forward => Vector3.forward,
                LocalDirection.Back => Vector3.back,
                LocalDirection.Right => Vector3.right,
                LocalDirection.Left => Vector3.left,
                LocalDirection.Up => Vector3.up,
                LocalDirection.Down => Vector3.down,
                _ => Vector3.forward
            };
        }

        private void OnValidate()
        {
            // Ensure reasonable grid size
            gridSize.x = Mathf.Max(1, gridSize.x);
            gridSize.y = Mathf.Max(1, gridSize.y);
            spacing.x = Mathf.Max(0.1f, spacing.x);
            spacing.y = Mathf.Max(0.1f, spacing.y);

            // Recreate grid if in play mode and values changed
            if (Application.isPlaying && _gridPivots.Count != gridSize.x * gridSize.y)
            {
                CreateGridPivots();
            }
        }

        private void OnDrawGizmos()
        {
            DrawGrid(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGrid(true);
        }

        private void DrawGrid(bool selected)
        {
            var oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            // Draw grid plane
            var planeNormal = GetPlaneNormal();
            var planeSize = new Vector2((gridSize.x - 1) * spacing.x + 1f, (gridSize.y - 1) * spacing.y + 1f);
            
            Gizmos.color = selected ? new Color(0f, 1f, 1f, 0.3f) : new Color(0f, 1f, 1f, 0.1f);
            
            var planeThickness = gridPlane switch
            {
                LocalDirection.Forward or LocalDirection.Back => new Vector3(planeSize.x, planeSize.y, 0.1f),
                LocalDirection.Right or LocalDirection.Left => new Vector3(0.1f, planeSize.y, planeSize.x),
                LocalDirection.Up or LocalDirection.Down => new Vector3(planeSize.x, 0.1f, planeSize.y),
                _ => new Vector3(planeSize.x, planeSize.y, 0.1f)
            };
            
            Gizmos.DrawCube(localOffset, planeThickness);

            // Draw grid points
            Gizmos.color = selected ? Color.cyan : new Color(0f, 1f, 1f, 0.7f);
            
            for (int row = 0; row < gridSize.y; row++)
            {
                for (int col = 0; col < gridSize.x; col++)
                {
                    var gridPos = CalculateGridPosition(col, row) + localOffset;
                    Gizmos.DrawWireSphere(gridPos, 0.1f);
                }
            }

            Gizmos.matrix = oldMatrix;
        }
    }
}
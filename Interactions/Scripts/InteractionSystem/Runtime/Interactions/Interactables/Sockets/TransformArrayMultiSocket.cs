using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// A multi-socket that uses manually placed transforms as socket points.
    /// Most flexible option - designers can place socket points exactly where needed.
    /// Perfect for irregular layouts, custom arrangements, or when precise control is required.
    /// </summary>
    public class TransformArrayMultiSocket : AbstractSocket
    {
        [Header("Socket Points")]
        [Tooltip("Array of transforms that define socket positions. Objects will snap to the closest available socket point.")]
        [SerializeField] private Transform[] socketPoints = new Transform[0];
        
        [Header("Settings")]
        [Tooltip("Whether to automatically select the closest available socket.")]
        [SerializeField] private bool useClosestSocket = true;
        [Tooltip("Maximum distance for socketable objects to snap to a socket point.")]
        [SerializeField] private float maxSocketDistance = 2f;
        
        [Header("Visual Feedback")]
        [Tooltip("Whether to show socket gizmos in the scene view.")]
        [SerializeField] private bool showSocketGizmos = true;
        [Tooltip("Color for available (empty) socket points.")]
        [SerializeField] private Color availableSocketColor = Color.green;
        [Tooltip("Color for occupied socket points.")]
        [SerializeField] private Color occupiedSocketColor = Color.red;
        [Tooltip("Color for the socket point currently being hovered.")]
        [SerializeField] private Color hoverSocketColor = Color.yellow;

        private readonly HashSet<Transform> _occupiedSockets = new();
        private Transform _currentHoverSocket;

        private void Awake()
        {
            ValidateSocketPoints();
        }

        private void ValidateSocketPoints()
        {
            // Remove null entries
            var validSockets = new List<Transform>();
            foreach (var socket in socketPoints)
            {
                if (socket != null)
                {
                    validSockets.Add(socket);
                }
            }
            socketPoints = validSockets.ToArray();
        }

        public override (Vector3 position, Quaternion rotation) GetPivotForSocketable(Socketable socketable)
        {
            var targetSocket = FindBestSocketForPosition(socketable.transform.position);
            if (targetSocket != null)
            {
                return (targetSocket.position, targetSocket.rotation);
            }
            
            // Fallback to main pivot if no suitable socket found
            return (Pivot.position, Pivot.rotation);
        }

        public override Transform Insert(Socketable socketable)
        {
            var targetSocket = FindBestSocketForPosition(socketable.transform.position);
            if (targetSocket != null)
            {
                _occupiedSockets.Add(targetSocket);
                base.Insert(socketable);
                return targetSocket;
            }
            
            return null; // No suitable socket found
        }

        public override void Remove(Socketable socketable)
        {
            var socketTransform = socketable.transform.parent;
            if (socketTransform != null && _occupiedSockets.Contains(socketTransform))
            {
                _occupiedSockets.Remove(socketTransform);
            }
            
            base.Remove(socketable);
        }

        public override bool CanSocket()
        {
            return _occupiedSockets.Count < socketPoints.Length;
        }

        public override void StartHovering(Socketable socketable)
        {
            _currentHoverSocket = FindBestSocketForPosition(socketable.transform.position);
            base.StartHovering(socketable);
        }

        public override void EndHovering(Socketable socketable)
        {
            _currentHoverSocket = null;
            base.EndHovering(socketable);
        }

        private Transform FindBestSocketForPosition(Vector3 worldPosition)
        {
            if (socketPoints.Length == 0) return null;

            Transform bestSocket = null;
            float closestDistance = float.MaxValue;

            foreach (var socket in socketPoints)
            {
                if (socket == null || _occupiedSockets.Contains(socket)) continue;

                var distance = Vector3.Distance(worldPosition, socket.position);
                
                // Check distance limit if using closest socket mode
                if (useClosestSocket && distance > maxSocketDistance) continue;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestSocket = socket;
                }
            }

            return bestSocket;
        }

        /// <summary>
        /// Gets the index of a specific socket point.
        /// </summary>
        /// <param name="socketTransform">The socket transform to find</param>
        /// <returns>Index of the socket, or -1 if not found</returns>
        public int GetSocketIndex(Transform socketTransform)
        {
            for (int i = 0; i < socketPoints.Length; i++)
            {
                if (socketPoints[i] == socketTransform)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Gets whether a specific socket is occupied.
        /// </summary>
        /// <param name="socketIndex">Index of the socket to check</param>
        /// <returns>True if the socket is occupied</returns>
        public bool IsSocketOccupied(int socketIndex)
        {
            if (socketIndex < 0 || socketIndex >= socketPoints.Length) return false;
            return _occupiedSockets.Contains(socketPoints[socketIndex]);
        }

        /// <summary>
        /// Gets the number of available sockets.
        /// </summary>
        /// <returns>Number of unoccupied sockets</returns>
        public int GetAvailableSocketCount()
        {
            return socketPoints.Length - _occupiedSockets.Count;
        }

        /// <summary>
        /// Forces a socketable into a specific socket slot.
        /// </summary>
        /// <param name="socketable">The socketable to place</param>
        /// <param name="socketIndex">Index of the socket to use</param>
        /// <returns>True if successful</returns>
        public bool ForceSocketToSlot(Socketable socketable, int socketIndex)
        {
            if (socketIndex < 0 || socketIndex >= socketPoints.Length) return false;
            if (_occupiedSockets.Contains(socketPoints[socketIndex])) return false;

            var targetSocket = socketPoints[socketIndex];
            _occupiedSockets.Add(targetSocket);
            
            // Position the socketable
            socketable.transform.SetParent(targetSocket);
            socketable.transform.localPosition = Vector3.zero;
            socketable.transform.localRotation = Quaternion.identity;
            
            return true;
        }

        private void OnValidate()
        {
            // Clean up null references
            if (socketPoints != null)
            {
                ValidateSocketPoints();
            }
            
            // Ensure reasonable limits
            maxSocketDistance = Mathf.Max(0.1f, maxSocketDistance);
        }

        private void OnDrawGizmos()
        {
            if (!showSocketGizmos) return;
            DrawSocketGizmos(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showSocketGizmos) return;
            DrawSocketGizmos(true);
        }

        private void DrawSocketGizmos(bool selected)
        {
            if (socketPoints == null) return;

            float gizmoSize = selected ? 0.2f : 0.15f;
            float alpha = selected ? 1f : 0.7f;

            for (int i = 0; i < socketPoints.Length; i++)
            {
                var socket = socketPoints[i];
                if (socket == null) continue;

                // Choose color based on state
                Color gizmoColor;
                if (_currentHoverSocket == socket)
                {
                    gizmoColor = hoverSocketColor;
                }
                else if (_occupiedSockets.Contains(socket))
                {
                    gizmoColor = occupiedSocketColor;
                }
                else
                {
                    gizmoColor = availableSocketColor;
                }

                gizmoColor.a = alpha;
                Gizmos.color = gizmoColor;

                // Draw socket point
                Gizmos.DrawSphere(socket.position, gizmoSize);
                
                // Draw connection to parent
                if (selected && socket.parent != null)
                {
                    Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
                    Gizmos.DrawLine(socket.position, socket.parent.position);
                }

                // Draw socket orientation
                if (selected)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(socket.position, socket.forward * 0.3f);
                }

                // Draw index label in editor
                #if UNITY_EDITOR
                if (selected)
                {
                    var labelPos = socket.position + Vector3.up * (gizmoSize + 0.1f);
                    UnityEditor.Handles.Label(labelPos, $"Socket {i}");
                }
                #endif
            }

            // Draw distance limit if using closest socket mode
            if (selected && useClosestSocket)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
                foreach (var socket in socketPoints)
                {
                    if (socket != null)
                    {
                        Gizmos.DrawWireSphere(socket.position, maxSocketDistance);
                    }
                }
            }
        }
    }
}
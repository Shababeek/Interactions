using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions
{
    using System.Collections.Generic;
    using UnityEngine;

    namespace Shababeek.Interactions
    {
        public enum LocalDirection
        {
            Forward = 0,
            Back = 1,
            Right = 2,
            Left = 3,
            Up = 4,
            Down = 5
        }

        public class MultiSocket : AbstractSocket
        {
            [Header("Positioning")] [SerializeField]
            private Vector3 localOffset = Vector3.zero;

            [SerializeField] private LocalDirection projectionDirection = LocalDirection.Forward;

            [Header("Pivot Settings")] [SerializeField]
            private Vector3 pivotRotationOffset = Vector3.zero;

            [SerializeField] private int initialPivotCount = 10;

            private readonly List<Transform> _availablePivots = new();
            private int _nextPivotIndex = 0;

            private void Awake()
            {
                _availablePivots.Clear();
                for (var i = 0; i < initialPivotCount; i++)
                {
                    CreateNewPivot();
                }
            }

            private void CreateNewPivot()
            {
                var parent = Pivot;
                var attachmentPoint = new GameObject($"Pivot{_nextPivotIndex}").transform;
                attachmentPoint.SetParent(parent);
                attachmentPoint.localPosition = Vector3.zero;
                attachmentPoint.localRotation = Quaternion.Euler(pivotRotationOffset);

                _availablePivots.Add(attachmentPoint);
                _nextPivotIndex++;
            }
            private Vector3 GetLocalNormal()
            {
                return projectionDirection switch
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
            

            public override Transform Insert(Socketable socketable)
            {
                var socket = GetSocket();
                var pivotInfo = GetPivotForSocketable(socketable);
                socket.position = pivotInfo.position;
                socket.rotation = pivotInfo.rotation;
                base.Insert(socketable);
                return socket;
            }

            public override void Remove(Socketable socketable)
            {
                // Return the pivot to the pool
                var pivotTransform = socketable.transform.parent;
                if (pivotTransform != null && pivotTransform != transform)
                {
                    pivotTransform.SetParent(Pivot);
                    _availablePivots.Add(pivotTransform);
                }

                base.Remove(socketable);
            }

            public override bool CanSocket()
            {
                return true; // Always can socket (will create more pivots if needed)
            }

            public override (Vector3 position, Quaternion rotation) GetPivotForSocketable(Socketable socketable)
            {
                
                var localProjectedPosition = localOffset + Vector3.ProjectOnPlane(transform.InverseTransformPoint(socketable.transform.position), GetLocalNormal());

                var worldPosition = transform.TransformPoint(localProjectedPosition);
                var worldRotation = transform.rotation * Quaternion.Euler(pivotRotationOffset);

                return (worldPosition, worldRotation);
            }

            private Transform GetSocket()
            {
                // If no available pivots, create a new one
                if (_availablePivots.Count == 0)
                {
                    CreateNewPivot();
                }

                var socket = _availablePivots[^1];
                _availablePivots.RemoveAt(_availablePivots.Count - 1);
                return socket;
            }

            private void OnDrawGizmos()
            {
                // Draw the local projection plane
                Gizmos.color = Color.yellow;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawRay(localOffset, GetLocalNormal() * 0.5f);
                
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                var size = projectionDirection switch
                {
                    LocalDirection.Forward or LocalDirection.Back => new Vector3(1, 1, 0.1f),
                    LocalDirection.Right or LocalDirection.Left => new Vector3(0.1f, 1, 1),
                    LocalDirection.Up or LocalDirection.Down => new Vector3(1, 0.1f, 1),
                    _ => new Vector3(1, 1, 0.1f)
                };
                Gizmos.DrawCube(localOffset, size);
            }
        }
    }
}
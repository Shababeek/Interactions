using System;
using UnityEngine;
using UnityEngine.Events;
using Shababeek.Interactions.Core;
using Shababeek.Utilities;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Drawer/slider interactable that moves linearly between two points.
    /// Provides smooth movement along a defined path with optional return-to-start behavior.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Interactables/DrawerInteractable")]
    public class DrawerInteractable : ConstrainedInteractableBase
    {
        [Header("Drawer/Slider Settings")]
        [Tooltip("Local space position where the drawer movement starts.")]
        [SerializeField] private Vector3 _localStart = Vector3.zero;

        [Tooltip("Local space position where the drawer movement ends.")]
        [SerializeField] private Vector3 _localEnd = Vector3.forward;
        [Tooltip("Whether the drawer should return to its starting position when deselected.")]
        [SerializeField] private bool returnToOriginal = false;
        [Tooltip("Speed at which the drawer returns to its starting position.")]
        [SerializeField] private float returnSpeed = 5f;

        [Header("Events")]
        [Tooltip("Event raised when the drawer is moved.")]
        [SerializeField] private UnityEvent onMoved;
        [Tooltip("Event raised when the drawer reaches either start or end limit.")]
        [SerializeField] private UnityEvent onLimitReached;
        [Tooltip("Event raised when the drawer's normalized position changes (0-1 range).")]
        [SerializeField] private FloatUnityEvent onValueChanged;

        [Header("Debug")]
        [Tooltip("Current normalized position of the drawer (0-1 range) (read-only).")]
        [ReadOnly] [SerializeField] private float currentValue = 0f;

        /// <summary>
        /// Gets or sets the local space starting position of the drawer.
        /// </summary>
        public Vector3 LocalStart
        {
            get => _localStart;
            set => _localStart = value;
        }

        /// <summary>
        /// Gets or sets the local space ending position of the drawer.
        /// </summary>
        public Vector3 LocalEnd
        {
            get => _localEnd;
            set => _localEnd = value;
        }

        /// <summary>
        /// Observable that fires when the drawer's normalized position changes.
        /// </summary>
        public IObservable<float> OnValueChanged => onValueChanged.AsObservable();

        // Private fields
        private Vector3 _lastPosition;
        private const float LimitEpsilon = 0.001f;
        private Vector3 _targetPosition;
        private bool _isReturning = false;
        private Vector3 _originalPosition;

        protected override void UseStarted()
        {
        }

        protected override void StartHover()
        {
        }

        protected override void EndHover()
        {
        }

        private void Start()
        {
            _originalPosition = interactableObject.transform.localPosition;
        }

        private void Reset()
        {
            AutoAssignInteractableObject();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoAssignInteractableObject();
        }
#endif

        private void AutoAssignInteractableObject()
        {
            if (interactableObject != null) return;
            if (transform.childCount > 0)
            {
                interactableObject = transform.GetChild(0);
            }
            else
            {
                var obj = new GameObject("InteractableObject");
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;
                interactableObject = obj.transform;
            }
        }

        protected override void HandleObjectMovement()
        {
            if (!IsSelected) return;
            
            var localInteractorPos = transform.InverseTransformPoint(CurrentInteractor.transform.position);
            var newLocalPos = GetPositionBetweenTwoPoints(localInteractorPos, _localStart, _localEnd);

            if (interactableObject.transform.localPosition != newLocalPos)
            {
                interactableObject.transform.localPosition = newLocalPos;
                UpdateValue(newLocalPos);
                onMoved?.Invoke();
            }

            // Check for limits
            if ((newLocalPos - _localStart).sqrMagnitude < LimitEpsilon * LimitEpsilon ||
                (newLocalPos - _localEnd).sqrMagnitude < LimitEpsilon * LimitEpsilon)
            {
                onLimitReached?.Invoke();
            }

            _isReturning = false;
        }

        protected override void HandleObjectDeselection()
        {
            if (returnToOriginal)
            {
                _isReturning = true;
            }
        }

        private void Update()
        {
            if (IsSelected)
            {
                HandleObjectMovement();
            }
            else if (returnToOriginal && _isReturning)
            {
                interactableObject.transform.localPosition = Vector3.Lerp(
                    interactableObject.transform.localPosition,
                    _originalPosition,
                    returnSpeed * Time.deltaTime
                );

                UpdateValue(interactableObject.transform.localPosition);

                // Stop returning when close enough to original position
                if (Vector3.Distance(interactableObject.transform.localPosition, _originalPosition) < 0.01f)
                {
                    _isReturning = false;
                }
            }
        }

        private void OnDrawGizmos()
        {
            var worldStart = transform.TransformPoint(_localStart);
            var worldEnd = transform.TransformPoint(_localEnd);
            var direction = worldEnd - worldStart;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldStart, .03f);
            Gizmos.DrawSphere(worldEnd, .03f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(worldStart, worldEnd);
            if (interactableObject != null)
            {
                var localObjPos = interactableObject.transform.localPosition;
                var projectedPoint = Vector3.Project(localObjPos - _localStart, _localEnd - _localStart) + _localStart;
                var worldProjected = transform.TransformPoint(projectedPoint);
                Gizmos.DrawSphere(worldProjected, .03f);
            }
        }

        private static Vector3 GetPositionBetweenTwoPoints(Vector3 point, Vector3 start, Vector3 end)
        {
            var direction = (end - start);
            var projectedPoint = Vector3.Project(point - start, direction) + start;
            var normalizedDistance = FindNormalizedDistanceAlongPath(direction, projectedPoint, start);
            return Vector3.Lerp(start, end, normalizedDistance);
        }

        private static float FindNormalizedDistanceAlongPath(Vector3 direction, Vector3 projectedPoint,
            Vector3 position1)
        {
            var axe = GetBiggestAxe(direction);
            var x = projectedPoint[axe];
            var m = 1 / direction[axe];
            var c = 0 - m * position1[axe];
            var t = m * x + c;
            return Mathf.Clamp01(t);
        }

        private static int GetBiggestAxe(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return Mathf.Abs(direction.x) > Mathf.Abs(direction.z) ? 0 : 2;
            }

            return Mathf.Abs(direction.y) > Mathf.Abs(direction.z) ? 1 : 2;
        }


        protected override void DeSelected()
        {
            base.DeSelected();
            HandleObjectDeselection();
        }

        private void UpdateValue(Vector3 position)
        {
            // Calculate normalized value (0-1) based on position
            float normalizedDistance = FindNormalizedDistanceAlongPath(_localEnd - _localStart, position, _localStart);
            currentValue = normalizedDistance;
            onValueChanged?.Invoke(currentValue);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Events;
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
        [Header("Drawer/Slider Settings")] [SerializeField]
        private Vector3 localStart = Vector3.zero;

        [SerializeField] private Vector3 localEnd = Vector3.forward;

        [SerializeField] private UnityEvent onOpened;
        [SerializeField] private UnityEvent onClosed;
        [SerializeField] private FloatUnityEvent onMoved;

        [Header("Debug")] [ReadOnly, SerializeField]
        private float currentValue = 0f;

        /// <summary>
        /// Gets or sets the local space starting position of the drawer.
        /// </summary>
        public Vector3 LocalStart
        {
            get => localStart;
            set => localStart = value;
        }

        /// <summary>
        /// Gets or sets the local space ending position of the drawer.
        /// </summary>
        public Vector3 LocalEnd
        {
            get => localEnd;
            set => localEnd = value;
        }

        /// <summary>
        /// Observable that fires when the drawer's normalized position changes.
        /// </summary>
        public IObservable<float> OnMoved => onMoved.AsObservable();

        /// <summary>
        /// Fires when The drawer is opened (position nears 1)
        /// </summary>
        public IObservable<Unit> OnOpened => onOpened.AsObservable();

        /// <summary>
        /// Fires when the drawer is closed (position nears 0)
        /// </summary>
        public IObservable<Unit> OnClosed => onClosed.AsObservable();

        private Vector3 _lastPosition;
        private const float LimitEpsilon = 0.05f;
        private Vector3 _targetPosition;
        private Vector3 _originalPosition;
        private float _returnTimer;
        private static float _normalizedDistance;


        private void Start()
        {
            _originalPosition = interactableObject.transform.localPosition;
        }

        protected override void Reset()
        {
            base.Reset();
            AutoAssignInteractableObject();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
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

        protected override void HandleObjectMovement(Vector3 target)
        {
            if (!IsSelected) return;

            var localInteractorPos = transform.InverseTransformPoint(target);
            var newLocalPos = GetPositionBetweenTwoPoints(localInteractorPos, localStart, localEnd);
            HandleEvents(newLocalPos);
        }

        private void HandleEvents(Vector3 newLocalPos)
        {
            if (interactableObject.transform.localPosition != newLocalPos)
            {
                interactableObject.transform.localPosition = newLocalPos;
                UpdateValue(newLocalPos);
                onMoved?.Invoke(_normalizedDistance);
            }

            if (_normalizedDistance >= 1 - LimitEpsilon)
            {
                onOpened?.Invoke();
            }
            else if (_normalizedDistance <= LimitEpsilon)
            {
                onClosed?.Invoke();
            }
        }

        protected override void HandleObjectDeselection()
        {
            _returnTimer = 0;
            if (returnWhenDeselected)
                return;
            switch (_normalizedDistance)
            {
                case >= 1 - LimitEpsilon:
                    HandleObjectMovement(transform.TransformPoint(localEnd));
                    break;
                case <= LimitEpsilon:
                    HandleObjectMovement(transform.TransformPoint(localStart));

                    break;
            }
        }


        protected override void HandleReturnToOriginalPosition()
        {
            _returnTimer += returnSpeed * Time.deltaTime;
            interactableObject.transform.localPosition = Vector3.Lerp(
                interactableObject.transform.localPosition,
                _originalPosition,
                _returnTimer
            );

            UpdateValue(interactableObject.transform.localPosition);

            if (Vector3.Distance(interactableObject.transform.localPosition, _originalPosition) < 0.001f)
            {
                IsReturning = false;
            }
        }

        private void OnDrawGizmos()
        {
            var worldStart = transform.TransformPoint(localStart);
            var worldEnd = transform.TransformPoint(localEnd);
            var direction = worldEnd - worldStart;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldStart, .03f);
            Gizmos.DrawSphere(worldEnd, .03f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(worldStart, worldEnd);
            if (interactableObject != null)
            {
                var localObjPos = interactableObject.transform.localPosition;
                var projectedPoint = Vector3.Project(localObjPos - localStart, localEnd - localStart) + localStart;
                var worldProjected = transform.TransformPoint(projectedPoint);
                Gizmos.DrawSphere(worldProjected, .03f);
            }
        }

        private static Vector3 GetPositionBetweenTwoPoints(Vector3 point, Vector3 start, Vector3 end)
        {
            var direction = (end - start);
            var projectedPoint = Vector3.Project(point - start, direction) + start;
            _normalizedDistance = Mathf.Clamp01(FindNormalizedDistanceAlongPath(direction, projectedPoint, start));
            return Vector3.Lerp(start, end, _normalizedDistance);
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
            float normalizedDistance = FindNormalizedDistanceAlongPath(localEnd - localStart, position, localStart);
            currentValue = normalizedDistance;
            onMoved?.Invoke(currentValue);
        }
    }
}
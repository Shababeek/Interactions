using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    public enum SpaceBoundsType
    {
        Rectangle,
        Circle
    }

    public enum SpaceMovementMode
    {
        Direct,
        Velocity,
        SmoothDamp
    }

    /// <summary>
    /// Maps a Vector2 input to object movement within bounded space (rectangle or circle).
    /// Useful for joystick-to-cursor or joystick-to-object mapping.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Vector2 Space Binder")]
    public class Vector2SpaceBinder : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private Vector2Variable inputVariable;

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 centerPosition;
        [SerializeField] private bool useLocalSpace = true;

        [Header("Bounds")]
        [SerializeField] private SpaceBoundsType boundsType = SpaceBoundsType.Rectangle;
        [SerializeField] private Vector2 rectangleSize = new Vector2(1f, 1f);
        [SerializeField] private float circleRadius = 0.5f;

        [Header("Mapping Plane")]
        [SerializeField] private Plane2D plane = Plane2D.XY;

        [Header("Movement")]
        [SerializeField] private SpaceMovementMode movementMode = SpaceMovementMode.Direct;
        [SerializeField] private float velocityMultiplier = 5f;
        [SerializeField] private float smoothTime = 0.1f;

        private CompositeDisposable _disposable;
        private Vector2 _currentInput;
        private Vector3 _targetPosition;
        private Vector3 _velocity;

        private void OnEnable()
        {
            if (target == null) target = transform;
            _disposable = new CompositeDisposable();

            if (inputVariable != null)
            {
                inputVariable.OnValueChanged
                    .Subscribe(OnInputChanged)
                    .AddTo(_disposable);
                _currentInput = inputVariable.Value;
            }

            _targetPosition = GetTargetPositionFromInput(_currentInput);
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnInputChanged(Vector2 input)
        {
            _currentInput = input;

            if (movementMode == SpaceMovementMode.Direct)
            {
                _targetPosition = GetTargetPositionFromInput(input);
                ApplyPosition(_targetPosition);
            }
        }

        private void Update()
        {
            if (movementMode == SpaceMovementMode.Direct) return;

            if (movementMode == SpaceMovementMode.Velocity)
            {
                // Input acts as velocity
                Vector3 delta = InputToWorldDirection(_currentInput) * velocityMultiplier * Time.deltaTime;
                _targetPosition += delta;
                _targetPosition = ClampToBoounds(_targetPosition);
                ApplyPosition(_targetPosition);
            }
            else if (movementMode == SpaceMovementMode.SmoothDamp)
            {
                // Smooth interpolation to target
                _targetPosition = GetTargetPositionFromInput(_currentInput);
                Vector3 currentPos = GetCurrentPosition();
                Vector3 newPos = Vector3.SmoothDamp(currentPos, _targetPosition, ref _velocity, smoothTime);
                ApplyPosition(newPos);
            }
        }

        private Vector3 GetTargetPositionFromInput(Vector2 input)
        {
            // Clamp input to bounds
            Vector2 clampedInput = ClampInputToBounds(input);

            // Map to world position
            return InputToWorldPosition(clampedInput);
        }

        private Vector2 ClampInputToBounds(Vector2 input)
        {
            if (boundsType == SpaceBoundsType.Circle)
            {
                if (input.magnitude > 1f)
                    return input.normalized;
                return input;
            }
            else
            {
                return new Vector2(
                    Mathf.Clamp(input.x, -1f, 1f),
                    Mathf.Clamp(input.y, -1f, 1f)
                );
            }
        }

        private Vector3 InputToWorldPosition(Vector2 input)
        {
            Vector3 offset;
            if (boundsType == SpaceBoundsType.Rectangle)
            {
                float x = input.x * rectangleSize.x * 0.5f;
                float y = input.y * rectangleSize.y * 0.5f;
                offset = PlaneToVector3(x, y);
            }
            else
            {
                float x = input.x * circleRadius;
                float y = input.y * circleRadius;
                offset = PlaneToVector3(x, y);
            }

            return centerPosition + offset;
        }

        private Vector3 InputToWorldDirection(Vector2 input)
        {
            return PlaneToVector3(input.x, input.y);
        }

        private Vector3 PlaneToVector3(float x, float y)
        {
            return plane switch
            {
                Plane2D.XY => new Vector3(x, y, 0),
                Plane2D.XZ => new Vector3(x, 0, y),
                Plane2D.YZ => new Vector3(0, x, y),
                _ => new Vector3(x, y, 0)
            };
        }

        private Vector3 ClampToBoounds(Vector3 position)
        {
            Vector3 offset = position - centerPosition;
            Vector2 offset2D = Vector3ToPlane(offset);

            if (boundsType == SpaceBoundsType.Circle)
            {
                if (offset2D.magnitude > circleRadius)
                    offset2D = offset2D.normalized * circleRadius;
            }
            else
            {
                offset2D.x = Mathf.Clamp(offset2D.x, -rectangleSize.x * 0.5f, rectangleSize.x * 0.5f);
                offset2D.y = Mathf.Clamp(offset2D.y, -rectangleSize.y * 0.5f, rectangleSize.y * 0.5f);
            }

            return centerPosition + PlaneToVector3(offset2D.x, offset2D.y);
        }

        private Vector2 Vector3ToPlane(Vector3 v)
        {
            return plane switch
            {
                Plane2D.XY => new Vector2(v.x, v.y),
                Plane2D.XZ => new Vector2(v.x, v.z),
                Plane2D.YZ => new Vector2(v.y, v.z),
                _ => new Vector2(v.x, v.y)
            };
        }

        private void ApplyPosition(Vector3 position)
        {
            if (useLocalSpace)
                target.localPosition = position;
            else
                target.position = position;
        }

        private Vector3 GetCurrentPosition()
        {
            return useLocalSpace ? target.localPosition : target.position;
        }

        public void SetCenter(Vector3 center)
        {
            centerPosition = center;
        }

        public void ResetToCenter()
        {
            ApplyPosition(centerPosition);
            _targetPosition = centerPosition;
            _velocity = Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = centerPosition;
            if (target != null && useLocalSpace && target.parent != null)
                center = target.parent.TransformPoint(centerPosition);

            Gizmos.color = Color.cyan;

            if (boundsType == SpaceBoundsType.Rectangle)
            {
                Vector3 size = PlaneToVector3(rectangleSize.x, rectangleSize.y);
                Gizmos.DrawWireCube(center, size);
            }
            else
            {
                DrawCircleGizmo(center, circleRadius);
            }
        }

        private void DrawCircleGizmo(Vector3 center, float radius)
        {
            int segments = 32;
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 p1 = center + PlaneToVector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius);
                Vector3 p2 = center + PlaneToVector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius);

                Gizmos.DrawLine(p1, p2);
            }
        }

        public enum Plane2D { XY, XZ, YZ }
    }
}

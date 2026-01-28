using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds any numeric variable to an object's movement speed between two positions.
    /// </summary>
    /// <remarks>
    /// Unlike NumericalPositionBinder which maps values directly to positions,
    /// this binder maps values to movement speed. A value of -1 moves toward start position,
    /// 0 stops movement, and 1 moves toward end position.
    ///
    /// Common use cases include:
    /// - Sliding doors (button held = door moves, released = stops)
    /// - Conveyor belts (speed control)
    /// - Elevator platforms (up/down input)
    /// - Throttle-controlled movement
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Numerical Position Speed Binder")]
    public class NumericalPositionSpeedBinder : MonoBehaviour
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Positions")]
        [Tooltip("The starting position (reached when moving at negative speed).")]
        [SerializeField] private Vector3 startPosition;

        [Tooltip("The ending position (reached when moving at positive speed).")]
        [SerializeField] private Vector3 endPosition;

        [Tooltip("Whether to use local position instead of world position.")]
        [SerializeField] private bool useLocalPosition = true;

        [Header("Value Mapping")]
        [Tooltip("The variable value that maps to maximum speed toward start position.")]
        [SerializeField] private float minValue = -1f;

        [Tooltip("The variable value that maps to maximum speed toward end position.")]
        [SerializeField] private float maxValue = 1f;

        [Header("Speed Settings")]
        [Tooltip("Maximum movement speed in units per second.")]
        [SerializeField] private float maxSpeed = 2f;

        [Tooltip("Values within this threshold from center will be treated as zero (no movement).")]
        [SerializeField] private float deadZone = 0.01f;

        [Header("Behavior")]
        [Tooltip("If true, object stops at start/end positions. If false, wraps around.")]
        [SerializeField] private bool clampToEndpoints = true;

        [Tooltip("Smoothly accelerate/decelerate instead of instant speed change.")]
        [SerializeField] private bool smoothAcceleration = false;

        [Tooltip("Acceleration rate when smoothAcceleration is enabled.")]
        [SerializeField] private float accelerationRate = 10f;

        private CompositeDisposable _disposable;
        private float _targetSpeed;
        private float _currentSpeed;
        private float _currentT; // 0 = start, 1 = end
        private INumericalVariable _numericalVariable;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (variable == null)
            {
                Debug.LogWarning($"Variable is not assigned on {gameObject.name}", this);
                return;
            }

            _numericalVariable = variable as INumericalVariable;
            if (_numericalVariable == null)
            {
                Debug.LogWarning($"Variable on {gameObject.name} is not a numerical variable (IntVariable or FloatVariable)", this);
                return;
            }

            // Initialize position
            _currentT = CalculateCurrentT();
            UpdateSpeed(_numericalVariable.AsFloat);

            // Subscribe to value changes
            variable.OnRaised
                .Subscribe(_ => UpdateSpeed(_numericalVariable.AsFloat))
                .AddTo(_disposable);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            // Handle smooth acceleration
            if (smoothAcceleration)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, accelerationRate * Time.deltaTime);
            }
            else
            {
                _currentSpeed = _targetSpeed;
            }

            if (Mathf.Approximately(_currentSpeed, 0f)) return;

            // Calculate distance between points
            float totalDistance = Vector3.Distance(startPosition, endPosition);
            if (Mathf.Approximately(totalDistance, 0f)) return;

            // Calculate delta T based on speed
            float deltaT = (_currentSpeed / totalDistance) * Time.deltaTime;
            float newT = _currentT + deltaT;

            // Apply clamping or wrapping
            if (clampToEndpoints)
            {
                newT = Mathf.Clamp01(newT);
            }
            else
            {
                // Wrap around
                newT = Mathf.Repeat(newT, 1f);
            }

            _currentT = newT;
            ApplyPosition();
        }

        private void UpdateSpeed(float value)
        {
            // Map input value to speed
            float center = (minValue + maxValue) / 2f;
            float range = (maxValue - minValue) / 2f;

            if (Mathf.Approximately(range, 0f))
            {
                _targetSpeed = 0f;
                return;
            }

            // Normalize to -1 to 1 range
            float normalizedValue = (value - center) / range;

            // Apply dead zone
            if (Mathf.Abs(normalizedValue) < deadZone)
            {
                _targetSpeed = 0f;
                return;
            }

            // Clamp and apply speed
            normalizedValue = Mathf.Clamp(normalizedValue, -1f, 1f);
            _targetSpeed = normalizedValue * maxSpeed;
        }

        private void ApplyPosition()
        {
            Vector3 position = Vector3.Lerp(startPosition, endPosition, _currentT);

            if (useLocalPosition)
                transform.localPosition = position;
            else
                transform.position = position;
        }

        private float CalculateCurrentT()
        {
            Vector3 currentPos = useLocalPosition ? transform.localPosition : transform.position;
            Vector3 toEnd = endPosition - startPosition;

            if (toEnd.sqrMagnitude < 0.0001f) return 0f;

            Vector3 toCurrent = currentPos - startPosition;
            return Mathf.Clamp01(Vector3.Dot(toCurrent, toEnd.normalized) / toEnd.magnitude);
        }

        #region Public API

        /// <summary>
        /// Gets the current movement speed in units per second.
        /// </summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>
        /// Gets the current position as a value between 0 (start) and 1 (end).
        /// </summary>
        public float CurrentT => _currentT;

        /// <summary>
        /// Gets the current world/local position.
        /// </summary>
        public Vector3 CurrentPosition => Vector3.Lerp(startPosition, endPosition, _currentT);

        /// <summary>
        /// Immediately sets the position to a specific T value (0-1).
        /// </summary>
        public void SetPositionImmediate(float t)
        {
            _currentT = clampToEndpoints ? Mathf.Clamp01(t) : Mathf.Repeat(t, 1f);
            ApplyPosition();
        }

        /// <summary>
        /// Moves to start position immediately.
        /// </summary>
        public void GoToStart() => SetPositionImmediate(0f);

        /// <summary>
        /// Moves to end position immediately.
        /// </summary>
        public void GoToEnd() => SetPositionImmediate(1f);

        /// <summary>
        /// Moves to center position immediately.
        /// </summary>
        public void GoToCenter() => SetPositionImmediate(0.5f);

        #endregion

        #region Editor Helpers

        /// <summary>Sets start position to current transform position.</summary>
        [ContextMenu("Set Start Position")]
        public void SetStartPosition()
        {
            startPosition = useLocalPosition ? transform.localPosition : transform.position;
        }

        /// <summary>Sets end position to current transform position.</summary>
        [ContextMenu("Set End Position")]
        public void SetEndPosition()
        {
            endPosition = useLocalPosition ? transform.localPosition : transform.position;
        }

        /// <summary>Preview start position in editor.</summary>
        [ContextMenu("Preview Start")]
        public void PreviewStart()
        {
            if (useLocalPosition) transform.localPosition = startPosition;
            else transform.position = startPosition;
        }

        /// <summary>Preview end position in editor.</summary>
        [ContextMenu("Preview End")]
        public void PreviewEnd()
        {
            if (useLocalPosition) transform.localPosition = endPosition;
            else transform.position = endPosition;
        }

        #endregion
    }
}

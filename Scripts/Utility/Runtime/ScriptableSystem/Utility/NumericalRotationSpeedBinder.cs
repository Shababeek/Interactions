using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds any numeric variable (IntVariable or FloatVariable) to an object's rotation speed.
    /// </summary>
    /// <remarks>
    /// Unlike NumericalRotationBinder which maps values directly to rotation angles,
    /// this binder maps values to rotation speed. A value of -1 rotates at negative max speed,
    /// 0 stops rotation, and 1 rotates at positive max speed.
    ///
    /// Common use cases include:
    /// - Steering wheels (joystick input controls turn rate)
    /// - Propellers/fans (throttle controls spin speed)
    /// - Turrets (input controls rotation speed with optional angle limits)
    /// - Continuous rotation objects
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Numerical Rotation Speed Binder")]
    public class NumericalRotationSpeedBinder : MonoBehaviour
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Value Mapping")]
        [Tooltip("The variable value that maps to maximum negative rotation speed.")]
        [SerializeField] private float minValue = -1f;

        [Tooltip("The variable value that maps to maximum positive rotation speed.")]
        [SerializeField] private float maxValue = 1f;

        [Header("Rotation Speed Settings")]
        [Tooltip("The axis to rotate around.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;

        [Tooltip("Maximum rotation speed in degrees per second (at maxValue).")]
        [SerializeField] private float maxRotationSpeed = 180f;

        [Tooltip("Whether to use local rotation instead of world rotation.")]
        [SerializeField] private bool useLocalRotation = true;

        [Header("Angle Limits (Optional)")]
        [Tooltip("Whether to constrain rotation within angle limits.")]
        [SerializeField] private bool useAngleLimits = false;

        [Tooltip("Minimum angle limit in degrees (only used if useAngleLimits is true).")]
        [SerializeField] private float minAngle = -180f;

        [Tooltip("Maximum angle limit in degrees (only used if useAngleLimits is true).")]
        [SerializeField] private float maxAngle = 180f;

        [Header("Dead Zone")]
        [Tooltip("Values within this threshold from center (0) will be treated as zero (no rotation).")]
        [SerializeField] private float deadZone = 0.01f;

        private CompositeDisposable _disposable;
        private float _currentSpeed;
        private float _currentAngle;
        private INumericalVariable _numericalVariable;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (variable == null)
            {
                Debug.LogWarning($"Variable is not assigned on {gameObject.name}", this);
                return;
            }

            // Check if it's a numerical variable
            _numericalVariable = variable as INumericalVariable;
            if (_numericalVariable == null)
            {
                Debug.LogWarning($"Variable on {gameObject.name} is not a numerical variable (IntVariable or FloatVariable)", this);
                return;
            }

            // Initialize current angle from transform
            _currentAngle = GetCurrentRotation();

            // Set initial speed from variable
            UpdateSpeed(_numericalVariable.AsFloat);

            // Subscribe to value changes
            variable.OnRaised
                .Subscribe(_ => UpdateSpeed(_numericalVariable.AsFloat))
                .AddTo(_disposable);
        }

        private void Update()
        {
            if (Mathf.Approximately(_currentSpeed, 0f)) return;

            // Calculate new angle
            float newAngle = _currentAngle + _currentSpeed * Time.deltaTime;

            // Apply angle limits if enabled
            if (useAngleLimits)
            {
                newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);
            }

            _currentAngle = newAngle;
            ApplyRotation(_currentAngle);
        }

        private void UpdateSpeed(float value)
        {
            // Map input value to speed
            // Center value ((minValue + maxValue) / 2) = 0 speed
            // maxValue = +maxRotationSpeed
            // minValue = -maxRotationSpeed

            float center = (minValue + maxValue) / 2f;
            float range = (maxValue - minValue) / 2f;

            if (Mathf.Approximately(range, 0f))
            {
                _currentSpeed = 0f;
                return;
            }

            // Normalize to -1 to 1 range
            float normalizedValue = (value - center) / range;

            // Apply dead zone
            if (Mathf.Abs(normalizedValue) < deadZone)
            {
                _currentSpeed = 0f;
                return;
            }

            // Clamp to valid range and apply speed
            normalizedValue = Mathf.Clamp(normalizedValue, -1f, 1f);
            _currentSpeed = normalizedValue * maxRotationSpeed;
        }

        private void ApplyRotation(float angle)
        {
            Vector3 eulerAngles = GetCurrentRotationEuler();

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    eulerAngles.x = angle;
                    break;
                case RotationAxis.Y:
                    eulerAngles.y = angle;
                    break;
                case RotationAxis.Z:
                    eulerAngles.z = angle;
                    break;
            }

            if (useLocalRotation)
            {
                transform.localRotation = Quaternion.Euler(eulerAngles);
            }
            else
            {
                transform.rotation = Quaternion.Euler(eulerAngles);
            }
        }

        private float GetCurrentRotation()
        {
            Vector3 eulerAngles = GetCurrentRotationEuler();

            return rotationAxis switch
            {
                RotationAxis.X => eulerAngles.x,
                RotationAxis.Y => eulerAngles.y,
                RotationAxis.Z => eulerAngles.z,
                _ => 0f
            };
        }

        private Vector3 GetCurrentRotationEuler()
        {
            return useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        /// <summary>
        /// Gets the current rotation speed in degrees per second.
        /// </summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>
        /// Gets the current rotation angle.
        /// </summary>
        public float CurrentAngle => _currentAngle;

        /// <summary>
        /// Sets the rotation angle immediately.
        /// </summary>
        /// <param name="angle">The angle in degrees</param>
        public void SetAngleImmediate(float angle)
        {
            if (useAngleLimits)
            {
                angle = Mathf.Clamp(angle, minAngle, maxAngle);
            }
            _currentAngle = angle;
            ApplyRotation(angle);
        }

        /// <summary>
        /// Resets the rotation to the center of the angle limits (or 0 if no limits).
        /// </summary>
        public void ResetRotation()
        {
            float resetAngle = useAngleLimits ? (minAngle + maxAngle) / 2f : 0f;
            SetAngleImmediate(resetAngle);
        }

        /// <summary>
        /// Defines which axis to rotate around.
        /// </summary>
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }
    }
}

using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    [AddComponentMenu("Shababeek/Scriptable System/Numerical Rotation Speed Binder")]
    public class NumericalRotationSpeedBinder : MonoBehaviour
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Value Mapping")]
        [SerializeField] private float minValue = -1f;
        [SerializeField] private float maxValue = 1f;

        [Header("Rotation Speed Settings")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;
        [SerializeField] private float maxRotationSpeed = 180f;
        [SerializeField] private bool useLocalRotation = true;

        [Header("Angle Limits")]
        [SerializeField] private bool useAngleLimits = false;
        [SerializeField] private float minAngle = -90f;
        [SerializeField] private float maxAngle = 90f;

        [Header("Dead Zone")]
        [SerializeField] private float deadZone = 0.01f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private CompositeDisposable _disposable;
        private INumericalVariable _numericalVariable;

        private Vector3 _trackedEulerAngles;
        private float _currentAngle;
        private float _currentSpeed;

        #region Unity Lifecycle

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (!TryResolveVariable())
                return;

            InitializeRotation();
            UpdateSpeed(_numericalVariable.AsFloat);

            variable.OnRaised
                .Subscribe(_ => UpdateSpeed(_numericalVariable.AsFloat))
                .AddTo(_disposable);
        }

        private void Update()
        {
            if (Mathf.Approximately(_currentSpeed, 0f))
                return;

            float deltaAngle = _currentSpeed * Time.deltaTime;
            float newAngle = _currentAngle + deltaAngle;

            if (useAngleLimits)
            {
                // HARD STOP at limits
                if (newAngle >= maxAngle && _currentSpeed > 0f)
                {
                    newAngle = maxAngle;
                    _currentSpeed = 0f;
                }
                else if (newAngle <= minAngle && _currentSpeed < 0f)
                {
                    newAngle = minAngle;
                    _currentSpeed = 0f;
                }
            }

            _currentAngle = newAngle;
            SetAxisAngle(_currentAngle);
            ApplyRotation();

            if (enableDebugLogs)
            {
                Debug.Log($"[ROT] Angle: {_currentAngle:F2}, Speed: {_currentSpeed:F2}");
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        #endregion

        #region Initialization

        private bool TryResolveVariable()
        {
            if (variable == null)
            {
                Debug.LogWarning($"Variable not assigned on {name}", this);
                return false;
            }

            _numericalVariable = variable as INumericalVariable;
            if (_numericalVariable == null)
            {
                Debug.LogWarning($"Variable on {name} is not numerical", this);
                return false;
            }

            return true;
        }

        private void InitializeRotation()
        {
            Vector3 rawEuler = useLocalRotation
                ? transform.localEulerAngles
                : transform.eulerAngles;

            _trackedEulerAngles = new Vector3(
                NormalizeAngle(rawEuler.x),
                NormalizeAngle(rawEuler.y),
                NormalizeAngle(rawEuler.z)
            );

            _currentAngle = GetAxisAngle();

            if (useAngleLimits)
                _currentAngle = Mathf.Clamp(_currentAngle, minAngle, maxAngle);

            SetAxisAngle(_currentAngle);
            ApplyRotation();
        }

        #endregion

        #region Speed Logic

        private void UpdateSpeed(float value)
        {
            float center = (minValue + maxValue) * 0.5f;
            float range = (maxValue - minValue) * 0.5f;

            if (Mathf.Approximately(range, 0f))
            {
                _currentSpeed = 0f;
                return;
            }

            float normalized = (value - center) / range;

            if (Mathf.Abs(normalized) < deadZone)
            {
                _currentSpeed = 0f;
                return;
            }

            normalized = Mathf.Clamp(normalized, -1f, 1f);
            _currentSpeed = normalized * maxRotationSpeed;

            // Prevent speed from pushing past limits
            if (useAngleLimits)
            {
                if (_currentAngle >= maxAngle && _currentSpeed > 0f)
                    _currentSpeed = 0f;
                else if (_currentAngle <= minAngle && _currentSpeed < 0f)
                    _currentSpeed = 0f;
            }
        }

        #endregion

        #region Rotation Helpers

        private void ApplyRotation()
        {
            Quaternion rotation = Quaternion.Euler(_trackedEulerAngles);

            if (useLocalRotation)
                transform.localRotation = rotation;
            else
                transform.rotation = rotation;
        }

        private float GetAxisAngle()
        {
            return rotationAxis switch
            {
                RotationAxis.X => _trackedEulerAngles.x,
                RotationAxis.Y => _trackedEulerAngles.y,
                RotationAxis.Z => _trackedEulerAngles.z,
                _ => 0f
            };
        }

        private void SetAxisAngle(float angle)
        {
            switch (rotationAxis)
            {
                case RotationAxis.X:
                    _trackedEulerAngles.x = angle;
                    break;
                case RotationAxis.Y:
                    _trackedEulerAngles.y = angle;
                    break;
                case RotationAxis.Z:
                    _trackedEulerAngles.z = angle;
                    break;
            }
        }

        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }

        #endregion

        #region Public API

        public float CurrentAngle => _currentAngle;
        public float CurrentSpeed => _currentSpeed;

        public void SetAngleImmediate(float angle)
        {
            if (useAngleLimits)
                angle = Mathf.Clamp(angle, minAngle, maxAngle);

            _currentAngle = angle;
            SetAxisAngle(angle);
            ApplyRotation();
        }

        public void ResetRotation()
        {
            SetAngleImmediate(useAngleLimits ? (minAngle + maxAngle) * 0.5f : 0f);
        }

        #endregion

        public enum RotationAxis
        {
            X,
            Y,
            Z
        }
    }
}
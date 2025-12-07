using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds an IntVariable to an object's rotation for live updates.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Int Variable Rotation Binder")]
    public class IntVariableRotationBinder : MonoBehaviour
    {
        [Tooltip("The IntVariable to bind to the rotation.")]
        [SerializeField] private IntVariable intVariable;
        
        [Header("Rotation Settings")]
        [Tooltip("The axis to rotate around.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;
        
        [Tooltip("The minimum rotation angle in degrees.")]
        [SerializeField] private float minAngle = 0f;
        
        [Tooltip("The maximum rotation angle in degrees.")]
        [SerializeField] private float maxAngle = 360f;
        
        [Tooltip("The minimum value from the IntVariable that maps to minAngle.")]
        [SerializeField] private int minValue = 0;
        
        [Tooltip("The maximum value from the IntVariable that maps to maxAngle.")]
        [SerializeField] private int maxValue = 100;
        
        [Tooltip("Whether to use local rotation instead of world rotation.")]
        [SerializeField] private bool useLocalRotation = true;
        
        [Tooltip("Whether to smoothly interpolate rotation changes.")]
        [SerializeField] private bool smoothRotation = false;
        
        [Tooltip("Rotation speed for smooth interpolation (degrees per second).")]
        [SerializeField] private float rotationSpeed = 90f;
        
        private CompositeDisposable _disposable;
        private float _targetAngle;
        private float _currentAngle;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            
            if (intVariable == null)
            {
                Debug.LogWarning($"IntVariable is not assigned on {gameObject.name}", this);
                return;
            }

            // Initialize rotation
            UpdateRotation(intVariable.Value);
            
            // Subscribe to value changes
            intVariable.OnValueChanged
                .Subscribe(value => UpdateRotation(value))
                .AddTo(_disposable);
        }

        private void Update()
        {
            if (smoothRotation)
            {
                _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, rotationSpeed * Time.deltaTime);
                ApplyRotation(_currentAngle);
            }
        }

        private void UpdateRotation(int value)
        {
            // Clamp value to range
            int clampedValue = Mathf.Clamp(value, minValue, maxValue);
            
            // Map value to angle
            float t = maxValue != minValue ? (float)(clampedValue - minValue) / (maxValue - minValue) : 0f;
            _targetAngle = Mathf.Lerp(minAngle, maxAngle, t);
            
            if (!smoothRotation)
            {
                ApplyRotation(_targetAngle);
                _currentAngle = _targetAngle;
            }
            else
            {
                // Initialize current angle if not set
                if (_currentAngle == 0f && _targetAngle != 0f)
                {
                    _currentAngle = GetCurrentRotation();
                }
            }
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

        private enum RotationAxis
        {
            X,
            Y,
            Z
        }
    }
}


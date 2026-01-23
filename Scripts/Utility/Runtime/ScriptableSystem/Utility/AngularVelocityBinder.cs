using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    public enum AngularInputMode
    {
        Vector3,
        Vector2XY,
        Vector2XZ,
        FloatSingleAxis
    }

    /// <summary>
    /// Binds variables to Rigidbody angular velocity or applies as torque.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Angular Velocity Binder")]
    public class AngularVelocityBinder : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Rigidbody rb3D;
        [SerializeField] private Rigidbody2D rb2D;

        [Header("Input Mode")]
        [SerializeField] private AngularInputMode inputMode = AngularInputMode.Vector3;

        [Header("Vector3 Input")]
        [SerializeField] private Vector3Variable vector3Input;

        [Header("Vector2 Input")]
        [SerializeField] private Vector2Variable vector2Input;

        [Header("Float Input")]
        [SerializeField] private FloatVariable floatInput;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [Header("Application")]
        [SerializeField] private bool setVelocity = true;
        [SerializeField] private bool useLocalSpace = false;
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private bool continuous = true;

        private CompositeDisposable _disposable;
        private Vector3 _currentAngularVelocity;

        private void OnEnable()
        {
            if (rb3D == null) rb3D = GetComponent<Rigidbody>();
            if (rb2D == null) rb2D = GetComponent<Rigidbody2D>();
            _disposable = new CompositeDisposable();
            SubscribeToInputs();
        }

        private void OnDisable() => _disposable?.Dispose();

        private void SubscribeToInputs()
        {
            switch (inputMode)
            {
                case AngularInputMode.Vector3:
                    if (vector3Input != null)
                    {
                        vector3Input.OnValueChanged
                            .Subscribe(v => _currentAngularVelocity = v)
                            .AddTo(_disposable);
                        _currentAngularVelocity = vector3Input.Value;
                    }
                    break;

                case AngularInputMode.Vector2XY:
                    if (vector2Input != null)
                    {
                        vector2Input.OnValueChanged
                            .Subscribe(v => _currentAngularVelocity = new Vector3(v.x, v.y, 0))
                            .AddTo(_disposable);
                        var val = vector2Input.Value;
                        _currentAngularVelocity = new Vector3(val.x, val.y, 0);
                    }
                    break;

                case AngularInputMode.Vector2XZ:
                    if (vector2Input != null)
                    {
                        vector2Input.OnValueChanged
                            .Subscribe(v => _currentAngularVelocity = new Vector3(v.x, 0, v.y))
                            .AddTo(_disposable);
                        var v2 = vector2Input.Value;
                        _currentAngularVelocity = new Vector3(v2.x, 0, v2.y);
                    }
                    break;

                case AngularInputMode.FloatSingleAxis:
                    if (floatInput != null)
                    {
                        floatInput.OnValueChanged
                            .Subscribe(f => _currentAngularVelocity = rotationAxis.normalized * f)
                            .AddTo(_disposable);
                        _currentAngularVelocity = rotationAxis.normalized * floatInput.Value;
                    }
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (!continuous) return;
            Apply();
        }

        public void Apply()
        {
            Vector3 angularVel = _currentAngularVelocity * multiplier;
            if (useLocalSpace && rb3D != null)
            {
                angularVel = transform.TransformDirection(angularVel);
            }

            if (rb3D != null)
            {
                if (setVelocity)
                    rb3D.angularVelocity = angularVel;
                else
                    rb3D.AddTorque(angularVel, ForceMode.Force);
            }
            else if (rb2D != null)
            {
                // 2D only uses Z axis rotation
                float z = inputMode == AngularInputMode.FloatSingleAxis
                    ? _currentAngularVelocity.magnitude * Mathf.Sign(rotationAxis.z)
                    : _currentAngularVelocity.z;

                if (setVelocity)
                    rb2D.angularVelocity = z * multiplier;
                else
                    rb2D.AddTorque(z * multiplier, ForceMode2D.Force);
            }
        }
    }
}

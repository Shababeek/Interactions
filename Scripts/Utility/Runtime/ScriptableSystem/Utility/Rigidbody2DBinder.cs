using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    public enum Velocity2DInputMode
    {
        Vector2,
        TwoFloats,
        FloatDirection
    }

    public enum Velocity2DApplicationMode
    {
        SetVelocity,
        AddForce,
        AddForceImpulse
    }

    /// <summary>
    /// Binds variables to Rigidbody2D velocity/forces.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Rigidbody 2D Binder")]
    public class Rigidbody2DBinder : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Rigidbody2D rb;

        [Header("Input Mode")]
        [SerializeField] private Velocity2DInputMode inputMode = Velocity2DInputMode.Vector2;

        [Header("Vector2 Input")]
        [SerializeField] private Vector2Variable vector2Input;

        [Header("Float Inputs (for TwoFloats mode)")]
        [SerializeField] private FloatVariable floatX;
        [SerializeField] private FloatVariable floatY;

        [Header("Direction Input (for FloatDirection mode)")]
        [SerializeField] private FloatVariable floatMagnitude;
        [SerializeField] private Vector2 direction = Vector2.right;

        [Header("Application")]
        [SerializeField] private Velocity2DApplicationMode applicationMode = Velocity2DApplicationMode.SetVelocity;
        [SerializeField] private bool useLocalSpace = false;
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private bool continuous = true;

        private CompositeDisposable _disposable;
        private Vector2 _currentVelocity;

        private void OnEnable()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            _disposable = new CompositeDisposable();
            SubscribeToInputs();
        }

        private void OnDisable() => _disposable?.Dispose();

        private void SubscribeToInputs()
        {
            switch (inputMode)
            {
                case Velocity2DInputMode.Vector2:
                    if (vector2Input != null)
                    {
                        vector2Input.OnValueChanged
                            .Subscribe(v => _currentVelocity = v)
                            .AddTo(_disposable);
                        _currentVelocity = vector2Input.Value;
                    }
                    break;

                case Velocity2DInputMode.TwoFloats:
                    if (floatX != null)
                    {
                        floatX.OnValueChanged
                            .Subscribe(_ => UpdateFromFloats())
                            .AddTo(_disposable);
                    }
                    if (floatY != null)
                    {
                        floatY.OnValueChanged
                            .Subscribe(_ => UpdateFromFloats())
                            .AddTo(_disposable);
                    }
                    UpdateFromFloats();
                    break;

                case Velocity2DInputMode.FloatDirection:
                    if (floatMagnitude != null)
                    {
                        floatMagnitude.OnValueChanged
                            .Subscribe(f => _currentVelocity = direction.normalized * f)
                            .AddTo(_disposable);
                        _currentVelocity = direction.normalized * floatMagnitude.Value;
                    }
                    break;
            }
        }

        private void UpdateFromFloats()
        {
            float x = floatX != null ? floatX.Value : 0f;
            float y = floatY != null ? floatY.Value : 0f;
            _currentVelocity = new Vector2(x, y);
        }

        private void FixedUpdate()
        {
            if (!continuous || rb == null) return;
            Apply();
        }

        public void Apply()
        {
            if (rb == null) return;

            Vector2 velocity = _currentVelocity * multiplier;
            if (useLocalSpace)
            {
                velocity = transform.TransformDirection(velocity);
            }

            switch (applicationMode)
            {
                case Velocity2DApplicationMode.SetVelocity:
                    rb.linearVelocity = velocity;
                    break;
                case Velocity2DApplicationMode.AddForce:
                    rb.AddForce(velocity, ForceMode2D.Force);
                    break;
                case Velocity2DApplicationMode.AddForceImpulse:
                    rb.AddForce(velocity, ForceMode2D.Impulse);
                    break;
            }
        }

        public void SetVelocity(Vector2 velocity)
        {
            _currentVelocity = velocity;
            if (!continuous) Apply();
        }
    }
}

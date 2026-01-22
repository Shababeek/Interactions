using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds Vector2/Vector3 variables to Rigidbody velocity or applies as acceleration.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Rigidbody Velocity Binder")]
    public class RigidbodyVelocityBinder : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private PhysicsMode physicsMode = PhysicsMode.Rigidbody3D;
        [SerializeField] private ApplicationMode applicationMode = ApplicationMode.Velocity;

        [Header("3D Settings")]
        [SerializeField] private Rigidbody rb3D;
        [SerializeField] private Vector3Variable vector3Variable;

        [Header("2D Settings")]
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private Vector2Variable vector2Variable;
        [SerializeField] private Plane2D plane = Plane2D.XY;

        [Header("Options")]
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private bool continuous = true;

        private CompositeDisposable _disposable;
        private Vector3 _currentValue3D;
        private Vector2 _currentValue2D;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (physicsMode == PhysicsMode.Rigidbody3D)
            {
                if (rb3D == null) rb3D = GetComponent<Rigidbody>();
                if (vector3Variable != null)
                {
                    _currentValue3D = vector3Variable.Value;
                    vector3Variable.OnValueChanged.Subscribe(v => _currentValue3D = v).AddTo(_disposable);
                }
            }
            else
            {
                if (rb2D == null) rb2D = GetComponent<Rigidbody2D>();
                if (vector2Variable != null)
                {
                    _currentValue2D = vector2Variable.Value;
                    vector2Variable.OnValueChanged.Subscribe(v => _currentValue2D = v).AddTo(_disposable);
                }
            }
        }

        private void OnDisable() => _disposable?.Dispose();

        private void FixedUpdate()
        {
            if (!continuous) return;
            Apply();
        }

        /// <summary>Manually apply the current value once.</summary>
        public void Apply()
        {
            if (physicsMode == PhysicsMode.Rigidbody3D)
                Apply3D();
            else
                Apply2D();
        }

        private void Apply3D()
        {
            if (rb3D == null) return;
            var value = _currentValue3D * multiplier;

            if (applicationMode == ApplicationMode.Velocity)
                rb3D.linearVelocity = value;
            else
                rb3D.AddForce(value, ForceMode.Acceleration);
        }

        private void Apply2D()
        {
            if (rb2D == null) return;
            var value = _currentValue2D * multiplier;

            if (applicationMode == ApplicationMode.Velocity)
            {
                rb2D.linearVelocity = value;
            }
            else
            {
                rb2D.AddForce(value, ForceMode2D.Force);
            }
        }

        /// <summary>Convert Vector3 to the appropriate 2D plane.</summary>
        public Vector2 ConvertToPlane(Vector3 v)
        {
            return plane switch
            {
                Plane2D.XY => new Vector2(v.x, v.y),
                Plane2D.XZ => new Vector2(v.x, v.z),
                Plane2D.YZ => new Vector2(v.y, v.z),
                _ => new Vector2(v.x, v.y)
            };
        }

        public enum PhysicsMode { Rigidbody3D, Rigidbody2D }
        public enum ApplicationMode { Velocity, Acceleration }
        public enum Plane2D { XY, XZ, YZ }
    }
}

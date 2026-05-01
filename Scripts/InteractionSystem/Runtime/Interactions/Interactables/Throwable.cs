using System;
using UnityEngine;
using Shababeek.ReactiveVars;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Velocity tracker that records linear and angular velocity samples while a Grabable
    /// is held, then applies the averaged velocity to its Rigidbody on release.
    /// Used as a serialized sub-feature of Grabable rather than a standalone component.
    /// </summary>
    [Serializable]
    public class Throwable
    {
        [Tooltip("Number of velocity samples to average for the throw. Higher values smooth the throw at the cost of latency.")]
        [SerializeField, Min(1)] private int velocitySampleCount = 10;

        [Tooltip("Multiplier applied to the calculated linear throw velocity. > 1 throws harder, < 1 softer.")]
        [SerializeField] private float throwMultiplier = 1f;

        [Tooltip("Whether to apply angular velocity (spin) to the thrown object on release.")]
        [SerializeField] private bool enableAngularVelocity = true;

        [Tooltip("Multiplier applied to the calculated angular velocity.")]
        [SerializeField] private float angularVelocityMultiplier = 1f;

        [Tooltip("Event invoked when the object is thrown, passing the final throw velocity.")]
        [SerializeField] private Vector3UnityEvent onThrowEnd = new();

        private Rigidbody _body;
        private Transform _transform;
        private Vector3[] _velocitySamples;
        private Vector3[] _angularVelocitySamples;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private int _index;
        private int _count;
        private bool _tracking;

        /// <summary>Event invoked when the object is thrown, passing the final throw velocity.</summary>
        public Vector3UnityEvent OnThrowEnd => onThrowEnd;

        /// <summary>Begins tracking velocity samples for the given body / transform.</summary>
        public void StartTracking(Rigidbody body, Transform tr)
        {
            _body = body;
            _transform = tr;

            if (_velocitySamples == null || _velocitySamples.Length != velocitySampleCount)
            {
                _velocitySamples = new Vector3[velocitySampleCount];
                _angularVelocitySamples = new Vector3[velocitySampleCount];
            }

            Array.Clear(_velocitySamples, 0, _velocitySamples.Length);
            Array.Clear(_angularVelocitySamples, 0, _angularVelocitySamples.Length);

            _lastPosition = _transform.position;
            _lastRotation = _transform.rotation;
            _index = 0;
            _count = 0;
            _tracking = true;
        }

        /// <summary>
        /// Records one velocity / angular velocity sample. Call from the owner's FixedUpdate
        /// while the object is being held.
        /// </summary>
        public void Sample()
        {
            if (!_tracking || _transform == null) return;

            float dt = Time.fixedDeltaTime;
            if (dt <= 0f) return;

            Vector3 currentPosition = _transform.position;
            _velocitySamples[_index] = (currentPosition - _lastPosition) / dt;
            _lastPosition = currentPosition;

            Quaternion currentRotation = _transform.rotation;
            Quaternion deltaRotation = currentRotation * Quaternion.Inverse(_lastRotation);
            _angularVelocitySamples[_index] = AngularVelocityFromDelta(deltaRotation, dt);
            _lastRotation = currentRotation;

            _index = (_index + 1) % _velocitySamples.Length;
            _count++;
        }

        /// <summary>
        /// Stops tracking and applies the averaged throw velocity to the Rigidbody.
        /// No-op if the body is kinematic on release (preserves designer intent).
        /// Returns the throw velocity that was applied (zero if nothing applied).
        /// </summary>
        public Vector3 ApplyThrow()
        {
            if (!_tracking) return Vector3.zero;
            _tracking = false;

            int n = Mathf.Min(_count, _velocitySamples?.Length ?? 0);
            if (n == 0 || _body == null)
            {
                onThrowEnd?.Invoke(Vector3.zero);
                return Vector3.zero;
            }

            Vector3 avgVelocity = Vector3.zero;
            Vector3 avgAngular = Vector3.zero;
            for (int i = 0; i < n; i++)
            {
                avgVelocity += _velocitySamples[i];
                avgAngular += _angularVelocitySamples[i];
            }
            avgVelocity /= n;
            avgAngular /= n;

            Vector3 throwVelocity = avgVelocity * throwMultiplier;
            Vector3 throwAngular = avgAngular * angularVelocityMultiplier;

            // The Rigidbody.linearVelocity setter is a no-op on kinematic bodies, so respect that contract.
            if (!_body.isKinematic)
            {
                _body.linearVelocity = throwVelocity;
                if (enableAngularVelocity) _body.angularVelocity = throwAngular;
            }

            onThrowEnd?.Invoke(throwVelocity);
            return throwVelocity;
        }

        /// <summary>Cancels tracking without applying any throw (e.g. for forced release).</summary>
        public void CancelTracking()
        {
            _tracking = false;
            _count = 0;
        }

        private static Vector3 AngularVelocityFromDelta(Quaternion deltaRotation, float deltaTime)
        {
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            return axis * (angle * Mathf.Deg2Rad / deltaTime);
        }
    }
}

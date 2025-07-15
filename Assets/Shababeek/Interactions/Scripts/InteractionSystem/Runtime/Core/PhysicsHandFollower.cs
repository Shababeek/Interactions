using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Follows a target transform using physics, with smoothing, deadzone, and anti-jitter features for VR hand tracking.
    /// </summary>
    public class PhysicsHandFollower : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The transform the hand should follow.")]
        [SerializeField] private Transform target;
        
        [Header("Velocity Settings")]
        [Tooltip("Maximum allowed velocity for the hand.")]
        [SerializeField] private float maxVelocity = 30f;
        [Tooltip("Maximum allowed distance before teleporting the hand.")]
        [SerializeField] private float maxDistance = 0.3f;
        [Tooltip("Minimum distance before the hand starts following.")]
        [SerializeField] private float minDistance = 0.02f;
        
        [Header("Smoothing Settings")]
        [Tooltip("Smoothing factor for position interpolation.")]
        [SerializeField] private float positionSmoothing = 0.15f;
        [Tooltip("Smoothing factor for rotation interpolation.")]
        [SerializeField] private float rotationSmoothing = 0.15f;
        [Tooltip("Damping factor for linear velocity.")]
        [SerializeField] private float velocityDamping = 0.9f;
        [Tooltip("Damping factor for angular velocity.")]
        [SerializeField] private float angularVelocityDamping = 0.9f;
        
        [Header("Advanced Settings")]
        [Tooltip("Enable deadzone to ignore small movements and reduce jitter.")]
        [SerializeField] private bool useDeadzone = true;
        [Tooltip("Deadzone threshold for position changes.")]
        [SerializeField] private float positionDeadzone = 0.002f;
        [Tooltip("Deadzone threshold for rotation changes (in degrees).")]
        [SerializeField] private float rotationDeadzone = 0.2f;
        
        private float _maxVelocitySqrt;
        private Rigidbody _body;
        
        // Smoothing variables
        private Vector3 _smoothedPosition;
        private Quaternion _smoothedRotation;
        
        /// <summary>
        /// The target transform that the hand should follow.
        /// </summary>
        public Transform Target
        {
            set => target = value;
        }

        private void Start()
        {
            if (!target)
            {
                Debug.LogError("target is not set");
                return;
            }
            _maxVelocitySqrt = maxVelocity * maxVelocity;
            _body = GetComponent<Rigidbody>();
            
            _smoothedPosition = target != null ? target.position : transform.position;
            _smoothedRotation = target != null ? target.rotation : transform.rotation;
            
            Teleport();
        }

        void FixedUpdate()
        {
            if (!target) return;
            
            UpdateSmoothing();
            SetVelocity();
            SetAngularVelocity();
        }

        private void UpdateSmoothing()
        {
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, target.position, positionSmoothing);
            _smoothedRotation = Quaternion.Slerp(_smoothedRotation, target.rotation, rotationSmoothing);
        }

        private void SetVelocity()
        {
            Vector3 velocityVector = _smoothedPosition - transform.position;
            
            // Check distance limits
            var distance = velocityVector.magnitude;
            if (distance > maxDistance || distance < minDistance)
            {
                Teleport();
                return;
            }
            
            // Apply deadzone
            if (useDeadzone && distance < positionDeadzone)
            {
                _body.linearVelocity *= velocityDamping;
                return;
            }
            
            // Calculate smooth velocity
            var speed = Mathf.Lerp(0f, maxVelocity, distance / maxDistance);
            var targetVelocity = velocityVector.normalized * speed;
            
            // Apply damping and smoothing
            _body.linearVelocity = Vector3.Lerp(_body.linearVelocity, targetVelocity, 1f - velocityDamping);
            _body.linearVelocity = Vector3.ClampMagnitude(_body.linearVelocity, maxVelocity);
        }

        private void SetAngularVelocity()
        {
            Quaternion relativeRotation = FindRelativeRotation(_smoothedRotation, transform.rotation);
            relativeRotation.ToAngleAxis(out float angle, out Vector3 axis);
            
            // Apply deadzone
            if (useDeadzone && angle < rotationDeadzone)
            {
                _body.angularVelocity *= angularVelocityDamping;
                return;
            }
            
            // Calculate smooth angular velocity
            var angularSpeed = Mathf.Lerp(0f, maxVelocity * 0.5f, angle / 45f);
            Vector3 targetAngularVelocity = axis * (Mathf.Deg2Rad * angularSpeed);
            
            // Apply damping and smoothing
            _body.angularVelocity = Vector3.Lerp(_body.angularVelocity, targetAngularVelocity, 1f - angularVelocityDamping);
        }

        private Quaternion FindRelativeRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
            }
            return a * Quaternion.Inverse(b);
        }

        private void Teleport()
        {
            if (!target) return;
            
            _body.position = target.position;
            _body.rotation = target.rotation;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            
            _smoothedPosition = target.position;
            _smoothedRotation = target.rotation;
        }
    }
} 
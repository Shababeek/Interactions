using UnityEngine;

namespace Shababeek.Interactions.Core
{
    [AddComponentMenu("Shababeek/Interactions/Physics Hand Follower")]
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsHandFollower : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The transform the hand should follow (typically the hand pivot in the camera rig).")]
        [SerializeField] private Transform target;
        
        [Header("Movement Settings")]
        [Tooltip("Strength multiplier for position following. Higher values = faster response.")]
        [SerializeField] private float positionStrength = 1000f;
        
        [Tooltip("Strength multiplier for rotation following. Higher values = faster response.")]
        [SerializeField] private float rotationStrength = 100f;
        
        [Tooltip("Maximum velocity magnitude in units per second. Prevents excessive speeds.")]
        [SerializeField] private float maxVelocity = 10f;
        
        [Tooltip("Maximum angular velocity magnitude in radians per second. Prevents excessive rotation speeds.")]
        [SerializeField] private float maxAngularVelocity = 20f;
        
        [Header("Deadzone Settings")]
        [Tooltip("Distance threshold below which the hand won't move (reduces micro-jitter).")]
        [SerializeField] private float positionDeadzone = 0.001f;
        
        [Tooltip("Angle threshold in degrees below which the hand won't rotate (reduces micro-jitter).")]
        [SerializeField] private float rotationDeadzone = 0.5f;
        
        [Header("Teleport Settings")]
        [Tooltip("Distance threshold above which the hand will teleport instead of follow.")]
        [SerializeField] private float teleportDistance = 1f;
        
        [Tooltip("If enabled, the hand will teleport when it gets too far from the target.")]
        [SerializeField] private bool enableTeleport = true;
        
        [Header("Collision Settings")]
        [Tooltip("If enabled, stops applying forces when in contact with objects to prevent fighting physics.")]
        [SerializeField] private bool respectCollisions = true;

        private Rigidbody _rigidbody;
        private bool _isInitialized;
        private bool _hasCollision;

        /// <summary>
        /// Gets or sets the target transform that the hand should follow.
        /// </summary>
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        /// <summary>
        /// Applies settings to the follower from a configuration preset.
        /// </summary>
        public void ApplySettings(PhysicsFollowerSettings settings)
        {
            positionStrength = settings.positionStrength;
            rotationStrength = settings.rotationStrength;
            maxVelocity = settings.maxVelocity;
            maxAngularVelocity = settings.maxAngularVelocity;
            positionDeadzone = settings.positionDeadzone;
            rotationDeadzone = settings.rotationDeadzone;
            teleportDistance = settings.teleportDistance;
            respectCollisions = settings.respectCollisions;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        private void Start()
        {
            if (target == null)
            {
                Debug.LogError($"[PhysicsHandFollower] Target is not assigned on {gameObject.name}", this);
                enabled = false;
                return;
            }

            Teleport();
            _isInitialized = true;
        }

        private void FixedUpdate()
        {
            if (!_isInitialized || target == null) return;

            float distance = Vector3.Distance(transform.position, target.position);
            
            if (enableTeleport && distance > teleportDistance)
            {
                Teleport();
                return;
            }

            if (respectCollisions && _hasCollision)
            {
                return;
            }

            UpdatePosition(distance);
            UpdateRotation();
        }

        private void ConfigureRigidbody()
        {
            _rigidbody.useGravity = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void UpdatePosition(float distance)
        {
            if (distance < positionDeadzone)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 direction = (target.position - transform.position).normalized;
            float velocityMagnitude = Mathf.Min(distance * positionStrength * Time.fixedDeltaTime, maxVelocity);
            
            _rigidbody.linearVelocity = direction * velocityMagnitude;
        }

        private void UpdateRotation()
        {
            Quaternion deltaRotation = target.rotation * Quaternion.Inverse(transform.rotation);
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            
            if (angle > 180f)
                angle -= 360f;

            if (Mathf.Abs(angle) < rotationDeadzone)
            {
                _rigidbody.angularVelocity = Vector3.zero;
                return;
            }

            Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad * rotationStrength * Time.fixedDeltaTime);
            _rigidbody.angularVelocity = Vector3.ClampMagnitude(angularVelocity, maxAngularVelocity);
        }

        private void OnCollisionEnter(Collision collision)
        {
            _hasCollision = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            _hasCollision = false;
        }

        private void Teleport()
        {
            if (target == null) return;

            _rigidbody.position = target.position;
            _rigidbody.rotation = target.rotation;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (target == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position, 0.02f);
            
            if (enableTeleport)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawWireSphere(target.position, teleportDistance);
            }
        }
#endif
    }
}
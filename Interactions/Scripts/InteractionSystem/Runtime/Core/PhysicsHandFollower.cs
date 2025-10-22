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
        [Tooltip("If enabled, reduces velocity when blocked by objects to prevent twitching.")]
        [SerializeField] private bool preventCollisionTwitching = true;
        
        [Tooltip("Velocity threshold to detect if the hand is stuck. Lower values = more sensitive.")]
        [SerializeField] private float stuckVelocityThreshold = 0.1f;
        
        [Tooltip("Distance threshold to detect if hand is making progress towards target.")]
        [SerializeField] private float stuckDistanceThreshold = 0.01f;
        
        [Tooltip("If enabled, freezes rotation constraints to prevent unwanted spinning.")]
        [SerializeField] private bool constrainRotation = false;
        
        [Tooltip("Damping factor applied to existing angular velocity (0-1). Lower values = more damping.")]
        [SerializeField] private float angularVelocityDamping = 0.7f;

        private Rigidbody _rigidbody;
        private bool _isInitialized;
        private Vector3 _lastPosition;
        private float _lastDistance;
        private int _stuckFrameCount;
        private bool _lastConstrainRotation;
        private bool _isColliding;

        /// <summary>
        /// Gets or sets the target transform that the hand should follow.
        /// </summary>
        public Transform Target
        {
            get => target;
            set 
            { 
                target = value;
                if (target != null && _isInitialized)
                {
                    SyncParentWithTarget();
                }
            }
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

            SyncParentWithTarget();
            Teleport();
            _lastPosition = transform.position;
            _lastDistance = Vector3.Distance(transform.position, target.position);
            _isInitialized = true;
        }

        private void FixedUpdate()
        {
            if (!_isInitialized || target == null) return;

            // Update rigidbody constraints if setting changed
            if (_lastConstrainRotation != constrainRotation)
            {
                ConfigureRigidbody();
                _lastConstrainRotation = constrainRotation;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            
            if (enableTeleport && distance > teleportDistance)
            {
                Teleport();
                _lastPosition = transform.position;
                _lastDistance = distance;
                _stuckFrameCount = 0;
                return;
            }

            bool isStuck = CheckIfStuck(distance);
            
            if (isStuck && preventCollisionTwitching)
            {
                // Zero both linear and angular velocity to prevent twitching
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                
                // Also reduce any residual velocities
                _rigidbody.Sleep();
                _rigidbody.WakeUp();
            }
            else
            {
                UpdatePosition(distance);
                
                // Handle rotation based on constraint setting
                if (constrainRotation)
                {
                    // Manually set rotation to match target when rotation is constrained
                    transform.rotation = target.rotation;
                }
                else
                {
                    // Only update rotation if not colliding to prevent fighting with physics engine
                    if (!_isColliding || !preventCollisionTwitching)
                    {
                        UpdateRotation();
                    }
                }
            }
            
            _lastPosition = transform.position;
            _lastDistance = distance;
        }

        private void ConfigureRigidbody()
        {
            _rigidbody.useGravity = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            
            // Apply rotation constraints if enabled to reduce unwanted spinning
            if (constrainRotation)
            {
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                _rigidbody.constraints = RigidbodyConstraints.None;
            }
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

            // Calculate desired angular velocity
            Vector3 desiredAngularVelocity = axis * (angle * Mathf.Deg2Rad * rotationStrength * Time.fixedDeltaTime);
            
            // Dampen existing angular velocity and add new velocity
            Vector3 dampedCurrentVelocity = _rigidbody.angularVelocity * angularVelocityDamping;
            Vector3 newAngularVelocity = dampedCurrentVelocity + desiredAngularVelocity;
            
            // Clamp to max magnitude
            _rigidbody.angularVelocity = Vector3.ClampMagnitude(newAngularVelocity, maxAngularVelocity);
        }

        private bool CheckIfStuck(float currentDistance)
        {
            if (currentDistance < positionDeadzone)
            {
                _stuckFrameCount = 0;
                return false;
            }

            float actualMovement = Vector3.Distance(transform.position, _lastPosition);
            float distanceImprovement = _lastDistance - currentDistance;
            
            bool isMovingSlow = actualMovement < stuckVelocityThreshold * Time.fixedDeltaTime;
            bool notMakingProgress = distanceImprovement < stuckDistanceThreshold * Time.fixedDeltaTime;
            
            if (isMovingSlow && notMakingProgress)
            {
                _stuckFrameCount++;
                return _stuckFrameCount > 2;
            }
            
            _stuckFrameCount = 0;
            return false;
        }

        private void Teleport()
        {
            if (target == null) return;

            _rigidbody.position = target.position;
            _rigidbody.rotation = target.rotation;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            _stuckFrameCount = 0;
        }

        private void OnCollisionEnter(Collision collision)
        {
            _isColliding = true;
        }
        
        private void OnCollisionExit(Collision collision)
        {
            _isColliding = false;
        }

        /// <summary>
        /// Synchronizes the parent of this GameObject with the target's parent.
        /// </summary>
        private void SyncParentWithTarget()
        {
            if (target == null) return;
            
            Transform targetParent = target.parent;
            if (transform.parent != targetParent)
            {
                transform.SetParent(targetParent, true);
            }
        }

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
    }
}
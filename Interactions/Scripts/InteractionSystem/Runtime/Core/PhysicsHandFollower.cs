using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Follows a target transform using physics, with smoothing, deadzone, and anti-jitter features for VR hand tracking.
    /// </summary>
    /// <remarks>
    /// This component provides smooth, physics-based following behavior for VR hands. It uses a combination of
    /// smoothing, deadzones, and velocity clamping to create natural hand movement while reducing jitter and
    /// maintaining performance. The component automatically handles teleportation when the target moves too far
    /// and provides configurable smoothing for both position and rotation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Add this component to a hand GameObject with a Rigidbody
    /// var follower = hand.AddComponent<PhysicsHandFollower>();
    /// 
    /// // Set the target transform (usually the hand pivot in the camera rig)
    /// follower.Target = handPivot;
    /// </code>
    /// </example>
    [AddComponentMenu("Shababeek/Interactions/Physics Hand Follower")]
    public class PhysicsHandFollower : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The transform the hand should follow. This is typically the hand pivot in the camera rig.")]
        [SerializeField] private Transform target;
        
        [Header("Velocity Settings")]
        [Tooltip("Maximum allowed velocity for the hand in units per second. Higher values allow faster movement but may cause instability.")]
        [SerializeField] private float maxVelocity = 30f;
        
        [Tooltip("Maximum allowed distance before teleporting the hand. If the target moves beyond this distance, the hand will instantly teleport.")]
        [SerializeField] private float maxDistance = 0.3f;
        
        [Tooltip("Minimum distance before the hand starts following. Small movements below this threshold are ignored to reduce jitter.")]
        [SerializeField] private float minDistance = 0.02f;
        
        [Header("Smoothing Settings")]
        [Tooltip("Smoothing factor for position interpolation. Lower values create smoother but slower movement, higher values create faster but potentially jittery movement.")]
        [SerializeField] private float positionSmoothing = 0.15f;
        
        [Tooltip("Smoothing factor for rotation interpolation. Lower values create smoother but slower rotation, higher values create faster but potentially jittery rotation.")]
        [SerializeField] private float rotationSmoothing = 0.15f;
        
        [Tooltip("Damping factor for linear velocity. Higher values (closer to 1) create more responsive movement, lower values create more gradual deceleration.")]
        [SerializeField] private float velocityDamping = 0.9f;
        
        [Tooltip("Damping factor for angular velocity. Higher values (closer to 1) create more responsive rotation, lower values create more gradual deceleration.")]
        [SerializeField] private float angularVelocityDamping = 0.9f;
        
        [Header("Advanced Settings")]
        [Tooltip("Enable deadzone to ignore small movements and reduce jitter. When enabled, very small position and rotation changes are ignored.")]
        [SerializeField] private bool useDeadzone = true;
        
        [Tooltip("Deadzone threshold for position changes in world units. Movements smaller than this value are ignored.")]
        [SerializeField] private float positionDeadzone = 0.002f;
        
        [Tooltip("Deadzone threshold for rotation changes in degrees. Rotations smaller than this value are ignored.")]
        [SerializeField] private float rotationDeadzone = 0.2f;
        
        #region Private Fields

        private float _maxVelocitySqrt;
        private Rigidbody _body;
        
        // Smoothing variables
        private Vector3 _smoothedPosition;
        private Quaternion _smoothedRotation;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the target transform that the hand should follow.
        /// </summary>
        /// <remarks>
        /// This transform is typically the hand pivot in the camera rig. The hand will
        /// smoothly follow this target using physics-based movement.
        /// </remarks>
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes the physics hand follower component.
        /// </summary>
        /// <remarks>
        /// This method validates the target, calculates the squared maximum velocity for performance,
        /// gets the Rigidbody component, and initializes the smoothing variables. It also performs
        /// an initial teleport to ensure the hand starts at the correct position.
        /// </remarks>
        private void Start()
        {
            if (!target)
            {
                Debug.LogError("PhysicsHandFollower: Target is not set. Please assign a target transform in the inspector.");
                return;
            }
            
            _maxVelocitySqrt = maxVelocity * maxVelocity;
            _body = GetComponent<Rigidbody>();
            
            if (_body == null)
            {
                Debug.LogError("PhysicsHandFollower: No Rigidbody component found. This component requires a Rigidbody to function.");
                return;
            }
            
            _smoothedPosition = target.position;
            _smoothedRotation = target.rotation;
            
            Teleport();
        }

        /// <summary>
        /// Updates the hand physics every fixed physics frame.
        /// </summary>
        /// <remarks>
        /// This method is called every fixed physics frame to update the hand's position and rotation.
        /// It updates smoothing, sets linear velocity, and sets angular velocity based on the target.
        /// </remarks>
        private void FixedUpdate()
        {
            if (!target) return;
            
            UpdateSmoothing();
            SetVelocity();
            SetAngularVelocity();
        }

        #endregion

        #region Smoothing

        /// <summary>
        /// Updates the smoothed position and rotation values.
        /// </summary>
        /// <remarks>
        /// This method interpolates between the current smoothed values and the target values
        /// using the configured smoothing factors. This creates the smooth following behavior.
        /// </remarks>
        private void UpdateSmoothing()
        {
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, target.position, positionSmoothing);
            _smoothedRotation = Quaternion.Slerp(_smoothedRotation, target.rotation, rotationSmoothing);
        }

        #endregion

        #region Velocity Control

        /// <summary>
        /// Sets the linear velocity of the hand based on the smoothed position.
        /// </summary>
        /// <remarks>
        /// This method calculates the appropriate velocity to move the hand toward the smoothed position.
        /// It includes distance checking, deadzone application, and velocity clamping for smooth movement.
        /// </remarks>
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

        /// <summary>
        /// Sets the angular velocity of the hand based on the smoothed rotation.
        /// </summary>
        /// <remarks>
        /// This method calculates the appropriate angular velocity to rotate the hand toward the smoothed rotation.
        /// It includes deadzone application and angular velocity clamping for smooth rotation.
        /// </remarks>
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

        #endregion

        #region Utility Methods

        /// <summary>
        /// Finds the relative rotation between two quaternions.
        /// </summary>
        /// <param name="a">The target rotation</param>
        /// <param name="b">The current rotation</param>
        /// <returns>The relative rotation from b to a</returns>
        /// <remarks>
        /// This method handles the shortest rotation path between two quaternions by ensuring
        /// the dot product is positive, which prevents the hand from taking the long way around
        /// when rotating.
        /// </remarks>
        private Quaternion FindRelativeRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
            }
            return a * Quaternion.Inverse(b);
        }

        /// <summary>
        /// Instantly teleports the hand to the target position and rotation.
        /// </summary>
        /// <remarks>
        /// This method is called when the target moves too far away or when the component starts.
        /// It resets all velocities and positions to ensure smooth following can resume.
        /// </remarks>
        private void Teleport()
        {
            if (!target || _body == null) return;
            
            _body.position = target.position;
            _body.rotation = target.rotation;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            
            _smoothedPosition = target.position;
            _smoothedRotation = target.rotation;
        }

        #endregion
    }
} 
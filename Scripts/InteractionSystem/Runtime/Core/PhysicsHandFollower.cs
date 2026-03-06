using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Makes a rigidbody-based hand follow a VR controller target using velocity-based movement.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Physics Hand Follower")]
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsHandFollower : MonoBehaviour
    {
        [Tooltip("The VR controller transform to follow.")]
        [SerializeField] private Transform target;
        [Tooltip("Speed multiplier for position-based hand movement.")]
        [SerializeField] private float followSpeed = 60;
        [Tooltip("Speed multiplier for rotation-based hand movement.")]
        [SerializeField] private float rotationSpeed = 40f;

        private Rigidbody _rb;

        /// <summary>
        /// The VR controller transform to follow.
        /// </summary>
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            if (target == null) return;
            
            Vector3 directionVector = target.position - _rb.position;
            _rb.linearVelocity = directionVector * followSpeed;
            
            Quaternion rotationDelta = target.rotation * Quaternion.Inverse(_rb.rotation);
            rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
            
            if (angle > 180f) angle -= 360f;
            
            _rb.angularVelocity = axis * (angle * Mathf.Deg2Rad * rotationSpeed);
        }
    }
}
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

        [Tooltip(
            "Upper bound in m/s on the velocity this follower will command. A real hand peaks " +
            "around 10 m/s, so this never engages in normal play; it exists so that a large " +
            "target-to-hand gap cannot turn into a physics explosion. Set to 0 to disable.")]
        [SerializeField] private float maxSpeed = 20f;

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
            EnsureBody();
        }

        private void EnsureBody()
        {
            if (_rb != null) return;
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        /// <summary>
        /// Moves the hand to a pose without the physics engine treating it as travel.
        /// <para>
        /// The follower drives the hand by commanding a velocity proportional to the gap between
        /// hand and target (<c>gap * followSpeed</c>). That is correct while the gap is a few
        /// centimetres and catastrophic when it is not: relocating the rig across a room leaves a
        /// gap of metres, which asks for hundreds of m/s on a body using continuous collision
        /// detection — a sweep across the whole level in a single step, shoving every collider on
        /// the path. Teleporting must therefore close the gap directly and discard the momentum
        /// rather than letting the follower chase it.
        /// </para>
        /// <para>
        /// Interpolation is suspended across the move because an interpolated body blends from its
        /// previous pose, which would draw the hand smearing from the old location to the new one
        /// over the following frame.
        /// </para>
        /// </summary>
        /// <param name="position">World position to place the hand at.</param>
        /// <param name="rotation">World rotation to place the hand at.</param>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            EnsureBody();

            var previousInterpolation = _rb.interpolation;
            _rb.interpolation = RigidbodyInterpolation.None;

            transform.SetPositionAndRotation(position, rotation);

            // Push the transform write into the physics scene before touching the body, so the
            // body is moved from its new pose rather than from a stale one.
            Physics.SyncTransforms();

            _rb.position = position;
            _rb.rotation = rotation;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            _rb.interpolation = previousInterpolation;
        }

        /// <summary>Moves the hand onto its target's current pose. Convenience for rig relocation.</summary>
        public void TeleportToTarget()
        {
            if (target == null) return;
            Teleport(target.position, target.rotation);
        }

        private void FixedUpdate()
        {
            if (target == null) return;

            Vector3 directionVector = target.position - _rb.position;
            Vector3 velocity = directionVector * followSpeed;

            // Clamping the commanded velocity rather than the gap keeps the follower's feel
            // identical in normal use — the clamp only engages once the gap is far larger than a
            // hand can actually be from its controller, which means something moved that should
            // have gone through Teleport.
            if (maxSpeed > 0f && velocity.sqrMagnitude > maxSpeed * maxSpeed)
                velocity = velocity.normalized * maxSpeed;

            _rb.linearVelocity = velocity;

            Quaternion rotationDelta = target.rotation * Quaternion.Inverse(_rb.rotation);
            rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);

            if (angle > 180f) angle -= 360f;

            _rb.angularVelocity = axis * (angle * Mathf.Deg2Rad * rotationSpeed);
        }
    }
}

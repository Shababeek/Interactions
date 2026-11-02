using System.Collections.Generic;
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

        [Header("Recovery")]
        [Tooltip(
            "Gap in metres between hand and target beyond which the hand is teleported at once. " +
            "Nothing a real hand does opens a gap this wide; it means the hand was left behind. " +
            "Set to 0 to disable the whole recovery guard.")]
        [SerializeField] private float recoverFarDistance = 1f;

        [Tooltip("Gap in metres that, held for Recover Stuck Seconds, marks the hand as stuck behind a collider.")]
        [SerializeField] private float recoverStuckDistance = 0.25f;

        [Tooltip("Seconds the gap must stay at or above Recover Stuck Distance before the hand is teleported.")]
        [SerializeField] private float recoverStuckSeconds = 0.75f;

        [Tooltip(
            "Longest time in seconds the hand and its carried prop ignore the collider that trapped them. " +
            "They stop ignoring it earlier as soon as they are clear of it.")]
        [SerializeField] private float recoverIgnoreSeconds = 2f;

        private readonly List<Collider> _ignoredTraps = new();
        private readonly List<Collider> _ignoringColliders = new();
        private readonly Collider[] _overlapBuffer = new Collider[32];
        private Rigidbody _rb;
        private float _stuckTime;
        private float _ignoreExpiry;

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

        private void OnDisable()
        {
            RestoreCollisions();
            _stuckTime = 0f;
        }

        /// <summary>
        /// Whether the hand is stranded: far from its target, or held back from it by something it
        /// cannot pass. A hand resting on a surface with its controller only slightly through it is
        /// not stranded — the gap has to be wide, or stay wide.
        /// </summary>
        private bool IsStranded(out string reason)
        {
            reason = null;
            if (recoverFarDistance <= 0f) return false;

            float gap = (target.position - _rb.position).magnitude;
            if (gap >= recoverFarDistance)
            {
                reason = $"far ({gap:0.00}m from its target)";
                return true;
            }

            if (gap < recoverStuckDistance)
            {
                _stuckTime = 0f;
                return false;
            }

            _stuckTime += Time.fixedDeltaTime;
            if (_stuckTime < recoverStuckSeconds) return false;

            reason = $"stuck ({gap:0.00}m from its target for {_stuckTime:0.##}s)";
            return true;
        }

        /// <summary>
        /// Snaps a stranded hand onto its target, carrying whatever it holds, and lets both pass
        /// through the colliders that trapped them until they are clear. Teleporting alone would
        /// put a hand whose controller is inside a table straight back into the table, where it
        /// would strand again on the next step.
        /// </summary>
        public void Recover(string reason = "requested")
        {
            if (target == null) return;
            EnsureBody();

            var handColliders = GetComponentsInChildren<Collider>();
            var held = HeldInteractable();
            var heldColliders = held != null ? held.GetComponentsInChildren<Collider>() : null;

            // The prop's pose relative to the hand, so it can be put back in the same grip. A prop
            // parented under the hand has already travelled with it and needs nothing.
            bool carry = held != null && !held.transform.IsChildOf(transform);
            Vector3 heldLocalPosition = default;
            Quaternion heldLocalRotation = default;
            if (carry)
            {
                heldLocalPosition = transform.InverseTransformPoint(held.transform.position);
                heldLocalRotation = Quaternion.Inverse(transform.rotation) * held.transform.rotation;
            }

            // Both ends of the move can be the trap: the table the hand is stuck behind and the
            // floor or table the target sits inside.
            RestoreCollisions();
            CollectTraps(handColliders, heldColliders, _rb.position);
            CollectTraps(handColliders, heldColliders, target.position);
            IgnoreTraps(handColliders);
            IgnoreTraps(heldColliders);

            Vector3 from = _rb.position;
            Teleport(target.position, target.rotation);

            if (carry)
            {
                held.transform.SetPositionAndRotation(
                    transform.TransformPoint(heldLocalPosition),
                    transform.rotation * heldLocalRotation);
                var heldBody = held.GetComponent<Rigidbody>();
                if (heldBody != null && !heldBody.isKinematic)
                {
                    heldBody.linearVelocity = Vector3.zero;
                    heldBody.angularVelocity = Vector3.zero;
                }

                Physics.SyncTransforms();
            }

            _stuckTime = 0f;
            _ignoreExpiry = Time.time + recoverIgnoreSeconds;

            Debug.Log(
                $"[HandRecovery] '{name}' teleported to its target: {reason}. Moved {(target.position - from).magnitude:0.00}m" +
                $"{(held != null ? $", carrying '{held.name}'" : string.Empty)}" +
                $", ignoring {_ignoredTraps.Count} collider(s) until clear.", this);
        }

        private InteractableBase HeldInteractable()
        {
            var interactor = GetComponentInChildren<InteractorBase>();
            if (interactor == null) interactor = GetComponent<InteractorBase>();
            var interactable = interactor != null ? interactor.CurrentInteractable : null;
            if (interactable == null || interactable.CurrentState != InteractionState.Selected) return null;
            return interactable;
        }

        private void CollectTraps(Collider[] handColliders, Collider[] heldColliders, Vector3 around)
        {
            float radius = 0.1f;
            foreach (var c in handColliders)
                radius = Mathf.Max(radius, c.bounds.extents.magnitude);
            if (heldColliders != null)
                foreach (var c in heldColliders)
                    radius = Mathf.Max(radius, c.bounds.extents.magnitude);

            int count = Physics.OverlapSphereNonAlloc(around, radius + 0.05f, _overlapBuffer,
                ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var other = _overlapBuffer[i];
                if (other == null || _ignoredTraps.Contains(other)) continue;
                if (System.Array.IndexOf(handColliders, other) >= 0) continue;
                if (heldColliders != null && System.Array.IndexOf(heldColliders, other) >= 0) continue;
                _ignoredTraps.Add(other);
            }
        }

        private void IgnoreTraps(Collider[] colliders)
        {
            if (colliders == null) return;
            foreach (var mine in colliders)
            {
                if (mine == null) continue;
                foreach (var trap in _ignoredTraps)
                    if (trap != null) Physics.IgnoreCollision(mine, trap, true);
                _ignoringColliders.Add(mine);
            }
        }

        private void RestoreCollisions()
        {
            foreach (var mine in _ignoringColliders)
            {
                if (mine == null) continue;
                foreach (var trap in _ignoredTraps)
                    if (trap != null) Physics.IgnoreCollision(mine, trap, false);
            }

            _ignoringColliders.Clear();
            _ignoredTraps.Clear();
        }

        private void UpdateIgnoredTraps()
        {
            if (_ignoredTraps.Count == 0) return;

            bool clear = Time.time >= _ignoreExpiry;
            if (!clear)
            {
                clear = true;
                foreach (var mine in _ignoringColliders)
                {
                    if (mine == null || !mine.enabled) continue;
                    foreach (var trap in _ignoredTraps)
                    {
                        if (trap != null && trap.enabled && mine.bounds.Intersects(trap.bounds))
                        {
                            clear = false;
                            break;
                        }
                    }

                    if (!clear) break;
                }
            }

            if (clear) RestoreCollisions();
        }

        private void FixedUpdate()
        {
            if (target == null) return;

            UpdateIgnoredTraps();

            if (IsStranded(out var strandedReason))
            {
                Recover(strandedReason);
                return;
            }

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

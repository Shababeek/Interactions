using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Mirrors a target transform's world position and rotation onto this transform every LateUpdate.
    /// Used for transform-based (non-physics) hands that must track a separate pivot while living
    /// outside the pivot's hierarchy.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Kinematic Hand Follower")]
    [DefaultExecutionOrder(10000)]
    public class KinematicHandFollower : MonoBehaviour
    {
        [Tooltip("The pivot transform whose world position and rotation will be mirrored onto this object each frame.")]
        [SerializeField] private Transform target;

        /// <summary>
        /// The pivot transform whose world position and rotation are mirrored each frame.
        /// </summary>
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        /// <summary>
        /// Moves the hand to a pose without the physics engine treating it as travel.
        /// <para>
        /// This follower already snaps rather than accelerating, so the move itself is trivial —
        /// the signature exists so that relocating a rig never has to know which follower its hands
        /// use. A kinematic hand still carries a Rigidbody, and a kinematic body that is moved by
        /// its transform alone reports a stale pose to queries and to any contact resolved in the
        /// same step, so the physics scene is synced explicitly.
        /// </para>
        /// </summary>
        /// <param name="position">World position to place the hand at.</param>
        /// <param name="rotation">World rotation to place the hand at.</param>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);

            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                var previousInterpolation = rb.interpolation;
                rb.interpolation = RigidbodyInterpolation.None;
                rb.position = position;
                rb.rotation = rotation;
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.interpolation = previousInterpolation;
            }

            Physics.SyncTransforms();
        }

        /// <summary>Moves the hand onto its target's current pose. Convenience for rig relocation.</summary>
        public void TeleportToTarget()
        {
            if (target == null) return;
            Teleport(target.position, target.rotation);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            var t = transform;
            t.SetPositionAndRotation(target.position, target.rotation);
        }
    }
}

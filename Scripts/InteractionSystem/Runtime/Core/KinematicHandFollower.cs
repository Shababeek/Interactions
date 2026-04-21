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

        private void LateUpdate()
        {
            if (target == null) return;
            var t = transform;
            t.SetPositionAndRotation(target.position, target.rotation);
        }
    }
}

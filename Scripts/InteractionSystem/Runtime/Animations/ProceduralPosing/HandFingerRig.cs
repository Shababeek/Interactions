using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Per-finger bone references used by procedural pose fitting (arc baking and surface solving).
    /// Add to hand prefabs; use the inspector's auto-populate button to fill from a humanoid avatar.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Animations/Hand Finger Rig")]
    public class HandFingerRig : MonoBehaviour
    {
        /// <summary>Bone references for a single finger.</summary>
        [System.Serializable]
        public class FingerBones
        {
            [Tooltip("Transform at the fingertip (distal bone or its end child). Used as the contact probe.")]
            public Transform tip;

            [Tooltip("Transform at the middle phalanx. Improves arc shape for the solver; optional.")]
            public Transform mid;

            [Tooltip("Approximate finger radius in meters, used as the cast thickness.")]
            public float radius = 0.008f;
        }

        [Tooltip("Bones per finger, ordered Thumb, Index, Middle, Ring, Pinky.")]
        [SerializeField] private FingerBones[] fingers = new FingerBones[5];

        /// <summary>Bone references for the given finger (0=Thumb..4=Pinky).</summary>
        public FingerBones this[int finger] => fingers[finger];

        /// <summary>True when every finger has at least a tip reference.</summary>
        public bool IsValid
        {
            get
            {
                if (fingers == null || fingers.Length != 5) return false;
                for (int i = 0; i < 5; i++)
                {
                    if (fingers[i] == null || fingers[i].tip == null) return false;
                }
                return true;
            }
        }

        /// <summary>Replaces all finger bone references. Used by editor auto-population.</summary>
        public void SetFingers(FingerBones[] newFingers)
        {
            fingers = newFingers;
        }
    }
}

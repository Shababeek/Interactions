
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>How a finger responds to player input during a constrained interaction.</summary>
    public enum FingerConstraintMode
    {
        /// <summary>Legacy data written before the mode enum; resolves from the locked flag and range.</summary>
        Unset = 0,
        /// <summary>Input passes through unchanged (full 0-1 curl).</summary>
        Free = 1,
        /// <summary>Input is remapped into the [min, max] range.</summary>
        Range = 2,
        /// <summary>Finger is held at a fixed curl value; input is ignored.</summary>
        Fixed = 3
    }

    /// <summary>Constraints for a single finger's pose.</summary>
    [System.Serializable]
    public struct FingerConstraints
    {
        [Tooltip("How the finger responds to input: Free (pass-through), Range (remapped into min-max), or Fixed (held at a value).")]
        public FingerConstraintMode mode;

        /// <summary>Legacy flag from before the mode enum. Kept for serialized data; use Mode instead.</summary>
        [HideInInspector]
        public bool locked;

        /// <summary>Minimum finger curl (Range mode) or the held curl value (Fixed mode).</summary>
        [Range(0, 1)]
        public float min;

        /// <summary>Maximum finger curl value (Range mode).</summary>
        [Range(0, 1)]
        public float max;

        /// <summary>Resolved constraint mode; transparently migrates legacy locked/min/max data.</summary>
        public FingerConstraintMode Mode
        {
            get
            {
                if (mode != FingerConstraintMode.Unset) return mode;
                if (locked) return FingerConstraintMode.Fixed;
                return min <= 0f && max >= 1f ? FingerConstraintMode.Free : FingerConstraintMode.Range;
            }
            set
            {
                mode = value;
                // Keep the legacy flag consistent for any external readers of serialized data.
                locked = value == FingerConstraintMode.Fixed;
            }
        }

        /// <summary>The curl value a Fixed finger is held at (stored in min).</summary>
        public float FixedValue
        {
            get => min;
            set => min = value;
        }

        /// <summary>Initializes finger constraints with an explicit mode and value range.</summary>
        public FingerConstraints(FingerConstraintMode mode, float min, float max)
        {
            this.min = min;
            this.max = max;
            this.mode = mode;
            locked = mode == FingerConstraintMode.Fixed;
        }

        /// <summary>Initializes finger constraints from legacy lock/range values.</summary>
        public FingerConstraints(bool locked, float min, float max)
        {
            this.min = min;
            this.max = max;
            this.locked = locked;
            mode = locked
                ? FingerConstraintMode.Fixed
                : (min <= 0f && max >= 1f ? FingerConstraintMode.Free : FingerConstraintMode.Range);
        }

        /// <summary>Applies constraints to a finger value, returning the constrained result.</summary>
        public float GetConstrainedValue(float value)
        {
            switch (Mode)
            {
                case FingerConstraintMode.Fixed:
                    return min;
                case FingerConstraintMode.Free:
                    return value;
                default:
                    return (max - min) * value + min;
            }
        }

        /// <summary>
        /// Unconstrained finger (Free mode, full 0-1 curl).
        /// </summary>
        public static FingerConstraints Free => new(FingerConstraintMode.Free, 0, 1);
    }
    /// <summary>Constraints for all fingers in a hand pose.</summary>
    [System.Serializable]
    public struct PoseConstrains
    {
        /// <summary>Index of the target pose.</summary>
        public int targetPoseIndex;

        /// <summary>Constraints for the index finger.</summary>
        public FingerConstraints indexFingerLimits;
        /// <summary>Constraints for the middle finger.</summary>
        public FingerConstraints middleFingerLimits;
        /// <summary>Constraints for the ring finger.</summary>
        public FingerConstraints ringFingerLimits;
        /// <summary>Constraints for the pinky finger.</summary>
        public FingerConstraints pinkyFingerLimits;
        /// <summary>Constraints for the thumb.</summary>
        public FingerConstraints thumbFingerLimits;

   


        /// <summary>Unconstrained hand with all fingers free.</summary>
        public static PoseConstrains Free
        {
            get
            {
                var hand = new PoseConstrains();
                hand.indexFingerLimits = FingerConstraints.Free;
                hand.middleFingerLimits = FingerConstraints.Free;
                hand.ringFingerLimits = FingerConstraints.Free;
                hand.pinkyFingerLimits = FingerConstraints.Free;
                hand.thumbFingerLimits = FingerConstraints.Free;
                hand.targetPoseIndex = 0;
                return hand;
            }
        }
        /// <summary>Pointing hand pose with index free and other fingers constrained.</summary>
        public static PoseConstrains Pointing
        {
            get
            {
                var hand = new PoseConstrains();
                hand.indexFingerLimits = new FingerConstraints(FingerConstraintMode.Free, 0, 1);
                hand.middleFingerLimits = new FingerConstraints(FingerConstraintMode.Range, .3f, 1);
                hand.ringFingerLimits = new FingerConstraints(FingerConstraintMode.Range, .3f, 1);
                hand.pinkyFingerLimits = new FingerConstraints(FingerConstraintMode.Range, .3f, 1);
                hand.thumbFingerLimits = new FingerConstraints(FingerConstraintMode.Range, .3f, 1);
                return hand;
            }
        }
        /// <summary>Gets finger constraints by index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).</summary>
        public (FingerConstraints constraints, int pose) this[int index] => (this[(FingerName)index]);

        /// <summary>Gets finger constraints by finger name.</summary>
        public (FingerConstraints constraints, int targetPoseIndex) this[FingerName index]
        {
            get
            {
                var constraint = FingerConstraints.Free;
                switch (index)
                {
                    case FingerName.Thumb:
                        constraint = thumbFingerLimits;
                        break;
                    case FingerName.Index:
                        constraint = indexFingerLimits;
                        break;
                    case FingerName.Middle:
                        constraint = middleFingerLimits;
                        break;
                    case FingerName.Ring:
                        constraint = ringFingerLimits;
                        break;
                    case FingerName.Pinky:
                        constraint = pinkyFingerLimits;
                        break;
                }

                return (constraint, targetPoseIndex);
            }
        }
    }
}
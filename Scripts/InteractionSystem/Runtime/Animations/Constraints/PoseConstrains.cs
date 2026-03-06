
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>Constraints for a single finger's pose.</summary>
    [System.Serializable]
    public struct FingerConstraints
    {
        /// <summary>Whether the finger is locked at its minimum value.</summary>
        public bool locked;
        /// <summary>Minimum finger curl value (0-1).</summary>
        [Range(0, 1)]
        public float min;
        /// <summary>Maximum finger curl value (0-1).</summary>
        public float max;
        /// <summary>Initializes finger constraints with lock state and value range.</summary>
        public FingerConstraints(bool locked, float min, float max)
        {
            this.min = min;
            this.max = max;
            this.locked = locked;
        }
        /// <summary>Applies constraints to a finger value, returning the constrained result.</summary>
        public float GetConstrainedValue(float value)
        {
            if (locked)
            {
                return min;
            }
            return (max - min) * value + min;
        }
        /// <summary>
        /// Unconstrained finger (min=0, max=1, not locked).
        /// </summary>
        public static FingerConstraints Free
        {
            get
            {


                return new FingerConstraints(false, 0, 1); ;
            }
        }
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
                hand.indexFingerLimits = new FingerConstraints(false, 0, 1);
                hand.middleFingerLimits = new FingerConstraints(false, .3f, 1);
                hand.ringFingerLimits = new FingerConstraints(false, .3f, 1);
                hand.pinkyFingerLimits = new FingerConstraints(false, .3f, 1);
                hand.thumbFingerLimits = new FingerConstraints(false, .3f, 1);
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
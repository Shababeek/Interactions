
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Constraints for finger poses.
    /// </summary>
    [System.Serializable]
    public struct FingerConstraints
    {
        /// <summary>
        /// Whether the finger is locked at its minimum value.
        /// </summary>
        public bool locked;
        [Range(0, 1)]
        public float min;
        public float max;
        public FingerConstraints(bool locked, float min, float max)
        {
            this.min = min;
            this.max = max;
            this.locked = locked;
        }
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
    [System.Serializable]
    public struct PoseConstrains
    {

        public int targetPoseIndex;

        public FingerConstraints indexFingerLimits;
        public FingerConstraints middleFingerLimits;
        public FingerConstraints ringFingerLimits;
        public FingerConstraints pinkyFingerLimits;
        public FingerConstraints thumbFingerLimits;

   


        /// <summary>
        /// Unconstrained hand with all fingers free.
        /// </summary>
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
        public (FingerConstraints constraints,int pose) this[int index] => (this[(FingerName)index]);
        
        public (FingerConstraints constraints,int targetPoseIndex) this[FingerName index]
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

using UnityEngine;

namespace Shababeek.Interactions.Core
{
    [System.Serializable]
    /// <summary>
    /// Represents constraints for finger poses in the interaction system.
    /// </summary>
    /// <remarks>
    /// This struct defines the constraints for each finger, including whether it is locked,
    /// and the minimum and maximum values for its pose.
    /// It is used to manage finger poses during interactions.
    /// </remarks>
    /// <seealso cref="PoseConstrains"/>
    /// <seealso cref="IPoseConstrainer"/>
    /// <seealso cref="HandConstraints"/>
    public struct FingerConstraints
    {
        /// <summary>
        /// Indicates whether the finger is locked in its current pose.
        /// If true, the finger will not change its pose and will remain at the minimum value.
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
        /// Short for new FingerConstraints(false,0,1);
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
        /// A non constrained Hand
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
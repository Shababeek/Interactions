using Shababeek.Interactions.Animations;

namespace Shababeek.Interactions.Core
{
    public interface IPoseable
    {
        /// <summary>
        /// Finger position between two poses (0-1, where 0 is open and 1 is closed).
        /// </summary>
        public float this[int index]
        {
            get;
            set;
        }
        
        /// <summary>
        /// Finger position between two poses (0-1, where 0 is open and 1 is closed).
        /// </summary>
        public float this[FingerName index]
        {
            get;
            set;
        }
        
        /// <summary>
        /// Custom pose (0 is default).
        /// </summary>
        public int Pose
        {
            set;
        }
        
        /// <summary>
        /// Pose constraints for this hand.
        /// </summary>
        public PoseConstrains  Constrains { set; }

        HandData HandData { get;}
    }
    
}
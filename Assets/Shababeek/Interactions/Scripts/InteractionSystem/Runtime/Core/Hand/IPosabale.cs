namespace Shababeek.Interactions.Core
{
    public interface IPoseable
    {
        /// <summary>
        /// set the Position of the finger between two poses
        /// the value should be between 0 and 1, where 0 is open and 1 is closed
        /// </summary>
        /// <param name="index">0 is thumb, 4 is pinky</param>
        public float this[int index]
        {
            get;
            set;
        }
        /// <summary>
        /// set the Position of the finger between two poses
        /// the value should be between 0 and 1, where 0 is open and 1 is closed
        /// </summary>
        /// <param name="index"></param>
        public float this[FingerName index]
        {
            get;
            set;
        }
        /// <summary>
        /// sets a custom pose 0 is default
        /// </summary>
        public int Pose
        {
            set;
        }
        /// <summary>
        ///  changes the pose constraints
        /// </summary>
        public PoseConstrains  Constrains { set; } 
    }
    
}
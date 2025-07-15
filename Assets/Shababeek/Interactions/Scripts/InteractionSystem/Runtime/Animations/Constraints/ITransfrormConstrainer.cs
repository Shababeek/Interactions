using UnityEngine;

namespace Kinteractions_VR.Runtime.Animations.Constraints
{
    public interface ITransfrormConstrainer
    {
        public  Transform LeftHandPivot { get; set; }
        public  Transform rightHandPivot { get; set; }

    }
    
}
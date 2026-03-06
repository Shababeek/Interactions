using UnityEngine;

namespace Kinteractions_VR.Runtime.Animations.Constraints
{
    /// <summary>Interface for constraining hand transforms.</summary>
    public interface ITransfrormConstrainer
    {
        /// <summary>Gets or sets the left hand pivot transform.</summary>
        public Transform LeftHandPivot { get; set; }
        /// <summary>Gets or sets the right hand pivot transform.</summary>
        public Transform rightHandPivot { get; set; }
    }

}
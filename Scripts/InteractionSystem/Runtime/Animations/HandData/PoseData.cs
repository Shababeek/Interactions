using System;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>Hand pose including name, animation clips, and type (static or dynamic).</summary>
    [Serializable]
    public struct PoseData
    {
        [Tooltip("The animation clip for when the hand is fully open (no buttons are pressed).")]
        [SerializeField] private AnimationClip open;

        [Tooltip("The animation clip for when the hand is fully closed (all buttons are pressed).")]
        [SerializeField] private AnimationClip closed;

        [Tooltip("The name of the pose. If empty, it will be derived from the animation clips.")]
        [SerializeField] private string name;

        [Tooltip("The type of the pose: static (single pose) or dynamic (fingers will follow a value between two poses).")]
        [SerializeField] private PoseType type;

        /// <summary>Gets or sets the name of the pose (auto-generated if not set).</summary>
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }

                return Type == PoseType.Static ? open.name : $"{open.name}--{closed.name}";
            }
            set => name = value;
        } 
        
        /// <summary>Gets the animation clip for when the hand is open.</summary>
        public AnimationClip OpenAnimationClip => open;

        /// <summary>Gets the animation clip for when the hand is closed.</summary>
        public AnimationClip ClosedAnimationClip => closed;

        /// <summary>Gets the type of the pose (static or dynamic).</summary>
        public PoseType Type => type;

        /// <summary>Type of pose behavior.</summary>
        public enum PoseType
        {
            /// <summary>Dynamic pose with real-time control between open and closed states.</summary>
            Dynamic = 0,

            /// <summary>Static pose that plays a predefined animation.</summary>
            Static = 1
        }

        /// <summary>Sets the pose name if it is currently empty.</summary>
        public void SetPosNameIfEmpty(string name)
        {
            if (this.name == "")
            {
                this.name = name;
            }
        }

        /// <summary>Sets the pose type.</summary>
        public void SetType(PoseType type)
        {
            this.type = type;
        }
    }
}
using System;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Represents a single hand pose, including its name, animation clips, and type (static or dynamic).
    /// </summary>
    /// <remarks>
    /// PoseData defines how a hand should be animated, either through static predefined animations
    /// or dynamic real-time control between open and closed states. This struct is used by HandData
    /// to organize multiple poses for a hand.
    /// </remarks>
    [Serializable]
    public struct PoseData
    {
        [Header("Animation Clips")]
        [Tooltip("The animation clip for when the hand is fully open (no buttons are pressed).")]
        [SerializeField] private AnimationClip open;
        
        [Tooltip("The animation clip for when the hand is fully closed (all buttons are pressed).")]
        [SerializeField] private AnimationClip closed;
        
        [Header("Pose Configuration")]
        [Tooltip("The name of the pose. If empty, it will be derived from the animation clips.")]
        [SerializeField] private string name;
        
        [Tooltip("The type of the pose: static (single pose) or dynamic (fingers will follow a value between two poses).")]
        [SerializeField] private PoseType type;
        
        /// <summary>
        /// The name of the pose. If empty, it will be derived from the animation clips.
        /// </summary>
        /// <value>The name of the pose</value>
        /// <returns>The pose name, auto-generated if not set</returns>
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
        
        /// <summary>
        /// The animation clip for when the hand is open.
        /// </summary>
        /// <returns>The open hand animation clip</returns>
        public AnimationClip OpenAnimationClip => open;
        
        /// <summary>
        /// The animation clip for when the hand is closed.
        /// </summary>
        /// <returns>The closed hand animation clip</returns>
        public AnimationClip ClosedAnimationClip => closed;

        /// <summary>
        /// The type of the pose: static (predefined animation) or dynamic (real-time control).
        /// </summary>
        /// <returns>The current pose type</returns>
        public PoseType Type => type;

        /// <summary>
        /// Defines the type of pose behavior.
        /// </summary>
        public enum PoseType
        {
            /// <summary>
            /// Dynamic pose that allows real-time control between open and closed states.
            /// Fingers can be positioned anywhere between fully open and fully closed.
            /// </summary>
            Dynamic = 0,
            
            /// <summary>
            /// Static pose that plays a predefined animation clip.
            /// The pose is fixed and cannot be modified during runtime.
            /// </summary>
            Static = 1
        }

        /// <summary>
        /// Sets the name of the pose if it is currently empty.
        /// </summary>
        /// <param name="name">The new name to set</param>
        public void SetPosNameIfEmpty(string name)
        {
            if (this.name == "")
            {
                this.name = name;
            }
        }

        /// <summary>
        /// Sets the type of the pose.
        /// </summary>
        /// <param name="type">The new pose type</param>
        public void SetType(PoseType type)
        {
            this.type = type;
        }
    }
}
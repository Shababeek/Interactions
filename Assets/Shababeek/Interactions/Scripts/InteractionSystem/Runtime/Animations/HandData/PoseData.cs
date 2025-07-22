using System;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Represents a single hand pose, including its name, animation clips, and type (static or dynamic).
    /// </summary>
    [Serializable]
    public struct PoseData
    {
        [Tooltip("The animation clip for when the hand is fully open( no buttons are pressed).")]
        [SerializeField] private AnimationClip open;
        [Tooltip("The animation clip for when the hand is fully closed (all buttons are pressed).")]
        [SerializeField] private AnimationClip closed;
        [Tooltip("The name of the pose. If empty, it will be derived from the animation clips.")]
        [SerializeField] private string name;
        [Tooltip("The type of the pose: static (singe pose) or dynamic (fingers will follow a value between two poses).")]
        [SerializeField] private PoseType type;
        /// <summary>
        /// The name of the pose. If empty, it will be derived from the animation clips.
        /// </summary>
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
        public AnimationClip OpenAnimationClip => open;
        /// <summary>
        /// The animation clip for when the hand is closed.
        /// </summary>
        public AnimationClip ClosedAnimationClip => closed;

        /// <summary>
        /// The type of the pose: static (predefined animation) or dynamic (real-time control).
        /// </summary>
        public PoseType Type => type;

        /// <summary>
        /// The type of the pose: static (predefined animation) or dynamic (real-time control).
        /// </summary>
        public enum PoseType
        {
            Dynamic = 0,
            Static = 1
        }

        /// <summary>
        /// Sets the name of the pose if it is currently empty.
        /// </summary>
        /// <param name="name">The new name to set.</param>
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
        /// <param name="type">The new pose type.</param>
        public void SetType(PoseType type)
        {
            this.type = type;
        }
    }
}
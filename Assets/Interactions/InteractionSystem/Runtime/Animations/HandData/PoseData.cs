using System;
using UnityEngine;

namespace Kandooz.InteractionSystem.Animations
{
    [Serializable]
    public struct PoseData
    {
        [SerializeField] private AnimationClip open;
        [SerializeField] private AnimationClip closed;
        [SerializeField] private string name;
        [SerializeField] private PoseType type;
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
        public AnimationClip OpenAnimationClip => open;
        public AnimationClip ClosedAnimationClip => closed;

        public PoseType Type => type;

        public enum PoseType
        {
            Dynamic = 0,
            Static = 1
        }

        public void SetPosNameIfEmpty(string name)
        {
            if (this.name == "")
            {
                this.name = name;
            }
        }

        public void SetType(PoseType type)
        {
            this.type = type;
        }
    }
}
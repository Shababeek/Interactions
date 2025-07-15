using System.Collections.Generic;
using Shababeek.Interactions.Core;
using UnityEngine;
using UnityEngine.Serialization;


namespace Shababeek.Interactions.Animations
{
    /// <summary>
    /// Container for avatar masks for each finger, used for hand animation masking.
    /// </summary>
    [System.Serializable]
    public class HandAvatarMaskContainer
    {
        [SerializeField] private AvatarMask thumb;
        [SerializeField] private AvatarMask index;
        [SerializeField] private AvatarMask middle;
        [SerializeField] private AvatarMask ring;
        [SerializeField] private AvatarMask pinky;

        public AvatarMask this[int i]
        {
            get
            {
                var mask = thumb;
                switch (i)
                {
                    case 0:
                        mask = thumb;
                        break;
                    case 1:
                        mask = index;
                        break;
                    case 2:
                        mask = middle;
                        break;
                    case 3:
                        mask = ring;
                        break;
                    case 4:
                        mask = pinky;
                        break;
                }

                return mask;
            }
        }
    }

    /// <summary>
    /// ScriptableObject that contains all hand pose data, avatar masks, and prefab references for a hand.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Interaction System/Hand Data")]
    public class HandData : ScriptableObject, IAvatarMaskIndexer
    {
        [Tooltip("The Hand must have HandAnimationController Script attached")] [HideInInspector] [SerializeField]
        private HandPoseController leftHandPrefab;

        [HideInInspector] [SerializeField] private HandPoseController rightHandPrefab;

        [Header("Default pose clips")] [HideInInspector] [SerializeField]
        private PoseData defaultPose;

        [Header("Custom Poses")] [HideInInspector] [SerializeField]
        private List<PoseData> poses;

        [HideInInspector] [SerializeField] private HandAvatarMaskContainer handAvatarMaskContainer;
        private PoseData[] posesArray;

        public AvatarMask this[int i] => handAvatarMaskContainer[i];
        public AvatarMask this[FingerName i] => handAvatarMaskContainer[(int)i];
        public PoseData DefaultPose => defaultPose;
        public HandPoseController LeftHandPrefab => leftHandPrefab;
        public HandPoseController RightHandPrefab => rightHandPrefab;

        public PoseData[] Poses
        {
            get
            {
                //if (posesArray != null && posesArray.Length == poses.Count + 1) return posesArray;
                posesArray = new PoseData[poses.Count + 1];
                posesArray[0] = defaultPose;
                defaultPose.Name = "Default";
                defaultPose.SetType(PoseData.PoseType.Dynamic);
                for (var i = 0; i < poses.Count; i++) posesArray[i + 1] = poses[i];
                return posesArray;
            }
        }
    }

    /// <summary>
    /// Interface for accessing avatar masks by index.
    /// </summary>
    public interface IAvatarMaskIndexer
    {
        public AvatarMask this[int i] { get; }
    }
}
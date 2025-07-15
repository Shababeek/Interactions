using System.Collections.Generic;
using Kandooz.InteractionSystem.Core;
using Kinteractions_VR.Core.Runtime.Hand;
using UnityEngine;


namespace Kandooz.InteractionSystem.Animations
{
    [System.Serializable]
    public class HandAvatarMaskContainer
    {

        [SerializeField] private AvatarMask thumbAvatarMask;

        [SerializeField] private AvatarMask indexAvatarMask;
        [SerializeField] private AvatarMask middleAvatarMask;
        [SerializeField] private AvatarMask ringAvatarMask;
        [SerializeField] private AvatarMask pinkyAvatarMask;

        public AvatarMask this[int i]
        {
            get
            {
                AvatarMask mask = thumbAvatarMask;
                switch (i)
                {
                    case 0:
                        mask = thumbAvatarMask;
                        break;
                    case 1:
                        mask = indexAvatarMask;
                        break;
                    case 2:
                        mask = middleAvatarMask;
                        break;
                    case 3:
                        mask = ringAvatarMask;
                        break;
                    case 4:
                        mask = pinkyAvatarMask;
                        break;
                }

                return mask;
            }
        }

    }

    [CreateAssetMenu(menuName = "Kandooz/KVR/Hand Data")]
    public class HandPoseData : ScriptableObject, IAvatarMaskIndexer
    {
        [Tooltip("The Hand must have HandAnimationController Script attached")]
        [SerializeField] private HandPoseController leftHandPrefab;
        [SerializeField] private HandPoseController rightHandPrefab;
        public HandPoseController LeftHandPrefab => leftHandPrefab;
        public HandPoseController RightHandPrefab => rightHandPrefab;
        [Header("Default pose clips")]
        [HideInInspector]
        [SerializeField]
        private PoseData defaultPose;

        [Header("Custom Poses")] [HideInInspector] [SerializeField]
        private List<PoseData> poses;

        [SerializeField] private HandAvatarMaskContainer handAvatarMaskContainer;
        private PoseData[] posesArray;

        public AvatarMask this[int i] => handAvatarMaskContainer[i];
        public AvatarMask this[FingerName i] => handAvatarMaskContainer[(int)i];
        public PoseData DefaultPose => defaultPose;

        public PoseData[] Poses
        {
            get
            {
                //if (posesArray != null && posesArray.Length == poses.Count + 1) return posesArray;
                posesArray = new PoseData[poses.Count + 1];
                posesArray[0] = defaultPose;
                defaultPose.Name = "Default";
                defaultPose.SetType(PoseData.PoseType.Dynamic);
                for (int i = 0; i < poses.Count; i++)
                {
                    posesArray[i + 1] = poses[i];
                }
                return posesArray;
            }
        }
    }

    public interface IAvatarMaskIndexer
    {
        public AvatarMask this[int i] { get; }
    }
}
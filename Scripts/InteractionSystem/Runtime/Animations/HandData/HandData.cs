using System.Collections.Generic;
using Shababeek.Interactions.Core;
using UnityEngine;


namespace Shababeek.Interactions.Animations
{
    
    /// <summary>Container for avatar masks for each finger.</summary>
    [System.Serializable]
    public class HandAvatarMaskContainer
    {
        [Header("Finger Avatar Masks")]
        [Tooltip("Avatar mask for the thumb finger.")]
        [SerializeField] private AvatarMask thumb;

        [Tooltip("Avatar mask for the index finger.")]
        [SerializeField] private AvatarMask index;

        [Tooltip("Avatar mask for the middle finger.")]
        [SerializeField] private AvatarMask middle;

        [Tooltip("Avatar mask for the ring finger.")]
        [SerializeField] private AvatarMask ring;

        [Tooltip("Avatar mask for the pinky finger.")]
        [SerializeField] private AvatarMask pinky;

        /// <summary>Gets avatar mask by numeric index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).</summary>
        public AvatarMask this[int i]
        {
            get
            {
                var mask = i switch
                {
                    0 => thumb,
                    1 => index,
                    2 => middle,
                    3 => ring,
                    4 => pinky,
                    _ => thumb
                };

                return mask;
            }
        }
    }

    /// <summary>Selects which pose system HandPoseController uses to drive the fingers.</summary>
    public enum HandPoseSystem
    {
        /// <summary>Legacy per-finger AvatarMask system. Requires masks authored against the hand's skeleton; does not support animation retargeting.</summary>
        LegacyBoneBased = 0,
        /// <summary>Humanoid muscle-space system. Reads muscle values from humanoid clips and drives finger muscles directly; supports animation retargeting.</summary>
        MuscleBased = 1,
    }

    /// <summary>Hand pose data, avatar masks, and prefab references.</summary>
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Hand Data")]
    public class HandData : ScriptableObject, IAvatarMaskIndexer
    {
        [Header("Visual Preview")]
        [Tooltip("Preview image for this hand, shown in setup wizard and UI.")]
        public Texture2D previewSprite;

        [Header("Description")]
        [Tooltip("Description of this hand data asset for organization purposes.")]
        [SerializeField] private string description;

        [Header("Pose System")]
        [Tooltip("Which pose system to drive the fingers with. LegacyBoneBased uses per-finger AvatarMasks on a custom skeleton. MuscleBased reads humanoid muscle values from clips and requires a Humanoid rig; it supports animation retargeting but ignores the AvatarMasks below.")]
        [SerializeField] private HandPoseSystem poseSystem = HandPoseSystem.LegacyBoneBased;

        [Header("Hand Prefabs")]
        [Tooltip("The left hand prefab must have HandAnimationController Script attached")]
        [HideInInspector] [SerializeField] private HandPoseController leftHandPrefab;

        [Tooltip("The right hand prefab must have HandAnimationController Script attached")]
        [HideInInspector] [SerializeField] private HandPoseController rightHandPrefab;

        [Header("Pose Configuration")]
        [Tooltip("The default pose that will be used when no specific pose is selected.")]
        [HideInInspector] [SerializeField] private PoseData defaultPose;

        [Tooltip("List of custom poses that can be selected and used by the hand.")]
        [HideInInspector] [SerializeField] private List<PoseData> poses;

        [Header("Avatar Masks")]
        [Tooltip("Container for avatar masks for each finger of the hand.")]
        [HideInInspector] [SerializeField] private HandAvatarMaskContainer handAvatarMaskContainer;
        
        private PoseData[] posesArray;
        
        /// <inheritdoc/>
        public AvatarMask this[int i] => handAvatarMaskContainer[i];
        
        /// <inheritdoc/>
        public AvatarMask this[FingerName i] => handAvatarMaskContainer[(int)i];
        
        /// <summary>Which pose system HandPoseController should drive the fingers with. Legacy paths keep working by default; switch to MuscleBased to opt into humanoid retargeting.</summary>
        public HandPoseSystem PoseSystem => poseSystem;

        /// <summary>Gets the default pose for this hand.</summary>
        public PoseData DefaultPose => defaultPose;

        /// <summary>Gets the left hand prefab.</summary>
        public HandPoseController LeftHandPrefab => leftHandPrefab;

        /// <summary>Gets the right hand prefab.</summary>
        public HandPoseController RightHandPrefab => rightHandPrefab;

        /// <summary>Gets the array of all poses (default pose at index 0, followed by custom poses).</summary>
        public PoseData[] Poses
        {
            get
            {
                if (posesArray != null && posesArray.Length > 0) 
                    return posesArray;
                
                var validPoses = new List<PoseData>();
                
                // Validate and add default pose
                if (ValidatePose(defaultPose, "Default"))
                {
                    defaultPose.Name = "Default";
                    defaultPose.SetType(PoseData.PoseType.Dynamic);
                    validPoses.Add(defaultPose);
                }
                else
                {
                    Debug.LogError($"[HandData] Default pose is invalid in {name}. This will cause issues!", this);
                }
                
                // Validate and add custom poses
                for (var i = 0; i < poses.Count; i++)
                {
                    if (ValidatePose(poses[i], $"Pose {i}"))
                    {
                        validPoses.Add(poses[i]);
                    }
                }
                
                posesArray = validPoses.ToArray();
                return posesArray;
            }
        }

        private bool ValidatePose(PoseData pose, string poseName)
        {
            bool isValid = true;
            bool muscleBased = poseSystem == HandPoseSystem.MuscleBased;

            // Static poses only need the open clip
            if (pose.Type == PoseData.PoseType.Static)
            {
                if (pose.OpenAnimationClip == null)
                {
                    Debug.LogWarning($"[HandData] {poseName} in {name} is missing OpenAnimationClip. This pose will be skipped.", this);
                    isValid = false;
                }
            }
            // Dynamic poses need both clips, except in muscle-based mode where both-null falls
            // back to a procedural open/close default.
            else if (pose.Type == PoseData.PoseType.Dynamic)
            {
                bool openMissing = pose.OpenAnimationClip == null;
                bool closedMissing = pose.ClosedAnimationClip == null;
                bool useProceduralFallback = muscleBased && openMissing && closedMissing;

                if (!useProceduralFallback)
                {
                    if (openMissing)
                    {
                        Debug.LogWarning($"[HandData] {poseName} in {name} is missing OpenAnimationClip. This pose will be skipped.", this);
                        isValid = false;
                    }
                    if (closedMissing)
                    {
                        Debug.LogWarning($"[HandData] {poseName} in {name} is missing ClosedAnimationClip. This pose will be skipped.", this);
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        /// <summary>Gets the description of this hand data asset.</summary>
        public string Description => description;

        /// <summary>Invalidates the cached poses array.</summary>
        public void InvalidatePoseCache()
        {
            posesArray = null;
        }

        private void OnValidate()
        {
            InvalidatePoseCache();
        }

        /// <summary>Gets all avatar masks as an array.</summary>
        public AvatarMask[] GetAvatarMasks()
        {
            return new []{this[0],this[1],this[2],this[3],this[4]};
        }
    }

    /// <summary>Interface for accessing avatar masks by index.</summary>
    public interface IAvatarMaskIndexer
    {
        /// <summary>Gets avatar mask by numeric index.</summary>
        public AvatarMask this[int i] { get; }

        /// <summary>Gets avatar mask by finger name.</summary>
        public AvatarMask this[FingerName i] { get; }
    }
}
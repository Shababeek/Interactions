using System.Collections.Generic;
using Shababeek.Interactions.Core;
using UnityEngine;


namespace Shababeek.Interactions.Animations
{
    
    /// <summary>
    /// Container for avatar masks for each finger of a hand.
    /// Provides indexer access to individual finger masks.
    /// </summary>
    /// <remarks>
    /// This class holds AvatarMask references for each finger (thumb, index, middle, ring, pinky)
    /// and provides convenient access through indexers using either numeric indices or FingerName enums.
    /// </remarks>
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

        /// <summary>
        /// Indexer that provides access to avatar masks by numeric index.
        /// </summary>
        /// <param name="i">The finger index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky)</param>
        /// <returns>The avatar mask for the specified finger</returns>
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
    /// <remarks>
    /// This scriptable object holds the default pose, custom poses, and references to the hand prefabs for both left and right hands.
    /// It also provides an indexer to access avatar masks by index or finger name.
    /// </remarks>
    /// <seealso cref="PoseData"/>
    /// <seealso cref="HandAvatarMaskContainer"/>
    /// <seealso cref="IAvatarMaskIndexer"/>
    /// <seealso cref="HandPoseController"/>
    /// <seealso cref="FingerName"/>
    [CreateAssetMenu(menuName = "Shababeek/Interaction System/Hand Data")]
    public class HandData : ScriptableObject, IAvatarMaskIndexer
    {
        [Header("Visual Preview")]
        [Tooltip("Preview image for this hand, shown in setup wizard and UI.")]
        public Texture2D previewSprite;
        
        [Header("Description")]
        [Tooltip("Description of this hand data asset for organization purposes.")]
        [SerializeField] private string description;
        
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
        
        /// <summary>
        /// Gets the default pose for this hand.
        /// </summary>
        /// <returns>The default pose data</returns>
        public PoseData DefaultPose => defaultPose;
        
        /// <summary>
        /// Gets the left hand prefab with HandPoseController component.
        /// </summary>
        /// <returns>The left hand prefab</returns>
        public HandPoseController LeftHandPrefab => leftHandPrefab;
        
        /// <summary>
        /// Gets the right hand prefab with HandPoseController component.
        /// </summary>
        /// <returns>The right hand prefab</returns>
        public HandPoseController RightHandPrefab => rightHandPrefab;

        /// <summary>
        /// Returns an array of all poses, including the default pose at index 0.
        /// </summary>
        /// <returns>Array containing default pose at index 0, followed by custom poses</returns>
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
            
            // Static poses only need the open clip
            if (pose.Type == PoseData.PoseType.Static)
            {
                if (pose.OpenAnimationClip == null)
                {
                    Debug.LogWarning($"[HandData] {poseName} in {name} is missing OpenAnimationClip. This pose will be skipped.", this);
                    isValid = false;
                }
            }
            // Dynamic poses need both clips
            else if (pose.Type == PoseData.PoseType.Dynamic)
            {
                if (pose.OpenAnimationClip == null)
                {
                    Debug.LogWarning($"[HandData] {poseName} in {name} is missing OpenAnimationClip. This pose will be skipped.", this);
                    isValid = false;
                }
                if (pose.ClosedAnimationClip == null)
                {
                    Debug.LogWarning($"[HandData] {poseName} in {name} is missing ClosedAnimationClip. This pose will be skipped.", this);
                    isValid = false;
                }
            }
            
            return isValid;
        }

        /// <summary>
        /// Gets the description of this hand data asset.
        /// </summary>
        /// <returns>The description text</returns>
        public string Description => description;

        /// <summary>
        /// Invalidates the cached poses array, forcing it to be rebuilt on next access.
        /// Call this when poses are modified in the editor.
        /// </summary>
        public void InvalidatePoseCache()
        {
            posesArray = null;
        }

        private void OnValidate()
        {
            InvalidatePoseCache();
        }

        public AvatarMask[] GetAvatarMasks()
        {
            return new []{this[0],this[1],this[2],this[3],this[4]};
        }
    }

    /// <summary>
    /// Interface for accessing avatar masks by index.
    /// </summary>
    public interface IAvatarMaskIndexer
    {
        /// <summary>
        /// Gets the avatar mask for a finger by numeric index.
        /// </summary>
        /// <param name="i">The finger index</param>
        /// <returns>The avatar mask for the specified finger</returns>
        public AvatarMask this[int i] { get; }

        /// <summary>
        /// Gets the avatar mask for a finger by finger name.
        /// </summary>
        /// <param name="i">The finger name</param>
        /// <returns>The avatar mask for the specified finger</returns>
        public AvatarMask this[FingerName i] { get; }
    }
}
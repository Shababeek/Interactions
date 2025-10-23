using System;
using Shababeek.Interactions.Animations;
using Shababeek.Interactions.Animations.Constraints;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// VR hand managing input, pose constraints, and visual representation.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Hand")]
    public class Hand : MonoBehaviour
    {
        [Header("Hand Configuration")]
        [Tooltip("Identifies whether this hand is the left or right hand in the VR system. This determines input mappings and constraints.")]
        [SerializeField] private HandIdentifier hand;
        
        [Tooltip("Reference to the global configuration that contains input manager and system settings.")]
        [SerializeField] private Config config;
        
        [Tooltip("The hand model GameObject that will be shown or hidden based on interaction state. If not assigned, will be auto-assigned to the first child with any renderer.")]
        [SerializeField] private GameObject handModel;
        
        #region Private Fields

        private IPoseable poseDriver;

        #endregion

        #region Public Properties

        /// <summary>
        /// Hand identifier (Left or Right) for this hand instance.
        /// </summary>
        public HandIdentifier HandIdentifier
        {
            get => hand;
            internal set => hand = value;
        }
        
        /// <summary>
        /// Observable for trigger button state changes.
        /// </summary>
        public IObservable<VRButtonState> OnTriggerTriggerButtonStateChange => config?.InputManager?[hand]?.TriggerObservable;
        
        /// <summary>
        /// Observable for grip button state changes.
        /// </summary>
        public IObservable<VRButtonState> OnGripButtonStateChange => config?.InputManager?[hand]?.GripObservable;
        
        /// <summary>
        /// Finger value by finger name (0-1, where 0 is extended and 1 is curled).
        /// </summary>
        public float this[FingerName index] => config?.InputManager?[hand]?[(int)index] ?? 0f;
        
        /// <summary>
        /// Finger value by index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).
        /// </summary>
        public float this[int index] => config?.InputManager?[hand]?[index] ?? 0f;
        
        internal Config Config
        {
            set => config = value;
        }

        /// <summary>
        /// HandData asset associated with this hand.
        /// </summary>
        public HandData HandData => poseDriver?.HandData;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            poseDriver = GetComponent<IPoseable>();
            AutoAssignHandModel();
        }

        #endregion

        #region Hand Model Management

        private void AutoAssignHandModel()
        {
            if (handModel != null) return;
            
            // Try to find any Renderer in children (MeshRenderer, SkinnedMeshRenderer, etc.)
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                handModel = renderer.gameObject;
            }
            else
            {
                // Fallback: use the first child GameObject
                if (transform.childCount > 0)
                {
                    handModel = transform.GetChild(0).gameObject;
                }
                else
                {
                    Debug.LogWarning($"Hand component on {gameObject.name} could not find a suitable handModel. Please assign one manually.");
                }
            }
        }

        /// <summary>
        /// Toggles the visibility of the hand model.
        /// </summary>
        public void ToggleRenderer(bool enable) => handModel?.gameObject.SetActive(enable);

        #endregion

        #region Pose Constraints

        /// <summary>
        /// Applies pose constraints to this hand.
        /// </summary>
        public void Constrain(IPoseConstrainer constrainer)
        {
            if (poseDriver == null || constrainer == null) return;
            
            switch (hand)
            {
                case HandIdentifier.Left:
                    poseDriver.Constrains = constrainer.LeftPoseConstrains;
                    break;
                case HandIdentifier.Right:
                    poseDriver.Constrains = constrainer.RightPoseConstrains;
                    break;
            }
        }
        
        /// <summary>
        /// Removes all pose constraints from this hand.
        /// </summary>
        public void Unconstrain(IPoseConstrainer constrain)
        {
            if (poseDriver == null) return;
            poseDriver.Constrains = PoseConstrains.Free;
        }

        #endregion

        #region Operators
        /// This provides convenient access to the hand identifier without explicitly accessing the property.
        public static implicit operator HandIdentifier(Hand hand) => hand?.HandIdentifier ?? HandIdentifier.Left;

        #endregion
    }
}
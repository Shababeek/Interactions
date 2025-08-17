using System;
using Shababeek.Interactions.Animations;
using Shababeek.Interactions.Animations.Constraints;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Represents a VR hand in the interaction system, managing input, pose constraints, and visual representation.
    /// </summary>
    /// <remarks>
    /// This class serves as the central hub for hand-related functionality, connecting input systems with
    /// pose constraints and visual feedback. It provides a unified interface for accessing hand input data,
    /// managing hand pose constraints, and controlling hand model visibility. The Hand component automatically
    /// manages input mapping, pose constraints, and hand model visibility based on the configuration.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get the hand component
    /// var hand = GetComponent<Hand>();
    /// 
    /// // Subscribe to button state changes
    /// hand.OnTriggerTriggerButtonStateChange.Subscribe(state => 
    /// {
    ///     if (state == VRButtonState.Pressed)
    ///         Debug.Log("Trigger pressed!");
    /// });
    /// 
    /// // Get finger values
    /// float indexCurl = hand[FingerName.Index];
    /// float thumbCurl = hand[0]; // Using numeric index
    /// 
    /// // Toggle hand visibility
    /// hand.ToggleRenderer(false);
    /// </code>
    /// </example>
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
        /// Gets or sets the hand identifier (Left or Right) for this hand instance.
        /// </summary>
        /// <remarks>
        /// This property determines which input mappings and constraints are applied.
        /// It's automatically set during initialization by the CameraRig component.
        /// </remarks>
        /// <value>The hand identifier indicating whether this is the left or right hand.</value>
        public HandIdentifier HandIdentifier
        {
            get => hand;
            internal set => hand = value;
        }
        
        /// <summary>
        /// Observable stream for trigger button state changes on this hand.
        /// </summary>
        /// <remarks>
        /// Provides real-time updates when the trigger button is pressed or released.
        /// Subscribe to this observable to respond to trigger input changes.
        /// </remarks>
        /// <value>An observable that emits ButtonState changes for the trigger button.</value>
        public IObservable<VRButtonState> OnTriggerTriggerButtonStateChange => config?.InputManager?[hand]?.TriggerObservable;
        
        /// <summary>
        /// Observable stream for grip button state changes on this hand.
        /// </summary>
        /// <remarks>
        /// Provides real-time updates when the grip button is pressed or released.
        /// Subscribe to this observable to respond to grip input changes.
        /// </remarks>
        /// <value>An observable that emits ButtonState changes for the grip button.</value>
        public IObservable<VRButtonState> OnGripButtonStateChange => config?.InputManager?[hand]?.GripObservable;
        
        /// <summary>
        /// Indexer that provides access to individual finger values by finger name.
        /// </summary>
        /// <remarks>
        /// Returns the current value (0-1) for the specified finger, where 0 is fully extended and 1 is fully curled.
        /// This provides a convenient way to access finger curl values using the FingerName enum.
        /// </remarks>
        /// <param name="index">The finger to get the value for (Thumb, Index, Middle, Ring, Pinky).</param>
        /// <returns>A float value between 0 and 1 representing the finger's curl state.</returns>
        public float this[FingerName index] => config?.InputManager?[hand]?[(int)index] ?? 0f;
        
        /// <summary>
        /// Indexer that provides access to individual finger values by numeric index.
        /// </summary>
        /// <remarks>
        /// Returns the current value (0-1) for the specified finger index, where 0 is thumb and 4 is pinky.
        /// This provides a convenient way to access finger curl values using numeric indices.
        /// </remarks>
        /// <param name="index">The finger index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).</param>
        /// <returns>A float value between 0 and 1 representing the finger's curl state.</returns>
        public float this[int index] => config?.InputManager?[hand]?[index] ?? 0f;
        
        /// <summary>
        /// Internal property to set the configuration reference.
        /// </summary>
        /// <remarks>
        /// This is typically called by the system during initialization to establish the connection to the global config.
        /// It's automatically set by the CameraRig component during hand initialization.
        /// </remarks>
        /// <value>The configuration object containing input manager and system settings.</value>
        internal Config Config
        {
            set => config = value;
        }

        /// <summary>
        /// Gets the HandData asset associated with this hand.
        /// </summary>
        /// <remarks>
        /// This property provides access to the hand's pose data, avatar masks, and prefab references.
        /// It's retrieved from the pose driver component.
        /// </remarks>
        /// <value>The HandData asset containing hand poses and configuration.</value>
        public HandData HandData => poseDriver?.HandData;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes the hand component on startup.
        /// </summary>
        /// <remarks>
        /// This method gets the IPoseable component and automatically assigns the hand model
        /// if one hasn't been manually assigned.
        /// </remarks>
        private void Awake()
        {
            poseDriver = GetComponent<IPoseable>();
            AutoAssignHandModel();
        }

        #endregion

        #region Hand Model Management

        /// <summary>
        /// Automatically assigns the hand model GameObject if none is manually assigned.
        /// </summary>
        /// <remarks>
        /// This method tries to find a suitable hand model by looking for Renderer components
        /// in the children. If no renderer is found, it falls back to using the first child GameObject.
        /// </remarks>
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
        /// Toggles the visibility of the hand model renderer.
        /// </summary>
        /// <remarks>
        /// This can be used to hide the hand when it's not needed or when interacting with objects.
        /// The hand model GameObject is activated/deactivated based on the enable parameter.
        /// </remarks>
        /// <param name="enable">True to show the hand model, false to hide it.</param>
        public void ToggleRenderer(bool enable) => handModel?.gameObject.SetActive(enable);

        #endregion

        #region Pose Constraints

        /// <summary>
        /// Applies pose constraints to this hand based on the provided constraint system.
        /// </summary>
        /// <remarks>
        /// The constraints applied depend on whether this is the left or right hand.
        /// This method delegates the constraint application to the pose driver component.
        /// </remarks>
        /// <param name="constrainer">The constraint system that defines the pose constraints to apply.</param>
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
        /// Removes all pose constraints from this hand, allowing free movement.
        /// </summary>
        /// <remarks>
        /// This is typically called when the hand is no longer interacting with constrained objects.
        /// The hand will return to its natural pose without any external constraints.
        /// </remarks>
        /// <param name="constrain">The constraint system to remove (parameter name kept for API consistency).</param>
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
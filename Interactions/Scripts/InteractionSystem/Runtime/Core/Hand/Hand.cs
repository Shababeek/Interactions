using System;
using Shababeek.Interactions.Animations.Constraints;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Represents a VR hand in the interaction system, managing input, pose constraints, and visual representation.
    /// This class serves as the central hub for hand-related functionality, connecting input systems with
    /// pose constraints and visual feedback. It provides a unified interface for accessing hand input data,
    /// managing hand pose constraints, and controlling hand model visibility.
    /// </summary>
    public class Hand : MonoBehaviour
    {
        [Tooltip("Identifies whether this hand is the left or right hand in the VR system.")]
        [SerializeField] private HandIdentifier hand;        
        [Tooltip("Reference to the global configuration that contains input manager and system settings.")]
        [SerializeField] private Config config;
        [Tooltip("The hand model GameObject that will be shown or hidden based on interaction state.")  ]
        [SerializeField] private GameObject handModel;
        private IPoseable poseDriver;
        
        /// <summary>
        /// Gets or sets the hand identifier (Left or Right) for this hand instance.
        /// This property determines which input mappings and constraints are applied.
        /// </summary>
        /// <value>The hand identifier indicating whether this is the left or right hand.</value>
        public HandIdentifier HandIdentifier
        {
            get => hand;
            internal set => hand = value;
        }
        
        /// <summary>
        /// Observable stream for trigger button state changes on this hand.
        /// Provides real-time updates when the trigger button is pressed or released.
        /// </summary>
        /// <value>An observable that emits ButtonState changes for the trigger button.</value>
        public IObservable<VRButtonState> OnTriggerTriggerButtonStateChange => config.InputManager[hand].TriggerObservable;
        
        /// <summary>
        /// Observable stream for grip button state changes on this hand.
        /// Provides real-time updates when the grip button is pressed or released.
        /// </summary>
        /// <value>An observable that emits ButtonState changes for the grip button.</value>
        public IObservable<VRButtonState> OnGripButtonStateChange => config.InputManager[hand].GripObservable;
        
        /// <summary>
        /// Indexer that provides access to individual finger values by finger name.
        /// Returns the current value (0-1) for the specified finger, where 0 is fully extended and 1 is fully curled.
        /// </summary>
        /// <param name="index">The finger to get the value for (Thumb, Index, Middle, Ring, Pinky).</param>
        /// <returns>A float value between 0 and 1 representing the finger's curl state.</returns>
        public float this[FingerName index] => config.InputManager[hand][(int)index];
        
        /// <summary>
        /// Indexer that provides access to individual finger values by numeric index.
        /// Returns the current value (0-1) for the specified finger index, where 0 is thumb and 4 is pinky.
        /// </summary>
        /// <param name="index">The finger index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).</param>
        /// <returns>A float value between 0 and 1 representing the finger's curl state.</returns>
        public float this[int index] => config.InputManager[hand][index];
        
        /// <summary>
        /// Internal property to set the configuration reference.
        /// This is typically called by the system during initialization to establish the connection to the global config.
        /// </summary>
        /// <value>The configuration object containing input manager and system settings.</value>
        internal Config Config
        {
            set => config = value;
        }

        /// <summary>
        /// Toggles the visibility of the hand model renderer.
        /// This can be used to hide the hand when it's not needed or when interacting with objects.
        /// </summary>
        /// <param name="enable">True to show the hand model, false to hide it.</param>
        public void ToggleRenderer(bool enable) => handModel.gameObject.SetActive(enable);


        private void Awake()
        {
            poseDriver = GetComponent<IPoseable>();
            if (handModel == null) handModel = GetComponentInChildren<MeshRenderer>().gameObject;
        }

        /// <summary>
        /// Applies pose constraints to this hand based on the provided constraint system.
        /// The constraints applied depend on whether this is the left or right hand.
        /// </summary>
        /// <param name="constrainer">The constraint system that defines the pose constraints to apply.</param>
        /// <remarks>
        public void Constrain(IPoseConstrainer constrainer)
        {
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
        /// This is typically called when the hand is no longer interacting with constrained objects.
        /// </summary>
        /// <param name="constrain">The constraint system to remove (parameter name kept for API consistency).</param>

        public void Unconstrain(IPoseConstrainer constrain)
        {
            poseDriver.Constrains = PoseConstrains.Free;
        }
        
        /// <summary>
        /// Implicit conversion operator that allows a Hand instance to be used where a HandIdentifier is expected.
        /// This provides convenient access to the hand identifier without explicitly accessing the property.
        /// </summary>
   
        public static implicit operator HandIdentifier(Hand hand) => hand.HandIdentifier;
    }
}
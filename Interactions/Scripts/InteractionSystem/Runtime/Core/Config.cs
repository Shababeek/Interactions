using System.Runtime.CompilerServices;
using Shababeek.Core;
using Shababeek.Interactions.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
[assembly: InternalsVisibleTo("Shababeek.Interactions.Editor")]

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Settings for the old input manager, containing axis names and button IDs.
    /// </summary>
    /// <remarks>
    /// This struct contains all the input mappings needed for the legacy Unity Input Manager.
    /// It provides both axis-based inputs (for triggers and grips) and button-based inputs
    /// (for primary/secondary buttons and debug keys).
    /// </remarks>
    [System.Serializable]
    public struct OldInputManagerSettings
    {
        [Header("Left Hand Input")]
        [Tooltip("Axis name for left hand trigger input. This should match an axis defined in Unity's Input Manager.")]
        public string leftTriggerAxis;
        
        [Tooltip("Axis name for left hand grip input. This should match an axis defined in Unity's Input Manager.")]
        public string leftGripAxis;
        
        [Tooltip("Button name for left hand primary button (usually A button on VR controllers).")]
        public string leftPrimaryButton;
        
        [Tooltip("Button name for left hand secondary button (usually B button on VR controllers).")]
        public string leftSecondaryButton;
        
        [Tooltip("Keyboard key for left hand grip debugging (e.g., 'z' for testing without VR hardware).")]
        public string leftGripDebugKey;
        
        [Tooltip("Keyboard key for left hand trigger debugging (e.g., 'x' for testing without VR hardware).")]
        public string leftTriggerDebugKey;
        
        [Tooltip("Keyboard key for left hand thumb button debugging (e.g., 'c' for testing without VR hardware).")]
        public string leftThumbDebugKey;
        
        [Header("Right Hand Input")]
        [Tooltip("Axis name for right hand trigger input. This should match an axis defined in Unity's Input Manager.")]
        public string rightTriggerAxis;
        
        [Tooltip("Axis name for right hand grip input. This should match an axis defined in Unity's Input Manager.")]
        public string rightGripAxis;
        
        [Tooltip("Button name for right hand primary button (usually A button on VR controllers).")]
        public string rightPrimaryButton;
        
        [Tooltip("Button name for right hand secondary button (usually B button on VR controllers).")]
        public string rightSecondaryButton;
        
        [Tooltip("Keyboard key for right hand grip debugging (e.g., 'n' for testing without VR hardware).")]
        public string rightGripDebugKey;
        
        [Tooltip("Keyboard key for right hand trigger debugging (e.g., 'm' for testing without VR hardware).")]
        public string rightTriggerDebugKey;
        
        [Tooltip("Keyboard key for right hand thumb button debugging (e.g., ',' for testing without VR hardware).")]
        public string rightThumbDebugKey;
        
        /// <summary>
        /// Creates default settings with Shababeek input names.
        /// </summary>
        /// <remarks>
        /// These default values provide a complete set of input mappings that work with
        /// the Shababeek input system. You can modify these in the inspector or create
        /// custom mappings as needed.
        /// </remarks>
        public static OldInputManagerSettings Default => new OldInputManagerSettings
        {
            // Left Hand
            leftTriggerAxis = "Shababeek_Left_Trigger",
            leftGripAxis = "Shababeek_Left_Grip",
            leftPrimaryButton = "Shababeek_Left_PrimaryButton",
            leftSecondaryButton = "Shababeek_Left_SecondaryButton",
            leftGripDebugKey = "Shababeek_Left_Grip_DebugKey",
            leftTriggerDebugKey = "Shababeek_Left_Index_DebugKey",
            leftThumbDebugKey = "Shababeek_Left_Primary_DebugKey",
            
            // Right Hand
            rightTriggerAxis = "Shababeek_Right_Trigger",
            rightGripAxis = "Shababeek_Right_Grip",
            rightPrimaryButton = "Shababeek_Right_PrimaryButton",
            rightSecondaryButton = "Shababeek_Right_SecondaryButton",
            rightGripDebugKey = "Shababeek_Right_Grip_DebugKey",
            rightTriggerDebugKey = "Shababeek_Right_Index_DebugKey",
            rightThumbDebugKey = "Shababeek_Right_Primary_DebugKey"
        };
    }

    /// <summary>
    /// ScriptableObject that holds all configuration settings for the interaction system, including hand data, input, and layers.
    /// </summary>
    /// <remarks>
    /// This is the central configuration asset for the Shababeek interaction system. It contains
    /// all the settings needed for hands, input, physics, and layers. Create this asset through
    /// the Create Asset Menu and configure it according to your project's needs.
    /// </remarks>

    [CreateAssetMenu(menuName = "Shababeek/Interactions/Config")]
    public class Config : ScriptableObject
    {
        [Header("Hand Configuration")]
        [Tooltip("HandData ScriptableObject containing hand poses, prefabs, and avatar masks. Required for the interaction system to function.")]
        [SerializeField] private HandData handData;
        
        [Header("Layer Configuration")]
        [Tooltip("Layer for the left hand, used for physics interactions. This prevents the hand from interacting with itself.")]
        [SerializeField] private int leftHandLayer;
        
        [Tooltip("Layer for the right hand, used for physics interactions. This prevents the hand from interacting with itself.")]
        [SerializeField] private int rightHandLayer;
        
        [Tooltip("Layer for interactable objects. Objects on this layer can be grabbed and manipulated by hands.")]
        [SerializeField] private int interactableLayer;
        
        [Tooltip("Layer for the player/character. Used for physics collision settings to prevent hands from colliding with the player.")]
        [SerializeField] private int playerLayer;
        
        [Header("Input Manager Settings")]
        [Tooltip("Type of input manager to use for the interaction system. InputSystem is recommended for modern projects.")]
        [SerializeField] private InputManagerType inputType = InputManagerType.InputSystem;
        
        [Tooltip("Input action references for the left hand when using the new Input System.")]
        [SerializeField] private HandInputActions leftHandActions;
        
        [Tooltip("Input action references for the right hand when using the new Input System.")]
        [SerializeField] private HandInputActions rightHandActions;
        
        [Header("Old Input Manager Settings")]
        [Tooltip("Input axis and button names for the old input manager. Configure these if using InputManager input type.")]
        [SerializeField] private OldInputManagerSettings oldInputSettings;

        [Header("Editor UI Settings")]
        [Tooltip("StyleSheet for the feedback system UI elements in the editor.")]
        [SerializeField] private StyleSheet feedbackSystemStyleSheet;

        [Header("Hand Physics Settings")]
        [Tooltip("Mass of the hand physics objects. Higher values make hands more stable but less responsive.")]
        [SerializeField] private float handMass = 30f;
        
        [Tooltip("Linear damping for hand physics. Higher values reduce hand movement more quickly.")]
        [SerializeField] private float linearDamping = 5f;
        
        [Tooltip("Angular damping for hand physics. Higher values reduce hand rotation more quickly.")]
        [SerializeField] private float angularDamping = 1f;
        
        [Header("System References")]
        [Tooltip("GameObject that manages the input system. Created automatically when needed.")]
        [ReadOnly, SerializeField] private GameObject gameManager;
        
        [Tooltip("Current input manager instance. Automatically created based on the selected input type.")]
        [ReadOnly, SerializeField] private InputManagerBase inputManager;

        #region Public Properties

        /// <summary>
        /// Gets or sets the layer index for the left hand.
        /// </summary>
        /// <remarks>
        /// This layer is used for physics interactions and prevents the hand from
        /// colliding with itself or other hands.
        /// </remarks>
        public int LeftHandLayer
        {
            get => leftHandLayer;
            internal set => leftHandLayer = value;
        }

        /// <summary>
        /// Gets or sets the layer index for the right hand.
        /// </summary>
        /// <remarks>
        /// This layer is used for physics interactions and prevents the hand from
        /// colliding with itself or other hands.
        /// </remarks>
        public int RightHandLayer
        {
            get => rightHandLayer;
            internal set => rightHandLayer = value;
        }

        /// <summary>
        /// Gets or sets the layer index for interactable objects.
        /// </summary>
        /// <remarks>
        /// Objects on this layer can be grabbed and manipulated by hands.
        /// This should be different from hand layers to enable interactions.
        /// </remarks>
        public int InteractableLayer
        {
            get => interactableLayer;
            internal set => interactableLayer = value;
        }

        /// <summary>
        /// Gets or sets the layer index for the player/character.
        /// </summary>
        /// <remarks>
        /// This layer is used for physics collision settings to prevent hands
        /// from colliding with the player character.
        /// </remarks>
        public int PlayerLayer
        {
            get => playerLayer;
            internal set => playerLayer = value;
        }

        /// <summary>
        /// Gets the HandData asset containing hand poses and prefabs.
        /// </summary>
        /// <remarks>
        /// This asset contains all the information needed for hand visualization,
        /// including poses, avatar masks, and prefab references.
        /// </remarks>
        public HandData HandData => handData;

        /// <summary>
        /// Gets the current input manager instance.
        /// </summary>
        /// <remarks>
        /// This property automatically creates the appropriate input manager based on
        /// the selected input type. The manager is created as a child of a persistent
        /// GameObject to ensure it survives scene changes.
        /// </remarks>
        public InputManagerBase InputManager
        {
            get
            {
                if (inputManager) return inputManager;
                if (gameManager) return CreateInputManager();
                gameManager = new GameObject("VR Manager");
                DontDestroyOnLoad(gameManager);
                return CreateInputManager();
            }
        }

        /// <summary>
        /// Gets the mass value for hand physics objects.
        /// </summary>
        /// <remarks>
        /// Higher mass values make hands more stable but less responsive to input.
        /// Lower values make hands more responsive but potentially unstable.
        /// </remarks>
        public float HandMass => handMass;
        
        /// <summary>
        /// Gets the linear damping value for hand physics.
        /// </summary>
        /// <remarks>
        /// Linear damping affects how quickly hand movement is reduced.
        /// Higher values create more "heavy" feeling hands.
        /// </remarks>
        public float HandLinearDamping => linearDamping;
        
        /// <summary>
        /// Gets the angular damping value for hand physics.
        /// </summary>
        /// <remarks>
        /// Angular damping affects how quickly hand rotation is reduced.
        /// Higher values create more stable hand orientation.
        /// </remarks>
        public float HandAngularDamping => angularDamping;
        
        /// <summary>
        /// Gets the StyleSheet for the feedback system UI.
        /// </summary>
        /// <remarks>
        /// This StyleSheet is used to style the feedback system UI elements
        /// in the Unity editor.
        /// </remarks>
        public StyleSheet FeedbackSystemStyleSheet => feedbackSystemStyleSheet;
        
        /// <summary>
        /// Gets or sets the old input manager settings.
        /// </summary>
        /// <remarks>
        /// These settings are used when the input type is set to InputManager.
        /// They contain axis names and button mappings for the legacy input system.
        /// </remarks>
        public OldInputManagerSettings OldInputSettings
        {
            get => oldInputSettings;
            set => oldInputSettings = value;
        }

        /// <summary>
        /// Gets the currently selected input manager type.
        /// </summary>
        /// <remarks>
        /// This determines which input system is used for hand input.
        /// InputSystem is recommended for modern projects.
        /// </remarks>
        public InputManagerType InputType => inputType;

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates and initializes the appropriate input manager based on the selected input type.
        /// </summary>
        /// <returns>The created or existing input manager instance</returns>
        /// <remarks>
        /// This method automatically creates the correct input manager type and initializes it
        /// with the appropriate settings. It also handles cleanup of old managers.
        /// </remarks>
        private InputManagerBase CreateInputManager()
        {
            switch (inputType)
            {
                case InputManagerType.InputManager:
                    if (inputManager != null && inputManager is AxisBasedInputManager) return inputManager;
                    if (inputManager) Destroy(inputManager);
                    var axisManager = gameManager.AddComponent<AxisBasedInputManager>();
                    axisManager.Initialize(this);
                    inputManager = axisManager;
                    break;
                case InputManagerType.InputSystem:
                    if (inputManager != null && inputManager is NewInputSystemBasedInputManager) return inputManager;
                    if (inputManager) Destroy(inputManager);
                    var manager = gameManager.AddComponent<NewInputSystemBasedInputManager>();
                    manager.Initialize(leftHandActions, rightHandActions);
                    inputManager = manager;
                    break;
            }

            return inputManager;
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Struct containing input action references for hand input using the new Input System.
        /// </summary>
        /// <remarks>
        /// This struct holds references to InputAction assets for each finger on a hand.
        /// These actions should be configured in your Input Action Asset to map to
        /// the appropriate controller inputs.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Configure input actions in the inspector
        /// leftHandActions.thumbAction = thumbActionAsset;
        /// leftHandActions.indexAction = indexActionAsset;
        /// </code>
        /// </example>
        [System.Serializable]
        public struct HandInputActions
        {
            [Header("Finger Actions")]
            [Tooltip("Input action for the thumb finger (usually grip or primary button).")]
            [SerializeField] private InputActionReference thumbAction;
            
            [Tooltip("Input action for the index finger (usually trigger).")]
            [SerializeField] private InputActionReference indexAction;
            
            [Tooltip("Input action for the middle finger.")]
            [SerializeField] private InputActionReference middleAction;
            
            [Tooltip("Input action for the ring finger.")]
            [SerializeField] private InputActionReference ringAction;
            
            [Tooltip("Input action for the pinky finger.")]
            [SerializeField] private InputActionReference pinkyAction;

            /// <summary>
            /// Gets the InputAction for the thumb finger.
            /// </summary>
            public InputAction ThumbAction => thumbAction?.action;
            
            /// <summary>
            /// Gets the InputAction for the index finger.
            /// </summary>
            public InputAction IndexAction => indexAction?.action;
            
            /// <summary>
            /// Gets the InputAction for the middle finger.
            /// </summary>
            public InputAction MiddleAction => middleAction?.action;
            
            /// <summary>
            /// Gets the InputAction for the ring finger.
            /// </summary>
            public InputAction RingAction => ringAction?.action;
            
            /// <summary>
            /// Gets the InputAction for the pinky finger.
            /// </summary>
            public InputAction PinkyAction => pinkyAction?.action;
        }

        /// <summary>
        /// Enum defining the available input manager types.
        /// </summary>
        /// <remarks>
        /// Choose between the legacy Unity Input Manager (Axis-based) and the modern
        /// Input System. The Input System is recommended for new projects.
        /// </remarks>
        public enum InputManagerType
        {
            //TODO: Move to the input System Based Input Manager
            /// <summary>
            /// Legacy Unity Input Manager using axis and button names.
            /// </summary>
            InputManager = 0,
            
            /// <summary>
            /// Modern Unity Input System using InputAction assets.
            /// </summary>
            InputSystem = 1,
        }

        #endregion
    }
}
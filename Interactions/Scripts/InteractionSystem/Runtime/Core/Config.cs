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
    [System.Serializable]
    public struct OldInputManagerSettings
    {
        [Header("Left Hand Input")]
        [Tooltip("Axis name for left hand trigger")]
        public string leftTriggerAxis;
        [Tooltip("Axis name for left hand grip")]
        public string leftGripAxis;
        [Tooltip("Button name for left hand primary button")]
        public string leftPrimaryButton;
        [Tooltip("Button name for left hand secondary button")]
        public string leftSecondaryButton;
        [Tooltip("Debug key for left hand grip")]
        public string leftGripDebugKey;
        [Tooltip("Debug key for left hand trigger")]
        public string leftTriggerDebugKey;
        [Tooltip("Debug key for left hand thumb")]
        public string leftThumbDebugKey;
        
        [Header("Right Hand Input")]
        [Tooltip("Axis name for right hand trigger")]
        public string rightTriggerAxis;
        [Tooltip("Axis name for right hand grip")]
        public string rightGripAxis;
        [Tooltip("Button name for right hand primary button")]
        public string rightPrimaryButton;
        [Tooltip("Button name for right hand secondary button")]
        public string rightSecondaryButton;
        [Tooltip("Debug key for right hand grip")]
        public string rightGripDebugKey;
        [Tooltip("Debug key for right hand trigger")]
        public string rightTriggerDebugKey;
        [Tooltip("Debug key for right hand thumb")]
        public string rightThumbDebugKey;
        
        /// <summary>
        /// Creates default settings with Shababeek input names.
        /// </summary>
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
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Config")]
    public class Config : ScriptableObject
    {
        [Tooltip("you need to create a HandData ScriptableObject for all hands you want to use in the interaction system., instructions are in teh documentation.")]
        [SerializeField] private HandData handData;
        [Header("Layers")]
        [Tooltip("Layer for the left hand, used for physics interactions, needed to make sure the hand Does not interact with itself.")]
        [SerializeField] private int leftHandLayer;
        [Tooltip("Layer for the right hand, used for physics interactions,needed to make sure the hand Does not interact with itself")]
        [SerializeField] private int rightHandLayer;
        [Tooltip("Layer for interactable objects, used for physics interactions .")]
        [SerializeField] private int interactableLayer;
        [SerializeField] private int playerLayer;
        [Header("Input Manager Settings")]
        [Tooltip("Type of input manager to use for the interaction system. Options are Unity Axis")]
        [SerializeField] private InputManagerType inputType = InputManagerType.InputSystem;
        [SerializeField] private HandInputActions leftHandActions;
        [SerializeField] private HandInputActions rightHandActions;
        
        [Header("Old Input Manager Settings")]
        [Tooltip("Input axis and button names for the old input manager")]
        [SerializeField] private OldInputManagerSettings oldInputSettings;

        [Header("Editor UI Settings")]
        [SerializeField] private StyleSheet feedbackSystemStyleSheet;

        [Header("Hand Physics Data")] [SerializeField]
        private float handMass = 30;

        [SerializeField] private float linearDamping = 5, angularDamping = 1;
        [ReadOnly, SerializeField] private GameObject gameManager;

        [ReadOnly] [SerializeField] private InputManagerBase inputManager;
        public int LeftHandLayer
        {
            get => leftHandLayer;
            internal set => leftHandLayer = value;
        }

        public int RightHandLayer
        {
            get => rightHandLayer;
            internal set => rightHandLayer = value;
        }

        public int InteractableLayer
        {
            get => interactableLayer;
            internal set => interactableLayer = value;
        }

        public int PlayerLayer
        {
            get => playerLayer;
            internal set => playerLayer = value;
        }

        public HandData HandData => handData;

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

        public float HandMass => handMass;
        public float HandLinearDamping => linearDamping;
        public float HandAngularDamping => angularDamping;
        public StyleSheet FeedbackSystemStyleSheet => feedbackSystemStyleSheet;
        public OldInputManagerSettings OldInputSettings
        {
            get => oldInputSettings;
            set => oldInputSettings = value;
        }
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

        /// <summary>
        /// Struct containing input action references for hand input.
        /// </summary>
        [System.Serializable]
        public struct HandInputActions
        {
            [SerializeField] private InputActionReference thumbAction;
            [SerializeField] private InputActionReference indexAction;
            [SerializeField] private InputActionReference middleAction;
            [SerializeField] private InputActionReference ringAction;
            [SerializeField] private InputActionReference pinkyAction;
            public InputAction ThumbAction => thumbAction.action;
            public InputAction IndexAction => indexAction.action;
            public InputAction MiddleAction => middleAction.action;
            public InputAction RingAction => ringAction.action;
            public InputAction PinkyAction => pinkyAction.action;
        }

     

        private enum InputManagerType
        {
            InputManager = 0,
            InputSystem = 1,
        }
    }


}
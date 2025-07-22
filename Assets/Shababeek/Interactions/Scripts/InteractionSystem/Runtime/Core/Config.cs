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
    /// ScriptableObject that holds all configuration settings for the interaction system, including hand data, input, and layers.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Config")]
    public class Config : ScriptableObject
    {
        [Tooltip("you need to create a HandData ScriptableObject for all hands you want to use in the interaction system., instructions are in teh documentation.")]
        [SerializeField] private HandData handData;
        [Header("Layers")]
        [Tooltip("Layer for the left hand, used for physics interactions, needed to make sure the hand Does not interact with itself.")]
        [SerializeField] private LayerMask leftHandLayer;
        [Tooltip("Layer for the right hand, used for physics interactions,needed to make sure the hand Does not interact with itself")]
        [SerializeField] private LayerMask rightHandLayer;
        [Tooltip("Layer for interactable objects, used for physics interactions .")]
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask playerLayer;
        [Header("Input Manager Settings")]
        [Tooltip("Type of input manager to use for the interaction system. Options are Unity Axis")]
        [SerializeField] private InputManagerType inputType = InputManagerType.InputSystem;
        [SerializeField] private HandInputActions leftHandActions;
        [SerializeField] private HandInputActions rightHandActions;

        [Header("Editor UI Settings")]
        [SerializeField] private StyleSheet feedbackSystemStyleSheet;

        [Header("Hand Physics Data")] [SerializeField]
        private float handMass = 30;

        [SerializeField] private float linearDamping = 5, angularDamping = 1;
        [ReadOnly, SerializeField] private GameObject gameManager;

        [ReadOnly] [SerializeField] private InputManagerBase inputManager;
        public int LeftHandLayer
        {
            get => (int)(Mathf.Log(leftHandLayer, 2) + .5f);
            internal set => leftHandLayer = value;
        }

        public int RightHandLayer
        {
            get => (int)(Mathf.Log(rightHandLayer, 2) + .5f);
            internal set => rightHandLayer = value;
        }

        public int InteractableLayer
        {
            get => (int)(Mathf.Log(interactableLayer, 2) + .5f);
            internal set => interactableLayer = value;
        }


        public int PlayerLayer
        {
            get => (int)(Mathf.Log(playerLayer, 2) + .5f);
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

        private InputManagerBase CreateInputManager()
        {
            switch (inputType)
            {
                case InputManagerType.UnityAxisBased:
                    if (inputManager != null && inputManager is AxisBasedInputManager) return inputManager;
                    if (inputManager) Destroy(inputManager);
                    inputManager = gameManager.AddComponent<AxisBasedInputManager>();
                    break;
                case InputManagerType.InputSystem:
                    if (inputManager != null && inputManager is NewInputSystemBasedInputManager) return inputManager;
                    if (inputManager) Destroy(inputManager);
                        var manager = gameManager.AddComponent<NewInputSystemBasedInputManager>();
                    manager.Initialize(leftHandActions, rightHandActions);
                    inputManager = manager;

                    break;
                case InputManagerType.KeyboardMock:
                    if (inputManager != null && inputManager is KeyboardBasedInput) return inputManager;
                    if (inputManager) Destroy(inputManager);
                    inputManager = gameManager.AddComponent<KeyboardBasedInput>();

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
            UnityAxisBased = 0,
            InputSystem = 1,
            KeyboardMock = -1
        }
    }


}
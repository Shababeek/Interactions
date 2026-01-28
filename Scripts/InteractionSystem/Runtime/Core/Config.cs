using System;
using System.Runtime.CompilerServices;
using Shababeek.Utilities;
using Shababeek.Interactions.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[assembly: InternalsVisibleTo("Shababeek.Interactions.Editor")]

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Configuration settings for the interaction system including hand data, input, and layers.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Config")]
    public class Config : ScriptableObject
    {
        [Header("Hand Configuration")]
        [Tooltip(
            "HandData ScriptableObject containing hand poses, prefabs, and avatar masks. Required for the interaction system to function.")]
        [SerializeField]
        private HandData handData;

        [Header("Layer Configuration")]
        [Tooltip(
            "Layer for the left hand, used for physics interactions. This prevents the hand from interacting with itself.")]
        [SerializeField]
        private int leftHandLayer;

        [Tooltip(
            "Layer for the right hand, used for physics interactions. This prevents the hand from interacting with itself.")]
        [SerializeField]
        private int rightHandLayer;

        [Tooltip("Layer for interactable objects. Objects on this layer can be grabbed and manipulated by hands.")]
        [SerializeField]
        private int interactableLayer;

        [Tooltip(
            "Layer for the player/character. Used for physics collision settings to prevent hands from colliding with the player.")]
        [SerializeField]
        private int playerLayer;

        [Header("Input Manager Settings")]
        [Tooltip(
            "Type of input manager to use for the interaction system. InputSystem is recommended for modern projects.")]
        [SerializeField]
        private TrackingType inputType = TrackingType.ControllerTracking;

        [Tooltip("Input action references for the left hand when using the new Input System.")] [SerializeField]
        private HandInputActions leftHandActions;

        [Tooltip("Input action references for the right hand when using the new Input System.")] [SerializeField]
        private HandInputActions rightHandActions;

        [Header("Editor UI Settings")]
        [Tooltip("StyleSheet for the feedback system UI elements in the editor.")]
        [SerializeField]
        private StyleSheet feedbackSystemStyleSheet;

        [Header("Hand Physics Settings")]
        [Tooltip("Mass of the hand physics objects. Higher values make hands more stable but less responsive.")]
        [SerializeField]
        private float handMass = 30f;

        [Tooltip("Linear damping for hand physics. Higher values reduce hand movement more quickly.")] [SerializeField]
        private float linearDamping = 5f;

        [Tooltip("Angular damping for hand physics. Higher values reduce hand rotation more quickly.")] [SerializeField]
        private float angularDamping = 1f;

        [Header("Hand Input Providers")] [Tooltip("Input provider for the left hand.")] [SerializeField]
        private MonoBehaviour leftHandInputProvider;

        [Tooltip("Input provider for the right hand.")] [SerializeField]
        private MonoBehaviour rightHandInputProvider;

        private IHandInputProvider _leftProvider;
        private IHandInputProvider _rightProvider;
        #region Public Properties

        /// <summary>
        /// Gets the input provider for the left hand.
        /// </summary>
        public IHandInputProvider LeftHandProvider
        {
            get
            {
                if (_leftProvider == null && leftHandInputProvider != null)
                {
                    _leftProvider = leftHandInputProvider as IHandInputProvider;
                }

                return _leftProvider;
            }
        }

        /// <summary>
        /// Gets the input provider for the right hand.
        /// </summary>
        public IHandInputProvider RightHandProvider
        {
            get
            {
                if (_rightProvider == null && rightHandInputProvider != null)
                {
                    _rightProvider = rightHandInputProvider as IHandInputProvider;
                }

                return _rightProvider;
            }
        }

        /// <summary>
        /// Layer index for the left hand.
        /// </summary>
        public int LeftHandLayer
        {
            get => leftHandLayer;
            internal set => leftHandLayer = value;
        }

        /// <summary>
        /// Layer index for the right hand.
        /// </summary>
        public int RightHandLayer
        {
            get => rightHandLayer;
            internal set => rightHandLayer = value;
        }

        /// <summary>
        /// Layer index for interactable objects.
        /// </summary>
        public int InteractableLayer
        {
            get => interactableLayer;
            internal set => interactableLayer = value;
        }

        /// <summary>
        /// Layer index for the player/character.
        /// </summary>
        public int PlayerLayer
        {
            get => playerLayer;
            internal set => playerLayer = value;
        }

        /// <summary>
        /// HandData asset containing hand poses and prefabs.
        /// </summary>
        public HandData HandData => handData;

        /// <summary>
        /// Mass value for hand physics objects.
        /// </summary>
        public float HandMass => handMass;

        /// <summary>
        /// Linear damping value for hand physics.
        /// </summary>
        public float HandLinearDamping => linearDamping;

        /// <summary>
        /// Angular damping value for hand physics.
        /// </summary>
        public float HandAngularDamping => angularDamping;

        /// <summary>
        /// StyleSheet for the feedback system UI.
        /// </summary>
        public StyleSheet FeedbackSystemStyleSheet => feedbackSystemStyleSheet;

        /// <summary>
        /// Currently selected input manager type.
        /// </summary>
        public TrackingType InputType => inputType;

        public HandInputActions LeftHandActions => leftHandActions;
        public HandInputActions RightHandActions => rightHandActions;

        #endregion

        public IHandInputProvider this[HandIdentifier hand] =>
            hand switch
            {
                HandIdentifier.Left => LeftHandProvider,
                HandIdentifier.Right => RightHandProvider,
                _ => null
            };

        #region Public Methods
/// <summary>
/// Sets the input provider for a specific hand at runtime.
/// </summary>
public void SetHandProvider(HandIdentifier hand, IHandInputProvider provider)
{
    if (hand == HandIdentifier.Left)
    {
        leftHandInputProvider = provider as MonoBehaviour;
        _leftProvider = provider;
    }
    else
    {
        rightHandInputProvider = provider as MonoBehaviour;
        _rightProvider = provider;
    }
}
#endregion
        #region Nested Types

        /// <summary>
        /// Input action references for hand input using the new Input System.
        /// </summary>
        [System.Serializable]
        public struct HandInputActions
        {
            [Header("Finger Actions")]
            [SerializeField] private InputActionReference thumbAction;
            [SerializeField] private InputActionReference indexAction;
            [SerializeField] private InputActionReference middleAction;
            [SerializeField] private InputActionReference ringAction;
            [SerializeField] private InputActionReference pinkyAction;


            public InputAction ThumbAction => thumbAction?.action;

            /// <summary>
            /// InputAction for the index finger.
            /// </summary>
            public InputAction IndexAction => indexAction?.action;

            /// <summary>
            /// InputAction for the middle finger.
            /// </summary>
            public InputAction MiddleAction => middleAction?.action;

            /// <summary>
            /// InputAction for the ring finger.
            /// </summary>
            public InputAction RingAction => ringAction?.action;

            /// <summary>
            /// InputAction for the pinky finger.
            /// </summary>
            public InputAction PinkyAction => pinkyAction?.action;
        }

        /// <summary>
        /// Available input manager types.
        /// </summary>
        public enum TrackingType
        {
            ControllerTracking = 1,
#if XR_HANDS_AVAILABLE
            HandTracking = 2,
            Both = 3
#endif
        }

        #endregion
    }
}
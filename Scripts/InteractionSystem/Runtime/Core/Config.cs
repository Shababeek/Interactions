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

        [Header("Hand Following Settings")]
        [Tooltip("Preset configuration for physics hand following behavior.")]
        [SerializeField]
        private PhysicsFollowerPreset followerPreset = PhysicsFollowerPreset.Standard;

        [Tooltip("Custom settings for physics hand following. Only used when preset is set to Custom.")]
        [SerializeField]
        private PhysicsFollowerSettings customFollowerSettings = PhysicsFollowerSettings.Standard;

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
        /// Physics follower settings based on the selected preset.
        /// </summary>
        public PhysicsFollowerSettings FollowerSettings
        {
            get
            {
                if (followerPreset == PhysicsFollowerPreset.Custom)
                    return customFollowerSettings;

                return PhysicsFollowerSettings.GetPreset(followerPreset);
            }
        }

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
        [Flags]
        public enum TrackingType
        {
     
            ControllerTracking = 1,
            HandTracking = 2,
            Both = 3
       }

        #endregion
    }

  
    #region Physics Follower Presets

    public enum PhysicsFollowerPreset
    {
        Standard,
        Responsive,
        Smooth,
        Precise,
        Custom
    }

    [System.Serializable]
    public struct PhysicsFollowerSettings
    {
        [Tooltip("Strength multiplier for position following. Higher values = faster response.")]
        public float positionStrength;

        [Tooltip("Strength multiplier for rotation following. Higher values = faster response.")]
        public float rotationStrength;

        [Tooltip("Maximum velocity magnitude in units per second.")]
        public float maxVelocity;

        [Tooltip("Maximum angular velocity magnitude in radians per second.")]
        public float maxAngularVelocity;

        [Tooltip("Distance threshold below which the hand won't move (reduces micro-jitter).")]
        public float positionDeadzone;

        [Tooltip("Angle threshold in degrees below which the hand won't rotate (reduces micro-jitter).")]
        public float rotationDeadzone;

        [Tooltip("Distance threshold above which the hand will teleport instead of follow.")]
        public float teleportDistance;

        [Tooltip("If enabled, stops applying forces when in contact with objects.")]
        public bool respectCollisions;

        /// <summary>
        /// Standard preset balanced for general VR use.
        /// </summary>
        public static PhysicsFollowerSettings Standard => new PhysicsFollowerSettings
        {
            positionStrength = 1000f,
            rotationStrength = 100f,
            maxVelocity = 10f,
            maxAngularVelocity = 20f,
            positionDeadzone = 0.001f,
            rotationDeadzone = 0.5f,
            teleportDistance = 1f,
            respectCollisions = true
        };

        /// <summary>
        /// Responsive preset with snappy, fast response for precise interactions.
        /// </summary>
        public static PhysicsFollowerSettings Responsive => new PhysicsFollowerSettings
        {
            positionStrength = 2000f,
            rotationStrength = 200f,
            maxVelocity = 15f,
            maxAngularVelocity = 30f,
            positionDeadzone = 0.002f,
            rotationDeadzone = 1f,
            teleportDistance = 1f,
            respectCollisions = true
        };

        /// <summary>
        /// Smooth preset with floaty, gradual movement for comfortable experience.
        /// </summary>
        public static PhysicsFollowerSettings Smooth => new PhysicsFollowerSettings
        {
            positionStrength = 500f,
            rotationStrength = 50f,
            maxVelocity = 5f,
            maxAngularVelocity = 10f,
            positionDeadzone = 0.0005f,
            rotationDeadzone = 0.2f,
            teleportDistance = 1f,
            respectCollisions = true
        };

        /// <summary>
        /// Precise preset with slower, controlled movement for delicate manipulation.
        /// </summary>
        public static PhysicsFollowerSettings Precise => new PhysicsFollowerSettings
        {
            positionStrength = 800f,
            rotationStrength = 80f,
            maxVelocity = 7f,
            maxAngularVelocity = 15f,
            positionDeadzone = 0.0005f,
            rotationDeadzone = 0.3f,
            teleportDistance = 0.5f,
            respectCollisions = true
        };

        /// <summary>
        /// Preset settings for a given preset type.
        /// </summary>
        public static PhysicsFollowerSettings GetPreset(PhysicsFollowerPreset preset)
        {
            switch (preset)
            {
                case PhysicsFollowerPreset.Standard:
                    return Standard;
                case PhysicsFollowerPreset.Responsive:
                    return Responsive;
                case PhysicsFollowerPreset.Smooth:
                    return Smooth;
                case PhysicsFollowerPreset.Precise:
                    return Precise;
                case PhysicsFollowerPreset.Custom:
                    return Standard;
                default:
                    return Standard;
            }
        }
    }

    #endregion
}
using Shababeek.Interactions.Animations;
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Defines a custom layer assignment for specific objects in the camera rig.
    /// </summary>
    /// <remarks>
    /// This struct allows you to assign specific layers to specific objects during
    /// camera rig initialization, providing fine-grained control over layer management.
    /// </remarks>
    [System.Serializable]
    public struct LayerAssignment
    {
        [Tooltip("The target object to assign the layer to.")]
        public Transform target;
        
        [Tooltip("The layer index to assign to the target object and all its children.")]
        public int layer;
    }

    /// <summary>
    /// Main component for managing the XR camera rig and hand interactions.
    /// </summary>
    /// <remarks>
    /// The CameraRig component is responsible for setting up the XR environment, managing hand prefabs,
    /// handling layer assignments, and coordinating between different interaction systems. It automatically
    /// initializes hands based on the configuration and manages the relationship between the camera and
    /// the rig's coordinate system.
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Camera Rig")]
    public class CameraRig : MonoBehaviour
    {
        [Header("Core Configuration")]
        [Tooltip("Configuration asset containing hand data, layer settings, and input configuration.")]
        [SerializeField] private Config config;
        
        [Tooltip("Whether to automatically initialize hands when the rig starts. Disable for manual control.")]
        [SerializeField][HideInInspector] private bool initializeHands = true;

        [Header("Tracking Configuration")]
        [Tooltip("The tracking method used for hand interactions. Physics-based provides more realistic physics, Transform-based is more performant.")]
        [SerializeField][HideInInspector] private InteractionSystemType trackingMethod = InteractionSystemType.PhysicsBased;

        [Header("Hand Pivots")]
        [Tooltip("Transform for the left hand pivot point. This defines where the left hand will be positioned relative to the camera rig.")]
        [SerializeField][HideInInspector] private Transform leftHandPivot;
        
        [Tooltip("Transform for the right hand pivot point. This defines where the right hand will be positioned relative to the camera rig.")]
        [SerializeField][HideInInspector] private Transform rightHandPivot;
        
        [Header("Interactor Configuration")]
        [Tooltip("Type of interactor to use for the left hand. Trigger provides direct collision detection, Ray provides distance-based interaction.")]
        [SerializeField] private HandInteractorType leftHandInteractorType = HandInteractorType.Trigger;
        
        [Tooltip("Type of interactor to use for the right hand. Trigger provides direct collision detection, Ray provides distance-based interaction.")]
        [SerializeField] private HandInteractorType rightHandInteractorType = HandInteractorType.Trigger;
        
        [Header("Camera Configuration")]
        [Tooltip("Transform used to offset the camera position and height. Usually a child of the main camera.")]
        [SerializeField] private Transform offsetObject;

        [Tooltip("The XR camera component for the camera rig. Automatically found if not assigned.")]
        [SerializeField] private Camera xrCamera;
        
        [Tooltip("Height offset for the camera rig in world units. Default is 1 unit (typical standing height).")]
        [SerializeField] private float cameraHeight = 1f;
        
        [Tooltip("Whether to align the rig's forward direction with the tracking origin on initialization.")]
        [SerializeField] private bool alignRigForwardOnTracking = true;
        
        [Header("Layer Management")]
        [Tooltip("Whether to automatically initialize layers for the camera rig and hands on startup.")]
        [SerializeField][HideInInspector] private bool initializeLayers = true;

        [Tooltip("Custom layer assignments for specific objects. These will be applied during initialization.")]
        [SerializeField] private LayerAssignment[] customLayerAssignments;

        #region Private Fields

        private bool _trackingInitialized = false;
        private HandPoseController _leftPoseController, _rightPoseController;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the left hand prefab from the configuration's HandData.
        /// </summary>
        /// <remarks>
        /// This prefab is used to instantiate the left hand in the scene when editing pose constraints.
        /// The prefab should contain a HandPoseController component.
        /// </remarks>
        public HandPoseController LeftHandPrefab => config?.HandData?.LeftHandPrefab;
        
        /// <summary>
        /// Gets the right hand prefab from the configuration's HandData.
        /// </summary>
        /// <remarks>
        /// This prefab is used to instantiate the right hand in the scene when editing pose constraints.
        /// The prefab should contain a HandPoseController component.
        /// </remarks>
        public HandPoseController RightHandPrefab => config?.HandData?.RightHandPrefab;
        
        /// <summary>
        /// Gets the configuration asset for the camera rig.
        /// </summary>
        /// <remarks>
        /// Contains all the settings for hands, layers, and input management.
        /// This is the central configuration that drives the entire interaction system.
        /// </remarks>
        public Config Config => config;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes the camera rig and hands on startup.
        /// </summary>
        /// <remarks>
        /// This method automatically finds the XR camera if not assigned, creates and initializes hands,
        /// and sets up the initial layer configuration.
        /// </remarks>
        private void Awake()
        {
            if (xrCamera == null)
                xrCamera = GetComponentInChildren<Camera>(true);
                
            CreateAndInitializeHands();
            InitializeLayers();
        }

        /// <summary>
        /// Sets up the camera offset and subscribes to tracking events when enabled.
        /// </summary>
        /// <remarks>
        /// Applies the initial camera height offset and begins listening for XR tracking origin updates.
        /// </remarks>
        private void OnEnable()
        {
            if (offsetObject != null)
            {
                offsetObject.transform.localPosition = Vector3.up * cameraHeight;
                offsetObject.transform.localRotation = Quaternion.identity;
            }

            SubscribeToTrackingEvents();
        }

        /// <summary>
        /// Unsubscribes from tracking events when disabled.
        /// </summary>
        /// <remarks>
        /// Ensures proper cleanup of event subscriptions to prevent memory leaks.
        /// </remarks>
        private void OnDisable()
        {
            UnsubscribeFromTrackingEvents();
        }

        #endregion

        #region XR Tracking

        /// <summary>
        /// Subscribes to XR tracking origin update events.
        /// </summary>
        /// <remarks>
        /// This method finds all available XR input subsystems and subscribes to their
        /// tracking origin update events to handle camera positioning.
        /// </remarks>
        private void SubscribeToTrackingEvents()
        {
            var subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var subsystem in subsystems)
            {
                subsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
            }
        }

        /// <summary>
        /// Unsubscribes from XR tracking origin update events.
        /// </summary>
        /// <remarks>
        /// Properly cleans up event subscriptions to prevent memory leaks and errors.
        /// </remarks>
        private void UnsubscribeFromTrackingEvents()
        {
            var subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var subsystem in subsystems)
            {
                subsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
            }
        }

        /// <summary>
        /// Handles XR tracking origin updates.
        /// </summary>
        /// <param name="subsystem">The XR input subsystem that triggered the update</param>
        /// <remarks>
        /// This callback is triggered when the XR system's tracking origin changes,
        /// such as when the user recenters their view or changes tracking spaces.
        /// </remarks>
        private void OnTrackingOriginUpdated(XRInputSubsystem subsystem)
        {
            TryApplyCameraOffsetAndAlignment();
        }

        /// <summary>
        /// Attempts to apply camera offset and alignment when tracking is initialized.
        /// </summary>
        /// <remarks>
        /// This method ensures that camera positioning and alignment only happen once
        /// when tracking is first established, preventing unnecessary updates.
        /// </remarks>
        private void TryApplyCameraOffsetAndAlignment()
        {
            if (_trackingInitialized) return;
            _trackingInitialized = true;
            ApplyCameraOffset();
            if (alignRigForwardOnTracking)
                AlignRigForward();
        }

        #endregion

        #region Camera Positioning

        /// <summary>
        /// Applies the configured camera offset to position the camera correctly.
        /// </summary>
        /// <remarks>
        /// This method sets the camera height and adjusts the XZ position to ensure
        /// the camera's world position matches the camera rig's world position.
        /// </remarks>
        private void ApplyCameraOffset()
        {
            if (offsetObject == null) return;
            
            // Set height
            var pos = offsetObject.localPosition;
            pos.y = cameraHeight;

            // Shift XZ so camera's world XZ matches CameraRig's world XZ
            var camera = xrCamera;
            if (camera != null)
            {
                // Calculate the difference in XZ between CameraRig and camera
                Vector3 rigXZ = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 camXZ = new Vector3(camera.transform.position.x, 0, camera.transform.position.z);
                Vector3 deltaXZ = rigXZ - camXZ;
                
                // Apply the delta to the offset object's local XZ
                pos.x += deltaXZ.x;
                pos.z += deltaXZ.z;
            }
            offsetObject.localPosition = pos;
        }

        /// <summary>
        /// Aligns the rig's forward direction with the tracking origin.
        /// </summary>
        /// <remarks>
        /// This method ensures that the camera rig's forward direction matches the
        /// desired orientation, typically aligned with the user's intended forward direction.
        /// </remarks>
        private void AlignRigForward()
        {
            if (offsetObject == null) return;
            
            var rigRoot = transform;
            var camera = xrCamera;
            if (camera == null) return;
            
            // Project forward onto XZ plane
            Vector3 desiredForward = rigRoot.forward;
            desiredForward.y = 0;
            if (desiredForward.sqrMagnitude > 0.001f)
            {
                desiredForward.Normalize();
                Vector3 cameraForward = camera.transform.forward;
                cameraForward.y = 0;
                if (cameraForward.sqrMagnitude > 0.001f)
                {
                    cameraForward.Normalize();
                    float angle = Vector3.SignedAngle(cameraForward, desiredForward, Vector3.up);
                    rigRoot.Rotate(Vector3.up, angle, Space.World);
                }
            }
        }

        #endregion

        #region Layer Management

        /// <summary>
        /// Initializes layers for the camera rig and hands.
        /// </summary>
        /// <remarks>
        /// This method assigns the appropriate layers to the camera rig, hands, and any
        /// custom layer assignments. It ensures proper physics interactions and collision detection.
        /// </remarks>
        private void InitializeLayers()
        {
            if (!initializeLayers || config == null) return;

            ChangeLayerRecursive(transform, config.PlayerLayer);
            
            if (leftHandPivot != null)
                ChangeLayerRecursive(leftHandPivot, config.LeftHandLayer);
            if (rightHandPivot != null)
                ChangeLayerRecursive(rightHandPivot, config.RightHandLayer);

            // Apply custom layer assignments
            if (customLayerAssignments != null)
            {
                foreach (var assignment in customLayerAssignments)
                {
                    if (assignment.target != null)
                        ChangeLayerRecursive(assignment.target, assignment.layer);
                }
            }
        }

        /// <summary>
        /// Recursively changes the layer of a transform and all its children.
        /// </summary>
        /// <param name="transform">The root transform to change layers for</param>
        /// <param name="layer">The layer index to assign</param>
        /// <remarks>
        /// This method ensures that the entire hierarchy under the specified transform
        /// is assigned to the same layer, which is important for physics interactions.
        /// </remarks>
        private static void ChangeLayerRecursive(Transform transform, int layer)
        {
            if (transform == null) return;
            
            transform.gameObject.layer = layer;
            for (var i = 0; i < transform.childCount; i++)
            {
                ChangeLayerRecursive(transform.GetChild(i), layer);
            }
        }

        #endregion

        #region Hand Initialization

        /// <summary>
        /// Creates and initializes hands based on the selected tracking method.
        /// </summary>
        /// <remarks>
        /// This method determines whether to use transform-based or physics-based hand tracking
        /// and initializes the appropriate hand system accordingly.
        /// </remarks>
        private void CreateAndInitializeHands()
        {
            if (!initializeHands || config?.HandData == null) return;
            
            switch (trackingMethod)
            {
                case InteractionSystemType.TransformBased:
                    InitializeHands();
                    break;
                case InteractionSystemType.PhysicsBased:
                    InitializePhysicsBasedHands();
                    break;
            }
        }

        /// <summary>
        /// Initializes hands with physics-based tracking.
        /// </summary>
        /// <remarks>
        /// This method first initializes the basic hands and then adds physics components
        /// to enable realistic physics interactions and hand following.
        /// </remarks>
        private void InitializePhysicsBasedHands()
        {
            InitializeHands();
            
            if (_rightPoseController != null)
                InitializePhysics(_rightPoseController.gameObject, rightHandPivot);
            if (_leftPoseController != null)
                InitializePhysics(_leftPoseController.gameObject, leftHandPivot);
        }

        /// <summary>
        /// Initializes the basic hand system.
        /// </summary>
        /// <remarks>
        /// This method creates hand instances from prefabs, positions them at the appropriate
        /// pivots, and sets up their basic configuration including interactors.
        /// </remarks>
        private void InitializeHands()
        {
            if (config?.HandData == null) return;
            
            _leftPoseController = InitializeHand(LeftHandPrefab, leftHandPivot, HandIdentifier.Left, leftHandInteractorType);
            _rightPoseController = InitializeHand(RightHandPrefab, rightHandPivot, HandIdentifier.Right, rightHandInteractorType);
        }

        /// <summary>
        /// Initializes physics components for a hand GameObject.
        /// </summary>
        /// <param name="hand">The hand GameObject to add physics to</param>
        /// <param name="target">The target transform for the hand to follow</param>
        /// <remarks>
        /// This method adds a Rigidbody and PhysicsHandFollower component to enable
        /// realistic physics-based hand movement and following behavior.
        /// </remarks>
        private void InitializePhysics(GameObject hand, Transform target)
        {
            if (hand == null || target == null) return;
            
            var rb = hand.GetComponent<Rigidbody>();
            if (rb == null) rb = hand.AddComponent<Rigidbody>();
            
            rb.mass = config.HandMass;
            rb.linearDamping = config.HandLinearDamping;
            rb.angularDamping = config.HandAngularDamping;
            
            var follower = hand.AddComponent<PhysicsHandFollower>();
            follower.Target = target;
        }

        /// <summary>
        /// Initializes a single hand with the specified configuration.
        /// </summary>
        /// <param name="handPrefab">The hand prefab to instantiate</param>
        /// <param name="handPivot">The pivot point for the hand</param>
        /// <param name="handIdentifier">Whether this is the left or right hand</param>
        /// <param name="interactorType">The type of interactor to use</param>
        /// <returns>The initialized HandPoseController component</returns>
        /// <remarks>
        /// This method creates a hand instance, positions it correctly, adds necessary components,
        /// and configures it according to the specified parameters.
        /// </remarks>
        private HandPoseController InitializeHand(HandPoseController handPrefab, Transform handPivot,
            HandIdentifier handIdentifier, HandInteractorType interactorType)
        {
            if (handPrefab == null || handPivot == null) return null;
            
            var hand = Instantiate(handPrefab, handPivot);
            var handTransform = hand.transform;
            handTransform.localPosition = Vector3.zero;
            handTransform.localRotation = Quaternion.identity;
            
            var handGameObject = hand.gameObject;
            var handController = handGameObject.GetComponent<Hand>();
            handController ??= handGameObject.AddComponent<Hand>();
            
            handController.HandIdentifier = handIdentifier;
            handController.Config = config;
            
            if (interactorType == HandInteractorType.Trigger)
                handGameObject.AddComponent<TriggerInteractor>();
            else
                handGameObject.AddComponent<RaycastInteractor>();
                
            return hand;
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// Defines the type of interaction system to use for hand tracking.
    /// </summary>
    /// <remarks>
    /// Transform-based tracking is more performant but less realistic, while physics-based
    /// tracking provides more realistic physics interactions at the cost of performance.
    /// </remarks>
    public enum InteractionSystemType
    {
        TransformBased,
        PhysicsBased
    }

    /// <summary>
    /// Defines the type of interactor to use for hand interactions.
    /// </summary>
    /// <remarks>
    /// Different interactor types provide different interaction capabilities and performance characteristics.
    /// </remarks>
    public enum HandInteractorType
    {
        Trigger,
        Ray
    }

    #endregion
}
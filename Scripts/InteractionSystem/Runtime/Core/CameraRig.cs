using Shababeek.Interactions.Animations;
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace Shababeek.Interactions.Core
{
    [System.Serializable]
    public struct LayerAssignment
    {
        [Tooltip("The target object to assign the layer to.")]
        public Transform target;

        [Tooltip("The layer index to assign to the target object and all its children.")]
        public int layer;
    }

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
        private float _lastCameraHeight;

        #endregion

        #region Public Properties

        /// <summary>
        /// Left hand prefab 
        /// </summary>
        public HandPoseController LeftHandPrefab => config?.HandData?.LeftHandPrefab;

        /// <summary>
        /// Right hand prefab 
        /// </summary>
        public HandPoseController RightHandPrefab => config?.HandData?.RightHandPrefab;

        /// <summary>
        /// Configuration asset.
        /// </summary>
        public Config Config => config;

        /// <summary>
        /// Offset transform used for camera positioning.
        /// </summary>
        public Transform Offset => offsetObject;

        /// <summary>
        /// Camera height. Automatically updates the offset object position.
        /// </summary>
        public float CameraHeight
        {
            get => cameraHeight;
            set
            {
                if (Mathf.Approximately(cameraHeight, value)) return;
                cameraHeight = value;
                ApplyCameraHeight();
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (xrCamera == null)
                xrCamera = GetComponentInChildren<Camera>(true);

            CreateAndInitializeHands();
            InitializeLayers();
        }

        private async void OnEnable()
        {

            ApplyCameraHeight();
            SubscribeToTrackingEvents();
            await Awaitable.NextFrameAsync();
            RecenterCameraToRig();
        }

        private void OnDisable()
        {
            UnsubscribeFromTrackingEvents();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Mathf.Approximately(_lastCameraHeight, cameraHeight))
            {
                _lastCameraHeight = cameraHeight;
                ApplyCameraHeight();
            }
        }
#endif

        #endregion

        #region Camera Height Management

        private void ApplyCameraHeight()
        {
            if (offsetObject == null) return;

            offsetObject.transform.localPosition = Vector3.up * cameraHeight;
            offsetObject.transform.localRotation = Quaternion.identity;
        }

        #endregion

        #region XR Tracking

        private void SubscribeToTrackingEvents()
        {
            var subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var subsystem in subsystems)
            {
                subsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
            }
        }

        private void UnsubscribeFromTrackingEvents()
        {
            var subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var subsystem in subsystems)
            {
                subsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
            }
        }

        private async void OnTrackingOriginUpdated(XRInputSubsystem subsystem)
        {
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();
            if (xrCamera == null || offsetObject == null) return;

            // Step 1: Align rig forward direction (only once if enabled)
            if (!_trackingInitialized && alignRigForwardOnTracking)
            {
                Vector3 cameraForward = xrCamera.transform.forward;
                cameraForward.y = 0;
                if (cameraForward.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(cameraForward);
                    _trackingInitialized = true;
                }
            }

            RecenterCameraToRig();
        }


        private void RecenterCameraToRig()
        {
            if (xrCamera == null || offsetObject == null) return;

            // Reset offsetObject rotation to identity first
            offsetObject.localRotation = Quaternion.identity;

            // Get camera's current local position relative to offsetObject
            Vector3 cameraLocalPos = xrCamera.transform.localPosition;

            // Target: Camera should be at (0, cameraHeight, 0) in rig's local space
            // Formula: offsetObject.localPosition + cameraLocalPos = (0, cameraHeight, 0)
            // Therefore: offsetObject.localPosition = (0, cameraHeight, 0) - cameraLocalPos
            Vector3 targetCameraPosInRigSpace = new Vector3(0f, cameraHeight, 0f);
            offsetObject.localPosition = targetCameraPosInRigSpace - cameraLocalPos;
        }

        private IHandInputProvider SetupHandProviders(Transform handPivot, HandIdentifier hand)
        {
            var inputActions = hand == HandIdentifier.Left ? config.LeftHandActions : config.RightHandActions;
            var trackingType = config.InputType;

            // Create provider based on tracking type
            switch (trackingType)
            {

                case Config.TrackingType.ControllerTracking:
                    // Create only controller provider
                    var controllerOnly = handPivot.gameObject.AddComponent<ControllerInputProvider>();
                    controllerOnly.Handedness = hand;
                    controllerOnly.Initialize(inputActions);
                    return controllerOnly;

#if XR_HANDS_AVAILABLE
                case Config.TrackingType.HandTracking:
                    // Create only hand tracking provider
                    var handTrackingOnly = handPivot.gameObject.AddComponent<HandTrackingInputProvider>();
                    handTrackingOnly.Handedness = hand;
                    return handTrackingOnly;
#endif

                default:
                    // Fallback to controller if unknown
                    var fallbackController = handPivot.gameObject.AddComponent<ControllerInputProvider>();
                    fallbackController.Handedness = hand;
                    fallbackController.Initialize(inputActions);
                    return fallbackController;
            }
        }

        #endregion

        #region Layer Management

        private void InitializeLayers()
        {
            if (!initializeLayers || config == null) return;

            ChangeLayerRecursive(transform, config.PlayerLayer);

            if (leftHandPivot != null)
                ChangeLayerRecursive(leftHandPivot, config.LeftHandLayer);
            if (rightHandPivot != null)
                ChangeLayerRecursive(rightHandPivot, config.RightHandLayer);

            if (customLayerAssignments != null)
            {
                foreach (var assignment in customLayerAssignments)
                {
                    if (assignment.target != null)
                        ChangeLayerRecursive(assignment.target, assignment.layer);
                }
            }
        }

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

        private void CreateAndInitializeHands()
        {
            if (!initializeHands || config?.HandData == null) return;

            switch (trackingMethod)
            {
                case InteractionSystemType.TransformBased:
                    InitializeTransformBasedHands();
                    break;
                case InteractionSystemType.PhysicsBased:
                    InitializePhysicsBasedHands();
                    break;
            }
        }

        private void InitializeTransformBasedHands()
        {
            InitializeHands();

            if (_rightPoseController != null)
                InitializeKinematicRigidbody(_rightPoseController.gameObject);
            if (_leftPoseController != null)
                InitializeKinematicRigidbody(_leftPoseController.gameObject);
        }

        private void InitializePhysicsBasedHands()
        {
            InitializeHands();

            if (_rightPoseController != null)
                InitializePhysics(_rightPoseController.gameObject, rightHandPivot);
            if (_leftPoseController != null)
                InitializePhysics(_leftPoseController.gameObject, leftHandPivot);
        }

        private void InitializeHands()
        {
            if (config?.HandData == null) return;
            var leftProvider = SetupHandProviders(leftHandPivot, HandIdentifier.Left);
            var rightProvider = SetupHandProviders(rightHandPivot, HandIdentifier.Right);

            // Assign to config
            config.SetHandProvider(HandIdentifier.Left, leftProvider);
            config.SetHandProvider(HandIdentifier.Right, rightProvider);
            _leftPoseController = InitializeHand(LeftHandPrefab, leftHandPivot, HandIdentifier.Left, leftHandInteractorType);
            _rightPoseController = InitializeHand(RightHandPrefab, rightHandPivot, HandIdentifier.Right, rightHandInteractorType);

            // Initialize hand pivot updater
            InitializeHandPivotUpdater();
        }

        /// <summary>
        /// Initializes the HandPivotUpdater component to update hand pivots from input providers.
        /// </summary>
        private void InitializeHandPivotUpdater()
        {
            if (config == null || leftHandPivot == null || rightHandPivot == null)
                return;

            // Get or create HandPivotUpdater component
            var updater = GetComponent<HandPivotUpdater>();
            if (updater == null)
            {
                return; updater = gameObject.AddComponent<HandPivotUpdater>();
            }

            // Initialize with references
            updater.Initialize(config, leftHandPivot, rightHandPivot);
        }

        private void InitializeKinematicRigidbody(GameObject hand)
        {
            if (hand == null) return;

            var rb = hand.GetComponent<Rigidbody>();
            if (rb == null) rb = hand.AddComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private void InitializePhysics(GameObject hand, Transform target)
        {
            if (hand == null || target == null) return;

            var rb = hand.GetComponent<Rigidbody>();
            if (rb == null) rb = hand.AddComponent<Rigidbody>();

            rb.mass = config.HandMass;
            rb.linearDamping = config.HandLinearDamping;
            rb.angularDamping = config.HandAngularDamping;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var follower = hand.AddComponent<PhysicsHandFollower>();
            follower.Target = target;
            //follower.ApplySettings(config.FollowerSettings);
        }

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
            {
                handGameObject.GetComponent<TriggerInteractor>().enabled = true;
                handGameObject.AddComponent<RaycastInteractor>().enabled = false;
            }

            else
            {
                handGameObject.AddComponent<RaycastInteractor>().enabled = true;
                handGameObject.AddComponent<TriggerInteractor>().enabled = false;

            }


            return hand;
        }

        #endregion
    }

    #region Enums

    public enum InteractionSystemType
    {
        TransformBased,
        PhysicsBased
    }

    public enum HandInteractorType
    {
        Trigger,
        Ray
    }

    #endregion
}
using Shababeek.Interactions.Animations;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;

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
        [Tooltip("Transform for the left hand pivot point.")]
        [SerializeField][HideInInspector] private Transform leftHandPivot;

        [Tooltip("Transform for the right hand pivot point.")]
        [SerializeField][HideInInspector] private Transform rightHandPivot;

        [Header("Interactor Configuration")]
        [Tooltip("Type of interactor to use for the left hand.")]
        [SerializeField] private HandInteractorType leftHandInteractorType = HandInteractorType.Trigger;

        [Tooltip("Type of interactor to use for the right hand.")]
        [SerializeField] private HandInteractorType rightHandInteractorType = HandInteractorType.Trigger;

        [Header("Camera Configuration")]
        [Tooltip("Transform used to offset the camera position. Usually a child containing the XR camera.")]
        [SerializeField] private Transform offsetObject;

        [Tooltip("The XR camera component. Automatically found if not assigned.")]
        [SerializeField] private Camera xrCamera;

        [Tooltip("Expected height of the camera from the floor in world units.")]
        [SerializeField] private float cameraHeight = 1.7f;

        [Tooltip("Whether to align the rig's forward direction with the initial head direction.")]
        [SerializeField] private bool alignRigForwardOnStart = true;

        [Header("Layer Management")]
        [Tooltip("Whether to automatically initialize layers on startup.")]
        [SerializeField][HideInInspector] private bool initializeLayers = true;

        [Tooltip("Custom layer assignments for specific objects.")]
        [SerializeField] private LayerAssignment[] customLayerAssignments;

        #region Private Fields

        private bool _initialized;
        private bool _cameraInitialized;
        private bool _cameraInitializing;
        private HandPoseController _leftPoseController, _rightPoseController;
        private float _lastCameraHeight;
        private static readonly List<XRInputSubsystem> s_InputSubsystems = new();

        #endregion

        #region Public Properties

        public HandPoseController LeftHandPrefab => config?.HandData?.LeftHandPrefab;
        public HandPoseController RightHandPrefab => config?.HandData?.RightHandPrefab;
        public Config Config => config;
        public Transform Offset => offsetObject;

        public float CameraHeight
        {
            get => cameraHeight;
            set
            {
                if (Mathf.Approximately(cameraHeight, value)) return;
                cameraHeight = value;
                RecenterCamera();
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

        private void Start()
        {
            // Like XROrigin, attempt camera initialization on Start
            TryInitializeCamera();
        }

        private void OnDestroy()
        {
            // Unsubscribe from all XR input subsystem events
            foreach (var inputSubsystem in s_InputSubsystems)
            {
                if (inputSubsystem != null)
                    inputSubsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Mathf.Approximately(_lastCameraHeight, cameraHeight))
            {
                _lastCameraHeight = cameraHeight;
                if (Application.isPlaying)
                    RecenterCamera();
            }
        }
#endif

        #endregion

        #region Camera Positioning

        /// <summary>
        /// Attempts to initialize the camera tracking. If XR subsystems aren't ready yet,
        /// starts a coroutine that keeps trying until successful.
        /// </summary>
        private void TryInitializeCamera()
        {
            if (!Application.isPlaying) return;

            _cameraInitialized = SetupCamera();
            if (!_cameraInitialized && !_cameraInitializing)
                StartCoroutine(RepeatInitializeCamera());
        }

        /// <summary>
        /// Sets up camera tracking and subscribes to XR tracking origin events.
        /// Returns true if setup was successful.
        /// </summary>
        private bool SetupCamera()
        {
            bool initialized = true;

#if UNITY_2023_2_OR_NEWER
            SubsystemManager.GetSubsystems(s_InputSubsystems);
#else
            SubsystemManager.GetInstances(s_InputSubsystems);
#endif

            if (s_InputSubsystems.Count > 0)
            {
                foreach (var inputSubsystem in s_InputSubsystems)
                {
                    if (inputSubsystem == null) continue;

                    // Check if subsystem is ready (can get tracking origin mode)
                    var mode = inputSubsystem.GetTrackingOriginMode();
                    if (mode == TrackingOriginModeFlags.Unknown)
                    {
                        initialized = false;
                        continue;
                    }

                    // Subscribe to tracking origin updates (unsubscribe first to prevent duplicates)
                    inputSubsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
                    inputSubsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
                }
            }
            else
            {
                // No XR subsystems found yet
                initialized = false;
            }

            if (initialized)
            {
                // Do initial alignment and recenter
                DoInitialAlignment();
                RecenterCamera();
            }

            return initialized;
        }

        /// <summary>
        /// Called when XR tracking origin is updated (headset detected, recentered, etc.)
        /// </summary>
        private void OnTrackingOriginUpdated(XRInputSubsystem inputSubsystem)
        {
            // Recenter when tracking origin changes
            RecenterCamera();
        }

        /// <summary>
        /// Coroutine that repeatedly tries to initialize the camera until successful.
        /// </summary>
        private IEnumerator RepeatInitializeCamera()
        {
            _cameraInitializing = true;
            while (!_cameraInitialized)
            {
                yield return null;
                if (!_cameraInitialized)
                    _cameraInitialized = SetupCamera();
            }
            _cameraInitializing = false;
        }

        /// <summary>
        /// Performs initial alignment of the rig to face the camera's forward direction.
        /// </summary>
        private void DoInitialAlignment()
        {
            if (_initialized) return;
            if (xrCamera == null) return;

            if (alignRigForwardOnStart)
            {
                Vector3 cameraForward = xrCamera.transform.forward;
                cameraForward.y = 0;
                if (cameraForward.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(cameraForward);
                }
            }

            _initialized = true;
        }

        /// <summary>
        /// Recenters the camera so the XR camera is at the expected height.
        /// Call this after XR tracking has initialized.
        /// </summary>
        public void RecenterCamera()
        {
            if (xrCamera == null || offsetObject == null) return;

            // Calculate offset needed to place camera at desired height
            // We want the camera world Y to be at rig.position.y + cameraHeight
            float currentCameraWorldY = xrCamera.transform.position.y;
            float desiredCameraWorldY = transform.position.y + cameraHeight;
            float yOffset = desiredCameraWorldY - currentCameraWorldY;

            // Apply offset (only adjust Y, keep XZ from tracking)
            offsetObject.localPosition = new Vector3(0f, offsetObject.localPosition.y + yOffset, 0f);
            offsetObject.localRotation = Quaternion.identity;
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

            config.SetHandProvider(HandIdentifier.Left, leftProvider);
            config.SetHandProvider(HandIdentifier.Right, rightProvider);

            _leftPoseController = InitializeHand(LeftHandPrefab, leftHandPivot, HandIdentifier.Left, leftHandInteractorType);
            _rightPoseController = InitializeHand(RightHandPrefab, rightHandPivot, HandIdentifier.Right, rightHandInteractorType);

            InitializeHandPivotUpdater();
        }

        private IHandInputProvider SetupHandProviders(Transform handPivot, HandIdentifier hand)
        {
            var inputActions = hand == HandIdentifier.Left ? config.LeftHandActions : config.RightHandActions;
            var trackingType = config.InputType;

            switch (trackingType)
            {
                case Config.TrackingType.ControllerTracking:
                    var controllerOnly = handPivot.gameObject.AddComponent<ControllerInputProvider>();
                    controllerOnly.Handedness = hand;
                    controllerOnly.Initialize(inputActions);
                    return controllerOnly;

#if XR_HANDS_AVAILABLE
                case Config.TrackingType.HandTracking:
                    var handTrackingOnly = handPivot.gameObject.AddComponent<HandTrackingInputProvider>();
                    handTrackingOnly.Handedness = hand;
                    return handTrackingOnly;
#endif

                default:
                    var fallbackController = handPivot.gameObject.AddComponent<ControllerInputProvider>();
                    fallbackController.Handedness = hand;
                    fallbackController.Initialize(inputActions);
                    return fallbackController;
            }
        }

        private void InitializeHandPivotUpdater()
        {
            if (config == null || leftHandPivot == null || rightHandPivot == null)
                return;

            var updater = GetComponent<HandPivotUpdater>();
            if (updater == null)
                return;

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

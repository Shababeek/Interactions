using Shababeek.Interactions.Animations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace Shababeek.Interactions.Core
{
    /// <summary>Specifies a layer assignment for a target transform.</summary>
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

        [Tooltip("Whether to align the player's physical facing with the rig's authored forward every time the rig is enabled and on tracking-origin updates.")]
        [SerializeField] private bool alignRigForwardOnTracking = true;

        [Header("Eyelid Effect")]
        [Tooltip("Eyelid overlay effect used for blink transitions between steps.")]
        [SerializeField] private EyelidEffect eyelidEffect;

        [Tooltip("Duration in seconds for a single close or open transition.")]
        [SerializeField] private float eyelidTransitionDuration = 2f;

        [Header("Layer Management")]
        [Tooltip("Whether to automatically initialize layers for the camera rig and hands on startup.")]
        [SerializeField][HideInInspector] private bool initializeLayers = true;

        [Tooltip("Custom layer assignments for specific objects. These will be applied during initialization.")]
        [SerializeField] private LayerAssignment[] customLayerAssignments;

        #region Private Fields

        private bool _trackingInitialized = false;
        private HandPoseController _leftPoseController, _rightPoseController;
        private float _lastCameraHeight;
        private Quaternion _authoredRotation;

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

        /// <summary>The XR camera used as the head/eye transform.</summary>
        public Camera XRCamera => xrCamera;

        /// <summary>Transform the left hand follows.</summary>
        public Transform LeftHandPivot => leftHandPivot;

        /// <summary>Transform the right hand follows.</summary>
        public Transform RightHandPivot => rightHandPivot;

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

        #region Activation Ownership

        private object _activeOwner;

        /// <summary>
        /// Activates the rig for the given owner. A later owner replaces the previous one without deactivating first.
        /// </summary>
        public void ActivateFor(object owner)
        {
            if (owner == null) return;
            _activeOwner = owner;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivates the rig only when the caller is still the active owner.
        /// </summary>
        public void DeactivateFor(object owner)
        {
            if (owner == null || _activeOwner != owner) return;
            _activeOwner = null;
            gameObject.SetActive(false);
        }

        #endregion

        #region Eyelid Control

        /// <summary>Snap lids fully closed (no animation).</summary>
        public void CloseLids() => eyelidEffect?.SetClosed();

        /// <summary>Snap lids fully open (no animation).</summary>
        public void OpenLids() => eyelidEffect?.SetOpen();

        

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _authoredRotation = transform.rotation;

            if (xrCamera == null)
                xrCamera = GetComponentInChildren<Camera>(true);

            CreateAndInitializeHands();
            InitializeLayers();
        }

        private  void OnEnable()
        {
            eyelidEffect?.SetClosed();
            _= HandleRigPositionUpdate();
        }

        private async Awaitable HandleRigPositionUpdate()
        {
            await SubscribeToTrackingEventsWhenReady();
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();
            if (!isActiveAndEnabled) return;
            ApplyInitialTrackingAlignment();
            if (!isActiveAndEnabled) return;
            await WaitForActiveHeadTracking();
            if (!isActiveAndEnabled) return;
            ApplyInitialTrackingAlignment();
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

            if (_trackingInitialized && xrCamera != null)
            {
                RecenterCameraToRig();
            }
            else
            {
                offsetObject.transform.localPosition = Vector3.up * cameraHeight;
                offsetObject.transform.localRotation = Quaternion.identity;
            }
        }

        #endregion

        #region XR Tracking

        private async Awaitable SubscribeToTrackingEventsWhenReady()
        {
            const int maxFramesToWait = 600;
            var subsystems = new List<XRInputSubsystem>();

            for (int frame = 0; frame < maxFramesToWait; frame++)
            {
                subsystems.Clear();
                SubsystemManager.GetSubsystems(subsystems);
                if (subsystems.Count > 0) break;

                await Awaitable.NextFrameAsync();
                if (this == null || !isActiveAndEnabled) return;
            }

            foreach (var subsystem in subsystems)
            {
                // Anchor tracking to the physical floor. Without this the runtime default can
                // be Device mode, where positions are relative to wherever the headset was at
                // session start (e.g. on a desk) — RecenterCameraToRig then bakes that stale
                // reference into the rig offset and the player spawns elevated until the
                // headset's recenter button re-zeroes the origin.
                if ((subsystem.GetSupportedTrackingOriginModes() & TrackingOriginModeFlags.Floor) != 0)
                    subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

                subsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
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

        private async Awaitable WaitForActiveHeadTracking()
        {
            const int maxFramesToWait = 240;
            var nodeStates = new List<XRNodeState>();

            for (int frame = 0; frame < maxFramesToWait; frame++)
            {
                InputTracking.GetNodeStates(nodeStates);
                foreach (var state in nodeStates)
                {
                    // tracked alone isn't enough — over Link the head can report tracked while
                    // the pose is still at origin. Require a real position so the alignment
                    // doesn't bake a zero/stale head pose into the rig offset.
                    if ((state.nodeType == XRNode.CenterEye || state.nodeType == XRNode.Head) && state.tracked
                        && state.TryGetPosition(out var headPosition) && headPosition.sqrMagnitude > 0.0001f)
                        return;
                }

                await Awaitable.NextFrameAsync();
                if (this == null || !isActiveAndEnabled) return;
            }
        }

        private static bool IsHeadTracked()
        {
            var nodeStates = new List<XRNodeState>();
            InputTracking.GetNodeStates(nodeStates);
            foreach (var state in nodeStates)
            {
                if ((state.nodeType == XRNode.CenterEye || state.nodeType == XRNode.Head) && state.tracked)
                    return true;
            }
            return false;
        }

        private async void OnTrackingOriginUpdated(XRInputSubsystem subsystem)
        {
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();
            if (this == null || !isActiveAndEnabled) return;
            ApplyInitialTrackingAlignment();
        }

        private void ApplyInitialTrackingAlignment()
        {
            if (xrCamera == null || offsetObject == null) return;

            if (alignRigForwardOnTracking)
            {
                AlignRigForwardToHead();
            }

            RecenterCameraToRig();
        }

        private void AlignRigForwardToHead()
        {
            // Head forward expressed in rig-local space, flattened to yaw only.
            Vector3 headLocalForward = Quaternion.Inverse(transform.rotation) * xrCamera.transform.forward;
            headLocalForward.y = 0f;
            if (headLocalForward.sqrMagnitude <= 0.001f) return;

            // Counter-rotate the rig by the head's local yaw so the player's physical
            // facing lines up with the rig's authored forward. Idempotent: computed from
            // the authored rotation, so re-running on every enable never drifts.
            var headLocalYaw = Quaternion.LookRotation(headLocalForward);
            transform.rotation = _authoredRotation * Quaternion.Inverse(headLocalYaw);
            _trackingInitialized = true;
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

            // Hands are spawned under the rig root, so the player-layer recursion above overwrites
            // their hand layers. Reapply the hand layers to the spawned hand hierarchies.
            if (_leftPoseController != null)
                ChangeLayerRecursive(_leftPoseController.transform, config.LeftHandLayer);
            if (_rightPoseController != null)
                ChangeLayerRecursive(_rightPoseController.transform, config.RightHandLayer);

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
            {
                InitializeKinematicRigidbody(_rightPoseController.gameObject);
                AttachKinematicFollower(_rightPoseController.gameObject, rightHandPivot);
            }
            if (_leftPoseController != null)
            {
                InitializeKinematicRigidbody(_leftPoseController.gameObject);
                AttachKinematicFollower(_leftPoseController.gameObject, leftHandPivot);
            }
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

            // Provider ownership lives on the Config asset, not on the rig. Touching these
            // properties forces lazy creation of a single shared provider per hand on the
            // Config's runtime host GameObject, so multiple rigs reuse the same providers.
            _ = config.LeftHandProvider;
            _ = config.RightHandProvider;

            _leftPoseController = InitializeHand(LeftHandPrefab, leftHandPivot, HandIdentifier.Left, leftHandInteractorType);
            _rightPoseController = InitializeHand(RightHandPrefab, rightHandPivot, HandIdentifier.Right, rightHandInteractorType);

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

        private void AttachKinematicFollower(GameObject hand, Transform target)
        {
            if (hand == null || target == null) return;

            var follower = hand.GetComponent<KinematicHandFollower>();
            if (follower == null) follower = hand.AddComponent<KinematicHandFollower>();
            follower.Target = target;
        }

        private HandPoseController InitializeHand(HandPoseController handPrefab, Transform handPivot,
            HandIdentifier handIdentifier, HandInteractorType interactorType)
        {
            if (handPrefab == null || handPivot == null) return null;

            // Hands are spawned directly under the CameraRig root rather than inside the pivot so
            // that a single humanoid skeleton can own the whole hand hierarchy. The hand's world
            // transform is synced to the pivot by PhysicsHandFollower or KinematicHandFollower.
            var hand = Instantiate(handPrefab, transform);
            var handTransform = hand.transform;
            handTransform.SetPositionAndRotation(handPivot.position, handPivot.rotation);

            var handGameObject = hand.gameObject;
            var handController = handGameObject.GetComponent<Hand>();
            handController ??= handGameObject.AddComponent<Hand>();

            handController.HandIdentifier = handIdentifier;
            handController.Config = config;

            // Mirror the side onto the pose controller so muscle-based pin-bone resolution picks
            // the matching humanoid bone (LeftHand vs RightHand). Finger muscle writes are
            // side-agnostic; only the pin side depends on this.
            hand.Hand = handIdentifier;

            // Get-or-add both interactors, then enable only the requested one. The previous
            // code NRE'd when the prefab lacked a TriggerInteractor, and in the Ray branch
            // added a duplicate TriggerInteractor while leaving the prefab's original enabled —
            // two live interactors fighting over the same interactable state machine.
            var triggerInteractor = handGameObject.GetComponent<TriggerInteractor>();
            if (triggerInteractor == null) triggerInteractor = handGameObject.AddComponent<TriggerInteractor>();
            var raycastInteractor = handGameObject.GetComponent<RaycastInteractor>();
            if (raycastInteractor == null) raycastInteractor = handGameObject.AddComponent<RaycastInteractor>();

            triggerInteractor.enabled = interactorType == HandInteractorType.Trigger;
            raycastInteractor.enabled = interactorType != HandInteractorType.Trigger;


            return hand;
        }

        #endregion

        #region Blinking

        /// <summary>Animate lids closed to hide the scene.</summary>
        public async Awaitable BlinkIn(CancellationToken cancellationToken = default)
        {
            if (eyelidEffect == null) return;
            eyelidEffect.Close(eyelidTransitionDuration);
            await Awaitable.WaitForSecondsAsync(eyelidTransitionDuration, cancellationToken);
        }

        /// <summary>Animate lids open to reveal the scene.</summary>
        public async Awaitable BlinkOut(CancellationToken cancellationToken = default)
        {
            if (eyelidEffect == null) return;
            eyelidEffect.Open(eyelidTransitionDuration);
            await Awaitable.WaitForSecondsAsync(eyelidTransitionDuration, cancellationToken);
        }

        /// <summary>
        /// Plays repeated close-then-open eyelid cycles, then ends with lids closed.
        /// One cycle is a full close followed by a full open.
        /// </summary>
        public async Awaitable PlayEyelidCyclesThenClose(
            int cycleCount,
            float intervalBetweenTransitions,
            CancellationToken cancellationToken = default)
        {
            if (eyelidEffect == null) return;

            for (int i = 0; i < cycleCount; i++)
            {
                await BlinkIn(cancellationToken);

                if (intervalBetweenTransitions > 0f)
                    await Awaitable.WaitForSecondsAsync(intervalBetweenTransitions, cancellationToken);

                await BlinkOut(cancellationToken);

                if (intervalBetweenTransitions > 0f)
                    await Awaitable.WaitForSecondsAsync(intervalBetweenTransitions, cancellationToken);
            }

            await BlinkIn(cancellationToken);
        }

        #endregion
    }

    #region Enums

    /// <summary>Type of hand tracking system to use.</summary>
    public enum InteractionSystemType
    {
        /// <summary>Transform-based hand tracking (kinematic).</summary>
        TransformBased,
        /// <summary>Physics-based hand tracking (dynamic rigidbody).</summary>
        PhysicsBased
    }

    /// <summary>Type of interactor to use for hand interactions.</summary>
    public enum HandInteractorType
    {
        /// <summary>Trigger-based interactor for direct collisions.</summary>
        Trigger,
        /// <summary>Ray-based interactor for distance interactions.</summary>
        Ray
    }

    #endregion
}
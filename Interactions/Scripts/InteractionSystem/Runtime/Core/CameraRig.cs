using Shababeek.Interactions.Animations;
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace Shababeek.Interactions.Core
{
    [System.Serializable]
    public struct LayerAssignment
    {
        public Transform target;
        public int layer;
    }

    public class CameraRig : MonoBehaviour
    {
        [Header("Camera Rig Settings")]
        [Tooltip("Configuration for the camera rig, including hand prefabs and settings.")]
        [SerializeField] private Config config;
        [SerializeField][HideInInspector] private bool initializeHands = true;

        [SerializeField][HideInInspector] private InteractionSystemType trackingMethod = InteractionSystemType.PhysicsBased;

        [SerializeField][HideInInspector] private Transform leftHandPivot;
        [SerializeField][HideInInspector] private Transform rightHandPivot;
        [SerializeField] private HandInteractorType leftHandInteractorType = HandInteractorType.Trigger;
        [SerializeField] private HandInteractorType rightHandInteractorType = HandInteractorType.Trigger;
        [SerializeField] private Transform offsetObject;

        [SerializeField] private Camera xrCamera;
        [SerializeField] private float cameraHeight = 1f; // Default standing height
        [SerializeField] private bool alignRigForwardOnTracking = true;
        [Tooltip("If true, initializes layers for the camera rig and hands.")]
        [SerializeField][HideInInspector] private bool initializeLayers = true;

        [Tooltip("Assign specific layers to specific objects on initialization.")]
        [SerializeField] private LayerAssignment[] customLayerAssignments;

        private bool _trackingInitialized = false;

        private HandPoseController _leftPoseController, _rightPoseController;
        /// <summary>
        /// Gets the left hand prefab from the configuration's HandData.
        /// </summary>
        public HandPoseController LeftHandPrefab => config.HandData.LeftHandPrefab;
        /// <summary>
        /// Gets the right hand prefab from the configuration's HandData.
        /// </summary>
        public HandPoseController RightHandPrefab => config.HandData.RightHandPrefab;
        /// <summary>
        /// Gets the configuration asset for the camera rig.
        /// </summary>
        public Config Config => config;

        private void Awake()
        {
            if (xrCamera == null)
                xrCamera = GetComponentInChildren<Camera>(true);
            CreateAndItializeHands();
            InitializeLayers();
        }

        private void OnEnable()
        {
            offsetObject.transform.localPosition = Vector3.up * cameraHeight;
            offsetObject.transform.localRotation = Quaternion.identity;

            SubscribeToTrackingEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromTrackingEvents();
        }

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

        private void OnTrackingOriginUpdated(XRInputSubsystem subsystem)
        {
            TryApplyCameraOffsetAndAlignment();
        }


        private void TryApplyCameraOffsetAndAlignment()
        {
            if (_trackingInitialized) return;
            _trackingInitialized = true;
            ApplyCameraOffset();
            if (alignRigForwardOnTracking)
                AlignRigForward();
        }

        private void ApplyCameraOffset()
        {
            if (offsetObject != null)
            {
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
        }

        private void AlignRigForward()
        {
            // Align the rig's forward to match the CameraRig's forward
            if (offsetObject != null)
            {
                var rigRoot = transform;
                var camera = xrCamera;
                if (camera != null)
                {
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
            }
        }

        private void InitializeLayers()
        {
            if (!initializeLayers) return;

            ChangeLayerRecursive(transform, config.PlayerLayer);
            ChangeLayerRecursive(leftHandPivot, config.LeftHandLayer);
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

        private void CreateAndItializeHands()
        {
            if (!initializeHands) return;
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

        private static void ChangeLayerRecursive(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            for (var i = 0; i < transform.childCount; i++) ChangeLayerRecursive(transform.GetChild(i), layer);
        }
        


        private void InitializePhysicsBasedHands()
        {
            InitializeHands();
            InitializePhysics(_rightPoseController.gameObject, rightHandPivot);
            InitializePhysics(_leftPoseController.gameObject, leftHandPivot);
        }

        private void InitializeHands()
        {
            _leftPoseController =
                InitializeHand(LeftHandPrefab, leftHandPivot, HandIdentifier.Left, leftHandInteractorType);
            _rightPoseController = InitializeHand(RightHandPrefab, rightHandPivot, HandIdentifier.Right,
                rightHandInteractorType);
        }

        private void InitializePhysics(GameObject hand, Transform target)
        {
            var rb = hand.GetComponent<Rigidbody>();
            if (rb == null) rb = hand.AddComponent<Rigidbody>();
            rb.mass = config.HandMass;
            rb.linearDamping = config.HandLinearDamping;
            rb.angularDamping = config.HandAngularDamping;
            var follower = hand.AddComponent<PhysicsHandFollower>();
            follower.Target = target;
        }

        private HandPoseController InitializeHand(HandPoseController handPrefab, Transform handPivot,
            HandIdentifier handIdentifier, HandInteractorType interactorType)
        {
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
    }

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

}
#if XR_HANDS_AVAILABLE
using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Hand input provider that reads directly from XR Hand Subsystem for reliable hand tracking.
    /// Provides 0-1 finger curl values calculated from joint rotations relative to the hand.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Input Providers/Hand Tracking Input Provider")]
    public class HandTrackingInputProvider : HandInputProviderBase
    {
        [Header("Hand Tracking")]
        [Tooltip("Maximum flex angle in degrees for each finger (when fully curled).")]
        [SerializeField] private float maxFlexAngle = 90f;
        
        [Tooltip("Minimum flex angle in degrees (when fully extended).")]
        [SerializeField] private float minFlexAngle = 0f;

        [Tooltip("Bend in degrees at which the index, middle, ring and little fingers count as fully open. A relaxed hand is never perfectly straight; this keeps it from reading as a light press.")]
        [SerializeField] private float fingerRestAngle = 15f;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging for finger values.")]
        [SerializeField] private bool debugLog = false;

        private XRHandSubsystem handSubsystem;
        private bool _isInitialized = false;
    
        private void Awake()
        {
            EnsureDefaultPriority();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InitializeHandTracking();
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            ShutdownHandTracking();
        }
        
        protected override void Update()
        {
            // Try to initialize if not already initialized
            if (!_isInitialized)
            {
                InitializeHandTracking();
            }
            
            // Check if subsystem is still running, if not try to restart
            if (handSubsystem != null && !handSubsystem.running)
            {
                if (debugLog)
                    Debug.LogWarning($"HandTrackingInputProvider: Subsystem stopped, attempting to restart...");
                handSubsystem.Start();
            }
            
            // Call base Update to trigger UpdateFingerValues and UpdateButtonStates
            base.Update();
        }
        
        protected override void UpdateFingerValues()
        {
            if (!_isInitialized || handSubsystem == null || !handSubsystem.running || !Hand.isTracked)
            {
                // Set all to zero when not tracked
                SetFingerValues(0f, 0f, 0f, 0f, 0f);
                return;
            }
            
            // Calculate finger curl values
            this[FingerName.Thumb] = CalculateFingerCurl(XRHandFingerID.Thumb);
            this[FingerName.Index] = CalculateFingerCurl(XRHandFingerID.Index);
            this[FingerName.Middle] = CalculateFingerCurl(XRHandFingerID.Middle);
            this[FingerName.Ring] = CalculateFingerCurl(XRHandFingerID.Ring);
            this[FingerName.Pinky] = CalculateFingerCurl(XRHandFingerID.Little);
            
            if (debugLog)
            {
                Debug.Log($"Finger Curls - T:{this[FingerName.Thumb]:F2} I:{this[FingerName.Index]:F2} " +
                         $"M:{this[FingerName.Middle]:F2} R:{this[FingerName.Ring]:F2} P:{this[FingerName.Pinky]:F2}");
            }
        }

        /// <summary>True while the hand subsystem is running and reports this hand as tracked.</summary>
        public bool IsTracked =>
            _isInitialized && handSubsystem != null && handSubsystem.running && Hand.isTracked;

        // Palm joint axes are +Z toward the fingers and +Y out of the back of the hand. A grip
        // pose points +Z from the little finger toward the thumb with +X along the palm normal.
        private static readonly Quaternion LeftPalmToGrip = Quaternion.LookRotation(Vector3.right, Vector3.back);
        private static readonly Quaternion RightPalmToGrip = Quaternion.LookRotation(Vector3.left, Vector3.back);

        /// <summary>
        /// Pose of the tracked hand in tracking space, in the same convention as a controller's
        /// grip pose. Uses the runtime's grip pose when it provides one, otherwise the palm joint.
        /// </summary>
        public bool TryGetGripPose(out Pose pose)
        {
            pose = Pose.identity;
            if (!IsTracked) return false;

            var gestures = handedness == HandIdentifier.Left
                ? handSubsystem.leftHandCommonGestures
                : handSubsystem.rightHandCommonGestures;
            if (gestures != null && gestures.TryGetGripPose(out pose)) return true;

            if (!TryGetJointPose(XRHandJointID.Palm, out var palm)) return false;
            var palmToGrip = handedness == HandIdentifier.Left ? LeftPalmToGrip : RightPalmToGrip;
            pose = new Pose(palm.position, palm.rotation * palmToGrip);
            return true;
        }

        private void EnsureDefaultPriority()
        {
            if (priority <= 0 || priority >= 20)
                priority = 10;
        }
        
        private XRHand Hand
        {
            get
            {
                if (handSubsystem == null || !handSubsystem.running)
                    return default;
                
                return handedness == HandIdentifier.Left ? 
                    handSubsystem.leftHand : 
                    handSubsystem.rightHand;
            }
        }
        
        private void InitializeHandTracking()
        {
            List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            
            if (subsystems.Count == 0)
            {
                if (debugLog)
                    Debug.LogWarning("HandTrackingInputProvider: No XR Hand Subsystem found yet. Will retry...");
                return;
            }
            
            handSubsystem = subsystems[0];
            
            // Ensure subsystem is started
            if (!handSubsystem.running)
            {
                handSubsystem.Start();
                
                // Check if it actually started
                if (!handSubsystem.running)
                {
                    if (debugLog)
                        Debug.LogWarning($"HandTrackingInputProvider: Failed to start subsystem. Will retry...");
                    return;
                }
            }
            
            _isInitialized = true;
            
            if (debugLog)
                Debug.Log($"HandTrackingInputProvider: Initialized for {handedness} hand. Subsystem running: {handSubsystem.running}");
        }
        
        private void ShutdownHandTracking()
        {
            // The XRHandSubsystem is shared with the other hand's provider (and anything else
            // reading hands), so it is left running; the XR loader owns its lifetime.
            _isInitialized = false;
            handSubsystem = null;
        }
        
        private float CalculateFingerCurl(XRHandFingerID fingerID)
        {
            XRHandJointID metacarpalJoint = GetMetacarpalJoint(fingerID);
            XRHandJointID proximalJoint = GetProximalJoint(fingerID);

            if (!TryGetJointPose(metacarpalJoint, out Pose metacarpalPose) ||
                !TryGetJointPose(proximalJoint, out Pose proximalPose))
            {
                return 0f;
            }

            if (fingerID == XRHandFingerID.Thumb)
            {
                float thumbAngle = ThumbFlexAngle(metacarpalPose, proximalPose);
                return Mathf.Clamp01(Mathf.InverseLerp(minFlexAngle, maxFlexAngle, thumbAngle));
            }

            float fingerAngle = FingerFlexAngle(fingerID, metacarpalPose, proximalPose);
            float openAngle = Mathf.Max(minFlexAngle, fingerRestAngle);
            return Mathf.Clamp01(Mathf.InverseLerp(openAngle, maxFlexAngle, fingerAngle));
        }

        private static float ThumbFlexAngle(Pose metacarpalPose, Pose proximalPose)
        {
            Quaternion relativeRotation = Quaternion.Inverse(metacarpalPose.rotation) * proximalPose.rotation;
            float angle = relativeRotation.eulerAngles.x;
            if (angle > 180f) angle -= 360f;
            return Mathf.Abs(angle);
        }

        // Average of the bend at the knuckle and at the middle joint, measured between bone
        // directions taken from joint positions, so it does not depend on how the runtime
        // orients each joint. Reading one Euler component of the knuckle rotation does not work:
        // it misses flexion on some fingers and flips once the bend passes 90 degrees.
        private float FingerFlexAngle(XRHandFingerID fingerID, Pose metacarpalPose, Pose proximalPose)
        {
            XRHandJointID intermediateJoint = XRHandJointID.IndexIntermediate;
            XRHandJointID distalJoint = XRHandJointID.IndexDistal;
            switch (fingerID)
            {
                case XRHandFingerID.Middle:
                    intermediateJoint = XRHandJointID.MiddleIntermediate;
                    distalJoint = XRHandJointID.MiddleDistal;
                    break;
                case XRHandFingerID.Ring:
                    intermediateJoint = XRHandJointID.RingIntermediate;
                    distalJoint = XRHandJointID.RingDistal;
                    break;
                case XRHandFingerID.Little:
                    intermediateJoint = XRHandJointID.LittleIntermediate;
                    distalJoint = XRHandJointID.LittleDistal;
                    break;
            }

            if (!TryGetJointPose(intermediateJoint, out Pose intermediatePose)) return 0f;

            Vector3 palmBone = proximalPose.position - metacarpalPose.position;
            Vector3 proximalBone = intermediatePose.position - proximalPose.position;
            float knuckleBend = Vector3.Angle(palmBone, proximalBone);

            if (!TryGetJointPose(distalJoint, out Pose distalPose)) return knuckleBend;

            Vector3 middleBone = distalPose.position - intermediatePose.position;
            return (knuckleBend + Vector3.Angle(proximalBone, middleBone)) * 0.5f;
        }

        private XRHandJointID GetMetacarpalJoint(XRHandFingerID fingerID)
        {
            switch (fingerID)
            {
                case XRHandFingerID.Thumb:
                    return XRHandJointID.ThumbMetacarpal;
                case XRHandFingerID.Index:
                    return XRHandJointID.IndexMetacarpal;
                case XRHandFingerID.Middle:
                    return XRHandJointID.MiddleMetacarpal;
                case XRHandFingerID.Ring:
                    return XRHandJointID.RingMetacarpal;
                case XRHandFingerID.Little:
                    return XRHandJointID.LittleMetacarpal;
                default:
                    return XRHandJointID.Palm;
            }
        }
        
        private XRHandJointID GetProximalJoint(XRHandFingerID fingerID)
        {
            switch (fingerID)
            {
                case XRHandFingerID.Thumb:
                    return XRHandJointID.ThumbProximal;
                case XRHandFingerID.Index:
                    return XRHandJointID.IndexProximal;
                case XRHandFingerID.Middle:
                    return XRHandJointID.MiddleProximal;
                case XRHandFingerID.Ring:
                    return XRHandJointID.RingProximal;
                case XRHandFingerID.Little:
                    return XRHandJointID.LittleProximal;
                default:
                    return XRHandJointID.Palm;
            }
        }
        
        private bool TryGetJointPose(XRHandJointID jointID, out Pose pose)
        {
            var hand = Hand;
            if (!hand.isTracked)
            {
                pose = default;
                return false;
            }
            
            var joint = hand.GetJoint(jointID);
            if (joint.id == XRHandJointID.Invalid)
            {
                pose = default;
                return false;
            }
            
            return joint.TryGetPose(out pose);
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureDefaultPriority();
        }
#endif
    }
}
#endif

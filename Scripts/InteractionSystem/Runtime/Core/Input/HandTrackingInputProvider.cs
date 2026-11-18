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
        
        [Tooltip("Total bend in degrees, summed over the three finger joints, that counts as a fully curled index/middle/ring/little finger.")]
        [SerializeField] private float maxFingerCurlAngle = 200f;

        [Tooltip("Minimum flex angle in degrees (when fully extended).")]
        [SerializeField] private float minFlexAngle = 0f;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging for finger values.")]
        [SerializeField] private bool debugLog = false;

        private XRHandSubsystem handSubsystem;
        private bool _isInitialized = false;
        
        // Cached position/rotation for efficiency
        [SerializeField] private Vector3 _position = Vector3.zero;
        [SerializeField] private Quaternion _rotation = Quaternion.identity;
        [SerializeField] private uint _trackingState = 0;
    
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
                _position = Vector3.zero;
                _rotation = Quaternion.identity;
                _trackingState = 0;
                return;
            }
            
            // Calculate finger curl values
            this[FingerName.Thumb] = CalculateFingerCurl(XRHandFingerID.Thumb);
            this[FingerName.Index] = CalculateFingerCurl(XRHandFingerID.Index);
            this[FingerName.Middle] = CalculateFingerCurl(XRHandFingerID.Middle);
            this[FingerName.Ring] = CalculateFingerCurl(XRHandFingerID.Ring);
            this[FingerName.Pinky] = CalculateFingerCurl(XRHandFingerID.Little);
            
            // Update position, rotation, and tracking state
            if (TryGetHandPosition(out Vector3 pos))
            {
                _position = pos;
            }
            else
            {
                _position = Vector3.zero;
            }
            
            if (TryGetHandRotation(out Quaternion rot))
            {
                _rotation = rot;
            }
            else
            {
                _rotation = Quaternion.identity;
            }
            
            _trackingState = GetTrackingState();
            
            if (debugLog)
            {
                Debug.Log($"Finger Curls - T:{this[FingerName.Thumb]:F2} I:{this[FingerName.Index]:F2} " +
                         $"M:{this[FingerName.Middle]:F2} R:{this[FingerName.Ring]:F2} P:{this[FingerName.Pinky]:F2}");
            }
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
            if (fingerID != XRHandFingerID.Thumb)
                return CalculateLongFingerCurl(fingerID);

            // Thumb keeps the joint-rotation estimate: its joint axes do not match the other fingers.
            XRHandJointID metacarpalJoint = GetMetacarpalJoint(fingerID);
            XRHandJointID proximalJoint = GetProximalJoint(fingerID);

            if (!TryGetJointPose(metacarpalJoint, out Pose metacarpalPose) ||
                !TryGetJointPose(proximalJoint, out Pose proximalPose))
            {
                return 0f;
            }

            Quaternion relativeRotation = Quaternion.Inverse(metacarpalPose.rotation) * proximalPose.rotation;
            float flexAngle = GetFlexAngle(fingerID, relativeRotation);
            return Mathf.Clamp01(Mathf.InverseLerp(minFlexAngle, maxFlexAngle, flexAngle));
        }

        /// <summary>
        /// Curl for index/middle/ring/little from joint POSITIONS: the sum of the bend angles
        /// between consecutive bone directions (knuckle, middle joint, fingertip joint). Positions
        /// do not depend on each joint's local axis convention, and the metacarpal joint, which
        /// some runtimes do not report for every finger, is replaced by the wrist when missing.
        /// </summary>
        private float CalculateLongFingerCurl(XRHandFingerID fingerID)
        {
            GetLongFingerJoints(fingerID, out var proximal, out var intermediate, out var distal, out var tip, out var metacarpal);

            if (!TryGetJointPose(proximal, out Pose proximalPose) ||
                !TryGetJointPose(intermediate, out Pose intermediatePose) ||
                !TryGetJointPose(distal, out Pose distalPose) ||
                !TryGetJointPose(tip, out Pose tipPose))
            {
                return 0f;
            }

            Vector3 origin;
            if (TryGetJointPose(metacarpal, out Pose metacarpalPose))
                origin = metacarpalPose.position;
            else if (TryGetJointPose(XRHandJointID.Wrist, out Pose wristPose))
                origin = wristPose.position;
            else
                return 0f;

            Vector3 palmBone = proximalPose.position - origin;
            Vector3 proximalBone = intermediatePose.position - proximalPose.position;
            Vector3 intermediateBone = distalPose.position - intermediatePose.position;
            Vector3 distalBone = tipPose.position - distalPose.position;

            float total = Vector3.Angle(palmBone, proximalBone)
                          + Vector3.Angle(proximalBone, intermediateBone)
                          + Vector3.Angle(intermediateBone, distalBone);

            return Mathf.Clamp01(Mathf.InverseLerp(0f, maxFingerCurlAngle, total));
        }

        private static void GetLongFingerJoints(XRHandFingerID fingerID, out XRHandJointID proximal,
            out XRHandJointID intermediate, out XRHandJointID distal, out XRHandJointID tip,
            out XRHandJointID metacarpal)
        {
            switch (fingerID)
            {
                case XRHandFingerID.Index:
                    metacarpal = XRHandJointID.IndexMetacarpal; proximal = XRHandJointID.IndexProximal;
                    intermediate = XRHandJointID.IndexIntermediate; distal = XRHandJointID.IndexDistal;
                    tip = XRHandJointID.IndexTip; break;
                case XRHandFingerID.Middle:
                    metacarpal = XRHandJointID.MiddleMetacarpal; proximal = XRHandJointID.MiddleProximal;
                    intermediate = XRHandJointID.MiddleIntermediate; distal = XRHandJointID.MiddleDistal;
                    tip = XRHandJointID.MiddleTip; break;
                case XRHandFingerID.Ring:
                    metacarpal = XRHandJointID.RingMetacarpal; proximal = XRHandJointID.RingProximal;
                    intermediate = XRHandJointID.RingIntermediate; distal = XRHandJointID.RingDistal;
                    tip = XRHandJointID.RingTip; break;
                default:
                    metacarpal = XRHandJointID.LittleMetacarpal; proximal = XRHandJointID.LittleProximal;
                    intermediate = XRHandJointID.LittleIntermediate; distal = XRHandJointID.LittleDistal;
                    tip = XRHandJointID.LittleTip; break;
            }
        }

        private float GetFlexAngle(XRHandFingerID fingerID, Quaternion relativeRotation)
        {
            Vector3 euler = relativeRotation.eulerAngles;
            
            // Different fingers flex on different axes
            // For most fingers, flexion is around the Z axis
            // For thumb, it's more complex due to different joint orientation
            
            if (fingerID == XRHandFingerID.Thumb)
            {
                // Thumb flexion is typically around X axis
                float angle = euler.x;
                // Normalize to 0-180 range
                if (angle > 180f) angle -= 360f;
                return Mathf.Abs(angle);
            }
            else
            {
                // Other fingers flex around Z axis
                float angle = euler.z;
                // Normalize to 0-180 range
                if (angle > 180f) angle -= 360f;
                return Mathf.Abs(angle);
            }
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
        
        private bool TryGetHandPosition(out Vector3 position)
        {
            var hand = Hand;
            if (!hand.isTracked)
            {
                position = Vector3.zero;
                return false;
            }
            
            if (TryGetJointPose(XRHandJointID.Wrist, out Pose wristPose))
            {
                position = wristPose.position;
                return true;
            }
            
            position = Vector3.zero;
            return false;
        }
        
        private bool TryGetHandRotation(out Quaternion rotation)
        {
            var hand = Hand;
            if (!hand.isTracked)
            {
                rotation = Quaternion.identity;
                return false;
            }
            
            if (TryGetJointPose(XRHandJointID.Wrist, out Pose wristPose))
            {
                rotation = wristPose.rotation;
                return true;
            }
            
            rotation = Quaternion.identity;
            return false;
        }
        
        private uint GetTrackingState()
        {
            var hand = Hand;
            if (!hand.isTracked)
                return 0;
            
            var wristJoint = hand.GetJoint(XRHandJointID.Wrist);
            if (wristJoint.id == XRHandJointID.Invalid)
                return 0;
            
            uint state = 0;
            var trackingState = wristJoint.trackingState;
            
            // XRHandJointTrackingState.Pose indicates both position and rotation are tracked
            if ((trackingState & XRHandJointTrackingState.Pose) != 0)
            {
                state |= 3; // Both position (1) and rotation (2) flags
            }
            else
            {
                // If Pose is not available, check if we can at least get position/rotation from the pose
                if (TryGetJointPose(XRHandJointID.Wrist, out Pose wristPose))
                {
                    // If pose is valid (not zero/identity), assume both are tracked
                    if (wristPose.position != Vector3.zero)
                        state |= 1; // Position flag
                    if (wristPose.rotation != Quaternion.identity)
                        state |= 2; // Rotation flag
                }
            }
            
            return state;
        }

        /// <summary>
        /// Current position of the hand in world space.
        /// </summary>
        public override Vector3 Position => _position;

        /// <summary>
        /// Current rotation of the hand in world space.
        /// </summary>
        public override Quaternion Rotation => _rotation;

        /// <summary>
        /// Current tracking state of the hand (flags: Position = 1, Rotation = 2, Both = 3).
        /// </summary>
        public override uint TrackingState => _trackingState;

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureDefaultPriority();
        }
#endif
    }
}
#endif

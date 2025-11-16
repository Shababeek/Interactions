using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Reads hand tracking data from XR Hand Subsystem and converts joint rotations to 0-1 finger curl values.
    /// Provides relative rotation calculations and automatic curl value mapping for VR hand interactions.
    /// </summary>
    public class HandTrackingReader : MonoBehaviour
    {
        [Header("Hand Configuration")]
        [Tooltip("Which hand this reader is tracking (left or right).")]
        [SerializeField] private HandIdentifier handedness = HandIdentifier.Left;
        
        [Header("Curl Mapping")]
        [Tooltip("Maximum flex angle in degrees for each finger (when fully curled).")]
        [SerializeField] private float maxFlexAngle = 90f;
        
        [Tooltip("Minimum flex angle in degrees (when fully extended).")]
        [SerializeField] private float minFlexAngle = 0f;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging for finger values.")]
        [SerializeField] private bool debugLog = false;
        
        private XRHandSubsystem handSubsystem;
        private XRHand _hand;
        private float[] _fingerCurls = new float[5];
        
        /// <summary>
        /// Finger curl values (0 = extended, 1 = curled).
        /// </summary>
        public float[] FingerCurls => _fingerCurls;
        
        /// <summary>
        /// Checks if hand tracking is currently active for this hand.
        /// </summary>
        public bool IsTracked => _hand.isTracked;
        
        void OnEnable()
        {
            InitializeHandTracking();
        }
        
        void OnDisable()
        {
            ShutdownHandTracking();
        }
        
        void Update()
        {
            if (handSubsystem != null && _hand.isTracked)
            {
                UpdateFingerCurls();
            }
        }
        
        /// <summary>
        /// Initializes the hand tracking subsystem.
        /// </summary>
        private void InitializeHandTracking()
        {
            List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            
            if (subsystems.Count == 0)
            {
                Debug.LogError("HandTrackingReader: No XR Hand Subsystem found! Install XR Hands package.");
                return;
            }
            
            handSubsystem = subsystems[0];
            
            if (!handSubsystem.running)
            {
                handSubsystem.Start();
            }
            
            // Get the appropriate hand
            _hand = handedness == HandIdentifier.Left ? 
                handSubsystem.leftHand : 
                handSubsystem.rightHand;
            
            if (debugLog)
                Debug.Log($"HandTrackingReader: Initialized for {handedness} hand");
        }
        
        /// <summary>
        /// Shuts down hand tracking subsystem.
        /// </summary>
        private void ShutdownHandTracking()
        {
            if (handSubsystem != null && handSubsystem.running)
            {
                handSubsystem.Stop();
            }
        }
        
        /// <summary>
        /// Updates finger curl values from hand tracking data.
        /// </summary>
        private void UpdateFingerCurls()
        {
            _fingerCurls[0] = CalculateFingerCurl(XRHandFingerID.Thumb);
            _fingerCurls[1] = CalculateFingerCurl(XRHandFingerID.Index);
            _fingerCurls[2] = CalculateFingerCurl(XRHandFingerID.Middle);
            _fingerCurls[3] = CalculateFingerCurl(XRHandFingerID.Ring);
            _fingerCurls[4] = CalculateFingerCurl(XRHandFingerID.Little);
            
            if (debugLog)
            {
                Debug.Log($"Finger Curls - T:{_fingerCurls[0]:F2} I:{_fingerCurls[1]:F2} " +
                         $"M:{_fingerCurls[2]:F2} R:{_fingerCurls[3]:F2} P:{_fingerCurls[4]:F2}");
            }
        }
        
        /// <summary>
        /// Calculates the curl value for a specific finger (0 = extended, 1 = curled).
        /// </summary>
        private float CalculateFingerCurl(XRHandFingerID fingerID)
        {
            // Get the relevant joints
            XRHandJointID metacarpalJoint = GetMetacarpalJoint(fingerID);
            XRHandJointID proximalJoint = GetProximalJoint(fingerID);
            
            // Try to get poses
            if (!TryGetJointPose(metacarpalJoint, out Pose metacarpalPose) ||
                !TryGetJointPose(proximalJoint, out Pose proximalPose))
            {
                return 0f; // Return extended if we can't get data
            }
            
            // Calculate relative rotation
            Quaternion relativeRotation = Quaternion.Inverse(metacarpalPose.rotation) * proximalPose.rotation;
            
            // Get the flex angle (rotation around the local X or Z axis depending on finger)
            float flexAngle = GetFlexAngle(fingerID, relativeRotation);
            
            // Map angle to 0-1 range
            float curl = Mathf.InverseLerp(minFlexAngle, maxFlexAngle, flexAngle);
            
            return Mathf.Clamp01(curl);
        }
        
        /// <summary>
        /// Extracts the flex angle from the relative rotation for a specific finger.
        /// </summary>
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
        
        /// <summary>
        /// Gets the metacarpal joint ID for a finger.
        /// </summary>
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
        
        /// <summary>
        /// Gets the proximal joint ID for a finger.
        /// </summary>
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
        
        /// <summary>
        /// Tries to get the pose for a specific joint.
        /// </summary>
        private bool TryGetJointPose(XRHandJointID jointID, out Pose pose)
        {
            var joint = _hand.GetJoint(jointID);
            return joint.TryGetPose(out pose);
        }
        
        /// <summary>
        /// Gets the curl value for a specific finger by index.
        /// </summary>
        public float GetFingerCurl(int fingerIndex)
        {
            if (fingerIndex < 0 || fingerIndex >= 5)
                return 0f;
            
            return _fingerCurls[fingerIndex];
        }
        
        /// <summary>
        /// Gets the curl value for a specific finger by name.
        /// </summary>
        public float GetFingerCurl(FingerName fingerName)
        {
            return GetFingerCurl((int)fingerName);
        }
    }
}
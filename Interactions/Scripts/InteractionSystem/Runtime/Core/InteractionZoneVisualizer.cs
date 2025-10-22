using UnityEngine;

namespace Shababeek.Interactions.Core
{
    [RequireComponent(typeof(CameraRig))]
    [AddComponentMenu("Shababeek/Interactions/VR Interaction Zone Visualizer")]
    public class VRInteractionZoneVisualizer : MonoBehaviour
    {
        #region Constants
        
        // Default player values
        private const float defaultStandingHeight = 1.7f;
        private const float defaultSeatedHeight = 1.2f;
        private const float defaultPlayerRadius = 0.25f;
        
        // Room-scale preset values
        private const float roomScaleDefaultPlayAreaWidth = 2.0f;
        private const float roomScaleDefaultPlayAreaDepth = 2.0f;
        private const float roomScaleDefaultAtEdgeMax = 0.3f;
        private const float roomScaleDefaultExtendedMax = 0.6f;
        private const float roomScaleDefaultMaximumMax = 0.8f;
        private const float roomScaleDefaultHeightMinPercent = -70f;
        private const float roomScaleDefaultHeightMaxPercent = 0f;
        private const float roomScaleDefaultOptimalHeightMinPercent = -53f;
        private const float roomScaleDefaultOptimalHeightMaxPercent = -18f;
        
        // Seated preset values
        private const float seatedDefaultOptimalMin = 0.3f;
        private const float seatedDefaultOptimalMax = 0.5f;
        private const float seatedDefaultExtendedMax = 0.7f;
        private const float seatedDefaultMaximumMax = 0.9f;
        private const float seatedDefaultHeightMinPercent = -40f;
        private const float seatedDefaultHeightMaxPercent = 25f;
        private const float seatedDefaultOptimalHeightMinPercent = -25f;
        private const float seatedDefaultOptimalHeightMaxPercent = 8f;
        private const float seatedDefaultArcAngle = 60f;
        
        // Common defaults
        private const float defaultDeadZoneRadius = 0.25f;
        
        #endregion
        
        [Header("VR Mode")]
        [SerializeField] private VRMode vrMode = VRMode.RoomScale;
        
        [Header("Player Representation")]
        [Tooltip("Radius of the player cylinder representation")]
        [SerializeField] private float playerRadius = defaultPlayerRadius;
        
        [Header("Room-Scale Play Area")]
        [Tooltip("Width of the play area (X-axis)"), SerializeField, HideInInspector]
        private float playAreaWidth = roomScaleDefaultPlayAreaWidth;
        [Tooltip("Depth of the play area (Z-axis)"), SerializeField, HideInInspector]
        private float playAreaDepth = roomScaleDefaultPlayAreaDepth;
        
        [Header("Distance Zones - Seated (forward from head)")]
        [Tooltip("Minimum comfortable grab distance"), SerializeField, HideInInspector]
        private float seatedOptimalMin = seatedDefaultOptimalMin;
        [Tooltip("Maximum comfortable grab distance"), SerializeField, HideInInspector]
        private float seatedOptimalMax = seatedDefaultOptimalMax;
        [Tooltip("Maximum extended reach distance"), SerializeField, HideInInspector]
        private float seatedExtendedMax = seatedDefaultExtendedMax;
        [Tooltip("Absolute maximum reach distance"), SerializeField, HideInInspector]
        private float seatedMaximumMax = seatedDefaultMaximumMax;
        
        [Header("Distance Zones - Room-Scale (from play area edge)")]
        [Tooltip("At edge zone"), SerializeField, HideInInspector]
        private float roomScaleAtEdgeMax = roomScaleDefaultAtEdgeMax;
        [Tooltip("Extended reach from edge"), SerializeField, HideInInspector]
        private float roomScaleExtendedMax = roomScaleDefaultExtendedMax;
        [Tooltip("Maximum reach from edge"), SerializeField, HideInInspector]
        private float roomScaleMaximumMax = roomScaleDefaultMaximumMax;
        
        [Header("Height Ranges (% of player height)")]
        [Tooltip("Minimum height offset (% of player height)"), SerializeField]
        private float heightMinPercent = roomScaleDefaultHeightMinPercent;
        [Tooltip("Maximum height offset (% of player height)"), SerializeField]
        private float heightMaxPercent = roomScaleDefaultHeightMaxPercent;
        
        [Header("Optimal Height Range (% of player height)")]
        [Tooltip("Minimum optimal grab height"), SerializeField]
        private float optimalHeightMinPercent = roomScaleDefaultOptimalHeightMinPercent;
        [Tooltip("Maximum optimal grab height"), SerializeField]
        private float optimalHeightMaxPercent = roomScaleDefaultOptimalHeightMaxPercent;
        
        [Header("Visualization Settings")]
        [SerializeField] private bool showOptimalZone = true;
        [SerializeField] private bool showExtendedZone = true;
        [SerializeField] private bool showMaximumZone = true;
        [SerializeField] private bool showDeadZone = true;
        [Tooltip("Dead zone radius around the head (too close to interact with)")]
        [SerializeField] private float deadZoneRadius = defaultDeadZoneRadius;
        [SerializeField, HideInInspector] private bool showPlayAreaBounds = true;
        [SerializeField] private bool showPlayerRepresentation = true;
        
        [Header("Arc Settings")]
        [Tooltip("Number of segments for drawing arcs (higher = smoother)")]
        [Range(8, 32), SerializeField]
        private int arcSegments = 16;
        [Tooltip("Horizontal arc angle (degrees from center) - for Seated mode")]
        [Range(30f, 90f), SerializeField, HideInInspector]
        private float seatedArcAngle = seatedDefaultArcAngle;
        
        private CameraRig _cameraRig;
        
        #region Public Properties
        
        public VRMode VrMode => vrMode;
        
        // Player
        public float PlayerRadius => playerRadius;
        
        // Play Area
        public float PlayAreaWidth => playAreaWidth;
        public float PlayAreaDepth => playAreaDepth;
        
        // Seated distances
        public float SeatedOptimalMin => seatedOptimalMin;
        public float SeatedOptimalMax => seatedOptimalMax;
        public float SeatedExtendedMax => seatedExtendedMax;
        public float SeatedMaximumMax => seatedMaximumMax;
        
        // Room-scale distances
        public float RoomScaleAtEdgeMax => roomScaleAtEdgeMax;
        public float RoomScaleExtendedMax => roomScaleExtendedMax;
        public float RoomScaleMaximumMax => roomScaleMaximumMax;
        
        // Height percentages
        public float HeightMinPercent => heightMinPercent;
        public float HeightMaxPercent => heightMaxPercent;
        public float OptimalHeightMinPercent => optimalHeightMinPercent;
        public float OptimalHeightMaxPercent => optimalHeightMaxPercent;
        
        // Visualization
        public bool ShowOptimalZone => showOptimalZone;
        public bool ShowExtendedZone => showExtendedZone;
        public bool ShowMaximumZone => showMaximumZone;
        public bool ShowDeadZone => showDeadZone;
        public float DeadZoneRadius => deadZoneRadius;
        public bool ShowPlayAreaBounds => showPlayAreaBounds;
        public bool ShowPlayerRepresentation => showPlayerRepresentation;
        public int ArcSegments => arcSegments;
        public float SeatedArcAngle => seatedArcAngle;
        
        #endregion
        
        #region Public Setters (for editor)
        
        public void SetPlayAreaWidth(float value) => playAreaWidth = value;
        public void SetPlayAreaDepth(float value) => playAreaDepth = value;
        public void SetSeatedOptimalMin(float value) => seatedOptimalMin = value;
        public void SetSeatedOptimalMax(float value) => seatedOptimalMax = value;
        public void SetSeatedExtendedMax(float value) => seatedExtendedMax = value;
        public void SetSeatedMaximumMax(float value) => seatedMaximumMax = value;
        public void SetRoomScaleAtEdgeMax(float value) => roomScaleAtEdgeMax = value;
        public void SetRoomScaleExtendedMax(float value) => roomScaleExtendedMax = value;
        public void SetRoomScaleMaximumMax(float value) => roomScaleMaximumMax = value;
        public void SetShowPlayAreaBounds(bool value) => showPlayAreaBounds = value;
        public void SetSeatedArcAngle(float value) => seatedArcAngle = value;
        
        #endregion
        
        public CameraRig GetCameraRig()
        {
            if (_cameraRig == null)
            {
                _cameraRig = GetComponent<CameraRig>();
            }
            return _cameraRig;
        }
        
        public Vector3 GetHeadPosition()
        {
            if (_cameraRig == null) _cameraRig = GetComponent<CameraRig>();
            if (_cameraRig == null) return transform.position + new Vector3(0, defaultStandingHeight, 0);
            var offset = _cameraRig.Offset;
            if (offset != null)
            {
                return offset.position;
            }
            return _cameraRig.transform.position + new Vector3(0, _cameraRig.CameraHeight, 0);
        }
        
        public float GetPlayerHeight()
        {
            if (_cameraRig == null) _cameraRig = GetComponent<CameraRig>();
            return _cameraRig != null ? _cameraRig.CameraHeight : defaultStandingHeight;
        }
        
        public float GetHeightFromPercent(float percent)
        {
            return GetPlayerHeight() * (percent / 100f);
        }
        
        /// <summary>
        /// Apply preset values based on VR mode
        /// </summary>
        public void ApplyVRModePreset()
        {
            switch (vrMode)
            {
                case VRMode.RoomScale:
                    ApplyRoomScalePreset();
                    break;
                case VRMode.Seated:
                    ApplySeatedPreset();
                    break;
            }
        }
        
        private void ApplyRoomScalePreset()
        {
            playAreaWidth = roomScaleDefaultPlayAreaWidth;
            playAreaDepth = roomScaleDefaultPlayAreaDepth;
            
            roomScaleAtEdgeMax = roomScaleDefaultAtEdgeMax;
            roomScaleExtendedMax = roomScaleDefaultExtendedMax;
            roomScaleMaximumMax = roomScaleDefaultMaximumMax;
            
            heightMinPercent = roomScaleDefaultHeightMinPercent;
            heightMaxPercent = roomScaleDefaultHeightMaxPercent;
            optimalHeightMinPercent = roomScaleDefaultOptimalHeightMinPercent;
            optimalHeightMaxPercent = roomScaleDefaultOptimalHeightMaxPercent;
            
            deadZoneRadius = defaultDeadZoneRadius;
            showPlayAreaBounds = true;
        }
        
        private void ApplySeatedPreset()
        {
            seatedOptimalMin = seatedDefaultOptimalMin;
            seatedOptimalMax = seatedDefaultOptimalMax;
            seatedExtendedMax = seatedDefaultExtendedMax;
            seatedMaximumMax = seatedDefaultMaximumMax;
            
            heightMinPercent = seatedDefaultHeightMinPercent;
            heightMaxPercent = seatedDefaultHeightMaxPercent;
            optimalHeightMinPercent = seatedDefaultOptimalHeightMinPercent;
            optimalHeightMaxPercent = seatedDefaultOptimalHeightMaxPercent;
            
            seatedArcAngle = seatedDefaultArcAngle;
            deadZoneRadius = defaultDeadZoneRadius;
            showPlayAreaBounds = false;
        }
    }
    
    public enum VRMode
    {
        [Tooltip("Standing/walking VR with play area bounds")]
        RoomScale,
        
        [Tooltip("Seated VR experience")]
        Seated
    }
}
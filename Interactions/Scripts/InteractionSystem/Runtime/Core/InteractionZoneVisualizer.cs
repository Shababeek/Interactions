using UnityEngine;

namespace Shababeek.Interactions.Core
{
    [RequireComponent(typeof(CameraRig))]
    [AddComponentMenu("Shababeek/Interactions/VR Interaction Zone Visualizer")]
    public class VRInteractionZoneVisualizer : MonoBehaviour
    {
        [Header("Distance Zones (forward from player)")]
        [Tooltip("Minimum comfortable grab distance"), SerializeField]
        private float optimalMinDistance = 0.3f;
        [Tooltip("Maximum comfortable grab distance"), SerializeField]
        private float optimalMaxDistance = 0.6f;
        [Tooltip("Maximum extended reach distance"), SerializeField]
        private float extendedMaxDistance = 0.8f;
        [Tooltip("Absolute maximum reach distance"), SerializeField]
        private float maximumMaxDistance = 1.0f;
        
        [Header("Height Ranges (relative to head height)")]
        [Tooltip("Minimum height offset from head (negative = below head)"), SerializeField]
        private float heightMin = -0.9f;
        [Tooltip("Maximum height offset from head (negative = below head)"), SerializeField]
        private float heightMax = -0.2f;
        
        [Header("Optimal Height Range (relative to head height)")]
        [Tooltip("Minimum optimal grab height (chest level: around -0.7m)"), SerializeField]
        private float optimalHeightMin = -0.7f;
        [Tooltip("Maximum optimal grab height (face to chest: around -0.3m)"), SerializeField]
        private float optimalHeightMax = -0.3f;
        
        [Header("Visualization Settings")]
        [SerializeField] private bool showOptimalZone = true;
        [SerializeField] private bool showExtendedZone = true;
        [SerializeField] private bool showMaximumZone = true;
        [SerializeField] private bool showDeadZone = true;
        [Tooltip("Dead zone radius around the head (too close to interact with)")]
        [SerializeField] private float deadZoneRadius = 0.25f;
        [SerializeField] private bool showLegend = true;
        
        [Header("Arc Settings")]
        [Tooltip("Number of segments for drawing arcs (higher = smoother)")]
        [Range(8, 32), SerializeField]
        private int arcSegments = 16;
        [Tooltip("Horizontal arc angle (degrees from center)")]
        [Range(30f, 90f), SerializeField]
        private float arcAngle = 60;
        
        private CameraRig _cameraRig;
        
        #region Public Properties
        
        public float OptimalMinDistance => optimalMinDistance;
        public float OptimalMaxDistance => optimalMaxDistance;
        public float ExtendedMaxDistance => extendedMaxDistance;
        public float MaximumMaxDistance => maximumMaxDistance;
        public float HeightMin => heightMin;
        public float HeightMax => heightMax;
        public float OptimalHeightMin => optimalHeightMin;
        public float OptimalHeightMax => optimalHeightMax;
        public bool ShowOptimalZone => showOptimalZone;
        public bool ShowExtendedZone => showExtendedZone;
        public bool ShowMaximumZone => showMaximumZone;
        public bool ShowDeadZone => showDeadZone;
        public float DeadZoneRadius => deadZoneRadius;
        public bool ShowLegend => showLegend;
        public int ArcSegments => arcSegments;
        public float ArcAngle => arcAngle;
        
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
            if (_cameraRig == null) return transform.position + new Vector3(0, 1, 0);
            var offset = _cameraRig.Offset;
            if (offset != null)
            {
                return offset.position;
            }
            return _cameraRig.transform.position + new Vector3(0, _cameraRig.CameraHeight, 0);
        }
    }
}
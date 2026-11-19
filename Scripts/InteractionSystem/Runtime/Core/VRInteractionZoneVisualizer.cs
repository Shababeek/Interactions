using System;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Holds the reach zones drawn around the Camera Rig in the Scene view, as a guide for placing interactables.
    /// Room-scale and seated values are stored separately; what is drawn is chosen in the editor, not here.
    /// </summary>
    [RequireComponent(typeof(CameraRig))]
    [AddComponentMenu("Shababeek/Interactions/Debug/VR Interaction Zone Visualizer")]
    public class VRInteractionZoneVisualizer : MonoBehaviour, ISerializationCallbackReceiver
    {
        private const float DefaultEyeHeight = 1.7f;
        private const float LegacyMigrated = -1f;

        [Tooltip("Which set of zones to draw. Each mode keeps its own values.")]
        [SerializeField] private VRMode vrMode = VRMode.RoomScale;

        [Tooltip("Radius of the player body drawn at the rig, in metres.")]
        [SerializeField, Min(0.05f)] private float playerRadius = 0.25f;

        [Tooltip("Radius around the head that is too close to interact with comfortably, in metres.")]
        [SerializeField, Min(0f)] private float deadZoneRadius = 0.2f;

        [Tooltip("Zones used while the mode is Room Scale.")]
        [SerializeField] private RoomScaleZoneSettings roomScale = new RoomScaleZoneSettings();

        [Tooltip("Zones used while the mode is Seated.")]
        [SerializeField] private SeatedZoneSettings seated = new SeatedZoneSettings();

        // Flat fields from the previous layout. Read once to fill the per-mode settings, then parked at LegacyMigrated.
        [SerializeField, HideInInspector] private float playAreaWidth = LegacyMigrated;
        [SerializeField, HideInInspector] private float playAreaDepth;
        [SerializeField, HideInInspector] private float seatedOptimalMin;
        [SerializeField, HideInInspector] private float seatedOptimalMax;
        [SerializeField, HideInInspector] private float seatedExtendedMax;
        [SerializeField, HideInInspector] private float seatedMaximumMax;
        [SerializeField, HideInInspector] private float roomScaleAtEdgeMax;
        [SerializeField, HideInInspector] private float roomScaleExtendedMax;
        [SerializeField, HideInInspector] private float roomScaleMaximumMax;
        [SerializeField, HideInInspector] private float heightMinPercent;
        [SerializeField, HideInInspector] private float heightMaxPercent;
        [SerializeField, HideInInspector] private float optimalHeightMinPercent;
        [SerializeField, HideInInspector] private float optimalHeightMaxPercent;
        [SerializeField, HideInInspector] private float seatedArcAngle;

        private CameraRig _cameraRig;

        /// <summary>Mode whose zones are drawn.</summary>
        public VRMode VrMode
        {
            get => vrMode;
            set => vrMode = value;
        }

        /// <summary>Radius of the player body, in metres.</summary>
        public float PlayerRadius => playerRadius;

        /// <summary>Radius around the head that is too close to use, in metres.</summary>
        public float DeadZoneRadius => deadZoneRadius;

        /// <summary>Room-scale zone values.</summary>
        public RoomScaleZoneSettings RoomScale => roomScale;

        /// <summary>Seated zone values.</summary>
        public SeatedZoneSettings Seated => seated;

        /// <summary>Camera Rig this component sits on.</summary>
        public CameraRig CameraRig
        {
            get
            {
                if (!_cameraRig) _cameraRig = GetComponent<CameraRig>();
                return _cameraRig;
            }
        }

        /// <summary>Eye height of the player above the rig origin, in metres.</summary>
        public float EyeHeight => CameraRig ? CameraRig.CameraHeight : DefaultEyeHeight;

        /// <summary>World position of the player's head.</summary>
        public Vector3 GetHeadPosition()
        {
            var rig = CameraRig;
            if (rig && rig.Offset) return rig.Offset.position;
            return transform.position + transform.up * EyeHeight;
        }

        /// <summary>Converts a percentage of eye height to metres.</summary>
        public float PercentToMetres(float percent) => EyeHeight * percent / 100f;

        /// <summary>Restores the default values of one mode and leaves the other untouched.</summary>
        public void ResetToDefaults(VRMode mode)
        {
            if (mode == VRMode.RoomScale) roomScale = new RoomScaleZoneSettings();
            else seated = new SeatedZoneSettings();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize() => MigrateLegacyFields();

        private void OnValidate()
        {
            roomScale.Validate();
            seated.Validate();
        }

        private void MigrateLegacyFields()
        {
            if (playAreaWidth < 0f) return;

            roomScale.playAreaWidth = playAreaWidth;
            roomScale.playAreaDepth = playAreaDepth;
            roomScale.optimalDistance = roomScaleAtEdgeMax;
            roomScale.extendedDistance = roomScaleExtendedMax;
            roomScale.maximumDistance = roomScaleMaximumMax;

            seated.optimalMin = seatedOptimalMin;
            seated.optimalMax = seatedOptimalMax;
            seated.extendedDistance = seatedExtendedMax;
            seated.maximumDistance = seatedMaximumMax;
            seated.arcAngle = seatedArcAngle;

            // The old layout shared one set of heights; they belonged to whichever mode was active when saved.
            var reach = new Vector2(heightMinPercent, heightMaxPercent);
            var optimal = new Vector2(optimalHeightMinPercent, optimalHeightMaxPercent);
            if (vrMode == VRMode.RoomScale)
            {
                roomScale.reachHeightPercent = reach;
                roomScale.optimalHeightPercent = optimal;
            }
            else
            {
                seated.reachHeightPercent = reach;
                seated.optimalHeightPercent = optimal;
            }

            playAreaWidth = LegacyMigrated;
        }

        internal static void ValidateHeights(ref Vector2 reach, ref Vector2 optimal)
        {
            if (reach.y < reach.x) reach.y = reach.x;
            optimal.x = Mathf.Clamp(optimal.x, reach.x, reach.y);
            optimal.y = Mathf.Clamp(optimal.y, optimal.x, reach.y);
        }
    }
}

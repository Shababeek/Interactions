using System;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>Which kind of VR experience the interaction zones are drawn for.</summary>
    public enum VRMode
    {
        /// <summary>Standing or walking inside a play area.</summary>
        RoomScale,

        /// <summary>Seated or standing in one spot.</summary>
        Seated
    }

    /// <summary>Reach zones for a player who walks inside a play area.</summary>
    [Serializable]
    public class RoomScaleZoneSettings
    {
        [Tooltip("Width of the walkable play area in metres (rig X axis).")]
        [Min(0.1f)] public float playAreaWidth = 2f;

        [Tooltip("Depth of the walkable play area in metres (rig Z axis).")]
        [Min(0.1f)] public float playAreaDepth = 2f;

        [Tooltip("How far past the play area edge is comfortable to reach, in metres.")]
        [Min(0f)] public float optimalDistance = 0.3f;

        [Tooltip("How far past the play area edge a stretched arm reaches, in metres.")]
        [Min(0f)] public float extendedDistance = 0.6f;

        [Tooltip("Furthest distance past the play area edge the player can reach at all, in metres.")]
        [Min(0f)] public float maximumDistance = 0.8f;

        [Tooltip("Lowest and highest reachable height, as a percentage of eye height measured from the head (negative is below the head).")]
        public Vector2 reachHeightPercent = new Vector2(-60f, 10f);

        [Tooltip("Lowest and highest comfortable height, as a percentage of eye height measured from the head (negative is below the head).")]
        public Vector2 optimalHeightPercent = new Vector2(-40f, -10f);

        /// <summary>Keeps distances ordered and heights inside each other.</summary>
        public void Validate()
        {
            playAreaWidth = Mathf.Max(0.1f, playAreaWidth);
            playAreaDepth = Mathf.Max(0.1f, playAreaDepth);
            optimalDistance = Mathf.Max(0f, optimalDistance);
            extendedDistance = Mathf.Max(optimalDistance, extendedDistance);
            maximumDistance = Mathf.Max(extendedDistance, maximumDistance);
            VRInteractionZoneVisualizer.ValidateHeights(ref reachHeightPercent, ref optimalHeightPercent);
        }
    }

    /// <summary>Reach zones for a player who stays in one spot.</summary>
    [Serializable]
    public class SeatedZoneSettings
    {
        [Tooltip("Closest comfortable distance in front of the head, in metres.")]
        [Min(0f)] public float optimalMin = 0.3f;

        [Tooltip("Furthest comfortable distance in front of the head, in metres.")]
        [Min(0f)] public float optimalMax = 0.5f;

        [Tooltip("Distance a stretched arm reaches, in metres.")]
        [Min(0f)] public float extendedDistance = 0.7f;

        [Tooltip("Furthest distance the player can reach by leaning, in metres.")]
        [Min(0f)] public float maximumDistance = 0.9f;

        [Tooltip("Half angle of the reach arc in degrees, measured from straight ahead.")]
        [Range(30f, 180f)] public float arcAngle = 60f;

        [Tooltip("Lowest and highest reachable height, as a percentage of eye height measured from the head (negative is below the head).")]
        public Vector2 reachHeightPercent = new Vector2(-35f, 15f);

        [Tooltip("Lowest and highest comfortable height, as a percentage of eye height measured from the head (negative is below the head).")]
        public Vector2 optimalHeightPercent = new Vector2(-25f, 5f);

        /// <summary>Keeps distances ordered and heights inside each other.</summary>
        public void Validate()
        {
            optimalMin = Mathf.Max(0f, optimalMin);
            optimalMax = Mathf.Max(optimalMin, optimalMax);
            extendedDistance = Mathf.Max(optimalMax, extendedDistance);
            maximumDistance = Mathf.Max(extendedDistance, maximumDistance);
            arcAngle = Mathf.Clamp(arcAngle, 30f, 180f);
            VRInteractionZoneVisualizer.ValidateHeights(ref reachHeightPercent, ref optimalHeightPercent);
        }
    }
}

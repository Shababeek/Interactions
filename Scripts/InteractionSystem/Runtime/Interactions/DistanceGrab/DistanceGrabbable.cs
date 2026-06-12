using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Optional per-object distance-grab settings. All Grabables are distance-grabbable by
    /// default; add this component to opt an object out or override the grabber's flight timing.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Shababeek/Interactions/Interactables/Distance Grabbable")]
    public class DistanceGrabbable : MonoBehaviour
    {
        [Tooltip("Allow this object to be distance grabbed. Untick to opt out.")]
        [SerializeField] private bool allowDistanceGrab = true;

        [Tooltip("Override flight time in seconds for this object. 0 = use the grabber's distance-scaled default.")]
        [SerializeField, Min(0f)] private float flightTimeOverride = 0f;

        /// <summary>Whether this object may be distance grabbed.</summary>
        public bool AllowDistanceGrab => allowDistanceGrab;

        /// <summary>Flight time override in seconds; 0 means use the grabber default.</summary>
        public float FlightTimeOverride => flightTimeOverride;
    }
}

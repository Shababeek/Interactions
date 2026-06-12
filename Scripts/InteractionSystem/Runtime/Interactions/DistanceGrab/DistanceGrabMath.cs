using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>Ballistic math for distance-grab launches. Stateless and unit-testable.</summary>
    public static class DistanceGrabMath
    {
        /// <summary>
        /// Initial velocity that carries a body from 'from' to 'to' in 'flightTime' seconds
        /// under the given gravity (pass Vector3.zero for gravity-less bodies).
        /// </summary>
        public static Vector3 ComputeLaunchVelocity(Vector3 from, Vector3 to, float flightTime, Vector3 gravity)
        {
            flightTime = Mathf.Max(0.01f, flightTime);
            return (to - from) / flightTime - 0.5f * gravity * flightTime;
        }

        /// <summary>Flight time for a distance, scaled and clamped to a range.</summary>
        public static float ComputeFlightTime(float distance, float timePerMeter, Vector2 range)
        {
            return Mathf.Clamp(distance * timePerMeter, range.x, range.y);
        }
    }
}

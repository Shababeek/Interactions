using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>Result of fitting one finger against a surface.</summary>
    public struct FingerFitResult
    {
        /// <summary>True when the finger's arc intersects the surface.</summary>
        public bool hit;

        /// <summary>Curl value (0-1) at which the fingertip first touches the surface.</summary>
        public float curl;

        /// <summary>World-space contact point.</summary>
        public Vector3 point;

        /// <summary>World-space contact normal.</summary>
        public Vector3 normal;
    }

    /// <summary>
    /// Sweeps baked finger arcs against a specific set of colliders to find the curl value where
    /// each fingertip first touches the surface. Stateless; safe to call from editor tools.
    /// </summary>
    public static class PoseFitSolver
    {
        /// <summary>
        /// Fits all five fingers. handLocalToWorld maps baked hand-local arc space to world
        /// (the hand root's localToWorldMatrix at the grab pose).
        /// </summary>
        public static FingerFitResult[] Fit(FingerArcs arcs, Matrix4x4 handLocalToWorld, IReadOnlyList<Collider> colliders)
        {
            var results = new FingerFitResult[5];
            for (int f = 0; f < 5; f++)
            {
                results[f] = FitFinger(arcs, f, handLocalToWorld, colliders);
            }
            return results;
        }

        /// <summary>Fits a single finger (0=Thumb..4=Pinky); no hit leaves curl at 1.</summary>
        public static FingerFitResult FitFinger(FingerArcs arcs, int finger, Matrix4x4 handLocalToWorld, IReadOnlyList<Collider> colliders)
        {
            var result = new FingerFitResult { hit = false, curl = 1f };
            if (arcs == null || colliders == null || colliders.Count == 0) return result;

            int count = arcs.SampleCount;
            float radius = arcs.GetRadius(finger);

            for (int k = 0; k < count - 1; k++)
            {
                Vector3 a = handLocalToWorld.MultiplyPoint3x4(arcs.GetSample(finger, k).tip);
                Vector3 b = handLocalToWorld.MultiplyPoint3x4(arcs.GetSample(finger, k + 1).tip);
                Vector3 segment = b - a;
                float length = segment.magnitude;
                if (length < 1e-6f) continue;

                // Ray extended by the finger radius approximates a sphere cast along the arc
                // segment; the radius is subtracted from the hit distance so the finger stops
                // at surface contact rather than at bone penetration.
                if (RaycastColliders(new Ray(a, segment / length), length + radius, colliders, out var hit))
                {
                    float hitFraction = Mathf.Clamp01((hit.distance - radius) / length);
                    result.hit = true;
                    result.curl = Mathf.Clamp01((k + hitFraction) / (count - 1));
                    result.point = hit.point;
                    result.normal = hit.normal;
                    return result;
                }
            }

            return result;
        }

        private static bool RaycastColliders(Ray ray, float maxDistance, IReadOnlyList<Collider> colliders, out RaycastHit closest)
        {
            closest = default;
            var found = false;
            var best = float.MaxValue;
            for (int i = 0; i < colliders.Count; i++)
            {
                var col = colliders[i];
                if (col == null || !col.enabled) continue;
                if (col.Raycast(ray, out var hit, maxDistance) && hit.distance < best)
                {
                    best = hit.distance;
                    closest = hit;
                    found = true;
                }
            }
            return found;
        }
    }
}

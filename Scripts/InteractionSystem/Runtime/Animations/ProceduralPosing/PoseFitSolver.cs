using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions.Animations
{
    /// <summary>Result of fitting one finger against a surface.</summary>
    public struct FingerFitResult
    {
        /// <summary>True when the finger's arc intersects the surface.</summary>
        public bool hit;

        /// <summary>Curl value (0-1) at which the finger first touches the surface.</summary>
        public float curl;

        /// <summary>World-space contact point.</summary>
        public Vector3 point;

        /// <summary>World-space contact normal.</summary>
        public Vector3 normal;
    }

    /// <summary>
    /// Sweeps baked finger arcs against a specific set of colliders to find the curl value where
    /// each finger first touches the surface. Both the fingertip and the mid-phalanx are swept so
    /// larger objects stop the curl when the phalanx touches, not only the tip.
    /// Stateless; safe to call from editor tools.
    /// </summary>
    public static class PoseFitSolver
    {
        /// <summary>Default extra clearance in meters kept between the finger and the surface.</summary>
        public const float DefaultSkin = 0.002f;

        private const float MinBackoffCos = 0.35f;

        /// <summary>
        /// Fits all five fingers. handLocalToWorld maps baked hand-local arc space to world
        /// (the hand root's localToWorldMatrix at the grab pose).
        /// </summary>
        public static FingerFitResult[] Fit(FingerArcs arcs, Matrix4x4 handLocalToWorld, IReadOnlyList<Collider> colliders, float skin = DefaultSkin)
        {
            var results = new FingerFitResult[5];
            for (int f = 0; f < 5; f++)
            {
                results[f] = FitFinger(arcs, f, handLocalToWorld, colliders, skin);
            }
            return results;
        }

        /// <summary>Fits a single finger (0=Thumb..4=Pinky); no hit leaves curl at 1.</summary>
        public static FingerFitResult FitFinger(FingerArcs arcs, int finger, Matrix4x4 handLocalToWorld, IReadOnlyList<Collider> colliders, float skin = DefaultSkin)
        {
            var result = new FingerFitResult { hit = false, curl = 1f };
            if (arcs == null || colliders == null || colliders.Count == 0) return result;

            float radius = arcs.GetRadius(finger) + skin;

            // A finger already touching (or inside) the surface at the open pose can never be
            // reached by a forward ray sweep — report contact at curl 0 so it stays open.
            Vector3 openTip = handLocalToWorld.MultiplyPoint3x4(arcs.GetSample(finger, 0).tip);
            if (TryGetRestingContact(openTip, radius, colliders, out var restPoint, out var restNormal))
            {
                result.hit = true;
                result.curl = 0f;
                result.point = restPoint;
                result.normal = restNormal;
                return result;
            }

            bool tipHit = SweepProbe(arcs, finger, handLocalToWorld, colliders, radius, false, out var tipResult);
            bool midHit = SweepProbe(arcs, finger, handLocalToWorld, colliders, radius, true, out var midResult);

            if (tipHit && (!midHit || tipResult.curl <= midResult.curl)) return tipResult;
            if (midHit) return midResult;
            return result;
        }

        private static bool SweepProbe(FingerArcs arcs, int finger, Matrix4x4 handLocalToWorld, IReadOnlyList<Collider> colliders, float radius, bool useMid, out FingerFitResult result)
        {
            result = new FingerFitResult { hit = false, curl = 1f };
            int count = arcs.SampleCount;

            for (int k = 0; k < count - 1; k++)
            {
                var sampleA = arcs.GetSample(finger, k);
                var sampleB = arcs.GetSample(finger, k + 1);
                Vector3 a = handLocalToWorld.MultiplyPoint3x4(useMid ? sampleA.mid : sampleA.tip);
                Vector3 b = handLocalToWorld.MultiplyPoint3x4(useMid ? sampleB.mid : sampleB.tip);
                Vector3 segment = b - a;
                float length = segment.magnitude;
                if (length < 1e-6f) continue;

                Vector3 direction = segment / length;
                if (!RaycastColliders(new Ray(a, direction), length + radius, colliders, out var hit)) continue;

                // The ray approximates a sphere cast; back the hit off by the radius projected
                // onto the surface slope so oblique contacts stop at the surface instead of
                // sinking in. The cosine clamp keeps grazing hits from backing off to infinity.
                float slopeCos = Mathf.Max(Mathf.Abs(Vector3.Dot(direction, hit.normal)), MinBackoffCos);
                float hitFraction = Mathf.Clamp01((hit.distance - radius / slopeCos) / length);
                result.hit = true;
                result.curl = Mathf.Clamp01((k + hitFraction) / (count - 1));
                result.point = hit.point;
                result.normal = hit.normal;
                return true;
            }

            return false;
        }

        private static bool TryGetRestingContact(Vector3 point, float radius, IReadOnlyList<Collider> colliders, out Vector3 contactPoint, out Vector3 contactNormal)
        {
            contactPoint = point;
            contactNormal = Vector3.up;

            for (int i = 0; i < colliders.Count; i++)
            {
                var col = colliders[i];
                if (col == null || !col.enabled) continue;
                if (col is MeshCollider mesh && !mesh.convex) continue; // ClosestPoint unsupported

                Vector3 closest = col.ClosestPoint(point);
                Vector3 delta = point - closest;
                if (delta.sqrMagnitude > radius * radius) continue;

                contactPoint = closest;
                contactNormal = delta.sqrMagnitude > 1e-12f
                    ? delta.normalized
                    : (point - col.bounds.center).normalized;
                return true;
            }

            return false;
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


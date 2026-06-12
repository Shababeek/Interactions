using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Look-rotation solve for two-handed grabbing: positions an object so its primary grip
    /// anchor sits at the primary hand while its grip axis aims at the secondary hand.
    /// Stateless and unit-testable.
    /// </summary>
    public static class TwoHandSolver
    {
        /// <summary>
        /// Solves the object pose from both hand positions and the grip anchors.
        /// </summary>
        /// <param name="primaryHand">World position of the primary hand.</param>
        /// <param name="secondaryHand">World position of the secondary hand.</param>
        /// <param name="upHint">World up hint (primary hand's up) controlling roll.</param>
        /// <param name="primaryAnchorLocal">Primary grip anchor in object-local space.</param>
        /// <param name="secondaryAnchorLocal">Secondary grip anchor in object-local space.</param>
        /// <param name="localScale">Object lossy scale applied to the anchors.</param>
        public static (Vector3 position, Quaternion rotation) Solve(
            Vector3 primaryHand, Vector3 secondaryHand, Vector3 upHint,
            Vector3 primaryAnchorLocal, Vector3 secondaryAnchorLocal, Vector3 localScale)
        {
            Vector3 worldDirection = secondaryHand - primaryHand;
            Vector3 scaledPrimaryAnchor = Vector3.Scale(primaryAnchorLocal, localScale);
            Vector3 scaledSecondaryAnchor = Vector3.Scale(secondaryAnchorLocal, localScale);
            Vector3 localDirection = scaledSecondaryAnchor - scaledPrimaryAnchor;

            Quaternion rotation;
            if (worldDirection.sqrMagnitude < 1e-8f || localDirection.sqrMagnitude < 1e-8f)
            {
                // Hands or anchors coincide — direction is undefined; keep identity rotation
                // and let the caller smooth from its current pose.
                rotation = Quaternion.identity;
            }
            else
            {
                Vector3 localUpHint = Mathf.Abs(Vector3.Dot(localDirection.normalized, Vector3.up)) > 0.99f
                    ? Vector3.forward
                    : Vector3.up;
                Quaternion worldFrame = Quaternion.LookRotation(worldDirection.normalized, upHint);
                Quaternion localFrame = Quaternion.LookRotation(localDirection.normalized, localUpHint);
                rotation = worldFrame * Quaternion.Inverse(localFrame);
            }

            Vector3 position = primaryHand - rotation * scaledPrimaryAnchor;
            return (position, rotation);
        }
    }
}

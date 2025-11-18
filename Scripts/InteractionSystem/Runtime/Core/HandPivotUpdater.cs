using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Mirrors the positional/rotational data coming from configured hand input providers
    /// onto the left and right hand pivots.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Hand Pivot Updater")]
    public class HandPivotUpdater : MonoBehaviour
    {
        [SerializeField] private Config config;
        [SerializeField] private Transform leftHandPivot;
        [SerializeField] private Transform rightHandPivot;

        private void LateUpdate()
        {
            if (config == null)
                return;

            ApplyProviderToPivot(config[HandIdentifier.Left], leftHandPivot);
            ApplyProviderToPivot(config[HandIdentifier.Right], rightHandPivot);
        }

        private static void ApplyProviderToPivot(IHandInputProvider provider, Transform pivot)
        {
            if (provider == null || pivot == null)
            {
                return;                
            }
                pivot.localPosition = provider.Position;
                pivot.localRotation = provider.Rotation;
        }

        public void Initialize(Config configRef, Transform leftPivot, Transform rightPivot)
        {
            config = configRef;
            leftHandPivot = leftPivot;
            rightHandPivot = rightPivot;
        }
    }
}


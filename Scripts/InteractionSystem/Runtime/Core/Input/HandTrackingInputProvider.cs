using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Hand input provider that reads directly from XR Hand Subsystem for reliable hand tracking.
    /// Provides 0-1 finger curl values calculated from joint rotations relative to the hand.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Input Providers/Hand Tracking Input Provider")]
    public class HandTrackingInputProvider : HandInputProviderBase
    {
        [Header("Hand Tracking")]
        [Tooltip("Hand tracking reader component that provides finger curl data.")]
        [SerializeField] private HandTrackingReader handTrackingReader;
        
        [Tooltip("Automatically create HandTrackingReader if not assigned.")]
        [SerializeField] private bool autoCreateReader = true;
        
        /// <summary>
        /// Checks if hand tracking is active and providing data.
        /// </summary>
        public override bool IsActive => handTrackingReader != null && handTrackingReader.IsTracked;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Auto-create reader if needed
            if (handTrackingReader == null && autoCreateReader)
            {
                handTrackingReader = gameObject.AddComponent<HandTrackingReader>();
            }
        }
        
        protected override void UpdateFingerValues()
        {
            if (handTrackingReader == null || !handTrackingReader.IsTracked)
            {
                // Set all to zero when not tracked
                SetFingerValues(0f, 0f, 0f, 0f, 0f);
                return;
            }
            
            // Read finger curl values from hand tracking
            this[FingerName.Thumb] = handTrackingReader.GetFingerCurl(FingerName.Thumb);
            this[FingerName.Index] = handTrackingReader.GetFingerCurl(FingerName.Index);
            this[FingerName.Middle] = handTrackingReader.GetFingerCurl(FingerName.Middle);
            this[FingerName.Ring] = handTrackingReader.GetFingerCurl(FingerName.Ring);
            this[FingerName.Pinky] = handTrackingReader.GetFingerCurl(FingerName.Pinky);
        }
    }
}
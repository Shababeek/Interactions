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
        

        // Cached position/rotation for efficiency
        [SerializeField] private Vector3 _position = Vector3.zero;
        [SerializeField] private Quaternion _rotation = Quaternion.identity;
        [SerializeField] private uint _trackingState = 0;
    
        private void Awake()
        {
            EnsureDefaultPriority();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Auto-create reader if needed
            if (handTrackingReader == null && autoCreateReader)
            {
                handTrackingReader = gameObject.AddComponent<HandTrackingReader>();
                // Set handedness to match this provider
                handTrackingReader.Handedness = handedness;
            }
            else if (handTrackingReader != null)
            {
                // Ensure handedness matches
                handTrackingReader.Handedness = handedness;
            }
        }
        
        protected override void UpdateFingerValues()
        {
            if (handTrackingReader == null || !handTrackingReader.IsTracked)
            {
                // Set all to zero when not tracked
                SetFingerValues(0f, 0f, 0f, 0f, 0f);
                _position = Vector3.zero;
                _rotation = Quaternion.identity;
                _trackingState = 0;
                return;
            }
            
            // Read finger curl values from hand tracking
            this[FingerName.Thumb] = handTrackingReader.GetFingerCurl(FingerName.Thumb);
            this[FingerName.Index] = handTrackingReader.GetFingerCurl(FingerName.Index);
            this[FingerName.Middle] = handTrackingReader.GetFingerCurl(FingerName.Middle);
            this[FingerName.Ring] = handTrackingReader.GetFingerCurl(FingerName.Ring);
            this[FingerName.Pinky] = handTrackingReader.GetFingerCurl(FingerName.Pinky);
            
            // Update position, rotation, and tracking state
            if (handTrackingReader.TryGetHandPosition(out Vector3 pos))
            {
                _position = pos;
            }
            else
            {
                _position = Vector3.zero;
            }
            
            if (handTrackingReader.TryGetHandRotation(out Quaternion rot))
            {
                _rotation = rot;
            }
            else
            {
                _rotation = Quaternion.identity;
            }
            
            _trackingState = handTrackingReader.GetTrackingState();
        }

        private void EnsureDefaultPriority()
        {
            if (priority <= 0 || priority >= 20)
                priority = 10;
        }

        /// <summary>
        /// Current position of the hand in world space.
        /// </summary>
        public override Vector3 Position => _position;

        /// <summary>
        /// Current rotation of the hand in world space.
        /// </summary>
        public override Quaternion Rotation => _rotation;

        /// <summary>
        /// Current tracking state of the hand (flags: Position = 1, Rotation = 2, Both = 3).
        /// </summary>
        public override uint TrackingState => _trackingState;

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureDefaultPriority();
        }
#endif
    }
}
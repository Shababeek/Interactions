#if XR_HANDS_AVAILABLE
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Moves the Camera Rig's hand pivots from the tracked hand while hand tracking is the input
    /// source for that hand. The pivot's Tracked Pose Driver is switched off for as long as hand
    /// tracking drives it and switched back on when the controller takes over.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Hand Tracking Pivot Driver")]
    [DefaultExecutionOrder(100)]
    public class HandTrackingPivotDriver : MonoBehaviour
    {
        [Tooltip("Configuration that owns the hand input providers.")]
        [SerializeField] private Config config;

        [Tooltip("Pivot of the left hand. Its Tracked Pose Driver (on the pivot or a parent) is the transform that gets driven.")]
        [SerializeField] private Transform leftHandPivot;

        [Tooltip("Pivot of the right hand. Its Tracked Pose Driver (on the pivot or a parent) is the transform that gets driven.")]
        [SerializeField] private Transform rightHandPivot;

        [Header("Fine Tuning")]
        [Tooltip("Extra position offset for the left hand, in the tracked hand's own space (meters).")]
        [SerializeField] private Vector3 leftPositionOffset;

        [Tooltip("Extra rotation offset for the left hand, in the tracked hand's own space (degrees).")]
        [SerializeField] private Vector3 leftRotationOffset;

        [Tooltip("Extra position offset for the right hand, in the tracked hand's own space (meters).")]
        [SerializeField] private Vector3 rightPositionOffset;

        [Tooltip("Extra rotation offset for the right hand, in the tracked hand's own space (degrees).")]
        [SerializeField] private Vector3 rightRotationOffset;

        private DrivenPivot _left;
        private DrivenPivot _right;

        /// <summary>True while the left hand's pivot is being driven from hand tracking.</summary>
        public bool IsDrivingLeft => _left.Driving;

        /// <summary>True while the right hand's pivot is being driven from hand tracking.</summary>
        public bool IsDrivingRight => _right.Driving;

        /// <summary>Sets the config and pivots this driver works with.</summary>
        public void Initialize(Config configRef, Transform leftPivot, Transform rightPivot)
        {
            ReleaseAll();
            config = configRef;
            leftHandPivot = leftPivot;
            rightHandPivot = rightPivot;
            _left = DrivenPivot.For(leftHandPivot);
            _right = DrivenPivot.For(rightHandPivot);
        }

        private void Awake()
        {
            _left = DrivenPivot.For(leftHandPivot);
            _right = DrivenPivot.For(rightHandPivot);
        }

        private void OnEnable() => Application.onBeforeRender += DriveBothHands;

        private void OnDisable()
        {
            Application.onBeforeRender -= DriveBothHands;
            ReleaseAll();
        }

        private void Update() => DriveBothHands();

        private void DriveBothHands()
        {
            if (config == null) return;
            Drive(ref _left, config[HandIdentifier.Left], leftPositionOffset, leftRotationOffset);
            Drive(ref _right, config[HandIdentifier.Right], rightPositionOffset, rightRotationOffset);
        }

        private static void Drive(ref DrivenPivot pivot, IHandInputProvider provider, Vector3 positionOffset, Vector3 rotationOffset)
        {
            if (pivot.Target == null) return;

            bool handTrackingIsSource = TryGetTrackedHand(provider, out var handTracking, out bool usingHandTracking)
                                        && usingHandTracking;
            if (!handTrackingIsSource)
            {
                pivot.Release();
                return;
            }

            pivot.TakeOver();

            // No valid pose this frame: hold the last one instead of snapping to the origin.
            if (!handTracking.TryGetGripPose(out var pose)) return;

            var rotation = pose.rotation * Quaternion.Euler(rotationOffset);
            pivot.Target.SetLocalPositionAndRotation(pose.position + rotation * positionOffset, rotation);
        }

        private static bool TryGetTrackedHand(IHandInputProvider provider, out HandTrackingInputProvider handTracking, out bool usingHandTracking)
        {
            switch (provider)
            {
                case AutoSwitchHandInputProvider autoSwitch:
                    handTracking = autoSwitch.HandTracking;
                    usingHandTracking = autoSwitch.IsUsingHandTracking;
                    return handTracking != null;
                case HandTrackingInputProvider direct:
                    handTracking = direct;
                    usingHandTracking = direct.IsTracked;
                    return true;
                default:
                    handTracking = null;
                    usingHandTracking = false;
                    return false;
            }
        }

        private void ReleaseAll()
        {
            _left.Release();
            _right.Release();
        }

        private struct DrivenPivot
        {
            public Transform Target;
            public bool Driving;
            private TrackedPoseDriver _poseDriver;

            public static DrivenPivot For(Transform pivot)
            {
                if (pivot == null) return default;
                var poseDriver = pivot.GetComponentInParent<TrackedPoseDriver>(true);
                return new DrivenPivot
                {
                    _poseDriver = poseDriver,
                    Target = poseDriver != null ? poseDriver.transform : pivot
                };
            }

            public void TakeOver()
            {
                if (Driving) return;
                Driving = true;
                if (_poseDriver != null) _poseDriver.enabled = false;
            }

            // Only re-enables a pose driver this component switched off itself.
            public void Release()
            {
                if (!Driving) return;
                Driving = false;
                if (_poseDriver != null) _poseDriver.enabled = true;
            }
        }
    }
}
#endif

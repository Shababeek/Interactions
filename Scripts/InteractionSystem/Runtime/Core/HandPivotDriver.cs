using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Moves a hand pivot from the tracked hand's wrist pose while <see cref="AutoSwitchInputProvider"/>
    /// is using hand tracking. The rig's pivots are normally driven by a <see cref="TrackedPoseDriver"/>
    /// bound to <c>&lt;XRController&gt;</c> pose controls, which do not exist for a bare hand, so without
    /// this the hand would sit at its authored spot. The driver is switched off while this takes over
    /// and back on as soon as the controller is the active source again.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class HandPivotDriver : MonoBehaviour
    {
        [Tooltip("Extra rotation applied on top of the wrist pose, in degrees. Tune this if the hand model sits rotated relative to the real hand.")]
        [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

        [Tooltip("Local rotation of the LEFT pivot child while a tracked hand is the source, in degrees. The authored (controller) rotation is captured at startup and restored when the controller takes over.")]
        [SerializeField] private Vector3 leftHandPivotLocalEuler = new Vector3(0f, 0f, -90f);

        [Tooltip("Local rotation of the RIGHT pivot child while a tracked hand is the source, in degrees.")]
        [SerializeField] private Vector3 rightHandPivotLocalEuler = new Vector3(0f, 0f, 90f);

        private AutoSwitchInputProvider _provider;
        private TrackedPoseDriver _poseDriver;
        private Transform _pivot;
        private Quaternion _controllerPivotRotation = Quaternion.identity;

        /// <summary>Binds this driver to the provider of the hand it moves and to the pivot it re-orients.</summary>
        public void Initialize(AutoSwitchInputProvider provider, TrackedPoseDriver poseDriver, Transform pivot)
        {
            _provider = provider;
            _poseDriver = poseDriver;

            if (_pivot != pivot)
            {
                _pivot = pivot;
                // Controllers need the authored pivot rotation (e.g. 90 on X); remember it.
                if (_pivot != null) _controllerPivotRotation = _pivot.localRotation;
            }
        }

        private void OnDisable()
        {
            if (_poseDriver != null) _poseDriver.enabled = true;
            if (_pivot != null) _pivot.localRotation = _controllerPivotRotation;
        }

        private void LateUpdate()
        {
            if (_provider == null) return;

            bool useHand = _provider.UsingHandTracking && _provider.TrackingState != 0;

            if (_poseDriver != null && _poseDriver.enabled == useHand)
                _poseDriver.enabled = !useHand;

            // The pivot child carries the controller-grip-to-hand rotation; a tracked wrist
            // pose needs none, so swap it with the source.
            if (_pivot != null)
                _pivot.localRotation = useHand ? Quaternion.Euler(_provider.Handedness == HandIdentifier.Left ? leftHandPivotLocalEuler : rightHandPivotLocalEuler) : _controllerPivotRotation;

            if (!useHand) return;

            transform.SetLocalPositionAndRotation(
                _provider.Position,
                _provider.Rotation * Quaternion.Euler(rotationOffsetEuler));
        }
    }
}

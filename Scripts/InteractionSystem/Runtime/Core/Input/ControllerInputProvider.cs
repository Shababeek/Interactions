using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Shababeek.Utilities;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Hand input provider that reads from Unity Input System actions for VR controller input.
    /// Gets input actions from the Config file and updates finger values.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Input Providers/Controller Input Provider")]
    public class ControllerInputProvider : HandInputProviderBase
    {
        [Header("Controller Detection")]
        [Tooltip("Check for controller device connection to determine if active.")]
        [SerializeField] private bool useDeviceDetection = true;

        [SerializeField]private InputAction _thumbAction;
        [SerializeField]private InputAction _indexAction;
        [SerializeField]private InputAction _middleAction;
        [SerializeField]private InputAction _ringAction;
        [SerializeField]private InputAction _pinkyAction;

        private bool _controllerConnected = false;
        private XRController _controllerDevice;
        
        // Cached position/rotation for efficiency
        [SerializeField]private Vector3 _position = Vector3.zero;
        [SerializeField]private Quaternion _rotation = Quaternion.identity;
        [SerializeField]private uint _trackingState = 0;
        
        // Retry controller detection if not found
        private float _lastControllerCheckTime = 0f;
        private const float CONTROLLER_CHECK_INTERVAL = 0.5f; // Check every 0.5 seconds


        private void Awake()
        {
            if (priority < 20)
                priority = 20;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Enable actions if they exist
            _thumbAction?.Enable();
            _indexAction?.Enable();
            _middleAction?.Enable();
            _ringAction?.Enable();
            _pinkyAction?.Enable();

            // Subscribe to device changes
            if (useDeviceDetection)
            {
                InputSystem.onDeviceChange += OnDeviceChange;
                _lastControllerCheckTime = 0f; // Check immediately
                CheckForControllers();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Unsubscribe from device changes
            if (useDeviceDetection)
            {
                InputSystem.onDeviceChange -= OnDeviceChange;
            }

            // Disable actions
            _thumbAction?.Disable();
            _indexAction?.Disable();
            _middleAction?.Disable();
            _ringAction?.Disable();
            _pinkyAction?.Disable();
        }

        protected override void UpdateFingerValues()
        {
            // Keep checking for controllers if not found yet
            if (useDeviceDetection && _controllerDevice == null)
            {
                if (Time.time - _lastControllerCheckTime >= CONTROLLER_CHECK_INTERVAL)
                {
                    CheckForControllers();
                    _lastControllerCheckTime = Time.time;
                }
            }
            
            // Read values from Input System actions
            this[FingerName.Thumb] = _thumbAction?.ReadValue<float>() ?? 0f;
            this[FingerName.Index] = _indexAction?.ReadValue<float>() ?? 0f;
            this[FingerName.Middle] = _middleAction?.ReadValue<float>() ?? 0f;
            this[FingerName.Ring] = _ringAction?.ReadValue<float>() ?? 0f;
            this[FingerName.Pinky] = _pinkyAction?.ReadValue<float>() ?? 0f;
            
            // Update position, rotation, and tracking state
            if (_controllerDevice != null)
            {
                try
                {
                    _position = _controllerDevice.devicePosition.ReadValue();
                    _rotation = _controllerDevice.deviceRotation.ReadValue();
                    _trackingState = (uint)_controllerDevice.trackingState.ReadValue();
                }
                catch
                {
                    // Device might have been disconnected
                    _controllerDevice = null;
                    _controllerConnected = false;
                }
            }
            else
            {
                _position = Vector3.zero;
                _rotation = Quaternion.identity;
                _trackingState = 0;
            }
        }

        /// <summary>
        /// Handles Input System device changes (connection/disconnection).
        /// </summary>
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!(device is XRController))
                return;

            CheckForControllers();
        }

        /// <summary>
        /// Checks if any XR controllers for this hand are currently connected.
        /// Uses Input System device queries for more reliable detection.
        /// </summary>
        private void CheckForControllers()
        {
            _controllerDevice = null;
            bool foundController = false;

            // Try to find controller using Input System queries
            string handTag = handedness == HandIdentifier.Left ? "LeftHand" : "RightHand";
            string controllerPath = $"<XRController>{{{handTag}}}";
            
            try
            {
                // Try to find controller by path
                var controls = InputSystem.FindControls(controllerPath);
                if (controls.Count > 0)
                {
                    var device = controls[0].device;
                    if (device is XRController controller)
                    {
                        _controllerDevice = controller;
                        foundController = true;
                    }
                }
            }
            catch
            {
                // Path-based search failed, fall back to device enumeration
            }

            // Fallback: Check all connected XR controllers by name and characteristics
            if (!foundController)
            {
                foreach (var device in InputSystem.devices)
                {
                    if (device is XRController controller)
                    {
                        string deviceName = controller.name.ToLower();
                        string displayName = controller.displayName?.ToLower() ?? "";
                        
                        // Check multiple ways to identify handedness
                        bool isLeftController = deviceName.Contains("left") || 
                                               displayName.Contains("left") ||
                                               (handedness == HandIdentifier.Left && !deviceName.Contains("right") && !displayName.Contains("right"));
                        bool isRightController = deviceName.Contains("right") || 
                                                displayName.Contains("right") ||
                                                (handedness == HandIdentifier.Right && !deviceName.Contains("left") && !displayName.Contains("left"));
                        
                        // Also check by device characteristics - if we have exactly 2 controllers, assign by order
                        var allControllers = InputSystem.devices.OfType<XRController>().ToList();
                        if (allControllers.Count == 2)
                        {
                            // First controller is typically left, second is right
                            int index = allControllers.IndexOf(controller);
                            isLeftController = index == 0 && handedness == HandIdentifier.Left;
                            isRightController = index == 1 && handedness == HandIdentifier.Right;
                        }

                        if ((handedness == HandIdentifier.Left && isLeftController) ||
                            (handedness == HandIdentifier.Right && isRightController))
                        {
                            _controllerDevice = controller;
                            foundController = true;
                            break;
                        }
                    }
                }
            }

            _controllerConnected = foundController;
            
            // Reset tracking data if controller not found
            if (!foundController)
            {
                _position = Vector3.zero;
                _rotation = Quaternion.identity;
                _trackingState = 0;
            }
        }

        /// <summary>
        /// Initializes the provider with InputActions from the Config file.
        /// </summary>
        public void Initialize(Config.HandInputActions actions)
        {
            // Get actions from config
            _thumbAction = actions.ThumbAction;
            _indexAction = actions.IndexAction;
            _middleAction = actions.MiddleAction;
            _ringAction = actions.RingAction;
            _pinkyAction = actions.PinkyAction;

            // Enable actions if component is already enabled
            if (enabled)
            {
                _thumbAction?.Enable();
                _indexAction?.Enable();
                _middleAction?.Enable();
                _ringAction?.Enable();
                _pinkyAction?.Enable();
            }
        }

        /// <summary>
        /// Current position of the controller in world space.
        /// </summary>
        public override Vector3 Position => _position;

        /// <summary>
        /// Current rotation of the controller in world space.
        /// </summary>
        public override Quaternion Rotation => _rotation;

        /// <summary>
        /// Current tracking state of the controller (flags: Position = 1, Rotation = 2, Both = 3).
        /// </summary>
        public override uint TrackingState => _trackingState;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (priority < 20)
                priority = 20;
        }
#endif
    }
}


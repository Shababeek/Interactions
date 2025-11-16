using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Hand input provider that reads from Unity Input System actions for VR controller input.
    /// Detects controller connection/disconnection and provides event-based activation.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Input Providers/Controller Input Provider")]
    public class ControllerInputProvider : HandInputProviderBase
    {
        [Header("Input Actions")]
        [SerializeField] private InputAction thumbAction;
        [SerializeField] private InputAction indexAction;

        [SerializeField] private InputAction middleAction;

        [SerializeField] private InputAction ringAction;

        [SerializeField] private InputAction pinkyAction;

        [Header("Controller Detection")]
        [Tooltip("Check for controller device connection to determine if active.")]
        [SerializeField] private bool useDeviceDetection = true;

        private bool _controllerConnected = false;

        /// <summary>
        /// Checks if controller is connected and providing input.
        /// </summary>
        public override bool IsActive
        {
            get
            {
                if (!useDeviceDetection)
                {
                    // Fallback: check if any button is pressed
                    return (indexAction?.ReadValue<float>() ?? 0f) > 0.01f ||
                           (middleAction?.ReadValue<float>() ?? 0f) > 0.01f ||
                           (thumbAction?.ReadValue<float>() ?? 0f) > 0.01f;
                }

                return _controllerConnected;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Enable actions
            thumbAction?.Enable();
            indexAction?.Enable();
            middleAction?.Enable();
            ringAction?.Enable();
            pinkyAction?.Enable();

            // Subscribe to device changes
            if (useDeviceDetection)
            {
                InputSystem.onDeviceChange += OnDeviceChange;
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
            thumbAction?.Disable();
            indexAction?.Disable();
            middleAction?.Disable();
            ringAction?.Disable();
            pinkyAction?.Disable();
        }

        protected override void UpdateFingerValues()
        {
            if (!IsActive)
            {
                // Clear values when inactive
                SetFingerValues(0f, 0f, 0f, 0f, 0f);
                return;
            }

            // Read values from Input System actions
            this[FingerName.Thumb] = thumbAction?.ReadValue<float>() ?? 0f;
            this[FingerName.Index] = indexAction?.ReadValue<float>() ?? 0f;
            this[FingerName.Middle] = middleAction?.ReadValue<float>() ?? 0f;
            this[FingerName.Ring] = ringAction?.ReadValue<float>() ?? 0f;
            this[FingerName.Pinky] = pinkyAction?.ReadValue<float>() ?? 0f;
        }

        /// <summary>
        /// Handles Input System device changes (connection/disconnection).
        /// </summary>
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!(device is XRController))
                return;

            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Enabled:
                    CheckForControllers();
                    break;

                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Disabled:
                    CheckForControllers();
                    break;
            }
        }

        /// <summary>
        /// Checks if any XR controllers for this hand are currently connected.
        /// </summary>
        private void CheckForControllers()
        {
            bool foundController = false;

            // Check all connected XR controllers
            foreach (var device in InputSystem.devices)
            {
                if (device is XRController controller)
                {
                    // Check if this controller matches our handedness
                    bool isLeftController = controller.name.ToLower().Contains("left");
                    bool isRightController = controller.name.ToLower().Contains("right");

                    if ((handedness == HandIdentifier.Left && isLeftController) ||
                        (handedness == HandIdentifier.Right && isRightController))
                    {
                        foundController = true;
                        break;
                    }
                }
            }

            _controllerConnected = foundController;
        }


        /// <summary>
        /// Initializes the provider with InputActionReference instances (for use in Inspector).
        /// </summary>
        public void Initialize(Config.HandInputActions actions)
        {
            thumbAction = actions.ThumbAction;
            indexAction = actions.IndexAction;
            middleAction = actions.MiddleAction;
            ringAction = actions.RingAction;
            pinkyAction = actions.PinkyAction;

            if (enabled)
            {
                OnDisable();
                OnEnable();
            }
        }
    }
}
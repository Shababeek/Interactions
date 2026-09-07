using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Input manager implementation that uses keyboard input for hand and finger simulation in the interaction system.
    /// Left Ctrl/Alt drive the left hand, Right Ctrl/Alt drive the right hand.
    /// </summary>
    public class KeyboardBasedInput : InputManagerBase
    {
        private void Update()
        {
            ReadKeys(out var leftTrigger, out var leftGrip, out var rightTrigger, out var rightGrip);

            LeftHand[1] = leftTrigger ? 1 : 0;
            LeftHand[2] = LeftHand[3] = LeftHand[4] = leftGrip ? 1 : 0;
            LeftHand.TriggerObserver.ButtonState = leftTrigger;
            LeftHand.GripObserver.ButtonState = leftGrip;

            RightHand[1] = rightTrigger ? 1 : 0;
            RightHand[2] = RightHand[3] = RightHand[4] = rightGrip ? 1 : 0;
            RightHand.TriggerObserver.ButtonState = rightTrigger;
            RightHand.GripObserver.ButtonState = rightGrip;
        }

        private static void ReadKeys(out bool leftTrigger, out bool leftGrip, out bool rightTrigger, out bool rightGrip)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                leftTrigger = leftGrip = rightTrigger = rightGrip = false;
                return;
            }
            leftTrigger = keyboard.leftCtrlKey.isPressed;
            leftGrip = keyboard.leftAltKey.isPressed;
            rightTrigger = keyboard.rightCtrlKey.isPressed;
            rightGrip = keyboard.rightAltKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            leftTrigger = Input.GetKey(KeyCode.LeftControl);
            leftGrip = Input.GetKey(KeyCode.LeftAlt);
            rightTrigger = Input.GetKey(KeyCode.RightControl);
            rightGrip = Input.GetKey(KeyCode.RightAlt);
#else
            leftTrigger = leftGrip = rightTrigger = rightGrip = false;
#endif
        }
    }
}

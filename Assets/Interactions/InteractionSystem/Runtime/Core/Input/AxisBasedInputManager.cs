using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    internal class AxisBasedInputManager : InputManagerBase
    {
        private const string LEFT_TRIGGER_AXIS = "XRI_Left_Trigger";
        private const string LEFT_TRIGGER_BUTTON = "XRI_Left_TriggerButton";
        private const string LEFT_GRIP_AXIS = "XRI_Left_Grip";
        private const string LEFT_GRIP_BUTTON = "XRI_Left_GripButton";
        private const string XRI_LEFT_PRIMARY_TOUCH = "XRI_Left_PrimaryButton";
        private const string XRI_LEFT_SECONDARY_TOUCH = "XRI_Left_SecondaryButton";

        private const string RIGHT_TRIGGER_AXIS = "XRI_Right_Trigger";
        private const string RIGHT_TRIGGER_BUTTON = "XRI_Right_TriggerButton";
        private const string RIGHT_GRIP_AXIS = "XRI_Right_Grip";
        private const string RIGHT_GRIP_BUTTON = "XRI_Right_GripButton";
        private const string XRI_RIGHT_PRIMARY_TOUCH = "XRI_Right_PrimaryButton";
        private const string XRI_RIGHT_SECONDARY_TOUCH = "XRI_Right_SecondaryButton";


        private void Update()
        {
            var leftThumbPressed =Input.GetButton(XRI_LEFT_PRIMARY_TOUCH) || Input.GetButton(XRI_LEFT_SECONDARY_TOUCH);

            leftHand[0] = leftThumbPressed ? 1 : 0;
            leftHand[1] = Input.GetAxisRaw(LEFT_TRIGGER_AXIS);
            leftHand[2] = leftHand[3] = leftHand[4] = Input.GetAxisRaw(LEFT_GRIP_AXIS);
            leftHand.triggerObserver.ButtonState = Input.GetButton(LEFT_TRIGGER_BUTTON);
            leftHand.gripObserver.ButtonState = Input.GetButton(LEFT_GRIP_BUTTON);
            var rightThumbPressed = Input.GetButton(XRI_RIGHT_PRIMARY_TOUCH) || Input.GetButton(XRI_RIGHT_SECONDARY_TOUCH);
            rightHand[0] = rightThumbPressed ? 1 : 0;
            rightHand[1] = Input.GetAxisRaw(RIGHT_TRIGGER_AXIS);
            rightHand[2] = rightHand[3] = rightHand[4] = Input.GetAxisRaw(RIGHT_GRIP_AXIS);
            rightHand.triggerObserver.ButtonState = Input.GetButton(RIGHT_TRIGGER_BUTTON);
            rightHand.gripObserver.ButtonState = Input.GetButton(RIGHT_GRIP_BUTTON);
        }
    }
}
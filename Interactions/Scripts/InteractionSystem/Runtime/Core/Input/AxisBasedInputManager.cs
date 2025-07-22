using System;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// This class is used to handle the input for the axis based input manager.
    /// </summary>
    [Obsolete("This class is obsolete, use NewInputSystemBasedInputManager instead")]
    internal class AxisBasedInputManager : InputManagerBase
    {
        private string LeftTriggerAxis = "XRI_Left_Trigger";
        private string LeftGripAxis = "XRI_Left_Grip";
        private string LeftPrimaryButton = "XRI_Left_PrimaryButton";
        private string LeftSecondryButton = "XRI_Left_SecondaryButton";
        private string LeftGripDebugKey = "XRI_Left_Grip_DebugKey";
        private string LeftTriggerDebugKey = "XRI_Left_Index_DebugKey";
        private string leftThumbDebugKey= "XRI_Left_Primary_DebugKey";

        private string RightTriggerAxis = "XRI_Right_Trigger";
        private string RightGripAxis = "XRI_Right_Grip";
        private string RightPrimaryButton = "XRI_Right_PrimaryButton";
        private string RightSecondryButton = "XRI_Right_SecondaryButton";
        private string RightGripDebugKey = "XRI_Right_Grip_DebugKey";
        private string RightTriggerDebugKey = "XRI_Right_Index_DebugKey";
        private string rightThumbDebugKey= "XRI_Right_Primary_DebugKey";

        void Initialize(string leftTriggerName, string leftGripName, string leftPrimaryButton,
            string leftSecondryButton, string leftGripDebugKey, string leftTriggerDebugKey, string rightTriggerName,
            string rightGripName, string rightPrimaryButton, string rightSecondryButton, string rightGripDebugKey,
            string rightTriggerDebugKey)
        {
            LeftTriggerAxis = leftTriggerName;
            LeftGripAxis = leftGripName;
            LeftPrimaryButton = leftPrimaryButton;
            LeftSecondryButton = leftSecondryButton;
            LeftGripDebugKey = leftGripDebugKey;
            LeftTriggerDebugKey = leftTriggerDebugKey;
            RightTriggerAxis = rightTriggerName;
            RightGripAxis = rightGripName;
            RightPrimaryButton = rightPrimaryButton;
            RightSecondryButton = rightSecondryButton;
            RightGripDebugKey = rightGripDebugKey;
            RightTriggerDebugKey = rightTriggerDebugKey;
            Debug.Log("AxisBasedInputManager initialized");
        }

        private void Update()
        {
            HandleLeftHand();
            HandRightHand();

            void HandleLeftHand()
            {
                var (thumb, triggerAxe, gripAxe) = GetAxes(
                    LeftPrimaryButton, LeftSecondryButton, LeftTriggerAxis, LeftGripAxis,
                    LeftGripDebugKey, LeftTriggerDebugKey,leftThumbDebugKey);
                LeftHand[0] = thumb ? 1 : 0;
                LeftHand[1] = triggerAxe;
                LeftHand[2] = LeftHand[3] = LeftHand[4] = gripAxe;
                LeftHand.TriggerObserver.ButtonState = triggerAxe > .5f;
                LeftHand.GripObserver.ButtonState = gripAxe > .5;
            }

            void HandRightHand()    
            {
                var (thumb, triggerAxe, gripAxe) = GetAxes(
                    RightPrimaryButton, RightSecondryButton, RightTriggerAxis, RightGripAxis,
                    RightGripDebugKey, RightTriggerDebugKey,rightThumbDebugKey);

                RightHand[0] = thumb ? 1 : 0;
                RightHand[1] = triggerAxe;
                RightHand[2] = RightHand[3] = RightHand[4] = gripAxe;
                RightHand.TriggerObserver.ButtonState = triggerAxe > .5f;
                RightHand.GripObserver.ButtonState = gripAxe > .5;
            }
        }

        private (bool thumb, float triggerAxe, float gripAxe) GetAxes(string primaryButton, string secondryButton,
            string triggerAxeName, string gripAxeName, string gripDebugKey, string triggerDebugKey,string thumbDebugKey)
        {
            var thumb = Input.GetButton(primaryButton) || Input.GetButton(secondryButton)|| Input.GetButton(thumbDebugKey);
            var triggerAxe = Input.GetAxisRaw(triggerAxeName);
            var gripAxe = Input.GetAxisRaw(gripAxeName);
            if (triggerAxe < .001 && triggerAxe > -.001)
            {
                triggerAxe = Input.GetAxis(triggerDebugKey);
            }

            if (gripAxe < .001 && gripAxe > -.001)
            {
                gripAxe = Input.GetAxis(gripDebugKey);
            }

            return (thumb, triggerAxe, gripAxe);
        }
    }
}
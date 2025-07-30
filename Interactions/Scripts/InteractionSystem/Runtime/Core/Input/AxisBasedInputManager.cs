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
        private Config config;
        private OldInputManagerSettings inputSettings;

        public void Initialize(Config config)
        {
            this.config = config;
            this.inputSettings = config.OldInputSettings;
        }

        private void Update()
        {
            HandleLeftHand();
            HandRightHand();

            void HandleLeftHand()
            {
                var (thumb, triggerAxe, gripAxe) = GetAxes(
                    inputSettings.leftPrimaryButton, inputSettings.leftSecondaryButton, inputSettings.leftTriggerAxis, inputSettings.leftGripAxis,
                    inputSettings.leftGripDebugKey, inputSettings.leftTriggerDebugKey, inputSettings.leftThumbDebugKey);
                LeftHand[0] = thumb ? 1 : 0;
                LeftHand[1] = triggerAxe;
                LeftHand[2] = LeftHand[3] = LeftHand[4] = gripAxe;
                LeftHand.TriggerObserver.ButtonState = triggerAxe > .5f;
                LeftHand.GripObserver.ButtonState = gripAxe > .5;
            }

            void HandRightHand()    
            {
                var (thumb, triggerAxe, gripAxe) = GetAxes(
                    inputSettings.rightPrimaryButton, inputSettings.rightSecondaryButton, inputSettings.rightTriggerAxis, inputSettings.rightGripAxis,
                    inputSettings.rightGripDebugKey, inputSettings.rightTriggerDebugKey, inputSettings.rightThumbDebugKey);

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
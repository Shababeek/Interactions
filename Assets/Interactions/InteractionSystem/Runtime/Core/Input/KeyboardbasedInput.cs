using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{

    public class KeyboardBasedInput : InputManagerBase
    {
        private void Update()
        {
            leftHand[1] = Input.GetKey(KeyCode.LeftControl) ? 1 : 0;
            leftHand[2] = leftHand[3] = leftHand[4] = Input.GetKey(KeyCode.LeftAlt) ? 1 : 0;

            leftHand.triggerObserver.ButtonState = Input.GetKey(KeyCode.LeftControl);
            leftHand.gripObserver.ButtonState = Input.GetKey(KeyCode.LeftAlt);

            rightHand[1] = Input.GetKey(KeyCode.RightControl) ? 1 : 0;
            rightHand[2] = rightHand[3] = rightHand[4] = Input.GetKey(KeyCode.RightAlt) ? 1 : 0;

            rightHand.triggerObserver.ButtonState = Input.GetKey(KeyCode.RightControl);
            rightHand.gripObserver.ButtonState = Input.GetKey(KeyCode.RightAlt);
        }
    }}
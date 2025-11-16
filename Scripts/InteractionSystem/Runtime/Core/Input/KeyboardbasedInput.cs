using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Input manager implementation that uses keyboard input for hand and finger simulation in the interaction system.
    /// </summary>
    public class KeyboardBasedInput : InputManagerBase
    {
        private void Update()
        {
            LeftHand[1] = Input.GetKey(KeyCode.LeftControl) ? 1 : 0;
            LeftHand[2] = LeftHand[3] = LeftHand[4] = Input.GetKey(KeyCode.LeftAlt) ? 1 : 0;

            LeftHand.TriggerObserver.ButtonState = Input.GetKey(KeyCode.LeftControl);
            LeftHand.GripObserver.ButtonState = Input.GetKey(KeyCode.LeftAlt);

            RightHand[1] = Input.GetKey(KeyCode.RightControl) ? 1 : 0;
            RightHand[2] = RightHand[3] = RightHand[4] = Input.GetKey(KeyCode.RightAlt) ? 1 : 0;

            RightHand.TriggerObserver.ButtonState = Input.GetKey(KeyCode.RightControl);
            RightHand.GripObserver.ButtonState = Input.GetKey(KeyCode.RightAlt);
        }
    }}
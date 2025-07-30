using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Input manager using Unity's new Input System, mapping each VR button to a finger (0=thumb, 4=pinky) for both hands.
    /// </summary>
    internal class NewInputSystemBasedInputManager : InputManagerBase
    {
        [SerializeField]private Config.HandInputActions leftHandActions;
        [SerializeField]private Config.HandInputActions rightHandActions;

        public void Initialize(Config.HandInputActions leftHandActions, Config.HandInputActions rightHandActions)
        {
            this.leftHandActions = leftHandActions;
            this.rightHandActions = rightHandActions;
            EnableActions(leftHandActions, rightHandActions);
        }

        private static void EnableActions(Config.HandInputActions leftHandActions, Config.HandInputActions rightHandActions)
        {
            leftHandActions.ThumbAction.Enable();
            leftHandActions.IndexAction.Enable();
            leftHandActions.MiddleAction.Enable();
            leftHandActions.RingAction.Enable();
            leftHandActions.PinkyAction.Enable();
            rightHandActions.ThumbAction.Enable();
            rightHandActions.IndexAction.Enable();
            rightHandActions.MiddleAction.Enable();
            rightHandActions.RingAction.Enable();
            rightHandActions.PinkyAction.Enable();
        }

        private void OnDestroy()
        {
            DisableActions();
        }

        private void DisableActions()
        {
            leftHandActions.ThumbAction.Disable();
            leftHandActions.IndexAction.Disable();
            leftHandActions.MiddleAction.Disable();
            leftHandActions.RingAction.Disable();
            leftHandActions.PinkyAction.Disable();
            rightHandActions.ThumbAction.Disable();
            rightHandActions.IndexAction.Disable();
            rightHandActions.MiddleAction.Disable();
            rightHandActions.RingAction.Disable();
            rightHandActions.PinkyAction.Disable();
            
        }

        private void Update()
        {
            LeftHand[0] = leftHandActions.ThumbAction?.ReadValue<float>() ?? 0f;
            LeftHand[1] = leftHandActions.IndexAction?.ReadValue<float>() ?? 0f;
            LeftHand[2] = leftHandActions.MiddleAction?.ReadValue<float>() ?? 0f;
            LeftHand[3] = leftHandActions.RingAction?.ReadValue<float>() ?? 0f;
            LeftHand[4] = leftHandActions.PinkyAction?.ReadValue<float>() ?? 0f;

            RightHand[0] = rightHandActions.ThumbAction?.ReadValue<float>() ?? 0f;
            RightHand[1] = rightHandActions.IndexAction?.ReadValue<float>() ?? 0f;
            RightHand[2] = rightHandActions.MiddleAction?.ReadValue<float>() ?? 0f;
            RightHand[3] = rightHandActions.RingAction?.ReadValue<float>() ?? 0f;
            RightHand[4] = rightHandActions.PinkyAction?.ReadValue<float>() ?? 0f;

            //  treat value > 0.2 as button pressed for observers, check if assigned
            LeftHand.TriggerObserver.ButtonState = (leftHandActions.IndexAction != null) && (leftHandActions.IndexAction.ReadValue<float>() > 0.2f);
            LeftHand.GripObserver.ButtonState = (leftHandActions.MiddleAction != null) && (leftHandActions.MiddleAction.ReadValue<float>() > 0.2f);
            RightHand.TriggerObserver.ButtonState = (rightHandActions.IndexAction != null) && (rightHandActions.IndexAction.ReadValue<float>() > 0.2f);
            RightHand.GripObserver.ButtonState = (rightHandActions.MiddleAction != null) && (rightHandActions.MiddleAction.ReadValue<float>() > 0.2f);
        }
    }
} 
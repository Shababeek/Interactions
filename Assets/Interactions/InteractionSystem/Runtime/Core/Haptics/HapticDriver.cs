using UnityEngine;
using UnityEngine.XR;

namespace Kandooz.InteractionSystem.Core.Haptics
{
    public class HapticDriver : MonoBehaviour
    {
        private float t = 0;
        private void Update()
        {
            t += Time.deltaTime;
            if (t > 3)
            {
                t = 0;
                var inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                inputDevice.SendHapticImpulse(0, 1, 1);
                //OpenXRInput.SendHapticImpulse();
                
            }
        }
    }
}
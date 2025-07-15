using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;
using UnityEngine.XR;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// Drives the haptic of the controller currently interacting with the interactable attached to the same object
    /// </summary>
    [RequireComponent(typeof(InteractableBase))]
    public class HapticDriver : MonoBehaviour
    {
        [SerializeField] private bool hapticsOnHover = true;
        [HideInInspector] [SerializeField] private float hoverDuration = 1, hoverAmplitude = 1;
        [SerializeField] private bool hapticsOnSelected = false;
        [HideInInspector] [SerializeField] private float selectedDuration = 1, selectedAmplitude = 1;
        [SerializeField] private bool hapticsOnActivated = false;
        [HideInInspector] [SerializeField] private float activatedDuration = 1, activatedAmplitude = 1;
        
        private void Awake()
        {
            var interactable = GetComponent<InteractableBase>();
            interactable.OnHoverStarted
                .Where(_ => hapticsOnHover)
                .Select(interactor => (interactor.HandIdentifier, hoverAmplitude, hoverDuration))
                .Do(ExecuteHaptic).Subscribe().AddTo(this);
            interactable.OnSelected
                .Where(_ => hapticsOnSelected)
                .Select(interactor => (interactor.HandIdentifier, selectedAmplitude, selectedDuration))
                .Do(ExecuteHaptic).Subscribe().AddTo(this);
            interactable.OnActivated
                .Where(_ => hapticsOnActivated)
                .Select(interactor => (interactor.HandIdentifier, activatedAmplitude, activatedDuration))
                .Do(ExecuteHaptic).Subscribe().AddTo(this);
        }

        private void ExecuteHaptic((HandIdentifier handIdentifier, float amplitude, float duration) data)
        {
            var hand = data.handIdentifier == HandIdentifier.Left ? XRNode.LeftHand : XRNode.RightHand;
            var inputDevice = InputDevices.GetDeviceAtXRNode(hand);
            inputDevice.SendHapticImpulse(0, data.amplitude, data.duration);
        }
    }
}
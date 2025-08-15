using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;
using UnityEngine.XR;

namespace Shababeek.Interactions.Feedback
{
    /// <summary>
    /// [DEPRECATED] Provides haptic feedback to VR controllers during interaction events.
    /// Sends haptic impulses to the controller that is interacting with this object.
    /// </summary>
    /// <remarks>
    /// <b>DEPRECATED:</b> This component is deprecated. Please use the FeedbackSystem with HapticFeedback instead.
    /// The FeedbackSystem provides more comprehensive feedback options and better integration with the interaction system.
    /// 
    /// To migrate:
    /// 1. Remove this HapticDriver component
    /// 2. Add a FeedbackSystem component to your interactable
    /// 3. Add a HapticFeedback to the FeedbackSystem
    /// 4. Configure the HapticFeedback settings for your desired haptic behavior
    /// </remarks>
    [RequireComponent(typeof(InteractableBase))]
    [System.Obsolete("This component is deprecated. Use FeedbackSystem with HapticFeedback instead for better integration and more features.")]
    public class HapticDriver : MonoBehaviour
    {
        [Tooltip("Whether to provide haptic feedback when hovering over the interactable.")]
        [SerializeField] private bool hapticsOnHover = true;
        
        [Tooltip("Duration of haptic feedback for hover events (in seconds).")]
        [HideInInspector] [SerializeField] private float hoverDuration = 1;
        
        [Tooltip("Amplitude of haptic feedback for hover events (0-1).")]
        [HideInInspector] [SerializeField] private float hoverAmplitude = 1;
        
        [Tooltip("Whether to provide haptic feedback when selecting the interactable.")]
        [SerializeField] private bool hapticsOnSelected = false;
        
        [Tooltip("Duration of haptic feedback for selection events (in seconds).")]
        [HideInInspector] [SerializeField] private float selectedDuration = 1;
        
        [Tooltip("Amplitude of haptic feedback for selection events (0-1).")]
        [HideInInspector] [SerializeField] private float selectedAmplitude = 1;
        
        [Tooltip("Whether to provide haptic feedback when activating the interactable.")]
        [SerializeField] private bool hapticsOnActivated = false;
        
        [Tooltip("Duration of haptic feedback for activation events (in seconds).")]
        [HideInInspector] [SerializeField] private float activatedDuration = 1;
        
        [Tooltip("Amplitude of haptic feedback for activation events (0-1).")]
        [HideInInspector] [SerializeField] private float activatedAmplitude = 1;
        
        private void Awake()
        {
            // Deprecation warning
            Debug.LogWarning($"[DEPRECATED] HapticDriver on {gameObject.name} is deprecated. " +
                           "Please migrate to FeedbackSystem with HapticFeedback for better integration and more features. " +
                           "See the component documentation for migration steps.");
            
            var interactable = GetComponent<InteractableBase>();
            interactable.OnHoverStarted
                .Where(_ => hapticsOnHover)
                .Select(interactor => (interactor.HandIdentifier, hoverAmplitude, hoverDuration))
                .Do(ExecuteHaptic).Subscribe().AddTo(this);
            interactable.OnSelected
                .Where(_ => hapticsOnSelected)
                .Select(interactor => (interactor.HandIdentifier, selectedAmplitude, selectedDuration))
                .Do(ExecuteHaptic).Subscribe().AddTo(this);
            interactable.OnUseStarted
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
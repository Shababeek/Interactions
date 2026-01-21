using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Completes a step when a specific XR controller button is pressed.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/ControllerButtonAction")]
    public class ControllerButtonAction : AbstractSequenceAction
    {
        [Tooltip("The configuration containing controller input references.")]
        [SerializeField] private Config config;

        [Tooltip("Which hand's controller to monitor.")]
        [SerializeField] private HandIdentifier hand;

        [Tooltip("Which button press triggers step completion.")]
        [SerializeField] private XRButton button;

        private void Awake()
        {
            if (config == null)
            {
                var cameraRig = FindAnyObjectByType<CameraRig>();
                if (cameraRig != null)
                {
                    config = cameraRig.Config;
                }
            }
        }

        private void Subscribe()
        {
            if (config == null) return;

            var handConfig = config[hand];
            if (handConfig == null) return;

            switch (button)
            {
                case XRButton.Trigger:
                    handConfig.TriggerObservable
                        .Where(state => state == VRButtonState.Down)
                        .Do(_ => CompleteStep())
                        .Subscribe()
                        .AddTo(StepDisposable);
                    break;

                case XRButton.Grip:
                    handConfig.GripObservable
                        .Where(state => state == VRButtonState.Down)
                        .Do(_ => CompleteStep())
                        .Subscribe()
                        .AddTo(StepDisposable);
                    break;
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                Subscribe();
            }
        }
    }
}
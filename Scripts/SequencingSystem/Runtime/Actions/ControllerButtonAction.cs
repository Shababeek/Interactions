using Shababeek.Interactions.Core;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    [AddComponentMenu(menuName : "Shababeek/Sequencing/Actions/ControllerButtonAction")]
    public class ControllerButtonAction : AbstractSequenceAction
    {
        [SerializeField] private Config config; 
        [SerializeField] private HandIdentifier hand;
        [SerializeField] private XRButton button;
        private StepEventListener listener;

        private void Awake()
        {
            if (config == null)
            {
                config = FindAnyObjectByType<CameraRig>().Config;
            }
            listener = GetComponent<StepEventListener>();
            config[hand].TriggerObservable
                .Where(_ => Started)
                .Where(_ => button == XRButton.Trigger)
                .Where(state => state == VRButtonState.Down)
                .Do(_=>listener.OnActionCompleted())
                .Subscribe().AddTo(this);
            config[hand].GripObservable
                .Where(_ => Started)
                .Where(_ => button == XRButton.Grip)
                .Where(state => state == VRButtonState.Down)
                .Do(_=>listener.OnActionCompleted())
                .Subscribe().AddTo(this);
        }


        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            
        }
    }
}
using Shababeek.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    [AddComponentMenu(menuName : "Shababeek/Sequencing/Actions/TriggerAction")]
    public class TriggerAction : AbstractSequenceAction
    {
        [SerializeField]private string objectTag;
        [SerializeField] private UnityEvent onTRiggerEnter;
        bool active = false;



        private void OnTriggerEnter(Collider other)
        {
            if(!active)return;
            if (string.IsNullOrEmpty(objectTag) || other.attachedRigidbody. CompareTag(objectTag))
            {
                onTRiggerEnter.Invoke();
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            active = status == SequenceStatus.Started;
        }
    }
}
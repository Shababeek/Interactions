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
        [SerializeField] private UnityEvent onTriggerEnter;
        private bool _active = false;



        private void OnTriggerEnter(Collider other)
        {
            if (!_active) return;
            if (string.IsNullOrEmpty(objectTag) || other.attachedRigidbody.CompareTag(objectTag))
            {
                _active = false;
                onTriggerEnter.Invoke();
                CompleteStep();
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            _active = status == SequenceStatus.Started;
        }
    }
}
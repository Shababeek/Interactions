using System;
using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    [AddComponentMenu(menuName : "Shababeek/Sequencing/Actions/InsertionAction")]
    public class InsertionAction : AbstractSequenceAction
    {
        [SerializeField] private InteractableBase interactable;
        private GameObject interactableObject;
        private bool insideTrigger = false;
        private void Awake()
        {
            interactableObject = interactable.gameObject;
            interactable.OnDeselected
                .Where(_ => Started && insideTrigger)
                .Do(OnSelectionEnded)
                .Subscribe()
                .AddTo(this);
        }

        private void OnSelectionEnded(InteractorBase interactor)
        {
            interactableObject.transform.position = transform.position;
            interactableObject.transform.rotation = transform.rotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == interactableObject)
            {
                insideTrigger = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == interactableObject)
            {
                insideTrigger = false;
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
        }
    }
}
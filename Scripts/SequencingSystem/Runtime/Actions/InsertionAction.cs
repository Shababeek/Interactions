using System;
using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Snaps an interactable object to a target position when released inside a trigger.
    /// </summary>
    [AddComponentMenu(menuName : "Shababeek/Sequencing/Actions/InsertionAction")]
    public class InsertionAction : AbstractSequenceAction
    {
        [Tooltip("The interactable object to snap into position.")]
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
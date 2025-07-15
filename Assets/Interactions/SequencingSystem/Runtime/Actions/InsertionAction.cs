using System;
using Kandooz.InteractionSystem.Interactions;
using UniRx;
using UnityEngine;

namespace Kandooz.Kuest
{
    [RequireComponent(typeof(StepEvenListener))]
    [AddComponentMenu("Kandooz/SequenceSystem/Actions/Insertion Action")]

    public class InsertionAction : MonoBehaviour
    {
        [SerializeField] private InteractableBase interactable;
        private GameObject interactableObject;
        private bool insideTrigger = false;
        private StepEvenListener listner;
        private void Awake()
        {
            listner = GetComponent<StepEvenListener>();
            interactableObject = interactable.gameObject;
            interactable.OnDeselected
                .Where(_ => listner.Current && insideTrigger)
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
    }
}
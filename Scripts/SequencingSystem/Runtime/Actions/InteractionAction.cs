using System;
using Shababeek.Interactions;
using UnityEngine;
using Shababeek.Interactions.Core;
using UniRx;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Types of interactions that can trigger step completion.
    /// </summary>
    public enum InteractionType
    {
        Selected = 0,
        Deselected = 1,
        Used = 2,
        StartedHover = 3,
        EndedHover = 4,
    }

    /// <summary>
    /// Completes a step when a specific interaction occurs with an interactable object.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/InteractionAction")]
    public class InteractionAction : AbstractSequenceAction
    {
        [Tooltip("The interactable object to monitor for interactions.")]
        [SerializeField] private InteractableBase interactableObject;

        [Tooltip("The type of interaction that will complete the step.")]
        [SerializeField] private InteractionType interactionType;

        private void Subscribe()
        {
            if (interactableObject == null) return;

            IObservable<InteractorBase> observable = interactionType switch
            {
                InteractionType.Selected => interactableObject.OnSelected,
                InteractionType.Deselected => interactableObject.OnDeselected,
                InteractionType.Used => interactableObject.OnUseStarted,
                InteractionType.StartedHover => interactableObject.OnHoverStarted,
                InteractionType.EndedHover => interactableObject.OnHoverEnded,
                _ => null
            };

            observable?.Do(_ => CompleteStep()).Subscribe().AddTo(StepDisposable);
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
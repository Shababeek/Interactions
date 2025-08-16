using Shababeek.Interactions;
using UnityEngine;
using Shababeek.Interactions.Core;
using UniRx;

namespace Shababeek.Sequencing
{
    public enum InteractionType
    {
        Selection = 0,
        Deselection = 1,
        Activation = 2,
        HoverStart = 3,
        HoverEnd = 4,
    }

    [CreateAssetMenu(menuName = "Shababeek/Sequencing/Actions/InteractionAction")]
    public class InteractionAction : AbstractSequenceAction
    {
        [SerializeField] private InteractableBase interactableObject;
        [SerializeField] private InteractionType interactionType;

        private CompositeDisposable _disposable;


        void Subscribe()
        {
            switch (interactionType)
            {
                case InteractionType.Selection:
                    interactableObject.OnSelected.Do(OnInteractionStarted).Subscribe().AddTo(_disposable);
                    break;
                case InteractionType.Deselection:
                    interactableObject.OnDeselected.Do(OnInteractionStarted).Subscribe().AddTo(_disposable);
                    break;
                case InteractionType.Activation:
                    interactableObject.OnUseStarted.Do(OnInteractionStarted).Subscribe().AddTo(_disposable);
                    break;
                case InteractionType.HoverStart:
                    interactableObject.OnHoverStarted.Do(OnInteractionStarted).Subscribe().AddTo(_disposable);
                    break;
                case InteractionType.HoverEnd:
                    interactableObject.OnHoverEnded.Do(OnInteractionStarted).Subscribe().AddTo(_disposable);
                    break;
            }
        }

        private void OnInteractionStarted(InteractorBase interactor)
        {
            Step.CompleteStep();
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                _disposable = new CompositeDisposable();
                Subscribe();
            }
            else
            {
                _disposable?.Dispose();
            }
        }
    }
}
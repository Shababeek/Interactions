using Kandooz.InteractionSystem.Interactions;
using UniRx;
using UnityEngine;

namespace Kandooz.Kuest
{
    public enum InteractionType
    {
        Selection = 0,
        Deselection = 1,
        Activation = 2,
        HoverStart = 3,
        HoverEnd = 4,
    }
    [AddComponentMenu("Kandooz/SequenceSystem/Actions/InteractionAcion")]

    [RequireComponent(typeof(StepEvenListener))]
    public class InteractionAction : MonoBehaviour
    {
        [SerializeField] private InteractableBase interactableObject;
        [SerializeField] private InteractionType interactionType;

        private StepEvenListener listener;
        private CompositeDisposable disposable;

        private void Awake()
        {
            listener = GetComponent<StepEvenListener>();
            listener.OnStarted
                .Do(_ => disposable = new CompositeDisposable())
                .Do(Subscribe).Subscribe().AddTo(this);
            listener.OnFinished
                .Do(_ => disposable.Dispose())
                .Subscribe().AddTo(this);
        }

        void Subscribe(Unit unit)
        {
            switch (interactionType)
            {
                case InteractionType.Selection:
                    interactableObject.OnSelected.Do(OnInteractionStarted).Subscribe().AddTo(disposable);
                    break;
                case InteractionType.Deselection:
                    interactableObject.OnDeselected.Do(OnInteractionStarted).Subscribe().AddTo(disposable);
                    break;
                case InteractionType.Activation:
                    interactableObject.OnActivated.Do(OnInteractionStarted).Subscribe().AddTo(disposable);
                    break;
                case InteractionType.HoverStart:
                    interactableObject.OnHoverStarted.Do(OnInteractionStarted).Subscribe().AddTo(disposable);
                    break;
                case InteractionType.HoverEnd:
                    interactableObject.OnHoverEnded.Do(OnInteractionStarted).Subscribe().AddTo(disposable);
                    break;
            }
        }

        private void OnInteractionStarted(InteractorBase interactor)
        {
            listener.OnActionCompleted();
        }
    }
}
using Kandooz.InteractionSystem.Interactions;
using UniRx;
using UnityEngine;

namespace Kandooz.Kuest
{
    [AddComponentMenu("Kandooz/SequenceSystem/Actions/ActivationAction")]

    [RequireComponent(typeof(StepEvenListener))]

    public class ActivatingAction : MonoBehaviour
    {
        [SerializeField] private InteractableBase interactableObject;
        private StepEvenListener listener;
        private CompositeDisposable disposable;
        private void Awake()
        {
            listener = GetComponent<StepEvenListener>();
            listener.OnStarted.Do(OnStarted).Subscribe().AddTo(this);
            listener.OnFinished.Do(_ => disposable.Dispose()).Subscribe().AddTo(this);
        }

        void OnStarted(Unit unit)
        {
            disposable = new();
            interactableObject.OnActivated.Do(OnInteractionStarted).Subscribe().AddTo(disposable);
        }

        private void OnInteractionStarted(InteractorBase interactor)
        {
            listener.OnActionCompleted();
        }
    }
}
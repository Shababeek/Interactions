using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    public enum ActionType {
        ActivationAction,
        AnimationAction,
        ButtonPressAction,
        GazeAction,
        InteractionAction,
        InsertionAction,
        TimerAction,
        TriggerAction,
        VoiceOverAction,
        ComplexAction
    }
    [AddComponentMenu("Shababeek/SequenceSystem/Actions/ActivationAction")]
    public class ActivatingAction : AbstractSequenceAction
    {
        [SerializeField] private ActionType action;
        [SerializeField] private InteractableBase interactableObject;
        private CompositeDisposable _disposable = new CompositeDisposable();
        
        
        private void OnInteractionStarted(InteractorBase interactor)
        {
            Step.CompleteStep();
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                _disposable = new CompositeDisposable();
                interactableObject.OnUseStarted.Do(OnInteractionStarted).Subscribe().AddTo(_disposable);
            }
            else
            {
                _disposable?.Dispose();
            }
        }
    }
}
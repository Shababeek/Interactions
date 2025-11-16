using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Represents the type of action in the sequencing system.
    /// </summary>
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
    
    /// <summary>
    /// Completes a step when an interactable object is used.
    /// </summary>
    [AddComponentMenu("Shababeek/SequenceSystem/Actions/ActivationAction")]
    public class ActivatingAction : AbstractSequenceAction
    {
        [Tooltip("The type of action being performed.")]
        [SerializeField] private ActionType action;
        
        [Tooltip("The interactable object to monitor for use events.")]
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
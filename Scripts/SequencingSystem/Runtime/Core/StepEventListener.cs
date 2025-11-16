using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Listens to step events in the sequencing system and raises Unity events accordingly.
    /// Provides a bridge between the sequencing system and Unity's event system.
    /// </summary>
    /// <remarks>
    /// This component allows you to respond to step status changes (started, completed)
    /// by raising Unity events that can be connected to other systems in the scene.
    /// </remarks>
    [AddComponentMenu("Shababeek/Sequencing/StepEventListener")]
    public class StepEventListener : MonoBehaviour
    {
        [Tooltip("List of steps to listen to with their associated Unity events.")]
        [SerializeField] internal List<StepWithEvents> stepList;
        
        private CompositeDisposable _disposable;

        /// <summary>
        /// Gets or sets the list of steps with their associated events.
        /// </summary>
        /// <value>The list of StepWithEvents containing steps and their Unity events.</value>
        public List<StepWithEvents> StepList
        {
            get => stepList;
            set => stepList = value;
        }

        /// <summary>
        /// Adds a new step to the listener with default event settings.
        /// </summary>
        /// <param name="step">The step to add to the listener.</param>
        /// <remarks>
        /// This method creates a new StepWithEvents with the provided step and adds it to the list.
        /// The step will be automatically subscribed to when the component is enabled.
        /// </remarks>
        public void AddStep(Step step)
        {
            var swe = new StepWithEvents();
            swe.step = step;
            stepList ??= new List<StepWithEvents>();
            stepList.Add(swe);
        }

        /// <summary>
        /// Completes the first step in the list.
        /// </summary>
        /// <remarks>
        /// This method completes the first step in the stepList. You can extend this method
        /// to add logic for determining which step to complete based on your requirements.
        /// </remarks>
        public void OnActionCompleted()
        {
            // Complete the first step in the list, or you could add logic to determine which step to complete
            if (stepList.Count > 0 && stepList[0].step != null)
                stepList[0].step.CompleteStep();
        }
        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            foreach (var stepWithEvents in stepList)
            {
                if (stepWithEvents.step != null)
                {
                    stepWithEvents.step.OnRaisedData.Do(status => OnStepStatusChanged(stepWithEvents, status))
                        .Subscribe().AddTo(_disposable);
                }
            }
        }
        private void OnDisable()
        {
            _disposable.Dispose();
        }
        private void OnStepStatusChanged(StepWithEvents stepWithEvents, SequenceStatus status)
        {
            switch (status)
            {
                case SequenceStatus.Started:
                    stepWithEvents.onStepStarted?.Invoke();
                    break;
                case SequenceStatus.Completed:
                    stepWithEvents.onStepCompleted?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Container class that associates a step with Unity events.
        /// </summary>
        [Serializable]
        public class StepWithEvents
        {
            [Tooltip("The step to listen to for status changes.")]
            public Step step;
            
            [Tooltip("Unity event raised when the step starts.")]
            public UnityEvent onStepStarted;
            
            [Tooltip("Unity event raised when the step completes.")]
            public UnityEvent onStepCompleted;
        }
    }
}
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    [AddComponentMenu("Shababeek/Sequencing/StepEventListener")]
    public class StepEventListener : MonoBehaviour
    {
        [SerializeField] internal List<StepWithEvents> stepList;
        private CompositeDisposable _disposable;

        public List<StepWithEvents> StepList
        {
            get => stepList;
            set => stepList = value;
        }

        public void AddStep(Step step)
        {
            var swe = new StepWithEvents();
            stepList ??= new List<StepWithEvents>();
            stepList.Add(swe);
                
        }

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

        [Serializable]
        public class StepWithEvents
        {
            public Step step;
            public UnityEvent onStepStarted;
            public UnityEvent onStepCompleted;
        }
    }
}
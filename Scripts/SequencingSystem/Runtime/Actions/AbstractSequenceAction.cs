using System;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Base class for all sequence actions that respond to step status changes.
    /// Provides common subscription management and lifecycle handling.
    /// </summary>
    public abstract class AbstractSequenceAction : MonoBehaviour
    {
        [SerializeField] private Step step;
        [ReadOnly][SerializeField] private bool started;
        

        /// <summary>
        /// Main disposable for the action's lifetime (OnEnable to OnDisable).
        /// </summary>
        private CompositeDisposable _disposable;

        /// <summary>
        /// Disposable for subscriptions that should only be active during step execution.
        /// Automatically disposed when step completes or becomes inactive.
        /// </summary>
        protected CompositeDisposable StepDisposable;

        /// <summary>
        /// Gets whether this action has been started.
        /// </summary>
        protected bool Started => started;

        /// <summary>
        /// Gets the step associated with this action.
        /// </summary>
        public Step Step => step;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            started = false;
            step.OnRaisedData.Do(ChangeStatus).Subscribe().AddTo(_disposable);
        }

        private void OnDisable()
        {
            DisposeStepSubscriptions();
            _disposable?.Dispose();
        }

        private void ChangeStatus(SequenceStatus status)
        {
            switch (status)
            {
                case SequenceStatus.Inactive:
                case SequenceStatus.Completed:
                    started = false;
                    DisposeStepSubscriptions();
                    break;
                case SequenceStatus.Started:
                    started = true;
                    InitializeStepSubscriptions();
                    break;
            }
            OnStepStatusChanged(status);
        }
        
        private void InitializeStepSubscriptions()
        {
            DisposeStepSubscriptions();
            StepDisposable = new CompositeDisposable();
        }


        private void DisposeStepSubscriptions()
        {
            StepDisposable?.Dispose();
            StepDisposable = null;
        }

        /// <summary>
        /// Completes the step. Convenience method for derived classes.
        /// </summary>
        protected void CompleteStep()
        {
            Step.CompleteStep();
        }

        /// <summary>
        /// Called when the associated step's status changes.
        /// </summary>
        protected abstract void OnStepStatusChanged(SequenceStatus status);
    }
}
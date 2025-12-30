using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Base class for all sequence actions that respond to step status changes.
    /// </summary>
    public abstract class AbstractSequenceAction : MonoBehaviour
    {
        [Tooltip("The step this action is associated with.")]
        [SerializeField] private Step step;
        protected bool _started;
        protected CompositeDisposable Disposable;

        /// <summary>
        /// Gets whether this action has been started.
        /// </summary>
        public bool Started => _started;

        /// <summary>
        /// Gets the step associated with this action.
        /// </summary>
        public Step Step => step;

        private void OnEnable()
        {
            Disposable = new CompositeDisposable();
            _started = false;
            step.OnRaisedData.Do(ChangeStatus).Subscribe().AddTo(Disposable);
        }

        private void OnDisable()
        {
            Disposable?.Dispose();
        }

        private void ChangeStatus(SequenceStatus status)
        {
            switch (status)
            {
                case SequenceStatus.Inactive:
                case SequenceStatus.Completed:
                    _started = false;
                    break;
                case SequenceStatus.Started:
                    _started = true;
                    break;
            }
            OnStepStatusChanged(status);
        }
        
        /// <summary>
        /// Called when the associated step's status changes.
        /// </summary>
        protected abstract void OnStepStatusChanged(SequenceStatus status);
    }
}
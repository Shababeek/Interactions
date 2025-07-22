using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    public abstract class AbstractSequenceAction : MonoBehaviour
    {
        [SerializeField] private Step step;
        protected bool _started;
        protected CompositeDisposable Disposable;


        public bool Started => _started;

        public Step Step => step;

        private void OnEnable()
        {
            Disposable = new CompositeDisposable();
            _started = false;
            step.OnRaisedData.Do(ChangeStatus).Subscribe().AddTo(Disposable);
        }

        private void OnDisable()
        {
            Disposable.Dispose();
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
        protected abstract void OnStepStatusChanged(SequenceStatus status);
    }
}
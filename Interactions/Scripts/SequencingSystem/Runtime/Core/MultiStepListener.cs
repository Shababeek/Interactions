using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    [AddComponentMenu("Shababeek/SequenceSystem/MultiStepListener")]
    public class MultiStepListener : MonoBehaviour
    {
        [SerializeField] internal Step[] steps;
        [SerializeField] private UnityEvent onStarted;
        [SerializeField] private UnityEvent onEnded;
        private bool current;
        public bool Current => current;
        

        public IObservable<Unit> OnStarted => onStarted.AsObservable();
        public IObservable<Unit> OnFinished => onEnded.AsObservable();
        private CompositeDisposable disposable;

        private void OnEnable()
        {
            disposable = new();
            foreach (var step in steps)
            {
                step.OnRaisedData.Do(OnStatusChanged).Subscribe().AddTo(disposable);
            }
        }

        private void OnDisable()
        {
            disposable.Dispose();
        }

        public void OnStatusChanged(SequenceStatus elementStatus)
        {
            switch (elementStatus)
            {
                case SequenceStatus.Started:
                    current = true;
                    onStarted?.Invoke();
                    break;
                case SequenceStatus.Completed:
                    current = false;
                    onEnded?.Invoke();
                    break;
            }
        }
    }
}
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Kandooz.Kuest
{
    [AddComponentMenu("Kandooz/SequenceSystem/StepListener")]
    public class StepEvenListener : MonoBehaviour, IObserver<SequenceStatus>
    {
        [SerializeField] internal Step step;
        [SerializeField] private UnityEvent onStarted;
        [SerializeField] private UnityEvent onEnded;
        private bool current;
        public bool Current => current;
        public void OnActionCompleted() =>step.OnActionCompleted();

        public IObservable<Unit> OnStarted => onStarted.AsObservable();
        public IObservable<Unit> OnFinished => onEnded.AsObservable();
        private IDisposable disposable;
        private void OnEnable()
        {
            //TODO rewrite to use UniRX
            disposable = step.OnRaised.Subscribe(this);
        }
        private void OnDisable()
        {
            disposable.Dispose();
        }

        public void OnCompleted()
        {
            Destroy(this);
        }

        public void OnError(Exception error)
        {
            Debug.LogError(error);
        }

        public void OnNext(SequenceStatus elementStatus)
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
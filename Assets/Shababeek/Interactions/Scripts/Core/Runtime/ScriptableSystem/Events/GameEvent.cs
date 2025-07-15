using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Core
{
    public abstract class GameEvent<T> : GameEvent, IObservable<T>
    {
        private new readonly Subject<T> _onRaised = new();
        public IObservable<T> OnRaisedData => _onRaised;

        protected abstract T DefaultValue { get; }

        public override void Raise()
        {
            base.Raise();
            _onRaised.OnNext(DefaultValue);
        }

        public void Raise(T data)
        {
            try
            {
                Raise();
                _onRaised.OnNext(data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return OnRaisedData.Subscribe(observer);
        }
    }

    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Events/GameEvent")]
    public class GameEvent : ScriptableObject, IObservable<Unit>
    {
        [SerializeField] protected UnityEvent onRaised;
        public IObservable<Unit> OnRaised => onRaised.AsObservable();
        public virtual void Raise() => onRaised.Invoke();

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            return onRaised.AsObservable().Subscribe(observer);
        }
    }
}
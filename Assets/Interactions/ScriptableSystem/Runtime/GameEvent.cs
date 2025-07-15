using System;
using UniRx;
using UnityEngine;

namespace Kandooz.ScriptableSystem
{
    public class GameEvent<T> : ScriptableObject
    {
        private Subject<T> onRaised = new();
        public IObservable<T> OnRaised => onRaised;
 
        public void Raise(T data)
        {
            try
            {
                onRaised.OnNext(data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        protected virtual void Complete()
        {
            try
            {
                onRaised.OnCompleted();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
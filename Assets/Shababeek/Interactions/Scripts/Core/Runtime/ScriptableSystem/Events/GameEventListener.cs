using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Core
{
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent @event;
        [SerializeField] private UnityEvent onRaised;
        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (@event != null)
            {
                @event.OnRaised.Do(_=>OnEventRaised()).Subscribe().AddTo(_disposable);
                _disposable = new CompositeDisposable();
            }
        }

        private void OnDisable()
        {
          _disposable?.Dispose();
        }

        private void OnEventRaised()
        {
            onRaised?.Invoke();
        }
        
    }
}
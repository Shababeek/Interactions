using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Core
{
    /// <summary>
    /// Listener for game events that can be subscribed to and respond to event triggers.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("The game event to listen to.")]
        [SerializeField] private GameEvent @event;
        [Tooltip("Event raised when the game event is triggered.")]
        [SerializeField] private UnityEvent onRaised;
        private CompositeDisposable _disposable;

        private void OnEnable()
        {
            if (@event != null)
            {
                @event.OnRaised.Do(_ => OnEventRaised()).Subscribe().AddTo(_disposable);
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
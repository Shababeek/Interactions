using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Core
{
    /// <summary>
    /// Base class for game events that can be raised and observed.
    /// </summary>
    [Serializable]
    public abstract class GameEvent<T> : GameEvent
    {
        
        private new readonly Subject<T> _onRaised = new();
        public IObservable<T> OnRaisedData => _onRaised;

        protected abstract T DefaultValue { get; }
        /// <summary>
        /// Raises the event with the default value.
        /// </summary>
        public override void Raise()
        {
            base.Raise();
            _onRaised.OnNext(DefaultValue);
        }
        /// <summary>
        /// Raises the event with the provided data.
        /// </summary>
        /// <param name="data">The data to raise the event with.</param>
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

    }

    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Events/GameEvent")]
    /// <summary>
    /// Base class for game events that can be raised and observed without data.
    /// </summary>
    public class GameEvent : ScriptableObject
    {
        [Tooltip("Event raised when the event is triggered.")]
        [SerializeField] protected UnityEvent onRaised;
        /// <summary>
        /// Event raised when the event is triggered.
        /// </summary>
        public IObservable<Unit> OnRaised => onRaised.AsObservable();
        /// <summary>
        ///     Raises the event, invoking all subscribers.
        /// </summary>
        public virtual void Raise() => onRaised.Invoke();
    }
}
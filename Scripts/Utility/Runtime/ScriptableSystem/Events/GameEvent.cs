using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Base class for game events that can be raised and observed with data.
    /// </summary>
    /// <typeparam name="T">The type of data associated with this event</typeparam>
    /// <remarks>
    /// This generic class extends GameEvent to support events with data payloads.
    /// It provides both UnityEvent integration and UniRx IObservable support.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a float event
    /// var floatEvent = ScriptableObject.CreateInstance<GameEvent<float>>();
    /// 
    /// // Subscribe to the event
    /// floatEvent.OnRaisedData.Subscribe(value => Debug.Log($"Received: {value}"));
    /// 
    /// // Raise the event with data
    /// floatEvent.Raise(42f);
    /// </code>
    /// </example>
    [Serializable]
    public abstract class GameEvent<T> : GameEvent
    {
        //TODO: seperate GameEvent<T> from GameEvent cause it's adding unnecessary complexity
        private new readonly Subject<T> _onRaised = new();
        
        /// <summary>
        /// Observable stream for when this event is raised with data.
        /// </summary>
        /// <remarks>
        /// Subscribe to this observable to receive notifications when the event is raised.
        /// This provides a reactive programming approach to event handling.
        /// </remarks>
        public IObservable<T> OnRaisedData => _onRaised;

        /// <summary>
        /// Gets the default value for this event type.
        /// </summary>
        /// <remarks>
        /// This abstract property must be implemented by derived classes to provide
        /// a sensible default value when raising the event without data.
        /// </remarks>
        protected abstract T DefaultValue { get; }
        
        /// <summary>
        /// Raises the event with the default value.
        /// </summary>
        /// <remarks>
        /// This method calls the base Raise() method and then raises the data event
        /// with the default value for this event type.
        /// </remarks>
        public override void Raise()
        {
            base.Raise();
            _onRaised.OnNext(DefaultValue);
        }
        
        /// <summary>
        /// Raises the event with the provided data.
        /// </summary>
        /// <param name="data">The data to raise the event with</param>
        /// <remarks>
        /// This method raises both the base event and the data event with the specified value.
        /// Any errors during the raise process are logged but do not prevent the event from firing.
        /// </remarks>
        public void Raise(T data)
        {
            try
            {
                Raise();
                _onRaised.OnNext(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error raising GameEvent<{typeof(T).Name}>: {e}");
            }
        }
    }

    /// <summary>
    /// Base class for game events that can be raised and observed without data.
    /// </summary>
    /// <remarks>
    /// This class provides the foundation for the event system, integrating Unity's event system
    /// with UniRx observables. It can be used directly for simple events or extended for events with data.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a simple event
    /// var event = ScriptableObject.CreateInstance<GameEvent>();
    /// 
    /// // Subscribe to the event
    /// event.OnRaised.Subscribe(_ => Debug.Log("Event raised!"));
    /// 
    /// // Raise the event
    /// event.Raise();
    /// </code>
    /// </example>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Events/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        [Header("Event Configuration")]
        [Tooltip("Unity event that will be invoked when this game event is raised. Connect other systems here through the inspector.")]
        [SerializeField] protected UnityEvent onRaised;
        
        /// <summary>
        /// Observable stream for when this event is raised.
        /// </summary>
        /// <remarks>
        /// Subscribe to this observable to receive notifications when the event is raised.
        /// This provides a reactive programming approach to event handling.
        /// </remarks>
        public IObservable<Unit> OnRaised => onRaised.AsObservable();
        
        /// <summary>
        /// Raises the event, invoking all subscribers.
        /// </summary>
        /// <remarks>
        /// This method invokes the UnityEvent, which can trigger connected systems
        /// in the inspector, and notifies all UniRx subscribers.
        /// </remarks>
        public virtual void Raise() => onRaised.Invoke();
    }
}
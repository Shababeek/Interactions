using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Base class for scriptable variables that can be observed and raised as game events.
    /// </summary>
    /// <typeparam name="T">The type of value stored by this variable</typeparam>
    /// <remarks>
    /// This generic class combines the functionality of a ScriptableObject variable with
    /// event system integration. It can be observed for value changes and raised as a game event.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a float variable
    /// var floatVar = ScriptableObject.CreateInstance<ScriptableVariable<float>>();
    /// 
    /// // Subscribe to value changes
    /// floatVar.OnValueChanged.Subscribe(value => Debug.Log($"Value changed to: {value}"));
    /// 
    /// // Set a new value (triggers events)
    /// floatVar.Value = 42f;
    /// </code>
    /// </example>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/ScriptableVariable")]
    [Serializable]
    public class ScriptableVariable<T> : ScriptableVariable, IObservable<T>
    {
        [Header("Variable Value")]
        [Tooltip("The current value stored by this scriptable variable.")]
        [SerializeField] protected T value;
        
        private readonly Subject<T> _onValueChanged = new();
        
        /// <summary>
        /// Observable stream for when the variable's value changes.
        /// </summary>
        /// <remarks>
        /// Subscribe to this observable to receive notifications whenever the Value property
        /// is set to a new value. This provides reactive programming capabilities.
        /// </remarks>
        public IObservable<T> OnValueChanged => _onValueChanged;
        
        /// <summary>
        /// Gets or sets the current value of this variable.
        /// </summary>
        /// <remarks>
        /// Setting this property will automatically raise the game event and notify
        /// all subscribers of the value change. Getting the value does not trigger any events.
        /// </remarks>
        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                Raise();
            }
        }

        /// <summary>
        /// Raises the game event with the current value.
        /// </summary>
        /// <remarks>
        /// This method calls the base Raise() method and then notifies all value change
        /// subscribers with the current value.
        /// </remarks>
        public override void Raise()
        {
            base.Raise();
            _onValueChanged.OnNext(value);
        }

        /// <summary>
        /// Raises the game event with the provided data.
        /// </summary>
        /// <param name="data">The data to raise the event with</param>
        /// <remarks>
        /// This method raises both the base game event and the value change event with
        /// the specified data. Any errors during the raise process are logged.
        /// </remarks>
        public void Raise(T data)
        {
            try
            {
                Raise();
                _onValueChanged.OnNext(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error raising ScriptableVariable<{typeof(T).Name}>: {e}");
            }
        }

        /// <summary>
        /// Subscribes an observer to value change notifications.
        /// </summary>
        /// <param name="observer">The observer to subscribe</param>
        /// <returns>A disposable subscription that can be used to unsubscribe</returns>
        /// <remarks>
        /// This method implements IObservable<T> to allow reactive programming patterns.
        /// The returned disposable should be disposed when no longer needed to prevent memory leaks.
        /// </remarks>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return OnValueChanged.Subscribe(observer);
        }

        /// <summary>
        /// Returns a string representation of the current value.
        /// </summary>
        /// <returns>A string representation of the value</returns>
        public override string ToString()
        {
            return value?.ToString() ?? "null";
        }

        /// <summary>
        /// DON'T USE THIS METHOD
        /// it's only for prototyping and will be removed in the future
        /// </summary>
        /// <param name="value">The object value to set</param>
        [Obsolete("This method should not be used, I it only exsist for whan am prototyping and don't want to waste time on a proper archticture")]
        public override void SetValue(object value)
        {
            if (value is T typedValue)
            {
                Value = typedValue;
            }
            else
            {
                Debug.LogError($"Cannot set value of type {value?.GetType()} to ScriptableVariable<{typeof(T).Name}>");
            }
        }

         /// <summary>
        /// DON'T USE THIS METHOD
        /// it's only for prototyping and will be removed in the future
        /// </summary>
        /// <returns>The current value as an object</returns>
        [Obsolete("This method should not be used, I it only exsist for whan am prototyping and don't want to waste time on a proper archticture")]
        public override object GetValue()
        {
            return Value;
        }
    }

    /// <summary>
    /// Abstract base class for all scriptable variables.
    /// </summary>
    /// <remarks>
    /// This class provides the common interface for all scriptable variables, including
    /// the ability to raise game events and access values through object references.
    /// </remarks>
    public abstract class ScriptableVariable : GameEvent
    {
        //TODO: remove this class, I already figured out another workaround for the editor issue
        /// <summary>
        /// Returns a string representation of the variable's value.
        /// </summary>
        /// <returns>A string representation of the value</returns>
        public abstract override string ToString();

        /// <summary>
        /// Sets the variable's value using an object parameter.
        /// </summary>
        /// <param name="value">The object value to set</param>
        public abstract void SetValue(object value);

        /// <summary>
        /// Gets the variable's current value as an object.
        /// </summary>
        /// <returns>The current value as an object</returns>
        public abstract object GetValue();
    }
}
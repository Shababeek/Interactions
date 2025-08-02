using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Core
{
    /// <summary>
    /// Base class for scriptable variables that can be observed and raised as game events.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/ScriptableVariable")]
    [Serializable]
    public class ScriptableVariable<T> : ScriptableVariable, IObservable<T>
    {
        
        [SerializeField] protected T value;
        private readonly Subject<T> _onValueChanged = new();
        public IObservable<T> OnValueChanged => _onValueChanged;
        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                Raise();
            }
        }

        public override void Raise()
        {
            base.Raise();
            _onValueChanged.OnNext(value);
        }

        public void Raise(T data)
        {
            try
            {
                Raise();
                _onValueChanged.OnNext(data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return OnValueChanged.Subscribe(observer);
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override void SetValue(object value)
        {
            if (value is T value1)
            {
                Value = value1;
            }
            else
            {
                Debug.LogError("wrong type");
            }
        }

        public override object GetValue()
        {
            return Value;
        }
    }

    public abstract class ScriptableVariable : GameEvent
    {
        public abstract override string ToString();


        public abstract void SetValue(object value);

        public abstract object GetValue();
    }
}
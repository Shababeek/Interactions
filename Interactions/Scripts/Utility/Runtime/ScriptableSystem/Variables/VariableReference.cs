using System;
using UnityEngine;
using UniRx;

namespace Shababeek.Utilities
{
    /// <summary>
    /// A reference that can point to either a ScriptableVariable or use a constant value.
    /// Provides UniRx integration for reactive programming.
    /// </summary>
    [Serializable]
    public class VariableReference<T>
    {
        [SerializeField] private ScriptableVariable<T> variable;
        [SerializeField] private bool useConstant;
        [SerializeField] private T constantValue;
        [SerializeField] private string name;
        private IDisposable _subscription;
        private readonly Subject<T> _onValueChangedSubject = new();

        public string Name
        {
            get => useConstant ? name : variable.name;
            set
            {
                if (useConstant) name = value;
                else variable.name = value;
            }
        }

        /// <summary>
        /// Gets the current value (either from the variable or constant).
        /// </summary>
        public T Value
        {
            get => useConstant ? constantValue : (variable.Value);
            set
            {
                if (useConstant)
                {
                    constantValue = value;
                    _onValueChangedSubject.OnNext(constantValue);
                }
                else if (variable != null)
                {
                    variable.Value = value;
                }
            }
        }

        /// <summary>
        /// Observable that fires when the value changes.
        /// </summary>
        public IObservable<T> OnValueChanged =>
            useConstant ? _onValueChangedSubject.AsObservable() : variable.OnValueChanged;

        public static implicit operator T(VariableReference<T> reference) => reference.Value;
    }
}
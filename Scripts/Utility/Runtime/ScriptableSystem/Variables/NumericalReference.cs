using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// A reference that can point to any numeric variable (IntVariable or FloatVariable) or use a constant float value.
    /// </summary>
    [Serializable]
    public class NumericalReference
    {
        [Tooltip("If true, uses the constant value. If false, uses the variable reference.")]
        [SerializeField] private bool useConstant = true;

        [Tooltip("The constant float value to use when useConstant is true.")]
        [SerializeField] private float constantValue;

        [Tooltip("The variable to reference (can be IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        /// <summary>
        /// Gets or sets whether this reference uses a constant value.
        /// </summary>
        public bool UseConstant
        {
            get => useConstant;
            set => useConstant = value;
        }

        /// <summary>
        /// Gets or sets the constant value.
        /// </summary>
        public float ConstantValue
        {
            get => constantValue;
            set => constantValue = value;
        }

        /// <summary>
        /// Gets or sets the referenced variable.
        /// </summary>
        public ScriptableVariable Variable
        {
            get => variable;
            set => variable = value;
        }

        /// <summary>
        /// Gets the current value, either from the constant or the variable.
        /// </summary>
        public float Value
        {
            get
            {
                if (useConstant) return constantValue;
                return GetNumericValue();
            }
            set
            {
                if (useConstant)
                {
                    constantValue = value;
                }
                else
                {
                    SetNumericValue(value);
                }
            }
        }

        /// <summary>
        /// Gets the value as an integer.
        /// </summary>
        public int IntValue => Mathf.RoundToInt(Value);

        /// <summary>
        /// Gets an observable that fires when the value changes.
        /// </summary>
        /// <remarks>
        /// If using a constant, returns an empty observable that never fires.
        /// If using a variable, returns the variable's OnValueChanged observable converted to float.
        /// </remarks>
        public IObservable<float> OnValueChanged
        {
            get
            {
                if (useConstant || variable == null)
                    return Observable.Empty<float>();

                if (variable is INumericalVariable numVar)
                {
                    // Subscribe to the variable's changes and convert to float
                    return variable.OnRaised.Select(_ => numVar.AsFloat);
                }

                return Observable.Empty<float>();
            }
        }

        /// <summary>
        /// Gets the display name of this reference.
        /// </summary>
        public string Name
        {
            get
            {
                if (useConstant) return constantValue.ToString("F2");
                return variable != null ? variable.name : "None";
            }
        }

        /// <summary>
        /// Creates a new NumericalReference with a constant value.
        /// </summary>
        public NumericalReference()
        {
            useConstant = true;
            constantValue = 0f;
        }

        /// <summary>
        /// Creates a new NumericalReference with a constant value.
        /// </summary>
        /// <param name="value">The constant value</param>
        public NumericalReference(float value)
        {
            useConstant = true;
            constantValue = value;
        }

        /// <summary>
        /// Creates a new NumericalReference pointing to a variable.
        /// </summary>
        /// <param name="variable">The variable to reference</param>
        public NumericalReference(ScriptableVariable variable)
        {
            useConstant = false;
            this.variable = variable;
        }

        private float GetNumericValue()
        {
            if (variable == null) return 0f;

            if (variable is INumericalVariable numVar)
            {
                return numVar.AsFloat;
            }

            // Fallback: try to get value through the deprecated method
            var value = variable.GetValue();
            if (value is float f) return f;
            if (value is int i) return i;
            if (value is double d) return (float)d;

            return 0f;
        }

        private void SetNumericValue(float value)
        {
            if (variable == null) return;

            if (variable is INumericalVariable numVar)
            {
                numVar.SetFromFloat(value);
            }
        }

        /// <summary>
        /// Gets the normalized value (0-1) within the specified range.
        /// </summary>
        /// <param name="min">The minimum value of the range</param>
        /// <param name="max">The maximum value of the range</param>
        /// <returns>The normalized value between 0 and 1</returns>
        public float GetNormalized(float min, float max)
        {
            if (Mathf.Approximately(max, min)) return 0f;
            return Mathf.Clamp01((Value - min) / (max - min));
        }

        /// <summary>
        /// Implicit conversion to float.
        /// </summary>
        public static implicit operator float(NumericalReference reference)
        {
            return reference?.Value ?? 0f;
        }

        /// <summary>
        /// Implicit conversion to int.
        /// </summary>
        public static implicit operator int(NumericalReference reference)
        {
            return reference?.IntValue ?? 0;
        }
    }
}

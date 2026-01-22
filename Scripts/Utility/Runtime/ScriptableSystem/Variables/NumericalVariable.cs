using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Base class for numeric scriptable variables (int, float, double, etc.)
    /// Provides common operations for numeric types, enabling type-agnostic binders.
    /// </summary>
    /// <typeparam name="T">The numeric type stored by this variable</typeparam>
    public abstract class NumericalVariable<T> : ScriptableVariable<T>, INumericalVariable where T : struct
    {
        /// <summary>
        /// Gets the value as a float for normalized operations.
        /// </summary>
        public abstract float AsFloat { get; }

        /// <summary>
        /// Gets the value as an integer for integer-based operations.
        /// </summary>
        public abstract int AsInt { get; }

        
        public abstract void SetFromFloat(float value);
        
        public abstract void Add(float amount);


        public virtual void Subtract(float amount) => Add(-amount);
        
        public abstract void Multiply(float factor);
        
        public virtual void Divide(float divisor)
        {
            if (Mathf.Approximately(divisor, 0f))
            {
                Debug.LogWarning($"Cannot divide by zero in {name}");
                return;
            }
            Multiply(1f / divisor);
        }
        
        public abstract void Clamp(float min, float max);

        /// <summary>
        /// Returns the normalized value (0-1) given a minimum and maximum range.
        /// </summary>
        /// <param name="min">The minimum value of the range (maps to 0)</param>
        /// <param name="max">The maximum value of the range (maps to 1)</param>
        /// <returns>A value between 0 and 1 representing the position within the range</returns>
       
        public float GetNormalized(float min, float max)
        {
            if (Mathf.Approximately(max, min)) return 0f;
            return Mathf.Clamp01((AsFloat - min) / (max - min));
        }

        /// <summary>
        /// Sets the value based on a normalized input (0-1) and a given range.
        /// </summary>
        /// <param name="normalized">The normalized value (0-1)</param>
        /// <param name="min">The minimum value of the range</param>
        /// <param name="max">The maximum value of the range</param>
        public void SetFromNormalized(float normalized, float min, float max)
        {
            float value = Mathf.Lerp(min, max, Mathf.Clamp01(normalized));
            SetFromFloat(value);
        }

        /// <summary>
        /// Linearly interpolates the value towards a target.
        /// </summary>
        /// <param name="target">The target value to lerp towards</param>
        /// <param name="t">The interpolation factor (0-1)</param>
        public void LerpTo(float target, float t)
        {
            SetFromFloat(Mathf.Lerp(AsFloat, target, Mathf.Clamp01(t)));
        }

        /// <summary>
        /// Moves the value towards a target by a maximum delta.
        /// </summary>
        /// <param name="target">The target value to move towards</param>
        /// <param name="maxDelta">The maximum change allowed</param>
        public void MoveTowards(float target, float maxDelta)
        {
            SetFromFloat(Mathf.MoveTowards(AsFloat, target, maxDelta));
        }
    }

    /// <summary>
    /// Interface for numeric variables, allowing type-agnostic operations.
    /// </summary>

    public interface INumericalVariable
    {
        /// <summary>Gets the value as a float.</summary>
        float AsFloat { get; }

        /// <summary>Gets the value as an integer.</summary>
        int AsInt { get; }

        /// <summary>Sets the value from a float.</summary>
        void SetFromFloat(float value);

        /// <summary>Adds an amount to the value.</summary>
        void Add(float amount);

        /// <summary>Subtracts an amount from the value.</summary>
        void Subtract(float amount);

        /// <summary>Multiplies the value by a factor.</summary>
        void Multiply(float factor);

        /// <summary>Divides the value by a divisor.</summary>
        void Divide(float divisor);

        /// <summary>Clamps the value between min and max.</summary>
        void Clamp(float min, float max);

        /// <summary>Gets the normalized value (0-1) within a range.</summary>
        float GetNormalized(float min, float max);

        /// <summary>Sets the value from a normalized input within a range.</summary>
        void SetFromNormalized(float normalized, float min, float max);

        /// <summary>Lerps the value towards a target.</summary>
        void LerpTo(float target, float t);

        /// <summary>Moves the value towards a target by a max delta.</summary>
        void MoveTowards(float target, float maxDelta);
    }
    
}

using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores an integer value with full arithmetic operator support.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/IntVariable")]
    public class IntVariable : NumericalVariable<int>
    {
        #region INumericalVariable Implementation

        /// <inheritdoc/>
        public override float AsFloat => Value;

        /// <inheritdoc/>
        public override int AsInt => Value;

        /// <inheritdoc/>
        public override void SetFromFloat(float value) => Value = Mathf.RoundToInt(value);

        /// <inheritdoc/>
        public override void Add(float amount) => Value += Mathf.RoundToInt(amount);

        /// <inheritdoc/>
        public override void Multiply(float factor) => Value = Mathf.RoundToInt(Value * factor);

        /// <inheritdoc/>
        public override void Clamp(float min, float max) => Value = Mathf.Clamp(Value, Mathf.RoundToInt(min), Mathf.RoundToInt(max));

        #endregion

        #region Int-Specific Methods

        /// <summary>
        /// Increments the integer value by 1.
        /// </summary>
        public void Increment()
        {
            Value++;
        }

        /// <summary>
        /// Decrements the integer value by 1.
        /// </summary>
        public void Decrement()
        {
            Value--;
        }

        /// <summary>
        /// Adds an integer amount to the current value.
        /// </summary>
        /// <param name="amount">The integer amount to add</param>
        public void Add(int amount)
        {
            Value += amount;
        }

        /// <summary>
        /// Clamps the value between integer min and max bounds.
        /// </summary>
        /// <param name="min">The minimum allowed value</param>
        /// <param name="max">The maximum allowed value</param>
        public void Clamp(int min, int max)
        {
            Value = Mathf.Clamp(Value, min, max);
        }

        #endregion

        #region Operators

        public static IntVariable operator ++(IntVariable variable)
        {
            variable.Value++;
            return variable;
        }

        public static IntVariable operator --(IntVariable variable)
        {
            variable.Value--;
            return variable;
        }

        public static bool operator ==(IntVariable a, IntVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(IntVariable a, IntVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(IntVariable a, int b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(IntVariable a, int b)
        {
            return !(a == b);
        }

        public static bool operator ==(int a, IntVariable b)
        {
            return b == a;
        }

        public static bool operator !=(int a, IntVariable b)
        {
            return !(b == a);
        }

        // Arithmetic operators
        public static int operator +(IntVariable a, IntVariable b)
        {
            if (a == null || b == null) return 0;
            return a.Value + b.Value;
        }

        public static int operator +(IntVariable a, int b)
        {
            if (a == null) return b;
            return a.Value + b;
        }

        public static int operator +(int a, IntVariable b)
        {
            if (b == null) return a;
            return a + b.Value;
        }

        public static int operator -(IntVariable a, IntVariable b)
        {
            if (a == null || b == null) return 0;
            return a.Value - b.Value;
        }

        public static int operator -(IntVariable a, int b)
        {
            if (a == null) return -b;
            return a.Value - b;
        }

        public static int operator -(int a, IntVariable b)
        {
            if (b == null) return a;
            return a - b.Value;
        }

        public static int operator *(IntVariable a, IntVariable b)
        {
            if (a == null || b == null) return 0;
            return a.Value * b.Value;
        }

        public static int operator *(IntVariable a, int b)
        {
            if (a == null) return 0;
            return a.Value * b;
        }

        public static int operator *(int a, IntVariable b)
        {
            if (b == null) return 0;
            return a * b.Value;
        }

        public static int operator /(IntVariable a, IntVariable b)
        {
            if (a == null || b == null || b.Value == 0) return 0;
            return a.Value / b.Value;
        }

        public static int operator /(IntVariable a, int b)
        {
            if (a == null || b == 0) return 0;
            return a.Value / b;
        }

        public static int operator /(int a, IntVariable b)
        {
            if (b == null || b.Value == 0) return 0;
            return a / b.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is IntVariable other) return this == other;
            if (obj is int intValue) return this == intValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// A reference that can point to either an IntVariable or use a constant integer value.
    /// </summary>
    [System.Serializable]
    public class IntReference : VariableReference<int>
    {
    }
}
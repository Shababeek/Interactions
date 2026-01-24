using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a float value with full arithmetic operator support.
    /// </summary>

    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/FloatVariable")]
    public class FloatVariable : NumericalVariable<float>
    {
        #region INumericalVariable Implementation

        /// <inheritdoc/>
        public override float AsFloat => Value;

        /// <inheritdoc/>
        public override int AsInt => Mathf.RoundToInt(Value);

        /// <inheritdoc/>
        public override void SetFromFloat(float value) => Value = value;

        /// <inheritdoc/>
        public override void Add(float amount) => Value += amount;

        /// <inheritdoc/>
        public override void Multiply(float factor) => Value *= factor;

        /// <inheritdoc/>
        public override void Clamp(float min, float max) => Value = Mathf.Clamp(Value, min, max);

        #endregion

        #region Operators

        public static bool operator ==(FloatVariable a, FloatVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return Mathf.Approximately(a.Value, b.Value);
        }


        public static bool operator !=(FloatVariable a, FloatVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(FloatVariable a, float b)
        {
            if (ReferenceEquals(a, null)) return false;
            return Mathf.Approximately(a.Value, b);
        }

        public static bool operator !=(FloatVariable a, float b)
        {
            return !(a == b);
        }

        public static bool operator ==(float a, FloatVariable b)
        {
            return b == a;
        }

        public static bool operator !=(float a, FloatVariable b)
        {
            return !(b == a);
        }

        // Arithmetic operators
        public static float operator +(FloatVariable a, FloatVariable b)
        {
            if (a == null || b == null) return 0f;
            return a.Value + b.Value;
        }

        public static float operator +(FloatVariable a, float b)
        {
            if (a == null) return b;
            return a.Value + b;
        }

        public static float operator +(float a, FloatVariable b)
        {
            if (b == null) return a;
            return a + b.Value;
        }

        public static float operator -(FloatVariable a, FloatVariable b)
        {
            if (a == null || b == null) return 0f;
            return a.Value - b.Value;
        }

        public static float operator -(FloatVariable a, float b)
        {
            if (a == null) return -b;
            return a.Value - b;
        }

        public static float operator -(float a, FloatVariable b)
        {
            if (b == null) return a;
            return a - b.Value;
        }

        public static float operator *(FloatVariable a, FloatVariable b)
        {
            if (a == null || b == null) return 0f;
            return a.Value * b.Value;
        }

        public static float operator *(FloatVariable a, float b)
        {
            if (a == null) return 0f;
            return a.Value * b;
        }

        public static float operator *(float a, FloatVariable b)
        {
            if (b == null) return 0f;
            return a * b.Value;
        }

        public static float operator /(FloatVariable a, FloatVariable b)
        {
            if (a == null || b == null || Mathf.Approximately(b.Value, 0f)) return 0f;
            return a.Value / b.Value;
        }

        public static float operator /(FloatVariable a, float b)
        {
            if (a == null || Mathf.Approximately(b, 0f)) return 0f;
            return a.Value / b;
        }

        public static float operator /(float a, FloatVariable b)
        {
            if (b == null || Mathf.Approximately(b.Value, 0f)) return 0f;
            return a / b.Value;
        }

        // Use reference equality for Equals (standard object behavior)
        // Use == operator for value comparison in code
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();

        // Increment and decrement operators
        public static FloatVariable operator ++(FloatVariable variable)
        {
            if (variable != null)
                variable.Value++;
            return variable;
        }

        public static FloatVariable operator --(FloatVariable variable)
        {
            if (variable != null)
                variable.Value--;
            return variable;
        }

        #endregion
    }

    /// <summary>
    /// A reference that can point to either a FloatVariable or use a constant float value.
    /// Provides type-safe float variable handling with UniRx integration.
    /// </summary>
    /// <remarks>
    /// This class allows you to reference either a ScriptableObject FloatVariable or use
    /// a constant float value. It's useful for creating flexible systems that can work
    /// with both dynamic and static values.
    /// </remarks>
    [System.Serializable]
    public class FloatReference : VariableReference<float>
    {
    }
}
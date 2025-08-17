using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a float value with full arithmetic operator support.
    /// </summary>
    /// <remarks>
    /// This class extends ScriptableVariable<float> to provide mathematical operators for
    /// float operations. It supports comparison, arithmetic, and increment/decrement operations
    /// between FloatVariable instances and float values.
    /// </remarks>
    /// <example>
    /// <code>
    /// var var1 = ScriptableObject.CreateInstance<FloatVariable>();
    /// var var2 = ScriptableObject.CreateInstance<FloatVariable>();
    /// 
    /// var1.Value = 5f;
    /// var2.Value = 3f;
    /// 
    /// float result = var1 + var2; // result = 8f
    /// bool isEqual = var1 == 5f;  // isEqual = true
    /// </code>
    /// </example>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/FloatVariable")]
    public class FloatVariable : ScriptableVariable<float>
    {
        #region Comparison Operators

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

        public override bool Equals(object obj)
        {
            if (obj is FloatVariable other) return this == other;
            if (obj is float floatValue) return this == floatValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

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
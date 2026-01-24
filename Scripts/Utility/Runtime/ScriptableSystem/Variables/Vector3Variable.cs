using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a Vector3 value with full arithmetic operator support.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/Vector3Variable")]
    public class Vector3Variable : ScriptableVariable<Vector3>
    {
        // Equality operators
        public static bool operator ==(Vector3Variable a, Vector3Variable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(Vector3Variable a, Vector3Variable b)
        {
            return !(a == b);
        }

        public static bool operator ==(Vector3Variable a, Vector3 b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(Vector3Variable a, Vector3 b)
        {
            return !(a == b);
        }

        public static bool operator ==(Vector3 a, Vector3Variable b)
        {
            return b == a;
        }

        public static bool operator !=(Vector3 a, Vector3Variable b)
        {
            return !(b == a);
        }

        // Arithmetic operators
        public static Vector3 operator +(Vector3Variable a, Vector3Variable b)
        {
            if (a == null && b == null) return Vector3.zero;
            if (a == null) return b.Value;
            if (b == null) return a.Value;
            return a.Value + b.Value;
        }

        public static Vector3 operator +(Vector3Variable a, Vector3 b)
        {
            if (a == null) return b;
            return a.Value + b;
        }

        public static Vector3 operator +(Vector3 a, Vector3Variable b)
        {
            if (b == null) return a;
            return a + b.Value;
        }

        public static Vector3 operator -(Vector3Variable a, Vector3Variable b)
        {
            if (a == null && b == null) return Vector3.zero;
            if (a == null) return -b.Value;
            if (b == null) return a.Value;
            return a.Value - b.Value;
        }

        public static Vector3 operator -(Vector3Variable a, Vector3 b)
        {
            if (a == null) return -b;
            return a.Value - b;
        }

        public static Vector3 operator -(Vector3 a, Vector3Variable b)
        {
            if (b == null) return a;
            return a - b.Value;
        }

        public static Vector3 operator *(Vector3Variable a, float b)
        {
            if (a == null) return Vector3.zero;
            return a.Value * b;
        }

        public static Vector3 operator *(float a, Vector3Variable b)
        {
            if (b == null) return Vector3.zero;
            return a * b.Value;
        }

        public static Vector3 operator /(Vector3Variable a, float b)
        {
            if (a == null || Mathf.Approximately(b, 0f)) return Vector3.zero;
            return a.Value / b;
        }

        // Use reference equality for Equals (standard object behavior)
        // Use == operator for value comparison in code
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// A reference that can point to either a Vector3Variable or use a constant Vector3 value.
    /// </summary>
    public class Vector3Reference : VariableReference<Vector3>
    {
    }
}
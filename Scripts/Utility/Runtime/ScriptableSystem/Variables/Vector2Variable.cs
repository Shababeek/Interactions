using UnityEngine;

namespace Shababeek.Utilities
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/Vector2Variable")]
    public class Vector2Variable : ScriptableVariable<Vector2>
    {
        public static bool operator ==(Vector2Variable a, Vector2Variable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(Vector2Variable a, Vector2Variable b)
        {
            return !(a == b);
        }

        public static bool operator ==(Vector2Variable a, Vector2 b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(Vector2Variable a, Vector2 b)
        {
            return !(a == b);
        }

        public static bool operator ==(Vector2 a, Vector2Variable b)
        {
            return b == a;
        }

        public static bool operator !=(Vector2 a, Vector2Variable b)
        {
            return !(b == a);
        }

        public static Vector2 operator +(Vector2Variable a, Vector2Variable b)
        {
            if (a == null && b == null) return Vector2.zero;
            if (a == null) return b.Value;
            if (b == null) return a.Value;
            return a.Value + b.Value;
        }

        public static Vector2 operator +(Vector2Variable a, Vector2 b)
        {
            if (a == null) return b;
            return a.Value + b;
        }

        public static Vector2 operator +(Vector2 a, Vector2Variable b)
        {
            if (b == null) return a;
            return a + b.Value;
        }

        public static Vector2 operator -(Vector2Variable a, Vector2Variable b)
        {
            if (a == null && b == null) return Vector2.zero;
            if (a == null) return -b.Value;
            if (b == null) return a.Value;
            return a.Value - b.Value;
        }

        public static Vector2 operator -(Vector2Variable a, Vector2 b)
        {
            if (a == null) return -b;
            return a.Value - b;
        }

        public static Vector2 operator -(Vector2 a, Vector2Variable b)
        {
            if (b == null) return a;
            return a - b.Value;
        }

        public static Vector2 operator *(Vector2Variable a, float b)
        {
            if (a == null) return Vector2.zero;
            return a.Value * b;
        }

        public static Vector2 operator *(float a, Vector2Variable b)
        {
            if (b == null) return Vector2.zero;
            return a * b.Value;
        }

        public static Vector2 operator /(Vector2Variable a, float b)
        {
            if (a == null || Mathf.Approximately(b, 0f)) return Vector2.zero;
            return a.Value / b;
        }

        // Use reference equality for Equals (standard object behavior)
        // Use == operator for value comparison in code
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }

    public class Vector2Reference : VariableReference<Vector2>
    {
    }
}

using UnityEngine;

namespace Shababeek.Utilities
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/Vector2IntVariable")]
    public class Vector2IntVariable : ScriptableVariable<Vector2Int>
    {
        public static bool operator ==(Vector2IntVariable a, Vector2IntVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(Vector2IntVariable a, Vector2IntVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(Vector2IntVariable a, Vector2Int b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(Vector2IntVariable a, Vector2Int b)
        {
            return !(a == b);
        }

        public static bool operator ==(Vector2Int a, Vector2IntVariable b)
        {
            return b == a;
        }

        public static bool operator !=(Vector2Int a, Vector2IntVariable b)
        {
            return !(b == a);
        }

        public static Vector2Int operator +(Vector2IntVariable a, Vector2IntVariable b)
        {
            if (a == null && b == null) return Vector2Int.zero;
            if (a == null) return b.Value;
            if (b == null) return a.Value;
            return a.Value + b.Value;
        }

        public static Vector2Int operator +(Vector2IntVariable a, Vector2Int b)
        {
            if (a == null) return b;
            return a.Value + b;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2IntVariable b)
        {
            if (b == null) return a;
            return a + b.Value;
        }

        public static Vector2Int operator -(Vector2IntVariable a, Vector2IntVariable b)
        {
            if (a == null && b == null) return Vector2Int.zero;
            if (a == null) return -b.Value;
            if (b == null) return a.Value;
            return a.Value - b.Value;
        }

        public static Vector2Int operator -(Vector2IntVariable a, Vector2Int b)
        {
            if (a == null) return -b;
            return a.Value - b;
        }

        public static Vector2Int operator -(Vector2Int a, Vector2IntVariable b)
        {
            if (b == null) return a;
            return a - b.Value;
        }

        public static Vector2Int operator *(Vector2IntVariable a, int b)
        {
            if (a == null) return Vector2Int.zero;
            return a.Value * b;
        }

        public static Vector2Int operator *(int a, Vector2IntVariable b)
        {
            if (b == null) return Vector2Int.zero;
            return a * b.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2IntVariable other) return this == other;
            if (obj is Vector2Int vec2Value) return this == vec2Value;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class Vector2IntReference : VariableReference<Vector2Int>
    {
    }
}

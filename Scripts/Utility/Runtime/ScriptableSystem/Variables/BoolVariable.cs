using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a boolean value with logical operator support.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/BoolVariable")]
    public class BoolVariable : ScriptableVariable<bool>
    {
        /// <summary>
        /// Toggles the boolean value (true becomes false, false becomes true).
        /// </summary>
        public void Toggle()
        {
            Value = !Value;
        }

        public static bool operator ==(BoolVariable a, BoolVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(BoolVariable a, BoolVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(BoolVariable a, bool b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(BoolVariable a, bool b)
        {
            return !(a == b);
        }

        public static bool operator ==(bool a, BoolVariable b)
        {
            if (ReferenceEquals(b, null)) return false;
            return b.Value == a;
        }

        public static bool operator !=(bool a, BoolVariable b)
        {
            return !(b == a);
        }

        // Logical operators
        public static bool operator &(BoolVariable a, BoolVariable b)
        {
            return a.Value && b.Value;
        }

        public static bool operator &(BoolVariable a, bool b)
        {
            return a.Value && b;
        }

        public static bool operator &(bool a, BoolVariable b)
        {
            return b.Value && a;
        }

        public static bool operator |(BoolVariable a, BoolVariable b)
        {
            return a.Value || b.Value;
        }

        public static bool operator |(BoolVariable a, bool b)
        {
            return a.Value || b;
        }

        public static bool operator |(bool a, BoolVariable b)
        {
            return b.Value || a;
        }

        public static bool operator !(BoolVariable a)
        {
            return !a.Value;
        }

        // Use reference equality for Equals (standard object behavior)
        // Use == operator for value comparison in code
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// A reference that can point to either a BoolVariable or use a constant boolean value.
    /// </summary>
    [System.Serializable]
    public class BoolReference : VariableReference<bool>
    {
    }
}
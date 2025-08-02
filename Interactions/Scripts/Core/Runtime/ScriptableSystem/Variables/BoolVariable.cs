using System;
using UnityEngine;

namespace Shababeek.Core
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/BoolVariable")]
    public class BoolVariable : ScriptableVariable<bool>
    {
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
            return  a.Value && b.Value;
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

        public override bool Equals(object obj)
        {
            if (obj is BoolVariable other) return this == other;
            if (obj is bool boolValue) return this == boolValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
    
    /// <summary>
    /// A reference that can point to either a BoolVariable or use a constant boolean value.
    /// </summary>
    [System.Serializable]
    public class BoolReference : VariableReference<bool>
    { }
} 
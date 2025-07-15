using UnityEngine;

namespace Shababeek.Core
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/TextVariable")]
    public class TextVariable : ScriptableVariable<string>
    {
        // Equality operators
        public static bool operator ==(TextVariable a, TextVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(TextVariable a, TextVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(TextVariable a, string b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(TextVariable a, string b)
        {
            return !(a == b);
        }

        public static bool operator ==(string a, TextVariable b)
        {
            return b == a;
        }

        public static bool operator !=(string a, TextVariable b)
        {
            return !(b == a);
        }

        // String concatenation operator
        public static string operator +(TextVariable a, TextVariable b)
        {
            if (!a  && !b ) return "";
            if (!a) return b.Value ?? "";
            if (!b) return a.Value ?? "";
            return (a.Value ?? "") + (b.Value ?? "");
        }


        public static string operator +(TextVariable a, string b)
        {
            if (!a ) return b ?? "";
            return (a.Value ?? "") + (b ?? "");
        }

        public static string operator +(string a, TextVariable b)
        {
            if (!b) return a ?? "";
            return (a ?? "") + (b.Value ?? "");
        }

        public override bool Equals(object obj)
        {
            if (obj is TextVariable other) return this == other;
            if (obj is string stringValue) return this == stringValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
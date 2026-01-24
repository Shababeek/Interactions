using System;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores an enum value as an integer with enum manipulation methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/EnumVariable")]
    public class EnumVariable : ScriptableVariable<int>
    {
        [Tooltip("The name of the enum type this variable represents.")]
        [SerializeField] private string enumTypeName;
        
        [Tooltip("Array of enum value names for this enum type.")]
        [SerializeField] private string[] enumNames;

        /// <summary>
        /// Sets the enum value using a typed enum.
        /// </summary>
        public void SetEnumValue<T>(T enumValue) where T : Enum
        {
            Value = Convert.ToInt32(enumValue);
        }

        /// <summary>
        /// Gets the enum value as a typed enum.
        /// </summary>
        public T GetEnumValue<T>() where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), Value);
        }

        /// <summary>
        /// Sets the enum value using an enum name string.
        /// </summary>
        public void SetEnumValue(string enumName)
        {
            if (enumNames != null)
            {
                for (int i = 0; i < enumNames.Length; i++)
                {
                    if (enumNames[i] == enumName)
                    {
                        Value = i;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the current enum value.
        /// </summary>
        public string GetEnumName()
        {
            if (enumNames != null && Value >= 0 && Value < enumNames.Length)
            {
                return enumNames[Value];
            }

            return "Unknown";
        }

        /// <summary>
        /// Gets all enum value names for this enum type.
        /// </summary>
        public string[] GetEnumNames()
        {
            return enumNames;
        }

        /// <summary>
        /// Initializes the enum variable with a specific enum type.
        /// </summary>
        public void InitializeEnum<T>() where T : Enum
        {
            enumTypeName = typeof(T).Name;
            enumNames = Enum.GetNames(typeof(T));
        }

        /// <summary>
        /// Gets the name of the enum type.
        /// </summary>
        public string EnumTypeName => enumTypeName;
        
        /// <summary>
        /// Gets the number of enum values.
        /// </summary>
        public int EnumCount => enumNames?.Length ?? 0;

        // Equality operators
        public static bool operator ==(EnumVariable a, EnumVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(EnumVariable a, EnumVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(EnumVariable a, int b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(EnumVariable a, int b)
        {
            return !(a == b);
        }

        public static bool operator ==(int a, EnumVariable b)
        {
            return b == a;
        }

        public static bool operator !=(int a, EnumVariable b)
        {
            return !(b == a);
        }

        // Arithmetic operators
        public static int operator +(EnumVariable a, EnumVariable b)
        {
            if (a == null || b == null) return 0;
            return a.Value + b.Value;
        }

        public static int operator +(EnumVariable a, int b)
        {
            if (a == null) return b;
            return a.Value + b;
        }

        public static int operator +(int a, EnumVariable b)
        {
            if (b == null) return a;
            return a + b.Value;
        }

        public static int operator -(EnumVariable a, EnumVariable b)
        {
            if (a == null || b == null) return 0;
            return a.Value - b.Value;
        }

        public static int operator -(EnumVariable a, int b)
        {
            if (a == null) return -b;
            return a.Value - b;
        }

        public static int operator -(int a, EnumVariable b)
        {
            if (b == null) return a;
            return a - b.Value;
        }

        public static int operator *(EnumVariable a, EnumVariable b)
        {
            if (a == null || b == null) return 0;
            return a.Value * b.Value;
        }

        public static int operator *(EnumVariable a, int b)
        {
            if (a == null) return 0;
            return a.Value * b;
        }

        public static int operator *(int a, EnumVariable b)
        {
            if (b == null) return 0;
            return a * b.Value;
        }

        // Use reference equality for Equals (standard object behavior)
        // Use == operator for value comparison in code
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}
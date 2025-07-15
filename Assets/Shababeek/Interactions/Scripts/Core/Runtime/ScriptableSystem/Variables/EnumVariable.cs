using System;
using UnityEngine;

namespace Shababeek.Core
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/EnumVariable")]
    public class EnumVariable : ScriptableVariable<int>
    {
        [SerializeField] private string enumTypeName;
        [SerializeField] private string[] enumNames;
        
        public void SetEnumValue<T>(T enumValue) where T : Enum
        {
            Value = Convert.ToInt32(enumValue);
        }
        
        public T GetEnumValue<T>() where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), Value);
        }
        
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
        
        public string GetEnumName()
        {
            if (enumNames != null && Value >= 0 && Value < enumNames.Length)
            {
                return enumNames[Value];
            }
            return "Unknown";
        }
        
        public string[] GetEnumNames()
        {
            return enumNames;
        }
        
        public void InitializeEnum<T>() where T : Enum
        {
            enumTypeName = typeof(T).Name;
            enumNames = Enum.GetNames(typeof(T));
        }
        
        public string EnumTypeName => enumTypeName;
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

        public override bool Equals(object obj)
        {
            if (obj is EnumVariable other) return this == other;
            if (obj is int intValue) return this == intValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
} 
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a Color value with color manipulation methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/ColorVariable")]
    public class ColorVariable : ScriptableVariable<Color>
    {
        /// <summary>
        /// Sets the RGB components while preserving the alpha channel.
        /// </summary>
        public void SetRGB(float r, float g, float b)
        {
            Value = new Color(r, g, b, Value.a);
        }

        /// <summary>
        /// Sets all RGBA components of the color.
        /// </summary>
        public void SetRGBA(float r, float g, float b, float a)
        {
            Value = new Color(r, g, b, a);
        }

        /// <summary>
        /// Sets the alpha channel while preserving RGB components.
        /// </summary>
        public void SetAlpha(float alpha)
        {
            Value = new Color(Value.r, Value.g, Value.b, alpha);
        }

        /// <summary>
        /// Gets the red component of the color.
        /// </summary>
        public float R => Value.r;
        
        /// <summary>
        /// Gets the green component of the color.
        /// </summary>
        public float G => Value.g;
        
        /// <summary>
        /// Gets the blue component of the color.
        /// </summary>
        public float B => Value.b;
        
        /// <summary>
        /// Gets the alpha component of the color.
        /// </summary>
        public float A => Value.a;

        // Equality operators
        public static bool operator ==(ColorVariable a, ColorVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(ColorVariable a, ColorVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(ColorVariable a, Color b)
        {
            if (ReferenceEquals(a, null)) return false;
            return a.Value == b;
        }

        public static bool operator !=(ColorVariable a, Color b)
        {
            return !(a == b);
        }

        public static bool operator ==(Color a, ColorVariable b)
        {
            return b == a;
        }

        public static bool operator !=(Color a, ColorVariable b)
        {
            return !(b == a);
        }

        // Color arithmetic operators
        public static Color operator +(ColorVariable a, ColorVariable b)
        {
            if (a == null && b == null) return Color.black;
            if (a == null) return b.Value;
            if (b == null) return a.Value;
            return a.Value + b.Value;
        }

        public static Color operator +(ColorVariable a, Color b)
        {
            if (a == null) return b;
            return a.Value + b;
        }

        public static Color operator +(Color a, ColorVariable b)
        {
            if (b == null) return a;
            return a + b.Value;
        }

        public static Color operator *(ColorVariable a, ColorVariable b)
        {
            if (a == null && b == null) return Color.black;
            if (a == null) return b.Value;
            if (b == null) return a.Value;
            return a.Value * b.Value;
        }

        public static Color operator *(ColorVariable a, Color b)
        {
            if (a == null) return b;
            return a.Value * b;
        }

        public static Color operator *(Color a, ColorVariable b)
        {
            if (b == null) return a;
            return a * b.Value;
        }

        public static Color operator *(ColorVariable a, float b)
        {
            if (a == null) return Color.black;
            return a.Value * b;
        }

        public static Color operator *(float a, ColorVariable b)
        {
            if (b == null) return Color.black;
            return a * b.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ColorVariable other) return this == other;
            if (obj is Color colorValue) return this == colorValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores a Gradient with evaluation and manipulation methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/GradientVariable")]
    public class GradientVariable : ScriptableVariable<Gradient>
    {
        private void OnEnable()
        {
            if (Value != null) return;
            Value = new Gradient();
            // Set default gradient (white to black)
            var colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.white, 0f);
            colorKeys[1] = new GradientColorKey(Color.black, 1f);

            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            Value.SetKeys(colorKeys, alphaKeys);
        }

        /// <summary>
        /// Evaluates the gradient at the specified time.
        /// </summary>
        public Color Evaluate(float time)
        {
            return Value?.Evaluate(time) ?? Color.white;
        }

        /// <summary>
        /// Evaluates the gradient using a normalized time value (0-1).
        /// </summary>
        public Color EvaluateNormalized(float normalizedTime)
        {
            return Value?.Evaluate(Mathf.Clamp01(normalizedTime)) ?? Color.white;
        }

        /// <summary>
        /// Sets the color keys of the gradient.
        /// </summary>
        public void SetColorKeys(params GradientColorKey[] colorKeys)
        {
            if (Value == null) Value = new Gradient();
            var alphaKeys = Value.alphaKeys;
            Value.SetKeys(colorKeys, alphaKeys);
        }

        /// <summary>
        /// Sets the alpha keys of the gradient.
        /// </summary>
        public void SetAlphaKeys(params GradientAlphaKey[] alphaKeys)
        {
            if (Value == null) Value = new Gradient();
            var colorKeys = Value.colorKeys;
            Value.SetKeys(colorKeys, alphaKeys);
        }

        /// <summary>
        /// Sets both color and alpha keys of the gradient.
        /// </summary>
        public void SetKeys(GradientColorKey[] colorKeys, GradientAlphaKey[] alphaKeys)
        {
            if (Value == null) Value = new Gradient();
            Value.SetKeys(colorKeys, alphaKeys);
        }

        /// <summary>
        /// Creates a simple two-color gradient.
        /// </summary>
        public void SetSimpleGradient(Color startColor, Color endColor)
        {
            if (Value == null) Value = new Gradient();

            var colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(startColor, 0f);
            colorKeys[1] = new GradientColorKey(endColor, 1f);

            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(startColor.a, 0f);
            alphaKeys[1] = new GradientAlphaKey(endColor.a, 1f);

            Value.SetKeys(colorKeys, alphaKeys);
        }

        /// <summary>
        /// Sets the gradient to a solid color.
        /// </summary>
        public void SetSolidColor(Color color)
        {
            SetSimpleGradient(color, color);
        }

        /// <summary>
        /// Sets the gradient to a rainbow spectrum.
        /// </summary>
        public void SetRainbow()
        {
            if (Value == null) Value = new Gradient();

            var colorKeys = new GradientColorKey[7];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);
            colorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0f), 1f / 6f); // Orange
            colorKeys[2] = new GradientColorKey(Color.yellow, 2f / 6f);
            colorKeys[3] = new GradientColorKey(Color.green, 3f / 6f);
            colorKeys[4] = new GradientColorKey(Color.blue, 4f / 6f);
            colorKeys[5] = new GradientColorKey(new Color(0.3f, 0f, 0.5f), 5f / 6f); // Indigo
            colorKeys[6] = new GradientColorKey(new Color(0.5f, 0f, 1f), 1f); // Violet

            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            Value.SetKeys(colorKeys, alphaKeys);
        }

        /// <summary>
        /// Sets the gradient from black to white.
        /// </summary>
        public void SetBlackToWhite()
        {
            SetSimpleGradient(Color.black, Color.white);
        }

        /// <summary>
        /// Sets the gradient from white to black.
        /// </summary>
        public void SetWhiteToBlack()
        {
            SetSimpleGradient(Color.white, Color.black);
        }

        /// <summary>
        /// Sets the gradient from transparent to opaque for the specified color.
        /// </summary>
        public void SetTransparentToOpaque(Color color)
        {
            Color transparent = color;
            transparent.a = 0f;
            Color opaque = color;
            opaque.a = 1f;
            SetSimpleGradient(transparent, opaque);
        }

        /// <summary>
        /// Gets or sets the blend mode of the gradient.
        /// </summary>
        public GradientMode Mode
        {
            get => Value?.mode ?? GradientMode.Blend;
            set
            {
                if (Value != null)
                    Value.mode = value;
            }
        }

        /// <summary>
        /// Gets the color keys of the gradient.
        /// </summary>
        public GradientColorKey[] ColorKeys => Value?.colorKeys ?? new GradientColorKey[0];
        
        /// <summary>
        /// Gets the alpha keys of the gradient.
        /// </summary>
        public GradientAlphaKey[] AlphaKeys => Value?.alphaKeys ?? new GradientAlphaKey[0];
        
        /// <summary>
        /// Gets whether the gradient is valid (not null).
        /// </summary>
        public bool IsValid => Value != null;

        // Equality operators
        public static bool operator ==(GradientVariable a, GradientVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return ReferenceEquals(a.Value, b.Value);
        }

        public static bool operator !=(GradientVariable a, GradientVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(GradientVariable a, Gradient b)
        {
            if (ReferenceEquals(a, null)) return b == null;
            return ReferenceEquals(a.Value, b);
        }

        public static bool operator !=(GradientVariable a, Gradient b)
        {
            return !(a == b);
        }

        public static bool operator ==(Gradient a, GradientVariable b)
        {
            return b == a;
        }

        public static bool operator !=(Gradient a, GradientVariable b)
        {
            return !(b == a);
        }

        public override bool Equals(object obj)
        {
            if (obj is GradientVariable other) return this == other;
            if (obj is Gradient gradientValue) return this == gradientValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }

    /// <summary>
    /// A reference that can point to either a GradientVariable or use a constant Gradient value.
    /// </summary>
    [System.Serializable]
    public class GradientReference : VariableReference<Gradient>
    {
    }
}
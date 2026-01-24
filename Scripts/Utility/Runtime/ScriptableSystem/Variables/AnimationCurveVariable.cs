using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Scriptable variable that stores an AnimationCurve with evaluation and manipulation methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/AnimationCurveVariable")]
    public class AnimationCurveVariable : ScriptableVariable<AnimationCurve>
    {
        [Tooltip("When enabled, the curve will loop by wrapping time values within the curve length.")]
        [SerializeField] private bool _loop = false;
        
        [Tooltip("Wrap mode for values before the start of the curve.")]
        [SerializeField] private WrapMode _preWrapMode = WrapMode.Clamp;
        
        [Tooltip("Wrap mode for values after the end of the curve.")]
        [SerializeField] private WrapMode _postWrapMode = WrapMode.Clamp;

        private void OnEnable()
        {
            if (Value == null)
            {
                Value = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            }

            UpdateWrapModes();
        }

        /// <summary>
        /// Evaluates the curve at the specified time.
        /// </summary>
        public float Evaluate(float time)
        {
            if (Value == null) return 0f;

            if (_loop && Length > 0f)
            {
                time = Mathf.Repeat(time, Length);
            }

            return Value.Evaluate(time);
        }

        /// <summary>
        /// Evaluates the curve using a normalized time value (0-1).
        /// </summary>
        public float EvaluateNormalized(float normalizedTime)
        {
            if (Value == null || Length <= 0f) return 0f;
            return Value.Evaluate(normalizedTime * Length);
        }

        /// <summary>
        /// Adds a new keyframe to the curve.
        /// </summary>
        public void AddKey(float time, float value)
        {
            if (Value == null) Value = new AnimationCurve();
            Value.AddKey(time, value);
            UpdateWrapModes();
        }

        /// <summary>
        /// Adds a keyframe to the curve.
        /// </summary>
        public void AddKey(Keyframe keyframe)
        {
            if (Value == null) Value = new AnimationCurve();
            Value.AddKey(keyframe);
            UpdateWrapModes();
        }

        /// <summary>
        /// Removes the keyframe at the specified index.
        /// </summary>
        public void RemoveKey(int index)
        {
            if (Value != null && index >= 0 && index < Value.length)
            {
                Value.RemoveKey(index);
                UpdateWrapModes();
            }
        }

        /// <summary>
        /// Smooths the tangents of the keyframe at the specified index.
        /// </summary>
        public void SmoothTangents(int index, float weight = 0f)
        {
            if (Value != null && index >= 0 && index < Value.length)
            {
                Value.SmoothTangents(index, weight);
            }
        }

        /// <summary>
        /// Sets the curve to a linear interpolation from 0 to 1.
        /// </summary>
        public void SetLinear()
        {
            Value = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            UpdateWrapModes();
        }

        /// <summary>
        /// Sets the curve to an ease-in-out interpolation from 0 to 1.
        /// </summary>
        public void SetEaseInOut()
        {
            Value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            UpdateWrapModes();
        }

        /// <summary>
        /// Sets the curve to a constant value.
        /// </summary>
        public void SetConstant(float value = 1f)
        {
            Value = new AnimationCurve(new Keyframe(0f, value), new Keyframe(1f, value));
            UpdateWrapModes();
        }

        private void UpdateWrapModes()
        {
            if (Value != null)
            {
                Value.preWrapMode = _preWrapMode;
                Value.postWrapMode = _postWrapMode;
            }
        }

        /// <summary>
        /// Gets or sets whether the curve should loop.
        /// </summary>
        public bool Loop
        {
            get => _loop;
            set => _loop = value;
        }

        /// <summary>
        /// Gets or sets the wrap mode for values before the start of the curve.
        /// </summary>
        public WrapMode PreWrapMode
        {
            get => _preWrapMode;
            set
            {
                _preWrapMode = value;
                UpdateWrapModes();
            }
        }

        /// <summary>
        /// Gets or sets the wrap mode for values after the end of the curve.
        /// </summary>
        public WrapMode PostWrapMode
        {
            get => _postWrapMode;
            set
            {
                _postWrapMode = value;
                UpdateWrapModes();
            }
        }

        /// <summary>
        /// Gets the time length of the curve.
        /// </summary>
        public float Length => Value?.length > 0 ? Value[Value.length - 1].time - Value[0].time : 0f;
        
        /// <summary>
        /// Gets the number of keyframes in the curve.
        /// </summary>
        public int KeyCount => Value?.length ?? 0;
        
        /// <summary>
        /// Gets whether the curve is valid (not null and has keyframes).
        /// </summary>
        public bool IsValid => Value != null && Value.length > 0;

        /// <summary>
        /// Gets the keyframe at the specified index.
        /// </summary>
        public Keyframe GetKey(int index)
        {
            if (Value != null && index >= 0 && index < Value.length)
                return Value[index];
            return new Keyframe();
        }

        /// <summary>
        /// Sets the keyframe at the specified index.
        /// </summary>
        public void SetKey(int index, Keyframe keyframe)
        {
            if (Value != null && index >= 0 && index < Value.length)
            {
                var keys = Value.keys;
                keys[index] = keyframe;
                Value.keys = keys;
                UpdateWrapModes();
            }
        }

        // Equality operators
        public static bool operator ==(AnimationCurveVariable a, AnimationCurveVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return ReferenceEquals(a.Value, b.Value);
        }

        public static bool operator !=(AnimationCurveVariable a, AnimationCurveVariable b)
        {
            return !(a == b);
        }

        public static bool operator ==(AnimationCurveVariable a, AnimationCurve b)
        {
            if (ReferenceEquals(a, null)) return b == null;
            return ReferenceEquals(a.Value, b);
        }

        public static bool operator !=(AnimationCurveVariable a, AnimationCurve b)
        {
            return !(a == b);
        }

        public static bool operator ==(AnimationCurve a, AnimationCurveVariable b)
        {
            return b == a;
        }

        public static bool operator !=(AnimationCurve a, AnimationCurveVariable b)
        {
            return !(b == a);
        }

        // Use reference equality for Equals (standard object behavior)
        // Use == operator for value comparison in code
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// A reference that can point to either an AnimationCurveVariable or use a constant AnimationCurve value.
    /// </summary>
    [System.Serializable]
    public class AnimationCurveReference : VariableReference<AnimationCurve>
    {
    }
}
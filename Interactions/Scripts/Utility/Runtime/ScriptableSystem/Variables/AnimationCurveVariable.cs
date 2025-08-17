using UnityEngine;

namespace Shababeek.Utilities
{
    [CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/AnimationCurveVariable")]
    public class AnimationCurveVariable : ScriptableVariable<AnimationCurve>
    {
        [SerializeField] private bool _loop = false;
        [SerializeField] private WrapMode _preWrapMode = WrapMode.Clamp;
        [SerializeField] private WrapMode _postWrapMode = WrapMode.Clamp;

        private void OnEnable()
        {
            if (Value == null)
            {
                Value = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            }

            UpdateWrapModes();
        }

        public float Evaluate(float time)
        {
            if (Value == null) return 0f;

            if (_loop && Length > 0f)
            {
                time = Mathf.Repeat(time, Length);
            }

            return Value.Evaluate(time);
        }

        public float EvaluateNormalized(float normalizedTime)
        {
            if (Value == null || Length <= 0f) return 0f;
            return Value.Evaluate(normalizedTime * Length);
        }

        public void AddKey(float time, float value)
        {
            if (Value == null) Value = new AnimationCurve();
            Value.AddKey(time, value);
            UpdateWrapModes();
        }

        public void AddKey(Keyframe keyframe)
        {
            if (Value == null) Value = new AnimationCurve();
            Value.AddKey(keyframe);
            UpdateWrapModes();
        }

        public void RemoveKey(int index)
        {
            if (Value != null && index >= 0 && index < Value.length)
            {
                Value.RemoveKey(index);
                UpdateWrapModes();
            }
        }

        public void SmoothTangents(int index, float weight = 0f)
        {
            if (Value != null && index >= 0 && index < Value.length)
            {
                Value.SmoothTangents(index, weight);
            }
        }

        public void SetLinear()
        {
            Value = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            UpdateWrapModes();
        }

        public void SetEaseInOut()
        {
            Value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            UpdateWrapModes();
        }

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

        public bool Loop
        {
            get => _loop;
            set => _loop = value;
        }

        public WrapMode PreWrapMode
        {
            get => _preWrapMode;
            set
            {
                _preWrapMode = value;
                UpdateWrapModes();
            }
        }

        public WrapMode PostWrapMode
        {
            get => _postWrapMode;
            set
            {
                _postWrapMode = value;
                UpdateWrapModes();
            }
        }

        public float Length => Value?.length > 0 ? Value[Value.length - 1].time - Value[0].time : 0f;
        public int KeyCount => Value?.length ?? 0;
        public bool IsValid => Value != null && Value.length > 0;

        public Keyframe GetKey(int index)
        {
            if (Value != null && index >= 0 && index < Value.length)
                return Value[index];
            return new Keyframe();
        }

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

        public override bool Equals(object obj)
        {
            if (obj is AnimationCurveVariable other) return this == other;
            if (obj is AnimationCurve curveValue) return this == curveValue;
            return false;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }

    /// <summary>
    /// A reference that can point to either an AnimationCurveVariable or use a constant AnimationCurve value.
    /// </summary>
    [System.Serializable]
    public class AnimationCurveReference : VariableReference<AnimationCurve>
    {
    }
}
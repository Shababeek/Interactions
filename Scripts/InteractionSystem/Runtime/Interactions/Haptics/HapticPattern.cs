using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>The shape of a haptic pattern: amplitude curve, duration, and strength.</summary>
    [System.Serializable]
    public struct HapticPatternData
    {
        [Tooltip("Vibration amplitude over normalized time (both axes 0-1).")]
        public AnimationCurve amplitude;

        [Tooltip("Total pattern duration in seconds.")]
        [Min(0.01f)] public float duration;

        [Tooltip("Overall strength multiplier applied to the curve (0-1).")]
        [Range(0f, 1f)] public float strength;
    }

    /// <summary>
    /// Curve-based haptic pattern asset. Defines vibration amplitude over a duration so the same
    /// tuned feel (click, detent, thud...) can be shared across interactables instead of
    /// scattering amplitude/duration pairs. As a ScriptableVariable it can also be observed,
    /// raised as a game event, and driven/bound like any other variable in the scriptable system.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Haptic Pattern", fileName = "HapticPattern")]
    public class HapticPattern : ScriptableVariable<HapticPatternData>
    {
        /// <summary>Total pattern duration in seconds.</summary>
        public float Duration => Mathf.Max(0.01f, value.duration);

        /// <summary>Amplitude (0-1) at the given normalized time (0-1).</summary>
        public float Evaluate(float normalizedTime)
        {
            var curve = value.amplitude;
            float sample = curve == null || curve.length == 0
                ? 1f
                : curve.Evaluate(Mathf.Clamp01(normalizedTime));
            return Mathf.Clamp01(sample * value.strength);
        }

        /// <summary>Overwrites the pattern's shape and notifies observers. Used by editor presets and tests.</summary>
        public void SetShape(AnimationCurve newAmplitude, float newDuration, float newStrength = 1f)
        {
            Value = new HapticPatternData
            {
                amplitude = newAmplitude,
                duration = Mathf.Max(0.01f, newDuration),
                strength = Mathf.Clamp01(newStrength)
            };
        }

        private void Reset()
        {
            SetValueWithoutNotify(new HapticPatternData
            {
                amplitude = AnimationCurve.Constant(0f, 1f, 0.5f),
                duration = 0.1f,
                strength = 1f
            });
        }
    }
}

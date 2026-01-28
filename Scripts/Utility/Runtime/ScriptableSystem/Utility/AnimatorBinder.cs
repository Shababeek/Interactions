using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Comprehensive Animator binder that binds ScriptableVariables to Animator parameters
    /// and GameEvents to Animator triggers.
    /// </summary>
    /// <remarks>
    /// Supports:
    /// - BoolVariable → Animator Bool
    /// - IntVariable/FloatVariable → Animator Int/Float
    /// - GameEvent → Animator Trigger
    ///
    /// This provides a more direct variable-to-parameter binding compared to EventAnimatorBinder.
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Binders/Animator Binder")]
    public class AnimatorBinder : MonoBehaviour
    {
        [Tooltip("The Animator component. Uses this object's Animator if not set.")]
        [SerializeField] private Animator animator;

        [Header("Bool Bindings")]
        [Tooltip("Bind BoolVariables directly to Animator bool parameters.")]
        [SerializeField] private BoolParameterBinding[] boolBindings;

        [Header("Float Bindings")]
        [Tooltip("Bind numeric variables to Animator float parameters.")]
        [SerializeField] private FloatParameterBinding[] floatBindings;

        [Header("Int Bindings")]
        [Tooltip("Bind numeric variables to Animator int parameters.")]
        [SerializeField] private IntParameterBinding[] intBindings;

        [Header("Trigger Bindings")]
        [Tooltip("Bind GameEvents to Animator triggers.")]
        [SerializeField] private TriggerParameterBinding[] triggerBindings;

        [Header("Options")]
        [Tooltip("Update parameters every frame (for smooth blending) vs only on change.")]
        [SerializeField] private bool continuousUpdate = false;

        private CompositeDisposable _disposable;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (animator == null)
            {
                Debug.LogWarning($"No Animator found on {gameObject.name}", this);
                return;
            }

            InitializeBoolBindings();
            InitializeFloatBindings();
            InitializeIntBindings();
            InitializeTriggerBindings();
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (!continuousUpdate || animator == null) return;

            // Continuous update for smooth parameter blending
            UpdateFloatParameters();
            UpdateIntParameters();
        }

        #region Bool Bindings

        private void InitializeBoolBindings()
        {
            if (boolBindings == null) return;

            foreach (var binding in boolBindings)
            {
                if (binding.variable == null || string.IsNullOrEmpty(binding.parameterName)) continue;

                var paramId = Animator.StringToHash(binding.parameterName);

                // Set initial value
                bool value = binding.invert ? !binding.variable.Value : binding.variable.Value;
                animator.SetBool(paramId, value);

                // Subscribe to changes
                binding.variable.OnRaised
                    .Subscribe(_ =>
                    {
                        bool v = binding.invert ? !binding.variable.Value : binding.variable.Value;
                        animator.SetBool(paramId, v);
                    })
                    .AddTo(_disposable);
            }
        }

        #endregion

        #region Float Bindings

        private void InitializeFloatBindings()
        {
            if (floatBindings == null) return;

            foreach (var binding in floatBindings)
            {
                if (binding.variable == null || string.IsNullOrEmpty(binding.parameterName)) continue;

                var numVar = binding.variable as INumericalVariable;
                if (numVar == null)
                {
                    Debug.LogWarning($"Variable {binding.variable.name} is not numeric", this);
                    continue;
                }

                var paramId = Animator.StringToHash(binding.parameterName);

                // Set initial value
                float value = CalculateFloatValue(binding, numVar.AsFloat);
                animator.SetFloat(paramId, value);

                // Subscribe to changes (unless continuous update is enabled)
                if (!continuousUpdate)
                {
                    binding.variable.OnRaised
                        .Subscribe(_ =>
                        {
                            float v = CalculateFloatValue(binding, numVar.AsFloat);
                            if (binding.useDamping)
                            {
                                animator.SetFloat(paramId, v, binding.dampTime, Time.deltaTime);
                            }
                            else
                            {
                                animator.SetFloat(paramId, v);
                            }
                        })
                        .AddTo(_disposable);
                }
            }
        }

        private void UpdateFloatParameters()
        {
            if (floatBindings == null) return;

            foreach (var binding in floatBindings)
            {
                if (binding.variable == null) continue;

                var numVar = binding.variable as INumericalVariable;
                if (numVar == null) continue;

                var paramId = Animator.StringToHash(binding.parameterName);
                float value = CalculateFloatValue(binding, numVar.AsFloat);

                if (binding.useDamping)
                {
                    animator.SetFloat(paramId, value, binding.dampTime, Time.deltaTime);
                }
                else
                {
                    animator.SetFloat(paramId, value);
                }
            }
        }

        private float CalculateFloatValue(FloatParameterBinding binding, float rawValue)
        {
            float value = rawValue;

            // Apply remapping if enabled
            if (binding.useRemapping)
            {
                float t = Mathf.InverseLerp(binding.inputMin, binding.inputMax, value);
                value = Mathf.Lerp(binding.outputMin, binding.outputMax, t);
            }

            // Apply multiplier
            value *= binding.multiplier;

            // Clamp if enabled
            if (binding.clampOutput)
            {
                value = Mathf.Clamp(value, binding.outputMin, binding.outputMax);
            }

            return value;
        }

        #endregion

        #region Int Bindings

        private void InitializeIntBindings()
        {
            if (intBindings == null) return;

            foreach (var binding in intBindings)
            {
                if (binding.variable == null || string.IsNullOrEmpty(binding.parameterName)) continue;

                var numVar = binding.variable as INumericalVariable;
                if (numVar == null)
                {
                    Debug.LogWarning($"Variable {binding.variable.name} is not numeric", this);
                    continue;
                }

                var paramId = Animator.StringToHash(binding.parameterName);

                // Set initial value
                int value = CalculateIntValue(binding, numVar.AsInt);
                animator.SetInteger(paramId, value);

                // Subscribe to changes
                if (!continuousUpdate)
                {
                    binding.variable.OnRaised
                        .Subscribe(_ =>
                        {
                            int v = CalculateIntValue(binding, numVar.AsInt);
                            animator.SetInteger(paramId, v);
                        })
                        .AddTo(_disposable);
                }
            }
        }

        private void UpdateIntParameters()
        {
            if (intBindings == null) return;

            foreach (var binding in intBindings)
            {
                if (binding.variable == null) continue;

                var numVar = binding.variable as INumericalVariable;
                if (numVar == null) continue;

                var paramId = Animator.StringToHash(binding.parameterName);
                int value = CalculateIntValue(binding, numVar.AsInt);
                animator.SetInteger(paramId, value);
            }
        }

        private int CalculateIntValue(IntParameterBinding binding, int rawValue)
        {
            int value = rawValue + binding.offset;

            if (binding.clampOutput)
            {
                value = Mathf.Clamp(value, binding.minValue, binding.maxValue);
            }

            return value;
        }

        #endregion

        #region Trigger Bindings

        private void InitializeTriggerBindings()
        {
            if (triggerBindings == null) return;

            foreach (var binding in triggerBindings)
            {
                if (binding.gameEvent == null || string.IsNullOrEmpty(binding.parameterName)) continue;

                var paramId = Animator.StringToHash(binding.parameterName);

                binding.gameEvent.OnRaised
                    .Subscribe(_ =>
                    {
                        if (binding.resetFirst)
                        {
                            animator.ResetTrigger(paramId);
                        }
                        animator.SetTrigger(paramId);
                    })
                    .AddTo(_disposable);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually trigger a parameter by name.
        /// </summary>
        public void Trigger(string parameterName)
        {
            if (animator != null)
                animator.SetTrigger(parameterName);
        }

        /// <summary>
        /// Manually set a bool parameter.
        /// </summary>
        public void SetBool(string parameterName, bool value)
        {
            if (animator != null)
                animator.SetBool(parameterName, value);
        }

        /// <summary>
        /// Manually set a float parameter.
        /// </summary>
        public void SetFloat(string parameterName, float value)
        {
            if (animator != null)
                animator.SetFloat(parameterName, value);
        }

        /// <summary>
        /// Manually set an int parameter.
        /// </summary>
        public void SetInt(string parameterName, int value)
        {
            if (animator != null)
                animator.SetInteger(parameterName, value);
        }

        /// <summary>
        /// Force sync all bindings to current variable values.
        /// </summary>
        [ContextMenu("Sync All Parameters")]
        public void SyncAllParameters()
        {
            if (animator == null) return;

            // Sync bools
            if (boolBindings != null)
            {
                foreach (var binding in boolBindings)
                {
                    if (binding.variable == null) continue;
                    bool value = binding.invert ? !binding.variable.Value : binding.variable.Value;
                    animator.SetBool(binding.parameterName, value);
                }
            }

            // Sync floats
            if (floatBindings != null)
            {
                foreach (var binding in floatBindings)
                {
                    if (binding.variable == null) continue;
                    var numVar = binding.variable as INumericalVariable;
                    if (numVar == null) continue;
                    float value = CalculateFloatValue(binding, numVar.AsFloat);
                    animator.SetFloat(binding.parameterName, value);
                }
            }

            // Sync ints
            if (intBindings != null)
            {
                foreach (var binding in intBindings)
                {
                    if (binding.variable == null) continue;
                    var numVar = binding.variable as INumericalVariable;
                    if (numVar == null) continue;
                    int value = CalculateIntValue(binding, numVar.AsInt);
                    animator.SetInteger(binding.parameterName, value);
                }
            }
        }

        #endregion

        #region Binding Classes

        [Serializable]
        public class BoolParameterBinding
        {
            [Tooltip("BoolVariable to bind.")]
            public BoolVariable variable;

            [Tooltip("Name of the Animator bool parameter.")]
            public string parameterName;

            [Tooltip("Invert the bool value.")]
            public bool invert = false;
        }

        [Serializable]
        public class FloatParameterBinding
        {
            [Tooltip("Numeric variable to bind (IntVariable or FloatVariable).")]
            public ScriptableVariable variable;

            [Tooltip("Name of the Animator float parameter.")]
            public string parameterName;

            [Tooltip("Multiplier applied to the value.")]
            public float multiplier = 1f;

            [Header("Remapping")]
            [Tooltip("Remap input range to output range.")]
            public bool useRemapping = false;

            [Tooltip("Input value minimum.")]
            public float inputMin = 0f;

            [Tooltip("Input value maximum.")]
            public float inputMax = 1f;

            [Tooltip("Output value minimum.")]
            public float outputMin = 0f;

            [Tooltip("Output value maximum.")]
            public float outputMax = 1f;

            [Header("Options")]
            [Tooltip("Clamp output to min/max range.")]
            public bool clampOutput = false;

            [Tooltip("Use Animator.SetFloat with damping for smooth transitions.")]
            public bool useDamping = false;

            [Tooltip("Damping time for smooth transitions.")]
            public float dampTime = 0.1f;
        }

        [Serializable]
        public class IntParameterBinding
        {
            [Tooltip("Numeric variable to bind (IntVariable or FloatVariable).")]
            public ScriptableVariable variable;

            [Tooltip("Name of the Animator integer parameter.")]
            public string parameterName;

            [Tooltip("Offset added to the value.")]
            public int offset = 0;

            [Tooltip("Clamp output to min/max range.")]
            public bool clampOutput = false;

            [Tooltip("Minimum output value.")]
            public int minValue = 0;

            [Tooltip("Maximum output value.")]
            public int maxValue = 100;
        }

        [Serializable]
        public class TriggerParameterBinding
        {
            [Tooltip("GameEvent that triggers the animation.")]
            public GameEvent gameEvent;

            [Tooltip("Name of the Animator trigger parameter.")]
            public string parameterName;

            [Tooltip("Reset the trigger before setting it (prevents queuing).")]
            public bool resetFirst = false;
        }

        #endregion
    }
}

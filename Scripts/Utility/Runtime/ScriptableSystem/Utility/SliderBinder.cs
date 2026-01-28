using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds a numeric variable (Int or Float) to a UI Slider.
    /// Supports bidirectional binding - variable changes update slider, slider changes update variable.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Binders/Slider Binder")]
    [RequireComponent(typeof(Slider))]
    public class SliderBinder : MonoBehaviour
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Binding Mode")]
        [Tooltip("How to bind the variable to the slider.")]
        [SerializeField] private BindingMode bindingMode = BindingMode.TwoWay;

        [Header("Value Mapping")]
        [Tooltip("Map variable value to slider range. If false, uses variable value directly.")]
        [SerializeField] private bool useValueMapping = false;

        [Tooltip("Minimum variable value (maps to slider min).")]
        [SerializeField] private float minVariableValue = 0f;

        [Tooltip("Maximum variable value (maps to slider max).")]
        [SerializeField] private float maxVariableValue = 100f;

        [Header("Options")]
        [Tooltip("Round values to whole numbers (useful for IntVariable).")]
        [SerializeField] private bool roundToInt = false;

        private Slider _slider;
        private CompositeDisposable _disposable;
        private INumericalVariable _numericalVariable;
        private bool _isUpdating;

        public enum BindingMode
        {
            /// <summary>Variable changes update slider only</summary>
            OneWayToSlider,
            /// <summary>Slider changes update variable only</summary>
            OneWayToVariable,
            /// <summary>Both directions sync</summary>
            TwoWay
        }

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (variable == null)
            {
                Debug.LogWarning($"Variable is not assigned on {gameObject.name}", this);
                return;
            }

            _numericalVariable = variable as INumericalVariable;
            if (_numericalVariable == null)
            {
                Debug.LogWarning($"Variable on {gameObject.name} is not a numerical variable", this);
                return;
            }

            // Subscribe to variable changes (for OneWayToSlider and TwoWay)
            if (bindingMode != BindingMode.OneWayToVariable)
            {
                // Initial sync
                UpdateSliderFromVariable();

                variable.OnRaised
                    .Subscribe(_ => UpdateSliderFromVariable())
                    .AddTo(_disposable);
            }

            // Subscribe to slider changes (for OneWayToVariable and TwoWay)
            if (bindingMode != BindingMode.OneWayToSlider)
            {
                _slider.onValueChanged.AsObservable()
                    .Subscribe(OnSliderValueChanged)
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void UpdateSliderFromVariable()
        {
            if (_isUpdating) return;
            _isUpdating = true;

            float variableValue = _numericalVariable.AsFloat;
            float sliderValue;

            if (useValueMapping)
            {
                // Map variable range to slider range (0-1 for normalized, or slider min/max)
                float t = Mathf.InverseLerp(minVariableValue, maxVariableValue, variableValue);
                sliderValue = Mathf.Lerp(_slider.minValue, _slider.maxValue, t);
            }
            else
            {
                sliderValue = variableValue;
            }

            _slider.value = sliderValue;
            _isUpdating = false;
        }

        private void OnSliderValueChanged(float sliderValue)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            float variableValue;

            if (useValueMapping)
            {
                // Map slider range to variable range
                float t = Mathf.InverseLerp(_slider.minValue, _slider.maxValue, sliderValue);
                variableValue = Mathf.Lerp(minVariableValue, maxVariableValue, t);
            }
            else
            {
                variableValue = sliderValue;
            }

            if (roundToInt)
            {
                variableValue = Mathf.Round(variableValue);
            }

            // Set the variable value
            if (variable is IntVariable intVar)
            {
                intVar.Value = Mathf.RoundToInt(variableValue);
            }
            else if (variable is FloatVariable floatVar)
            {
                floatVar.Value = variableValue;
            }

            _isUpdating = false;
        }

        /// <summary>
        /// Manually sync the slider to the current variable value.
        /// </summary>
        public void SyncSliderToVariable()
        {
            if (_numericalVariable != null)
            {
                UpdateSliderFromVariable();
            }
        }

        /// <summary>
        /// Manually sync the variable to the current slider value.
        /// </summary>
        public void SyncVariableToSlider()
        {
            if (_slider != null && _numericalVariable != null)
            {
                OnSliderValueChanged(_slider.value);
            }
        }

        /// <summary>
        /// Sets up the slider min/max from the variable value mapping.
        /// </summary>
        [ContextMenu("Setup Slider Range From Mapping")]
        public void SetupSliderRangeFromMapping()
        {
            if (_slider == null) _slider = GetComponent<Slider>();
            _slider.minValue = minVariableValue;
            _slider.maxValue = maxVariableValue;
            useValueMapping = false; // Direct mapping now
        }
    }
}

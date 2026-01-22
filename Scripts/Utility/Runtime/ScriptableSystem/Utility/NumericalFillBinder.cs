using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds any numeric variable (IntVariable or FloatVariable) to a UI Image's fill amount.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Numerical Fill Binder")]
    [RequireComponent(typeof(Image))]
    public class NumericalFillBinder : MonoBehaviour
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Fill Settings")]
        [Tooltip("The minimum value from the variable that maps to 0 fill.")]
        [SerializeField] private float minValue = 0f;

        [Tooltip("The maximum value from the variable that maps to 1 (full) fill.")]
        [SerializeField] private float maxValue = 100f;

        [Tooltip("Whether to invert the fill (max value = 0 fill, min value = full fill).")]
        [SerializeField] private bool invertFill = false;

        [Header("Interpolation")]
        [Tooltip("Whether to smoothly interpolate fill changes.")]
        [SerializeField] private bool smoothFill = false;

        [Tooltip("Fill speed for smooth interpolation (0-1 per second).")]
        [SerializeField] private float fillSpeed = 2f;

        private CompositeDisposable _disposable;
        private Image _image;
        private float _targetFillAmount;
        private float _currentFillAmount;
        private INumericalVariable _numericalVariable;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (variable == null)
            {
                Debug.LogWarning($"Variable is not assigned on {gameObject.name}", this);
                return;
            }

            // Check if it's a numerical variable
            _numericalVariable = variable as INumericalVariable;
            if (_numericalVariable == null)
            {
                Debug.LogWarning($"Variable on {gameObject.name} is not a numerical variable (IntVariable or FloatVariable)", this);
                return;
            }

            // Initialize fill
            _currentFillAmount = _image.fillAmount;
            UpdateFill(_numericalVariable.AsFloat);

            // Subscribe to value changes
            variable.OnRaised
                .Subscribe(_ => UpdateFill(_numericalVariable.AsFloat))
                .AddTo(_disposable);
        }

        private void Update()
        {
            if (!smoothFill) return;

            _currentFillAmount = Mathf.MoveTowards(_currentFillAmount, _targetFillAmount, fillSpeed * Time.deltaTime);
            _image.fillAmount = _currentFillAmount;
        }

        private void UpdateFill(float value)
        {
            // Calculate normalized fill amount
            float normalized;
            if (Mathf.Approximately(maxValue, minValue))
            {
                normalized = 0f;
            }
            else
            {
                normalized = Mathf.Clamp01((value - minValue) / (maxValue - minValue));
            }

            // Invert if needed
            if (invertFill)
            {
                normalized = 1f - normalized;
            }

            _targetFillAmount = normalized;

            if (!smoothFill)
            {
                _image.fillAmount = _targetFillAmount;
                _currentFillAmount = _targetFillAmount;
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        /// <summary>
        /// Sets the fill amount immediately without interpolation.
        /// </summary>
        /// <param name="fillAmount">The fill amount (0-1)</param>
        public void SetFillImmediate(float fillAmount)
        {
            _targetFillAmount = Mathf.Clamp01(fillAmount);
            _currentFillAmount = _targetFillAmount;
            _image.fillAmount = _targetFillAmount;
        }

        /// <summary>
        /// Gets the current fill amount.
        /// </summary>
        public float CurrentFillAmount => _currentFillAmount;

        /// <summary>
        /// Gets the target fill amount.
        /// </summary>
        public float TargetFillAmount => _targetFillAmount;

        /// <summary>
        /// Recalculates the fill based on the current variable value.
        /// </summary>
        public void Refresh()
        {
            if (_numericalVariable != null)
            {
                UpdateFill(_numericalVariable.AsFloat);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure image is set to filled type
            if (_image == null) _image = GetComponent<Image>();
            if (_image != null && _image.type != Image.Type.Filled)
            {
                Debug.LogWarning($"Image on {gameObject.name} is not set to Filled type. NumericalFillBinder requires a Filled image.", this);
            }
        }
#endif
    }
}

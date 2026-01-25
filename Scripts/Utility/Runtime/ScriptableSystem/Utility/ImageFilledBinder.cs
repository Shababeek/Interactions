using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds an IntVariable to an Image's fill amount for live updates.
    /// Perfect for health bars, progress bars, stamina bars, etc.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Image Filled Binder")]
    [RequireComponent(typeof(Image))]
    public class ImageFilledBinder : MonoBehaviour
    {
        [Tooltip("The IntVariable to bind to the image fill.")]
        [SerializeField] private IntVariable intVariable;

        [Header("Fill Settings")]
        [Tooltip("The minimum value from the IntVariable that maps to 0 fill.")]
        [SerializeField] private int minValue = 0;
        [Tooltip("The maximum value from the IntVariable that maps to 1 (full) fill.")]
        [SerializeField] private int maxValue = 100;
        [Tooltip("Invert the fill direction (useful for overlays that hide content as value increases).")]
        [SerializeField] private bool invertFill = false;
        [Tooltip("Whether to smoothly interpolate fill changes.")]
        [SerializeField] private bool smoothFill = false;
        [Tooltip("Fill speed for smooth interpolation (0-1 per second).")]
        [SerializeField] private float fillSpeed = 1f;

        private CompositeDisposable _disposable;
        private Image _image;
        private float _targetFillAmount;
        private float _currentFillAmount;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            _image = GetComponent<Image>();

            if (intVariable == null)
            {
                Debug.LogWarning($"IntVariable is not assigned on {gameObject.name}", this);
                return;
            }

            if (_image == null)
            {
                Debug.LogWarning($"Image component not found on {gameObject.name}", this);
                return;
            }

            // Initialize fill
            UpdateFill(intVariable.Value);

            // Subscribe to value changes
            intVariable.OnValueChanged
                .Subscribe(value => UpdateFill(value))
                .AddTo(_disposable);
        }

        private void Update()
        {
            if (smoothFill)
            {
                _currentFillAmount = Mathf.MoveTowards(_currentFillAmount, _targetFillAmount, fillSpeed * Time.deltaTime);
                _image.fillAmount = _currentFillAmount;
            }
        }

        private void UpdateFill(int value)
        {
            // Clamp value to range
            int clampedValue = Mathf.Clamp(value, minValue, maxValue);

            // Map value to fill amount (0-1)
            float t = maxValue != minValue ? (float)(clampedValue - minValue) / (maxValue - minValue) : 0f;
            _targetFillAmount = Mathf.Clamp01(t);

            // Invert if needed
            if (invertFill)
            {
                _targetFillAmount = 1f - _targetFillAmount;
            }

            if (!smoothFill)
            {
                _image.fillAmount = _targetFillAmount;
                _currentFillAmount = _targetFillAmount;
            }
            else
            {
                // Initialize current fill if not set
                if (_currentFillAmount == 0f && _targetFillAmount != 0f)
                {
                    _currentFillAmount = _image.fillAmount;
                }
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }
    }
}
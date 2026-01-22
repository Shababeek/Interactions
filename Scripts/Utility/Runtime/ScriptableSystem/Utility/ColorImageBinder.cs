using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds a ColorVariable to a UI Image's color for live updates.
    /// </summary>
 
    [AddComponentMenu("Shababeek/Scriptable System/Color Image Binder")]
    [RequireComponent(typeof(Image))]
    public class ColorImageBinder : MonoBehaviour
    {
        [Tooltip("The ColorVariable to bind to the image's color.")]
        [SerializeField] private ColorVariable colorVariable;

        [Header("Transition Settings")]
        [Tooltip("Whether to smoothly interpolate color changes.")]
        [SerializeField] private bool smoothTransition = false;

        [Tooltip("Speed of color interpolation (higher = faster transition).")]
        [SerializeField] private float transitionSpeed = 5f;

        [Header("Alpha Settings")]
        [Tooltip("Whether to also update the alpha channel from the variable.")]
        [SerializeField] private bool includeAlpha = true;

        private CompositeDisposable _disposable;
        private Image _image;
        private Color _targetColor;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (colorVariable == null)
            {
                Debug.LogWarning($"ColorVariable is not assigned on {gameObject.name}", this);
                return;
            }

            // Initialize with current color
            _targetColor = colorVariable.Value;
            ApplyColor(_targetColor);

            // Subscribe to value changes
            colorVariable.OnValueChanged
                .Subscribe(UpdateColor)
                .AddTo(_disposable);
        }

        private void Update()
        {
            if (!smoothTransition) return;

            Color currentColor = _image.color;
            Color newColor = Color.Lerp(currentColor, _targetColor, transitionSpeed * Time.deltaTime);

            // Optionally preserve the original alpha
            if (!includeAlpha)
            {
                newColor.a = currentColor.a;
            }

            _image.color = newColor;
        }

        private void UpdateColor(Color color)
        {
            _targetColor = color;

            if (!smoothTransition)
            {
                ApplyColor(color);
            }
        }

        private void ApplyColor(Color color)
        {
            if (!includeAlpha)
            {
                color.a = _image.color.a;
            }
            _image.color = color;
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        /// <summary>
        /// Sets the color immediately without interpolation.
        /// </summary>
        /// <param name="color">The color to apply</param>
        public void SetColorImmediate(Color color)
        {
            _targetColor = color;
            ApplyColor(color);
        }

        /// <summary>
        /// Gets the current actual color of the image (may differ from target during interpolation).
        /// </summary>
        public Color CurrentColor => _image != null ? _image.color : Color.white;

        /// <summary>
        /// Gets the target color (the color being interpolated towards).
        /// </summary>
        public Color TargetColor => _targetColor;
    }
}

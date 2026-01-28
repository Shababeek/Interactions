using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds numeric and color variables to a Light component's properties.
    /// Control intensity, color, range, spot angle, and more through variables.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Binders/Light Binder")]
    [RequireComponent(typeof(Light))]
    public class LightBinder : MonoBehaviour
    {
        [Header("Intensity Binding")]
        [Tooltip("Numeric variable to control light intensity.")]
        [SerializeField] private ScriptableVariable intensityVariable;

        [Tooltip("Map variable range to intensity range.")]
        [SerializeField] private bool useIntensityMapping = true;

        [Tooltip("Variable value for minimum intensity.")]
        [SerializeField] private float minIntensityValue = 0f;

        [Tooltip("Variable value for maximum intensity.")]
        [SerializeField] private float maxIntensityValue = 1f;

        [Tooltip("Minimum light intensity output.")]
        [SerializeField] private float minIntensity = 0f;

        [Tooltip("Maximum light intensity output.")]
        [SerializeField] private float maxIntensity = 1f;

        [Header("Color Binding")]
        [Tooltip("Color variable to control light color.")]
        [SerializeField] private ColorVariable colorVariable;

        [Header("Range Binding (Point/Spot)")]
        [Tooltip("Numeric variable to control light range.")]
        [SerializeField] private ScriptableVariable rangeVariable;

        [Tooltip("Map variable range to light range.")]
        [SerializeField] private bool useRangeMapping = true;

        [Tooltip("Variable value for minimum range.")]
        [SerializeField] private float minRangeValue = 0f;

        [Tooltip("Variable value for maximum range.")]
        [SerializeField] private float maxRangeValue = 1f;

        [Tooltip("Minimum light range output.")]
        [SerializeField] private float minRange = 1f;

        [Tooltip("Maximum light range output.")]
        [SerializeField] private float maxRange = 10f;

        [Header("Spot Angle Binding (Spot only)")]
        [Tooltip("Numeric variable to control spot angle.")]
        [SerializeField] private ScriptableVariable spotAngleVariable;

        [Tooltip("Map variable range to spot angle.")]
        [SerializeField] private bool useSpotAngleMapping = true;

        [Tooltip("Variable value for minimum spot angle.")]
        [SerializeField] private float minSpotAngleValue = 0f;

        [Tooltip("Variable value for maximum spot angle.")]
        [SerializeField] private float maxSpotAngleValue = 1f;

        [Tooltip("Minimum spot angle output.")]
        [SerializeField] private float minSpotAngle = 1f;

        [Tooltip("Maximum spot angle output.")]
        [SerializeField] private float maxSpotAngle = 179f;

        [Header("Enabled Binding")]
        [Tooltip("Bool variable to control light enabled state.")]
        [SerializeField] private BoolVariable enabledVariable;

        [Tooltip("Invert the enabled logic.")]
        [SerializeField] private bool invertEnabled = false;

        [Header("Animation")]
        [Tooltip("Smoothly animate property changes.")]
        [SerializeField] private bool smoothChanges = false;

        [Tooltip("Animation speed for smooth changes.")]
        [SerializeField] private float smoothSpeed = 5f;

        private Light _light;
        private CompositeDisposable _disposable;

        // Target values for smooth animation
        private float _targetIntensity;
        private Color _targetColor;
        private float _targetRange;
        private float _targetSpotAngle;

        // Numerical variable references
        private INumericalVariable _intensityNumerical;
        private INumericalVariable _rangeNumerical;
        private INumericalVariable _spotAngleNumerical;

        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            // Store current values as targets
            _targetIntensity = _light.intensity;
            _targetColor = _light.color;
            _targetRange = _light.range;
            _targetSpotAngle = _light.spotAngle;

            // Intensity binding
            if (intensityVariable != null)
            {
                _intensityNumerical = intensityVariable as INumericalVariable;
                if (_intensityNumerical != null)
                {
                    UpdateIntensity(_intensityNumerical.AsFloat);

                    intensityVariable.OnRaised
                        .Subscribe(_ => UpdateIntensity(_intensityNumerical.AsFloat))
                        .AddTo(_disposable);
                }
                else
                {
                    Debug.LogWarning($"Intensity variable on {gameObject.name} is not a numerical variable", this);
                }
            }

            // Color binding
            if (colorVariable != null)
            {
                UpdateColor(colorVariable.Value);

                colorVariable.OnRaised
                    .Subscribe(_ => UpdateColor(colorVariable.Value))
                    .AddTo(_disposable);
            }

            // Range binding
            if (rangeVariable != null)
            {
                _rangeNumerical = rangeVariable as INumericalVariable;
                if (_rangeNumerical != null)
                {
                    UpdateRange(_rangeNumerical.AsFloat);

                    rangeVariable.OnRaised
                        .Subscribe(_ => UpdateRange(_rangeNumerical.AsFloat))
                        .AddTo(_disposable);
                }
                else
                {
                    Debug.LogWarning($"Range variable on {gameObject.name} is not a numerical variable", this);
                }
            }

            // Spot angle binding
            if (spotAngleVariable != null)
            {
                _spotAngleNumerical = spotAngleVariable as INumericalVariable;
                if (_spotAngleNumerical != null)
                {
                    UpdateSpotAngle(_spotAngleNumerical.AsFloat);

                    spotAngleVariable.OnRaised
                        .Subscribe(_ => UpdateSpotAngle(_spotAngleNumerical.AsFloat))
                        .AddTo(_disposable);
                }
                else
                {
                    Debug.LogWarning($"Spot angle variable on {gameObject.name} is not a numerical variable", this);
                }
            }

            // Enabled binding
            if (enabledVariable != null)
            {
                UpdateEnabled(enabledVariable.Value);

                enabledVariable.OnRaised
                    .Subscribe(_ => UpdateEnabled(enabledVariable.Value))
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (!smoothChanges) return;

            float dt = smoothSpeed * Time.deltaTime;

            // Smooth intensity
            if (!Mathf.Approximately(_light.intensity, _targetIntensity))
            {
                _light.intensity = Mathf.Lerp(_light.intensity, _targetIntensity, dt);
            }

            // Smooth color
            if (_light.color != _targetColor)
            {
                _light.color = Color.Lerp(_light.color, _targetColor, dt);
            }

            // Smooth range
            if (!Mathf.Approximately(_light.range, _targetRange))
            {
                _light.range = Mathf.Lerp(_light.range, _targetRange, dt);
            }

            // Smooth spot angle
            if (_light.type == LightType.Spot && !Mathf.Approximately(_light.spotAngle, _targetSpotAngle))
            {
                _light.spotAngle = Mathf.Lerp(_light.spotAngle, _targetSpotAngle, dt);
            }
        }

        private void UpdateIntensity(float value)
        {
            float intensity;

            if (useIntensityMapping)
            {
                float t = Mathf.InverseLerp(minIntensityValue, maxIntensityValue, value);
                intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            }
            else
            {
                intensity = value;
            }

            _targetIntensity = Mathf.Max(0, intensity);

            if (!smoothChanges)
            {
                _light.intensity = _targetIntensity;
            }
        }

        private void UpdateColor(Color color)
        {
            _targetColor = color;

            if (!smoothChanges)
            {
                _light.color = color;
            }
        }

        private void UpdateRange(float value)
        {
            float range;

            if (useRangeMapping)
            {
                float t = Mathf.InverseLerp(minRangeValue, maxRangeValue, value);
                range = Mathf.Lerp(minRange, maxRange, t);
            }
            else
            {
                range = value;
            }

            _targetRange = Mathf.Max(0.01f, range);

            if (!smoothChanges)
            {
                _light.range = _targetRange;
            }
        }

        private void UpdateSpotAngle(float value)
        {
            float angle;

            if (useSpotAngleMapping)
            {
                float t = Mathf.InverseLerp(minSpotAngleValue, maxSpotAngleValue, value);
                angle = Mathf.Lerp(minSpotAngle, maxSpotAngle, t);
            }
            else
            {
                angle = value;
            }

            _targetSpotAngle = Mathf.Clamp(angle, 1f, 179f);

            if (!smoothChanges)
            {
                _light.spotAngle = _targetSpotAngle;
            }
        }

        private void UpdateEnabled(bool value)
        {
            _light.enabled = invertEnabled ? !value : value;
        }

        /// <summary>
        /// Sets the light intensity immediately without animation.
        /// </summary>
        public void SetIntensityImmediate(float intensity)
        {
            _targetIntensity = intensity;
            _light.intensity = intensity;
        }

        /// <summary>
        /// Sets the light color immediately without animation.
        /// </summary>
        public void SetColorImmediate(Color color)
        {
            _targetColor = color;
            _light.color = color;
        }

        /// <summary>
        /// Turns the light on.
        /// </summary>
        [ContextMenu("Turn On")]
        public void TurnOn()
        {
            _light.enabled = true;
        }

        /// <summary>
        /// Turns the light off.
        /// </summary>
        [ContextMenu("Turn Off")]
        public void TurnOff()
        {
            _light.enabled = false;
        }

        /// <summary>
        /// Toggles the light on/off.
        /// </summary>
        public void Toggle()
        {
            _light.enabled = !_light.enabled;
        }

        /// <summary>
        /// Sets the current light values as the max output values.
        /// </summary>
        [ContextMenu("Set Current As Max")]
        public void SetCurrentAsMax()
        {
            maxIntensity = _light.intensity;
            maxRange = _light.range;
            maxSpotAngle = _light.spotAngle;
        }
    }
}

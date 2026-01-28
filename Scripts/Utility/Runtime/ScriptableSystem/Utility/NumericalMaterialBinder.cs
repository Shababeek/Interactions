using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds any numeric variable to a material's shader property.
    /// </summary>
    /// <remarks>
    /// Maps a numeric value to shader properties like floats, colors, or texture offsets.
    ///
    /// Common use cases include:
    /// - Dissolve effects (controlled by float)
    /// - Emission intensity
    /// - Fresnel strength
    /// - Scrolling textures (UV offset)
    /// - Fill amount shaders
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Numerical Material Binder")]
    public class NumericalMaterialBinder : MonoBehaviour
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Target")]
        [Tooltip("The renderer containing the material. Uses this object's renderer if not set.")]
        [SerializeField] private Renderer targetRenderer;

        [Tooltip("Material index if renderer has multiple materials.")]
        [SerializeField] private int materialIndex = 0;

        [Tooltip("Use shared material (affects all instances) or instance material.")]
        [SerializeField] private bool useSharedMaterial = false;

        [Header("Property")]
        [Tooltip("The type of shader property to modify.")]
        [SerializeField] private PropertyType propertyType = PropertyType.Float;

        [Tooltip("The shader property name (e.g., '_Dissolve', '_EmissionIntensity').")]
        [SerializeField] private string propertyName = "_Value";

        [Header("Value Mapping")]
        [Tooltip("The minimum variable value.")]
        [SerializeField] private float minValue = 0f;

        [Tooltip("The maximum variable value.")]
        [SerializeField] private float maxValue = 1f;

        [Header("Float Property Settings")]
        [Tooltip("Shader value when variable equals minValue.")]
        [SerializeField] private float minPropertyValue = 0f;

        [Tooltip("Shader value when variable equals maxValue.")]
        [SerializeField] private float maxPropertyValue = 1f;

        [Header("Color Property Settings")]
        [Tooltip("Color when variable equals minValue.")]
        [SerializeField] private Color minColor = Color.black;

        [Tooltip("Color when variable equals maxValue.")]
        [SerializeField] private Color maxColor = Color.white;

        [Header("Vector Property Settings")]
        [Tooltip("Vector when variable equals minValue.")]
        [SerializeField] private Vector4 minVector = Vector4.zero;

        [Tooltip("Vector when variable equals maxValue.")]
        [SerializeField] private Vector4 maxVector = Vector4.one;

        [Header("Interpolation")]
        [Tooltip("Whether to smoothly interpolate changes.")]
        [SerializeField] private bool smooth = false;

        [Tooltip("Interpolation speed.")]
        [SerializeField] private float smoothSpeed = 5f;

        private CompositeDisposable _disposable;
        private Material _material;
        private int _propertyId;
        private INumericalVariable _numericalVariable;

        // Current/target values for smooth interpolation
        private float _currentFloat, _targetFloat;
        private Color _currentColor, _targetColor;
        private Vector4 _currentVector, _targetVector;

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

            // Get renderer
            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer == null)
            {
                Debug.LogWarning($"No Renderer found on {gameObject.name}", this);
                return;
            }

            // Get material
            if (useSharedMaterial)
            {
                var mats = targetRenderer.sharedMaterials;
                if (materialIndex < mats.Length)
                    _material = mats[materialIndex];
            }
            else
            {
                var mats = targetRenderer.materials;
                if (materialIndex < mats.Length)
                    _material = mats[materialIndex];
            }

            if (_material == null)
            {
                Debug.LogWarning($"Material at index {materialIndex} not found on {gameObject.name}", this);
                return;
            }

            // Cache property ID
            _propertyId = Shader.PropertyToID(propertyName);

            // Initialize current values
            InitializeCurrentValues();

            // Set initial value
            UpdateProperty(_numericalVariable.AsFloat);

            // Subscribe to changes
            variable.OnRaised
                .Subscribe(_ => UpdateProperty(_numericalVariable.AsFloat))
                .AddTo(_disposable);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (!smooth || _material == null) return;

            switch (propertyType)
            {
                case PropertyType.Float:
                    _currentFloat = Mathf.Lerp(_currentFloat, _targetFloat, smoothSpeed * Time.deltaTime);
                    _material.SetFloat(_propertyId, _currentFloat);
                    break;

                case PropertyType.Color:
                    _currentColor = Color.Lerp(_currentColor, _targetColor, smoothSpeed * Time.deltaTime);
                    _material.SetColor(_propertyId, _currentColor);
                    break;

                case PropertyType.Vector:
                    _currentVector = Vector4.Lerp(_currentVector, _targetVector, smoothSpeed * Time.deltaTime);
                    _material.SetVector(_propertyId, _currentVector);
                    break;
            }
        }

        private void InitializeCurrentValues()
        {
            if (_material == null) return;

            switch (propertyType)
            {
                case PropertyType.Float:
                    _currentFloat = _material.HasProperty(_propertyId) ? _material.GetFloat(_propertyId) : minPropertyValue;
                    _targetFloat = _currentFloat;
                    break;

                case PropertyType.Color:
                    _currentColor = _material.HasProperty(_propertyId) ? _material.GetColor(_propertyId) : minColor;
                    _targetColor = _currentColor;
                    break;

                case PropertyType.Vector:
                    _currentVector = _material.HasProperty(_propertyId) ? _material.GetVector(_propertyId) : minVector;
                    _targetVector = _currentVector;
                    break;
            }
        }

        private void UpdateProperty(float value)
        {
            if (_material == null) return;

            float t = Mathf.InverseLerp(minValue, maxValue, value);

            switch (propertyType)
            {
                case PropertyType.Float:
                    _targetFloat = Mathf.Lerp(minPropertyValue, maxPropertyValue, t);
                    if (!smooth)
                    {
                        _material.SetFloat(_propertyId, _targetFloat);
                        _currentFloat = _targetFloat;
                    }
                    break;

                case PropertyType.Color:
                    _targetColor = Color.Lerp(minColor, maxColor, t);
                    if (!smooth)
                    {
                        _material.SetColor(_propertyId, _targetColor);
                        _currentColor = _targetColor;
                    }
                    break;

                case PropertyType.Vector:
                    _targetVector = Vector4.Lerp(minVector, maxVector, t);
                    if (!smooth)
                    {
                        _material.SetVector(_propertyId, _targetVector);
                        _currentVector = _targetVector;
                    }
                    break;
            }
        }

        public enum PropertyType
        {
            Float,
            Color,
            Vector
        }
    }
}

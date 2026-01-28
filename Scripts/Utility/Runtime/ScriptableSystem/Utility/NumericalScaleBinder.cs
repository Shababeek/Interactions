using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds any numeric variable to an object's scale.
    /// </summary>
    /// <remarks>
    /// Maps a numeric value range to a scale range. Can scale uniformly or per-axis.
    ///
    /// Common use cases include:
    /// - Health bars that shrink/grow
    /// - Breathing/pulsing effects
    /// - Size indicators
    /// - Progress visualization
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Numerical Scale Binder")]
    public class NumericalScaleBinder : MonoBehaviour
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Value Mapping")]
        [Tooltip("The minimum value from the variable that maps to minScale.")]
        [SerializeField] private float minValue = 0f;

        [Tooltip("The maximum value from the variable that maps to maxScale.")]
        [SerializeField] private float maxValue = 1f;

        [Header("Scale Settings")]
        [Tooltip("How to apply the scale transformation.")]
        [SerializeField] private ScaleMode scaleMode = ScaleMode.Uniform;

        [Tooltip("The scale when value equals minValue.")]
        [SerializeField] private Vector3 minScale = Vector3.zero;

        [Tooltip("The scale when value equals maxValue.")]
        [SerializeField] private Vector3 maxScale = Vector3.one;

        [Header("Interpolation")]
        [Tooltip("Whether to smoothly interpolate scale changes.")]
        [SerializeField] private bool smoothScale = false;

        [Tooltip("Scale interpolation speed.")]
        [SerializeField] private float scaleSpeed = 5f;

        [Tooltip("Animation curve for scale interpolation (optional).")]
        [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        private CompositeDisposable _disposable;
        private Vector3 _targetScale;
        private Vector3 _currentScale;
        private INumericalVariable _numericalVariable;

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

            _currentScale = transform.localScale;
            UpdateScale(_numericalVariable.AsFloat);

            variable.OnRaised
                .Subscribe(_ => UpdateScale(_numericalVariable.AsFloat))
                .AddTo(_disposable);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (!smoothScale) return;

            _currentScale = Vector3.Lerp(_currentScale, _targetScale, scaleSpeed * Time.deltaTime);
            transform.localScale = _currentScale;
        }

        private void UpdateScale(float value)
        {
            float t = Mathf.InverseLerp(minValue, maxValue, value);
            t = curve.Evaluate(t);

            switch (scaleMode)
            {
                case ScaleMode.Uniform:
                    float uniformScale = Mathf.Lerp(minScale.x, maxScale.x, t);
                    _targetScale = Vector3.one * uniformScale;
                    break;

                case ScaleMode.PerAxis:
                    _targetScale = Vector3.Lerp(minScale, maxScale, t);
                    break;

                case ScaleMode.XOnly:
                    _targetScale = transform.localScale;
                    _targetScale.x = Mathf.Lerp(minScale.x, maxScale.x, t);
                    break;

                case ScaleMode.YOnly:
                    _targetScale = transform.localScale;
                    _targetScale.y = Mathf.Lerp(minScale.y, maxScale.y, t);
                    break;

                case ScaleMode.ZOnly:
                    _targetScale = transform.localScale;
                    _targetScale.z = Mathf.Lerp(minScale.z, maxScale.z, t);
                    break;
            }

            if (!smoothScale)
            {
                transform.localScale = _targetScale;
                _currentScale = _targetScale;
            }
        }

        /// <summary>
        /// Sets the current scale from the transform as min scale.
        /// </summary>
        [ContextMenu("Set Min Scale From Current")]
        public void SetMinScaleFromCurrent()
        {
            minScale = transform.localScale;
        }

        /// <summary>
        /// Sets the current scale from the transform as max scale.
        /// </summary>
        [ContextMenu("Set Max Scale From Current")]
        public void SetMaxScaleFromCurrent()
        {
            maxScale = transform.localScale;
        }

        public enum ScaleMode
        {
            /// <summary>Scale uniformly on all axes using minScale.x and maxScale.x</summary>
            Uniform,
            /// <summary>Scale each axis independently</summary>
            PerAxis,
            /// <summary>Only scale on X axis</summary>
            XOnly,
            /// <summary>Only scale on Y axis</summary>
            YOnly,
            /// <summary>Only scale on Z axis</summary>
            ZOnly
        }
    }
}

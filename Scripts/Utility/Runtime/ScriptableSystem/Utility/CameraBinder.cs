using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Binds ScriptableVariables to Camera component properties.
    /// Control field of view, near/far clip planes, background color, and more through variables.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Binders/Camera Binder")]
    [RequireComponent(typeof(Camera))]
    public class CameraBinder : MonoBehaviour
    {
        [Header("Field of View")]
        [Tooltip("Numeric variable to control FOV (perspective mode only).")]
        [SerializeField] private ScriptableVariable fovVariable;

        [Tooltip("Map variable range to FOV range.")]
        [SerializeField] private bool useFovMapping = false;

        [Tooltip("Variable value for minimum FOV.")]
        [SerializeField] private float minFovValue = 0f;

        [Tooltip("Variable value for maximum FOV.")]
        [SerializeField] private float maxFovValue = 1f;

        [Tooltip("Minimum FOV output.")]
        [SerializeField] private float minFov = 30f;

        [Tooltip("Maximum FOV output.")]
        [SerializeField] private float maxFov = 90f;

        [Header("Orthographic Size")]
        [Tooltip("Numeric variable to control orthographic size.")]
        [SerializeField] private ScriptableVariable orthoSizeVariable;

        [Tooltip("Map variable range to orthographic size range.")]
        [SerializeField] private bool useOrthoMapping = false;

        [Tooltip("Variable value for minimum ortho size.")]
        [SerializeField] private float minOrthoValue = 0f;

        [Tooltip("Variable value for maximum ortho size.")]
        [SerializeField] private float maxOrthoValue = 1f;

        [Tooltip("Minimum orthographic size.")]
        [SerializeField] private float minOrthoSize = 1f;

        [Tooltip("Maximum orthographic size.")]
        [SerializeField] private float maxOrthoSize = 10f;

        [Header("Clip Planes")]
        [Tooltip("Numeric variable to control near clip plane.")]
        [SerializeField] private ScriptableVariable nearClipVariable;

        [Tooltip("Numeric variable to control far clip plane.")]
        [SerializeField] private ScriptableVariable farClipVariable;

        [Header("Background")]
        [Tooltip("Color variable to control background color.")]
        [SerializeField] private ColorVariable backgroundColorVariable;

        [Header("Depth")]
        [Tooltip("Numeric variable to control camera depth.")]
        [SerializeField] private ScriptableVariable depthVariable;

        [Header("Viewport Rect")]
        [Tooltip("Vector2 variable for viewport position (x, y).")]
        [SerializeField] private Vector2Variable viewportPositionVariable;

        [Tooltip("Vector2 variable for viewport size (width, height).")]
        [SerializeField] private Vector2Variable viewportSizeVariable;

        [Header("Target Texture")]
        [Tooltip("Bool variable to control whether camera renders to target texture.")]
        [SerializeField] private BoolVariable useTargetTextureVariable;

        [Tooltip("Render texture to use when useTargetTextureVariable is true.")]
        [SerializeField] private RenderTexture targetTexture;

        [Header("Enabled")]
        [Tooltip("Bool variable to control camera enabled state.")]
        [SerializeField] private BoolVariable enabledVariable;

        [Tooltip("Invert the enabled logic.")]
        [SerializeField] private bool invertEnabled = false;

        [Header("Animation")]
        [Tooltip("Smoothly animate property changes.")]
        [SerializeField] private bool smoothChanges = false;

        [Tooltip("Animation speed for smooth changes.")]
        [SerializeField] private float smoothSpeed = 5f;

        private Camera _camera;
        private CompositeDisposable _disposable;

        // Target values for smooth animation
        private float _targetFov;
        private float _targetOrthoSize;
        private float _targetNearClip;
        private float _targetFarClip;
        private Color _targetBackgroundColor;

        // Numerical variable references
        private INumericalVariable _fovNumerical;
        private INumericalVariable _orthoNumerical;
        private INumericalVariable _nearClipNumerical;
        private INumericalVariable _farClipNumerical;
        private INumericalVariable _depthNumerical;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            // Store current values as targets
            _targetFov = _camera.fieldOfView;
            _targetOrthoSize = _camera.orthographicSize;
            _targetNearClip = _camera.nearClipPlane;
            _targetFarClip = _camera.farClipPlane;
            _targetBackgroundColor = _camera.backgroundColor;

            // FOV binding
            if (fovVariable != null)
            {
                _fovNumerical = fovVariable as INumericalVariable;
                if (_fovNumerical != null)
                {
                    UpdateFov(_fovNumerical.AsFloat);

                    fovVariable.OnRaised
                        .Subscribe(_ => UpdateFov(_fovNumerical.AsFloat))
                        .AddTo(_disposable);
                }
            }

            // Orthographic size binding
            if (orthoSizeVariable != null)
            {
                _orthoNumerical = orthoSizeVariable as INumericalVariable;
                if (_orthoNumerical != null)
                {
                    UpdateOrthoSize(_orthoNumerical.AsFloat);

                    orthoSizeVariable.OnRaised
                        .Subscribe(_ => UpdateOrthoSize(_orthoNumerical.AsFloat))
                        .AddTo(_disposable);
                }
            }

            // Near clip binding
            if (nearClipVariable != null)
            {
                _nearClipNumerical = nearClipVariable as INumericalVariable;
                if (_nearClipNumerical != null)
                {
                    UpdateNearClip(_nearClipNumerical.AsFloat);

                    nearClipVariable.OnRaised
                        .Subscribe(_ => UpdateNearClip(_nearClipNumerical.AsFloat))
                        .AddTo(_disposable);
                }
            }

            // Far clip binding
            if (farClipVariable != null)
            {
                _farClipNumerical = farClipVariable as INumericalVariable;
                if (_farClipNumerical != null)
                {
                    UpdateFarClip(_farClipNumerical.AsFloat);

                    farClipVariable.OnRaised
                        .Subscribe(_ => UpdateFarClip(_farClipNumerical.AsFloat))
                        .AddTo(_disposable);
                }
            }

            // Background color binding
            if (backgroundColorVariable != null)
            {
                UpdateBackgroundColor(backgroundColorVariable.Value);

                backgroundColorVariable.OnRaised
                    .Subscribe(_ => UpdateBackgroundColor(backgroundColorVariable.Value))
                    .AddTo(_disposable);
            }

            // Depth binding
            if (depthVariable != null)
            {
                _depthNumerical = depthVariable as INumericalVariable;
                if (_depthNumerical != null)
                {
                    _camera.depth = _depthNumerical.AsFloat;

                    depthVariable.OnRaised
                        .Subscribe(_ => _camera.depth = _depthNumerical.AsFloat)
                        .AddTo(_disposable);
                }
            }

            // Viewport position binding
            if (viewportPositionVariable != null)
            {
                UpdateViewportPosition(viewportPositionVariable.Value);

                viewportPositionVariable.OnRaised
                    .Subscribe(_ => UpdateViewportPosition(viewportPositionVariable.Value))
                    .AddTo(_disposable);
            }

            // Viewport size binding
            if (viewportSizeVariable != null)
            {
                UpdateViewportSize(viewportSizeVariable.Value);

                viewportSizeVariable.OnRaised
                    .Subscribe(_ => UpdateViewportSize(viewportSizeVariable.Value))
                    .AddTo(_disposable);
            }

            // Target texture binding
            if (useTargetTextureVariable != null)
            {
                UpdateTargetTexture(useTargetTextureVariable.Value);

                useTargetTextureVariable.OnRaised
                    .Subscribe(_ => UpdateTargetTexture(useTargetTextureVariable.Value))
                    .AddTo(_disposable);
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

            // Smooth FOV
            if (!Mathf.Approximately(_camera.fieldOfView, _targetFov))
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFov, dt);
            }

            // Smooth orthographic size
            if (!Mathf.Approximately(_camera.orthographicSize, _targetOrthoSize))
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetOrthoSize, dt);
            }

            // Smooth near clip
            if (!Mathf.Approximately(_camera.nearClipPlane, _targetNearClip))
            {
                _camera.nearClipPlane = Mathf.Lerp(_camera.nearClipPlane, _targetNearClip, dt);
            }

            // Smooth far clip
            if (!Mathf.Approximately(_camera.farClipPlane, _targetFarClip))
            {
                _camera.farClipPlane = Mathf.Lerp(_camera.farClipPlane, _targetFarClip, dt);
            }

            // Smooth background color
            if (_camera.backgroundColor != _targetBackgroundColor)
            {
                _camera.backgroundColor = Color.Lerp(_camera.backgroundColor, _targetBackgroundColor, dt);
            }
        }

        private void UpdateFov(float value)
        {
            float fov;

            if (useFovMapping)
            {
                float t = Mathf.InverseLerp(minFovValue, maxFovValue, value);
                fov = Mathf.Lerp(minFov, maxFov, t);
            }
            else
            {
                fov = value;
            }

            _targetFov = Mathf.Clamp(fov, 1f, 179f);

            if (!smoothChanges)
            {
                _camera.fieldOfView = _targetFov;
            }
        }

        private void UpdateOrthoSize(float value)
        {
            float size;

            if (useOrthoMapping)
            {
                float t = Mathf.InverseLerp(minOrthoValue, maxOrthoValue, value);
                size = Mathf.Lerp(minOrthoSize, maxOrthoSize, t);
            }
            else
            {
                size = value;
            }

            _targetOrthoSize = Mathf.Max(0.01f, size);

            if (!smoothChanges)
            {
                _camera.orthographicSize = _targetOrthoSize;
            }
        }

        private void UpdateNearClip(float value)
        {
            _targetNearClip = Mathf.Max(0.01f, value);

            if (!smoothChanges)
            {
                _camera.nearClipPlane = _targetNearClip;
            }
        }

        private void UpdateFarClip(float value)
        {
            _targetFarClip = Mathf.Max(_camera.nearClipPlane + 0.01f, value);

            if (!smoothChanges)
            {
                _camera.farClipPlane = _targetFarClip;
            }
        }

        private void UpdateBackgroundColor(Color color)
        {
            _targetBackgroundColor = color;

            if (!smoothChanges)
            {
                _camera.backgroundColor = color;
            }
        }

        private void UpdateViewportPosition(Vector2 position)
        {
            var rect = _camera.rect;
            rect.x = position.x;
            rect.y = position.y;
            _camera.rect = rect;
        }

        private void UpdateViewportSize(Vector2 size)
        {
            var rect = _camera.rect;
            rect.width = size.x;
            rect.height = size.y;
            _camera.rect = rect;
        }

        private void UpdateTargetTexture(bool useTexture)
        {
            _camera.targetTexture = useTexture ? targetTexture : null;
        }

        private void UpdateEnabled(bool value)
        {
            _camera.enabled = invertEnabled ? !value : value;
        }

        #region Public API

        /// <summary>
        /// Sets the FOV immediately without animation.
        /// </summary>
        public void SetFovImmediate(float fov)
        {
            _targetFov = fov;
            _camera.fieldOfView = fov;
        }

        /// <summary>
        /// Sets the orthographic size immediately without animation.
        /// </summary>
        public void SetOrthoSizeImmediate(float size)
        {
            _targetOrthoSize = size;
            _camera.orthographicSize = size;
        }

        /// <summary>
        /// Toggles between orthographic and perspective projection.
        /// </summary>
        public void SetOrthographic(bool orthographic)
        {
            _camera.orthographic = orthographic;
        }

        /// <summary>
        /// Sets the camera's culling mask.
        /// </summary>
        public void SetCullingMask(LayerMask mask)
        {
            _camera.cullingMask = mask;
        }

        /// <summary>
        /// Captures the current camera settings as the target values.
        /// </summary>
        [ContextMenu("Capture Current Settings")]
        public void CaptureCurrentSettings()
        {
            _targetFov = _camera.fieldOfView;
            _targetOrthoSize = _camera.orthographicSize;
            _targetNearClip = _camera.nearClipPlane;
            _targetFarClip = _camera.farClipPlane;
            _targetBackgroundColor = _camera.backgroundColor;
        }

        /// <summary>
        /// Sets the current camera values as the max mapping values.
        /// </summary>
        [ContextMenu("Set Current As Max")]
        public void SetCurrentAsMax()
        {
            maxFov = _camera.fieldOfView;
            maxOrthoSize = _camera.orthographicSize;
        }

        /// <summary>
        /// Sets the current camera values as the min mapping values.
        /// </summary>
        [ContextMenu("Set Current As Min")]
        public void SetCurrentAsMin()
        {
            minFov = _camera.fieldOfView;
            minOrthoSize = _camera.orthographicSize;
        }

        #endregion
    }
}

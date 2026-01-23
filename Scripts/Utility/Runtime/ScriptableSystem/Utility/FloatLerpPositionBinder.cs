using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    public enum FloatLerpMode
    {
        Direct,
        Velocity,
        SmoothDamp
    }

    /// <summary>
    /// Moves an object between two positions based on a float value (0-1).
    /// Supports direct, velocity-based, and smooth interpolation modes.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Float Lerp Position Binder")]
    public class FloatLerpPositionBinder : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private FloatVariable floatInput;

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool useLocalSpace = true;

        [Header("Positions")]
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Vector3 endPosition = Vector3.forward;

        [Header("Movement Mode")]
        [SerializeField] private FloatLerpMode mode = FloatLerpMode.Direct;
        [SerializeField] private float velocitySpeed = 2f;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private AnimationCurve easingCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private CompositeDisposable _disposable;
        private float _currentT;
        private float _targetT;
        private float _velocity;

        private void OnEnable()
        {
            if (target == null) target = transform;
            _disposable = new CompositeDisposable();

            if (floatInput != null)
            {
                floatInput.OnValueChanged
                    .Subscribe(OnValueChanged)
                    .AddTo(_disposable);
                _currentT = Mathf.Clamp01(floatInput.Value);
                _targetT = _currentT;
            }

            if (mode == FloatLerpMode.Direct)
            {
                ApplyPosition(_currentT);
            }
        }

        private void OnDisable() => _disposable?.Dispose();

        private void OnValueChanged(float value)
        {
            _targetT = Mathf.Clamp01(value);

            if (mode == FloatLerpMode.Direct)
            {
                _currentT = _targetT;
                ApplyPosition(_currentT);
            }
        }

        private void Update()
        {
            if (mode == FloatLerpMode.Direct) return;

            if (mode == FloatLerpMode.Velocity)
            {
                // Move towards target at fixed speed
                _currentT = Mathf.MoveTowards(_currentT, _targetT, velocitySpeed * Time.deltaTime);
            }
            else if (mode == FloatLerpMode.SmoothDamp)
            {
                _currentT = Mathf.SmoothDamp(_currentT, _targetT, ref _velocity, smoothTime);
            }

            ApplyPosition(_currentT);
        }

        private void ApplyPosition(float t)
        {
            float easedT = easingCurve.Evaluate(t);
            Vector3 position = Vector3.Lerp(startPosition, endPosition, easedT);

            if (useLocalSpace)
                target.localPosition = position;
            else
                target.position = position;
        }

        public void SetStartPosition(Vector3 pos)
        {
            startPosition = pos;
            ApplyPosition(_currentT);
        }

        public void SetEndPosition(Vector3 pos)
        {
            endPosition = pos;
            ApplyPosition(_currentT);
        }

        public void SetPositions(Vector3 start, Vector3 end)
        {
            startPosition = start;
            endPosition = end;
            ApplyPosition(_currentT);
        }

        public void SetValue(float value)
        {
            _targetT = Mathf.Clamp01(value);
            if (mode == FloatLerpMode.Direct)
            {
                _currentT = _targetT;
                ApplyPosition(_currentT);
            }
        }

        public void SnapToTarget()
        {
            _currentT = _targetT;
            _velocity = 0f;
            ApplyPosition(_currentT);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 start = startPosition;
            Vector3 end = endPosition;

            if (target != null && useLocalSpace && target.parent != null)
            {
                start = target.parent.TransformPoint(startPosition);
                end = target.parent.TransformPoint(endPosition);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(start, 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(end, 0.02f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, end);

            // Draw current position
            if (Application.isPlaying)
            {
                Vector3 current = Vector3.Lerp(start, end, _currentT);
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(current, 0.03f);
            }
        }
    }
}

using UniRx;
using UnityEngine;

namespace Shababeek.Utilities
{
    /// <summary>
    /// Unified binder for transform properties: position, rotation, and scale.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Transform Binder")]
    public class TransformBinder : MonoBehaviour
    {
        [Header("Position")]
        [SerializeField] private bool bindPosition;
        [SerializeField] private Vector3Variable positionVariable;
        [SerializeField] private bool useLocalPosition;
        [SerializeField] private Vector3 positionOffset;

        [Header("Rotation")]
        [SerializeField] private bool bindRotation;
        [SerializeField] private RotationMode rotationMode = RotationMode.Euler;
        [SerializeField] private Vector3Variable eulerVariable;
        [SerializeField] private QuaternionVariable quaternionVariable;
        [SerializeField] private Vector2Variable directionVariable;
        [SerializeField] private RotationPlane directionPlane = RotationPlane.XY;
        [SerializeField] private float angleOffset;
        [SerializeField] private bool useLocalRotation = true;

        [Header("Scale")]
        [SerializeField] private bool bindScale;
        [SerializeField] private ScaleMode scaleMode = ScaleMode.Uniform;
        [SerializeField] private Vector3Variable scaleVector;
        [SerializeField] private ScriptableVariable uniformScale;
        [SerializeField] private Vector3 baseScale = Vector3.one;
        [SerializeField] private float minScale = 0.001f;

        [Header("Interpolation")]
        [SerializeField] private bool smooth;
        [SerializeField] private float speed = 5f;

        private CompositeDisposable _disposable;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private Vector3 _targetScale;
        private INumericalVariable _uniformNumVar;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (bindPosition) SetupPosition();
            if (bindRotation) SetupRotation();
            if (bindScale) SetupScale();
        }

        private void OnDisable() => _disposable?.Dispose();

        private void Update()
        {
            if (!smooth) return;

            if (bindPosition)
            {
                var pos = useLocalPosition ? transform.localPosition : transform.position;
                pos = Vector3.Lerp(pos, _targetPosition, speed * Time.deltaTime);
                if (useLocalPosition) transform.localPosition = pos;
                else transform.position = pos;
            }

            if (bindRotation)
            {
                var rot = useLocalRotation ? transform.localRotation : transform.rotation;
                rot = Quaternion.Slerp(rot, _targetRotation, speed * Time.deltaTime);
                if (useLocalRotation) transform.localRotation = rot;
                else transform.rotation = rot;
            }

            if (bindScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, speed * Time.deltaTime);
            }
        }

        #region Position

        private void SetupPosition()
        {
            if (positionVariable == null) return;
            UpdatePosition(positionVariable.Value);
            positionVariable.OnValueChanged.Subscribe(UpdatePosition).AddTo(_disposable);
        }

        private void UpdatePosition(Vector3 pos)
        {
            _targetPosition = pos + positionOffset;
            if (!smooth) ApplyPosition();
        }

        private void ApplyPosition()
        {
            if (useLocalPosition) transform.localPosition = _targetPosition;
            else transform.position = _targetPosition;
        }

        #endregion

        #region Rotation

        private void SetupRotation()
        {
            switch (rotationMode)
            {
                case RotationMode.Euler when eulerVariable != null:
                    UpdateEuler(eulerVariable.Value);
                    eulerVariable.OnValueChanged.Subscribe(UpdateEuler).AddTo(_disposable);
                    break;
                case RotationMode.Quaternion when quaternionVariable != null:
                    UpdateQuaternion(quaternionVariable.Value);
                    quaternionVariable.OnValueChanged.Subscribe(UpdateQuaternion).AddTo(_disposable);
                    break;
                case RotationMode.Direction2D when directionVariable != null:
                    UpdateDirection(directionVariable.Value);
                    directionVariable.OnValueChanged.Subscribe(UpdateDirection).AddTo(_disposable);
                    break;
            }
        }

        private void UpdateEuler(Vector3 euler)
        {
            _targetRotation = Quaternion.Euler(euler);
            if (!smooth) ApplyRotation();
        }

        private void UpdateQuaternion(Quaternion quat)
        {
            _targetRotation = quat;
            if (!smooth) ApplyRotation();
        }

        private void UpdateDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude < 0.0001f) return;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffset;

            Vector3 euler = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
            switch (directionPlane)
            {
                case RotationPlane.XY: euler.z = angle; break;
                case RotationPlane.XZ: euler.y = angle; break;
                case RotationPlane.YZ: euler.x = angle; break;
            }
            _targetRotation = Quaternion.Euler(euler);
            if (!smooth) ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (useLocalRotation) transform.localRotation = _targetRotation;
            else transform.rotation = _targetRotation;
        }

        #endregion

        #region Scale

        private void SetupScale()
        {
            if (scaleMode == ScaleMode.Vector3 && scaleVector != null)
            {
                UpdateScale(scaleVector.Value);
                scaleVector.OnValueChanged.Subscribe(UpdateScale).AddTo(_disposable);
            }
            else if (scaleMode == ScaleMode.Uniform && uniformScale != null)
            {
                _uniformNumVar = uniformScale as INumericalVariable;
                if (_uniformNumVar == null) return;
                UpdateUniformScale(_uniformNumVar.AsFloat);
                uniformScale.OnRaised.Subscribe(_ => UpdateUniformScale(_uniformNumVar.AsFloat)).AddTo(_disposable);
            }
        }

        private void UpdateScale(Vector3 scale)
        {
            _targetScale = new Vector3(
                Mathf.Max(scale.x, minScale),
                Mathf.Max(scale.y, minScale),
                Mathf.Max(scale.z, minScale));
            if (!smooth) transform.localScale = _targetScale;
        }

        private void UpdateUniformScale(float value)
        {
            var scale = baseScale * Mathf.Max(value, minScale);
            _targetScale = scale;
            if (!smooth) transform.localScale = _targetScale;
        }

        #endregion

        public enum RotationMode { Euler, Quaternion, Direction2D }
        public enum RotationPlane { XY, XZ, YZ }
        public enum ScaleMode { Vector3, Uniform }
    }
}

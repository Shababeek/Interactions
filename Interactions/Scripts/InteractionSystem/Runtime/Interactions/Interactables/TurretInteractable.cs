using System;
using UnityEngine;
using Shababeek.Interactions.Core;
using Shababeek.Core;
using UniRx;
using UnityEngine.Serialization;

namespace Shababeek.Interactions
{
    [Serializable]
    public class TurretInteractable : ConstrainedInteractableBase
    {
        [Header("Rotation Limits")]
        [SerializeField] private bool limitXRotation = true;
        [SerializeField, MinMax(-180, 180)] private float minXAngle = -90f;
        [SerializeField, MinMax(-180, 180)] private float maxXAngle = 90f;
        
        [SerializeField] private bool limitYRotation = true;
        [SerializeField, MinMax(-180, 180)] private float minYAngle = -180f;
        [SerializeField, MinMax(-180, 180)] private float maxYAngle = 180f;
        
        [SerializeField] private bool limitZRotation = false;
        [SerializeField, MinMax(-180, 180)] private float minZAngle = -45f;
        [SerializeField, MinMax(-180, 180)] private float maxZAngle = 45f;

        [Header("Return Behavior")]
        [SerializeField] private bool returnToOriginal = false;
        [SerializeField] private float returnSpeed = 5f;

        [Header("Events")]
        [SerializeField] private Vector3UnityEvent onRotationChanged = new();
        [SerializeField] private FloatUnityEvent onXRotationChanged = new();
        [SerializeField] private FloatUnityEvent onYRotationChanged = new();
        [SerializeField] private FloatUnityEvent onZRotationChanged = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private Vector3 currentRotation = Vector3.zero;
        [ReadOnly, SerializeField] private Vector3 normalizedRotation = Vector3.zero;

        // Private fields
        private Quaternion _originalRotation;
        private Vector3 _oldRotation = Vector3.zero;
        private bool _isReturning = false;

        public IObservable<Vector3> OnRotationChanged => onRotationChanged.AsObservable();
        public IObservable<float> OnXRotationChanged => onXRotationChanged.AsObservable();
        public IObservable<float> OnYRotationChanged => onYRotationChanged.AsObservable();
        public IObservable<float> OnZRotationChanged => onZRotationChanged.AsObservable();

        public Vector3 CurrentRotation => currentRotation;
        public Vector3 NormalizedRotation => normalizedRotation;
        
        // Public properties for editor access
        public bool LimitXRotation => limitXRotation;
        public float MinXAngle => minXAngle;
        public float MaxXAngle => maxXAngle;
        public bool LimitYRotation => limitYRotation;
        public float MinYAngle => minYAngle;
        public float MaxYAngle => maxYAngle;
        public bool LimitZRotation => limitZRotation;
        public float MinZAngle => minZAngle;
        public float MaxZAngle => maxZAngle;
        
        // Public setters for editor
        public void SetMinXAngle(float value) => minXAngle = value;
        public void SetMaxXAngle(float value) => maxXAngle = value;
        public void SetMinYAngle(float value) => minYAngle = value;
        public void SetMaxYAngle(float value) => maxYAngle = value;
        public void SetMinZAngle(float value) => minZAngle = value;
        public void SetMaxZAngle(float value) => maxZAngle = value;

        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            
            OnDeselected
                .Where(_ => returnToOriginal)
                .Do(_ => ReturnToOriginal())
                .Do(_ => InvokeEvents())
                .Subscribe().AddTo(this);
        }

        protected override void Activate()
        {
        }

        protected override void StartHover()
        {
        }

        protected override void EndHover()
        {
        }

        private void Update()
        {
            if (IsSelected)
            {
                UpdateRotation();
                InvokeEvents();
                _isReturning = false;
            }
            else if (returnToOriginal && _isReturning)
            {
                ReturnToOriginalSmooth();
            }
        }

        private void UpdateRotation()
        {
            if (CurrentInteractor == null) return;

            // Get the direction from turret center to hand
            Vector3 direction = CurrentInteractor.transform.position - transform.position;
            
            // Calculate rotation based on direction
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);
            
            // Extract Euler angles
            Vector3 targetEuler = targetRotation.eulerAngles;
            
            // Normalize angles to -180 to 180 range
            targetEuler.x = NormalizeAngle(targetEuler.x);
            targetEuler.y = NormalizeAngle(targetEuler.y);
            targetEuler.z = NormalizeAngle(targetEuler.z);
            
            // Apply limits
            if (limitXRotation)
                targetEuler.x = Mathf.Clamp(targetEuler.x, minXAngle, maxXAngle);
            if (limitYRotation)
                targetEuler.y = Mathf.Clamp(targetEuler.y, minYAngle, maxYAngle);
            if (limitZRotation)
                targetEuler.z = Mathf.Clamp(targetEuler.z, minZAngle, maxZAngle);
            
            // Apply rotation
            interactableObject.transform.localRotation = Quaternion.Euler(targetEuler);
            
            // Update current rotation
            currentRotation = targetEuler;
            
            // Calculate normalized values
            normalizedRotation = new Vector3(
                limitXRotation ? (targetEuler.x - minXAngle) / (maxXAngle - minXAngle) : 0.5f,
                limitYRotation ? (targetEuler.y - minYAngle) / (maxYAngle - minYAngle) : 0.5f,
                limitZRotation ? (targetEuler.z - minZAngle) / (maxZAngle - minZAngle) : 0.5f
            );
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        private void ReturnToOriginal()
        {
            interactableObject.transform.localRotation = _originalRotation;
            currentRotation = _originalRotation.eulerAngles;
            normalizedRotation = Vector3.zero;
            _oldRotation = Vector3.zero;
        }

        private void ReturnToOriginalSmooth()
        {
            interactableObject.transform.localRotation = Quaternion.Lerp(
                interactableObject.transform.localRotation,
                _originalRotation,
                returnSpeed * Time.deltaTime
            );

            // Update current rotation
            currentRotation = interactableObject.transform.localRotation.eulerAngles;
            
            // Stop returning when close enough
            if (Quaternion.Angle(interactableObject.transform.localRotation, _originalRotation) < 1f)
            {
                _isReturning = false;
                ReturnToOriginal();
            }
        }

        private void InvokeEvents()
        {
            Vector3 difference = currentRotation - _oldRotation;
            float absDifference = difference.magnitude;
            
            if (absDifference < 0.1f) return;
            
            _oldRotation = currentRotation;
            
            // Invoke events
            onRotationChanged.Invoke(currentRotation);
            onXRotationChanged.Invoke(normalizedRotation.x);
            onYRotationChanged.Invoke(normalizedRotation.y);
            onZRotationChanged.Invoke(normalizedRotation.z);
        }

        protected override void DeSelected()
        {
            base.DeSelected();
            if (returnToOriginal)
            {
                _isReturning = true;
            }
        }

        // Public methods for external control
        public void SetRotation(Vector3 eulerAngles)
        {
            Vector3 clampedAngles = new Vector3(
                limitXRotation ? Mathf.Clamp(eulerAngles.x, minXAngle, maxXAngle) : eulerAngles.x,
                limitYRotation ? Mathf.Clamp(eulerAngles.y, minYAngle, maxYAngle) : eulerAngles.y,
                limitZRotation ? Mathf.Clamp(eulerAngles.z, minZAngle, maxZAngle) : eulerAngles.z
            );
            
            interactableObject.transform.localRotation = Quaternion.Euler(clampedAngles);
            currentRotation = clampedAngles;
        }

        public void SetNormalizedRotation(Vector3 normalized)
        {
            Vector3 eulerAngles = new Vector3(
                limitXRotation ? Mathf.Lerp(minXAngle, maxXAngle, normalized.x) : 0f,
                limitYRotation ? Mathf.Lerp(minYAngle, maxYAngle, normalized.y) : 0f,
                limitZRotation ? Mathf.Lerp(minZAngle, maxZAngle, normalized.z) : 0f
            );
            
            SetRotation(eulerAngles);
        }
    }
} 
using System;
using UnityEngine;
using Shababeek.Core;
using UniRx;

namespace Shababeek.Interactions
{
    public enum RotationAxis
    {
        Right,
        Up,
        Forward
    }

    [Serializable]
    public class LeverInteractable : ConstrainedInteractableBase
    {
        public IObservable<float> OnLeverChanged => onLeverChanged.AsObservable();
        
        [SerializeField] private bool returnToOriginal;
        
        [SerializeField,MinMax(-180,-1)] private float min = -40;
        [SerializeField,MinMax(1,180)] private float max = 40;
        [SerializeField] public RotationAxis rotationAxis = RotationAxis.Right;
        [SerializeField] private FloatUnityEvent onLeverChanged=new();

        [ReadOnly] [SerializeField] private float currentNormalizedAngle = 0;
        private float _oldNormalizedAngle = 0;
        private Quaternion _originalRotation;
        public float Min
        {
            get => min;
            set => min = value;
        }

        public float Max
        {
            get => max;
            set => max = value;
        }

        private void Start()
        {
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

        protected override void HandleObjectMovement()
        {
            if (!IsSelected) return;
            
            var (plane, normal) = GetRotationAxis();
            Rotate(CalculateAngle(plane, normal));
            InvokeEvents();
        }

        protected override void HandleObjectDeselection()
        {
            if (returnToOriginal)
            {
                ReturnToOriginal();
                InvokeEvents();
            }
        }

        private void Update()
        {
            if (IsSelected)
            {
                HandleObjectMovement();
            }
        }

        private void Rotate(float x)
        {
            var angle = LimitAngle(x, min, max);
            interactableObject.transform.localRotation = Quaternion.AngleAxis(angle, GetRotationAxis().plane);
            currentNormalizedAngle = (angle - min) / (max - min);
        }

        private void ReturnToOriginal()
        {
            interactableObject.transform.localRotation = _originalRotation;
            currentNormalizedAngle = 0;
            _oldNormalizedAngle = 0;
        }

        private void InvokeEvents()
        {
            var difference = currentNormalizedAngle - _oldNormalizedAngle;
            var absDifference = Mathf.Abs(difference);
            if (absDifference < .1f) return;
            _oldNormalizedAngle = currentNormalizedAngle;
            onLeverChanged.Invoke(currentNormalizedAngle);
        }

        private float CalculateAngle(Vector3 plane,Vector3 normal)
        {
            var direction = CurrentInteractor.transform.position - transform.position;
            direction = Vector3.ProjectOnPlane(direction, -plane).normalized;
            var angle = -Vector3.SignedAngle(direction, normal, plane);
            return angle;
        }


        public (Vector3 plane,Vector3 normal) GetRotationAxis()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => (transform.right, transform.up),
                RotationAxis.Up => (transform.up, transform.forward),
                RotationAxis.Forward => (transform.forward, transform.up),
                _ => (transform.right, transform.up)
            };
        }

        private float LimitAngle(float angle, float min, float max)
        {
            if (angle > max) angle = max;

            if (angle < min) angle = min;

            return angle;
        }
    }
}
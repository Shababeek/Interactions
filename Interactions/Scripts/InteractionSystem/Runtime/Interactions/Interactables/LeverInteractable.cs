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


        [SerializeField, MinMax(-180, -1)] private float min = -40;
        [SerializeField, MinMax(1, 180)] private float max = 40;
        [SerializeField] public RotationAxis rotationAxis = RotationAxis.Right;
        [SerializeField] private FloatUnityEvent onLeverChanged = new();
        [SerializeField] private bool returnToOriginal;

        [ReadOnly] [SerializeField] private float currentNormalizedAngle = 0;
        private float _oldNormalizedAngle = 0;
        private Quaternion _originalRotation;
        
        private Vector3 _axisWorld;
        private Vector3 _referenceNormalWorld;
        private const float ProjectedEpsilon = 1e-5f;

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
            // Cache the original local rotation of the interactable pivot
            _originalRotation = interactableObject != null
                ? interactableObject.transform.localRotation
                : Quaternion.identity;

            CacheWorldRotationBasis();

            OnDeselected
                .Where(_ => returnToOriginal)
                .Do(_ => ReturnToOriginal())
                .Do(_ => InvokeEvents())
                .Subscribe().AddTo(this);
        }

        protected override void UseStarted()
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

            Rotate(CalculateAngle(_axisWorld, _referenceNormalWorld));
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

 

        private void Rotate(float x)
        {
            var angle = LimitAngle(x, min, max);
            Vector3 localAxis = rotationAxis switch
            {
                RotationAxis.Right => Vector3.right,
                RotationAxis.Up => Vector3.up,
                RotationAxis.Forward => Vector3.forward,
                _ => Vector3.right
            };

            var relative = Quaternion.AngleAxis(angle, localAxis);
            interactableObject.transform.localRotation = _originalRotation * relative;
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

        private float CalculateAngle(Vector3 axisWorld, Vector3 referenceNormalWorld)
        {
            // Direction from pivot to hand
            var fromPivotToHand = CurrentInteractor.transform.position - interactableObject.transform.position;

            // Project the vector onto the plane perpendicular to the axis
            var projected = Vector3.ProjectOnPlane(fromPivotToHand, axisWorld);
            if (projected.sqrMagnitude < ProjectedEpsilon)
            {
                // Keep previous angle when the hand is on/near the axis to avoid jitter
                return (currentNormalizedAngle * (max - min)) + min;
            }
            var projectedDirection = projected.normalized;

            // Signed angle from reference normal to projected direction around the axis
            var angle = Vector3.SignedAngle(referenceNormalWorld, projectedDirection, axisWorld);
            return angle;
        }


        public (Vector3 plane, Vector3 normal) GetRotationAxis()
        {
            var t = interactableObject.transform;
            return rotationAxis switch
            {
                RotationAxis.Right => (t.right, t.up),
                RotationAxis.Up => (t.up, t.forward),
                RotationAxis.Forward => (t.forward, t.up),
                _ => (t.right, t.up)
            };
        }

        private void CacheWorldRotationBasis()
        {
            var parent = interactableObject.transform.parent;
            var basisRotation = parent != null ? parent.rotation : Quaternion.identity;

            Vector3 LocalAxis() => rotationAxis switch
            {
                RotationAxis.Right => Vector3.right,
                RotationAxis.Up => Vector3.up,
                RotationAxis.Forward => Vector3.forward,
                _ => Vector3.right
            };

            Vector3 LocalReference() => rotationAxis switch
            {
                RotationAxis.Right => Vector3.up,
                RotationAxis.Up => Vector3.forward,
                RotationAxis.Forward => Vector3.up,
                _ => Vector3.up
            };

            _axisWorld = basisRotation * LocalAxis();
            _referenceNormalWorld = basisRotation * LocalReference();
        }

        private float LimitAngle(float angle, float min, float max)
        {
            if (angle > max) angle = max;

            if (angle < min) angle = min;

            return angle;
        }
    }
}
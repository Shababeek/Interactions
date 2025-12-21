using System;
using UnityEngine;
using Shababeek.Utilities;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Defines the axis around which a lever rotates.
    /// </summary>
    public enum RotationAxis
    {
        Right,
        Up,
        Forward
    }

    /// <summary>
    /// Lever-style interactable that rotates around a single axis with configurable limits.
    /// Provides smooth rotation control and normalized output values.
    /// </summary>
    [Serializable]
    public class LeverInteractable : ConstrainedInteractableBase
    {
        [Tooltip("Minimum rotation angle in degrees. Must be negative.")]
        [SerializeField, MinMax(-180, -1)] private float min = -40;
        [Tooltip("Maximum rotation angle in degrees. Must be positive.")]
        [SerializeField, MinMax(1, 180)] private float max = 40;
        [Tooltip("The axis around which the lever rotates.")]
        [SerializeField] public RotationAxis rotationAxis = RotationAxis.Right;
        [Tooltip("Event raised when the lever's normalized position changes (0-1 range).")]
        [SerializeField] private FloatUnityEvent onLeverChanged = new();
        [Tooltip("Whether the lever should return to its original position when deselected.")]
        [SerializeField] private bool returnToOriginal;

        [Tooltip("Current normalized rotation angle (0-1 range) (read-only).")]
        [ReadOnly] [SerializeField] private float currentNormalizedAngle = 0;
        private float _oldNormalizedAngle = 0;
        private Quaternion _originalRotation;
        
        private Vector3 _axisWorld;
        private Vector3 _referenceNormalWorld;
        private Vector3 _previousTargetPosition;
        private const float ProjectedEpsilon = 1e-5f;

        /// <summary>
        /// Observable that fires when the lever's normalized position changes.
        /// </summary>
        public IObservable<float> OnLeverChanged => onLeverChanged.AsObservable();

        /// <summary>
        /// Gets or sets the minimum rotation angle in degrees.
        /// </summary>
        public float Min
        {
            get => min;
            set => min = value;
        }

        /// <summary>
        /// Gets or sets the maximum rotation angle in degrees.
        /// </summary>
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
                .Do(_ => HandleReturnToOriginalPosition())
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

        protected override void HandleObjectMovement(Vector3 target)
        {
            Rotate(CalculateAngle(target,_axisWorld, _referenceNormalWorld));
            if (Vector3.Distance(target, _previousTargetPosition)>.01f)
            {
                _previousTargetPosition=target;

                InvokeEvents();
            } 
        }

        protected override void HandleObjectDeselection()
        {
            if (returnToOriginal)
            {
                HandleReturnToOriginalPosition();
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

        protected override void HandleReturnToOriginalPosition()
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

        private float CalculateAngle(Vector3 target,Vector3 axisWorld, Vector3 referenceNormalWorld)
        {
            // Direction from pivot to hand
            var fromPivotToHand = target - interactableObject.transform.position;

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

        /// <summary>
        /// Gets the rotation axis and normal vector for the lever.
        /// </summary>
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
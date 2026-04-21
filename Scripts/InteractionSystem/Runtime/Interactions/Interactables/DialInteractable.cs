using System;
using UnityEngine;
using Shababeek.ReactiveVars;
using Shababeek.Interactions.Core;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Rotary dial with discrete steps (combination lock, selector switch, rotary phone dial).
    /// Mirrors WheelInteractable's structure but snaps to fixed step positions on release.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Dial")]
    public class DialInteractable : ConstrainedInteractableBase
    {
        [Header("Dial Settings")]
        [Tooltip("How the hand and dial interact during grab.")]
        [SerializeField] private WheelGrabMode grabMode = WheelGrabMode.ObjectFollowsHand;

        [Tooltip("The axis around which the dial rotates.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Forward;

        [Header("Steps")]
        [Tooltip("Number of discrete positions on the dial.")]
        [SerializeField, Min(2)] private int numberOfSteps = 8;

        [Tooltip("Starting step index (0-based).")]
        [SerializeField] private int startingStep = 0;

        [Tooltip("Total rotation angle covered by all steps (360 = full circle, 180 = half).")]
        [SerializeField] private float totalAngle = 360f;

        [Tooltip("Allow rotating past the last step to wrap back to the first.")]
        [SerializeField] private bool wrapAround = false;

        [Header("Haptics")]
        [Tooltip("Play a haptic pulse each time a step boundary is crossed.")]
        [SerializeField] private bool hapticOnStep = true;

        [Tooltip("Haptic amplitude (0-1) when crossing a step.")]
        [SerializeField, Range(0f, 1f)] private float hapticAmplitude = 0.3f;

        [Tooltip("Haptic pulse duration in seconds when crossing a step.")]
        [SerializeField] private float hapticDuration = 0.05f;

        [Header("Events")]
        [Tooltip("Fired when the current step changes. Passes the new step index.")]
        [SerializeField] private IntUnityEvent onStepChanged = new();

        [Tooltip("Fired when a step is committed (snap animation complete or direct API call).")]
        [SerializeField] private IntUnityEvent onStepConfirmed = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private int currentStep;
        [ReadOnly, SerializeField] private float currentAngle;

        private Quaternion _originalRotation;
        private float _previousHandAngle;
        private int _previousStep;
        private float _targetSnapAngle;

        private Transform _fakeHand;
        private float _fakeHandOrbitAngle;

        /// <summary>Observable fired when the current step changes.</summary>
        public IObservable<int> OnStepChanged => onStepChanged.AsObservable();

        /// <summary>Observable fired when a step is committed (after snap completes).</summary>
        public IObservable<int> OnStepConfirmed => onStepConfirmed.AsObservable();

        /// <summary>Current step index (0-based).</summary>
        public int CurrentStep => currentStep;

        /// <summary>Current dial rotation in degrees.</summary>
        public float CurrentAngle => currentAngle;

        /// <summary>Number of discrete steps on the dial.</summary>
        public int NumberOfSteps => numberOfSteps;

        /// <summary>Angle between two adjacent steps in degrees.</summary>
        public float AnglePerStep => totalAngle / numberOfSteps;

        /// <summary>Normalized value (0 to 1) based on the current step.</summary>
        public float NormalizedValue => numberOfSteps > 1 ? (float)currentStep / (numberOfSteps - 1) : 0f;

        /// <summary>How the hand and dial are positioned during grab.</summary>
        public WheelGrabMode GrabMode => grabMode;

        /// <summary>Axis the dial rotates around.</summary>
        public RotationAxis RotationAxis => rotationAxis;

        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            PoseConstrainer = GetComponent<PoseConstrainter>();

            // Dial always snaps on release — the base class's return pipeline drives the snap lerp.
            returnWhenDeselected = true;

            currentStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            currentAngle = currentStep * AnglePerStep;
            _previousStep = currentStep;
            _targetSnapAngle = currentAngle;

            ApplyDialRotation();
        }

        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!IsSelected || IsReturning) return;

            var handAngle = GetHandAngle(handWorldPosition);
            var delta = Mathf.DeltaAngle(_previousHandAngle, handAngle);
            _previousHandAngle = handAngle;

            var newAngle = currentAngle + delta;

            if (wrapAround)
            {
                while (newAngle < 0f) newAngle += totalAngle;
                while (newAngle >= totalAngle) newAngle -= totalAngle;
            }
            else
            {
                newAngle = Mathf.Clamp(newAngle, 0f, totalAngle - 0.0001f);
            }

            currentAngle = newAngle;

            if (grabMode == WheelGrabMode.HandFollowsObject && _fakeHand != null)
            {
                _fakeHandOrbitAngle += delta;
                UpdateFakeHandOrbit();
            }

            ApplyDialRotation();

            int newStep = Mathf.FloorToInt(currentAngle / AnglePerStep);
            newStep = Mathf.Clamp(newStep, 0, numberOfSteps - 1);

            if (newStep != _previousStep)
            {
                currentStep = newStep;
                _previousStep = newStep;

                onStepChanged?.Invoke(currentStep);
                TryPlayStepHaptic();
            }
        }

        private float GetHandAngle(Vector3 handWorldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(handWorldPosition);

            float x, y;
            switch (rotationAxis)
            {
                case RotationAxis.Right:   x = localPos.z; y = localPos.y; break;
                case RotationAxis.Up:      x = localPos.x; y = localPos.z; break;
                case RotationAxis.Forward:
                default:                   x = localPos.x; y = localPos.y; break;
            }

            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        private void ApplyDialRotation()
        {
            Vector3 axis = GetAxis();
            Quaternion rot = Quaternion.AngleAxis(currentAngle, axis);
            interactableObject.transform.localRotation = _originalRotation * rot;
        }

        private Vector3 GetAxis()
        {
            return rotationAxis switch
            {
                RotationAxis.Right => Vector3.right,
                RotationAxis.Up => Vector3.up,
                _ => Vector3.forward
            };
        }

        private void TryPlayStepHaptic()
        {
            if (!hapticOnStep || CurrentInteractor == null) return;
            CurrentInteractor.SendHapticImpulse(hapticAmplitude, hapticDuration);
        }

        #region Grab Handling

        protected override void PositionFakeHand(Transform fakeHand, HandIdentifier handIdentifier)
        {
            _fakeHand = fakeHand;

            float grabHandAngle = GetHandAngle(CurrentInteractor.transform.position);
            _previousHandAngle = grabHandAngle;

            if (grabMode == WheelGrabMode.ObjectFollowsHand)
            {
                base.PositionFakeHand(fakeHand, handIdentifier);
            }
            else
            {
                _fakeHandOrbitAngle = grabHandAngle;
                UpdateFakeHandOrbit();
            }
        }

        private void UpdateFakeHandOrbit()
        {
            if (_fakeHand == null || PoseConstrainer == null) return;

            var basePose = PoseConstrainer.GetTargetHandTransform(CurrentInteractor.HandIdentifier);
            var orbitRadius = basePose.position.magnitude;
            float rad = _fakeHandOrbitAngle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad) * orbitRadius;
            float sin = Mathf.Sin(rad) * orbitRadius;

            Vector3 offset = rotationAxis switch
            {
                RotationAxis.Right => new Vector3(0, sin, cos),
                RotationAxis.Up => new Vector3(cos, 0, sin),
                _ => new Vector3(cos, sin, 0)
            };

            _fakeHand.position = transform.TransformPoint(offset);

            Quaternion orbitRot = Quaternion.AngleAxis(_fakeHandOrbitAngle, GetAxis());
            _fakeHand.localRotation = orbitRot * basePose.rotation;
        }

        #endregion

        #region Deselection / Snap

        protected override void HandleObjectDeselection()
        {
            _fakeHand = null;

            int nearestStep = Mathf.RoundToInt(currentAngle / AnglePerStep);
            nearestStep = wrapAround
                ? ((nearestStep % numberOfSteps) + numberOfSteps) % numberOfSteps
                : Mathf.Clamp(nearestStep, 0, numberOfSteps - 1);

            if (nearestStep != _previousStep)
            {
                currentStep = nearestStep;
                _previousStep = nearestStep;
                onStepChanged?.Invoke(currentStep);
            }

            _targetSnapAngle = nearestStep * AnglePerStep;
        }

        protected override void HandleReturnToOriginalPosition()
        {
            currentAngle = Mathf.Lerp(currentAngle, _targetSnapAngle, Time.deltaTime * returnSpeed);
            ApplyDialRotation();

            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, _targetSnapAngle)) < 0.05f)
            {
                currentAngle = _targetSnapAngle;
                ApplyDialRotation();
                IsReturning = false;

                onStepConfirmed?.Invoke(currentStep);
            }
        }

        #endregion

        #region Public API

        /// <summary>Sets the dial to a specific step immediately, firing change and confirm events.</summary>
        public void SetStep(int step)
        {
            step = Mathf.Clamp(step, 0, numberOfSteps - 1);
            currentStep = step;
            currentAngle = step * AnglePerStep;
            _previousStep = step;
            _targetSnapAngle = currentAngle;

            if (interactableObject != null) ApplyDialRotation();

            onStepChanged?.Invoke(currentStep);
            onStepConfirmed?.Invoke(currentStep);
        }

        /// <summary>Increments the dial by one step (wraps if enabled, otherwise clamps).</summary>
        public void IncrementStep()
        {
            int newStep = currentStep + 1;
            newStep = wrapAround ? newStep % numberOfSteps : Mathf.Min(newStep, numberOfSteps - 1);
            SetStep(newStep);
        }

        /// <summary>Decrements the dial by one step (wraps if enabled, otherwise clamps).</summary>
        public void DecrementStep()
        {
            int newStep = currentStep - 1;
            newStep = wrapAround ? (newStep + numberOfSteps) % numberOfSteps : Mathf.Max(newStep, 0);
            SetStep(newStep);
        }

        /// <summary>Resets the dial to its configured starting step.</summary>
        public void ResetDial() => SetStep(startingStep);

        /// <summary>Sets the dial to the step closest to a normalized value (0-1).</summary>
        public void SetNormalized(float value)
        {
            int step = Mathf.RoundToInt(value * (numberOfSteps - 1));
            SetStep(step);
        }

        #endregion

        #region Editor

        /// <summary>Returns the dial's rotation axis in world space.</summary>
        public Vector3 GetWorldAxis()
        {
            var t = interactableObject != null ? interactableObject.transform : transform;
            return rotationAxis switch
            {
                RotationAxis.Right => t.right,
                RotationAxis.Up => t.up,
                _ => t.forward
            };
        }

        private void Reset()
        {
            returnSpeed = 10f;
        }

        private void OnValidate()
        {
            numberOfSteps = Mathf.Max(2, numberOfSteps);
            totalAngle = Mathf.Max(1f, totalAngle);
            startingStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            if (returnSpeed < 1f) returnSpeed = 10f;
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        private void OnDrawGizmosSelected()
        {
            var target = interactableObject != null ? interactableObject.transform : transform;
            if (target == null) return;

            var pos = target.position;
            var axis = GetWorldAxis();

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pos, axis * 0.1f);
            Gizmos.DrawRay(pos, -axis * 0.1f);

            Vector3 reference = rotationAxis switch
            {
                RotationAxis.Right => target.forward,
                RotationAxis.Up => target.right,
                _ => target.right
            };

            float anglePerStep = totalAngle / Mathf.Max(1, numberOfSteps);
            for (int i = 0; i < numberOfSteps; i++)
            {
                float angle = i * anglePerStep;
                Gizmos.color = (Application.isPlaying && i == currentStep) ? Color.green : Color.cyan;

                var rot = Quaternion.AngleAxis(angle, axis);
                Gizmos.DrawRay(pos, rot * reference * 0.15f);
                Gizmos.DrawWireSphere(pos + rot * reference * 0.15f, 0.01f);
            }

            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                var curRot = Quaternion.AngleAxis(currentAngle, axis);
                Gizmos.DrawRay(pos, curRot * reference * 0.2f);
            }
        }

        #endregion
    }
}

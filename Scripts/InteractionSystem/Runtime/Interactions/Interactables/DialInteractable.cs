using System;
using UnityEngine;
using Shababeek.ReactiveVars;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Rotary dial with discrete steps (combination lock, selector switch, rotary phone dial).
    /// Snaps to the nearest step on release.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Dial")]
    public class DialInteractable : RotaryInteractableBase
    {
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

        private int _previousStep;
        private float _targetSnapAngle;

        /// <summary>Observable fired when the current step changes.</summary>
        public IObservable<int> OnStepChanged => onStepChanged.AsObservable();

        /// <summary>Observable fired when a step is committed (after snap completes).</summary>
        public IObservable<int> OnStepConfirmed => onStepConfirmed.AsObservable();

        /// <summary>Current step index (0-based).</summary>
        public int CurrentStep => currentStep;

        /// <summary>Number of discrete steps on the dial.</summary>
        public int NumberOfSteps => numberOfSteps;

        /// <summary>Angle between two adjacent steps in degrees.</summary>
        public float AnglePerStep => totalAngle / numberOfSteps;

        /// <summary>Normalized value (0 to 1) based on the current step.</summary>
        public float NormalizedValue => numberOfSteps > 1 ? (float)currentStep / (numberOfSteps - 1) : 0f;

        protected override void Start()
        {
            base.Start();

            // Dial always snaps on release — the base class's return pipeline drives the snap lerp.
            returnWhenDeselected = true;

            currentStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            currentAngle = currentStep * AnglePerStep;
            _previousStep = currentStep;
            _targetSnapAngle = currentAngle;

            ApplyRotation();
        }

        protected override float ProcessAngleDelta(float currentAngle, float delta)
        {
            float newAngle = currentAngle + delta;

            if (wrapAround)
            {
                while (newAngle < 0f) newAngle += totalAngle;
                while (newAngle >= totalAngle) newAngle -= totalAngle;
            }
            else
            {
                newAngle = Mathf.Clamp(newAngle, 0f, totalAngle - 0.0001f);
            }

            return newAngle;
        }

        protected override void OnAngleApplied(float newAngle)
        {
            int newStep = Mathf.Clamp(Mathf.FloorToInt(newAngle / AnglePerStep), 0, numberOfSteps - 1);

            if (newStep != _previousStep)
            {
                currentStep = newStep;
                _previousStep = newStep;

                onStepChanged?.Invoke(currentStep);
                TryPlayStepHaptic();
            }
        }

        protected override void HandleObjectDeselection()
        {
            base.HandleObjectDeselection();

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
            ApplyRotation();

            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, _targetSnapAngle)) < 0.05f)
            {
                currentAngle = _targetSnapAngle;
                ApplyRotation();
                IsReturning = false;

                onStepConfirmed?.Invoke(currentStep);
            }
        }

        /// <summary>Sets the dial to a specific step immediately, firing change and confirm events.</summary>
        public void SetStep(int step)
        {
            step = Mathf.Clamp(step, 0, numberOfSteps - 1);
            currentStep = step;
            currentAngle = step * AnglePerStep;
            _previousStep = step;
            _targetSnapAngle = currentAngle;

            if (interactableObject != null) ApplyRotation();

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

        private void TryPlayStepHaptic()
        {
            if (!hapticOnStep || CurrentInteractor == null) return;
            CurrentInteractor.SendHapticImpulse(hapticAmplitude, hapticDuration);
        }

        protected override void Reset()
        {
            base.Reset();
            returnSpeed = 10f;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
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
    }
}

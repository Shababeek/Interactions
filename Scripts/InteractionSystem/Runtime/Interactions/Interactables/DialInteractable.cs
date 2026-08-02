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

        [Tooltip("Total rotation angle covered by all steps (360 = full circle, 180 = half). " +
                 "When wrap-around is off the last step sits exactly at this angle.")]
        [SerializeField] private float totalAngle = 360f;

        [Tooltip("Angle in degrees where step 0 sits, relative to the object's authored rotation.")]
        [SerializeField] private float offsetAngle = 0f;

        [Tooltip("Allow rotating past the last step to wrap back to the first.")]
        [SerializeField] private bool wrapAround = false;

        [Header("Haptics")]
        [Tooltip("Play a haptic pulse each time a step boundary is crossed.")]
        [SerializeField] private bool hapticOnStep = true;

        [Tooltip("Haptic amplitude (0-1) when crossing a step.")]
        [SerializeField, Range(0f, 1f)] private float hapticAmplitude = 0.3f;

        [Tooltip("Haptic pulse duration in seconds when crossing a step.")]
        [SerializeField] private float hapticDuration = 0.05f;

        [Tooltip("Optional haptic pattern asset; overrides amplitude/duration when assigned.")]
        [SerializeField] private HapticPattern hapticPattern;

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

        /// <summary>
        /// Angle between two adjacent steps in degrees. A wrapping dial closes the loop, so its steps
        /// divide the sweep evenly (360/8 = 45); a clamped dial puts the last step *on* the end of the
        /// sweep, so N steps span N-1 gaps (180/7 for 8 steps).
        /// </summary>
        public float AnglePerStep => wrapAround
            ? totalAngle / numberOfSteps
            : totalAngle / Mathf.Max(1, numberOfSteps - 1);

        /// <summary>Angle of the first step — the sweep's lower bound.</summary>
        public float MinAngle => offsetAngle;

        /// <summary>Angle at the end of the sweep — where the last step sits on a clamped dial.</summary>
        public float MaxAngle => offsetAngle + totalAngle;

        /// <summary>Angle in degrees at which the given step index sits.</summary>
        public float AngleForStep(int step) => offsetAngle + step * AnglePerStep;

        /// <summary>Normalized value (0 to 1) based on the current step.</summary>
        public float NormalizedValue => numberOfSteps > 1 ? (float)currentStep / (numberOfSteps - 1) : 0f;

        protected override void Start()
        {
            base.Start();

            // Dial always snaps on release — the base class's return pipeline drives the snap lerp.
            returnWhenDeselected = true;

            currentStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            currentAngle = AngleForStep(currentStep);
            _previousStep = currentStep;
            _targetSnapAngle = currentAngle;

            ApplyRotation();
        }

        protected override float ProcessAngleDelta(float currentAngle, float delta)
        {
            float newAngle = currentAngle + delta;

            if (wrapAround)
            {
                while (newAngle < MinAngle) newAngle += totalAngle;
                while (newAngle >= MaxAngle) newAngle -= totalAngle;
            }
            else
            {
                newAngle = Mathf.Clamp(newAngle, MinAngle, MaxAngle);
            }

            return newAngle;
        }

        protected override void OnAngleApplied(float newAngle)
        {
            int newStep = StepFromAngle(newAngle);

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

            // Snap towards the raw (unwrapped) step angle so the lerp always takes the short way round;
            // the reported index is the wrapped/clamped one.
            int rawStep = Mathf.RoundToInt((currentAngle - offsetAngle) / AnglePerStep);
            _targetSnapAngle = offsetAngle + rawStep * AnglePerStep;

            int nearestStep = ClampStep(rawStep);
            if (nearestStep != _previousStep)
            {
                currentStep = nearestStep;
                _previousStep = nearestStep;
                onStepChanged?.Invoke(currentStep);
            }
        }

        protected override void HandleReturnToOriginalPosition()
        {
            currentAngle = Mathf.Lerp(currentAngle, _targetSnapAngle, Time.deltaTime * returnSpeed);
            ApplyRotation();

            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, _targetSnapAngle)) < 0.05f)
            {
                // A wrapping dial can settle a full turn past the sweep; fold it back so the
                // angle stays inside [MinAngle, MaxAngle) for the next grab.
                currentAngle = AngleForStep(currentStep);
                _targetSnapAngle = currentAngle;
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
            currentAngle = AngleForStep(step);
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

        /// <summary>Nearest step index for an angle. Rounds, so it matches where the dial snaps on release.</summary>
        private int StepFromAngle(float angle) => ClampStep(
            Mathf.RoundToInt((angle - offsetAngle) / AnglePerStep));

        private int ClampStep(int step) => wrapAround
            ? ((step % numberOfSteps) + numberOfSteps) % numberOfSteps
            : Mathf.Clamp(step, 0, numberOfSteps - 1);

        private void TryPlayStepHaptic()
        {
            if (!hapticOnStep || CurrentInteractor == null) return;
            if (hapticPattern != null)
                CurrentInteractor.PlayHapticPattern(hapticPattern);
            else
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
            totalAngle = Mathf.Clamp(totalAngle, 1f, 360f);
            offsetAngle = Mathf.Repeat(offsetAngle, 360f);
            startingStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            if (returnSpeed < 1f) returnSpeed = 10f;
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        private void OnDrawGizmosSelected()
        {
            var target = interactableObject != null ? interactableObject.transform : transform;
            if (target == null) return;

            var pos = target.position;
            var axis = GetSignedWorldAxis();

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pos, axis * 0.1f);
            Gizmos.DrawRay(pos, -axis * 0.1f);

            Vector3 reference = rotationAxis switch
            {
                RotationAxis.Right => target.forward,
                RotationAxis.Up => target.right,
                _ => target.right
            };

            for (int i = 0; i < numberOfSteps; i++)
            {
                float angle = AngleForStep(i);
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

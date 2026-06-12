using System;
using UnityEngine;
using Shababeek.ReactiveVars;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Grabbable toggle switch that rotates like a lever but settles into discrete step positions.
    /// Moves freely while held and snaps to the nearest step on release (gear selector, mode toggle,
    /// multi-position rotary switch). This is the rotary counterpart to <see cref="SliderInteractable"/>.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Toggle Switch")]
    public class ToggleSwitchInteractable : RotaryLeverBase
    {
        [Header("Steps")]
        [Tooltip("Number of discrete positions on the switch.")]
        [SerializeField, Min(2)] private int numberOfSteps = 2;

        [Tooltip("Starting step index (0-based).")]
        [SerializeField] private int startingStep = 0;

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

        [Tooltip("Fired continuously as the switch moves. Passes normalized position (0-1).")]
        [SerializeField] private FloatUnityEvent onValueChanged = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private int currentStep;

        private int _previousStep;

        /// <summary>Observable fired when the current step changes.</summary>
        public IObservable<int> OnStepChanged => onStepChanged.AsObservable();

        /// <summary>Observable fired when a step is committed (after the snap completes).</summary>
        public IObservable<int> OnStepConfirmed => onStepConfirmed.AsObservable();

        /// <summary>Observable fired continuously as the switch moves.</summary>
        public IObservable<float> OnValueChanged => onValueChanged.AsObservable();

        /// <summary>Current step index (0-based).</summary>
        public int CurrentStep => currentStep;

        /// <summary>Number of discrete steps on the switch.</summary>
        public int NumberOfSteps => numberOfSteps;

        /// <summary>Normalized value (0-1) derived from the current step.</summary>
        public float NormalizedValue => StepToNormalized(currentStep);

        protected override void Start()
        {
            base.Start();

            // Always snap on release — the base return pipeline drives the lerp toward the step angle.
            returnWhenDeselected = true;

            currentStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            _previousStep = currentStep;
            currentAngle = StepToAngle(currentStep);
            _returnTargetAngle = currentAngle;

            ApplyRotationToTransform();
            UpdateDebugValues();
        }

        protected override float ProcessAngle(float requestedAngle)
        {
            // Free movement while held; snapping happens on release.
            return requestedAngle;
        }

        protected override void OnAngleChanged()
        {
            onValueChanged?.Invoke(currentNormalizedAngle);

            int newStep = AngleToStep(currentAngle);
            if (newStep != _previousStep)
            {
                currentStep = newStep;
                _previousStep = newStep;
                onStepChanged?.Invoke(currentStep);
                TryPlayStepHaptic();
            }
        }

        protected override void OnDeselected()
        {
            int nearestStep = AngleToStep(currentAngle);

            if (nearestStep != _previousStep)
            {
                currentStep = nearestStep;
                _previousStep = nearestStep;
                onStepChanged?.Invoke(currentStep);
            }

            _returnTargetAngle = StepToAngle(nearestStep);
        }

        protected override void OnReturnComplete()
        {
            onStepConfirmed?.Invoke(currentStep);
        }

        /// <summary>Sets the switch to a specific step immediately, firing change and confirm events.</summary>
        public void SetStep(int step)
        {
            step = Mathf.Clamp(step, 0, numberOfSteps - 1);
            currentStep = step;
            _previousStep = step;
            currentAngle = StepToAngle(step);
            _returnTargetAngle = currentAngle;

            ApplyRotationToTransform();
            UpdateDebugValues();

            onValueChanged?.Invoke(currentNormalizedAngle);
            onStepChanged?.Invoke(currentStep);
            onStepConfirmed?.Invoke(currentStep);
        }

        /// <summary>Moves the switch one step forward (clamps at the last step).</summary>
        public void IncrementStep() => SetStep(Mathf.Min(currentStep + 1, numberOfSteps - 1));

        /// <summary>Moves the switch one step backward (clamps at step 0).</summary>
        public void DecrementStep() => SetStep(Mathf.Max(currentStep - 1, 0));

        /// <summary>Resets the switch to its configured starting step.</summary>
        public void ResetToStartingStep() => SetStep(startingStep);

        /// <summary>Sets the switch to the step closest to a normalized value (0-1).</summary>
        public void SetNormalizedStep(float value) => SetStep(NormalizedToStep(Mathf.Clamp01(value)));

        private float StepToAngle(int step)
        {
            return Mathf.Lerp(angleRange.x, angleRange.y, StepToNormalized(step));
        }

        private int AngleToStep(float angle)
        {
            float normalized = Mathf.InverseLerp(angleRange.x, angleRange.y, angle);
            return NormalizedToStep(normalized);
        }

        private int NormalizedToStep(float normalized)
        {
            return Mathf.Clamp(Mathf.RoundToInt(normalized * (numberOfSteps - 1)), 0, numberOfSteps - 1);
        }

        private float StepToNormalized(int step)
        {
            return numberOfSteps > 1 ? (float)step / (numberOfSteps - 1) : 0f;
        }

        private void TryPlayStepHaptic()
        {
            if (!hapticOnStep || CurrentInteractor == null) return;
            if (hapticPattern != null)
                CurrentInteractor.PlayHapticPattern(hapticPattern);
            else
                CurrentInteractor.SendHapticImpulse(hapticAmplitude, hapticDuration);
        }

        protected override void OnValidate()
        {
            if (returnSpeed < 1f) returnSpeed = 10f;
            base.OnValidate();
            numberOfSteps = Mathf.Max(2, numberOfSteps);
            startingStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (interactableObject == null) return;

            var pivot = interactableObject.transform.position;
            var (axis, normal) = GetRotationAxis();
            float radius = 0.4f;

            for (int i = 0; i < numberOfSteps; i++)
            {
                float angle = StepToAngle(i);
                var dir = Quaternion.AngleAxis(angle, axis) * normal;
                Gizmos.color = (Application.isPlaying && i == currentStep) ? Color.green : Color.cyan;
                Gizmos.DrawWireSphere(pivot + dir * radius, 0.015f);
            }
        }
    }
}

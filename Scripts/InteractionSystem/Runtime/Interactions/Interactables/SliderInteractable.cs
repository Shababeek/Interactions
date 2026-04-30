using System;
using UnityEngine;
using Shababeek.ReactiveVars;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Linear slider with discrete steps (volume control, gear selector, multi-position switch).
    /// Snaps to the nearest step on release.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Slider")]
    public class SliderInteractable : LinearInteractableBase
    {
        [Header("Steps")]
        [Tooltip("Number of discrete positions on the slider.")]
        [SerializeField, Min(2)] private int numberOfSteps = 4;

        [Tooltip("Starting step index (0-based).")]
        [SerializeField] private int startingStep = 0;

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

        [Tooltip("Fired continuously as the slider moves. Passes normalized position (0-1).")]
        [SerializeField] private FloatUnityEvent onMoved = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private int currentStep;

        private int _previousStep;
        private float _targetSnapNormalized;

        /// <summary>Observable fired when the current step changes.</summary>
        public IObservable<int> OnStepChanged => onStepChanged.AsObservable();

        /// <summary>Observable fired when a step is committed (after snap completes).</summary>
        public IObservable<int> OnStepConfirmed => onStepConfirmed.AsObservable();

        /// <summary>Observable fired continuously as the slider moves.</summary>
        public IObservable<float> OnMoved => onMoved.AsObservable();

        /// <summary>Current step index (0-based).</summary>
        public int CurrentStep => currentStep;

        /// <summary>Number of discrete steps on the slider.</summary>
        public int NumberOfSteps => numberOfSteps;

        /// <summary>Normalized value (0-1) derived from the current step.</summary>
        public float NormalizedValue => StepToNormalized(currentStep);

        protected override void Start()
        {
            base.Start();

            // Slider always snaps on release — the base class return pipeline drives the lerp.
            returnWhenDeselected = true;

            currentStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            _previousStep = currentStep;
            currentNormalized = StepToNormalized(currentStep);
            _targetSnapNormalized = currentNormalized;

            ApplyNormalized(currentNormalized);
        }

        protected override float ProcessNormalizedPosition(float current, float requested)
        {
            return requested;
        }

        protected override void OnNormalizedApplied(float newNormalized)
        {
            onMoved?.Invoke(newNormalized);

            int newStep = NormalizedToStep(newNormalized);
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
            int nearestStep = NormalizedToStep(currentNormalized);

            if (nearestStep != _previousStep)
            {
                currentStep = nearestStep;
                _previousStep = nearestStep;
                onStepChanged?.Invoke(currentStep);
            }

            _targetSnapNormalized = StepToNormalized(nearestStep);
        }

        protected override void HandleReturnToOriginalPosition()
        {
            currentNormalized = Mathf.Lerp(currentNormalized, _targetSnapNormalized, Time.deltaTime * returnSpeed);
            ApplyNormalized(currentNormalized);
            onMoved?.Invoke(currentNormalized);

            if (Mathf.Abs(currentNormalized - _targetSnapNormalized) < 0.001f)
            {
                currentNormalized = _targetSnapNormalized;
                ApplyNormalized(currentNormalized);
                IsReturning = false;

                onStepConfirmed?.Invoke(currentStep);
            }
        }

        /// <summary>Sets the slider to a specific step immediately, firing change and confirm events.</summary>
        public void SetStep(int step)
        {
            step = Mathf.Clamp(step, 0, numberOfSteps - 1);
            currentStep = step;
            _previousStep = step;
            currentNormalized = StepToNormalized(step);
            _targetSnapNormalized = currentNormalized;

            if (interactableObject != null) ApplyNormalized(currentNormalized);

            onMoved?.Invoke(currentNormalized);
            onStepChanged?.Invoke(currentStep);
            onStepConfirmed?.Invoke(currentStep);
        }

        /// <summary>Moves the slider one step forward (clamps at the last step).</summary>
        public void IncrementStep() => SetStep(Mathf.Min(currentStep + 1, numberOfSteps - 1));

        /// <summary>Moves the slider one step backward (clamps at step 0).</summary>
        public void DecrementStep() => SetStep(Mathf.Max(currentStep - 1, 0));

        /// <summary>Resets the slider to its configured starting step.</summary>
        public void ResetSlider() => SetStep(startingStep);

        /// <summary>Sets the slider to the step closest to a normalized value (0-1).</summary>
        public void SetNormalized(float value) => SetStep(NormalizedToStep(Mathf.Clamp01(value)));

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
            startingStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            if (returnSpeed < 1f) returnSpeed = 10f;
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            var worldStart = transform.TransformPoint(localStart);
            var worldEnd = transform.TransformPoint(localEnd);

            for (int i = 0; i < numberOfSteps; i++)
            {
                float t = StepToNormalized(i);
                var worldPos = Vector3.Lerp(worldStart, worldEnd, t);

                Gizmos.color = (Application.isPlaying && i == currentStep) ? Color.green : Color.cyan;
                Gizmos.DrawWireSphere(worldPos, 0.015f);
            }
        }
    }
}

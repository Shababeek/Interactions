using System;
using UnityEngine;
using Shababeek.ReactiveVars;
using Shababeek.Interactions.Core;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Linear slider with discrete steps (volume control, gear selector, multi-position switch).
    /// Mirrors DrawerInteractable's structure but snaps to fixed step positions on release.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Slider")]
    public class SliderInteractable : ConstrainedInteractableBase
    {
        [Header("Slider Settings")]
        [Tooltip("Local space position when the slider is at step 0.")]
        [SerializeField] private Vector3 localStart = Vector3.zero;

        [Tooltip("Local space position when the slider is at the last step.")]
        [SerializeField] private Vector3 localEnd = Vector3.forward;

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
        [ReadOnly, SerializeField] private float currentNormalized;

        private int _previousStep;
        private float _targetSnapNormalized;

        // Shared projection helper — mirrors the DrawerInteractable pattern.
        private static float _normalizedDistance;

        /// <summary>Observable fired when the current step changes.</summary>
        public IObservable<int> OnStepChanged => onStepChanged.AsObservable();

        /// <summary>Observable fired when a step is committed (after snap completes).</summary>
        public IObservable<int> OnStepConfirmed => onStepConfirmed.AsObservable();

        /// <summary>Observable fired continuously as the slider moves.</summary>
        public IObservable<float> OnMoved => onMoved.AsObservable();

        /// <summary>Current step index (0-based).</summary>
        public int CurrentStep => currentStep;

        /// <summary>Current normalized position (0-1).</summary>
        public float CurrentNormalized => currentNormalized;

        /// <summary>Number of discrete steps on the slider.</summary>
        public int NumberOfSteps => numberOfSteps;

        /// <summary>Normalized value (0-1) derived from the current step.</summary>
        public float NormalizedValue => numberOfSteps > 1 ? (float)currentStep / (numberOfSteps - 1) : 0f;

        public Vector3 LocalStart { get => localStart; set => localStart = value; }
        public Vector3 LocalEnd   { get => localEnd;   set => localEnd   = value; }

        private void Start()
        {
            PoseConstrainer = GetComponent<PoseConstrainter>();

            // Slider always snaps on release — the base class return pipeline drives the lerp.
            returnWhenDeselected = true;

            currentStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            _previousStep = currentStep;
            currentNormalized = StepToNormalized(currentStep);
            _targetSnapNormalized = currentNormalized;

            ApplyNormalized(currentNormalized);
        }

        protected override void HandleObjectMovement(Vector3 target)
        {
            if (!IsSelected) return;

            var localInteractorPos = transform.InverseTransformPoint(target);
            var newLocalPos = GetPositionBetweenTwoPoints(localInteractorPos, localStart, localEnd);
            currentNormalized = _normalizedDistance;

            interactableObject.transform.localPosition = newLocalPos;
            onMoved?.Invoke(currentNormalized);

            int newStep = NormalizedToStep(currentNormalized);
            if (newStep != _previousStep)
            {
                currentStep = newStep;
                _previousStep = newStep;
                onStepChanged?.Invoke(currentStep);
                TryPlayStepHaptic();
            }
        }

        private void TryPlayStepHaptic()
        {
            if (!hapticOnStep || CurrentInteractor == null) return;
            CurrentInteractor.SendHapticImpulse(hapticAmplitude, hapticDuration);
        }

        #region Deselection / Snap

        protected override void HandleObjectDeselection()
        {
            int nearestStep = Mathf.Clamp(
                Mathf.RoundToInt(currentNormalized * (numberOfSteps - 1)),
                0, numberOfSteps - 1);

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

        #endregion

        #region Public API

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
        public void IncrementStep()
        {
            SetStep(Mathf.Min(currentStep + 1, numberOfSteps - 1));
        }

        /// <summary>Moves the slider one step backward (clamps at step 0).</summary>
        public void DecrementStep()
        {
            SetStep(Mathf.Max(currentStep - 1, 0));
        }

        /// <summary>Resets the slider to its configured starting step.</summary>
        public void ResetSlider()
        {
            SetStep(startingStep);
        }

        /// <summary>Sets the slider to the step closest to a normalized value (0-1).</summary>
        public void SetNormalized(float value)
        {
            SetStep(Mathf.RoundToInt(Mathf.Clamp01(value) * (numberOfSteps - 1)));
        }

        #endregion

        #region Geometry — mirrors DrawerInteractable math exactly

        private void ApplyNormalized(float t)
        {
            if (interactableObject == null) return;
            interactableObject.transform.localPosition = Vector3.Lerp(localStart, localEnd, t);
        }

        private int NormalizedToStep(float normalized)
        {
            return Mathf.Clamp(Mathf.RoundToInt(normalized * (numberOfSteps - 1)), 0, numberOfSteps - 1);
        }

        private float StepToNormalized(int step)
        {
            return numberOfSteps > 1 ? (float)step / (numberOfSteps - 1) : 0f;
        }

        private static Vector3 GetPositionBetweenTwoPoints(Vector3 point, Vector3 start, Vector3 end)
        {
            var direction = end - start;
            var projectedPoint = Vector3.Project(point - start, direction) + start;
            _normalizedDistance = Mathf.Clamp01(FindNormalizedDistanceAlongPath(direction, projectedPoint, start));
            return Vector3.Lerp(start, end, _normalizedDistance);
        }

        private static float FindNormalizedDistanceAlongPath(Vector3 direction, Vector3 projectedPoint, Vector3 start)
        {
            var axis = GetBiggestAxis(direction);
            var x = projectedPoint[axis];
            var m = 1 / direction[axis];
            var c = 0 - m * start[axis];
            var t = m * x + c;
            return Mathf.Clamp01(t);
        }

        private static int GetBiggestAxis(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
                return Mathf.Abs(direction.x) >= Mathf.Abs(direction.z) ? 0 : 2;
            return Mathf.Abs(direction.y) >= Mathf.Abs(direction.z) ? 1 : 2;
        }

        #endregion

        #region Editor

        protected override void Reset()
        {
            base.Reset();
            returnSpeed = 10f;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            numberOfSteps = Mathf.Max(2, numberOfSteps);
            startingStep  = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            if (returnSpeed < 1f) returnSpeed = 10f;
            returnSpeed = Mathf.Clamp(returnSpeed, 1f, 20f);
        }

        private void OnDrawGizmos()
        {
            var worldStart = transform.TransformPoint(localStart);
            var worldEnd   = transform.TransformPoint(localEnd);

            // Rail
            Gizmos.color = Color.green;
            Gizmos.DrawLine(worldStart, worldEnd);

            // End caps
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(worldStart, 0.02f);
            Gizmos.DrawSphere(worldEnd,   0.02f);

            // Step positions
            for (int i = 0; i < numberOfSteps; i++)
            {
                float t = numberOfSteps > 1 ? (float)i / (numberOfSteps - 1) : 0f;
                var worldPos = Vector3.Lerp(worldStart, worldEnd, t);

                Gizmos.color = (Application.isPlaying && i == currentStep) ? Color.green : Color.cyan;
                Gizmos.DrawWireSphere(worldPos, 0.015f);
            }

            // Current handle position at runtime
            if (Application.isPlaying && interactableObject != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(interactableObject.transform.position, 0.025f);
            }
        }

        #endregion
    }
}

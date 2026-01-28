using System;
using UnityEngine;
using UnityEngine.Events;
using Shababeek.Utilities;
using Shababeek.Interactions.Core;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Unity event that passes an integer step value.
    /// </summary>
    [Serializable]
    public class IntUnityEvent : UnityEvent<int> { }

    /// <summary>
    /// Dial interactable with discrete steps (like a rotary dial, combination lock, or selector switch).
    /// Similar to WheelInteractable but snaps to fixed positions and fires events with step index.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Dial")]
    public class DialInteractable : ConstrainedInteractableBase
    {
        [Header("Dial Settings")]
        [Tooltip("How the object behaves when grabbed.")]
        [SerializeField] private WheelGrabMode grabMode = WheelGrabMode.ObjectFollowsHand;

        [Tooltip("The axis around which the dial rotates.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Forward;

        [Header("Steps")]
        [Tooltip("Number of discrete positions on the dial.")]
        [SerializeField, Min(2)] private int numberOfSteps = 8;

        [Tooltip("Starting step index (0-based).")]
        [SerializeField] private int startingStep = 0;

        [Tooltip("Allow continuous rotation past the last step to wrap to the first.")]
        [SerializeField] private bool wrapAround = false;

        [Header("Rotation")]
        [Tooltip("Total rotation angle covered by all steps (e.g., 360 for full rotation, 180 for half).")]
        [SerializeField] private float totalAngle = 360f;

        [Tooltip("Snap to the nearest step when released.")]
        [SerializeField] private bool snapOnRelease = true;

        [Tooltip("Speed of snap animation.")]
        [SerializeField] private float snapSpeed = 10f;

        [Header("Feedback")]
        [Tooltip("Play haptic feedback when passing through steps.")]
        [SerializeField] private bool hapticOnStep = true;

        [Tooltip("Haptic amplitude when passing a step.")]
        [SerializeField, Range(0, 1)] private float hapticAmplitude = 0.3f;

        [Tooltip("Haptic duration when passing a step.")]
        [SerializeField] private float hapticDuration = 0.05f;

        [Header("Events")]
        [Tooltip("Fired when the current step changes. Passes the new step index.")]
        [SerializeField] private IntUnityEvent onStepChanged = new();

        [Tooltip("Fired continuously as the dial rotates. Passes the current angle.")]
        [SerializeField] private FloatUnityEvent onAngleChanged = new();

        [Tooltip("Fired when a step is confirmed (on release with snapOnRelease, or immediately without).")]
        [SerializeField] private IntUnityEvent onStepConfirmed = new();

        [Header("Debug")]
        [ReadOnly, SerializeField] private int currentStep;
        [ReadOnly, SerializeField] private float currentAngle;

        private Quaternion _originalRotation;
        private float _previousHandAngle;
        private int _previousStep;
        private bool _isSnapping;
        private float _targetSnapAngle;

        // For HandFollowsObject mode
        private Transform _fakeHand;
        private float _fakeHandOrbitAngle;

        // Observable subjects
        private readonly Subject<int> _stepChangedSubject = new();
        private readonly Subject<float> _angleChangedSubject = new();
        private readonly Subject<int> _stepConfirmedSubject = new();

        /// <summary>Observable that fires when the step changes.</summary>
        public IObservable<int> OnStepChanged => _stepChangedSubject;

        /// <summary>Observable that fires continuously as the dial rotates.</summary>
        public IObservable<float> OnAngleChanged => _angleChangedSubject;

        /// <summary>Observable that fires when a step is confirmed.</summary>
        public IObservable<int> OnStepConfirmed => _stepConfirmedSubject;

        /// <summary>Current step index (0-based).</summary>
        public int CurrentStep => currentStep;

        /// <summary>Current rotation angle.</summary>
        public float CurrentAngle => currentAngle;

        /// <summary>Number of discrete steps.</summary>
        public int NumberOfSteps => numberOfSteps;

        /// <summary>Angle between each step.</summary>
        public float AnglePerStep => totalAngle / numberOfSteps;

        /// <summary>Normalized value (0 to 1) based on current step.</summary>
        public float NormalizedValue => (float)currentStep / (numberOfSteps - 1);

        private float MinAngle => wrapAround ? float.MinValue : 0f;
        private float MaxAngle => wrapAround ? float.MaxValue : totalAngle;

        private void Start()
        {
            _originalRotation = interactableObject.transform.localRotation;
            PoseConstrainer = GetComponent<PoseConstrainter>();

            // Set initial step
            currentStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            currentAngle = currentStep * AnglePerStep;
            _previousStep = currentStep;

            ApplyDialRotation();
        }

        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!IsSelected || IsReturning || _isSnapping) return;

            var handAngle = GetHandAngle(handWorldPosition);
            var delta = Mathf.DeltaAngle(_previousHandAngle, handAngle);
            _previousHandAngle = handAngle;

            var newAngle = currentAngle + delta;

            // Handle wrap-around or clamping
            if (wrapAround)
            {
                // Normalize angle to 0-totalAngle range for step calculation
                while (newAngle < 0) newAngle += totalAngle;
                while (newAngle >= totalAngle) newAngle -= totalAngle;
            }
            else
            {
                newAngle = Mathf.Clamp(newAngle, 0f, totalAngle - AnglePerStep * 0.001f);
            }

            currentAngle = newAngle;

            // Calculate current step
            int newStep = Mathf.FloorToInt(currentAngle / AnglePerStep);
            newStep = Mathf.Clamp(newStep, 0, numberOfSteps - 1);

            // Update fake hand for HandFollowsObject mode
            if (grabMode == WheelGrabMode.HandFollowsObject && _fakeHand != null)
            {
                _fakeHandOrbitAngle += delta;
                UpdateFakeHandOrbit();
            }

            ApplyDialRotation();

            // Fire angle changed event
            onAngleChanged?.Invoke(currentAngle);
            _angleChangedSubject.OnNext(currentAngle);

            // Check if step changed
            if (newStep != _previousStep)
            {
                currentStep = newStep;
                _previousStep = newStep;

                onStepChanged?.Invoke(currentStep);
                _stepChangedSubject.OnNext(currentStep);

                // Haptic feedback
                if (hapticOnStep && CurrentInteractor != null)
                {
                    CurrentInteractor.SendHapticImpulse(hapticAmplitude, hapticDuration);
                }

                // If not snapping on release, confirm step immediately
                if (!snapOnRelease)
                {
                    onStepConfirmed?.Invoke(currentStep);
                    _stepConfirmedSubject.OnNext(currentStep);
                }
            }
        }

        private void Update()
        {
            if (!_isSnapping) return;

            // Animate snap
            currentAngle = Mathf.Lerp(currentAngle, _targetSnapAngle, snapSpeed * Time.deltaTime);
            ApplyDialRotation();

            if (Mathf.Abs(currentAngle - _targetSnapAngle) < 0.1f)
            {
                currentAngle = _targetSnapAngle;
                ApplyDialRotation();
                _isSnapping = false;

                // Fire confirmed event after snap completes
                onStepConfirmed?.Invoke(currentStep);
                _stepConfirmedSubject.OnNext(currentStep);
            }
        }

        private float GetHandAngle(Vector3 handWorldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(handWorldPosition);

            float x, y;
            switch (rotationAxis)
            {
                case RotationAxis.Right: x = localPos.z; y = localPos.y; break;
                case RotationAxis.Up: x = localPos.x; y = localPos.z; break;
                case RotationAxis.Forward:
                default: x = localPos.x; y = localPos.y; break;
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

        #region Grab Handling

        protected override void PositionFakeHand(Transform fakeHand, HandIdentifier handIdentifier)
        {
            _fakeHand = fakeHand;

            float grabHandAngle = GetHandAngle(CurrentInteractor.transform.position);
            _previousHandAngle = grabHandAngle;

            if (grabMode == WheelGrabMode.ObjectFollowsHand)
            {
                float constraintAngle = GetConstraintAngle(handIdentifier);
                float targetDialAngle = grabHandAngle - constraintAngle;

                // Snap to nearest step on grab
                int nearestStep = Mathf.RoundToInt(targetDialAngle / AnglePerStep);
                nearestStep = Mathf.Clamp(nearestStep, 0, numberOfSteps - 1);
                currentAngle = nearestStep * AnglePerStep;
                currentStep = nearestStep;
                _previousStep = nearestStep;

                ApplyDialRotation();
                base.PositionFakeHand(fakeHand, handIdentifier);
            }
            else
            {
                _fakeHandOrbitAngle = grabHandAngle;
                UpdateFakeHandOrbit();
            }

            onStepChanged?.Invoke(currentStep);
            _stepChangedSubject.OnNext(currentStep);
        }

        private float GetConstraintAngle(HandIdentifier handIdentifier)
        {
            if (PoseConstrainer == null) return 0f;

            var positioning = PoseConstrainer.GetTargetHandTransform(handIdentifier);
            Vector3 constraintLocal = positioning.position;

            float x, y;
            switch (rotationAxis)
            {
                case RotationAxis.Right: x = constraintLocal.z; y = constraintLocal.y; break;
                case RotationAxis.Up: x = constraintLocal.x; y = constraintLocal.z; break;
                case RotationAxis.Forward:
                default: x = constraintLocal.x; y = constraintLocal.y; break;
            }

            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
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

        #region Deselection / Return

        protected override void HandleObjectDeselection()
        {
            _fakeHand = null;

            if (snapOnRelease)
            {
                // Snap to nearest step
                int nearestStep = Mathf.RoundToInt(currentAngle / AnglePerStep);
                nearestStep = Mathf.Clamp(nearestStep, 0, numberOfSteps - 1);

                if (nearestStep != currentStep)
                {
                    currentStep = nearestStep;
                    onStepChanged?.Invoke(currentStep);
                    _stepChangedSubject.OnNext(currentStep);
                }

                _targetSnapAngle = nearestStep * AnglePerStep;
                _isSnapping = true;
            }
            else
            {
                // Already confirmed during interaction
            }
        }

        protected override void HandleReturnToOriginalPosition()
        {
            // Dial doesn't return to original, it stays at current step
            IsReturning = false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the dial to a specific step immediately.
        /// </summary>
        public void SetStep(int step)
        {
            step = Mathf.Clamp(step, 0, numberOfSteps - 1);
            currentStep = step;
            currentAngle = step * AnglePerStep;
            _previousStep = step;
            ApplyDialRotation();

            onStepChanged?.Invoke(currentStep);
            _stepChangedSubject.OnNext(currentStep);
            onStepConfirmed?.Invoke(currentStep);
            _stepConfirmedSubject.OnNext(currentStep);
        }

        /// <summary>
        /// Increments the dial by one step.
        /// </summary>
        public void IncrementStep()
        {
            int newStep = currentStep + 1;
            if (wrapAround)
            {
                newStep = newStep % numberOfSteps;
            }
            else
            {
                newStep = Mathf.Min(newStep, numberOfSteps - 1);
            }
            SetStep(newStep);
        }

        /// <summary>
        /// Decrements the dial by one step.
        /// </summary>
        public void DecrementStep()
        {
            int newStep = currentStep - 1;
            if (wrapAround)
            {
                newStep = (newStep + numberOfSteps) % numberOfSteps;
            }
            else
            {
                newStep = Mathf.Max(newStep, 0);
            }
            SetStep(newStep);
        }

        /// <summary>
        /// Resets the dial to the starting step.
        /// </summary>
        public void ResetDial()
        {
            SetStep(startingStep);
        }

        /// <summary>
        /// Sets the dial to a normalized value (0-1).
        /// </summary>
        public void SetNormalized(float value)
        {
            int step = Mathf.RoundToInt(value * (numberOfSteps - 1));
            SetStep(step);
        }

        #endregion

        #region Editor

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

        private void OnValidate()
        {
            numberOfSteps = Mathf.Max(2, numberOfSteps);
            totalAngle = Mathf.Max(1f, totalAngle);
            startingStep = Mathf.Clamp(startingStep, 0, numberOfSteps - 1);
            snapSpeed = Mathf.Max(1f, snapSpeed);
        }

        private void OnDrawGizmosSelected()
        {
            var target = interactableObject != null ? interactableObject.transform : transform;
            if (target == null) return;

            var pos = target.position;
            var axis = GetWorldAxis();

            // Draw axis
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pos, axis * 0.1f);
            Gizmos.DrawRay(pos, -axis * 0.1f);

            // Draw step indicators
            var t = interactableObject != null ? interactableObject.transform : transform;
            Vector3 reference = rotationAxis switch
            {
                RotationAxis.Right => t.forward,
                RotationAxis.Up => t.right,
                _ => t.right
            };

            float anglePerStep = totalAngle / numberOfSteps;
            for (int i = 0; i < numberOfSteps; i++)
            {
                float angle = i * anglePerStep;
                Gizmos.color = (Application.isPlaying && i == currentStep) ? Color.green : Color.cyan;

                var rot = Quaternion.AngleAxis(angle, axis);
                Gizmos.DrawRay(pos, rot * reference * 0.15f);

                // Draw small sphere at step position
                Gizmos.DrawWireSphere(pos + rot * reference * 0.15f, 0.01f);
            }

            // Draw current position if playing
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

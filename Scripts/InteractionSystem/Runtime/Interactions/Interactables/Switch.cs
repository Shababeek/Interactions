using System;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Enum representing an axis in 3D space.
    /// </summary>
    public enum Axis
    {
        X,
        Y,
        Z
    }

    /// <summary>
    /// Physical two-state (on/off) switch driven by trigger collisions.
    /// The side a finger or object approaches from decides whether the switch turns on or off,
    /// then the switch body rotates to the matching angle and latches there.
    /// </summary>
    /// <remarks>
    /// Attach to an object with a trigger collider. When a non-trigger collider crosses the
    /// detection threshold on one side the switch turns on, on the other side it turns off.
    /// Re-approaching from the same side does nothing, so the switch never thrashes between states.
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Switch")]
    public class Switch : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Raised when the switch turns on.")]
        [SerializeField] private UnityEvent onTurnedOn;

        [Tooltip("Raised when the switch turns off.")]
        [SerializeField] private UnityEvent onTurnedOff;

        [Tooltip("Raised whenever the switch state changes, passing the new state (true = on).")]
        [SerializeField] private BoolUnityEvent onStateChanged = new();

        [Header("Switch Configuration")]
        [Tooltip("The transform of the switch body that rotates during interaction.")]
        [SerializeField] private Transform switchBody;

        [Tooltip("The axis around which the switch rotates.")]
        [SerializeField] private Axis rotationAxis = Axis.Z;

        [Tooltip("The axis used to detect which side the hand approaches from.")]
        [SerializeField] private Axis detectionAxis = Axis.X;

        [Tooltip("Rotation angle in degrees for the on position.")]
        [SerializeField] private float onAngle = 20f;

        [Tooltip("Rotation angle in degrees for the off position.")]
        [SerializeField] private float offAngle = -20f;

        [Tooltip("Speed of the rotation animation (higher snaps faster).")]
        [SerializeField] private float rotateSpeed = 10f;

        [Tooltip("Angle in degrees the approach must clear before the switch flips. Prevents accidental toggles near the centre.")]
        [SerializeField] private float angleThreshold = 5f;

        [Tooltip("State the switch starts in when the scene loads.")]
        [SerializeField] private bool startOn = false;

        [Header("Debug")]
        [Tooltip("Current state of the switch (read-only).")]
        [ReadOnly, SerializeField] private bool currentState;

        private float _currentAngle;
        private float _targetAngle;
        private float _animStartAngle;
        private float _animProgress = 1f;
        private Collider _activeCollider;

        /// <summary>The transform that rotates between the on and off positions.</summary>
        public Transform SwitchBody
        {
            get => switchBody;
            set => switchBody = value;
        }

        /// <summary>True when the switch is currently on.</summary>
        public bool IsOn => currentState;

        /// <summary>Fires whenever the switch state changes, passing the new state.</summary>
        public IObservable<bool> OnStateChanged => onStateChanged.AsObservable();

        private void Start()
        {
            currentState = startOn;
            _targetAngle = startOn ? onAngle : offAngle;
            _currentAngle = _targetAngle;
            _animProgress = 1f;
            ApplyAngle(_currentAngle);
        }

        private void Update()
        {
            EvaluateApproach();
            Animate();
        }

        private void EvaluateApproach()
        {
            if (!_activeCollider) return;

            var toCollider = _activeCollider.transform.position - transform.position;
            var angle = Vector3.SignedAngle(GetDetectionVector(), toCollider, GetRotationAxisVector());

            if (Mathf.Abs(angle) < angleThreshold) return;

            var desiredOn = angle > 0f;
            if (desiredOn != currentState) ApplyState(desiredOn, true);
        }

        private void Animate()
        {
            if (!switchBody) return;

            _animProgress = Mathf.Clamp01(_animProgress + Time.deltaTime * rotateSpeed);
            _currentAngle = Mathf.Lerp(_animStartAngle, _targetAngle, _animProgress);
            ApplyAngle(_currentAngle);
        }

        private void ApplyState(bool on, bool animate)
        {
            if (switchBody == null) return;

            currentState = on;
            _targetAngle = on ? onAngle : offAngle;
            _animStartAngle = animate ? _currentAngle : _targetAngle;
            _animProgress = animate ? 0f : 1f;

            if (!animate)
            {
                _currentAngle = _targetAngle;
                ApplyAngle(_currentAngle);
            }

            onStateChanged?.Invoke(on);
            if (on) onTurnedOn?.Invoke();
            else onTurnedOff?.Invoke();
        }

        private void ApplyAngle(float angle)
        {
            var euler = switchBody.localRotation.eulerAngles;
            var rotation = rotationAxis switch
            {
                Axis.X => new Vector3(angle, euler.y, euler.z),
                Axis.Y => new Vector3(euler.x, angle, euler.z),
                Axis.Z => new Vector3(euler.x, euler.y, angle),
                _ => new Vector3(euler.x, euler.y, angle)
            };
            switchBody.localRotation = Quaternion.Euler(rotation);
        }

        private Vector3 GetDetectionVector()
        {
            return detectionAxis switch
            {
                Axis.X => transform.right,
                Axis.Y => transform.up,
                Axis.Z => transform.forward,
                _ => transform.right
            };
        }

        private Vector3 GetRotationAxisVector()
        {
            return rotationAxis switch
            {
                Axis.X => transform.right,
                Axis.Y => transform.up,
                Axis.Z => transform.forward,
                _ => transform.forward
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            _activeCollider = other;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != _activeCollider) return;
            _activeCollider = null;
        }

        private void OnValidate()
        {
            if (switchBody == null) switchBody = transform;
            angleThreshold = Mathf.Abs(angleThreshold);
            rotateSpeed = Mathf.Max(0.01f, rotateSpeed);
        }

        /// <summary>
        /// Sets the switch state, animating to the matching position and raising events.
        /// </summary>
        /// <param name="on">True to turn the switch on, false to turn it off.</param>
        public void SetState(bool on) => ApplyState(on, true);

        /// <summary>
        /// Flips the switch to the opposite state.
        /// </summary>
        public void Toggle() => ApplyState(!currentState, true);

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (switchBody == null) return;
            DrawSwitchVisualization(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (switchBody == null) return;
            DrawSwitchVisualization(true);
        }

        private void DrawSwitchVisualization(bool selected)
        {
            var detectionVector = GetDetectionVector();
            var rotationAxisVector = GetRotationAxisVector();
            var position = switchBody.position;

            Gizmos.color = selected ? Color.yellow : new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawRay(position, rotationAxisVector * 0.5f);
            Gizmos.DrawRay(position, -rotationAxisVector * 0.5f);

            Gizmos.color = selected ? Color.cyan : new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawRay(position, detectionVector * 0.3f);

            if (!selected) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
            DrawAngleRay(position, rotationAxisVector, detectionVector, onAngle);

            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            DrawAngleRay(position, rotationAxisVector, detectionVector, offAngle);
        }

        private void DrawAngleRay(Vector3 center, Vector3 axis, Vector3 from, float angle)
        {
            var direction = Quaternion.AngleAxis(angle, axis) * from;
            Gizmos.DrawRay(center, direction * 0.25f);
        }
#endif
    }
}

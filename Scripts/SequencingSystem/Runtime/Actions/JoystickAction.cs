using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    public enum JoystickCondition
    {
        /// <summary>Complete when joystick reaches target position.</summary>
        ReachTarget,
        /// <summary>Complete when joystick is held at target for duration.</summary>
        HoldAtTarget,
        /// <summary>Complete when joystick enters a direction zone.</summary>
        EnterDirection,
        /// <summary>Complete when joystick is pushed to max in any direction.</summary>
        MaxDeflection,
    }

    public enum JoystickDirection
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }

    /// <summary>
    /// Completes a step based on joystick position.
    /// </summary>
    [AddComponentMenu("Shababeek/Sequencing/Actions/JoystickAction")]
    public class JoystickAction : AbstractSequenceAction
    {
        [Tooltip("The joystick to monitor.")]
        [SerializeField] private JoystickInteractable joystick;

        [Tooltip("Condition for step completion.")]
        [SerializeField] private JoystickCondition condition = JoystickCondition.ReachTarget;

        [Tooltip("Target normalized position (-1 to 1 for each axis).")]
        [SerializeField] private Vector2 targetPosition = Vector2.up;

        [Tooltip("How close to target counts as reached.")]
        [SerializeField] private float tolerance = 0.1f;

        [Tooltip("Duration to hold at target (HoldAtTarget only).")]
        [SerializeField] private float holdDuration = 1f;

        [Tooltip("Direction to detect (EnterDirection only).")]
        [SerializeField] private JoystickDirection direction = JoystickDirection.Up;

        [Tooltip("Minimum deflection to count as direction (0-1).")]
        [SerializeField] private float directionThreshold = 0.5f;

        private float _holdTime;
        private Vector2 _currentValue;

        private void Subscribe()
        {
            if (joystick == null) return;

            joystick.OnRotationChanged
                .Do(OnValueChanged)
                .Subscribe()
                .AddTo(StepDisposable);

            _holdTime = 0f;
        }

        private void OnValueChanged(Vector2 normalizedValue)
        {
            _currentValue = normalizedValue;

            switch (condition)
            {
                case JoystickCondition.ReachTarget:
                    if (Vector2.Distance(normalizedValue, targetPosition) <= tolerance)
                    {
                        CompleteStep();
                    }
                    break;

                case JoystickCondition.EnterDirection:
                    if (IsInDirection(normalizedValue))
                    {
                        CompleteStep();
                    }
                    break;

                case JoystickCondition.MaxDeflection:
                    if (normalizedValue.magnitude >= 1f - tolerance)
                    {
                        CompleteStep();
                    }
                    break;

                case JoystickCondition.HoldAtTarget:
                    // Handled in Update
                    break;
            }
        }

        private bool IsInDirection(Vector2 value)
        {
            float threshold = directionThreshold;

            return direction switch
            {
                JoystickDirection.Up => value.y >= threshold && Mathf.Abs(value.x) < threshold,
                JoystickDirection.Down => value.y <= -threshold && Mathf.Abs(value.x) < threshold,
                JoystickDirection.Left => value.x <= -threshold && Mathf.Abs(value.y) < threshold,
                JoystickDirection.Right => value.x >= threshold && Mathf.Abs(value.y) < threshold,
                JoystickDirection.UpLeft => value.y >= threshold && value.x <= -threshold,
                JoystickDirection.UpRight => value.y >= threshold && value.x >= threshold,
                JoystickDirection.DownLeft => value.y <= -threshold && value.x <= -threshold,
                JoystickDirection.DownRight => value.y <= -threshold && value.x >= threshold,
                _ => false
            };
        }

        private void Update()
        {
            if (!Started || condition != JoystickCondition.HoldAtTarget || joystick == null) return;

            bool atTarget = Vector2.Distance(_currentValue, targetPosition) <= tolerance;

            if (atTarget)
            {
                _holdTime += Time.deltaTime;
                if (_holdTime >= holdDuration)
                {
                    CompleteStep();
                }
            }
            else
            {
                _holdTime = 0f;
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                _holdTime = 0f;
                _currentValue = Vector2.zero;
                Subscribe();
            }
        }

        public float HoldProgress => holdDuration > 0 ? Mathf.Clamp01(_holdTime / holdDuration) : 0f;
    }
}

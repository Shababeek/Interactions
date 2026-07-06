using System;
using UnityEngine;
using UnityEngine.Events;
using Shababeek.ReactiveVars;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Hinged door or cabinet door that swings around a single axis between a closed and an open
    /// limit, firing open/closed events when it reaches either extreme. Set <c>returnWhenDeselected</c>
    /// for a self-closing door; leave it off for one that stays where it is released.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Door")]
    public class DoorInteractable : RotaryLeverBase
    {
        [Tooltip("Event invoked when the door reaches the open position.")]
        [SerializeField] private UnityEvent onOpened;

        [Tooltip("Event invoked when the door reaches the closed position.")]
        [SerializeField] private UnityEvent onClosed;

        [Tooltip("Event invoked continuously as the door swings, passing normalized angle (0 = closed, 1 = open).")]
        [SerializeField] private FloatUnityEvent onMoved;

        private const float LimitEpsilon = 0.02f;

        // Latches the last extreme reported so onOpened/onClosed fire once per transition
        // rather than every frame the door is held against a limit.
        private DoorState _lastState = DoorState.Between;

        private enum DoorState { Between, Open, Closed }

        /// <summary>Observable fired continuously as the door's normalized angle changes.</summary>
        public IObservable<float> OnMoved => onMoved.AsObservable();

        /// <summary>Observable fired when the door reaches the open extreme.</summary>
        public IObservable<Unit> OnOpened => onOpened.AsObservable();

        /// <summary>Observable fired when the door reaches the closed extreme.</summary>
        public IObservable<Unit> OnClosed => onClosed.AsObservable();

        /// <summary>True when the door is at (or past) its open limit.</summary>
        public bool IsOpen => currentNormalizedAngle >= 1f - LimitEpsilon;

        /// <summary>True when the door is at (or past) its closed limit.</summary>
        public bool IsClosed => currentNormalizedAngle <= LimitEpsilon;

        protected override void OnAngleChanged()
        {
            onMoved?.Invoke(currentNormalizedAngle);

            if (IsOpen)
            {
                if (_lastState != DoorState.Open)
                {
                    _lastState = DoorState.Open;
                    onOpened?.Invoke();
                }
            }
            else if (IsClosed)
            {
                if (_lastState != DoorState.Closed)
                {
                    _lastState = DoorState.Closed;
                    onClosed?.Invoke();
                }
            }
            else
            {
                _lastState = DoorState.Between;
            }
        }
    }
}

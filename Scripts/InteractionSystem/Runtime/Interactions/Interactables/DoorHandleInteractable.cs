using System;
using UnityEngine;
using UnityEngine.Events;
using Shababeek.ReactiveVars;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Lever-style door handle. Grab it and rotate it: once turned past
    /// <see cref="unlockThreshold"/> it releases the latch on its <see cref="door"/>, and the same
    /// continuous grip then drives the door's swing so you can push/pull it open without letting go.
    /// Releasing the handle re-latches the door if it has returned to closed.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Door Handle")]
    public class DoorHandleInteractable : RotaryLeverBase
    {
        [Header("Door")]
        [Tooltip("The door this handle unlocks and swings. Must be set for the handle to do anything.")]
        [SerializeField] private DoorInteractable door;

        [Tooltip("Normalized handle turn (0-1) at which the latch releases.")]
        [Range(0f, 1f)]
        [SerializeField] private float unlockThreshold = 0.6f;

        [Tooltip("Re-latch the door when the handle is released, if the door is closed at that point.")]
        [SerializeField] private bool relockWhenReleased = true;

        [Header("Events")]
        [Tooltip("Event invoked continuously as the handle turns, passing normalized angle (0-1).")]
        [SerializeField] private FloatUnityEvent onHandleTurned;

        [Tooltip("Event invoked when the handle turn passes the unlock threshold and releases the latch.")]
        [SerializeField] private UnityEvent onUnlocked;

        // Once the latch releases within a grab, the same grip stops turning the handle further and
        // instead drives the door's swing.
        private bool _drivingDoor;

        // Relative-turn reference for the handle itself, captured on grab so the handle turns by the
        // hand's delta rather than snapping to the hand's absolute bearing (leaning down on touch).
        private bool _turning;
        private float _turnRefHandAngle;
        private float _turnRefAngle;

        /// <summary>Observable fired continuously as the handle's normalized turn changes.</summary>
        public IObservable<float> OnHandleTurned => onHandleTurned.AsObservable();

        /// <summary>Observable fired when the handle releases the latch.</summary>
        public IObservable<Unit> OnUnlocked => onUnlocked.AsObservable();

        /// <summary>The door this handle controls.</summary>
        public DoorInteractable Door
        {
            get => door;
            set => door = value;
        }

        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!IsSelected || IsReturning) return;

            // Once the door is unlocked, the grip swings the door directly — whether the latch was
            // just popped this grab or the door was already open from before. Only a still-locked
            // door needs the handle turned down first.
            if (door != null && (_drivingDoor || !door.IsLocked))
            {
                if (!_drivingDoor)
                {
                    // Capture the grab reference so the door moves relative to the hand (no snap).
                    _drivingDoor = true;
                    door.BeginSwing(handWorldPosition);
                }
                door.SwingByHand(handWorldPosition);
                return;
            }

            // Turning the handle to unlock — relative, so it stays at rest on grab and only turns
            // as the hand actually rotates it down.
            if (!_turning)
            {
                _turnRefHandAngle = HandAngleAroundPivot(handWorldPosition);
                _turnRefAngle = currentAngle;
                _turning = true;
            }

            float delta = Mathf.DeltaAngle(_turnRefHandAngle, HandAngleAroundPivot(handWorldPosition));
            currentAngle = ProcessAngle(_turnRefAngle + delta);
            ApplyRotationToTransform();
            UpdateDebugValues();
            OnAngleChanged();
        }

        protected override void OnAngleChanged()
        {
            onHandleTurned?.Invoke(currentNormalizedAngle);

            // Turning the handle only releases the latch. Driving the door begins next frame in
            // HandleObjectMovement, which captures a clean grab reference first.
            if (door != null && door.IsLocked && currentNormalizedAngle >= unlockThreshold)
            {
                door.Unlock();
                onUnlocked?.Invoke();
            }
        }

        protected override void OnDeselected()
        {
            _drivingDoor = false;
            _turning = false;

            if (relockWhenReleased && door != null && door.IsClosed)
            {
                door.Lock();
            }
        }
    }
}

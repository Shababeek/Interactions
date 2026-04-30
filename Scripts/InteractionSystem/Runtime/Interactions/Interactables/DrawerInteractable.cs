using System;
using UnityEngine;
using UnityEngine.Events;
using Shababeek.ReactiveVars;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Drawer interactable that moves linearly between two local-space points,
    /// firing open/closed events when reaching the rail extremes.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Drawer")]
    public class DrawerInteractable : LinearInteractableBase
    {
        [Tooltip("Event invoked when the drawer reaches the open position.")]
        [SerializeField] private UnityEvent onOpened;

        [Tooltip("Event invoked when the drawer reaches the closed position.")]
        [SerializeField] private UnityEvent onClosed;

        [Tooltip("Event invoked continuously as the drawer moves, passing normalized position (0-1).")]
        [SerializeField] private FloatUnityEvent onMoved;

        private const float LimitEpsilon = 0.05f;
        private float _returnTimer;

        /// <summary>Observable fired continuously as the drawer's normalized position changes.</summary>
        public IObservable<float> OnMoved => onMoved.AsObservable();

        /// <summary>Observable fired when the drawer reaches the open extreme.</summary>
        public IObservable<Unit> OnOpened => onOpened.AsObservable();

        /// <summary>Observable fired when the drawer reaches the closed extreme.</summary>
        public IObservable<Unit> OnClosed => onClosed.AsObservable();

        protected override float ProcessNormalizedPosition(float current, float requested)
        {
            return requested;
        }

        protected override void OnNormalizedApplied(float newNormalized)
        {
            onMoved?.Invoke(newNormalized);

            if (newNormalized >= 1f - LimitEpsilon)
                onOpened?.Invoke();
            else if (newNormalized <= LimitEpsilon)
                onClosed?.Invoke();
        }

        protected override void HandleObjectDeselection()
        {
            _returnTimer = 0f;

            if (returnWhenDeselected) return;

            // Snap to the nearest extreme if released near one — preserves the previous behavior.
            if (currentNormalized >= 1f - LimitEpsilon)
            {
                currentNormalized = 1f;
                ApplyNormalized(currentNormalized);
                OnNormalizedApplied(currentNormalized);
            }
            else if (currentNormalized <= LimitEpsilon)
            {
                currentNormalized = 0f;
                ApplyNormalized(currentNormalized);
                OnNormalizedApplied(currentNormalized);
            }
        }

        protected override void HandleReturnToOriginalPosition()
        {
            _returnTimer += returnSpeed * Time.deltaTime;
            interactableObject.transform.localPosition = Vector3.Lerp(
                interactableObject.transform.localPosition,
                _originalPosition,
                _returnTimer
            );

            currentNormalized = ProjectLocalPositionToNormalized(interactableObject.transform.localPosition);
            onMoved?.Invoke(currentNormalized);

            if (Vector3.Distance(interactableObject.transform.localPosition, _originalPosition) < 0.001f)
            {
                IsReturning = false;
            }
        }

        private float ProjectLocalPositionToNormalized(Vector3 localPos)
        {
            Vector3 direction = localEnd - localStart;
            if (direction.sqrMagnitude < 1e-6f) return 0f;
            return Mathf.Clamp01(Vector3.Dot(localPos - localStart, direction) / direction.sqrMagnitude);
        }
    }
}

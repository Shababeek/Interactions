using System;
using UnityEngine;
using Shababeek.ReactiveVars;
using UniRx;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Free-running lever that rotates around a single axis between configurable limits,
    /// reporting its position as a normalized value (dimmer, throttle, analog control).
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Lever")]
    public class LeverInteractable : RotaryLeverBase
    {
        [Tooltip("Event invoked when the lever position changes, passing normalized angle value.")]
        [SerializeField] private FloatUnityEvent onLeverChanged = new();

        /// <summary>
        /// Observable that fires when the lever's normalized position changes.
        /// </summary>
        public IObservable<float> OnLeverChanged => onLeverChanged.AsObservable();

        protected override void OnAngleChanged()
        {
            onLeverChanged?.Invoke(currentNormalizedAngle);
        }
    }
}

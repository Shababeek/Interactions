using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions
{
    /// <summary>
    /// VR button component that provides physical button interaction with visual feedback.
    /// Handles trigger-based activation, button press animations, and click events
    /// with configurable press depth and cooldown periods.
    /// </summary>
    /// <remarks>
    /// This component creates a physical button that can be pressed by VR controllers.
    /// It provides smooth press animations and prevents rapid-fire clicking through cooldown.
    /// The button raises events for click, button down, and button up actions with
    /// corresponding UniRx observables for reactive programming.
    /// </remarks>
    [AddComponentMenu(menuName : "Shababeek/Interactions/Interactables/VRButton")]
    public class VRButton : MonoBehaviour
    {
        [Tooltip("Event raised when the button is clicked.")]
        [SerializeField] private UnityEvent onClick;
        
        [Tooltip("Event raised when the button is pressed down.")]
        [SerializeField] private UnityEvent onButtonDown;
        
        [Tooltip("Event raised when the button is released.")]
        [SerializeField] private UnityEvent onButtonUp;
        
        [Tooltip("The transform of the button visual element that moves during press.")]
        [SerializeField] private Transform button;
        
        [Tooltip("The normal (unpressed) position of the button.")]
        [SerializeField] private Vector3 normalPosition = new Vector3(0, .5f, 0);
        
        [Tooltip("The pressed position of the button (how far it moves when pressed).")]
        [SerializeField] private Vector3 pressedPosition = new Vector3(0, .2f, 0);
        
        [Tooltip("Indicates whether the button is currently in a clicked state.")]
        [SerializeField] private bool isClicked;
        
        [Tooltip("Speed of the button press animation.")]
        [SerializeField] private float pressSpeed = 10;
        
        [Tooltip("Cooldown time between button clicks to prevent rapid-fire activation.")]
        [SerializeField] private float coolDownTime = .2f;
        
        private float _coolDownTimer = 0;
        private float t = 0;
        
        /// <summary>
        /// Observable that fires when the button is clicked.
        /// </summary>
        /// <value>An observable that emits a Unit when the button is activated.</value>
        public IObservable<Unit> OnClick => onClick.AsObservable();
        
        /// <summary>
        /// Observable that fires when the button is pressed down.
        /// </summary>
        /// <value>An observable that emits a Unit when the button is pressed down.</value>
        public IObservable<Unit> OnButtonDown => onButtonDown.AsObservable();
        
        /// <summary>
        /// Observable that fires when the button is released.
        /// </summary>
        /// <value>An observable that emits a Unit when the button is released.</value>
        public IObservable<Unit> OnButtonUp => onButtonUp.AsObservable();

        /// <summary>
        /// Gets or sets the button transform that moves during press animations.
        /// </summary>
        /// <value>The transform of the button visual element.</value>
        public Transform Button
        {
            get => button;
            set => button = value;
        }

        private void Update()
        {
            _coolDownTimer += Time.deltaTime;
            t += (isClicked ? Time.deltaTime : -Time.deltaTime) * pressSpeed;
            t = Mathf.Clamp01(t);
            button.transform.localPosition = Vector3.Lerp(normalPosition, pressedPosition, t);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(_coolDownTimer < coolDownTime)return;
            if (other.isTrigger || isClicked) return;
            _coolDownTimer = 0;
            onButtonDown.Invoke();
            isClicked = true;
        }
        private void OnTriggerExit(Collider other)
        {
            if (!isClicked ||other.isTrigger) return;
            onButtonUp.Invoke();
            onClick.Invoke();

            isClicked = false;
        }
    }
}
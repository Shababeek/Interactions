using System;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Toggle button that stays pressed until pressed again.
    /// Unlike VRButton which releases on trigger exit, VRToggleButton latches
    /// in the pressed state and only releases on a second press after cooldown.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/VRToggleButton")]
    public class VRToggleButton : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Event raised each time the toggle state changes.")]
        [SerializeField] private UnityEvent onToggled;

        [Tooltip("Event raised when the button is toggled on (pressed down).")]
        [SerializeField] private UnityEvent onToggledOn;

        [Tooltip("Event raised when the button is toggled off (released up).")]
        [SerializeField] private UnityEvent onToggledOff;

        [Header("Button Configuration")]
        [Tooltip("The transform of the button visual that moves during press animation.")]
        [SerializeField] private Transform button;

        [Tooltip("The button position when in the up (off) state.")]
        [SerializeField] private Vector3 normalPosition = new Vector3(0, .5f, 0);

        [Tooltip("The button position when in the down (on) state.")]
        [SerializeField] private Vector3 pressedPosition = new Vector3(0, .2f, 0);

        [Tooltip("Speed of the press/release animation.")]
        [SerializeField] private float pressSpeed = 10;

        [Tooltip("Cooldown in seconds before the button can be toggled again.")]
        [SerializeField] private float coolDownTime = .4f;

        [Tooltip("Only filter colliders whose name contains this string. Leave empty to accept all.")]
        [SerializeField] private string maskName = "tip";

        [Header("State")]
        [Tooltip("When false the button ignores all input and cannot be toggled.")]
        [SerializeField] private bool active = true;

        [Tooltip("Current toggle state — true means the button is pressed down.")]
        [ReadOnly] [SerializeField] private bool isToggledOn;

        private float _coolDownTimer;
        private float _animationT;

        /// <summary>
        /// Observable that fires every time the toggle state changes.
        /// </summary>
        public IObservable<Unit> OnToggled => onToggled.AsObservable();

        /// <summary>
        /// Observable that fires when the button is toggled on.
        /// </summary>
        public IObservable<Unit> OnToggledOn => onToggledOn.AsObservable();

        /// <summary>
        /// Observable that fires when the button is toggled off.
        /// </summary>
        public IObservable<Unit> OnToggledOff => onToggledOff.AsObservable();

        /// <summary>
        /// Whether the button is currently in the pressed (on) state.
        /// </summary>
        public bool IsToggledOn => isToggledOn;

        /// <summary>
        /// Controls whether the button accepts input. When false the button cannot be toggled.
        /// </summary>
        public bool Active
        {
            get => active;
            set => active = value;
        }

        /// <summary>
        /// Gets or sets the button visual transform.
        /// </summary>
        public Transform Button
        {
            get => button;
            set => button = value;
        }

        void Awake()
        {
            if (button == null)
            {
                button = transform.GetChild(0);
            }

            _animationT = isToggledOn ? 1f : 0f;
            _coolDownTimer = coolDownTime;
        }

        private void Update()
        {
            _coolDownTimer += Time.deltaTime;
            AnimateButton();
        }

        private void AnimateButton()
        {
            _animationT += (isToggledOn ? Time.deltaTime : -Time.deltaTime) * pressSpeed;
            _animationT = Mathf.Clamp01(_animationT);
            button.localPosition = Vector3.Lerp(normalPosition, pressedPosition, _animationT);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled) return;
            if (!active) return;
            if (maskName != "" && !other.gameObject.name.Contains(maskName)) return;
            if (_coolDownTimer < coolDownTime) return;

            _coolDownTimer = 0;
            isToggledOn = !isToggledOn;

            onToggled.Invoke();

            if (isToggledOn)
                onToggledOn.Invoke();
            else
                onToggledOff.Invoke();
        }

        /// <summary>
        /// Sets the toggle state without triggering events.
        /// </summary>
        /// <param name="on">Desired toggle state.</param>
        public void SetStateWithoutNotify(bool on)
        {
            isToggledOn = on;
        }

        /// <summary>
        /// Sets the toggle state and triggers the appropriate events.
        /// </summary>
        /// <param name="on">Desired toggle state.</param>
        public void SetState(bool on)
        {
            if (isToggledOn == on) return;

            isToggledOn = on;
            onToggled.Invoke();

            if (isToggledOn)
                onToggledOn.Invoke();
            else
                onToggledOff.Invoke();
        }
    }
}

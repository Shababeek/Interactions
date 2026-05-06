using System;
using Shababeek.ReactiveVars;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Determines how the button visually represents its inactive state.
    /// </summary>
    public enum ButtonInactiveMode
    {
        /// <summary>Swaps between two GameObjects for active/inactive visuals.</summary>
        SwapGameObjects,
        /// <summary>Sets an Animator bool to toggle active/inactive visuals.</summary>
        AnimationBool,
        /// <summary>Changes the button height to indicate inactive state.</summary>
        HeightChange
    }

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
    [AddComponentMenu(menuName: "Shababeek/Interactions/Interactables/VRButton")]
    public class VRButton : MonoBehaviour
    {
        [Tooltip("Event invoked when the button is clicked (pressed and released).")]
        [SerializeField] private UnityEvent onClick = new();

        [Tooltip("Event invoked when the button is pressed down.")]
        [SerializeField] private UnityEvent onButtonDown = new();

        [Tooltip("Event invoked when the button is released.")]
        [SerializeField] private UnityEvent onButtonUp = new();

        [Tooltip("The transform of the button visual element that moves during press.")]
        [SerializeField] private Transform button;

        [Tooltip("Position of the button when not pressed.")]
        [SerializeField] private Vector3 normalPosition = new Vector3(0, .5f, 0);

        [Tooltip("Position of the button when fully pressed.")]
        [SerializeField] private Vector3 pressedPosition = new Vector3(0, .2f, 0);


        [Tooltip("Speed of button press animation.")]
        [SerializeField] private float pressSpeed = 10;

        [Tooltip("Minimum time in seconds between successive button presses.")]
        [SerializeField] private float coolDownTime = .2f;

        [Tooltip("Name substring to identify the pressing collider.")]
        [SerializeField] private string maskName = "tip";

        [Header("Label")]
        [Tooltip("Text mesh used to display the button label.")]
        [SerializeField] private TMP_Text labelText;

        [Header("Inactive Mode")]
        [Tooltip("How the button represents its inactive state visually.")]
        [SerializeField] private ButtonInactiveMode inactiveMode = ButtonInactiveMode.SwapGameObjects;

        [Tooltip("GameObject shown when the button is active (SwapGameObjects mode).")]
        [SerializeField] private GameObject activeVisual;

        [Tooltip("GameObject shown when the button is inactive (SwapGameObjects mode).")]
        [SerializeField] private GameObject inactiveVisual;

        [Tooltip("Animator used for active/inactive transitions (AnimationBool mode).")]
        [SerializeField] private Animator buttonAnimator;

        [Tooltip("Animator bool parameter name for active state (AnimationBool mode).")]
        [SerializeField] private string activeBoolName = "Active";

        [Tooltip("Button height when inactive (HeightChange mode).")]
        [SerializeField] private float inactiveHeight = 0.05f;

        [ReadOnly][SerializeField] private bool isDown;

        private float _coolDownTimer = 0;
        private float _t = 0;
        private bool _isActive = true;
        private Vector3 _originalScale;

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

        /// <summary>
        /// Gets or sets the button label text.
        /// </summary>
        public string Label
        {
            get => labelText != null ? labelText.text : string.Empty;
            set { if (labelText != null) labelText.text = value; }
        }

        /// <summary>
        /// Gets or sets whether the button is interactable.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetActive(value);
        }

        void Awake()
        {
            if (button == null)
            {
                button = transform.GetChild(0);
            }
            _originalScale = transform.localScale;
        }

        /// <summary>
        /// Activates or deactivates the button using the configured inactive mode.
        /// </summary>
        public void SetActive(bool active)
        {
            _isActive = active;
            enabled = active;

            switch (inactiveMode)
            {
                case ButtonInactiveMode.SwapGameObjects:
                    if (activeVisual != null) activeVisual.SetActive(active);
                    if (inactiveVisual != null) inactiveVisual.SetActive(!active);
                    break;

                case ButtonInactiveMode.AnimationBool:
                    if (buttonAnimator != null) buttonAnimator.SetBool(activeBoolName, active);
                    break;

                case ButtonInactiveMode.HeightChange:
                    var scale = _originalScale;
                    if (!active) scale.y = inactiveHeight;
                    transform.localScale = scale;
                    break;
            }
        }

        private void Update()
        {
            _coolDownTimer += Time.deltaTime;
            _t += (isDown ? Time.deltaTime : -Time.deltaTime) * pressSpeed;
            _t = Mathf.Clamp01(_t);
            button.transform.localPosition = Vector3.Lerp(normalPosition, pressedPosition, _t);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled) return;
            if (maskName!="" && !other.gameObject.name .Contains( maskName)) return;
            if (_coolDownTimer < coolDownTime) return;
            if (isDown) return;
            _coolDownTimer = 0;
            onButtonDown.Invoke();
            isDown = true;
        }
        private void OnTriggerExit(Collider other)
        {
            if (!isDown) return;
            onButtonUp.Invoke();
            onClick.Invoke();

            isDown = false;
        }
    }
}
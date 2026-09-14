using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

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
    [AddComponentMenu(menuName: "Shababeek/Interactions/Interactables/VRButton")]
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

        [Tooltip("How long the finger must stay clear before the press is released. Absorbs the tracking jitter that would otherwise flicker one press into a dozen.")]
        [SerializeField] private float releaseGrace = .25f;

        [Tooltip("Layers allowed to press this button. Leave it empty and it resolves to the XRI interactor layers on startup.")]
        [SerializeField] private LayerMask pressableBy;

        [Tooltip("Logs every collider that enters or leaves this button, and why it was accepted or rejected. Turn off once the button is behaving.")]
        [SerializeField] private bool debugLog;

        /// <summary>
        /// Layers used when the inspector field is left empty.
        /// </summary>
        /// <remarks>
        /// A hand is a stack of bone capsules that all share one layer and one name, so a button
        /// cannot usefully tell them apart - and does not need to. Anything on an interactor layer
        /// is a hand touching the button, which is exactly what a press is.
        /// </remarks>
        private static readonly string[] DefaultPressableLayers = { "XRI_LeftInteractor", "XRI_RightInteractor" };

        private float _coolDownTimer = 0;
        private float t = 0;
        private readonly System.Collections.Generic.HashSet<Collider> _inside = new();
        private float _releaseTimer = -1f;

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
        void Awake()
        {
            if (button == null)
            {
                button = transform.GetChild(0);
            }

            if (pressableBy == 0 || IsEveryLayer(pressableBy)) pressableBy = ResolveDefaultLayers();

            WarnIfUnpressable();
            if (debugLog) LogSetup();
        }

        /// <summary>
        /// Builds the fallback layer mask from <see cref="DefaultPressableLayers"/>.
        /// </summary>
        /// <remarks>
        /// Buttons authored before this field existed serialize it as zero, which would otherwise
        /// mean "nothing may press me" and silently brick every console in the project.
        /// </remarks>
        /// <summary>
        /// Whether a mask accepts every physics layer, which is the same as accepting none of them.
        /// </summary>
        /// <remarks>
        /// Unity fills a newly added LayerMask on an already-serialized component with Everything,
        /// not with the field initializer. A button that accepts every layer accepts the crane, the
        /// room and its own interactable layer, so treat it as unconfigured rather than deliberate.
        /// </remarks>
        private static bool IsEveryLayer(LayerMask mask) => mask.value == ~0;

        private LayerMask ResolveDefaultLayers()
        {
            var mask = 0;
            foreach (var layerName in DefaultPressableLayers)
            {
                var layer = LayerMask.NameToLayer(layerName);
                if (layer >= 0) mask |= 1 << layer;
            }

            if (mask == 0)
                Debug.LogWarning(
                    $"[VRButton] {name} has no pressable layers set and none of " +
                    $"({string.Join(", ", DefaultPressableLayers)}) exist in this project, so nothing " +
                    "can press it. Set the Pressable By mask in the inspector.", this);

            return mask;
        }

        /// <summary>
        /// Warns when this button cannot receive trigger callbacks at all.
        /// </summary>
        /// <remarks>
        /// Unity only raises OnTriggerEnter when one collider in the pair belongs to a Rigidbody.
        /// Two static triggers touching produce nothing and no error, so a button set up this way is
        /// silently unpressable - worth saying out loud rather than leaving to a debug session.
        /// </remarks>
        private void WarnIfUnpressable()
        {
            if (GetComponentInParent<Rigidbody>()) return;
            Debug.LogWarning(
                $"[VRButton] {name} has no Rigidbody on it or any parent. Unless the hand collider " +
                "that touches it has one, Unity will never raise a trigger event and this button " +
                "can never be pressed. Add a kinematic Rigidbody to fix it.", this);
        }

        /// <summary>
        /// Reports the physics setup this button depends on.
        /// </summary>
        /// <remarks>
        /// A trigger only fires when one side of the pair carries a Rigidbody and the two layers
        /// collide in the physics matrix. Both are invisible from the inspector, and both silently
        /// produce a button that can never be pressed, so they are worth stating up front.
        /// </remarks>
        private void LogSetup()
        {
            var myCollider = GetComponent<Collider>();
            var myBody = GetComponentInParent<Rigidbody>();
            Debug.Log(
                $"[VRButton] {name} ready. layer={LayerMask.LayerToName(gameObject.layer)}({gameObject.layer}) " +
                $"collider={(myCollider ? myCollider.GetType().Name : "MISSING")} " +
                $"isTrigger={(myCollider && myCollider.isTrigger)} " +
                $"rigidbodyInParents={(myBody ? myBody.name : "none - the finger must supply one")} " +
                $"pressableBy={DescribeMask(pressableBy)} coolDown={coolDownTime}s", this);
        }

        private void Update()
        {
            _coolDownTimer += Time.deltaTime;

            // A hand that is destroyed or disabled inside the volume never sends OnTriggerExit,
            // and a button stuck down forever is worse than one that releases a frame late.
            if (_inside.Count > 0) _inside.RemoveWhere(c => !c || !c.enabled || !c.gameObject.activeInHierarchy);
            if (_inside.Count == 0 && isClicked && _releaseTimer < 0) _releaseTimer = releaseGrace;

            if (_releaseTimer >= 0)
            {
                _releaseTimer -= Time.deltaTime;
                if (_releaseTimer <= 0) CompleteRelease();
            }

            t += (isClicked ? Time.deltaTime : -Time.deltaTime) * pressSpeed;
            t = Mathf.Clamp01(t);
            button.transform.localPosition = Vector3.Lerp(normalPosition, pressedPosition, t);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled) { Reject(other, "the component is disabled"); return; }
            if (!Matches(other)) { Reject(other, $"its layer is not in this button's pressable layers ({DescribeMask(pressableBy)})"); return; }

            // A hand is a stack of bone capsules, and they cross the button face milliseconds apart.
            // Counting how many are inside - rather than remembering one of them - means the button
            // stays down for the whole poke and fires once, instead of once per capsule.
            var wasEmpty = _inside.Count == 0;
            _inside.Add(other);

            if (!wasEmpty) { Reject(other, "another finger collider is already holding the button down"); return; }

            // The hand came back before the grace period expired: that was jitter across the button
            // face, not a new poke, so resume the press already in progress.
            if (_releaseTimer >= 0)
            {
                _releaseTimer = -1f;
                Reject(other, "it never really left - the release was still within the grace period");
                return;
            }

            if (_coolDownTimer < coolDownTime)
            {
                Reject(other, $"still cooling down ({_coolDownTimer:0.00}s of {coolDownTime:0.00}s)");
                return;
            }
            if (isClicked) { Reject(other, "the button is already held down"); return; }

            _coolDownTimer = 0;
            isClicked = true;
            if (debugLog) Debug.Log($"[VRButton] {name} PRESSED by '{other.name}'.", this);
            onButtonDown.Invoke();

            // Fired on press, not on release: a physical button acts the moment it bottoms out,
            // and waiting for the finger to withdraw made every press feel dead.
            onClick.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_inside.Remove(other)) return;
            if (_inside.Count > 0) return;
            if (!isClicked) return;

            if (releaseGrace > 0)
            {
                // Held open rather than released outright: see the jitter note in OnTriggerEnter.
                _releaseTimer = releaseGrace;
                return;
            }

            CompleteRelease();
        }

        /// <summary>
        /// Finishes a release once the hand has stayed clear for the whole grace period.
        /// </summary>
        /// <remarks>
        /// The cooldown starts here rather than at the press, so the quiet time is measured from
        /// when the hand actually withdrew. Measuring it from the press let a press-release-press
        /// cycle slip through in a fifth of a second and toggle the switch straight back.
        /// </remarks>
        private void CompleteRelease()
        {
            _releaseTimer = -1f;
            isClicked = false;
            _coolDownTimer = 0;
            if (debugLog) Debug.Log($"[VRButton] {name} released.", this);
            onButtonUp.Invoke();
        }

        /// <summary>
        /// Notes a collider that touched the button but did not press it, and why.
        /// </summary>
        /// <remarks>
        /// Silence in the console means physics never delivered the touch at all, which is a
        /// different fault entirely from a touch that arrived and was filtered out.
        /// </remarks>
        private void Reject(Collider other, string reason)
        {
            if (!debugLog) return;
            Debug.Log(
                $"[VRButton] {name} ignored '{other.name}' " +
                $"(layer {LayerMask.LayerToName(other.gameObject.layer)}) because {reason}.", this);
        }

        private bool Matches(Collider other) =>
            (pressableBy.value & (1 << other.gameObject.layer)) != 0;

        /// <summary>Names the layers in a mask, for logs that would otherwise print a bare integer.</summary>
        private static string DescribeMask(LayerMask mask)
        {
            var names = new System.Collections.Generic.List<string>();
            for (var layer = 0; layer < 32; layer++)
            {
                if ((mask.value & (1 << layer)) == 0) continue;
                var layerName = LayerMask.LayerToName(layer);
                names.Add(string.IsNullOrEmpty(layerName) ? layer.ToString() : layerName);
            }

            return names.Count == 0 ? "nothing" : string.Join("|", names);
        }
    }
}
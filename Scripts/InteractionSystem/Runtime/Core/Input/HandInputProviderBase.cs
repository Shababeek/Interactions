using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Base implementation of IHandInputProvider with common functionality for button observables and finger tracking.
    /// Provides events for when the provider becomes active or inactive, enabling event-based switching.
    /// </summary>
    public abstract class HandInputProviderBase : MonoBehaviour, IHandInputProvider
    {
        [Header("Configuration")]
        [Tooltip("Which hand this provider is reading input for.")]
        [SerializeField] protected HandIdentifier handedness = HandIdentifier.Left;

        [Tooltip("Priority of this provider (higher = preferred when multiple providers available).")]
        [SerializeField] protected int priority = 0;

        [Header("Button Thresholds")]
        [Tooltip("Finger curl value threshold for trigger button press detection.")]
        [SerializeField] protected float triggerThreshold = 0.2f;

        [Tooltip("Finger curl value threshold for grip button press detection.")]
        [SerializeField] protected float gripThreshold = 0.2f;

        [Tooltip(
            "Thumb value threshold for thumb button press detection. Under controller tracking " +
            "the thumb action is a button and this only has to sit above zero; under hand " +
            "tracking it is a real curl, so raise it if a resting thumb reads as a press.")]
        [SerializeField] protected float thumbThreshold = 0.5f;

        private readonly ButtonObservable _triggerObserver = new();
        private readonly ButtonObservable _gripObserver = new();
        private readonly ButtonObservable _aButtonObserver = new();
        private readonly ButtonObservable _bButtonObserver = new();
        private readonly float[] _fingers = new float[5];

        private bool _wasActive = false;

        /// <summary>
        /// Event raised when this provider becomes active.
        /// </summary>
        public event Action OnProviderActivated;

        /// <summary>
        /// Event raised when this provider becomes inactive.
        /// </summary>
        public event Action OnProviderDeactivated;

        /// <summary>
        /// Observable for trigger button state changes.
        /// </summary>
        public IObservable<VRButtonState> TriggerObservable => _triggerObserver.OnStateChanged;

        /// <summary>
        /// Observable for grip button state changes.
        /// </summary>
        public IObservable<VRButtonState> GripObservable => _gripObserver.OnStateChanged;

        /// <summary>
        /// Observable for A button state changes.
        /// </summary>
        public IObservable<VRButtonState> AButtonObservable => _aButtonObserver.OnStateChanged;

        /// <summary>
        /// Observable for B button state changes.
        /// </summary>
        public IObservable<VRButtonState> BButtonObservable => _bButtonObserver.OnStateChanged;

        /// <summary>
        /// The A button observer. Protected so derived providers that read a real face button
        /// (<see cref="ControllerInputProvider"/>) can drive it directly instead of through the
        /// thumb-curl fallback. Writing the same state twice in a frame is a no-op — the
        /// observable only fires on changes — but writing two DIFFERENT states in one frame is
        /// not: the losing write dispatches a press or release the winner immediately retracts,
        /// and subscribers see a phantom event. A provider that takes over A must therefore also
        /// suppress the fallback via <see cref="DriveAButtonFromThumbCurl"/>, not merely
        /// overwrite its result afterwards.
        /// </summary>
        protected ButtonObservable AButtonObserver => _aButtonObserver;

        /// <summary>
        /// The B button observer, for the same direct-drive purpose as
        /// <see cref="AButtonObserver"/>. The base never writes it — there is no curl to
        /// approximate B with — so any provider that wants B events must drive it from a real
        /// secondary-button source.
        /// </summary>
        protected ButtonObservable BButtonObserver => _bButtonObserver;

        /// <summary>
        /// Whether <see cref="UpdateButtonStates"/> drives the A observer from the thumb value
        /// crossing <see cref="thumbThreshold"/>. The default is true because that fallback is
        /// the ONLY source of A events for hand tracking: a tracked hand has no face buttons,
        /// so a thumb curl is the closest thing to an A press it can offer. Controller
        /// providers override this to false when a dedicated A action is wired, because the
        /// thumb action still binds the B button and the thumbstick touch, and either of those
        /// would otherwise fire a phantom A press that the real button then retracts.
        /// </summary>
        protected virtual bool DriveAButtonFromThumbCurl => true;

        /// <summary>
        /// Current thumbstick axis of this hand's controller, or zero when the input source
        /// has no stick. Derived providers that do have one write it every frame from
        /// <see cref="UpdateFingerValues"/>; nobody else writes it, so a source without a
        /// stick simply leaves the default standing.
        /// </summary>
        public Vector2 Thumbstick { get; protected set; }

        /// <summary>
        /// Gets finger curl value by index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).
        /// </summary>
        public float this[int fingerIndex]
        {
            get => fingerIndex is >= 0 and < 5 ? _fingers[fingerIndex] : 0f;
            private set
            {
                if (fingerIndex is >= 0 and < 5)
                    _fingers[fingerIndex] = Mathf.Clamp01(value);
            }
        }

        /// <summary>
        /// Gets finger curl value by finger name.
        /// </summary>
        public float this[FingerName finger]
        {
            get => this[(int)finger];
            protected set => this[(int)finger] = value;
        }


        /// <summary>
        /// Priority of this provider (higher values take precedence).
        /// </summary>
        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        /// <summary>
        /// Which hand this provider is reading.
        /// </summary>
        public HandIdentifier Handedness
        {
            get => handedness;
            set => handedness = value;
        }
        ///<inheritdoc/>
        public virtual Vector3 Position => Vector3.zero;

        ///<inheritdoc/>
        public virtual Quaternion Rotation => Quaternion.identity;

        ///<inheritdoc/>
        public virtual uint TrackingState => 0;

        protected virtual void OnEnable()
        {
            // Override in derived classes for initialization
        }

        protected virtual void OnDisable()
        {
            // Override in derived classes for cleanup

            // Deactivate if we were active
            if (_wasActive)
            {
                _wasActive = false;
                OnProviderDeactivated?.Invoke();
            }
        }

        protected virtual void Update()
        {
                    // Update finger values from the specific input source
            UpdateFingerValues();

            // Update button states based on finger values
            UpdateButtonStates();
        }

        /// <summary>
        /// Override this to read finger values from your specific input source.
        /// Set finger values using: this[fingerIndex] = value;
        /// </summary>
        protected abstract void UpdateFingerValues();

        /// <summary>
        /// Updates button observers based on current finger curl values.
        /// Can be overridden for custom button detection logic.
        /// </summary>
        protected virtual void UpdateButtonStates()
        {
            // Trigger = index finger curl
            _triggerObserver.ButtonState = this[FingerName.Index] > triggerThreshold;

            // Grip = average of middle, ring, pinky
            float gripValue = (this[FingerName.Middle] + this[FingerName.Ring] + this[FingerName.Pinky]) / 3f;
            _gripObserver.ButtonState = gripValue > gripThreshold;

            // Thumb = the thumb value, whatever the tracking type made of it: a face button under
            // controllers, a curl under hand tracking. Without this the A and B observers are
            // constructed, exposed as AButtonObservable/BButtonObservable, and never written to —
            // which silently kills ThumbButtonObservable, since that is the merge of the two, and
            // with it every interactable's onThumbPressed and ThumbPressed override.
            //
            // Only the A observer is driven. The merge would deliver a press once per observer, so
            // feeding both would double every thumb event rather than distinguish the buttons.
            //
            // This write is the HAND-TRACKING fallback and stays the base's job, but it is
            // guarded: providers that read a real primary button (ControllerInputProvider with a
            // wired AButton action) suppress it via DriveAButtonFromThumbCurl, because the thumb
            // value under controllers also moves for the B button and the thumbstick touch — and
            // an unguarded fallback write followed by a correction would dispatch a phantom A
            // press on both. ButtonObservable fires inside the setter, synchronously, so a wrong
            // write cannot be taken back by overwriting it afterwards.
            if (DriveAButtonFromThumbCurl)
            {
                _aButtonObserver.ButtonState = this[FingerName.Thumb] > thumbThreshold;
            }
        }

        /// <summary>
        /// Helper method to set all finger values at once.
        /// </summary>
        protected void SetFingerValues(float thumb, float index, float middle, float ring, float pinky)
        {
            this[0] = thumb;
            this[1] = index;
            this[2] = middle;
            this[3] = ring;
            this[4] = pinky;
        }

        /// <summary>
        /// Manually trigger activation event (useful for event-based providers).
        /// </summary>
        protected void TriggerActivation()
        {
            if (!_wasActive)
            {
                _wasActive = true;
                OnProviderActivated?.Invoke();
            }
        }

        /// <summary>
        /// Manually trigger deactivation event (useful for event-based providers).
        /// </summary>
        protected void TriggerDeactivation()
        {
            if (_wasActive)
            {
                _wasActive = false;
                OnProviderDeactivated?.Invoke();
            }
        }
        
    }
}

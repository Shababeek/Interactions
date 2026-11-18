#if XR_HANDS_AVAILABLE
using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Hand input provider for the "Both" input type. Owns a controller provider and a
    /// hand-tracking provider for one hand and forwards to hand tracking while that hand is
    /// tracked, otherwise to the controller.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Input Providers/Auto Switch Input Provider")]
    public class AutoSwitchHandInputProvider : MonoBehaviour, IHandInputProvider
    {
        private const int ButtonCount = 4;
        private const int Trigger = 0, Grip = 1, AButton = 2, BButton = 3;

        [Tooltip("Which hand this provider is reading input for.")]
        [SerializeField] private HandIdentifier handedness = HandIdentifier.Left;

        private readonly ButtonObservable[] _buttons =
            { new(), new(), new(), new() };
        private readonly bool[] _controllerButtons = new bool[ButtonCount];
        private readonly bool[] _handTrackingButtons = new bool[ButtonCount];
        private readonly CompositeDisposable _subscriptions = new();

        private ControllerInputProvider _controller;
        private HandTrackingInputProvider _handTracking;
        private bool _usingHandTracking;

        /// <summary>Which hand this provider is reading input for.</summary>
        public HandIdentifier Handedness => handedness;

        /// <summary>True while input is coming from hand tracking, false while it comes from the controller.</summary>
        public bool IsUsingHandTracking => _usingHandTracking;

        /// <summary>The hand-tracking provider this provider switches to.</summary>
        public HandTrackingInputProvider HandTracking => _handTracking;

        /// <summary>Fires when the source changes; true means hand tracking, false means controller.</summary>
        public IObservable<bool> OnSourceChanged => _sourceChanged;
        private readonly Subject<bool> _sourceChanged = new();

        /// <inheritdoc/>
        public IObservable<VRButtonState> TriggerObservable => _buttons[Trigger].OnStateChanged;

        /// <inheritdoc/>
        public IObservable<VRButtonState> GripObservable => _buttons[Grip].OnStateChanged;

        /// <inheritdoc/>
        public IObservable<VRButtonState> AButtonObservable => _buttons[AButton].OnStateChanged;

        /// <inheritdoc/>
        public IObservable<VRButtonState> BButtonObservable => _buttons[BButton].OnStateChanged;

        /// <summary>Thumbstick axis. Tracked hands have no stick, so this reads zero under hand tracking.</summary>
        public Vector2 Thumbstick =>
            _usingHandTracking || _controller == null ? Vector2.zero : _controller.Thumbstick;

        /// <inheritdoc/>
        public float this[int fingerIndex] => ActiveProvider != null ? ActiveProvider[fingerIndex] : 0f;

        /// <inheritdoc/>
        public float this[FingerName finger] => this[(int)finger];

        /// <inheritdoc/>
        public int Priority => ActiveProvider != null ? ActiveProvider.Priority : 0;

        private HandInputProviderBase ActiveProvider => _usingHandTracking ? _handTracking : _controller;

        /// <summary>Creates the controller and hand-tracking providers for the given hand.</summary>
        public void Initialize(HandIdentifier hand, Config.HandInputActions actions)
        {
            handedness = hand;

            _controller = gameObject.AddComponent<ControllerInputProvider>();
            _controller.Handedness = hand;
            _controller.Initialize(actions);

            _handTracking = gameObject.AddComponent<HandTrackingInputProvider>();
            _handTracking.Handedness = hand;

            _subscriptions.Clear();
            TrackButtons(_controller, _controllerButtons, fromHandTracking: false);
            TrackButtons(_handTracking, _handTrackingButtons, fromHandTracking: true);
        }

        private void Update()
        {
            if (_handTracking == null) return;

            bool handTracked = _handTracking.IsTracked;
            if (handTracked == _usingHandTracking) return;

            _usingHandTracking = handTracked;
            PublishAllButtons();
            _sourceChanged.OnNext(_usingHandTracking);
        }

        private void OnDestroy()
        {
            _subscriptions.Dispose();
            if (_controller != null) Destroy(_controller);
            if (_handTracking != null) Destroy(_handTracking);
        }

        private void TrackButtons(IHandInputProvider provider, bool[] states, bool fromHandTracking)
        {
            TrackButton(provider.TriggerObservable, states, Trigger, fromHandTracking);
            TrackButton(provider.GripObservable, states, Grip, fromHandTracking);
            TrackButton(provider.AButtonObservable, states, AButton, fromHandTracking);
            TrackButton(provider.BButtonObservable, states, BButton, fromHandTracking);
        }

        private void TrackButton(IObservable<VRButtonState> source, bool[] states, int button, bool fromHandTracking)
        {
            source.Subscribe(state =>
            {
                states[button] = state == VRButtonState.Down;
                if (fromHandTracking == _usingHandTracking) _buttons[button].ButtonState = states[button];
            }).AddTo(_subscriptions);
        }

        // Re-publishes every button from the newly active source, so a button held on the old
        // source is released (and one held on the new source is pressed) at the moment of the switch.
        private void PublishAllButtons()
        {
            var states = _usingHandTracking ? _handTrackingButtons : _controllerButtons;
            for (int i = 0; i < ButtonCount; i++) _buttons[i].ButtonState = states[i];
        }
    }
}
#endif

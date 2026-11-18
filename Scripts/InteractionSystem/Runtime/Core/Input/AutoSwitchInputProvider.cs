using System;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Provider for <c>TrackingType.Both</c>. Wraps a controller provider and a hand-tracking
    /// provider for one hand and forwards whichever one is actually tracking: the controller
    /// while it is held, the tracked hand once the controller is put down. When neither is
    /// tracking, the previous source stays selected so a brief tracking loss does not flip
    /// the input source.
    /// </summary>
    public class AutoSwitchInputProvider : MonoBehaviour, IHandInputProvider
    {
        private const int ButtonCount = 4;
        private enum Source { Controller, Hand }

        private HandInputProviderBase _controller;
        private HandInputProviderBase _hand;
        private Source _active = Source.Controller;

        private readonly Subject<VRButtonState>[] _subjects =
        {
            new Subject<VRButtonState>(), new Subject<VRButtonState>(),
            new Subject<VRButtonState>(), new Subject<VRButtonState>()
        };
        private readonly bool[] _down = new bool[ButtonCount];
        private CompositeDisposable _subscriptions = new CompositeDisposable();

        private const int Trigger = 0, Grip = 1, AButton = 2, BButton = 3;

        /// <summary>Which hand this provider is reading.</summary>
        public HandIdentifier Handedness { get; set; }

        /// <summary>True while the tracked hand, not the controller, is the active source.</summary>
        public bool UsingHandTracking => _active == Source.Hand;

        /// <summary>Wires the two underlying providers. Must be called once, right after creation.</summary>
        public void Initialize(HandInputProviderBase controller, HandInputProviderBase hand)
        {
            _subscriptions.Dispose();
            _subscriptions = new CompositeDisposable();
            _controller = controller;
            _hand = hand;

            Forward(controller, Source.Controller);
            Forward(hand, Source.Hand);
        }

        private void Forward(HandInputProviderBase provider, Source source)
        {
            provider.TriggerObservable.Subscribe(s => OnChild(source, Trigger, s)).AddTo(_subscriptions);
            provider.GripObservable.Subscribe(s => OnChild(source, Grip, s)).AddTo(_subscriptions);
            provider.AButtonObservable.Subscribe(s => OnChild(source, AButton, s)).AddTo(_subscriptions);
            provider.BButtonObservable.Subscribe(s => OnChild(source, BButton, s)).AddTo(_subscriptions);
        }

        private void OnChild(Source source, int button, VRButtonState state)
        {
            if (source != _active) return;
            Emit(button, state == VRButtonState.Down);
        }

        private void Emit(int button, bool down)
        {
            if (_down[button] == down) return;
            _down[button] = down;
            _subjects[button].OnNext(down ? VRButtonState.Down : VRButtonState.Up);
        }

        private void Update()
        {
            if (_controller == null || _hand == null) return;

            var next = _active;
            if (_controller.TrackingState != 0) next = Source.Controller;
            else if (_hand.TrackingState != 0) next = Source.Hand;

            if (next == _active) return;

            _active = next;
            // Release anything the old source was holding so no button sticks down across the
            // switch; the new source reports its own presses from here on.
            for (int i = 0; i < ButtonCount; i++) Emit(i, false);
        }

        private IHandInputProvider Current => _active == Source.Controller ? _controller : _hand;

        private void OnDestroy()
        {
            _subscriptions.Dispose();
            foreach (var subject in _subjects) subject.Dispose();
        }

        public IObservable<VRButtonState> TriggerObservable => _subjects[Trigger];
        public IObservable<VRButtonState> GripObservable => _subjects[Grip];
        public IObservable<VRButtonState> AButtonObservable => _subjects[AButton];
        public IObservable<VRButtonState> BButtonObservable => _subjects[BButton];

        public Vector2 Thumbstick => _active == Source.Controller && _controller != null ? _controller.Thumbstick : Vector2.zero;

        public float this[int fingerIndex] => Current?[fingerIndex] ?? 0f;
        public float this[FingerName finger] => Current?[finger] ?? 0f;

        public int Priority => 20;
        public Vector3 Position => Current?.Position ?? Vector3.zero;
        public Quaternion Rotation => Current?.Rotation ?? Quaternion.identity;
        public uint TrackingState => Current?.TrackingState ?? 0;
    }
}

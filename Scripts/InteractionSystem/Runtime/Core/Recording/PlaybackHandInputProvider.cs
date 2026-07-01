using System;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Hand input provider that replays a recorded hand track. Position, rotation and finger
    /// curls are sampled (and lerped) from the recording; the button observables fire from the
    /// recording's discrete input events so presses are frame-accurate. Driven externally by
    /// <see cref="InteractionRecordingPlayer"/> via <see cref="Evaluate"/>.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Input Providers/Playback Input Provider")]
    public class PlaybackHandInputProvider : MonoBehaviour, IHandInputProvider
    {
        private InteractionRecording _recording;
        private HandIdentifier _hand;
        private int _eventCursor;

        private readonly ButtonObservable _trigger = new();
        private readonly ButtonObservable _grip = new();
        private readonly ButtonObservable _aButton = new();
        private readonly ButtonObservable _bButton = new();
        private readonly float[] _fingers = new float[5];
        private Vector3 _position;
        private Quaternion _rotation = Quaternion.identity;

        /// <summary>Observable for trigger button state changes.</summary>
        public IObservable<VRButtonState> TriggerObservable => _trigger.OnStateChanged;

        /// <summary>Observable for grip button state changes.</summary>
        public IObservable<VRButtonState> GripObservable => _grip.OnStateChanged;

        /// <summary>Observable for A button state changes.</summary>
        public IObservable<VRButtonState> AButtonObservable => _aButton.OnStateChanged;

        /// <summary>Observable for B button state changes.</summary>
        public IObservable<VRButtonState> BButtonObservable => _bButton.OnStateChanged;

        /// <summary>Finger curl value by index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).</summary>
        public float this[int fingerIndex] => fingerIndex is >= 0 and < 5 ? _fingers[fingerIndex] : 0f;

        /// <summary>Finger curl value by finger name.</summary>
        public float this[FingerName finger] => this[(int)finger];

        /// <summary>Playback providers take precedence over any live provider.</summary>
        public int Priority => 1000;

        /// <summary>Replayed hand position in CameraRig-local space.</summary>
        public Vector3 Position => _position;

        /// <summary>Replayed hand rotation in CameraRig-local space.</summary>
        public Quaternion Rotation => _rotation;

        /// <summary>Tracking state; always fully tracked during playback.</summary>
        public uint TrackingState => 3;

        /// <summary>Binds this provider to a recording and hand, resetting playback state.</summary>
        public void Initialize(InteractionRecording recording, HandIdentifier hand)
        {
            _recording = recording;
            _hand = hand;
            ResetPlayback();
        }

        /// <summary>Resets the event cursor and button states so playback can restart from zero.</summary>
        public void ResetPlayback()
        {
            _eventCursor = 0;
            _trigger.ButtonState = false;
            _grip.ButtonState = false;
            _aButton.ButtonState = false;
            _bButton.ButtonState = false;
        }

        /// <summary>Advances playback to the given time: updates pose and fingers, fires due button events.</summary>
        public void Evaluate(float time)
        {
            if (_recording == null) return;

            var pose = _recording.EvaluateHandPose(_hand, time);
            _position = pose.position;
            _rotation = pose.rotation;

            var fingers = _recording.EvaluateFingers(_hand, time);
            for (int i = 0; i < 5; i++) _fingers[i] = fingers[i];

            FireDueEvents(time);
        }

        private void FireDueEvents(float time)
        {
            var events = _recording.GetHand(_hand).inputEvents;
            if (events == null) return;
            if (_eventCursor > events.Length) _eventCursor = 0;

            while (_eventCursor < events.Length && events[_eventCursor].time <= time)
            {
                var recordedEvent = events[_eventCursor];
                SetButton(recordedEvent.button, recordedEvent.isDown);
                _eventCursor++;
            }
        }

        private void SetButton(RecordedButton button, bool isDown)
        {
            switch (button)
            {
                case RecordedButton.Trigger: _trigger.ButtonState = isDown; break;
                case RecordedButton.Grip: _grip.ButtonState = isDown; break;
                case RecordedButton.ButtonA: _aButton.ButtonState = isDown; break;
                case RecordedButton.ButtonB: _bButton.ButtonState = isDown; break;
            }
        }
    }
}

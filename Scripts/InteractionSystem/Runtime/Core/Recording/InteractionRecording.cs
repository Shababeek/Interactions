using System;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>A positional/rotational pose sample stored in CameraRig-local space.</summary>
    [Serializable]
    public struct PoseSample
    {
        [Tooltip("Position in CameraRig-local space.")]
        public Vector3 position;

        [Tooltip("Rotation in CameraRig-local space.")]
        public Quaternion rotation;

        /// <summary>Interpolates between two pose samples (linear position, spherical rotation).</summary>
        public static PoseSample Lerp(PoseSample a, PoseSample b, float t) => new()
        {
            position = Vector3.Lerp(a.position, b.position, t),
            rotation = Quaternion.Slerp(a.rotation, b.rotation, t)
        };
    }

    /// <summary>Analog curl values (0 = extended, 1 = curled) for the five fingers of one hand.</summary>
    [Serializable]
    public struct FingerSample
    {
        [Tooltip("Thumb curl (0-1).")] public float thumb;
        [Tooltip("Index curl (0-1).")] public float index;
        [Tooltip("Middle curl (0-1).")] public float middle;
        [Tooltip("Ring curl (0-1).")] public float ring;
        [Tooltip("Pinky curl (0-1).")] public float pinky;

        /// <summary>Gets a finger curl by index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky).</summary>
        public float this[int fingerIndex] => fingerIndex switch
        {
            0 => thumb,
            1 => index,
            2 => middle,
            3 => ring,
            4 => pinky,
            _ => 0f
        };

        /// <summary>Interpolates each finger curl between two samples.</summary>
        public static FingerSample Lerp(FingerSample a, FingerSample b, float t) => new()
        {
            thumb = Mathf.Lerp(a.thumb, b.thumb, t),
            index = Mathf.Lerp(a.index, b.index, t),
            middle = Mathf.Lerp(a.middle, b.middle, t),
            ring = Mathf.Lerp(a.ring, b.ring, t),
            pinky = Mathf.Lerp(a.pinky, b.pinky, t)
        };
    }

    /// <summary>Identifies which button a discrete recorded input event refers to.</summary>
    public enum RecordedButton
    {
        /// <summary>Trigger button (index finger).</summary>
        Trigger = 0,
        /// <summary>Grip button.</summary>
        Grip = 1,
        /// <summary>Primary thumb button (A).</summary>
        ButtonA = 2,
        /// <summary>Secondary thumb button (B).</summary>
        ButtonB = 3
    }

    /// <summary>A timestamped button state change captured during recording.</summary>
    [Serializable]
    public struct RecordedInputEvent
    {
        [Tooltip("Seconds from the start of the recording.")]
        public float time;

        [Tooltip("Which button changed state.")]
        public RecordedButton button;

        [Tooltip("True for a press (Down), false for a release (Up).")]
        public bool isDown;
    }

    /// <summary>Per-hand recorded data: fixed-rate pose and finger samples plus discrete input events.</summary>
    [Serializable]
    public class HandRecordingTrack
    {
        [Tooltip("Pose samples (CameraRig-local) captured at the recording's sample rate.")]
        public PoseSample[] poses = Array.Empty<PoseSample>();

        [Tooltip("Finger curl samples captured at the recording's sample rate.")]
        public FingerSample[] fingers = Array.Empty<FingerSample>();

        [Tooltip("Discrete button state changes, ordered by time.")]
        public RecordedInputEvent[] inputEvents = Array.Empty<RecordedInputEvent>();
    }

    /// <summary>
    /// A recorded interaction session: head and hand poses plus finger curls sampled at a fixed
    /// rate (lerped on playback) and discrete button events. Poses are stored in CameraRig-local
    /// space so a recording can be replayed regardless of where the rig sits in the world.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Interactions/Interaction Recording")]
    public class InteractionRecording : ScriptableObject
    {
        [Tooltip("Samples per second used for the pose and finger tracks.")]
        [SerializeField] private float sampleRate = 30f;

        [Tooltip("Number of pose samples in each track.")]
        [SerializeField] private int sampleCount;

        [Tooltip("Total length of the recording in seconds.")]
        [SerializeField] private float duration;

        [Tooltip("Head (camera) pose samples in CameraRig-local space.")]
        [SerializeField] private PoseSample[] headPoses = Array.Empty<PoseSample>();

        [Tooltip("Left hand recorded track.")]
        [SerializeField] private HandRecordingTrack leftHand = new();

        [Tooltip("Right hand recorded track.")]
        [SerializeField] private HandRecordingTrack rightHand = new();

        /// <summary>Samples per second for the pose and finger tracks.</summary>
        public float SampleRate => sampleRate;

        /// <summary>Number of pose samples in each track.</summary>
        public int SampleCount => sampleCount;

        /// <summary>Total length of the recording in seconds.</summary>
        public float Duration => duration;

        /// <summary>Head (camera) pose samples in CameraRig-local space.</summary>
        public PoseSample[] HeadPoses => headPoses;

        /// <summary>Left hand recorded track.</summary>
        public HandRecordingTrack LeftHand => leftHand;

        /// <summary>Right hand recorded track.</summary>
        public HandRecordingTrack RightHand => rightHand;

        /// <summary>Gets the recorded track for the given hand.</summary>
        public HandRecordingTrack GetHand(HandIdentifier hand) =>
            hand == HandIdentifier.Right ? rightHand : leftHand;

        /// <summary>Populates this recording with captured data and recomputes count and duration.</summary>
        public void SetData(float rate, PoseSample[] head, HandRecordingTrack left, HandRecordingTrack right)
        {
            sampleRate = Mathf.Max(1f, rate);
            headPoses = head ?? Array.Empty<PoseSample>();
            leftHand = left ?? new HandRecordingTrack();
            rightHand = right ?? new HandRecordingTrack();
            sampleCount = headPoses.Length;
            duration = sampleCount > 1 ? (sampleCount - 1) / sampleRate : 0f;
        }

        /// <summary>Samples the head pose at the given time, lerping between fixed-rate samples.</summary>
        public PoseSample EvaluateHead(float time) => EvaluatePose(headPoses, time);

        /// <summary>Samples a hand pose at the given time, lerping between fixed-rate samples.</summary>
        public PoseSample EvaluateHandPose(HandIdentifier hand, float time) => EvaluatePose(GetHand(hand).poses, time);

        /// <summary>Samples a hand's finger curls at the given time, lerping between fixed-rate samples.</summary>
        public FingerSample EvaluateFingers(HandIdentifier hand, float time) => EvaluateFingers(GetHand(hand).fingers, time);

        private PoseSample EvaluatePose(PoseSample[] samples, float time)
        {
            if (samples == null || samples.Length == 0) return new PoseSample { rotation = Quaternion.identity };
            int lower = FindLowerIndex(samples.Length, time, out float blend);
            return lower >= samples.Length - 1 ? samples[^1] : PoseSample.Lerp(samples[lower], samples[lower + 1], blend);
        }

        private FingerSample EvaluateFingers(FingerSample[] samples, float time)
        {
            if (samples == null || samples.Length == 0) return default;
            int lower = FindLowerIndex(samples.Length, time, out float blend);
            return lower >= samples.Length - 1 ? samples[^1] : FingerSample.Lerp(samples[lower], samples[lower + 1], blend);
        }

        private int FindLowerIndex(int length, float time, out float blend)
        {
            float position = Mathf.Clamp(time * sampleRate, 0f, length - 1);
            int lower = Mathf.FloorToInt(position);
            blend = position - lower;
            return lower;
        }
    }
}

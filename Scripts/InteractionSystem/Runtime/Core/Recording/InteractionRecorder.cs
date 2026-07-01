using System.Collections.Generic;
using UniRx;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Records head and hand poses plus finger curls (sampled at a fixed rate) and discrete button
    /// events from the live input providers, then writes the result to an
    /// <see cref="InteractionRecording"/> asset. All poses are stored in CameraRig-local space.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interaction Recorder")]
    public class InteractionRecorder : MonoBehaviour
    {
        [Tooltip("Camera rig whose head and hands are recorded. Auto-found in parents if left empty.")]
        [SerializeField] private CameraRig cameraRig;

        [Tooltip("Samples captured per second for poses and finger curls. Higher is smoother but larger.")]
        [SerializeField] private float sampleRate = 30f;

        [Tooltip("Begin recording automatically when play starts.")]
        [SerializeField] private bool recordOnStart;

        [Tooltip("Folder (under Assets) where recording assets are saved in the editor.")]
        [SerializeField] private string outputFolder = "Assets/Recordings";

        [Tooltip("Base file name for saved recording assets.")]
        [SerializeField] private string recordingName = "InteractionRecording";

        private bool _isRecording;
        private float _elapsed;
        private float _nextSampleTime;
        private readonly List<PoseSample> _head = new();
        private readonly List<PoseSample> _leftPoses = new();
        private readonly List<PoseSample> _rightPoses = new();
        private readonly List<FingerSample> _leftFingers = new();
        private readonly List<FingerSample> _rightFingers = new();
        private readonly List<RecordedInputEvent> _leftEvents = new();
        private readonly List<RecordedInputEvent> _rightEvents = new();
        private readonly CompositeDisposable _eventSubscriptions = new();

        /// <summary>Whether a recording is currently in progress.</summary>
        public bool IsRecording => _isRecording;

        /// <summary>Elapsed time of the in-progress recording, in seconds.</summary>
        public float ElapsedTime => _elapsed;

        /// <summary>Number of pose samples captured so far.</summary>
        public int SampleCount => _head.Count;

        /// <summary>The most recently saved recording, if any.</summary>
        public InteractionRecording LastRecording { get; private set; }

        private void Awake()
        {
            if (cameraRig == null) cameraRig = GetComponentInParent<CameraRig>();
        }

        private void Start()
        {
            if (recordOnStart) StartRecording();
        }

        /// <summary>Starts a new recording, clearing any previously captured data.</summary>
        public void StartRecording()
        {
            if (_isRecording) return;
            if (!ValidateRig()) return;

            ClearBuffers();
            _elapsed = 0f;
            _nextSampleTime = 0f;
            _isRecording = true;
            SubscribeToButtons();
        }

        /// <summary>Stops the recording and writes the captured data to an asset.</summary>
        public InteractionRecording StopRecording()
        {
            if (!_isRecording) return null;
            _isRecording = false;
            _eventSubscriptions.Clear();
            LastRecording = SaveRecording();
            return LastRecording;
        }

        private bool ValidateRig()
        {
            if (cameraRig == null)
            {
                Debug.LogError("[InteractionRecorder] No CameraRig assigned or found in parents.", this);
                return false;
            }
            if (cameraRig.Config == null)
            {
                Debug.LogError("[InteractionRecorder] The CameraRig has no Config assigned.", this);
                return false;
            }
            return true;
        }

        private void SubscribeToButtons()
        {
            SubscribeHand(HandIdentifier.Left, _leftEvents);
            SubscribeHand(HandIdentifier.Right, _rightEvents);
        }

        private void SubscribeHand(HandIdentifier hand, List<RecordedInputEvent> events)
        {
            var provider = cameraRig.Config[hand];
            if (provider == null) return;
            provider.TriggerObservable.Subscribe(state => RecordEvent(events, RecordedButton.Trigger, state)).AddTo(_eventSubscriptions);
            provider.GripObservable.Subscribe(state => RecordEvent(events, RecordedButton.Grip, state)).AddTo(_eventSubscriptions);
            provider.AButtonObservable.Subscribe(state => RecordEvent(events, RecordedButton.ButtonA, state)).AddTo(_eventSubscriptions);
            provider.BButtonObservable.Subscribe(state => RecordEvent(events, RecordedButton.ButtonB, state)).AddTo(_eventSubscriptions);
        }

        private void RecordEvent(List<RecordedInputEvent> events, RecordedButton button, VRButtonState state)
        {
            if (!_isRecording) return;
            events.Add(new RecordedInputEvent { time = _elapsed, button = button, isDown = state == VRButtonState.Down });
        }

        private void Update()
        {
            if (!_isRecording) return;
            _elapsed += Time.deltaTime;
            if (_elapsed < _nextSampleTime) return;
            CaptureSample();
            _nextSampleTime += 1f / Mathf.Max(1f, sampleRate);
        }

        private void CaptureSample()
        {
            var rig = cameraRig.transform;
            var camera = cameraRig.XRCamera;
            _head.Add(camera != null ? ToLocal(rig, camera.transform.position, camera.transform.rotation) : Identity());

            _leftPoses.Add(PivotSample(rig, cameraRig.LeftHandPivot));
            _rightPoses.Add(PivotSample(rig, cameraRig.RightHandPivot));

            _leftFingers.Add(SampleFingers(cameraRig.Config[HandIdentifier.Left]));
            _rightFingers.Add(SampleFingers(cameraRig.Config[HandIdentifier.Right]));
        }

        private static PoseSample PivotSample(Transform rig, Transform pivot) =>
            pivot != null ? ToLocal(rig, pivot.position, pivot.rotation) : Identity();

        private static PoseSample ToLocal(Transform rig, Vector3 worldPosition, Quaternion worldRotation) => new()
        {
            position = rig.InverseTransformPoint(worldPosition),
            rotation = Quaternion.Inverse(rig.rotation) * worldRotation
        };

        private static PoseSample Identity() => new() { rotation = Quaternion.identity };

        private static FingerSample SampleFingers(IHandInputProvider provider)
        {
            if (provider == null) return default;
            return new FingerSample
            {
                thumb = provider[0],
                index = provider[1],
                middle = provider[2],
                ring = provider[3],
                pinky = provider[4]
            };
        }

        private void ClearBuffers()
        {
            _head.Clear();
            _leftPoses.Clear();
            _rightPoses.Clear();
            _leftFingers.Clear();
            _rightFingers.Clear();
            _leftEvents.Clear();
            _rightEvents.Clear();
            _eventSubscriptions.Clear();
        }

        private InteractionRecording SaveRecording()
        {
            var recording = ScriptableObject.CreateInstance<InteractionRecording>();
            recording.SetData(
                sampleRate,
                _head.ToArray(),
                new HandRecordingTrack { poses = _leftPoses.ToArray(), fingers = _leftFingers.ToArray(), inputEvents = _leftEvents.ToArray() },
                new HandRecordingTrack { poses = _rightPoses.ToArray(), fingers = _rightFingers.ToArray(), inputEvents = _rightEvents.ToArray() });

#if UNITY_EDITOR
            PersistAsset(recording);
#else
            Debug.LogWarning("[InteractionRecorder] Recording captured in memory only. Saving to an asset requires the Unity editor.");
#endif
            return recording;
        }

#if UNITY_EDITOR
        private void PersistAsset(InteractionRecording recording)
        {
            EnsureFolderExists(outputFolder);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{recordingName}.asset");
            AssetDatabase.CreateAsset(recording, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[InteractionRecorder] Saved recording: {path} ({recording.SampleCount} samples, {recording.Duration:F2}s).", recording);
        }

        private static void EnsureFolderExists(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
#endif
    }
}

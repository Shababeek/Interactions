using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Replays an <see cref="InteractionRecording"/> through a CameraRig. It drives the head and
    /// hand pivot transforms directly and injects <see cref="PlaybackHandInputProvider"/> instances
    /// into the rig's Config, so recorded finger curls and button presses flow through the normal
    /// interaction pipeline as a "mock" input config. Live input is restored when playback stops.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interaction Recording Player")]
    public class InteractionRecordingPlayer : MonoBehaviour
    {
        [Tooltip("Camera rig to drive during playback. Auto-found in parents if left empty.")]
        [SerializeField] private CameraRig cameraRig;

        [Tooltip("Recording to replay.")]
        [SerializeField] private InteractionRecording recording;

        [Tooltip("Restart playback from the beginning when it reaches the end.")]
        [SerializeField] private bool loop = true;

        [Tooltip("Begin replaying automatically when play starts.")]
        [SerializeField] private bool playOnStart;

        [Tooltip("Playback speed multiplier (1 = real time).")]
        [SerializeField] private float playbackSpeed = 1f;

        [Tooltip("Behaviours disabled during playback so they don't fight the replayed poses (e.g. TrackedPoseDrivers on the head and hand pivots).")]
        [SerializeField] private Behaviour[] trackingToDisableDuringPlayback;

        private bool _isPlaying;
        private float _time;
        private GameObject _providerHost;
        private PlaybackHandInputProvider _leftProvider;
        private PlaybackHandInputProvider _rightProvider;
        private IHandInputProvider _cachedLeftProvider;
        private IHandInputProvider _cachedRightProvider;
        private readonly List<Behaviour> _disabledBehaviours = new();
        private HandPivotUpdater _disabledPivotUpdater;

        /// <summary>Whether a recording is currently being replayed.</summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>Current playback head time in seconds.</summary>
        public float Time => _time;

        /// <summary>The recording assigned to this player.</summary>
        public InteractionRecording Recording
        {
            get => recording;
            set => recording = value;
        }

        private void Awake()
        {
            if (cameraRig == null) cameraRig = GetComponentInParent<CameraRig>();
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        /// <summary>Starts replaying the assigned recording.</summary>
        public void Play()
        {
            if (_isPlaying) return;
            if (cameraRig == null || cameraRig.Config == null || recording == null)
            {
                Debug.LogError("[InteractionRecordingPlayer] Missing CameraRig, Config or recording.", this);
                return;
            }

            _time = 0f;
            InjectProviders();
            DisableLiveTracking();
            _isPlaying = true;
        }

        /// <summary>Stops playback and restores live input.</summary>
        public void Stop()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            RestoreLiveTracking();
            RestoreProviders();
        }

        private void LateUpdate()
        {
            if (!_isPlaying) return;

            _time += UnityEngine.Time.deltaTime * Mathf.Max(0f, playbackSpeed);
            if (_time >= recording.Duration)
            {
                if (loop)
                {
                    _time = 0f;
                    _leftProvider.ResetPlayback();
                    _rightProvider.ResetPlayback();
                }
                else
                {
                    _time = recording.Duration;
                }
            }

            Apply(_time);

            if (!loop && _time >= recording.Duration) Stop();
        }

        private void Apply(float time)
        {
            var rig = cameraRig.transform;
            var camera = cameraRig.XRCamera;
            if (camera != null) SetWorld(rig, camera.transform, recording.EvaluateHead(time));
            SetWorld(rig, cameraRig.LeftHandPivot, recording.EvaluateHandPose(HandIdentifier.Left, time));
            SetWorld(rig, cameraRig.RightHandPivot, recording.EvaluateHandPose(HandIdentifier.Right, time));

            _leftProvider.Evaluate(time);
            _rightProvider.Evaluate(time);
        }

        private static void SetWorld(Transform rig, Transform target, PoseSample local)
        {
            if (target == null) return;
            target.position = rig.TransformPoint(local.position);
            target.rotation = rig.rotation * local.rotation;
        }

        private void InjectProviders()
        {
            var config = cameraRig.Config;
            _cachedLeftProvider = config[HandIdentifier.Left];
            _cachedRightProvider = config[HandIdentifier.Right];

            _providerHost = new GameObject("[Playback] Input Providers");
            _providerHost.transform.SetParent(transform, false);
            _leftProvider = _providerHost.AddComponent<PlaybackHandInputProvider>();
            _rightProvider = _providerHost.AddComponent<PlaybackHandInputProvider>();
            _leftProvider.Initialize(recording, HandIdentifier.Left);
            _rightProvider.Initialize(recording, HandIdentifier.Right);

            config.SetHandProvider(HandIdentifier.Left, _leftProvider);
            config.SetHandProvider(HandIdentifier.Right, _rightProvider);
        }

        private void RestoreProviders()
        {
            var config = cameraRig.Config;
            if (_cachedLeftProvider != null) config.SetHandProvider(HandIdentifier.Left, _cachedLeftProvider);
            if (_cachedRightProvider != null) config.SetHandProvider(HandIdentifier.Right, _cachedRightProvider);

            if (_providerHost != null) Destroy(_providerHost);
            _providerHost = null;
            _leftProvider = null;
            _rightProvider = null;
        }

        private void DisableLiveTracking()
        {
            _disabledBehaviours.Clear();
            if (trackingToDisableDuringPlayback != null)
            {
                foreach (var behaviour in trackingToDisableDuringPlayback)
                {
                    if (behaviour == null || !behaviour.enabled) continue;
                    behaviour.enabled = false;
                    _disabledBehaviours.Add(behaviour);
                }
            }

            var pivotUpdater = cameraRig.GetComponent<HandPivotUpdater>();
            if (pivotUpdater != null && pivotUpdater.enabled)
            {
                pivotUpdater.enabled = false;
                _disabledPivotUpdater = pivotUpdater;
            }
        }

        private void RestoreLiveTracking()
        {
            foreach (var behaviour in _disabledBehaviours)
            {
                if (behaviour != null) behaviour.enabled = true;
            }
            _disabledBehaviours.Clear();

            if (_disabledPivotUpdater != null)
            {
                _disabledPivotUpdater.enabled = true;
                _disabledPivotUpdater = null;
            }
        }

        private void OnDisable()
        {
            if (_isPlaying) Stop();
        }
    }
}

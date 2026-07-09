using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// A screw-in fastener (nut / bolt). A matching <see cref="FastenerTool"/> turns it about
    /// its own axis; accumulated rotation drives it along that axis by the thread pitch, between
    /// fully tightened (seated) and fully loosened (removed). The fastener spins around its own
    /// centre — the tool rotates with it.
    /// </summary>
    public class Fastener : MonoBehaviour
    {
        [Tooltip("Which tool can turn this. Wrench=Nut, Drill=Bolt. Matched against FastenerTool's tool mask.")]
        [SerializeField] private SocketMask fastenerMask = SocketMask.FromInt(~0);

        [Tooltip("Local rotation / thread axis. Cylinder primitives are Y-up, so (0,1,0) by default.")]
        [SerializeField] private Vector3 axis = Vector3.up;

        [Tooltip("Metres travelled along the axis per full 360 degree turn (thread pitch).")]
        [SerializeField] private float pitch = 0.004f;

        [Tooltip("Number of full turns from seated (tight) to removed (loose).")]
        [SerializeField] private float turnsToRemove = 4f;

        [Tooltip("When fully loosened, detach from the mount and drop with physics.")]
        [SerializeField] private bool detachWhenFullyLoose = true;

        [Tooltip("Fires when the fastener reaches the fully seated (tight) position.")]
        [SerializeField] private UnityEvent onTightened = new();

        [Tooltip("Fires when the fastener reaches the fully loosened (removed) position.")]
        [SerializeField] private UnityEvent onFullyLoosened = new();

        [Tooltip("Normalised progress each turn: 0 = tight, 1 = fully loose.")]
        [SerializeField] private UnityEvent<float> onProgress = new();

        [Tooltip("Log seat / loosen / detach milestones to the Console.")]
        [SerializeField] private bool verboseLogs = true;

        private Vector3 _seatedLocalPos;
        private Vector3 _parentAxis; // thread axis in parent space; constant while spinning about it
        private float _accumDeg;     // 0 = tight, MaxDeg = removed
        private bool _detached;
        private bool _looseFired;    // guards the fully-loosened event/detach to once

        public SocketMask FastenerMask => fastenerMask;

        /// <summary>Thread/rotation axis in world space.</summary>
        public Vector3 WorldAxis => transform.TransformDirection(axis).normalized;

        /// <summary>True once fully loosened and released from its mount.</summary>
        public bool Detached => _detached;

        /// <summary>0 = fully tight, 1 = fully loose.</summary>
        public float Progress01 => MaxDeg > 0f ? _accumDeg / MaxDeg : 0f;

        private float MaxDeg => Mathf.Max(1f, turnsToRemove) * 360f;

        private void Awake()
        {
            _seatedLocalPos = transform.localPosition;
            _parentAxis = (transform.localRotation * axis).normalized;
            if (verboseLogs)
                Debug.Log($"[Fastener:{name}] READY seated. mask=0x{(int)fastenerMask.Value:X} " +
                          $"worldAxis={WorldAxis} pitch={pitch} turnsToRemove={turnsToRemove}", this);
        }

        /// <summary>
        /// Turns the fastener. Positive = loosen (backs out), negative = tighten (seats).
        /// Rotates about the axis AND translates along it. Returns the degrees actually applied
        /// (0 when already clamped at either end).
        /// </summary>
        public float ApplyTurn(float deltaDeg)
        {
            if (_detached) return 0f;

            float old = _accumDeg;
            _accumDeg = Mathf.Clamp(_accumDeg + deltaDeg, 0f, MaxDeg);
            float applied = _accumDeg - old;
            if (Mathf.Approximately(applied, 0f)) return 0f;
            if (_accumDeg < MaxDeg) _looseFired = false; // re-armed once backed off the end stop

            transform.Rotate(axis, applied, Space.Self);
            transform.localPosition = _seatedLocalPos + _parentAxis * (_accumDeg / 360f * pitch);

            onProgress.Invoke(Progress01);
            if (_accumDeg <= 0f && old > 0f)
            {
                if (verboseLogs) Debug.Log($"[Fastener:{name}] TIGHTENED — fully seated.", this);
                onTightened.Invoke();
            }
            if (_accumDeg >= MaxDeg) HandleFullyLoose();
            return applied;
        }

        private void HandleFullyLoose()
        {
            if (_looseFired) return; // only once, not every frame at the end stop
            _looseFired = true;

            if (verboseLogs) Debug.Log($"[Fastener:{name}] FULLY LOOSENED after {turnsToRemove} turns.", this);
            onFullyLoosened.Invoke();
            if (!detachWhenFullyLoose || _detached) return;

            _detached = true;
            transform.SetParent(null, true);
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            if (verboseLogs) Debug.Log($"[Fastener:{name}] DETACHED — dropped with physics.", this);
        }
    }
}

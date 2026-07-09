using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// A tool head (wrench / drill) that turns a matching <see cref="Fastener"/>. The FastenerTool
    /// sits on a child "tip" of the tool; the tool itself is a <see cref="Grabable"/> held by a hand.
    ///
    /// Operating requires: the tip is in range of a matching fastener AND the driving button is
    /// held on the controller that holds the tool. You do NOT have to aim precisely — the moment
    /// you hold the button in range, the tool SNAPS to the correct engagement angle (its drive
    /// axis rotated coaxial with the fastener axis) and locks onto the fastener. The grab stays
    /// intact the whole time; releasing the button frees the tool back to the hand:
    ///   - Manual (wrench): hold the Use button (Trigger) to lock the wrench onto the nut. Then
    ///     CRANK your hand around the nut axis — the wrench handle swings with your hand and the
    ///     nut turns with it, 1:1. The loosen/tighten direction follows the Fastener's own axis
    ///     convention (positive crank = the same direction the Fastener treats as "loosen").
    ///   - Motor (drill): hold Use (Trigger) to LOOSEN, or the A/B thumb button to TIGHTEN. The
    ///     drill is FROZEN at its engagement world pose while operating — it does NOT rotate or
    ///     drift with the bolt (or the hand); the bolt spins underneath it.
    ///
    /// While operating the tool is locked to the fastener (manual: parented so it rotates & backs
    /// out WITH the nut; motor: pinned to a fixed world pose) so the hand can drift without
    /// breaking contact. Releasing the button frees the tool back to the hand.
    /// </summary>
    public class FastenerTool : MonoBehaviour
    {
        public enum DriveMode { Manual, Motor }

        [Tooltip("Categories this tool can turn. Wrench=Nut, Drill=Bolt.")]
        [SerializeField] private SocketMask toolMask;

        [Tooltip("Tip point used to scan for fasteners. Defaults to this transform.")]
        [SerializeField] private Transform tip;

        [SerializeField] private float detectionRadius = 0.05f;
        [SerializeField] private LayerMask fastenerLayers = ~0;

        [Tooltip("Tool's socket/bit/drive axis in LOCAL space (relative to the tip). On engage the tool SNAPS so this axis becomes coaxial with the fastener axis. Set it to whichever local axis your bit/socket points along — use the cyan Gizmo arrow (select the tool) to verify. Either direction is accepted.")]
        [SerializeField] private Vector3 driveAxisLocal = Vector3.up;

        [Tooltip("Manual = wrench (crank your hand around the nut). Motor = drill (fixed rate while a button is held).")]
        [SerializeField] private DriveMode mode = DriveMode.Manual;

        [Tooltip("Motor mode: degrees per second applied while the loosen/tighten button is held.")]
        [SerializeField] private float motorDegreesPerSecond = 720f;

        [Tooltip("Stay engaged for this long after the tip briefly leaves the fastener, so detection flicker doesn't reset the turn. 0 = disable.")]
        [SerializeField] private float disengageGrace = 0.2f;

        [Tooltip("Optional visual that spins with the fastener (drill bit). Spun about its local Y.")]
        [SerializeField] private Transform spinVisual;

        [Tooltip("Log detection / buttons / lock / turning to the Console.")]
        [SerializeField] private bool verboseLogs = true;

        [Tooltip("Seconds between the per-frame 'turning' logs so they don't spam.")]
        [SerializeField] private float turnLogInterval = 0.25f;

        [SerializeField] private UnityEvent<Fastener> onEngaged = new();
        [SerializeField] private UnityEvent<Fastener> onDisengaged = new();

        private readonly Collider[] _hits = new Collider[8];
        private readonly CompositeDisposable _disposables = new();

        private InteractableBase _tool;   // the Grabable this tip belongs to
        private Fastener _engaged;
        private bool _operating;

        // button state driven by the tool's Grabable events
        private bool _useHeld;    // Trigger  (wrench = hold-the-nut, drill = loosen)
        private bool _thumbHeld;  // A/B      (drill = tighten)

        // crank tracking (manual): previous projected hand direction about the fastener axis
        private Vector3 _prevCrankDir;
        private bool _hasPrevCrank;

        // manual lock pose: tool pose in fastener-local space (tool is parented to the fastener,
        // so it rotates & backs out WITH the nut — the handle follows your crank 1:1).
        private Vector3 _lockPosLocal;
        private Quaternion _lockRot;

        // motor lock pose: tool WORLD pose captured at engagement. The drill is frozen here while
        // operating so it stays put instead of rotating/drift with the bolt.
        private Vector3 _lockWorldPos;
        private Quaternion _lockWorldRot;

        private float _graceUntil;
        private float _nextTurnLogTime;

        public SocketMask ToolMask => toolMask;

        private void Awake()
        {
            _tool = GetComponentInParent<InteractableBase>();
            if (_tool == null)
                Debug.LogWarning($"[FastenerTool:{name}] No Grabable/InteractableBase found in parents — buttons won't work.", this);
        }

        private void OnEnable()
        {
            if (_tool == null) return;
            _tool.OnUseStarted.Do(_ => SetUse(true)).Subscribe().AddTo(_disposables);
            _tool.OnUseEnded.Do(_ => SetUse(false)).Subscribe().AddTo(_disposables);
            _tool.OnThumbPressed.Do(_ => SetThumb(true)).Subscribe().AddTo(_disposables);
            _tool.OnThumbReleased.Do(_ => SetThumb(false)).Subscribe().AddTo(_disposables);
            // Dropping the tool must clear held buttons so it can't keep "operating".
            _tool.OnDeselected.Do(_ => { _useHeld = false; _thumbHeld = false; }).Subscribe().AddTo(_disposables);
        }

        private void OnDisable() => _disposables.Clear();

        private void SetUse(bool v)   { _useHeld = v;   Log($"BUTTON Use(Trigger)={_useHeld}"); }
        private void SetThumb(bool v) { _thumbHeld = v; Log($"BUTTON Thumb(A/B)={_thumbHeld}"); }

        private bool ToolHeld => _tool != null && _tool.IsSelected && _tool.CurrentInteractor != null;

        /// <summary>Whether the driving button(s) for this mode are currently held.</summary>
        private bool DriveButtonHeld => mode == DriveMode.Manual ? _useHeld : (_useHeld || _thumbHeld);

        private void Update()
        {
            UpdateEngagement();

            bool wantOperate = _engaged != null && !_engaged.Detached && ToolHeld && DriveButtonHeld;

            if (wantOperate && !_operating) StartOperate();
            else if (!wantOperate && _operating) StopOperate();

            if (!_operating) return;

            float delta = mode == DriveMode.Motor ? MotorDelta() : CrankDelta();
            if (Mathf.Abs(delta) <= 0f) return;

            float applied = _engaged.ApplyTurn(delta);
            if (spinVisual != null) spinVisual.Rotate(Vector3.up, applied, Space.Self);

            // Fastener just reached fully-loose and detached: release the tool THIS frame so it
            // isn't carried off by the dropping fastener (manual parents to it).
            if (_engaged.Detached) { StopOperate(); return; }

            if (verboseLogs && Time.time >= _nextTurnLogTime)
            {
                _nextTurnLogTime = Time.time + turnLogInterval;
                string dir = delta > 0 ? "LOOSEN" : "TIGHTEN";
                Log($"TURN '{_engaged.name}' {dir} req={delta:F1}° applied={applied:F1}° progress={_engaged.Progress01:P0}" +
                    (Mathf.Approximately(applied, 0f) ? "  (clamped — at end stop)" : ""));
            }
        }

        private void LateUpdate()
        {
            // Motor only: keep the drill FROZEN at its engagement world pose. The bolt spins
            // underneath; the drill body stays put (counters any hand motion applied this frame).
            // Manual mode is parented to the fastener and needs no per-frame pin.
            if (!_operating || _engaged == null || _tool == null) return;
            if (mode != DriveMode.Motor) return;
            var toolT = _tool.transform;
            toolT.position = _lockWorldPos;
            toolT.rotation = _lockWorldRot;
        }

        private void StartOperate()
        {
            _operating = true;
            _hasPrevCrank = false;

            var origin = tip ? tip : transform;
            var ft = _engaged.transform;
            var toolT = _tool.transform;

            // SNAP to the correct engagement angle: rotate the tool so its drive axis (socket/bit)
            // becomes coaxial with the fastener axis. You only have to aim roughly — the snap does
            // the precise alignment. The grab stays intact (we only override the tool's pose).
            float snappedDeg = SnapDriveAxisCoaxial();

            // After the rotation snap the tip swung off the fastener centre (the snap rotates about
            // the tool ROOT, and the tip is a child). Recompute the tip→root offset and place the
            // tip exactly on the fastener centre — for BOTH modes — so the detection tip stays on
            // the fastener while operating (otherwise the drill loses detection after one turn and
            // re-engages every few degrees).
            Vector3 rootFromTip = toolT.position - origin.position;
            toolT.position = ft.position + rootFromTip;

            if (mode == DriveMode.Manual)
            {
                // PARENT the tool to the fastener. The wrench now rotates & backs out WITH the nut.
                // Because the nut rotates by exactly the hand's crank delta (1:1 — see CrankDelta),
                // the handle stays aligned with the hand as you swing it around the axis.
                toolT.SetParent(ft, true);
                _lockPosLocal = ft.InverseTransformPoint(toolT.position);
                _lockRot = Quaternion.Inverse(ft.rotation) * toolT.rotation;
                toolT.localPosition = _lockPosLocal;
                toolT.localRotation = _lockRot;
                Log($"LOCK(parent→fastener) onto '{_engaged.name}' (manual/crank) snap={snappedDeg:F0}°");
            }
            else
            {
                // Motor: freeze the drill at its (now snapped + tip-centered) world pose.
                _lockWorldPos = toolT.position;
                _lockWorldRot = toolT.rotation;
                Log($"LOCK(frozen world) onto '{_engaged.name}' (motor) snap={snappedDeg:F0}°");
            }
        }

        /// <summary>
        /// Rotates the tool so its drive axis becomes coaxial with the engaged fastener's axis.
        /// Picks the closer of the two coaxial directions so a roughly-aligned tool isn't flipped
        /// 180°. Returns the correction in degrees (large values hint Drive Axis Local is wrong).
        /// </summary>
        private float SnapDriveAxisCoaxial()
        {
            var origin = tip ? tip : transform;
            var toolT = _tool.transform;

            Vector3 toolDrive = origin.TransformDirection(driveAxisLocal).normalized;
            if (toolDrive.sqrMagnitude < 1e-6f) return 0f;

            Vector3 fastAxis = _engaged.WorldAxis;
            Vector3 targetDrive = Vector3.Dot(toolDrive, fastAxis) >= 0f ? fastAxis : -fastAxis;
            Quaternion fix = Quaternion.FromToRotation(toolDrive, targetDrive);
            float deg = Quaternion.Angle(Quaternion.identity, fix);
            toolT.rotation = fix * toolT.rotation;
            return deg;
        }

        private void StopOperate()
        {
            _operating = false;
            _hasPrevCrank = false;

            // Return the tool to the hand's attachment pose (Grabable parents it to the
            // AttachmentPoint at local identity while held). If the tool was dropped, Grabable's
            // DeSelected already detached it — leave the pose alone.
            if (ToolHeld && _tool != null)
            {
                var toolT = _tool.transform;
                var hand = _tool.CurrentInteractor;
                if (mode == DriveMode.Manual && hand != null && hand.AttachmentPoint != null)
                    toolT.SetParent(hand.AttachmentPoint, false);
                toolT.localPosition = Vector3.zero;
                toolT.localRotation = Quaternion.identity;
            }
            Log("UNLOCK — tool released back to hand");
        }

        private float MotorDelta()
        {
            // Loosen (Trigger) takes priority if both are held.
            float dir = _useHeld ? 1f : (_thumbHeld ? -1f : 0f);
            return motorDegreesPerSecond * Time.deltaTime * dir;
        }

        /// <summary>
        /// Signed CRANK of the controller about the fastener axis since last frame (manual mode).
        /// Projects the hand position onto the plane perpendicular to the axis and measures the
        /// change in its angle around the axis. Always 1:1 (no gain) so the wrench handle — which
        /// is parented to the nut — stays perfectly in sync with the hand. Sign matches the
        /// Fastener's own rotation convention, so positive crank = loosen.
        /// </summary>
        private float CrankDelta()
        {
            var hand = _tool.CurrentInteractor;
            if (hand == null) { _hasPrevCrank = false; return 0f; }

            Vector3 axis = _engaged.WorldAxis;
            Vector3 toHand = hand.transform.position - _engaged.transform.position;
            Vector3 proj = Vector3.ProjectOnPlane(toHand, axis);
            if (proj.sqrMagnitude < 1e-6f) { _hasPrevCrank = false; return 0f; }
            proj.Normalize();

            if (!_hasPrevCrank) { _prevCrankDir = proj; _hasPrevCrank = true; return 0f; }

            float d = Vector3.SignedAngle(_prevCrankDir, proj, axis);
            _prevCrankDir = proj;
            return d;
        }

        private void UpdateEngagement()
        {
            var origin = tip ? tip : transform;
            var f = FindFastener(origin.position);

            if (f == null && _engaged != null && !_engaged.Detached && Time.time <= _graceUntil)
                f = _engaged; // ride out a brief detection gap
            else if (f != null && f == _engaged)
                _graceUntil = Time.time + disengageGrace;

            if (f == _engaged) return;

            if (_engaged != null)
            {
                Log($"DISENGAGE from '{_engaged.name}'");
                onDisengaged.Invoke(_engaged);
            }

            _engaged = f;
            if (_engaged != null)
            {
                _graceUntil = Time.time + disengageGrace;
                Log($"ENGAGE '{_engaged.name}' mode={mode} (progress={_engaged.Progress01:P0}) — hold the button to turn");
                onEngaged.Invoke(_engaged);
            }
        }

        private Fastener FindFastener(Vector3 pos)
        {
            int n = Physics.OverlapSphereNonAlloc(pos, detectionRadius, _hits, fastenerLayers.value);
            for (int i = 0; i < n; i++)
            {
                if (_hits[i] == null) continue;
                var f = _hits[i].GetComponentInParent<Fastener>();
                if (f == null || f.Detached) continue;
                if (!toolMask.Overlaps(f.FastenerMask)) continue; // wrong tool for this fastener
                return f;
            }

            return null;
        }

        private void Log(string msg)
        {
            if (verboseLogs) Debug.Log($"[FastenerTool:{name}] {msg}", this);
        }

        private void OnDrawGizmosSelected()
        {
            var origin = tip ? tip : transform;

            if (detectionRadius > 0f)
            {
                Gizmos.color = new Color(1f, 0.6f, 0f, 0.6f);
                Gizmos.DrawWireSphere(origin.position, detectionRadius);
            }

            // Drive axis (cyan arrow). Set Drive Axis Local so this points along the socket/bit.
            Vector3 driveWorld = origin.TransformDirection(driveAxisLocal);
            if (driveWorld.sqrMagnitude > 1e-6f)
                DrawArrow(origin.position, driveWorld, 0.12f, Color.cyan);

            // When touching a fastener, draw its axis (yellow) so you can eye-ball alignment.
            if (_engaged != null)
                DrawArrow(_engaged.transform.position, _engaged.WorldAxis, 0.12f, Color.yellow);
        }

        private static void DrawArrow(Vector3 start, Vector3 dir, float length, Color color)
        {
            dir = dir.normalized;
            Vector3 end = start + dir * length;
            Gizmos.color = color;
            Gizmos.DrawLine(start, end);

            // simple 4-line arrow head
            Vector3 perp = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) < 0.99f ? Vector3.up : Vector3.right;
            Vector3 a = Vector3.Cross(dir, perp).normalized;
            Vector3 b = Vector3.Cross(dir, a).normalized;
            float head = length * 0.3f;
            Gizmos.DrawLine(end, end - dir * head + a * head * 0.5f);
            Gizmos.DrawLine(end, end - dir * head - a * head * 0.5f);
            Gizmos.DrawLine(end, end - dir * head + b * head * 0.5f);
            Gizmos.DrawLine(end, end - dir * head - b * head * 0.5f);
        }
    }
}

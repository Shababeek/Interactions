using System;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>How much an <see cref="ElasticTetherInteractable"/> reports about itself.</summary>
    public enum TetherLogLevel
    {
        /// <summary>Silent, including detected bad states.</summary>
        Off = 0,

        /// <summary>Only detected bad states, each reported once.</summary>
        Problems = 1,

        /// <summary>Problems plus grab, slip, bump, clack, wake and sleep.</summary>
        Events = 2,

        /// <summary>Events plus a periodic state dump.</summary>
        Verbose = 3,
    }

    /// <summary>
    /// An object hanging from a fixed anchor on an elastic cord. Pulling it stretches the cord
    /// against a rising resistance; releasing it springs back with overshoot and swings like a
    /// pendulum. Yanking past the slip limit rips it out of the hand.
    /// </summary>
    /// <remarks>
    /// The motion is a damped harmonic oscillator solved in the constraint transform's local space,
    /// parameterized as frequency (Hz) and damping ratio (0-1) rather than raw stiffness/damper so
    /// the two knobs stay orthogonal: frequency is how fast it returns, ratio is how much it
    /// overshoots. The same spring carries stretch and swing — a 3D spring toward the rest point
    /// is a pendulum.
    ///
    /// The tether never parents the object to the hand (unlike <see cref="Grabable"/>), because a
    /// tethered object must be able to lag behind and eventually escape the hand. That lag is what
    /// reads as spring tension.
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Elastic Tether")]
    public class ElasticTetherInteractable : ConstrainedInteractableBase
    {
        private const float MaxSolverStep = 1f / 120f;
        private const int MaxSolverSteps = 8;
        private const float HapticInterval = 0.02f;
        private const float InteractorScanInterval = 0.5f;

        [Header("Cord")]
        [Tooltip("Cord mesh that visually connects the anchor to the hanging object. Stretched and aimed automatically. Leave empty for an invisible tether.")]
        [SerializeField] private Transform cordVisual;

        [Tooltip("Local offset from this transform's origin to the point the cord hangs from.")]
        [SerializeField] private Vector3 anchorLocalOffset = Vector3.zero;

        [Tooltip("Thin the cord as it stretches, preserving apparent volume. Off keeps a constant thickness.")]
        [SerializeField] private bool thinCordWhenStretched = true;

        [Header("Spring")]
        [Tooltip("How fast the toy springs back, in oscillations per second. Higher is snappier.")]
        [SerializeField, Range(0.25f, 12f)] private float frequency = 3.5f;

        [Tooltip("How much it overshoots. 1 = no overshoot, 0.3 = several visible bounces, 0 = never settles.")]
        [SerializeField, Range(0f, 1f)] private float dampingRatio = 0.35f;

        [Header("Limits")]
        [Tooltip("Stretch distance in metres at which the cord is considered fully extended. Resistance ramps toward this.")]
        [SerializeField, Min(0.001f)] private float softLimit = 0.12f;

        [Tooltip("Hand distance at which the grab breaks, as a multiple of Soft Limit. The toy rips free and whips back.")]
        [SerializeField, Min(1f)] private float slipLimitMultiplier = 1.6f;

        [Tooltip("Maps how far the hand has pulled (x, in Soft Limits) to how far the toy actually goes (y, in Soft Limits). A curve that flattens near 1 gives asymptotic rubbery resistance.")]
        [SerializeField] private AnimationCurve resistanceCurve = DefaultResistanceCurve();

        [Tooltip("Stop the toy from rising above its anchor. Prevents clipping into the ceiling it hangs from.")]
        [SerializeField] private bool clampToAnchorHeight = true;

        [Header("Rotation")]
        [Tooltip("How strongly the toy tilts to align with the cord direction. 0 keeps it upright, 1 fully aligns.")]
        [SerializeField, Range(0f, 1f)] private float rotationFollow = 1f;

        [Tooltip("Rotation catch-up speed. Lower values let the tilt lag behind the swing, but too low and a fast swing is over before the tilt develops.")]
        [SerializeField, Min(0.1f)] private float rotationSpeed = 30f;

        [Header("Bump")]
        [Tooltip("Let hands knock the toy without grabbing it.")]
        [SerializeField] private bool enableBump = true;

        [Tooltip("Distance from the toy at which a moving hand starts pushing it.")]
        [SerializeField, Min(0.01f)] private float bumpRadius = 0.09f;

        [Tooltip("Fraction of the hand's velocity transferred into the toy on a bump.")]
        [SerializeField, Range(0f, 1f)] private float bumpTransfer = 0.6f;

        [Tooltip("Hand speed below which a bump is ignored, so resting hands don't nudge the toy.")]
        [SerializeField, Min(0f)] private float bumpMinSpeed = 0.15f;

        [Header("Neighbour")]
        [Tooltip("Another tether on the same anchor that this one can collide with. Set on one of the pair; the other is found automatically.")]
        [SerializeField] private ElasticTetherInteractable neighbour;

        [Tooltip("Combined collision radius between this toy and its neighbour.")]
        [SerializeField, Min(0f)] private float neighbourRadius = 0.09f;

        [Tooltip("Bounciness of the neighbour collision. 0 absorbs, 1 fully rebounds.")]
        [SerializeField, Range(0f, 1f)] private float neighbourRestitution = 0.45f;

        [Header("Haptics")]
        [Tooltip("Rumble the controller continuously while pulling, scaled by stretch.")]
        [SerializeField] private bool tensionHaptics = true;

        [Tooltip("Stretch below which no tension rumble plays, so simply holding the toy is silent.")]
        [SerializeField, Range(0f, 1f)] private float hapticStartStretch = 0.2f;

        [Tooltip("Rumble amplitude at full stretch.")]
        [SerializeField, Range(0f, 1f)] private float maxHapticAmplitude = 0.5f;

        [Tooltip("Played once when the toy slips out of the hand. Optional.")]
        [SerializeField] private HapticPattern slipHaptic;

        [Header("Sleep")]
        [Tooltip("Speed in metres/second below which the toy snaps to rest and stops solving.")]
        [SerializeField, Min(0f)] private float sleepSpeed = 0.01f;

        [Tooltip("Displacement in metres below which the toy is considered at rest.")]
        [SerializeField, Min(0f)] private float sleepDistance = 0.001f;

        [Header("Events")]
        [Tooltip("Fired continuously with the current stretch, 0 at rest and 1 at the soft limit.")]
        [SerializeField] private FloatUnityEvent onStretchChanged = new();

        [Tooltip("Fired when the toy is yanked hard enough to rip out of the hand.")]
        [SerializeField] private UnityEvent onSlipRelease = new();

        [Tooltip("Fired when the toy stops moving and goes back to sleep.")]
        [SerializeField] private UnityEvent onSettled = new();

        [Header("Diagnostics")]
        [Tooltip("Problems logs only detected bad states (recommended to leave on). Events adds grab/slip/bump/clack/sleep. Verbose adds a periodic state dump.")]
        [SerializeField] private TetherLogLevel logLevel = TetherLogLevel.Problems;

        [Tooltip("Frames between state dumps at Verbose level.")]
        [SerializeField, Min(1)] private int verboseFrameInterval = 30;

        [Tooltip("Seconds the toy may keep moving while nobody holds it before it is reported as failing to settle.")]
        [SerializeField, Min(1f)] private float stuckAwakeSeconds = 10f;

        [Header("Debug")]
        [ReadOnly, SerializeField] private float normalizedStretch;
        [ReadOnly, SerializeField] private bool asleep = true;

        private Vector3 _restCenterLocal;
        private Vector3 _restDirLocal;
        private float _restLength;
        private Quaternion _restLocalRotation;
        private Vector3 _offset;
        private Vector3 _velocity;
        private Transform _cordPivot;
        private Quaternion _cordBaseRotation = Quaternion.identity;
        private Vector3 _cordTopLocal;
        private Vector3 _cordEndLocalInDie;
        private Vector3 _cordRestAxisLocal = Vector3.down;
        private float _cordRestLength;
        private InteractorBase[] _interactors = Array.Empty<InteractorBase>();
        private readonly System.Collections.Generic.Dictionary<InteractorBase, Vector3> _interactorLastPositions = new();
        private readonly System.Collections.Generic.List<InteractorBase> _staleInteractors = new();
        private float _nextInteractorScan;
        private float _hapticTimer;
        private bool _slipped;
        private bool _slipPending;
        private bool _initialized;

        private readonly System.Collections.Generic.HashSet<string> _reportedProblems = new();
        private int _grabCount, _slipCount, _bumpCount, _clackCount, _wakeCount, _sleepCount, _clampCount, _recoveryCount;
        private float _wokeAtTime;
        private float _lastSettleDuration;
        private float _peakStretchThisGrab;
        private Vector3 _grabAnchorLocal;
        private int _grabFrame = -1;

        /// <summary>Current stretch, 0 at rest and 1 at the soft limit. Exceeds 1 only while held.</summary>
        public float NormalizedStretch => normalizedStretch;

        /// <summary>Fired continuously with the current normalized stretch.</summary>
        public IObservable<float> OnStretchChanged => onStretchChanged.AsObservable();

        /// <summary>Fired when the toy is yanked hard enough to rip out of the hand.</summary>
        public UnityEvent OnSlipRelease => onSlipRelease;

        /// <summary>Fired when the toy stops moving and goes back to sleep.</summary>
        public UnityEvent OnSettled => onSettled;

        /// <summary>Whether the solver is idle because the toy is at rest.</summary>
        public bool IsAsleep => asleep;

        /// <summary>Hand distance at which the grab breaks, in metres.</summary>
        public float SlipLimit => softLimit * slipLimitMultiplier;

        /// <summary>Wakes the solver. Called automatically on grab, bump, and neighbour collision.</summary>
        public void Wake()
        {
            if (asleep)
            {
                _wakeCount++;
                _wokeAtTime = Time.time;
                if (logLevel >= TetherLogLevel.Verbose) LogEvent("wake");
            }
            asleep = false;
        }

        /// <summary>Adds velocity to the toy, in the constraint transform's local space.</summary>
        public void AddImpulse(Vector3 localVelocity)
        {
            _velocity += localVelocity;
            Wake();
        }

        private void Start()
        {
            // The tether drives its own motion every frame rather than using the base class
            // return pipeline, which only runs after a deselect and cannot handle bumps.
            returnWhenDeselected = false;
            Initialize();
        }

        private void Initialize()
        {
            // Runtime only: initializing spawns the CordPivot and reparents the cord under it.
            // Doing that outside play mode writes those changes into the scene or prefab as real
            // edits, leaving stray pivots behind that the author never made.
            if (!Application.isPlaying) return;
            if (_initialized || interactableObject == null) return;

            interactableObject.localPosition = Vector3.zero;
            _restLocalRotation = interactableObject.localRotation;
            _restCenterLocal = ResolveRestCenterLocal();
            _restLength = _restCenterLocal.magnitude;

            if (_restLength < 1e-5f)
            {
                // Nothing hangs below the anchor — without a rest direction there is no cord axis
                // to aim or stretch along, so fall back to straight down.
                _restDirLocal = Vector3.down;
                _restLength = Mathf.Max(_restLength, 1e-5f);
            }
            else
            {
                _restDirLocal = _restCenterLocal / _restLength;
            }

            SetupCordPivot();
            _initialized = true;

            if (logLevel >= TetherLogLevel.Events)
            {
                LogEvent("initialized  anchorLocal=" + anchorLocalOffset.ToString("F4")
                         + " restCenter=" + _restCenterLocal.ToString("F4")
                         + " restLength=" + _restLength.ToString("F4") + "m"
                         + " restDir=" + _restDirLocal.ToString("F3")
                         + " cord=" + (cordVisual != null ? cordVisual.name : "<none>")
                         + " softLimit=" + softLimit.ToString("F3") + " slipLimit=" + SlipLimit.ToString("F3"));
            }
        }

        /// <summary>
        /// The rest position of the hanging mass in constraint space, measured from the anchor.
        /// Derived from renderer bounds so the pivot can sit at the ceiling (as authored) while
        /// the swing still pivots around the visual centre of mass.
        /// </summary>
        private Vector3 ResolveRestCenterLocal()
        {
            var anchorWorld = ConstraintTransform.TransformPoint(anchorLocalOffset);
            var renderers = interactableObject.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return ConstraintTransform.InverseTransformPoint(interactableObject.position)
                       - anchorLocalOffset;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            return ConstraintTransform.InverseTransformPoint(bounds.center)
                   - ConstraintTransform.InverseTransformPoint(anchorWorld);
        }

        /// <summary>
        /// Builds the pivot that aims and stretches the cord.
        /// </summary>
        /// <remarks>
        /// Everything here is measured from the cord's own geometry rather than from the anchor and
        /// the hanging mass's centre. Those are two different segments: the cord runs from the hook
        /// to where it enters the die, the mass vector runs to the die's centre, and they differ by
        /// several degrees. Driving the cord from the mass vector leaves its top drifting off the
        /// hook and its bottom not landing where it enters the die.
        ///
        /// The pivot is placed exactly on the cord's top vertex, so scaling about it pins that end
        /// by construction. The bottom is stored as a point rigidly attached to the die, so it
        /// follows the die's translation and rotation instead of being re-derived each frame.
        /// </remarks>
        private void SetupCordPivot()
        {
            if (cordVisual == null) return;

            Vector3 topWorld, bottomWorld;
            ResolveCordEnds(out topWorld, out bottomWorld);

            _cordTopLocal = ConstraintTransform.InverseTransformPoint(topWorld);
            _cordEndLocalInDie = interactableObject.InverseTransformPoint(bottomWorld);

            var restAxis = ConstraintTransform.InverseTransformPoint(bottomWorld) - _cordTopLocal;
            _cordRestLength = restAxis.magnitude;
            if (_cordRestLength < 1e-5f)
            {
                ReportProblem("cord-degenerate",
                    "the cord's endpoints coincide, so it has no length to stretch along.");
                return;
            }

            _cordRestAxisLocal = restAxis / _cordRestLength;
            _cordBaseRotation = Quaternion.FromToRotation(Vector3.forward, _cordRestAxisLocal);

            _cordPivot = new GameObject("CordPivot").transform;
            _cordPivot.SetParent(ConstraintTransform, false);
            _cordPivot.localPosition = _cordTopLocal;
            _cordPivot.localRotation = _cordBaseRotation;
            _cordPivot.localScale = Vector3.one;

            // Preserve the authored world pose. Because the pivot sits on the cord's top vertex and
            // faces along the cord, the counter-rotation Unity bakes in leaves the geometry running
            // from the pivot origin along +Z — the axis the stretch scales.
            cordVisual.SetParent(_cordPivot, true);
        }

        /// <summary>
        /// Finds the cord's two ends in world space: the vertex nearest the anchor and the one
        /// farthest from it, measured along the direction the toy hangs.
        /// </summary>
        private void ResolveCordEnds(out Vector3 topWorld, out Vector3 bottomWorld)
        {
            var anchorWorld = ConstraintTransform.TransformPoint(anchorLocalOffset);
            var hangDirection = ConstraintTransform.TransformDirection(_restDirLocal).normalized;

            var filter = cordVisual.GetComponent<MeshFilter>();
            var mesh = filter != null ? filter.sharedMesh : null;

            if (mesh == null || !mesh.isReadable || mesh.vertexCount == 0)
            {
                // Bounds are axis-aligned so they only approximate a slanted cord, but they are the
                // only option when the mesh cannot be read.
                var renderer = cordVisual.GetComponent<Renderer>();
                if (renderer == null)
                {
                    topWorld = anchorWorld;
                    bottomWorld = anchorWorld + hangDirection * Mathf.Max(_restLength, 0.01f);
                    ReportProblem("cord-no-geometry",
                        "the cord has neither a readable mesh nor a renderer; its length is a guess.");
                    return;
                }

                // Project the eight bounds corners rather than using the extents magnitude: that is
                // a bounding-sphere radius, which for a thin slanted cord overshoots the real end
                // by centimetres and leaves the pivot off the cord's tip.
                var bounds = renderer.bounds;
                float nearest = float.MaxValue, farthest = float.MinValue;
                for (int corner = 0; corner < 8; corner++)
                {
                    var point = bounds.center + Vector3.Scale(bounds.extents, new Vector3(
                        (corner & 1) == 0 ? -1f : 1f,
                        (corner & 2) == 0 ? -1f : 1f,
                        (corner & 4) == 0 ? -1f : 1f));
                    float projection = Vector3.Dot(point - bounds.center, hangDirection);
                    if (projection < nearest) nearest = projection;
                    if (projection > farthest) farthest = projection;
                }
                topWorld = bounds.center + hangDirection * nearest;
                bottomWorld = bounds.center + hangDirection * farthest;

                ReportProblem("cord-mesh-unreadable",
                    "the cord mesh '" + cordVisual.name + "' is not marked Read/Write, so its ends are "
                    + "estimated from bounds. On a slanted cord that puts the top a few millimetres off "
                    + "the hook. Enable Read/Write on the model import for an exact fit.");
                return;
            }

            var vertices = mesh.vertices;
            var toWorld = cordVisual.localToWorldMatrix;
            float minProjection = float.MaxValue, maxProjection = float.MinValue;
            topWorld = bottomWorld = anchorWorld;

            for (int i = 0; i < vertices.Length; i++)
            {
                var world = toWorld.MultiplyPoint3x4(vertices[i]);
                float projection = Vector3.Dot(world - anchorWorld, hangDirection);
                if (projection < minProjection) { minProjection = projection; topWorld = world; }
                if (projection > maxProjection) { maxProjection = projection; bottomWorld = world; }
            }
        }

        protected override void Update()
        {
            // Runs before base.Update() so a pending slip never lets another movement step
            // execute against a grab that is already over.
            if (_slipPending)
            {
                _slipPending = false;
                var holder = CurrentInteractor;
                if (logLevel >= TetherLogLevel.Events)
                    LogEvent("slip released  holder=" + (holder != null ? holder.name : "<null>"));
                if (holder != null) holder.Release(this);
            }

            bool wasSelected = IsSelected;

            base.Update();

            // The base recovers a selected-but-holderless state by forcing a deselect. Surfacing it
            // matters: it means a holder vanished without releasing, which is a bug somewhere else.
            if (wasSelected && !IsSelected && CurrentInteractor == null)
            {
                _recoveryCount++;
                ReportProblem("holder-vanished",
                    "was selected but the holder disappeared without releasing; state was force-recovered. "
                    + "Usually a camera rig / interactor destroyed or disabled mid-grab.");
            }

            if (!_initialized)
            {
                Initialize();
                if (!_initialized)
                {
                    ReportProblem("not-initialized",
                        "cannot initialize: interactableObject is null, so the toy will never move.");
                    return;
                }
            }

            if (!IsSelected)
            {
                if (enableBump) ApplyBumps();
                if (!asleep) StepSpring(Time.deltaTime);
            }
            else
            {
                _peakStretchThisGrab = Mathf.Max(_peakStretchThisGrab, normalizedStretch);
            }

            ResolveNeighbourCollision();

            if (clampToAnchorHeight) ApplyAnchorClamp();

            ApplyTransforms();
            ReportStretch();
            TrySleep();

            if (logLevel >= TetherLogLevel.Problems) CheckInvariants();
            if (logLevel >= TetherLogLevel.Verbose && Time.frameCount % verboseFrameInterval == 0)
                LogEvent("state  " + BuildStateLine());
        }

        /// <inheritdoc/>
        protected override bool Select()
        {
            Wake();
            _slipped = false;
            _slipPending = false;
            _hapticTimer = 0f;
            _grabCount++;
            _peakStretchThisGrab = 0f;
            _grabFrame = Time.frameCount;

            // Capture the baseline before base.Select() runs, because base.Select() immediately
            // calls HandleObjectMovement — which would otherwise treat the whole grab offset as a
            // pull. Subtracting the toy's current displacement keeps a mid-swing grab from
            // snapping the toy to the hand.
            var interactor = CurrentInteractor;
            if (interactor != null && _initialized)
            {
                _grabAnchorLocal = HandOffsetFromRest(interactor.transform.position) - _offset;
            }
            else
            {
                _grabAnchorLocal = Vector3.zero;
                if (interactor == null)
                    ReportProblem("grab-without-interactor", "Select ran with no CurrentInteractor.");
                else if (!_initialized)
                    ReportProblem("grab-before-init",
                        "grabbed before initialization, so the pull baseline is zero and the grab may slip instantly.");
            }

            if (logLevel >= TetherLogLevel.Events)
            {
                LogEvent("grab #" + _grabCount
                         + "  hand=" + (interactor != null ? interactor.HandIdentifier.ToString() : "<null>")
                         + " handDistFromRest=" + (interactor != null ? HandDistanceFromRest(interactor.transform.position).ToString("F3") : "n/a") + "m"
                         + " grabBaseline=" + _grabAnchorLocal.magnitude.ToString("F3") + "m"
                         + " toyOffsetAtGrab=" + _offset.magnitude.ToString("F3") + "m"
                         + " constraintType=" + (Constrainter != null ? Constrainter.ConstraintType.ToString() : "<no PoseConstrainer>")
                         + " grabPoints=" + (Constrainter != null && Constrainter.GrabPoints != null ? Constrainter.GrabPoints.Count : 0));
            }

            return base.Select();
        }

        /// <summary>Offset of a world position from the toy's rest point, in constraint space.</summary>
        private Vector3 HandOffsetFromRest(Vector3 worldPosition)
        {
            return ConstraintTransform.InverseTransformPoint(worldPosition) - anchorLocalOffset - _restCenterLocal;
        }

        /// <summary>Distance from the toy's rest point to a world position, in metres.</summary>
        private float HandDistanceFromRest(Vector3 worldPosition)
        {
            return HandOffsetFromRest(worldPosition).magnitude;
        }

        /// <inheritdoc/>
        protected override void HandleObjectMovement(Vector3 handWorldPosition)
        {
            if (!_initialized)
            {
                Initialize();
                if (!_initialized) return;
            }

            // Pull is measured from where the hand was when it grabbed, not from the toy's centre.
            // The interactor origin sits wherever the hand happened to close — on a 21cm die that
            // baseline is easily 10-15cm, which measured absolutely would read as a huge pull and
            // trip the slip limit on the grab frame itself.
            var requested = HandOffsetFromRest(handWorldPosition) - _grabAnchorLocal;
            float requestedDistance = requested.magnitude;

            if (requestedDistance > SlipLimit)
            {
                if (!_slipped)
                {
                    bool sameFrameAsGrab = Time.frameCount == _grabFrame;
                    if (sameFrameAsGrab)
                    {
                        ReportProblem("slip-on-grab",
                            "slipped on the same frame as the grab: pull " + requestedDistance.ToString("F3")
                            + "m already exceeded slipLimit " + SlipLimit.ToString("F3")
                            + "m before the hand moved. The grab baseline is wrong, or slipLimit is too small "
                            + "for how far the interactor origin sits from the grab point.");
                    }
                    else if (logLevel >= TetherLogLevel.Events)
                    {
                        LogEvent("slip requested  pull=" + requestedDistance.ToString("F3")
                                 + "m > slipLimit=" + SlipLimit.ToString("F3") + "m"
                                 + "  framesHeld=" + (Time.frameCount - _grabFrame));
                    }
                }
                Slip();
                return;
            }

            Vector3 target;
            if (requestedDistance < 1e-5f)
            {
                target = Vector3.zero;
            }
            else
            {
                float allowed = resistanceCurve.Evaluate(requestedDistance / softLimit) * softLimit;
                target = requested / requestedDistance * allowed;
            }

            float dt = Time.deltaTime;
            if (dt > 1e-5f) _velocity = (target - _offset) / dt;
            _offset = target;

            if (tensionHaptics) StepTensionHaptics(dt);
        }

        /// <inheritdoc/>
        protected override void HandleObjectDeselection()
        {
            // The velocity accumulated while following the hand becomes the release velocity, so a
            // fast yank whips and a gentle let-go drifts.
            Wake();
            if (_slipped)
            {
                onSlipRelease.Invoke();
                if (slipHaptic != null && CurrentInteractor != null)
                    CurrentInteractor.PlayHapticPattern(slipHaptic);
            }

            int framesHeld = _grabFrame >= 0 ? Time.frameCount - _grabFrame : -1;

            // A grab that ends within a frame or two of starting was never a real hold: the player
            // saw the pose snap on and the hand let go again.
            if (framesHeld >= 0 && framesHeld <= 1)
            {
                ReportProblem("instant-release",
                    "released " + framesHeld + " frame(s) after grabbing (" + (_slipped ? "slipped" : "not a slip")
                    + "). The player sees the hand pose appear and immediately drop.");
            }

            if (logLevel >= TetherLogLevel.Events)
            {
                LogEvent("release  " + (_slipped ? "SLIPPED" : "let go")
                         + "  framesHeld=" + framesHeld
                         + " peakStretch=" + _peakStretchThisGrab.ToString("F2")
                         + " releaseSpeed=" + ConstraintTransform.TransformVector(_velocity).magnitude.ToString("F2") + "m/s"
                         + " offset=" + _offset.magnitude.ToString("F3") + "m");
            }

            _slipped = false;
            _slipPending = false;
        }

        /// <inheritdoc/>
        /// <remarks>Unused: the tether solves its own motion in Update, not through the base return pipeline.</remarks>
        protected override void HandleReturnToOriginalPosition() { }

        /// <summary>
        /// Requests the grab be broken. The release itself is deferred to the next Update.
        /// </summary>
        /// <remarks>
        /// Releasing inline would re-enter the state machine from inside HandleObjectMovement,
        /// which the base class calls from Select() as well as Update(). During Select() the
        /// interactable is still in the Hovering state, so the release is processed as a hover
        /// exit and clears CurrentInteractor — and then HandleSelectionState sets isSelected true
        /// on top of that, leaving a selected interactable with no interactor for the rest of the
        /// session. Deferring guarantees the release always runs from the Update path, where the
        /// state machine unwinds cleanly.
        /// </remarks>
        private void Slip()
        {
            if (_slipped) return;
            _slipped = true;
            _slipPending = true;
            _slipCount++;
        }

        private void StepSpring(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            float omega = 2f * Mathf.PI * frequency;
            int steps = Mathf.Clamp(Mathf.CeilToInt(deltaTime / MaxSolverStep), 1, MaxSolverSteps);
            float h = deltaTime / steps;

            for (int i = 0; i < steps; i++)
            {
                // Semi-implicit Euler: stable at the step sizes a spring this stiff needs.
                _velocity += (-2f * dampingRatio * omega * _velocity - omega * omega * _offset) * h;
                _offset += _velocity * h;
            }
        }

        private void ApplyBumps()
        {
            CacheInteractors();
            if (_interactors.Length == 0) return;

            float dt = Time.deltaTime;
            if (dt <= 1e-5f) return;

            var massWorld = ConstraintTransform.TransformPoint(anchorLocalOffset + _restCenterLocal + _offset);

            for (int i = 0; i < _interactors.Length; i++)
            {
                var interactor = _interactors[i];
                if (interactor == null || !interactor.gameObject.activeInHierarchy) continue;

                var position = interactor.transform.position;
                Vector3 lastPosition;
                bool known = _interactorLastPositions.TryGetValue(interactor, out lastPosition);
                _interactorLastPositions[interactor] = position;
                if (!known) continue;

                var handVelocity = (position - lastPosition) / dt;
                if (handVelocity.sqrMagnitude < bumpMinSpeed * bumpMinSpeed) continue;

                var toMass = massWorld - position;
                if (toMass.sqrMagnitude > bumpRadius * bumpRadius) continue;

                // Only the part of the hand's motion heading into the toy pushes it; a hand
                // pulling away must not drag it along.
                float approach = Vector3.Dot(handVelocity, toMass.normalized);
                if (approach <= 0f) continue;

                var push = toMass.normalized * (approach * bumpTransfer);
                AddImpulse(ConstraintTransform.InverseTransformVector(push));

                _bumpCount++;
                if (logLevel >= TetherLogLevel.Events)
                    LogEvent("bump #" + _bumpCount + " by " + interactor.HandIdentifier
                             + "  handSpeed=" + handVelocity.magnitude.ToString("F2") + "m/s"
                             + " approach=" + approach.ToString("F2") + "m/s"
                             + " dist=" + toMass.magnitude.ToString("F3") + "m");
            }
        }

        /// <summary>
        /// Refreshes the list of hands that can bump this toy, on an interval.
        /// </summary>
        /// <remarks>
        /// Rescanning is required rather than caching once: scenes swap the active camera rig, so
        /// the interactors that exist at Start are not the ones present later. The interval caps
        /// what would otherwise be a full scene search every frame — which is exactly what happens
        /// when no rig is active yet, the most common state during loading.
        /// </remarks>
        private void CacheInteractors()
        {
            if (Time.time < _nextInteractorScan) return;
            _nextInteractorScan = Time.time + InteractorScanInterval;

            _interactors = FindObjectsByType<InteractorBase>(FindObjectsSortMode.None);

            // Drop remembered positions for interactors that went away, so a rig that is disabled
            // and re-enabled elsewhere does not register one enormous phantom velocity.
            if (_interactorLastPositions.Count > 0)
            {
                _staleInteractors.Clear();
                foreach (var known in _interactorLastPositions.Keys)
                {
                    if (known == null || !known.gameObject.activeInHierarchy) _staleInteractors.Add(known);
                }
                for (int i = 0; i < _staleInteractors.Count; i++) _interactorLastPositions.Remove(_staleInteractors[i]);
            }
        }

        private void ResolveNeighbourCollision()
        {
            if (neighbour == null || neighbourRadius <= 0f) return;

            // Both halves of a pair see the same overlap; letting only one resolve it keeps the
            // separation from being applied twice.
            if (GetInstanceID() > neighbour.GetInstanceID()) return;
            if (!neighbour._initialized) return;

            var here = MassWorldPosition;
            var there = neighbour.MassWorldPosition;
            var delta = there - here;
            float distance = delta.magnitude;
            if (distance >= neighbourRadius || distance < 1e-5f) return;

            var normal = delta / distance;
            float penetration = neighbourRadius - distance;

            SeparateAlong(-normal * (penetration * 0.5f));
            neighbour.SeparateAlong(normal * (penetration * 0.5f));

            var vHere = ConstraintTransform.TransformVector(_velocity);
            var vThere = neighbour.ConstraintTransform.TransformVector(neighbour._velocity);
            float approach = Vector3.Dot(vThere - vHere, normal);
            if (approach >= 0f) return;

            // Equal masses: swap the normal components, scaled by restitution.
            var exchange = normal * (approach * (1f + neighbourRestitution) * 0.5f);
            AddImpulse(ConstraintTransform.InverseTransformVector(exchange));
            neighbour.AddImpulse(neighbour.ConstraintTransform.InverseTransformVector(-exchange));

            _clackCount++;
            if (logLevel >= TetherLogLevel.Events)
                LogEvent("clack #" + _clackCount + " with " + neighbour.name
                         + "  closingSpeed=" + Mathf.Abs(approach).ToString("F2") + "m/s"
                         + " penetration=" + penetration.ToString("F4") + "m");
        }

        private Vector3 MassWorldPosition =>
            ConstraintTransform.TransformPoint(anchorLocalOffset + _restCenterLocal + _offset);

        private void SeparateAlong(Vector3 worldDelta)
        {
            _offset += ConstraintTransform.InverseTransformVector(worldDelta);
        }

        private void ApplyAnchorClamp()
        {
            // The toy hangs from the anchor and cannot rise through it. Measured along the rest
            // direction so a tilted anchor still behaves.
            float alongCord = Vector3.Dot(_restCenterLocal + _offset, _restDirLocal);
            if (alongCord >= 0f) return;

            _offset -= _restDirLocal * alongCord;
            float velocityAlong = Vector3.Dot(_velocity, _restDirLocal);
            if (velocityAlong < 0f) _velocity -= _restDirLocal * velocityAlong;

            _clampCount++;
            if (logLevel >= TetherLogLevel.Verbose)
                LogEvent("anchor clamp #" + _clampCount + "  pushedBack=" + (-alongCord).ToString("F4") + "m");
        }

        private void ApplyTransforms()
        {
            interactableObject.localPosition = _offset;

            var currentDir = _restCenterLocal + _offset;
            float currentLength = currentDir.magnitude;
            if (currentLength < 1e-5f) return;
            currentDir /= currentLength;

            if (rotationFollow > 0f)
            {
                var aligned = Quaternion.FromToRotation(_restDirLocal, currentDir) * _restLocalRotation;
                var target = Quaternion.Slerp(_restLocalRotation, aligned, rotationFollow);
                interactableObject.localRotation = Quaternion.Slerp(
                    interactableObject.localRotation, target, 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime));
            }

            if (_cordPivot == null) return;

            // The cord chases a point rigidly attached to the die, so it stays plugged into the
            // same spot on the mesh however the die swings or tilts. Read after the position and
            // rotation above are applied, so it uses this frame's die pose, not last frame's.
            var attachLocal = ConstraintTransform.InverseTransformPoint(
                interactableObject.TransformPoint(_cordEndLocalInDie));

            var cordAxis = attachLocal - _cordTopLocal;
            float cordLength = cordAxis.magnitude;
            if (cordLength < 1e-5f) return;
            cordAxis /= cordLength;

            _cordPivot.localRotation = Quaternion.FromToRotation(_cordRestAxisLocal, cordAxis) * _cordBaseRotation;

            // A cord goes slack, it never compresses: on the inward half of an overshoot the mass
            // rides closer to the anchor than the cord is long, and a sub-1 scale there would pump
            // the cord like an accordion instead of leaving it loose.
            float stretch = Mathf.Max(cordLength / _cordRestLength, 1f);
            float thickness = thinCordWhenStretched ? 1f / Mathf.Sqrt(stretch) : 1f;
            _cordPivot.localScale = new Vector3(thickness, thickness, stretch);
        }

        private void ReportStretch()
        {
            float stretch = _offset.magnitude / softLimit;
            if (Mathf.Approximately(stretch, normalizedStretch)) return;
            normalizedStretch = stretch;
            onStretchChanged.Invoke(stretch);
        }

        private void StepTensionHaptics(float deltaTime)
        {
            if (CurrentInteractor == null) return;

            float t = Mathf.InverseLerp(hapticStartStretch, 1f, normalizedStretch);
            if (t <= 0f) return;

            _hapticTimer += deltaTime;
            if (_hapticTimer < HapticInterval) return;
            _hapticTimer = 0f;

            // Impulses slightly outlast the interval so consecutive samples blend into a
            // continuous rumble instead of a stutter.
            CurrentInteractor.SendHapticImpulse(t * maxHapticAmplitude, HapticInterval * 1.5f);
        }

        private void TrySleep()
        {
            if (asleep || IsSelected) return;
            if (_velocity.magnitude > sleepSpeed || _offset.magnitude > sleepDistance) return;

            _offset = Vector3.zero;
            _velocity = Vector3.zero;
            asleep = true;
            _sleepCount++;
            _lastSettleDuration = Time.time - _wokeAtTime;
            ApplyTransforms();
            ReportStretch();
            onSettled.Invoke();

            if (logLevel >= TetherLogLevel.Events)
                LogEvent("settled #" + _sleepCount + "  took=" + _lastSettleDuration.ToString("F2") + "s");
        }

        #region Diagnostics

        private void LogEvent(string message)
        {
            Debug.Log("[Tether] " + name + ": " + message, this);
        }

        /// <summary>
        /// Logs a detected bad state once per distinct key, with a running count on later hits.
        /// </summary>
        /// <remarks>
        /// Reported once rather than per frame: these conditions are almost always persistent, and
        /// a warning every frame buries the first occurrence — which is the one that says what
        /// actually went wrong.
        /// </remarks>
        private void ReportProblem(string key, string message)
        {
            if (logLevel < TetherLogLevel.Problems) return;
            if (!_reportedProblems.Add(key)) return;
            Debug.LogWarning("[Tether] " + name + " PROBLEM (" + key + "): " + message
                             + "\n" + BuildStateLine(), this);
        }

        /// <summary>Checks the states that should be impossible, and names the one that isn't.</summary>
        private void CheckInvariants()
        {
            if (IsSelected && CurrentInteractor == null)
                ReportProblem("selected-without-holder",
                    "IsSelected is true but CurrentInteractor is null. The toy will never move or release.");

            if (!IsSelected && _slipPending)
                ReportProblem("slip-pending-unselected",
                    "a slip is pending while nothing holds the toy; the release will be a no-op.");

            if (float.IsNaN(_offset.x) || float.IsInfinity(_offset.x) ||
                float.IsNaN(_velocity.x) || float.IsInfinity(_velocity.x))
                ReportProblem("non-finite",
                    "offset or velocity went NaN/Infinity — the solver has diverged and the toy will never recover.");

            if (_offset.magnitude > SlipLimit * 3f)
                ReportProblem("runaway",
                    "offset " + _offset.magnitude.ToString("F2") + "m is far beyond the slip limit "
                    + SlipLimit.ToString("F2") + "m; something is driving the toy externally.");

            if (cordVisual != null && _cordPivot == null)
                ReportProblem("cord-pivot-missing",
                    "a cord is assigned but its CordPivot was never created, so the cord cannot stretch.");

            if (_restLength <= 1e-4f)
                ReportProblem("no-rest-length",
                    "the hanging mass sits on top of the anchor, so there is no cord direction. "
                    + "Check anchorLocalOffset and that the visual actually hangs below it.");

            if (softLimit >= SlipLimit)
                ReportProblem("limits-inverted",
                    "softLimit " + softLimit.ToString("F3") + " >= slipLimit " + SlipLimit.ToString("F3")
                    + "; the toy slips before any resistance is felt.");

            if (neighbour != null && neighbour.neighbour != this)
                ReportProblem("neighbour-not-mutual",
                    "neighbour '" + neighbour.name + "' does not point back at this tether, so only "
                    + "one side resolves the collision.");

            if (!asleep && !IsSelected && Time.time - _wokeAtTime > stuckAwakeSeconds)
                ReportProblem("never-settles",
                    "still moving " + stuckAwakeSeconds.ToString("F0") + "s after waking with nobody holding it. "
                    + "Check dampingRatio (0 never settles) and the sleep thresholds.");

            // A non-uniform constraint scale shears the cord: the stretch axis and the two thinning
            // axes end up in different units, so the cord changes thickness as it swings.
            var scale = ConstraintTransform.lossyScale;
            float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            float minScale = Mathf.Min(Mathf.Abs(scale.x), Mathf.Min(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            if (minScale > 1e-6f && maxScale / minScale > 1.01f)
                ReportProblem("non-uniform-scale",
                    "constraint transform scale " + scale.ToString("F3") + " is non-uniform; the cord will "
                    + "change thickness as it swings.");
        }

        private string BuildStateLine()
        {
            var holder = CurrentInteractor;
            return "init=" + _initialized
                   + " selected=" + IsSelected
                   + " holder=" + (holder != null ? holder.HandIdentifier.ToString() : "<null>")
                   + " asleep=" + asleep
                   + " stretch=" + normalizedStretch.ToString("F3")
                   + " offset=" + _offset.magnitude.ToString("F4") + "m"
                   + " speed=" + _velocity.magnitude.ToString("F3") + "m/s"
                   + " cordScale=" + (_cordPivot != null ? _cordPivot.localScale.z.ToString("F3") : "n/a")
                   + " slipPending=" + _slipPending;
        }

        /// <summary>Dumps everything this tether knows about itself. Works in play mode and in the editor.</summary>
        [ContextMenu("Log State Report")]
        public void LogStateReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("[Tether] STATE REPORT for " + name);
            report.AppendLine("  " + BuildStateLine());
            report.AppendLine("  geometry: anchorLocal=" + anchorLocalOffset.ToString("F4")
                              + " restCenter=" + _restCenterLocal.ToString("F4")
                              + " restLength=" + _restLength.ToString("F4") + "m"
                              + " restDir=" + _restDirLocal.ToString("F3"));
            report.AppendLine("  spring: " + frequency.ToString("F2") + "Hz  ratio=" + dampingRatio.ToString("F2")
                              + "  softLimit=" + softLimit.ToString("F3") + "m  slipLimit=" + SlipLimit.ToString("F3") + "m");
            report.AppendLine("  wiring: cord=" + (cordVisual != null ? cordVisual.name : "<none>")
                              + "  cordPivot=" + (_cordPivot != null ? "ok" : "<none>")
                              + "  neighbour=" + (neighbour != null ? neighbour.name : "<none>")
                              + "  mutual=" + (neighbour != null && neighbour.neighbour == this)
                              + "  poseConstrainer=" + (Constrainter != null ? Constrainter.ConstraintType.ToString() : "<missing>")
                              + "  grabPoints=" + (Constrainter != null && Constrainter.GrabPoints != null ? Constrainter.GrabPoints.Count : 0));
            report.AppendLine("  counters: grabs=" + _grabCount + " slips=" + _slipCount + " bumps=" + _bumpCount
                              + " clacks=" + _clackCount + " wakes=" + _wakeCount + " sleeps=" + _sleepCount
                              + " anchorClamps=" + _clampCount + " holderRecoveries=" + _recoveryCount);
            report.AppendLine("  lastSettleDuration=" + _lastSettleDuration.ToString("F2") + "s"
                              + "  peakStretchThisGrab=" + _peakStretchThisGrab.ToString("F2"));
            report.AppendLine("  interactorsTracked=" + _interactors.Length
                              + "  problemsReported=" + _reportedProblems.Count
                              + (_reportedProblems.Count > 0 ? " [" + string.Join(", ", new System.Collections.Generic.List<string>(_reportedProblems).ToArray()) + "]" : ""));
            Debug.Log(report.ToString(), this);
        }

        /// <summary>Clears the reported-problem set so persistent issues are logged again.</summary>
        [ContextMenu("Reset Problem Log")]
        public void ResetProblemLog() => _reportedProblems.Clear();

        #endregion

        [ContextMenu("Simulate Pull")]
        private void SimulatePull()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Simulate Pull only runs in play mode.", this);
                return;
            }

            Initialize();
            var lateral = Vector3.Cross(_restDirLocal, Vector3.right);
            if (lateral.sqrMagnitude < 1e-4f) lateral = Vector3.Cross(_restDirLocal, Vector3.forward);

            _offset = (_restDirLocal + lateral.normalized * 0.5f).normalized * softLimit;
            _velocity = Vector3.zero;
            Wake();
        }

        private static AnimationCurve DefaultResistanceCurve()
        {
            // Near-linear through the usable range, flattening just past the soft limit so the
            // cord always feels rubbery and never hits a wall.
            var curve = new AnimationCurve(
                new Keyframe(0f, 0f, 1f, 1f),
                new Keyframe(1f, 0.85f, 0.45f, 0.45f),
                new Keyframe(2f, 1.05f, 0.06f, 0.06f));
            return curve;
        }

        protected override void Reset()
        {
            base.Reset();
            resistanceCurve = DefaultResistanceCurve();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            slipLimitMultiplier = Mathf.Max(1f, slipLimitMultiplier);
            resistanceCurve ??= DefaultResistanceCurve();
        }

        private void OnDrawGizmosSelected()
        {
            var anchor = transform.TransformPoint(anchorLocalOffset);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(anchor, 0.01f);

            if (interactableObject == null) return;

            var mass = Application.isPlaying ? MassWorldPosition : interactableObject.position;
            Gizmos.DrawLine(anchor, mass);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(mass, softLimit);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mass, SlipLimit);
        }
    }
}

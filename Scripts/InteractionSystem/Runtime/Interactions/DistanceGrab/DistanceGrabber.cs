using System;
using Shababeek.Interactions.Core;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>UnityEvent carrying the affected interactable.</summary>
    [Serializable]
    public class InteractableUnityEvent : UnityEvent<InteractableBase> { }

    /// <summary>
    /// Gravity-gloves style distance grabbing: aim at a far Grabable to highlight it, hold the
    /// prime button, flick the hand to launch the object on a ballistic arc toward the hand, and
    /// it is auto-grabbed on arrival while the button is still held. Near grabbing is untouched —
    /// this component only delivers objects into the regular interactor's reach.
    /// </summary>
    [RequireComponent(typeof(InteractorBase))]
    [AddComponentMenu("Shababeek/Interactions/Interactors/Distance Grabber")]
    public class DistanceGrabber : MonoBehaviour
    {
        private enum GrabState
        {
            Idle,
            Highlighting,
            Primed,
            Launched
        }

        [Header("Aiming")]
        [Tooltip("Origin and direction used for aiming. Defaults to this transform.")]
        [SerializeField] private Transform aimOrigin;

        [Tooltip("Maximum distance at which objects can be distance grabbed.")]
        [SerializeField] private float maxDistance = 6f;

        [Tooltip("Objects closer than this are left to the normal interactor.")]
        [SerializeField] private float minDistance = 0.5f;

        [Tooltip("Radius of the aim sphere-cast; larger is more forgiving at range.")]
        [SerializeField] private float aimRadius = 0.18f;

        [Tooltip("Layers considered when aiming.")]
        [SerializeField] private LayerMask aimLayers = -1;

        [Header("Priming & Flick")]
        [Tooltip("Button held to prime the aimed object for launching.")]
        [SerializeField] private XRButton primeButton = XRButton.Grip;

        [Tooltip("Hand speed in m/s that counts as a flick while primed.")]
        [SerializeField] private float flickSpeedThreshold = 1.2f;

        [Header("Flight")]
        [Tooltip("Seconds of flight per meter of distance to the object.")]
        [SerializeField] private float flightTimePerMeter = 0.12f;

        [Tooltip("Minimum (x) and maximum (y) flight time in seconds.")]
        [SerializeField] private Vector2 flightTimeRange = new(0.35f, 0.9f);

        [Tooltip("Catch point offset from the hand, in hand-local space.")]
        [SerializeField] private Vector3 catchOffset = new(0f, 0.08f, 0f);

        [Tooltip("Homing acceleration toward the hand during flight, in m/s².")]
        [SerializeField] private float homingAcceleration = 8f;

        [Tooltip("Distance from the catch point that counts as arrived.")]
        [SerializeField] private float catchRadius = 0.18f;

        [Header("Events")]
        [Tooltip("Fired when an object is primed for launch.")]
        [SerializeField] private InteractableUnityEvent onPrimed = new();

        [Tooltip("Fired when a primed object is launched toward the hand.")]
        [SerializeField] private InteractableUnityEvent onLaunched = new();

        [Tooltip("Fired when a launched object is caught and grabbed.")]
        [SerializeField] private InteractableUnityEvent onCaught = new();

        [Tooltip("Fired when a launched object misses (timeout or button released).")]
        [SerializeField] private InteractableUnityEvent onMissed = new();

        private InteractorBase _interactor;
        private GrabState _state = GrabState.Idle;
        private Grabable _target;
        private Rigidbody _targetBody;
        private bool _primeHeld;
        private float _flightTimer;
        private Vector3 _handVelocity;
        private Vector3 _lastHandPosition;
        private IDisposable _buttonSubscription;
        private readonly RaycastHit[] _aimHits = new RaycastHit[16];

        /// <summary>The Grabable currently highlighted, primed, or in flight (null when idle).</summary>
        public InteractableBase CurrentTarget => _target;

        private Transform AimTransform => aimOrigin != null ? aimOrigin : transform;

        private void Awake()
        {
            _interactor = GetComponent<InteractorBase>();
        }

        private void OnEnable()
        {
            _lastHandPosition = transform.position;
            var observable = primeButton == XRButton.Grip
                ? _interactor.Hand.OnGripButtonStateChange
                : _interactor.Hand.OnTriggerTriggerButtonStateChange;
            _buttonSubscription = observable?.Do(OnPrimeButton).Subscribe();
        }

        private void OnDisable()
        {
            _buttonSubscription?.Dispose();
            _buttonSubscription = null;
            AbortToIdle(invokeMissed: false);
        }

        private void OnPrimeButton(VRButtonState state)
        {
            switch (state)
            {
                case VRButtonState.Down:
                    _primeHeld = true;
                    if (_state == GrabState.Highlighting && _target != null)
                    {
                        _state = GrabState.Primed;
                        onPrimed.Invoke(_target);
                    }
                    break;
                case VRButtonState.Up:
                    _primeHeld = false;
                    if (_state == GrabState.Primed)
                    {
                        // Releasing the button un-primes; keep the highlight while still aiming.
                        _state = GrabState.Highlighting;
                    }
                    break;
            }
        }

        private void Update()
        {
            UpdateHandVelocity();

            switch (_state)
            {
                case GrabState.Idle:
                case GrabState.Highlighting:
                    if (_interactor.CurrentInteractable != null)
                    {
                        // The normal interactor is busy (hovering or holding) — stand down.
                        ClearTarget();
                        return;
                    }
                    UpdateAim();
                    break;

                case GrabState.Primed:
                    if (!IsTargetStillValid())
                    {
                        ClearTarget();
                        return;
                    }
                    if (_handVelocity.magnitude >= flickSpeedThreshold)
                    {
                        Launch();
                    }
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (_state != GrabState.Launched) return;

            if (_target == null || _targetBody == null || _target.IsSelected)
            {
                // Destroyed or stolen by another hand mid-flight.
                AbortToIdle(invokeMissed: false);
                return;
            }

            Vector3 catchPoint = CatchPoint();
            Vector3 toCatch = catchPoint - _targetBody.position;

            if (toCatch.magnitude <= catchRadius)
            {
                if (_primeHeld && TryHandOff())
                {
                    return;
                }
                Miss();
                return;
            }

            _targetBody.linearVelocity += toCatch.normalized * (homingAcceleration * Time.fixedDeltaTime);

            _flightTimer -= Time.fixedDeltaTime;
            if (_flightTimer <= 0f)
            {
                Miss();
            }
        }

        private void UpdateHandVelocity()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            var instantaneous = (transform.position - _lastHandPosition) / dt;
            _lastHandPosition = transform.position;
            // Light smoothing keeps single-frame tracking spikes from triggering flicks.
            _handVelocity = Vector3.Lerp(_handVelocity, instantaneous, 0.5f);
        }

        private void UpdateAim()
        {
            var candidate = FindAimedGrabable();
            if (candidate == _target)
            {
                _state = _target != null ? GrabState.Highlighting : GrabState.Idle;
                return;
            }

            ClearTarget();
            if (candidate == null) return;

            _target = candidate;
            _targetBody = candidate.GetComponent<Rigidbody>();
            _state = GrabState.Highlighting;

            // Reuse the interactable's hover state so outlines/feedback light up for free.
            if (_target.CurrentState == InteractionState.None)
            {
                _target.OnStateChanged(InteractionState.Hovering, _interactor);
            }
        }

        private Grabable FindAimedGrabable()
        {
            var origin = AimTransform.position;
            var direction = AimTransform.forward;
            int count = Physics.SphereCastNonAlloc(origin, aimRadius, direction, _aimHits, maxDistance, aimLayers);
            if (count == 0) return null;

            Array.Sort(_aimHits, 0, count, RaycastDistanceComparer.Instance);

            for (int i = 0; i < count; i++)
            {
                var hit = _aimHits[i];
                if (hit.collider == null || hit.collider.isTrigger) continue;

                var grabable = hit.collider.GetComponentInParent<Grabable>();
                if (grabable != null && IsEligible(grabable, hit.distance))
                {
                    return grabable;
                }

                // First solid non-grabable hit blocks the aim — no grabbing through walls.
                if (grabable == null) return null;
            }

            return null;
        }

        private bool IsEligible(Grabable grabable, float distance)
        {
            if (distance < minDistance) return false;
            if (grabable.IsSelected) return false;
            if (!grabable.isActiveAndEnabled) return false;
            if (!grabable.CanInteract(_interactor.Hand)) return false;

            var body = grabable.GetComponent<Rigidbody>();
            if (body == null || body.isKinematic) return false;

            var settings = grabable.GetComponent<DistanceGrabbable>();
            return settings == null || settings.AllowDistanceGrab;
        }

        private bool IsTargetStillValid()
        {
            return _target != null && !_target.IsSelected && _target.isActiveAndEnabled;
        }

        private void Launch()
        {
            float distance = Vector3.Distance(_targetBody.position, CatchPoint());
            float flightTime = DistanceGrabMath.ComputeFlightTime(distance, flightTimePerMeter, flightTimeRange);

            var settings = _target.GetComponent<DistanceGrabbable>();
            if (settings != null && settings.FlightTimeOverride > 0f)
            {
                flightTime = settings.FlightTimeOverride;
            }

            Vector3 gravity = _targetBody.useGravity ? Physics.gravity : Vector3.zero;
            _targetBody.linearVelocity =
                DistanceGrabMath.ComputeLaunchVelocity(_targetBody.position, CatchPoint(), flightTime, gravity);
            _targetBody.angularVelocity *= 0.2f;

            _flightTimer = flightTime * 1.6f;
            _state = GrabState.Launched;
            onLaunched.Invoke(_target);
        }

        private bool TryHandOff()
        {
            // End our borrowed hover state before the real selection takes over.
            EndDistanceHover();

            var caught = _target;
            _interactor.CurrentInteractable = caught;
            _interactor.Select();

            if (caught.IsSelected && caught.CurrentInteractor == _interactor)
            {
                onCaught.Invoke(caught);
                ResetToIdle();
                return true;
            }

            // Selection was refused (CanInteract, aborted Select...) — treat as a miss.
            _interactor.CurrentInteractable = null;
            return false;
        }

        private void Miss()
        {
            var missed = _target;
            AbortToIdle(invokeMissed: false);
            if (missed != null) onMissed.Invoke(missed);
        }

        private void AbortToIdle(bool invokeMissed)
        {
            var target = _target;
            ClearTarget();
            if (invokeMissed && target != null) onMissed.Invoke(target);
        }

        private void ClearTarget()
        {
            EndDistanceHover();
            ResetToIdle();
        }

        private void EndDistanceHover()
        {
            if (_target != null &&
                _target.CurrentState == InteractionState.Hovering &&
                _target.CurrentInteractor == _interactor)
            {
                _target.OnStateChanged(InteractionState.None, _interactor);
            }
        }

        private void ResetToIdle()
        {
            _target = null;
            _targetBody = null;
            _state = GrabState.Idle;
        }

        private Vector3 CatchPoint()
        {
            return transform.TransformPoint(catchOffset);
        }

        private class RaycastDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly RaycastDistanceComparer Instance = new();

            public int Compare(RaycastHit a, RaycastHit b)
            {
                // NonAlloc buffers can contain unused (default) entries with null colliders —
                // push them to the end.
                if (a.collider == null) return b.collider == null ? 0 : 1;
                if (b.collider == null) return -1;
                return a.distance.CompareTo(b.distance);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var t = AimTransform;
            Gizmos.color = new Color(0f, 1f, 0.6f, 0.5f);
            Gizmos.DrawRay(t.position, t.forward * maxDistance);
            Gizmos.DrawWireSphere(t.position + t.forward * maxDistance, aimRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.TransformPoint(catchOffset), catchRadius);
        }
    }
}

using System.Threading;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions
{
    /// <summary>
    /// Returns an interactable to its original position/rotation a configurable delay after it
    /// is released from the hand. If the object is grabbed again before the delay elapses, the
    /// pending return is cancelled.
    /// </summary>
    /// <remarks>
    /// Records the spawn pose on Start. Works with any <see cref="InteractableBase"/> (Grabable,
    /// TwoHandedGrabable, ...). When a Rigidbody is present it is temporarily forced kinematic
    /// during the return so physics does not fight the motion, then restored afterwards.
    /// </remarks>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Return To Origin")]
    [RequireComponent(typeof(InteractableBase))]
    public class ReturnToOrigin : MonoBehaviour
    {
        [Tooltip("Seconds to wait after release before the object starts returning.")]
        [SerializeField, Min(0f)] private float returnDelay = 1f;

        [Tooltip("Seconds the return motion takes. 0 = snap instantly to origin.")]
        [SerializeField, Min(0f)] private float returnDuration = 0.25f;

        [Tooltip("Also restore the original parent when returning (useful if the object gets re-parented).")]
        [SerializeField] private bool restoreParent = true;

        [Tooltip("Invoked once the object has finished returning to its origin.")]
        [SerializeField] private UnityEngine.Events.UnityEvent onReturned = new();

        private InteractableBase _interactable;
        private Rigidbody _body;
        private CompositeDisposable _disposable;
        private CancellationTokenSource _returnCts;

        private Vector3 _originPosition;
        private Quaternion _originRotation;
        private Transform _originParent;

        /// <summary>Fired after the object has finished returning to its origin.</summary>
        public UnityEngine.Events.UnityEvent OnReturned => onReturned;

        private void Awake()
        {
            _interactable = GetComponent<InteractableBase>();
            _body = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // Capture the spawn pose after any hierarchy setup (scale compensator, etc.) has run.
            CaptureOrigin();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            // Re-grabbing cancels a pending return; releasing schedules one.
            _interactable.OnSelected.Subscribe(_ => CancelReturn()).AddTo(_disposable);
            _interactable.OnDeselected.Subscribe(_ => ScheduleReturn()).AddTo(_disposable);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
            CancelReturn();
        }

        /// <summary>Records the current pose as the origin the object will return to.</summary>
        public void CaptureOrigin()
        {
            _originPosition = transform.position;
            _originRotation = transform.rotation;
            _originParent = transform.parent;
        }

        /// <summary>Cancels any pending or in-progress return.</summary>
        public void CancelReturn()
        {
            _returnCts?.Cancel();
            _returnCts?.Dispose();
            _returnCts = null;
        }

        /// <summary>Starts the delayed return.</summary>
        public void ScheduleReturn()
        {
            if (!isActiveAndEnabled) return;
            CancelReturn();
            _returnCts = new CancellationTokenSource();
            _ = ReturnAsync(_returnCts.Token);
        }

        private async Awaitable ReturnAsync(CancellationToken token)
        {
            try
            {
                if (returnDelay > 0f)
                    await Awaitable.WaitForSecondsAsync(returnDelay, token);

                // Do not fight the hand: if it got grabbed during the delay, bail out.
                if (_interactable.IsSelected) return;

                bool wasKinematic = false;
                if (_body != null)
                {
                    wasKinematic = _body.isKinematic;
                    _body.linearVelocity = Vector3.zero;
                    _body.angularVelocity = Vector3.zero;
                    _body.isKinematic = true;
                }

                try
                {
                    if (restoreParent && transform.parent != _originParent)
                        transform.SetParent(_originParent, true);

                    if (returnDuration > 0f)
                    {
                        Vector3 startPos = transform.position;
                        Quaternion startRot = transform.rotation;
                        float t = 0f;
                        while (t < returnDuration)
                        {
                            // Abort mid-flight if the object is grabbed again.
                            if (_interactable.IsSelected) return;

                            t += Time.deltaTime;
                            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / returnDuration));
                            transform.position = Vector3.Lerp(startPos, _originPosition, k);
                            transform.rotation = Quaternion.Slerp(startRot, _originRotation, k);
                            await Awaitable.NextFrameAsync(token);
                        }
                    }

                    transform.position = _originPosition;
                    transform.rotation = _originRotation;
                    onReturned.Invoke();
                }
                finally
                {
                    RestoreBody(wasKinematic);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Return was cancelled (re-grabbed or disabled) — expected, nothing to do.
            }
        }

        private void RestoreBody(bool wasKinematic)
        {
            if (_body == null) return;
            // If a grab re-acquired us mid-return, the Grabable now owns the body's
            // kinematic state — don't fight it.
            if (_interactable.IsSelected) return;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _body.isKinematic = wasKinematic;
        }
    }
}

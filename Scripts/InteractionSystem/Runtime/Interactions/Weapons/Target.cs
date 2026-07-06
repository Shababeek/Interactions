using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions.Weapons
{
    /// <summary>
    /// A shooting-range target. When hit by a shot it falls back to a knocked-down pose, holds there
    /// for <see cref="downTime"/> seconds, then rises back to its original local position and
    /// rotation. Implemented with Unity <see cref="Awaitable"/>s (matching the project convention);
    /// the routine is cancelled automatically if the object is destroyed mid-swing.
    ///
    /// Reacts to both hitscan and projectile guns via <see cref="IBulletHittable"/>. Hits are ignored
    /// while the target is already down, so rapid fire won't restart the animation.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Weapons/Target")]
    public class Target : MonoBehaviour, IBulletHittable
    {
        [Header("Knockdown Pose (relative to the target's rest pose)")]
        [Tooltip("Local rotation offset applied when hit. Default tips the target backwards around X.")]
        [SerializeField] private Vector3 knockdownEuler = new(-80f, 0f, 0f);

        [Tooltip("Local position offset applied when hit (e.g. push it back). Zero = rotate in place.")]
        [SerializeField] private Vector3 knockdownOffset = Vector3.zero;

        [Header("Timing")]
        [Tooltip("Seconds to fall into the knocked-down pose.")]
        [SerializeField, Min(0f)] private float fallDuration = 0.12f;

        [Tooltip("Seconds to stay down before rising.")]
        [SerializeField, Min(0f)] private float downTime = 1f;

        [Tooltip("Seconds to rise back up to the rest pose.")]
        [SerializeField, Min(0f)] private float riseDuration = 0.35f;

        [Header("Events")]
        [Tooltip("Fired the moment the target is hit (before it falls).")]
        [SerializeField] private UnityEvent onHit = new();

        private Vector3 _homePosition;
        private Quaternion _homeRotation;
        private bool _isDown;

        /// <summary>True while the target is knocked down or animating.</summary>
        public bool IsDown => _isDown;

        private void Awake()
        {
            _homePosition = transform.localPosition;
            _homeRotation = transform.localRotation;
        }

        /// <inheritdoc/>
        public void OnBulletHit(Vector3 point, Vector3 direction)
        {
            if (_isDown) return;
            onHit.Invoke();
            _ = KnockdownRoutine();
        }

        private async Awaitable KnockdownRoutine()
        {
            _isDown = true;
            CancellationToken token = destroyCancellationToken;
            try
            {
                Vector3 downPosition = _homePosition + knockdownOffset;
                Quaternion downRotation = _homeRotation * Quaternion.Euler(knockdownEuler);

                await LerpLocal(_homePosition, _homeRotation, downPosition, downRotation, fallDuration, token);
                await Awaitable.WaitForSecondsAsync(downTime, token);
                await LerpLocal(downPosition, downRotation, _homePosition, _homeRotation, riseDuration, token);
            }
            finally
            {
                _isDown = false;
            }
        }

        private async Awaitable LerpLocal(Vector3 fromPos, Quaternion fromRot, Vector3 toPos, Quaternion toRot,
            float duration, CancellationToken token)
        {
            if (duration <= 0f)
            {
                transform.SetLocalPositionAndRotation(toPos, toRot);
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.SetLocalPositionAndRotation(
                    Vector3.Lerp(fromPos, toPos, k),
                    Quaternion.Slerp(fromRot, toRot, k));
                await Awaitable.NextFrameAsync();
            }

            transform.SetLocalPositionAndRotation(toPos, toRot);
        }
    }
}

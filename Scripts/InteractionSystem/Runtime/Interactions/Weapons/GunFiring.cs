using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace Shababeek.Interactions.Weapons
{
    /// <summary>
    /// Adds "fire on use" behaviour to any grabbable interactable. Drop this next to a
    /// <see cref="Grabable"/> (or any <see cref="InteractableBase"/>): when the holding hand presses
    /// the "use" input (trigger / index-finger button) the gun fires. Semi-automatic: one shot per
    /// trigger press, rate-limited by <see cref="fireCooldown"/>.
    ///
    /// This is a <b>behaviour component</b>, not an interactable subclass, so a single object can
    /// combine it with other feature components (e.g. a blade for a gunblade) — something class
    /// inheritance could not express.
    ///
    /// Firing supports two modes out of the box:
    ///  - Hitscan (default): raycasts from the muzzle and applies impulse/damage instantly.
    ///  - Projectile: if a projectile prefab is assigned, spawns and launches it from the muzzle.
    ///
    /// Muzzle flash, sound, and the <see cref="onFire"/> UnityEvent let designers wire haptics,
    /// screen shake, and the feedback system without code.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Weapons/Gun Firing")]
    [RequireComponent(typeof(InteractableBase))]
    public class GunFiring : MonoBehaviour
    {
        [Header("Muzzle")]
        [Tooltip("Transform at the barrel tip. Its forward (+Z, blue) axis is the fire direction. Falls back to this object's transform if unset.")]
        [SerializeField] private Transform muzzle;

        [Header("Firing")]
        [Tooltip("Minimum seconds between shots. 0 = fire as fast as the trigger is pressed.")]
        [SerializeField, Min(0f)] private float fireCooldown = 0.12f;

        [Tooltip("Optional bullet prefab. When set, fires a pooled physical bullet instead of a hitscan raycast.")]
        [SerializeField] private Bullet bulletPrefab;

        [Tooltip("Launch speed for the bullet (m/s). Only used when a bullet prefab is set.")]
        [SerializeField, Min(0f)] private float projectileSpeed = 30f;

        [Tooltip("Bullets pre-warmed / kept ready in the pool.")]
        [SerializeField, Min(0)] private int poolDefaultCapacity = 8;

        [Tooltip("Hard cap on pooled bullets; extras beyond this are destroyed on return.")]
        [SerializeField, Min(1)] private int poolMaxSize = 64;

        [Header("Hitscan (used when no projectile prefab)")]
        [Tooltip("Max hitscan range in meters.")]
        [SerializeField, Min(0f)] private float range = 100f;

        [Tooltip("Layers the hitscan can hit.")]
        [SerializeField] private LayerMask hitMask = ~0;

        [Tooltip("Impulse applied to a hit Rigidbody, along the shot direction.")]
        [SerializeField, Min(0f)] private float impactForce = 10f;

        [Tooltip("Optional prefab spawned at the hit point (impact spark/decal). Auto-destroyed after Impact Effect Lifetime.")]
        [SerializeField] private GameObject impactEffectPrefab;

        [SerializeField, Min(0f)] private float impactEffectLifetime = 3f;

        [Header("Feel")]
        [Tooltip("Optional muzzle-flash particle system, played on each shot.")]
        [SerializeField] private ParticleSystem muzzleFlash;

        [Tooltip("Optional AudioSource for the gunshot. PlayOneShot of Fire Clip if set, else just Play().")]
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private AudioClip fireClip;

        [Header("Events")]
        [Tooltip("Fired on every shot. Wire haptics, screen shake, ammo UI, etc.")]
        [SerializeField] private UnityEvent onFire = new();

        [Tooltip("Fired when the trigger is pressed but the gun is on cooldown or out of ammo.")]
        [SerializeField] private UnityEvent onDryFire = new();

        [Header("Ammo (optional)")]
        [Tooltip("Enable a simple ammo count. When off the gun has unlimited ammo.")]
        [SerializeField] private bool useAmmo = false;

        [SerializeField, Min(0)] private int maxAmmo = 12;

        [Header("Debug")]
        [Tooltip("Log fire events to the console.")]
        [SerializeField] private bool debugLogs;

        private InteractableBase _interactable;
        private int _currentAmmo;
        private float _lastFireTime = -999f;
        private CompositeDisposable _disposable;
        private IObjectPool<Bullet> _bulletPool;

        /// <summary>Fired on every successful shot (after the muzzle solve). Arg is the hit point in world space.</summary>
        public event Action<Vector3> OnShotFired;

        /// <summary>Current ammo in the magazine. Only meaningful when Use Ammo is enabled.</summary>
        public int CurrentAmmo => _currentAmmo;

        private Transform MuzzlePoint => muzzle != null ? muzzle : transform;

        private void Awake()
        {
            _interactable = GetComponent<InteractableBase>();
            _currentAmmo = maxAmmo;

            if (bulletPrefab != null)
            {
                _bulletPool = new ObjectPool<Bullet>(
                    createFunc: () =>
                    {
                        var bullet = Instantiate(bulletPrefab);
                        bullet.SetPool(_bulletPool);
                        bullet.gameObject.SetActive(false);
                        return bullet;
                    },
                    actionOnGet: b => b.gameObject.SetActive(true),
                    actionOnRelease: b => b.gameObject.SetActive(false),
                    actionOnDestroy: b => { if (b != null) Destroy(b.gameObject); },
                    collectionCheck: false,
                    defaultCapacity: poolDefaultCapacity,
                    maxSize: poolMaxSize);
            }
        }

        private void OnDestroy()
        {
            _bulletPool?.Clear();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            // Fire when the holding hand presses the use (trigger) input.
            _interactable.OnUseStarted.Subscribe(_ => TryFire()).AddTo(_disposable);
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        /// <summary>Attempts a shot. Returns true if a round was fired.</summary>
        public bool TryFire()
        {
            if (!_interactable.IsSelected) return false;

            if (Time.time - _lastFireTime < fireCooldown)
            {
                return false;
            }

            if (useAmmo && _currentAmmo <= 0)
            {
                onDryFire.Invoke();
                return false;
            }

            _lastFireTime = Time.time;
            if (useAmmo) _currentAmmo--;

            Vector3 origin = MuzzlePoint.position;
            Vector3 direction = MuzzlePoint.forward;

            if (_bulletPool != null)
            {
                FireBullet(origin, direction);
                OnShotFired?.Invoke(origin + direction * range);
            }
            else
            {
                Vector3 hitPoint = FireHitscan(origin, direction);
                OnShotFired?.Invoke(hitPoint);
            }

            PlayShotFeel();
            onFire.Invoke();
            return true;
        }

        /// <summary>Refill ammo to the magazine max.</summary>
        public void Reload()
        {
            _currentAmmo = maxAmmo;
        }

        private void FireBullet(Vector3 origin, Vector3 direction)
        {
            var bullet = _bulletPool.Get();
            if (debugLogs)
                Debug.Log($"[GunFiring] Fire bullet from muzzle '{MuzzlePoint.name}' origin={origin} " +
                          $"dir={direction} speed={projectileSpeed}", this);
            bullet.Launch(origin, direction, projectileSpeed);
        }

        private Vector3 FireHitscan(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(origin, direction, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                var hittable = hit.collider.GetComponentInParent<IBulletHittable>();
                hittable?.OnBulletHit(hit.point, direction);

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForceAtPosition(direction * impactForce, hit.point, ForceMode.Impulse);
                }

                if (impactEffectPrefab != null)
                {
                    var fx = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    if (impactEffectLifetime > 0f) Destroy(fx, impactEffectLifetime);
                }

                return hit.point;
            }

            return origin + direction * range;
        }

        private void PlayShotFeel()
        {
            if (muzzleFlash != null) muzzleFlash.Play();

            if (audioSource != null)
            {
                if (fireClip != null) audioSource.PlayOneShot(fireClip);
                else audioSource.Play();
            }
        }

        private void OnDrawGizmosSelected()
        {
            var m = MuzzlePoint;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(m.position, m.position + m.forward * Mathf.Min(range, 5f));
        }
    }
}

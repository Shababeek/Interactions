using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions
{
    /// <summary>
    /// A grabbable pistol that fires when the holding hand presses the "use" input
    /// (the trigger / index-finger button). Semi-automatic: one shot per trigger press,
    /// rate-limited by <see cref="fireCooldown"/>.
    ///
    /// Firing supports two modes out of the box:
    ///  - Hitscan (default): raycasts from the muzzle and applies impulse/damage instantly.
    ///  - Projectile: if a projectile prefab is assigned, spawns and launches it from the muzzle.
    ///
    /// Muzzle flash, sound, and the <see cref="onFire"/> UnityEvent let designers wire haptics,
    /// screen shake, and the feedback system without code.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Interactables/Pistol")]
    public class PistolInteractable : Grabable
    {
        [Header("Muzzle")]
        [Tooltip("Transform at the barrel tip. Its forward (+Z, blue) axis is the fire direction. Falls back to this object's transform if unset.")]
        [SerializeField] private Transform muzzle;

        [Header("Firing")]
        [Tooltip("Minimum seconds between shots. 0 = fire as fast as the trigger is pressed.")]
        [SerializeField, Min(0f)] private float fireCooldown = 0.12f;

        [Tooltip("Optional projectile prefab. When set, fires a physical projectile instead of a hitscan raycast.")]
        [SerializeField] private Rigidbody projectilePrefab;

        [Tooltip("Launch speed for the projectile (m/s). Only used when a projectile prefab is set.")]
        [SerializeField, Min(0f)] private float projectileSpeed = 30f;

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
        [Tooltip("Enable a simple ammo count. When off the pistol has unlimited ammo.")]
        [SerializeField] private bool useAmmo = false;

        [SerializeField, Min(0)] private int maxAmmo = 12;

        private int _currentAmmo;
        private float _lastFireTime = -999f;

        /// <summary>Fired on every successful shot (after the muzzle solve). Arg is the hit point in world space.</summary>
        public event Action<Vector3> OnShotFired;

        /// <summary>Current ammo in the magazine. Only meaningful when Use Ammo is enabled.</summary>
        public int CurrentAmmo => _currentAmmo;

        private Transform MuzzlePoint => muzzle != null ? muzzle : transform;

        protected override void InitializeInteractable()
        {
            base.InitializeInteractable();
            _currentAmmo = maxAmmo;
        }

        /// <inheritdoc/>
        // Called by the interaction system when the holding hand presses the use (trigger) input.
        protected override void UseStarted()
        {
            TryFire();
        }

        /// <summary>Attempts a shot. Returns true if a round was fired.</summary>
        public bool TryFire()
        {
            if (!IsSelected) return false;

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

            if (projectilePrefab != null)
            {
                FireProjectile(origin, direction);
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

        private void FireProjectile(Vector3 origin, Vector3 direction)
        {
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
            projectile.linearVelocity = direction * projectileSpeed;
        }

        private Vector3 FireHitscan(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(origin, direction, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Interactions.Weapons
{
    /// <summary>
    /// Adds swing-based melee hit detection to any grabbable interactable. Drop this next to a
    /// <see cref="Grabable"/> or <see cref="TwoHandedGrabable"/> and put one or more <b>trigger</b>
    /// colliders along the blade.
    ///
    /// This is a <b>behaviour component</b>, not an interactable subclass, so it does not force the
    /// object to be two-handed — a one-handed dagger and a two-handed greatsword both just add this
    /// component next to whichever grab they use. It can also coexist with other feature components
    /// (e.g. <see cref="GunFiring"/> for a gunblade).
    ///
    /// A hit only registers when the blade is actually moving faster than <see cref="minSwingSpeed"/>,
    /// so resting the blade on something does nothing. Each target is rate-limited by
    /// <see cref="perTargetCooldown"/> so a single swing lands one hit per object.
    ///
    /// The <see cref="onHit"/> UnityEvent and <see cref="OnBladeHit"/> event let designers wire
    /// damage, sparks, haptics, hit-stop, and sound without code.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Weapons/Blade Hit")]
    [RequireComponent(typeof(InteractableBase))]
    public class BladeHit : MonoBehaviour
    {
        [Header("Blade")]
        [Tooltip("Transform at the blade tip. Its motion measures swing speed. Falls back to this transform if unset.")]
        [SerializeField] private Transform bladeTip;

        [Tooltip("Minimum blade-tip speed (m/s) for a contact to count as a hit. Prevents 'resting' hits.")]
        [SerializeField, Min(0f)] private float minSwingSpeed = 1.5f;

        [Tooltip("Layers the blade can hit.")]
        [SerializeField] private LayerMask hitMask = ~0;

        [Tooltip("Seconds before the SAME target can be hit again by this blade.")]
        [SerializeField, Min(0f)] private float perTargetCooldown = 0.3f;

        [Header("Impact")]
        [Tooltip("Base impulse applied to a hit Rigidbody along the swing direction.")]
        [SerializeField, Min(0f)] private float impactForce = 6f;

        [Tooltip("Multiply impact force by blade speed for heavier fast swings.")]
        [SerializeField] private bool scaleForceWithSpeed = true;

        [Tooltip("Optional prefab spawned at the contact point (spark/slash). Auto-destroyed after Hit Effect Lifetime.")]
        [SerializeField] private GameObject hitEffectPrefab;

        [SerializeField, Min(0f)] private float hitEffectLifetime = 3f;

        [Header("Feel")]
        [Tooltip("Optional AudioSource for the impact sound.")]
        [SerializeField] private AudioSource audioSource;

        [SerializeField] private AudioClip hitClip;

        [Header("Events")]
        [Tooltip("Fired on every registered blade hit. Wire damage, haptics, hit-stop, screen shake.")]
        [SerializeField] private UnityEvent onHit = new();

        private InteractableBase _interactable;

        // Per-collider last-hit time so one swing lands one hit per target.
        private readonly Dictionary<Collider, float> _lastHitTimes = new();
        private Vector3 _lastBladePos;
        private float _bladeSpeed;
        private bool _hasLastPos;

        /// <summary>Current blade-tip speed in m/s (updated while held).</summary>
        public float BladeSpeed => _bladeSpeed;

        /// <summary>Fired on every registered hit. Args: the collider hit, and the contact point.</summary>
        public event Action<Collider, Vector3> OnBladeHit;

        private Transform Tip => bladeTip != null ? bladeTip : transform;

        private void Awake()
        {
            _interactable = GetComponent<InteractableBase>();
        }

        private void OnDisable()
        {
            _hasLastPos = false;
            _bladeSpeed = 0f;
        }

        // Track blade-tip speed manually: the blade is kinematic while held and moved by the
        // grab solver, so Rigidbody.linearVelocity stays zero.
        private void Update()
        {
            if (!_interactable.IsSelected)
            {
                _hasLastPos = false;
                _bladeSpeed = 0f;
                return;
            }

            Vector3 pos = Tip.position;
            if (_hasLastPos && Time.deltaTime > 0f)
            {
                _bladeSpeed = (pos - _lastBladePos).magnitude / Time.deltaTime;
            }
            _lastBladePos = pos;
            _hasLastPos = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_interactable.IsSelected) return;
            if (_bladeSpeed < minSwingSpeed) return;
            if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

            // Ignore the hands / the interactor(s) holding us.
            if (_interactable.IsHeldBy(other.transform)) return;

            if (_lastHitTimes.TryGetValue(other, out float last) && Time.time - last < perTargetCooldown)
            {
                return;
            }
            _lastHitTimes[other] = Time.time;

            Vector3 contact = other.ClosestPoint(Tip.position);
            Vector3 swingDir = (Tip.position - _lastBladePos).sqrMagnitude > 1e-6f
                ? (Tip.position - _lastBladePos).normalized
                : Tip.forward;

            if (other.attachedRigidbody != null && !other.attachedRigidbody.isKinematic)
            {
                float force = scaleForceWithSpeed ? impactForce * _bladeSpeed : impactForce;
                other.attachedRigidbody.AddForceAtPosition(swingDir * force, contact, ForceMode.Impulse);
            }

            if (hitEffectPrefab != null)
            {
                var fx = Instantiate(hitEffectPrefab, contact, Quaternion.LookRotation(swingDir));
                if (hitEffectLifetime > 0f) Destroy(fx, hitEffectLifetime);
            }

            if (audioSource != null)
            {
                if (hitClip != null) audioSource.PlayOneShot(hitClip);
                else audioSource.Play();
            }

            OnBladeHit?.Invoke(other, contact);
            onHit.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            var t = Tip;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(t.position, 0.03f);
        }
    }
}

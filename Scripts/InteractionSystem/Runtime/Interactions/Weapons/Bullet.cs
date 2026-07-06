using UnityEngine;
using UnityEngine.Pool;

namespace Shababeek.Interactions.Weapons
{
    /// <summary>
    /// A physical, pooled projectile launched by <see cref="GunFiring"/>. Flies under Rigidbody
    /// velocity, reports the hit to any <see cref="IBulletHittable"/> it strikes, spawns an optional
    /// impact effect, and returns itself to its pool (or self-destructs when unpooled). Auto-returns
    /// after <see cref="lifetime"/> if it never hits anything.
    ///
    /// The pool is owned by the firing gun; this component just needs its release handle via
    /// <see cref="SetPool"/>. Continuous-speculative collision avoids fast bullets tunnelling
    /// through thin targets with no setup required on the target side.
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Weapons/Bullet")]
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        [Tooltip("Seconds before an unhit bullet returns to the pool.")]
        [SerializeField, Min(0.1f)] private float lifetime = 5f;

        [Tooltip("Impulse applied to a hit Rigidbody along the travel direction.")]
        [SerializeField, Min(0f)] private float impactForce = 10f;

        [Tooltip("Optional prefab spawned at the impact point. Auto-destroyed after Hit Effect Lifetime.")]
        [SerializeField] private GameObject hitEffectPrefab;

        [SerializeField, Min(0f)] private float hitEffectLifetime = 3f;

        [Header("Pass-Through")]
        [Tooltip("Layers the bullet flies through instead of hitting (does NOT release). Set this to the " +
                 "hand/interactor layers so a freshly-fired bullet doesn't instantly hit the shooter's hand " +
                 "or the held gun (the gun adopts the hand's layer while grabbed).")]
        [SerializeField] private LayerMask passThroughLayers;

        [Header("Debug")]
        [Tooltip("Log launch / collision / release events to the console.")]
        [SerializeField] private bool debugLogs;

        private Rigidbody _body;
        private Collider[] _colliders;
        private IObjectPool<Bullet> _pool;
        private bool _released;
        private float _despawnTime;
        private Vector3 _launchPosition;
        private Vector3 _launchDirection;
        private Vector3 _prevDrawPosition;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true);
            // Robust against tunnelling for fast shots; works against static, kinematic and
            // dynamic colliders without any per-target configuration.
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        /// <summary>Gives the bullet the pool to return to. Called once when the pool creates it.</summary>
        public void SetPool(IObjectPool<Bullet> pool) => _pool = pool;

        /// <summary>Places the bullet at the muzzle and sends it flying. Resets prior flight state.</summary>
        public void Launch(Vector3 position, Vector3 direction, float speed)
        {
            var rotation = Quaternion.LookRotation(direction);
            transform.SetPositionAndRotation(position, rotation);
            _body.position = position;
            _body.rotation = rotation;
            _body.linearVelocity = direction * speed;
            _body.angularVelocity = Vector3.zero;

            _released = false;
            _despawnTime = Time.time + lifetime;
            _launchPosition = position;
            _launchDirection = direction.normalized;
            _prevDrawPosition = position;

            if (debugLogs)
                Debug.Log($"[Bullet] Launch pos={position} speed={speed} vel={_body.linearVelocity} " +
                          $"gravity={_body.useGravity} layer={LayerMask.LayerToName(gameObject.layer)}", this);
        }

        private void FixedUpdate()
        {
            if (_released) return;

            // Persistent path trail visible in the Scene view (Gizmos on) — shows the real flight path.
            Debug.DrawLine(_prevDrawPosition, transform.position, Color.yellow, 2f);
            _prevDrawPosition = transform.position;

            if (Time.time >= _despawnTime)
            {
                if (debugLogs) Debug.Log($"[Bullet] Despawn (lifetime {lifetime}s elapsed, no hit)", this);
                Release();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_released) return;

            // Fly through hands / the held gun: permanently ignore this collider pair (pool-safe —
            // the same pooled bullet always wants to ignore these) and keep going without releasing.
            if ((passThroughLayers.value & (1 << collision.collider.gameObject.layer)) != 0)
            {
                IgnoreCollisionWith(collision.collider);
                if (debugLogs)
                    Debug.Log($"[Bullet] Pass-through '{collision.collider.name}' " +
                              $"(layer={LayerMask.LayerToName(collision.collider.gameObject.layer)}) — keeps flying", this);
                return;
            }

            if (debugLogs)
                Debug.Log($"[Bullet] Hit '{collision.collider.name}' " +
                          $"(layer={LayerMask.LayerToName(collision.collider.gameObject.layer)}) " +
                          $"{Time.time - (_despawnTime - lifetime):0.000}s after launch", this);

            var contact = collision.GetContact(0);
            Vector3 direction = _body.linearVelocity.sqrMagnitude > 1e-4f
                ? _body.linearVelocity.normalized
                : transform.forward;

            var hittable = collision.collider.GetComponentInParent<IBulletHittable>();
            hittable?.OnBulletHit(contact.point, direction);

            if (collision.rigidbody != null && !collision.rigidbody.isKinematic)
            {
                collision.rigidbody.AddForceAtPosition(direction * impactForce, contact.point, ForceMode.Impulse);
            }

            if (hitEffectPrefab != null)
            {
                var fx = Instantiate(hitEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
                if (hitEffectLifetime > 0f) Destroy(fx, hitEffectLifetime);
            }

            Release();
        }

        // Live direction gizmos: green = current velocity, cyan = launch direction, and a line back
        // to the muzzle so you can see where the shot originated relative to where it's going.
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Vector3 pos = transform.position;

            if (_body != null && _body.linearVelocity.sqrMagnitude > 1e-4f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(pos, _body.linearVelocity.normalized * 0.5f);
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(pos, _launchDirection * 0.3f);

            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(_launchPosition, pos);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pos, 0.02f);
        }

        private void IgnoreCollisionWith(Collider other)
        {
            foreach (var col in _colliders)
            {
                if (col != null) Physics.IgnoreCollision(col, other, true);
            }
        }

        private void Release()
        {
            if (_released) return;
            _released = true;

            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;

            if (_pool != null) _pool.Release(this);
            else Destroy(gameObject);
        }
    }
}

using UnityEngine;

namespace Shababeek.Interactions.Weapons
{
    /// <summary>
    /// Implement on anything that should react to being shot. Both the hitscan path in
    /// <see cref="GunFiring"/> and the physical <see cref="Bullet"/> look this up (via
    /// GetComponentInParent) on whatever they hit and call <see cref="OnBulletHit"/>, so a target
    /// works the same whether the gun is hitscan or projectile.
    /// </summary>
    public interface IBulletHittable
    {
        /// <summary>Called when a shot lands. <paramref name="point"/> is the world contact point,
        /// <paramref name="direction"/> the shot's travel direction (normalized).</summary>
        void OnBulletHit(Vector3 point, Vector3 direction);
    }
}

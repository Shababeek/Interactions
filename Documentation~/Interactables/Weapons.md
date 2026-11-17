# Weapons — Guns, Bullets, Blades, Targets

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Weapons
> **Use For:** Shooting ranges, combat prototypes, anything fired or swung while held

---

## What It Does

The weapon components are **add-ons**. They sit beside a [Grabable](Grabable.md) or [Two Handed Grabable](TwoHandedGrabable.md) and react to that object's events, so any grabbable object can become a weapon without changing how it is held.

| Component | Job |
|---|---|
| **Gun Firing** | Fires when the holding hand presses the use button (trigger). Hitscan or physical bullets. |
| **Bullet** | A pooled physical projectile launched by Gun Firing. |
| **Blade Hit** | Detects swings from trigger colliders along a blade. |
| **Target** | A knock-down target that reacts to being shot. |

---

## Gun Firing

Add next to the interactable on the gun. One shot per trigger press, limited by **Fire Cooldown**.

### Muzzle and Firing

| Field | What it does |
|---|---|
| **Muzzle** | Transform at the barrel tip. Its forward (blue) axis is the fire direction. Uses this object if left empty. |
| **Fire Cooldown** | Minimum seconds between shots. 0 fires as fast as the trigger is pressed. |
| **Bullet Prefab** | Optional. When set, the gun fires pooled physical bullets. When empty, it fires an instant raycast (hitscan). |
| **Projectile Speed** | Bullet launch speed in m/s. Only used with a Bullet Prefab. |
| **Pool Default Capacity / Pool Max Size** | Bullets kept ready, and the most the pool will hold. |

### Hitscan (used when there is no Bullet Prefab)

| Field | What it does |
|---|---|
| **Range** | Maximum distance in metres. |
| **Hit Mask** | Layers the shot can hit. |
| **Impact Force** | Push applied to a Rigidbody that is hit. |
| **Impact Effect Prefab / Lifetime** | Optional effect spawned at the hit point, removed after the lifetime. |

### Feel, Events, Ammo

| Field | What it does |
|---|---|
| **Muzzle Flash** | Optional particle system played per shot. |
| **Audio Source / Fire Clip** | Optional gunshot sound. |
| **On Fire** | Fired on every shot. Wire haptics, recoil, ammo UI. |
| **On Dry Fire** | Fired when the trigger is pressed with no ammo. |
| **Use Ammo / Max Ammo** | Optional magazine. Off means unlimited. Call `Reload()` to refill. |
| **Debug Logs** | Logs fire events to the Console. Leave off. |

```csharp
var gun = GetComponent<GunFiring>();
gun.OnShotFired += hitPoint => Debug.Log(hitPoint);
gun.Reload();
int rounds = gun.CurrentAmmo;
```

> **Pistol Interactable** (Interactables > Pistol) is an older all-in-one version: a Grabable with the same firing settings built in. Prefer **Grabable + Gun Firing** for new work.

---

## Bullet

Put on the bullet prefab (requires a Rigidbody).

| Field | What it does |
|---|---|
| **Lifetime** | Seconds before a bullet that hit nothing returns to the pool. |
| **Impact Force** | Push applied to a Rigidbody that is hit. |
| **Hit Effect Prefab / Lifetime** | Optional impact effect. |
| **Pass Through Layers** | Layers the bullet ignores, for example the hands and the gun itself. |
| **Debug Logs** | Logs launch, collision and release. Leave off. |

---

## Blade Hit

Add next to the interactable on a sword, knife or bat, and place one or more **trigger** colliders along the blade. A contact only counts while the blade is moving fast enough.

| Field | What it does |
|---|---|
| **Blade Tip** | Transform at the tip; its motion measures swing speed. |
| **Min Swing Speed** | Tip speed in m/s below which contacts are ignored, so a resting blade does not hit. |
| **Hit Mask** | Layers the blade can hit. |
| **Per Target Cooldown** | Seconds before the same target can be hit again. |
| **Impact Force / Scale Force With Speed** | Push applied to a hit Rigidbody, optionally stronger for faster swings. |
| **Hit Effect Prefab / Lifetime**, **Audio Source / Hit Clip** | Optional impact effect and sound. |
| **On Hit** | Fired on every registered hit. |

---

## Target

A shooting-range target. When shot it falls to a knocked-down pose, waits, then rises again.

| Field | What it does |
|---|---|
| **Knockdown Euler / Offset** | Rotation and position change applied when hit, relative to the rest pose. |
| **Fall Duration / Down Time / Rise Duration** | Timing of the fall, the wait and the recovery. |
| **On Hit** | Fired the moment the target is hit. |

### Making your own shootable object

Implement `IBulletHittable` on any component. Both hitscan shots and physical bullets look for it on what they hit (including parents) and call it.

```csharp
public class Balloon : MonoBehaviour, IBulletHittable
{
    public void OnBulletHit(Vector3 point, Vector3 direction) => Destroy(gameObject);
}
```

---

## Related Documentation

- [Grabable](Grabable.md) — Makes the weapon holdable
- [Two Handed Grabable](TwoHandedGrabable.md) — Long guns and two-handed blades
- [Haptic Patterns](../Systems/Haptics.md) — Recoil and impact feel
- [Feedback System](../Systems/FeedbackSystem.md) — Audio and visual feedback on events

---

**Last Updated:** November 2026

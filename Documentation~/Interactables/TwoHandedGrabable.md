# Two Handed Grabable — Rifles, Poles, Heavy Tools

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Interactables > Two Handed Grabable
> **Use For:** Objects held with one hand that a second hand can steady or aim
> **Requires:** PoseConstrainer (added automatically), a Collider; a Rigidbody if it should be throwable

---

## What It Does

A **Two Handed Grabable** can be picked up with one hand and then gripped by the other. With one hand it follows that hand. With two, the object pivots on the first hand and aims along the line to the second, the way a rifle or a broom handle does. If the first hand lets go, the second hand takes over and keeps holding the object.

Both hands are shown as posed hands attached to the grips while the real hands are hidden, so the grips always look right.

**Perfect for:** rifles and long guns, poles and staffs, two-handed tools, anything long enough to need steadying.

**Don't use for:** small props (use [Grabable](Grabable.md)), steering wheels (the [Wheel](ConstrainedInteractables.md#wheel-interactable) accepts a second hand on its own).

---

## Inspector Reference

### Two-Handed

| Field | What it does |
|---|---|
| **Require Two Hands** | When on, the object can be held with one hand but is not "functional" until both hands grip it. Read `IsFunctional` from your own scripts to gate behaviour such as firing. |
| **Follow Smoothing** | How quickly the object catches up with the hands. Higher snaps faster. Default 25. |

### Throwing

| Field | What it does |
|---|---|
| **Can Be Thrown** | Tracks velocity while held and applies it on release. Needs a Rigidbody. |
| **Throwable** | Throw settings, the same block as on [Grabable](Grabable.md). A throw is not applied when the object is handed from one hand to the other. |

### Events

| Event | Fires when |
|---|---|
| **On Secondary Grab Start** | The second hand grips the object |
| **On Secondary Grab End** | The second hand releases, or is promoted to the main hand |

The standard interactable events (On Selected, On Deselected, hover, use) work as on any interactable and refer to the main hand.

---

## Setup

1. Add **Two Handed Grabable** to the root of the object.
2. Put the visible model and its colliders under the generated **interactableObject** child, as with the other constrained interactables.
3. On the **PoseConstrainer**, add a grab point for each grip (for example the pistol grip and the fore-end) and pose each hand on it. Each hand claims the nearest free grab point, so the two hands never land on the same one.
4. Press Play. Grab with one hand, then grip with the other.

---

## Scripting API

```csharp
var rifle = GetComponent<TwoHandedGrabable>();

if (rifle.IsTwoHanded) { /* both hands are on it */ }
if (rifle.IsFunctional) { /* two hands, or two hands not required */ }
```

---

## Troubleshooting

| Problem | Check |
|---|---|
| Second hand does nothing | The second hand must be within reach of the object and press the same selection button. The PoseConstrainer needs a free grab point for it. |
| Object lags behind the hands | Raise **Follow Smoothing**. |

---

## Related Documentation

- [Grabable](Grabable.md) — One-handed pick up and throw
- [PoseConstrainer](../PoseSystem/PoseConstrainer.md) — Grab points and hand poses
- [Weapons](Weapons.md) — Add firing or blade hits to a held object
- [Two-handed grabbing design notes](../TwoHandedGrabbing-Design.md) — How it works internally

---

**Last Updated:** November 2026

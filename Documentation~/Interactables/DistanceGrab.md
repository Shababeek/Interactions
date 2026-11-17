# Distance Grab — Pull Objects to Your Hand

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Interactors > Distance Grabber (on the hand)
> **Optional:** Component > Shababeek > Interactions > Interactables > Distance Grabbable (on an object)
> **Use For:** Picking up objects that are out of reach

---

## What It Does

**Distance Grabber** adds gravity-glove style grabbing to a hand:

1. **Aim** at a far Grabable. It highlights.
2. **Hold** the prime button (Grip by default).
3. **Flick** the hand. The object launches on an arc toward the hand.
4. Keep the button held and the object is **grabbed on arrival**.

Normal close-range grabbing is not affected. Every [Grabable](Grabable.md) with a non-kinematic Rigidbody can be distance grabbed by default; nothing needs to be added to the object.

---

## Setup

1. Select each hand (the object that has the **Trigger Interactor**).
2. Add **Distance Grabber**.
3. Press Play, point at an object a few metres away, hold Grip and flick.

To exclude an object, or to give it its own flight time, add **Distance Grabbable** to it.

---

## Inspector Reference — Distance Grabber

### Aiming

| Field | What it does |
|---|---|
| **Aim Origin** | Where the aim starts and which way it points. Defaults to this transform. |
| **Max Distance** | Furthest an object can be and still be grabbed. Default 6 m. |
| **Min Distance** | Objects closer than this are left to the normal interactor. Default 0.5 m. |
| **Aim Radius** | Thickness of the aim. Larger is more forgiving at range. Default 0.18 m. |
| **Aim Layers** | Layers considered when aiming. Walls on these layers block the aim. |

### Priming & Flick

| Field | What it does |
|---|---|
| **Prime Button** | Button held to ready the aimed object. Default Grip. |
| **Flick Speed Threshold** | Hand speed, in m/s, that counts as a flick while primed. Default 1.2. Lower it if flicks are missed, raise it if objects launch by accident. |

### Flight

| Field | What it does |
|---|---|
| **Flight Time Per Meter** | Seconds of flight per metre of distance. Default 0.12. |
| **Flight Time Range** | Shortest (X) and longest (Y) flight, in seconds. Default 0.35 to 0.9. |
| **Catch Offset** | Where the object aims to arrive, relative to the hand. |
| **Homing Acceleration** | How strongly the object corrects toward the hand in flight, so it still arrives if the hand moves. Default 8. |
| **Catch Radius** | How close to the catch point counts as arrived. Default 0.18 m. |

### Events

| Event | Fires when |
|---|---|
| **On Primed** | An object is readied for launch |
| **On Launched** | The flick launches it |
| **On Caught** | It arrives and is grabbed |
| **On Missed** | The flight times out or the button is released before arrival |

Each event passes the interactable involved.

---

## Inspector Reference — Distance Grabbable (optional)

| Field | What it does |
|---|---|
| **Allow Distance Grab** | Untick to stop this object being distance grabbed. |
| **Flight Time Override** | Flight time in seconds for this object. 0 uses the grabber's distance-based time. |

---

## Troubleshooting

| Problem | Check |
|---|---|
| Object never highlights | It needs a Grabable and a non-kinematic Rigidbody, must be between Min and Max Distance, on one of the Aim Layers, and not hidden behind a wall. |
| Highlights but will not launch | Lower **Flick Speed Threshold**. |
| Launches but is not caught | Keep the prime button held until it arrives. Raise **Catch Radius** or **Homing Acceleration**. |
| A specific object should stay put | Add **Distance Grabbable** and untick **Allow Distance Grab**. |

---

## Related Documentation

- [Grabable](Grabable.md) — The objects that can be distance grabbed
- [Interactors](../Interactors/Interactors.md) — Trigger and Raycast interactors
- [Outline Highlight](../Systems/OutlineHighlight.md) — The highlight shown while aiming

---

**Last Updated:** November 2026

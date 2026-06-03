# Toggle Switch — Grabbable Multi-Position Switch

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Interactables > Toggle Switch
> **Use For:** Grabbable lever-style switches with discrete positions
> **Requires:** PoseConstrainer (added automatically), an Interactable Object child

---

## What It Does

The **Toggle Switch** is a grabbable switch that rotates around a single axis like a lever but
settles into **discrete step positions**. While held it moves freely; on release it snaps to the
nearest step and fires a confirmation event. It is the rotary counterpart to the
[Slider](Slider.md) — exactly as the [Drawer](Drawer.md) relates to the Slider, and the
[Wheel](ConstrainedInteractables.md#wheel) relates to the [Dial](ConstrainedInteractables.md#dial).

**Perfect for:**
- Gear / mode selectors
- Multi-position rotary switches
- Fan-speed or power-level levers
- Any control that needs to land on a fixed set of positions

**Don't use for:**
- Simple on/off flip switches touched by a finger (use [Switch](Switch.md))
- Continuous, position-free rotation (use [Lever](ConstrainedInteractables.md#lever))

---

## How It Works

The Toggle Switch inherits the shared **RotaryLeverBase** rotation engine (the same engine the
Lever uses). The base projects the hand position onto the rotation plane, converts it to an angle
within the configured range, and applies it. The Toggle Switch adds:

- **Steps** — the angle range is divided into `Number Of Steps` equal positions.
- **Snap on release** — when let go, it lerps to the nearest step's angle and confirms it.
- **Step haptics** — an optional pulse each time a step boundary is crossed while dragging.

---

## Inspector Reference

### Toggle Switch Settings

| Field | Description | Default |
|-------|-------------|---------|
| **Interactable Object** | The transform that rotates | (auto) |
| **Return To Original / Return Speed** | Snap-back speed (always returns to the snapped step) | 10 |
| **Rotation Axis** | Axis the switch rotates around | Forward |
| **Projection Distance** | Reference distance for angle calculation (sensitivity) | 0.3 |
| **Angle Range** | Min/max rotation in degrees | -40 to 40 |

### Steps

| Field | Description | Default |
|-------|-------------|---------|
| **Number Of Steps** | Discrete positions across the angle range (min 2) | 2 |
| **Starting Step** | Step the switch starts in (0-based) | 0 |

### Haptics

| Field | Description | Default |
|-------|-------------|---------|
| **Haptic On Step** | Pulse when a step boundary is crossed | true |
| **Amplitude** | Pulse strength (0-1) | 0.3 |
| **Duration** | Pulse length in seconds | 0.05 |

### Events

| Event | Fires |
|-------|-------|
| **On Step Changed** | When the current step changes (passes step index) |
| **On Step Confirmed** | When a step is committed after the snap completes |
| **On Value Changed** | Continuously as it moves (passes normalized 0-1) |

Use the edit button to drag the angle limits in the scene view. Yellow discs mark each step,
blue marks the rest direction.

---

## Scripting API

```csharp
// Read
int step = toggle.CurrentStep;
int count = toggle.NumberOfSteps;
float value = toggle.NormalizedValue;   // 0-1

// Set
toggle.SetStep(2);            // jump to a step (animates + confirms)
toggle.IncrementStep();       // next step (clamped)
toggle.DecrementStep();       // previous step (clamped)
toggle.SetNormalizedStep(0.5f);
toggle.ResetToStartingStep();

// React (UniRx)
toggle.OnStepChanged.Subscribe(s => Debug.Log($"Step {s}"));
toggle.OnStepConfirmed.Subscribe(s => Debug.Log($"Committed {s}"));
toggle.OnValueChanged.Subscribe(v => Debug.Log($"Value {v:F2}"));
```

### Driving a ScriptableVariable

Add a **Toggle Switch To Variable Driver** to write the current step into an `IntVariable`, raise
`GameEvent`s on step change / confirm, and optionally fire a per-step `GameEvent` array.

---

## Setup

1. Add the **Toggle Switch** component — a `PoseConstrainer` and an `Interactable Object` child are
   created automatically.
2. Parent your switch mesh under the **Interactable Object** so it rotates with the control.
3. Set the **Rotation Axis** and **Angle Range** to match the model.
4. Set **Number Of Steps** and **Starting Step**.
5. Configure the **PoseConstrainer** hand pose as you would for a Lever.
6. Wire **On Step Changed** / **On Step Confirmed** to your game logic.

---

## Related Documentation

- [Switch](Switch.md) — Simple finger-touched on/off switch
- [Slider](Slider.md) — Linear stepped control (the linear counterpart)
- [Lever](ConstrainedInteractables.md#lever) — Continuous rotation control
- [Feedback System](../Systems/FeedbackSystem.md) — Add haptic/audio feedback

---

**Last Updated:** June 2026
**Component Version:** 1.0.0

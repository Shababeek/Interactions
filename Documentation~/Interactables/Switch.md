# Switch — Physical Two-State Toggle

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Interactables > Switch
> **Use For:** Physical on/off switches that flip when touched
> **Requires:** Collider component (set as Trigger)

---

## What It Does

The **Switch** component is a physical on/off switch driven by trigger collisions. The side a
finger or object approaches from decides whether the switch turns **on** or **off**: cross the
threshold on one side and it turns on, the other side turns off. The switch body rotates to the
matching angle and latches there until pushed the other way.

Because state is decided by approach side, re-approaching from the same side does nothing — the
switch never flickers between states.

**Perfect for:**
- Light switches
- Power toggles
- Circuit breakers
- Any physical flip-switch mechanism

**Don't use for:**
- Push buttons (use VRButton)
- Objects that need to be grabbed (use Grabable)
- Continuous controls like dimmers (use Lever)
- Multi-position rotary switches (use Toggle Switch)

---

## Inspector Reference

### Events

| Event | Fires |
|-------|-------|
| **On Turned On** | When the switch turns on |
| **On Turned Off** | When the switch turns off |
| **On State Changed** | On every state change, passing the new state (`bool`, true = on) |

### Switch Configuration

| Field | Description | Default |
|-------|-------------|---------|
| **Switch Body** | The transform that rotates between on and off | (self) |
| **Rotation Axis** | Local axis the switch body pivots around | Z |
| **Detection Axis** | Axis used to decide which side the hand approaches from | X |
| **On Angle** | Rotation angle (°) for the on position | 20 |
| **Off Angle** | Rotation angle (°) for the off position | -20 |
| **Rotate Speed** | Animation speed (higher snaps faster) | 10 |
| **Angle Threshold** | Minimum approach angle (°) before the switch flips | 5 |
| **Start On** | State the switch starts in when the scene loads | false |

### Debug

| Field | Meaning |
|-------|---------|
| **Current State** | Read-only — true when the switch is on |

Use the edit (collider) button at the top of the inspector to drag the on/off angles directly in
the scene view.

---

## Scripting API

```csharp
// Read state
bool isOn = mySwitch.IsOn;

// Set state (animates + raises events)
mySwitch.SetState(true);   // turn on
mySwitch.SetState(false);  // turn off
mySwitch.Toggle();         // flip

// React to changes (UniRx)
mySwitch.OnStateChanged
    .Subscribe(on => Debug.Log($"Switch is now {(on ? "ON" : "OFF")}"));

// Assign the moving part
mySwitch.SwitchBody = leverTransform;
```

### Driving a ScriptableVariable

Add a **Switch To Variable Driver** alongside the Switch to mirror its state into a `BoolVariable`
and raise `GameEvent`s on turn-on / turn-off, without writing any glue code.

---

## Setup

1. Build the switch as a parent (base) with a child (the moving toggle).
2. Add the **Switch** component to the parent and assign the child to **Switch Body**.
3. Add a **Box Collider** and check **Is Trigger**; size it to cover the interaction area.
4. Set **Rotation Axis** / **Detection Axis** to match the model's orientation.
5. Wire **On Turned On** / **On Turned Off** (e.g. `Light.enabled`).

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Switch doesn't respond | Collider not a trigger | Check **Is Trigger** |
| Body doesn't move | Switch Body not assigned | Assign the moving transform |
| Flips the wrong way | Detection Axis wrong | Try a different Detection Axis |
| Too sensitive | Threshold too low | Increase Angle Threshold |

---

## Related Documentation

- [Toggle Switch](ToggleSwitch.md) — Grabbable multi-position rotary switch
- [VRButton](VRButton.md) — Press-style buttons
- [Lever](ConstrainedInteractables.md#lever) — Continuous rotation control
- [Feedback System](../Systems/FeedbackSystem.md) — Add haptic/audio feedback

---

**Last Updated:** June 2026
**Component Version:** 2.0.0

# Additional Interactables — Door, Elastic Tether, Toggle Button, Spawner, Return To Origin, Step Displays

Smaller components that build on the core interactables. Each section is self-contained.

| Component | Menu path (Component > Shababeek > Interactions > …) | Use for |
|---|---|---|
| [Door](#door-and-door-handle) | Interactables > Door | Hinged doors and cabinet doors, optionally latched |
| [Door Handle](#door-and-door-handle) | Interactables > Door Handle | A handle that unlatches and swings a Door |
| [Elastic Tether](#elastic-tether) | Interactables > Elastic Tether | An object hanging on an elastic cord |
| [VR Toggle Button](#vr-toggle-button) | Interactables > VRToggleButton | A push button that stays down until pressed again |
| [Spawning Interactable](#spawning-interactable) | Interactables > Spawning Interactable | A dispenser that hands out a new object on each grab |
| [Return To Origin](#return-to-origin) | Interactables > Return To Origin | Put an object back where it started after release |
| [Step displays](#step-displays) | Feedback > Step Value Display / Step Label Highlighter | Show the current step of a dial, slider or toggle switch |

---

## Door and Door Handle

A **Door** swings around one axis between a closed and an open limit, like a [Lever](ConstrainedInteractables.md#lever-interactable) with a hinge and a latch. A **Door Handle** is a lever on the door: turn it far enough and it releases the latch, and the same grip then pulls the door open.

### Door — Inspector

| Field | What it does |
|---|---|
| **Rotation Axis / Angle Range** | Hinge axis and the closed and open angles (shared with Lever). |
| **Hinge Offset** | Local offset from the door's origin to the hinge, so the door pivots at its edge. |
| **Start Locked** | The door starts latched and will not swing until unlocked. Default on. |
| **On Opened / On Closed** | Fired when the door reaches either limit. |
| **On Moved** | Fired while swinging, with 0 (closed) to 1 (open). |
| **On Unlocked / On Locked** | Fired when the latch releases or engages. |

The Inspector has **Simulate Closed** and **Simulate Open** buttons to preview the limits without entering Play mode.

### Door Handle — Inspector

| Field | What it does |
|---|---|
| **Door** | The door this handle controls. Required. |
| **Unlock Threshold** | How far the handle must turn (0 to 1) to release the latch. Default 0.6. |
| **Relock When Released** | Re-latch when the handle is let go, if the door is closed at that moment. |
| **On Handle Turned** | Fired while turning, with 0 to 1. |
| **On Unlocked** | Fired when the turn passes the threshold. |

### Setup

1. Add **Door** to the door object and set the axis, angle range and hinge offset. Use the simulate buttons to check the swing.
2. For a latched door, add a child handle object with **Door Handle** and assign the door.
3. For a door with no handle (a cupboard), untick **Start Locked** and let the player grab the door itself.

```csharp
door.Unlock();          // release the latch from a keypad, key or sequence
door.Lock();            // only takes effect while closed
bool open = door.IsOpen;
```

---

## Elastic Tether

An object hanging from a fixed point on an elastic cord: a pull-cord, a hanging toy, a light pull. Pulling stretches the cord against rising resistance; letting go springs it back and it swings. Pull too far and it slips out of the hand.

### Inspector

| Group | Fields | What they control |
|---|---|---|
| **Cord** | Cord Visual, Anchor Local Offset, Thin Cord When Stretched | The cord mesh (stretched and aimed automatically) and where it hangs from. Leave Cord Visual empty for an invisible tether. |
| **Spring** | Frequency, Damping Ratio | How fast it springs back (oscillations per second) and how much it overshoots (1 = none, 0.3 = several bounces). |
| **Limits** | Soft Limit, Slip Limit Multiplier, Slip Grace Time, Grab Grace Time, Resistance Curve, Clamp To Anchor Height | How far it stretches, when the grab breaks, and the feel of the resistance. |
| **Rotation** | Rotation Follow, Rotation Speed | How much and how quickly the object tilts along the cord. |
| **Bump** | Enable Bump, Bump Radius, Bump Transfer, Bump Min Speed | Lets a moving hand knock the object without grabbing it. |
| **Neighbour** | Neighbour, Neighbour Radius, Neighbour Restitution | A second tether on the same anchor that this one can collide with. Set it on one of the pair. |
| **Haptics** | Tension Haptics, Haptic Start Stretch, Max Haptic Amplitude, Slip Haptic | Rumble that grows with stretch, and an optional [pattern](../Systems/Haptics.md) on slip. |
| **Sleep** | Sleep Speed, Sleep Distance | When the object counts as at rest and stops updating. |
| **Events** | On Stretch Changed (0 to 1), On Slip Release, On Settled | Hooks for sound, light switches, counters. |
| **Diagnostics** | Log Level, Verbose Frame Interval, Stuck Awake Seconds | Leave Log Level on *Problems*. *Events* and *Verbose* are for debugging. |

**Tuning tips:** start with **Frequency** and **Damping Ratio** for the bounce, then **Soft Limit** for how far it can be pulled. If the object flies out of the hand while closing the grip, raise **Grab Grace Time**.

---

## VR Toggle Button

A physical push button that latches: press once and it stays down, press again and it comes back up. It is pushed by a finger collider, like [VRButton](VRButton.md), not grabbed.

| Field | What it does |
|---|---|
| **Button** | The transform that moves when pressed. |
| **Normal Position / Pressed Position** | Local positions for up and down. |
| **Press Speed** | Speed of the press animation. |
| **Cool Down Time** | Minimum seconds between toggles. Default 0.4. |
| **Mask Name** | Only colliders whose name contains this text can press the button (default `tip`, the fingertip colliders). Empty accepts anything. |
| **Audio Source, Toggle On / Off Sound, Sound Volume** | Optional click sounds. |
| **Use Haptics, Haptic Pattern** (or Amplitude / Duration) | Optional vibration on toggle. A [pattern](../Systems/Haptics.md) overrides the amplitude and duration values. |
| **Active** | When off the button ignores presses. |
| **On Toggled / On Toggled On / On Toggled Off** | Events. |

```csharp
toggle.SetState(true);                 // sets and raises events
toggle.SetStateWithoutNotify(false);   // sets silently, e.g. when loading a save
bool on = toggle.IsToggledOn;
```

---

## Spawning Interactable

A dispenser. Grabbing it creates a new copy of a Grabable prefab and puts that copy in the hand, leaving the dispenser in place. Use it for ammo boxes, tool bins and item pedestals.

| Field | What it does |
|---|---|
| **Prefab** | The Grabable prefab to create on each grab. |

---

## Return To Origin

Add next to any interactable. After the object is released it waits, then moves back to where it started. Grabbing it again before the delay ends cancels the return.

| Field | What it does |
|---|---|
| **Return Delay** | Seconds after release before it starts returning. Default 1. |
| **Return Duration** | Seconds the return takes. 0 snaps instantly. |
| **Restore Parent** | Also restore the original parent. |
| **On Returned** | Fired when the object is back. |

The starting pose is recorded on Start. Call `CaptureOrigin()` to set a new home, `ScheduleReturn()` or `CancelReturn()` to control it from script.

> **Note:** on a [Socketable](../SocketSystem/SocketSystem.md) object, the socket wins: an object that is in a socket, or on its way back to one, is not returned to its origin.

---

## Step Displays

Two small components that show which step a stepped control is on. They work with any stepped interactable: [Dial](ConstrainedInteractables.md#dial-interactable), [Slider](ConstrainedInteractables.md#slider-interactable) and [Toggle Switch](ToggleSwitch.md). Both preview in edit mode.

### Step Value Display — one readout

Shows the current step in a single TextMeshPro text, such as a channel number above a knob.

| Field | What it does |
|---|---|
| **Source** | The stepped interactable to follow. Found in parents if left empty. |
| **Text** | The text to write to. |
| **Update While Moving** | Update during the grab, not only when the step is confirmed. |
| **First Number** | Number shown for the first step (1 gives 1 to N, 0 gives 0 to N-1). |
| **Format** | Text format; `{0}` is the number. Examples: `CH {0}`, `{0:00}`. |
| **Step Texts** | Optional text per step (Low / Mid / High) that replaces the number. |

### Step Label Highlighter — one label per step

Highlights the label of the current step and dims the rest, such as the markings around a selector dial.

| Field | What it does |
|---|---|
| **Source** | The stepped interactable to follow. |
| **Labels** | One TextMeshPro label per step, in step order. |
| **Active Color / Material / Scale** | Look of the current step's label. |
| **Inactive Color** | Look of the other labels. |

The Inspector's **Sync Labels To Steps** tool creates and lays out the labels from the control's step settings.

---

## Related Documentation

- [Constrained Interactables](ConstrainedInteractables.md) — Lever, Drawer, Slider, Joystick, Wheel, Dial
- [Toggle Switch](ToggleSwitch.md) — Grabbable multi-position switch
- [VRButton](VRButton.md) — Momentary push button
- [Haptic Patterns](../Systems/Haptics.md)

---

**Last Updated:** November 2026

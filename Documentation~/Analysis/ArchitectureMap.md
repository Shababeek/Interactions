# Architecture Map

How the Interaction System is wired from the Unity Editor, how the pieces talk at runtime, and where to extend it.

---

## What you set up first

1. **Create a Config asset** — `Assets` context menu > **Create > Shababeek > Interactions > Config** (see [Config](../Core/config.md)).
2. **Initialize the scene** — Hierarchy context menu > **Shababeek > Initialize CameraRig** (or top menu **Shababeek > Initialize Scene**) to add the **Camera Rig**, hands and interactors (see [Quick Start](../GettingStarted/QuickStart.md)).
3. **Assign Config on Camera Rig** if it is not linked automatically.

<!-- IMAGE TODO: Images/General/architecture_config_and_camera_rig.png | Suggested screenshot: hierarchy with Camera Rig and Config assignment -->

---

## Runtime flow

```
Config
  └─ input provider per hand (controller / hand tracking / playback)
Camera Rig
  └─ Hand (left, right): pose controller + finger values + buttons
       └─ Interactor (Trigger or Raycast, optional Distance Grabber)
            └─ Interactable: None → Hovering → Selected, plus Use and button events
                 ├─ PoseConstrainer: hand pose and grab points
                 ├─ Feedback: FeedbackSystem, outline, haptic patterns
                 ├─ Socketable → Socket
                 └─ Writers / Drivers → ReactiveVars variables and events → binders, sequences
```

For Inspector detail on rigs, hands and input, see the [Interaction Core manual](../Core/InteractionCoreUserManual.md) and [Input providers and locomotion](../Core/InputAndLocomotion.md).

---

## Class families

### Interactables

```
InteractableBase
├─ Grabable
│   ├─ PistolInteractable
│   └─ InventoryBackpackGrabable
├─ SpawningInteractable
└─ ConstrainedInteractableBase            (fake hand on the control, return behaviour, second-hand support)
    ├─ RotaryLeverBase                    (single-axis lever engine)
    │   ├─ LeverInteractable
    │   ├─ ToggleSwitchInteractable
    │   ├─ DoorInteractable
    │   └─ DoorHandleInteractable
    ├─ RotaryInteractableBase             (wheel-style rotation, hand position or wrist twist)
    │   ├─ WheelInteractable
    │   └─ DialInteractable
    ├─ LinearInteractableBase             (start-to-end slide)
    │   ├─ DrawerInteractable
    │   └─ SliderInteractable
    ├─ JoystickInteractable
    ├─ TwoHandedGrabable
    └─ ElasticTetherInteractable
```

`Switch`, `VRButton` and `VRToggleButton` are deliberately outside this tree. They are pressed by finger colliders and have no hover or select.

Stepped controls (Dial, Slider, Toggle Switch) also implement `IStepInteractable`, which the step displays read.

### Interactors and input

- `InteractorBase` → `TriggerInteractor`, `RaycastInteractor`. `DistanceGrabber` is a companion component on the same hand.
- `IHandInputProvider` → `ControllerInputProvider`, `HandTrackingInputProvider`, `PlaybackHandInputProvider`. `Config` creates or holds one per hand.

### Sockets

`AbstractSocket` → `Socket`, `MultiSocket`, `GridMultiSocket`, `TransformArrayMultiSocket`, `ShiftingRackSocket`, `InventoryGridSocket`. A `Socketable` sits beside a Grabable. Matching uses `SocketMask` categories. See [Advanced Sockets](../SocketSystem/AdvancedSockets.md).

### Add-on behaviours

Components that sit beside an interactable and listen to its events, rather than subclassing it: `GunFiring`, `BladeHit`, `ReturnToOrigin`, `InteractableOutlineFeedback`, `FeedbackSystem`, the variable writers and drivers.

### Assemblies

| Assembly | Contents |
|---|---|
| `Shababeek.Interactions` | Runtime |
| `Shababeek.Interactions.Editor` | Custom inspectors, setup wizard, menus |
| `Shababeek.Sequencing.Interactions` | Interaction-specific sequence actions |
| `Shababeek.Interactions.Runtime.Tests`, `Shababeek.Interactions.Editor.Tests` | Test suites under `Tests/` |

---

## Extension points

| Goal | Where to start |
|------|----------------|
| New free-form interactable | Subclass **InteractableBase** ([how](../InteractionsUserManual.md#writing-your-own-interactable)) |
| New lever-like, wheel-like or sliding control | Subclass **RotaryLeverBase**, **RotaryInteractableBase** or **LinearInteractableBase** |
| Behaviour on an existing interactable | A companion component that subscribes to its events |
| New way of detecting objects | Subclass **InteractorBase** |
| New input source | Implement **IHandInputProvider** ([Config](../Core/config.md#custom-input-provider)) |
| Custom socket rules | Subclass **AbstractSocket** |
| Something that reacts to bullets | Implement **IBulletHittable** ([Weapons](../Interactables/Weapons.md)) |
| Data and UI binding | **Writers / Drivers** plus ReactiveVars binders ([Drivers](../ScriptableSystem/Drivers.md)) |

---

## Further reading

- [Two-handed grabbing design](../TwoHandedGrabbing-Design.md)
- [Procedural grab posing design](../ProceduralGrabPosing-Design.md)
- [Improvement plan, November 2026](ImprovementPlan-2026-11-17.md)

---

**Last Updated:** November 2026

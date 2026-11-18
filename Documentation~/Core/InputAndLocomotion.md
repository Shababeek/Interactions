# Input Providers and Locomotion

How hand input reaches the system, and how to move the player.

---

## Input Providers

Each hand gets its input from an **input provider**: a component that supplies the hand's pose, finger curl values and button presses. The Config asset decides which one is used.

| Provider | Source | Menu path (Component > Shababeek > Interactions > Input Providers > …) |
|---|---|---|
| **Controller Input Provider** | Input System actions assigned on the Config asset | Controller Input Provider |
| **Hand Tracking Input Provider** | The XR Hands subsystem; finger curls are computed from joint rotations | Hand Tracking Input Provider |
| **Auto Switch Input Provider** | Hand tracking while the hand is tracked, the controller otherwise | Auto Switch Input Provider (created automatically for Input Type **Both**) |
| **Playback Input Provider** | An [Interaction Recording](../Systems/InteractionRecording.md) | Playback Input Provider (installed automatically during playback) |

### Choosing the input type

On the [Config asset](config.md), **Input Type** selects **Controller Tracking**, **Hand Tracking** or **Both**. Hand Tracking and Both appear only when Unity's XR Hands package is installed. A provider is created for each hand at runtime; you do not need to add one by hand.

With **Both**, each hand switches source on its own: all five fingers follow the tracked hand's joints while it is tracked, and the controller's buttons take over when it is not. A button held on one source is released when the source changes.

To supply your own provider for a hand, assign it to the left or right **input provider override** on the Config asset.

### Controller Input Provider

| Field | What it does |
|---|---|
| **Use Device Detection** | Treat the provider as active only while a controller is connected. |

The finger, A / B button and thumbstick actions come from the hand action references on the Config asset. On older Config assets the A, B and thumbstick references may be empty; those inputs then simply never fire.

### Hand Tracking Input Provider

| Field | What it does |
|---|---|
| **Max Flex Angle** | Joint angle, in degrees, that counts as fully curled. Default 90. |
| **Min Flex Angle** | Joint angle that counts as fully extended. Default 0. |
| **Finger Rest Angle** | Bend, in degrees, at which the four fingers count as fully open. Default 15. Raise it if a relaxed hand grabs things; lower it if light pinches are missed. |
| **Debug Log** | Logs finger values. Leave off. |

### Hand position with hand tracking

With controllers, each hand pivot on the Camera Rig is moved by a Tracked Pose Driver. While hand tracking is the input source for a hand, the **Hand Tracking Pivot Driver** takes over: it switches that pivot's Tracked Pose Driver off, moves the pivot from the tracked hand, and switches the Tracked Pose Driver back on when the controller returns. The Camera Rig adds it automatically when Input Type is Hand Tracking or Both.

| Field | What it does |
|---|---|
| **Left / Right Position Offset** | Extra position offset for that hand, in the tracked hand's own space (metres). |
| **Left / Right Rotation Offset** | Extra rotation offset for that hand (degrees). |

The driver uses the grip pose reported by the XR runtime when there is one, so tracked hands line up the way controller-driven hands do. If the runtime does not report one it falls back to the palm joint, and the offsets above are there to correct the alignment. To set them, add the component to the Camera Rig yourself, enter Play mode with hand tracking, and adjust until the hand model sits on your real hand.

---

## Camera Rig Locomotion

> **Menu Path:** Component > Shababeek > Interactions > Camera Rig Locomotion
> **Requires:** Character Controller (added automatically)

Adds smooth movement and snap turning to the Camera Rig. Movement is relative to where the player is looking, flattened to the ground.

| Field | What it does |
|---|---|
| **Head** | The tracked camera. Found from the Camera Rig, or the main camera, if left empty. |
| **Move Speed** | Walking speed in metres per second. Default 2.5. |
| **Gravity** | Downward acceleration. Keep it negative. |
| **Move Deadzone** | Stick movement below this is ignored, to stop drift. |
| **Snap Turn Angle** | Degrees turned per snap. Default 45. |
| **Snap Turn Threshold** | How far the stick must be pushed sideways to turn. It must return below this before the next snap. |
| **Move Action / Turn Action** | The Input System actions for the move stick and the turn stick. |

### Setup

1. Add **Camera Rig Locomotion** to the Camera Rig.
2. Assign a thumbstick action to **Move Action** (usually the left stick) and to **Turn Action** (usually the right stick).
3. Size the Character Controller capsule to the player.

---

## Related Documentation

- [Config asset](config.md) — Input actions, layers, hand data
- [Interaction Core manual](InteractionCoreUserManual.md) — Camera Rig, Hand, physics hands
- [Interactors](../Interactors/Interactors.md) — How hands detect and select objects

---

**Last Updated:** November 2026

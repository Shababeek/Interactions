# Shababeek Interaction System — Documentation

**Unity-first:** most workflows begin in the **Inspector**, the **Hierarchy / Create** menus and **Project** assets. Code is only needed for custom behaviour.

This is the index for the Shababeek Interaction System (VR hands, grabbing, constrained controls, sockets, feedback).

> **Note:** this folder is named `Documentation~`. Unity ignores folders that end in `~`, so these files are not imported as assets. Open them in your IDE, a Markdown viewer or on GitHub.

---

## Getting Started

| Document | Description |
|----------|-------------|
| **[Quick Start Guide](GettingStarted/QuickStart.md)** | From install to your first grab, pose and lever |
| **[Package overview](UserManual.md)** | Install steps and core concepts on one page |
| **[Config asset](Core/config.md)** | The central settings asset: hands, layers, input |
| **[Sequence system quick start](GettingStarted/SequenceSystemQuickStart.md)** | Build a first tutorial sequence |
| **[Scriptable system for designers](GettingStarted/ScriptableSystemForDesigners.md)** | Variables and events without code |

---

## Interactables

Components you add to objects to make them interactive.

### Grab and hold

| Component | Description | Use for |
|-----------|-------------|---------|
| **[Grabable](Interactables/Grabable.md)** | Pick up, hold and throw | Tools, props, balls |
| **[Two Handed Grabable](Interactables/TwoHandedGrabable.md)** | One or two hands, aims along both grips | Rifles, poles, heavy tools |
| **[Distance Grab](Interactables/DistanceGrab.md)** | Flick far objects into the hand | Anything out of reach |
| **[Weapons](Interactables/Weapons.md)** | Gun firing, bullets, blade hits, targets | Shooting ranges, combat |

### Touch

| Component | Description | Use for |
|-----------|-------------|---------|
| **[VRButton](Interactables/VRButton.md)** | Push button | Control panels, keypads |
| **[VR Toggle Button](Interactables/AdditionalInteractables.md#vr-toggle-button)** | Push button that latches | Power buttons, mode buttons |
| **[Switch](Interactables/Switch.md)** | Finger-flipped on/off switch | Light switches |

### Constrained (grab and move along a path)

| Component | Description | Use for |
|-----------|-------------|---------|
| **[Lever](Interactables/ConstrainedInteractables.md#lever-interactable)** | Rotates around one axis | Throttles, gear shifts |
| **[Toggle Switch](Interactables/ToggleSwitch.md)** | Lever that snaps to steps | Mode selectors |
| **[Drawer](Interactables/ConstrainedInteractables.md#drawer-interactable)** | Slides along a line | Drawers, sliding doors |
| **[Slider](Interactables/ConstrainedInteractables.md#slider-interactable)** | Slides and snaps to steps | Faders, stepped selectors |
| **[Joystick](Interactables/ConstrainedInteractables.md#joystick-interactable)** | Rotates around two axes | Flight sticks, arcade sticks |
| **[Wheel](Interactables/ConstrainedInteractables.md#wheel-interactable)** | Continuous rotation, one or two hands | Valves, steering wheels |
| **[Dial](Interactables/ConstrainedInteractables.md#dial-interactable)** | Rotation that snaps to steps | Combination locks, selectors |
| **[Door and Door Handle](Interactables/AdditionalInteractables.md#door-and-door-handle)** | Hinged door with an optional latch | Doors, cabinets |
| **[Elastic Tether](Interactables/AdditionalInteractables.md#elastic-tether)** | Object on an elastic cord | Pull cords, hanging toys |

### Helpers

| Component | Description |
|-----------|-------------|
| **[Spawning Interactable](Interactables/AdditionalInteractables.md#spawning-interactable)** | Hands out a new object on each grab |
| **[Return To Origin](Interactables/AdditionalInteractables.md#return-to-origin)** | Puts an object back after release |
| **[Step displays](Interactables/AdditionalInteractables.md#step-displays)** | Show the current step of a dial, slider or toggle switch |

---

## Hands, Interactors and Input

| Document | Description |
|----------|-------------|
| **[Interactors](Interactors/Interactors.md)** | Hand component, Trigger Interactor, Raycast Interactor |
| **[Interaction Core](Core/InteractionCoreUserManual.md)** | Camera Rig, Hand, physics hands |
| **[Input providers and locomotion](Core/InputAndLocomotion.md)** | Controller and hand-tracking input, smooth move and snap turn |
| **[PoseConstrainer](PoseSystem/PoseConstrainer.md)** | Hand poses and grab points on an object |
| **[Animations and poses](PoseSystem/AnimationsUserManual.md)** | Hand Data, poses and the pose controller |
| **[Pose migration guide](PoseSystem/MuscleBasedMigration.md)** | Moving to muscle-based poses |

---

## Systems

| System | Description | Document |
|--------|-------------|----------|
| **Feedback System** | Haptic, audio, material, scale, particle and event feedback | **[Feedback System](Systems/FeedbackSystem.md)** |
| **Haptic Patterns** | Reusable vibration assets | **[Haptics](Systems/Haptics.md)** |
| **Outline Highlight** | Hover, selection and grab-hint outlines | **[Outline Highlight](Systems/OutlineHighlight.md)** |
| **Socket System** | Snapping objects into holders | **[Sockets](SocketSystem/SocketSystem.md)** |
| **Advanced Sockets** | Categories, multi-slot sockets, grid inventory, fasteners | **[Advanced Sockets](SocketSystem/AdvancedSockets.md)** |
| **Interaction Recording** | Record a session and replay it without a headset | **[Interaction Recording](Systems/InteractionRecording.md)** |

---

## Integration with ReactiveVars

The Interaction System uses the **com.shababeek.reactivevars** package for variables, events and sequencing. The pages below cover the interaction-specific parts.

| Document | Description |
|----------|-------------|
| **[Interaction Drivers](ScriptableSystem/Drivers.md)** | Write interactable and socket state to variables and events |
| **[Interaction Sequences](Systems/SequencingSystem.md)** | Tutorial steps that wait for grabs, levers, sockets |
| **[Sequencing core](Systems/SequencingCoreUserManual.md)** | Sequence assets and their editor |
| **[Scriptable Variables](ScriptableSystem/ScriptableVariables.md)** | Variable asset types |
| **[Variable Container](ScriptableSystem/VariableContainer.md)** | Grouping variables in one asset |
| **[Scriptable System Window](ScriptableSystem/ScriptableSystemWindow.md)** | Inspecting variables at runtime |
| **[Reactive utilities](Reference/CoreUtilitiesUserManual.md)** | Tweens and shared utilities |

For binders, the full variable reference and the tween system, see the [ReactiveVars documentation](https://github.com/Shababeek/ReactiveVars/blob/main/Documentation~/README.md).

---

## For Programmers and Maintainers

| Document | Description |
|----------|-------------|
| **[Interactions overview](InteractionsUserManual.md)** | States, events and how to write your own interactable |
| **[Architecture map](Analysis/ArchitectureMap.md)** | Class families, runtime flow, extension points |
| **[Two-handed grabbing design](TwoHandedGrabbing-Design.md)** | Design notes |
| **[Procedural grab posing design](ProceduralGrabPosing-Design.md)** | Design notes for finger auto-fit |
| **[Improvement plan, November 2026](Analysis/ImprovementPlan-2026-11-17.md)** | Current review and prioritised improvements |
| **[Code review, September 2026](Analysis/CodeReview-2026-09-06.md)** | Pre-publish review |
| **[Code review, June 2026](Analysis/CodeReview-2026-06-11.md)** | Full-system review |
| **[Documentation status](InteractionsDocumentationSummary.md)** | What is documented where, and what is missing |
| **[Images to do](ImagesTodo.md)** | Screenshots and GIFs still to capture |

---

## Quick Reference

### Hierarchy menus

Right-click in the Hierarchy > **Shababeek**:

- **Initialize CameraRig** — add the VR rig to the scene
- **Create** > Button, Drawer, Joystick, Lever, Switch, Wheel — a ready-made interactable with a placeholder model
- **Make Into** > Grabbable, Drawer, Joystick, Lever, Wheel — convert the selected object

Top menu > **Shababeek**: **Setup Wizard**, **Initialize Scene**, **Finger Collider Setup**.

Other components are added with **Add Component > Shababeek > Interactions**.

### Essential layers

| Layer | Purpose |
|-------|---------|
| LeftHand | Left hand collisions |
| RightHand | Right hand collisions |
| Interactable | Objects that can be interacted with |
| Player | Player body (no hand collision) |

### Default buttons

| Button | Default use |
|--------|-------------|
| **Grip** | Grab and release |
| **Trigger** | Use the held object |
| **A / B** | Extra events on a held object (On A Pressed, On B Pressed) |

---

## How the Manuals Are Organised

Component manuals follow the same order: what it does, setup, Inspector reference, common workflows, troubleshooting, then a short scripting section.

### Images

Manuals reference images under `Images/` with paths relative to the page (`../Images/...` from a subfolder). Images that have not been captured yet are listed in **[ImagesTodo.md](ImagesTodo.md)** and marked in the page with an HTML comment, so nothing renders as a broken image.

### Checking links

From the package root:

```bash
node Documentation~/validate-links.mjs
```

This checks every relative link and heading anchor in `Readme.md` and `Documentation~/`. It needs Node.js only.

---

## Getting Help

- **Bug reports:** [GitHub Issues](https://github.com/Shababeek/Interactions/issues)
- **Feature requests:** [GitHub Discussions](https://github.com/Shababeek/Interactions/discussions)
- **Email:** Ahmadabobakr@gmail.com
- **Website:** [ahmadabobakr.github.io](https://ahmadabobakr.github.io)

---

**Last Updated:** November 2026

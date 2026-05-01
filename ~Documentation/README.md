# Shababeek Interaction System — Documentation

**Unity-first:** Most workflows begin in the **Inspector**, **Hierarchy / Create** menus, and **Project** assets—then use code only when you need custom behavior.

Welcome to the documentation for the Shababeek Interaction System (VR / XR interactions, hands, grab, constrained controls, sockets, feedback).

> **Note:** This folder is named `~Documentation` so it stays easy to find in source control while Unity treats it like other package assets. Open these `.md` files in your IDE or on GitHub; Unity does not render them in the Inspector.

---

## 📚 Documentation Structure

### 🎓 Getting Started
Start here if you're new to the system.

| Document | Description |
|----------|-------------|
| **[Quick Start Guide](GettingStarted/QuickStart.md)** | 10-minute guide from install to first interaction |
| **[Config Asset](Core/config.md)** | Central configuration setup |

---

### 📎 Additional manuals (consolidated here)

| Document | Description |
|----------|-------------|
| **[Package overview](UserManual.md)** | High-level install & concepts |
| **[Interactions (Inspector)](InteractionsUserManual.md)** | Interactables, interactors, grab strategies |
| **[Interaction Core](Core/InteractionCoreUserManual.md)** | Config, Camera Rig, Hand, input |
| **[Animations & poses](PoseSystem/AnimationsUserManual.md)** | Pose / animation-oriented usage |
| **[Reactive utilities](Reference/CoreUtilitiesUserManual.md)** | Tweens & shared utilities used by interactables |
| **[Sequencing core](Systems/SequencingCoreUserManual.md)** | Sequence assets & editor (ReactiveVars companion) |
| **[Interactions summary](InteractionsDocumentationSummary.md)** | Short overview / index |

---

### 🎮 Interactables
Components you add to objects to make them interactive.

#### Basic Interactables
| Component | Description | Use Case |
|-----------|-------------|----------|
| **[Grabable](Interactables/Grabable.md)** | Pick up and hold objects (with built-in throwing) | Tools, props, weapons, balls, grenades |
| **[Switch](Interactables/Switch.md)** | Toggle switches | Light switches, levers |
| **[VRButton](Interactables/VRButton.md)** | Pressable buttons | Control panels, keypads |

#### Constrained Interactables
Objects that move with physical constraints.

| Component | Description | Use Case |
|-----------|-------------|----------|
| **[Lever](Interactables/ConstrainedInteractables.md#lever)** | Rotates around a single axis | Gear shifts, throttles |
| **[Drawer](Interactables/ConstrainedInteractables.md#drawer)** | Slides along a linear path | Desk drawers, sliding doors |
| **[Joystick](Interactables/ConstrainedInteractables.md#joystick)** | Two-axis rotation control | Flight controls, arcade sticks |
| **[Wheel](Interactables/ConstrainedInteractables.md#wheel)** | Continuous rotation | Valves, steering wheels |
| **[Slider](Interactables/ConstrainedInteractables.md#slider-interactable)** | Linear multi-step control | Volume strips, detented sliders |

---

### ✋ Interactors
Components that detect and perform interactions (typically on hands).

| Component | Description | Use Case |
|-----------|-------------|----------|
| **[Hand](Interactors/Interactors.md#hand-component)** | Left/right hand, input, poses | Spawned with Camera Rig |
| **[TriggerInteractor](Interactors/Interactors.md#trigger-interactor)** | Proximity-based detection | Close-range grabbing |
| **[RaycastInteractor](Interactors/Interactors.md#raycast-interactor)** | Ray-based selection | Distant object selection |

---

### 🎨 Systems
Higher-level systems that add functionality.

| System | Description | Document |
|--------|-------------|----------|
| **Pose Constrainer** | Controls hand poses during interactions | **[PoseConstrainer](PoseSystem/PoseConstrainer.md)** |
| **Feedback System** | Haptic, audio, visual, scale, particle feedback | **[Feedback](Systems/FeedbackSystem.md)** |
| **Socket System** | Object placement and snapping | **[Sockets](SocketSystem/SocketSystem.md)** |

### 🔗 Integration with ReactiveVars
Connect interactions to the external data flow system.

The Shababeek Interaction System integrates with the **com.shababeek.reactivevars** package to enable reactive programming and sequencing. This document covers the interaction-specific components only.

| Document | Description |
|----------|-------------|
| **[Interaction Drivers](ScriptableSystem/Drivers.md)** | Connect interactables and sockets to reactive variables |
| **[Interaction Sequences](Systems/SequencingSystem.md)** | Tutorials using interaction-specific sequence actions |
| **[Architecture Map](Analysis/ArchitectureMap.md)** | Runtime flow and extension points |
| **[Pose Migration Guide](PoseSystem/MuscleBasedMigration.md)** | Legacy to muscle-based pose migration |

For complete documentation on the ReactiveVars system (Scriptable Variables, Game Events, Sequencing System core, and generic binders), visit the **com.shababeek.reactivevars** package documentation.

---

## 🔍 Quick Reference

### Component Quick-Add
Right-click in hierarchy → **Shababeek** → Choose option:
- **Initialize Scene** — Sets up VR camera rig
- **Make Into** → **Grabable** — Convert object to grabbable
- **Setup Wizard** — Guided configuration

### Essential Layers
| Layer | Purpose |
|-------|---------|
| LeftHand | Left hand collisions |
| RightHand | Right hand collisions |
| Interactable | Objects that can be interacted with |
| Player | Player body (no hand collision) |

### Input Buttons
| Button | Default Use |
|--------|-------------|
| **Grip** | Grab/release objects |
| **Trigger** | Use/activate held objects |
| **Primary (A/X)** | UI interaction |

---

## 📖 How to Use This Documentation

Each component manual follows a consistent structure:

1. **What It Does** — Quick overview
2. **Quick Example** — Get started fast
3. **Inspector Reference** — All settings explained
4. **Common Workflows** — Step-by-step guides
5. **Troubleshooting** — Common issues and solutions
6. **Scripting API** — Code examples (brief)

### Missing Images
Throughout the documentation, missing images are marked with HTML comments like:

```
<!-- TODO: Add filename.gif -->
*Description of needed image*
```

See [ImagesTodo.md](ImagesTodo.md) for a complete tracking list of all needed images and their current status.

---

## ✅ Documentation Link Check

Run this from the package root to validate relative markdown links:

```bash
npm run docs:check
```

This executes `~Documentation/validate-links.mjs` and reports broken links.

---

## 🆘 Getting Help

- **Documentation Issues**: Check the troubleshooting section of each component
- **Bug Reports**: [GitHub Issues](https://github.com/Shababeek/Interactions/issues)
- **Feature Requests**: [GitHub Discussions](https://github.com/Shababeek/Interactions/discussions)
- **Email**: Ahmadabobakr@gmail.com
- **Website**: [ahmadabobakr.github.io](https://ahmadabobakr.github.io)

---

## 📋 Documentation Checklist

| Section | Status |
|---------|--------|
| Getting Started | ✅ Complete |
| Config | ✅ Complete |
| Grabable (incl. throwing) | ✅ Complete |
| Switch | ✅ Complete |
| VRButton | ✅ Complete |
| Constrained Interactables | ✅ Complete |
| PoseConstrainer | ✅ Complete |
| Feedback System | ✅ Complete (v1.1 - added Scale, Particle, Toggle, UnityEvent) |
| Socket System | ✅ Complete (v1.1 - added ReactiveVars integration guidance) |
| Interaction Drivers | ✅ Complete (v1.5 - interaction-specific only) |
| Interaction Sequences | ✅ Complete (v1.5 - interaction-specific actions only) |
| Architecture Map | ✅ Complete |
| Pose Migration Guide | ✅ Complete |
| Interactors | ✅ Complete |

---

**Last Updated:** March 2026

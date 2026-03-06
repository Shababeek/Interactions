# Shababeek Interaction System — Documentation

Welcome to the complete documentation for the Shababeek Interaction System. This guide covers everything from basic setup to advanced customization.

---

## 📚 Documentation Structure

### 🎓 Getting Started
Start here if you're new to the system.

| Document | Description |
|----------|-------------|
| **[Quick Start Guide](GettingStarted/QuickStart.md)** | 10-minute guide from install to first interaction |
| **[Config Asset](Core/config.md)** | Central configuration setup |

---

### 🎮 Interactables
Components you add to objects to make them interactive.

#### Basic Interactables
| Component | Description | Use Case |
|-----------|-------------|----------|
| **[Grabable](Interactables/Grabable.md)** | Pick up and hold objects | Tools, props, weapons |
| **[Throwable](Interactables/Throwable.md)** | Physics-based throwing | Balls, grenades, toys |
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

---

### ✋ Interactors
Components that detect and perform interactions (typically on hands).

| Component | Description | Use Case |
|-----------|-------------|----------|
| **Hand** *(coming soon)* | Full VR hand with poses | Main player hands |
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
| **[Interaction Binders](ScriptableSystem/Binders.md)** | Connect interactables and sockets to reactive variables |
| **[Interaction Sequences](Systems/SequencingSystem.md)** | Tutorials using interaction-specific sequence actions |

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
| Grabable | ✅ Complete |
| Throwable | ✅ Complete |
| Switch | ✅ Complete |
| VRButton | ✅ Complete |
| Constrained Interactables | ✅ Complete |
| PoseConstrainer | ✅ Complete |
| Feedback System | ✅ Complete (v1.1 - added Scale, Particle, Toggle, UnityEvent) |
| Socket System | ✅ Complete (v1.1 - added Socket Binders) |
| Interaction Binders | ✅ Complete (v1.5 - interaction-specific only) |
| Interaction Sequences | ✅ Complete (v1.5 - interaction-specific actions only) |
| Interactors | ✅ Complete |

---

**Last Updated:** March 2026

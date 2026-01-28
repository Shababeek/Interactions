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
| **[Hand](Interactors/Hand.md)** | Full VR hand with poses | Main player hands |
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
| **Sequencing System** | Tutorials and guided workflows | **[Sequencing](Systems/SequencingSystem.md)** |

### 🔗 Scriptable System
Decoupled variable-based architecture for data flow.

| Document | Description |
|----------|-------------|
| **[Scriptable Variables](ScriptableSystem/ScriptableVariables.md)** | Variables, events, and references |
| **[Variable Container](ScriptableSystem/VariableContainer.md)** | Group variables as sub-assets |
| **[Binders](ScriptableSystem/Binders.md)** | Connect variables to components |
| **[Scriptable System Window](ScriptableSystem/ScriptableSystemWindow.md)** | Editor tool for debugging and monitoring |
| **[Designer Guide](GettingStarted/ScriptableSystemForDesigners.md)** | No-code workflows |

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

### Screenshot Placeholders
Throughout the documentation, you'll see placeholders like:

```
![Documentation Example](Images/documentation-example.png)
```

These indicate where screenshots or GIFs should be added to improve clarity.

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
| Sequencing System | ✅ Complete |
| Interactors | ✅ Complete |
| Scriptable Variables | ✅ Complete |
| Variable Container | ✅ Complete |
| Binders | ✅ Complete (v1.3 - added Speed, Scale, Material, Socket binders) |
| Scriptable System Window | ✅ Complete |

---

**Last Updated:** January 2026

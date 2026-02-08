# Shababeek Interaction System

A comprehensive Unity package for building advanced VR/AR and 3D interactions with a focus on hand presence, pose constraints, and reactive programming.

[![Unity Version](https://img.shields.io/badge/Unity-6.0%2B-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

---
## **Why Choose Shababeek Interactions?**

### **Hand-First Design**
- Built around natural hand presence and interactions
- Advanced pose constraint system with smooth transitions
- Support for different kinds of interaction scenarios
- Interaction can be configured on a per-hand basis


### **Developer Friendly**
- ScriptableObject-driven architecture
- Comprehensive editor tools with real-time preview
- UniRx integration for reactive programming
- Clear separation of concerns
- Hand Poses are totally independent of VR SDKs allowing for on non-XR games

### **Designer Friendly**
- Visual editor tools with interactive scene view
- Drag-and-drop sequence creation
- Real-time feedback system configuration
- No coding required for basic experience design

## ✨ Features

- 🎯 **Complete Interaction System** - Grab, throw, press, switch, rotate, and more
- ✋ **Hand Presence** - Realistic hand models with dynamic pose blending
- 🎮 **Multiple Interactor Types** - Trigger-based, raycast, and direct hand interaction
- 📦 **Pre-built Components** - Buttons, switches, levers, joysticks, drawers, and more
- 🎨 **Feedback System** - Haptic, audio, and visual feedback out of the box
- 🔧 **Designer-Friendly** - Inspector-focused workflow with tooltips and validation
- 📊 **Data Flow System** - Scriptable variables and events for decoupled architecture
- 🎬 **Sequencing System** - Built-in tutorial and quest system
- 🔌 **Socket System** - Object placement and snapping

---


## 🚀 Quick Start

### Installation

#### Via Package Manager (Recommended)
```
1. Open Package Manager (Window > Package Manager)
2. Click + > Add package from git URL
3. Enter: https://github.com/Shababeek/Interactions.git
```

#### Manual Installation
```
1. Download the latest release
2. Extract to your project's Assets folder
3. Install dependencies (Unity Input System)
```

### Your First interactable Object

```csharp
1. Create a GameObject (e.g., Cube)
2. Right click in the hierarchy -> Shababeek-> Convert To Grabbable
3. Ensure it has a Collider
4. Press Play and grab it with your VR controllers!
```

**📚 Full guide:** [10-Minute Quick Start](Documentation/GettingStarted/QuickStart.md)

---

## 📖 Documentation

### 🎓 Getting Started
- **[Quick Start Guide](Documentation/GettingStarted/QuickStart.md)** - 10-minute guide from install to first interaction
- **[Component Overview](Documentation/README.md)** - Catalog of all components
- **[Core Concepts](UserManual.md)** - System architecture and design principles

### 📘 Component Manuals
- **[Grabable](Documentation/Interactables/Grabable.md)** - Pick up and throw objects
- **[Switch](Documentation/Interactables/Switch.md)** - Toggle switches and buttons
- **[Hand System](Documentation/Interactors/Interactors.md)** - Hand models and pose configuration
- **[Feedback System](Documentation/Systems/FeedbackSystem.md)** - Haptics, audio, and visuals
- **[More components...](Documentation/README.md)**

### 🛠️ Advanced Topics
- **[Custom Interactables](UserManual.md#customization--extensibility)** - Create your own components
- **[Scriptable System](Documentation/ScriptableSystem/ScriptableVariables.md)** - Variables and events
- **[Sequencing](Documentation/Systems/SequencingSystem.md)** - Tutorials and quests
- **[Hand Poses](Documentation/PoseSystem/PoseConstrainer.md)** - Import and configure hands

### 💻 For Developers
- **[Scripting Reference](Documentation/README.md)** - Complete API documentation
- **[Component Reference](Documentation/README.md)** - All components indexed

---

## 🎯 Core Components

### Interactables
Make objects interactive in your VR scene:

| Component | Description | Use Case |
|-----------|-------------|----------|
| **Grabable** | Pick up and throw objects | Tools, weapons, props |
| **Switch** | Toggle between states | Light switches, levers |
| **VRButton** | Pressable button | Control panels, keypads |
| **Joystick** | Virtual joystick control | Vehicle controls |
| **Lever** | Pull/push interaction | Gear shifts, throttles |
| **Wheel** | Rotatable wheel | Valves, steering wheels |
| **Drawer** | Sliding compartment | Desks, toolboxes |

### Interactors
Detect and manage interactions:

| Component | Description | Use Case |
|-----------|-------------|----------|
| **Hand** | Full VR hand with poses | Main player hand |
| **TriggerInteractor** | Proximity detection | Close-range grabbing |
| **RaycastInteractor** | Ray-based selection | Distant object selection |

### Systems
Additional functionality:

| System | Description |
|--------|-------------|
| **Feedback System** | Haptic, audio, and visual feedback |
| **Scriptable Variables** | Observable shared data |
| **Game Events** | Decoupled event communication |
| **Sequences** | Tutorial and quest system |
| **Sockets** | Object placement and snapping |

---

## 💡 Examples

### Basic Grabbable Object
```csharp
// 1. Create a Cube
// 2. Add Grabable component
// 3. It automatically adds:
//    - PoseConstrainer (hand positioning)
//    - Detects Rigidbody (for physics)
// 4. Done! Grab it in VR
```

### Button with Sound
```csharp
// 1. Add VRButton component
// 2. Add AudioSource component
// 3. Wire OnButtonDown event to AudioSource.Play()
// 4. Press button in VR to hear sound
```

### Flashlight Tool
```csharp
// 1. Create flashlight model with Light component
// 2. Add Grabable component
// 3. Wire OnUseStarted event to Light.enabled toggle
// 4. Grab flashlight and press Trigger to turn on/off
```

**📚 More examples:** [Component Manuals](Documentation/README.md)

---

## 🎨 Features Showcase

### Hand Presence
- Multiple hand models (available in asset store only)
- Dynamic pose blending based on object type
- Custom pose creation system
- Finger-level control

### Physics Integration
- Realistic grabbing with Rigidbody
- Throw mechanics with velocity calculation
- Collision-based interaction detection
- Socket snapping with constraints

### Event System
- UnityEvent integration for designer workflows
- UniRx observables for reactive programming
- Scriptable events for decoupled architecture
- Built-in lifecycle events (hover, select, activate)

---

## 🔧 System Requirements

- **Unity Version:** 6.0 or newer (2021.3 LTS+ supported)
- **Dependencies:**
    - Unity Input System (1.0.0+)
    - UniRx (included)
- **Platforms:** PC VR, Quest, PSVR, and all Unity-supported VR platforms
- **XR Plugin:** OpenXR recommended, Oculus works but not fully tested

---

## 📦 Package Structure

```
Shababeek Interactions/
├── Scripts/
│   ├── InteractionSystem/      # Core interaction components
│   ├── SequencingSystem/       # Tutorial/quest system
│   ├── Core/                   # Scriptable variables & events
│   └── Utility/                # Helper utilities
├── Documentation/              # Complete documentation
│   ├── GettingStarted/         # Quick start guides
│   ├── Interactables/          # Component documentation
│   ├── Interactors/            # Interactor documentation
│   ├── Systems/                # System documentation
│   ├── PoseSystem/             # Pose constrainer docs
│   ├── SocketSystem/           # Socket system docs
│   ├── ScriptableSystem/       # Scriptable system docs
│   ├── Tutorials/              # Video script tutorials
│   └── Images/                 # Documentation images
├── EditorResources/            # Editor icons and assets
├── Resources/                  # Runtime resources
├── Plugins/                    # Third-party plugins
└── Data/                       # Configuration data
```

---

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. **Documentation** - Improve guides and add screenshots
2. **Bug Reports** - [Open an issue](https://github.com/Shababeek/Interactions/issues)
3. **Feature Requests** - Share your ideas
4. **Pull Requests** - Submit improvements

For contribution guidelines, please reach out to the team at Ahmadabobakr@gmail.com

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.

---

## 📞 Support

- **Documentation:** [Full Documentation](Documentation/README.md)
- **Issues:** [GitHub Issues](https://github.com/Shababeek/Interactions/issues)
- **Discussions:** [GitHub Discussions](https://github.com/Shababeek/Interactions/discussions)
- **Email:** Ahmadabobakr@gmail.com
- **Website:** [ahmadabobakr.github.io](https://ahmadabobakr.github.io)

---

## 🙏 Acknowledgments

Created by **Ahmad Abo Bakr** at **Shababeek**

Special thanks to all contributors and the Unity VR community.

---

## 🗺️ Roadmap

- [ ] Video tutorial series
- [ ] More example scenes
- [ ] Enhanced physics interactions
- [ ] Multiplayer support
- [ ] Migrate from UniRX to R3
- [ ] Additional hand models


---

**Ready to get started?** → [Quick Start Guide](Documentation/GettingStarted/QuickStart.md)

**Need help?** → [Documentation](Documentation/README.md)

**Want to contribute?** → [Contact the Team](mailto:Ahmadabobakr@gmail.com)

---

<div align="center">

Made with ❤️ by the Shababeek team

[⭐ Star us on GitHub](https://github.com/Shababeek/Interactions) | [📖 Read the Docs](Documentation/README.md) | [💬 Join Discussion](https://github.com/Shababeek/Interactions/discussions)

</div>
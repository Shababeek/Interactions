# Shababeek Interaction System

![Unity Version](https://img.shields.io/badge/Unity-6.0%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![Version](https://img.shields.io/badge/Version-1.0.0-orange)

A comprehensive Unity package for building advanced VR/AR and 3D interactions with a focus on hand presence, pose constraints, and reactive programming.

## 🎯 Why Choose Shababeek Interactions?

### **Hand-First Design**
- Built around natural hand presence and interactions
- Advanced pose constraint system with smooth transitions
- Support for different kinds of interaction scenarios
- Interaction can be configured on a per-hand basis

### **Performance Optimized**
- Vector-based positioning (no Transform dependencies)
- Cached fake hand creation and reuse
- Configurable smooth transitions
- Optimized for VR applications

### **Developer Friendly**
- ScriptableObject-driven architecture
- Comprehensive editor tools with real-time preview
- UniRx integration for reactive programming
- Clear separation of concerns
- Hand Poses are totally independed of VR SDKs allowing for on non XR games

### **Designer Friendly**
- Visual editor tools with interactive scene view
- Drag-and-drop sequence creation
- Real-time feedback system configuration
- No coding required for basic experience design

## ✨ Key Features

### **🎮 Complete Interaction System**
- **10+ Interactable Types**: Grabable, Throwable, Lever, Drawer, Button, Switch, Joystick, Wheel, Turret, Spawning
- **Multiple Interaction Methods**: Trigger (near objects) and Raycast (far objects)
- **Extensible Architecture**: Easy to create custom interactables

### **🤲 Advanced Hand System**
- **6 Hand Models(Asset store only)**: Biker, Cartoon, Robot, Sci-fi, Viking, Realistic Woman, more are being added regularly
- **Dynamic Pose System**: Real-time hand pose blending and animation
- **Pose Constraints**: Hide, Free, or Constrained hand behavior
- **Smooth Transitions**: Configurable lerp-based positioning

### **🔄 Reactive Programming**
- **UniRx Integration**: Observable patterns throughout the system
- **Scriptable Variables**: Sharing variables across different component made easy
- **Event System**: Decoupled event-driven architecture
- **UI Binding**: Live UI updates with variable changes

### **🎵 Comprehensive Feedback System**
- **Multi-Modal Feedback**: Haptic, Audio, Visual, Animation
- **Event-Driven**: Automatic triggering based on interactions
- **Configurable**: Per-interaction type settings (hover, select, activate)
- **Performance Optimized**: Efficient feedback management

### **📋 Sequencing System**
- **Linear Sequences**: Step-by-step processes with audio support
- **Branching Sequences**: Complex conditional flows with visual editor
- **12+ Action Types**: Variable, Timer, Gesture, Gaze, and more
- **Event Listeners**: Monitor and respond to sequence events

## 🚀 Quick Start

### **1. Installation**
1. Import the Shababeek Interaction System package
2. Go to `Shababeek > Initialize Scene` to set up VR camera rig
3. Configure which hand to use in `Assets/Shababeek/Interactions/Data/config.asset`

### **2. Create Your First Interaction**
1. Right-click in Hierarchy → `Create > Shababeek > Interactables > Grabable`
2. Configure the interaction settings in the Inspector
3. Test in VR!

### **3. Add Feedback**
1. Find the **FeedbackSystem** component on your interactable
2. Add **HapticFeedback**, **AudioFeedback**, or **MaterialFeedback**
3. Configure settings for different interaction types

## 📖 Documentation

### **Getting Started**
- [Quick Start Guide](Documentation/QuickStartGuide.md) — Get up and running in minutes
- [System Overview](Documentation/SystemOverview.md) — Complete architecture breakdown

### **Component Guides**
- [Interactables Manual](Documentation/Interactables.md) — All interactable types and usage
- [Hand Posing & Constraints](Documentation/HandPosingAndConstraints.md) — Advanced hand management
- [Feedback System](Documentation/FeedbackSystem.md) — Multi-modal feedback setup
- [Sequence System](Documentation/SequenceSystem.md) — Step-by-step interaction flows
- [Scriptable System](Documentation/ScriptableSystem.md) — Variables and events

### **API Reference**
- [Component Reference](Documentation/ComponentReference.md) — Complete API documentation
- [Scripting Reference](Documentation/ScriptingReference.md) — Code examples and patterns

## 🎯 Perfect For

### **VR/AR Applications**
- Immersive training simulations
- Educational experiences
- Interactive museums and exhibits
- Virtual laboratories

### **Gaming & Entertainment**
- VR puzzle games
- Interactive storytelling
- Simulation games
- Educational games

### **Professional Applications**
- Industrial training
- Medical simulations
- Architectural visualization
- Product demonstrations

## 🛠️ Requirements

- **Unity**: 2022.3 LTS or newer
- **Packages**: OpenXR, XR Interaction Toolkit
- **Platform**: PC, Android, iOS (VR/AR capable)
- **VR Headsets**: Oculus Quest 2/3, HTC Vive, Valve Index, and more

## 📦 What's Included

### **Core Systems**
- Complete interaction framework
- Hand pose management system
- Reactive programming utilities
- Comprehensive feedback system
- Sequencing and workflow tools

### **Assets & Models**
- 6 different hand models with animations
- Pre-configured interactable prefabs
- Example materials and textures
- Demo scenes and examples

### **Documentation**
- Complete user manual
- API reference documentation
- Video tutorials and examples
- Best practices guide

## 🔄 How It's Different

### **vs XR Interaction Toolkit**
- **Scriptable System Core**: Built around ScriptableObjects for modularity
- **Hand Pose Constraints**: Advanced pose management not available in XRITK
- **Reactive Programming**: UniRx integration for complex behaviors
- **Designer-Friendly**: Visual tools for non-programmers

### **vs SteamVR Interaction System**
- **Platform Agnostic**: Not tied to Steam VR
- **Modern Architecture**: Built for Unity 2022.3+
- **Comprehensive Documentation**: Complete guides and examples
- **Extensible Design**: Easy to customize and extend

## 🎬 Example Use Cases

## 🏆 Success Stories

*"The Shababeek Interaction System transformed our VR training application. The hand pose constraints make interactions feel natural, and the sequencing system made it easy to create complex training scenarios."*
— VR Training Company

*"As a designer with limited coding experience, I was able to create rich interactive experiences using just the visual tools. The documentation is excellent!"*
— Experience Designer

## 📞 Support

### **Documentation & Guides**
- Comprehensive documentation included
- Video tutorials and examples
- Best practices and troubleshooting guides

### **Community Support**
- Active community forum
- Regular updates and improvements
- Responsive developer support

### **Professional Support**
- Priority email support for commercial licenses
- Custom development consultation available
- Training and workshops for teams

## 📋 Changelog

### **Version 1.0.0** (Current)
- Complete interaction system with 10+ interactable types
- Advanced hand pose constraint system
- Reactive programming with UniRx integration ( will migrate to R3 soon)
- Comprehensive feedback system
- Sequencing system 
- Scriptable variable system
- 6 different hand models(Asset store exclusive)
- Complete documentation and examples

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 👨‍💻 Author

**Ahmad Abobakr**  
*Founder, Shababeek*

- Website: [shababeek.com](https://shababeek-labs.com)
- Email: support@shababeek.com
- LinkedIn: [Ahmad Abobakr](https://linkedin.com/in/ahmadabobakr)

---

⭐ **If you find this asset useful, please consider leaving a review on the Asset Store! and starring this repo**

*Made with ❤️ for the Unity VR/AR community*
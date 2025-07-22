# Shababeek Interaction System

The **Shababeek Interaction System** is a modular, extensible Unity package for building advanced VR/AR and 3D interactions. It is designed to be more flexible, decoupled, and data-driven than Unity’s XR Interaction Toolkit or SteamVR Interaction System.

## Why Use This Asset?

- **Decoupled Architecture:** Uses ScriptableObjects for events and variables, enabling designer-friendly workflows and easy decoupling of logic.
- **Reactive Programming:** Built-in UniRx support for observables and event streams.
- **Customizable Interactions:** Easily extend or create new interactables, interactors, and feedback systems.
- **Hand Pose & Physics:** Advanced hand pose control, constraints, and physics-based or transform-based interaction.
- **Editor Tools:** Includes custom editors and utilities for rapid prototyping and debugging.

## How is it Different from XR Interaction Toolkit or SteamVR?

- **Scriptable System Core:** Unlike XRITK/SteamVR, this system is built around ScriptableObjects for both events and variables, making it more modular and testable.
- **Reactive & Data-Driven:** Uses UniRx for event streams, allowing for more complex, reactive behaviors.
- **Customizability:** You can easily add new types of interactors (e.g., gesture, raycast, trigger), interactables, and feedback mechanisms.
- **Hand Pose Constraints:** Built-in support for pose constraints and dynamic hand pose blending, not present in most other toolkits.
- **Not Tied to XR:** Can be used for non-XR 3D interactions as well (components are isolated to allow for such thing).

## Design Principles
- **Simplicity:** You can create powerful interactables with a single script or component—no need to distribute logic across multiple objects or scripts for basic use cases.
- **Focus on Interactions:** The system is purpose-built for object and hand interactions, intentionally leaving out locomotion, teleportation, and unrelated VR mechanics for maximum clarity and maintainability.
- **Hand Presence First:** The system is designed around hand presence. Each object can be grabbed and interacted with in a way defined by the designer, ensuring natural, immersive hand-object interactions.
- **Unity-Native Workflow:** No external editors or JSON files are required. Everything is done the Unity way, using ScriptableObjects, components, and the Inspector.
- **Low-Code, Highly Extendible:** Full experiences can be built with little to no coding. The system is designed to be extendible and decoupled, so advanced users can customize or expand any part.
- **Separation of Concerns:** Logic, data, and presentation are separated using ScriptableObjects and MonoBehaviours.
- **Extensibility:** Core interfaces and base classes allow for easy extension and customization.
- **Observability:** All key events and variable changes are observable via UniRx.
- **Editor Friendliness:** Designed for both programmers and designers, with custom inspectors and menu items.

## Core Components

- **Scriptable System:**  
  - `ScriptableVariable<T>`: Observable, serializable variables (float, int, bool, etc.) for decoupled data flow.
  - `GameEvent<T>`: Observable, serializable events for decoupled event-driven logic.
  - `VariableToUIBinder`: Binds variables to UI for live updates.

- **Interaction System:**  
  - `InteractorBase`: Base class for all interactors (hands, raycasters, triggers).
  - `TriggerInteractor`, `RaycastInteractor`: Example interactors.
  - `InteractionState`: Enum for interaction states (None, Hovering, Selected, Activated).
  - `InteractableBase`: Base class for all interactable objects.
  - `Grabable`, `Switch`, `TurretInteractable`, etc.: Example interactables.
  - `FeedbackSystem`: Unified feedback for haptics, audio, and visual cues.
  - `Sockets`: Abstract socket system for snap/interlock mechanics.
- **Hand & Pose System:**  
  - `Hand`: Represents a VR hand, manages input, pose, and constraints.
  - `HandPoseController`: Controls hand pose blending and animation.
  - `InteractionPoseConstrainer`, `HandConstraints`: For pose and position constraints during interaction.

- **Tween System:**  
  - `ITweenable`, `VariableTweener`, `TweenableFloat`, `TransformTweenable`: For smooth, frame-based or async animations.

## Documentation

All detailed documentation is located in [`Assets/Shababeek/Documentation`](Assets/Shababeek/Documentation):

- [UserManual.md](Assets/Shababeek/Documentation/UserManual.md) — General usage and concepts
- [GettingStarted.md](Assets/Shababeek/Documentation/GettingStarted.md) — Quick setup and first steps
- [ComponentReference.md](Assets/Shababeek/Documentation/ComponentReference.md) — Index of all core components
- [Interactors.md](Assets/Shababeek/Documentation/Interactors.md) — How to use and configure interactors
- [Interactables.md](Assets/Shababeek/Documentation/Interactables.md) — All interactable types and setup
- [FeedbackSystem.md](Assets/Shababeek/Documentation/FeedbackSystem.md) — Adding haptic, audio, and visual feedback
- [ScriptableVariable.md](Assets/Shababeek/Documentation/ScriptableVariable.md) — Using scriptable variables
- [Sockets.md](Assets/Shababeek/Documentation/Sockets.md) — Using the socket system
- [HandDataAndPoses.md](Assets/Shababeek/Documentation/HandDataAndPoses.md) — Importing hands and configuring poses
- [Config.md](Assets/Shababeek/Documentation/Config.md), [Hand.md](Assets/Shababeek/Documentation/Hand.md), etc. — Detailed component manuals
- [ScriptingReference.md](Assets/Shababeek/Documentation/ScriptingReference.md) — API and code samples

## Getting Started

1. **Import the Asset:**  
   Copy the `Assets/Shababeek` folder into your Unity project.

2. **Setup:**  
   - Create a `Config` asset via the menu.
   - Assign hand data, layers, and input settings in the Config.
   - Add `Hand`, `Interactor`, and `Interactable` components to your scene objects.

3. **Using as a Unity Package:**  
   - Copy `Assets/Shababeek/Interactions` to your `Packages` folder for embedded use.
   - Or reference it via a local path in your `manifest.json`.

## Example Use Cases

- VR hand grabbing, throwing, and socketing objects.
- Custom switches, levers, and dials with feedback.
- Reactive UI and variable binding for in-game displays.

## Author

- **Author:** Ahmad abobakr  
- **Company:** Shababeek

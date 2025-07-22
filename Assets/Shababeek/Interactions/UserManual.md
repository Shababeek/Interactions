# Shababeek Interaction System — User Manual

## Introduction
The Shababeek Interaction System is a Unity package for building advanced, designer-friendly VR/AR and 3D interactions. This manual will guide you through installation, setup, and usage.

## Installation
1. Copy the `Assets/Shababeek` folder into your Unity project.
2. (Optional) For Unity Package Manager: Move `Assets/Shababeek/Interactions` to your `Packages` folder or reference it via a local path in `manifest.json`.
3. Ensure the Unity Input System package is installed (via Package Manager).

## Quick Start
1. Create a new `Config` asset via the Unity menu (right-click in Project window > Create > Shababeek > Interactions > Config).
2. Assign hand data, layers, and input settings in the Config asset.
3. Add `Hand`, `Interactor`, and `Interactable` components to your scene objects.
4. Play the scene and interact with objects using your VR controllers or input devices.

## Core Concepts
- **Scriptable Variables & Events:** Use ScriptableObjects to store and observe data and events.
- **Interactables:** Objects that can be grabbed, activated, or manipulated.
- **Interactors:** Components (e.g., hands, raycasters) that interact with interactables.
- **Hand Presence:** The system is designed around natural hand-object interactions.
- **Feedback System:** Add haptic, audio, or visual feedback to any interaction.

## Creating Interactables
1. Add an `InteractableBase`-derived component (e.g., `Grabable`, `Switch`) to your GameObject.
2. Configure interaction settings in the Inspector (e.g., which hand, selection button, feedback).
3. (Optional) Add constraints or custom feedback via additional components.

## Customization & Extensibility
- **Add new interactables:** Inherit from `InteractableBase` and implement required methods.
- **Add new interactors:** Inherit from `InteractorBase` for custom input or interaction logic.
- **Extend feedback:** Use or extend the `FeedbackSystem` for custom responses.
- **Scriptable System:** Use or create new ScriptableVariables and GameEvents for decoupled logic.

## Troubleshooting
- Ensure all required layers and input settings are configured in the Config asset.
- Check console for errors related to missing references or input system setup.
- For hand presence issues, verify hand data and pose constraints are assigned.

## Support & Contact
- **Author:** Ahmadabobakr
- **Company:** Shababeek
- For questions, issues, or feature requests, contact Ahmadabobakr@gmail.com or visit [Ahmadabobakr.github.io](https://Ahmadabobakr.github.io) 
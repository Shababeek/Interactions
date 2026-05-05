# Shababeek Interaction System — User Manual

## Introduction
The Shababeek Interaction System is a Unity package for building designer-friendly VR/AR and 3D interactions. Work **in the Editor first** (Project assets, Inspector, hierarchy menus), then add scripts only when needed.

## Installation
1. Add this package via **Package Manager** (git URL or local path), or copy the package folder into your Unity project per your studio workflow.
2. Install **Unity Input System** (Package Manager dependency).
3. Add **com.shababeek.reactivevars** when using variables, drivers, or sequencing.

## Quick Start
1. Create a **Config** asset: Project window → **Create → Shababeek → Interactions → Config**.
2. Use **Shababeek → Initialize Scene** (or the Setup Wizard) so **Camera Rig**, **Hand**, and interactors are created for you.
3. Assign **Hand Data**, layers, and input on the Config asset; drag Config onto **Camera Rig** if needed.
4. Add interactables (e.g. **Grabable**) via **Shababeek** hierarchy menus or **Add Component**.
5. Enter Play Mode and test with your XR device or simulated input.

## Core Concepts
- **Interactables:** Objects that can be grabbed, activated, or manipulated.
- **Interactors:** Components (e.g., hands, raycasters) that interact with interactables.
- **Hand Presence:** The system is designed around natural hand-object interactions.
- **Feedback System:** Add haptic, audio, or visual feedback to any interaction.
- **Reactive Architecture:** This package integrates with the **com.shababeek.reactivevars** package for data flow, reactive variables, and sequencing systems.

## Creating Interactables
1. Add an `InteractableBase`-derived component (e.g., `Grabable`, `Switch`) to your GameObject.
2. Configure interaction settings in the Inspector (e.g., which hand, selection button, feedback).
3. (Optional) Add constraints or custom feedback via additional components.

## Customization & Extensibility
- **Add new interactables:** Inherit from `InteractableBase` and implement required methods.
- **Add new interactors:** Inherit from `InteractorBase` for custom input or interaction logic.
- **Extend feedback:** Use or extend the `FeedbackSystem` for custom responses.
- **Reactive Logic:** Use the ReactiveVars package (ScriptableVariables, GameEvents, and Interaction Drivers) to create decoupled systems without additional code.

## Troubleshooting
- Ensure all required layers and input settings are configured in the Config asset.
- Check console for errors related to missing references or input system setup.
- For hand presence issues, verify hand data and pose constraints are assigned.

## Support & Contact
- **Author:** Ahmadabobakr
- **Company:** Shababeek
- For questions, issues, or feature requests, contact Ahmadabobakr@gmail.com or visit [Ahmadabobakr.github.io](https://Ahmadabobakr.github.io) 
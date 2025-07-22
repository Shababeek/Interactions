# Shababeek Interaction System — User Manual

## Overview
The Shababeek Interaction System is a modular Unity package for building advanced VR/AR and 3D object interactions. It is designed for flexibility, simplicity, and designer-friendly workflows.

## Features
- ScriptableObject-based variables and events
- Modular interactables and interactors
- Hand presence and pose constraints
- Feedback system for haptics, audio, and visuals
- Unity-native workflow (no external editors)

## Concepts
- **Interactables:** Objects that can be grabbed, activated, or manipulated.
- **Interactors:** Components (e.g., hands, raycasters) that interact with interactables.
- **Scriptable Variables & Events:** Data and event decoupling using ScriptableObjects.
- **Hand Presence:** Natural, designer-defined hand-object interactions.

## Workflow
1. Import the package into your Unity project.
2. Create a Config asset and assign hand data, layers, and input settings.
3. Add Hand, Interactor, and Interactable components to your scene objects.
4. Configure interactables in the Inspector.
5. Play the scene and interact using your input devices.

## Inspector Reference
- **Config:** Central settings for hands, layers, and input.
- **InteractableBase:** Base for all interactable objects. Configure hand, selection button, and feedback.
- **InteractorBase:** Base for all interactors (hands, raycasters, triggers).
- **FeedbackSystem:** Add and configure feedback responses.

## FAQ
**Q:** Do I need to write code to use the system?
**A:** No, most experiences can be built with components and ScriptableObjects. Coding is only needed for custom logic.

**Q:** Can I use this with the Unity XR Interaction Toolkit?
**A:** The system is independent, but can coexist with other toolkits if needed.

## Best Practices
- Use ScriptableObjects for all shared data and events.
- Keep interactable logic in a single component when possible.
- Use the FeedbackSystem for haptics and audio cues.
- Test hand presence and pose constraints for each interactable. 
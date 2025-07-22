# Interactors — Manual

## Overview
Interactors are components that enable user input and interaction with interactable objects in the Shababeek Interaction System. They manage the logic for detecting, selecting, and activating interactables, and are typically attached to hand GameObjects.

[screenshot of Camera Rig with Interactor components in the Inspector]

## Types of Interactors

### 1. TriggerInteractor
- Uses Unity's trigger collider system to detect interactions.
- Allows for hover and selection based on trigger events.
- Designed for physical hand presence and direct touch.
- Add to a hand GameObject with a collider set as a trigger.

[screenshot of TriggerInteractor component and collider setup]

### 2. RaycastInteractor
- Uses raycasting to interact with objects at a distance.
- Includes a LineRenderer for visual feedback.
- Useful for UI, distant object selection, or pointer-based interaction.
- Add to a hand or controller GameObject; configure raycast origin and settings.

[screenshot of RaycastInteractor Inspector with raycast settings and LineRenderer]

### 3. (Custom Interactors)
- You can create your own by inheriting from InteractorBase and implementing custom logic.

## Adding/Selecting Interactors in the Camera Rig
- In your Camera Rig or hand prefab, add the desired Interactor component (TriggerInteractor, RaycastInteractor, etc.) to the hand GameObject.
- You can have multiple interactors per rig, but typically one per hand.
- Configure the Interactor in the Inspector (e.g., raycast settings, trigger collider).

[screenshot of Camera Rig prefab with both hands and interactors visible]

## Usage Tips & Best Practices
- Use TriggerInteractor for natural, physical hand interactions.
- Use RaycastInteractor for UI or distant object selection.
- Only one Interactor should be active per hand at a time for clarity.
- Customize or extend Interactors for unique input devices or behaviors. 
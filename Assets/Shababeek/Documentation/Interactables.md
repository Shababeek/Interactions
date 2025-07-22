# Interactables — Manual

## Overview
Interactables are objects in your scene that users can grab, activate, or manipulate using Interactors. They define how objects respond to user input and interaction events.

[screenshot of Inspector with a Grabable component on a GameObject]

## Main Types of Interactables

### 1. Grabable
- Allows objects to be grabbed and held by the user.
- Supports hand presence and designer-defined grab points.
- Can be combined with other components (e.g., Rigidbody, FeedbackSystem).

[screenshot of Grabable Inspector with hand and selection button settings]

### 2. Switch
- Represents a switch or lever that can be toggled or rotated.
- Can invoke UnityEvents on up/down/hold actions.

[screenshot of Switch Inspector with UnityEvents]

### 3. TurretInteractable
- Allows for constrained rotation (e.g., turrets, dials).
- Supports rotation limits and return-to-original behavior.

[screenshot of TurretInteractable Inspector showing rotation limits]

### 4. Throwable
- Extends Grabable to support throwing with velocity and angular velocity.
- Tracks velocity samples for realistic throws.

[screenshot of Throwable Inspector with throw settings]

### 5. Sockets
- AbstractSocket and related types allow for snap/snap-in-place mechanics.
- Used for plug-and-play or modular assembly interactions.

[screenshot of Socket Inspector with socket settings]

## Adding & Configuring Interactables
- Add the desired Interactable component (e.g., Grabable, Switch) to a GameObject in your scene.
- Configure properties in the Inspector (e.g., hand, selection button, feedback, constraints).
- Combine with Rigidbody, FeedbackSystem, or other components for advanced behavior.

[screenshot of GameObject with Grabable, Rigidbody, and FeedbackSystem components]

## Usage Tips & Best Practices
- Use Grabable for most pick-up objects.
- Combine Interactables with FeedbackSystem for haptics/audio.
- Use constraints (e.g., TurretInteractable) for dials, levers, or limited-movement objects.
- Test hand presence and grab points for each interactable. 
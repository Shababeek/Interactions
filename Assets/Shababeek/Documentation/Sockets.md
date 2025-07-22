# Socket System — Manual

## Overview
The Socket system enables objects to be snapped or inserted into designated locations (sockets) in your scene. This is useful for plug-and-play mechanics, modular assembly, or tool placement.

## Main Components
- **AbstractSocket:** Base class for all socket types. Handles connection, disconnection, and hover events.
- **Socketable:** Interface or component for objects that can be inserted into sockets.
- **Custom Sockets:** You can extend AbstractSocket for specialized behavior.

[screenshot of a GameObject with an Socketable component in the Inspector]

## How to Use
1. Add an AbstractSocket-derived component to a GameObject where you want objects to be inserted.
2. Add a Socketable component to any object you want to be insertable.
3. Configure socket properties in the Inspector (e.g., snap point, allowed types).
4. When a Socketable comes near, it will snap or connect to the socket.

[screenshot of configuring snap settings in the Socket Inspector]
[screenshot of a Socketable object being inserted into a socket]

## Inspector Properties
- **onSocketConnected** (UnityEvent<Socketable>): Event when an object is inserted.
- **onSocketDisconnected** (UnityEvent<Socketable>): Event when an object is removed.
- **onHoverStart/onHoverEnd** (UnityEvent<Socketable>): Events for hover feedback.
- **Pivot** (Transform): The snap point for inserted objects.

## Workflow
- Use sockets for tool racks, modular parts, or any system where objects need to be placed precisely.
- Combine with FeedbackSystem for haptic/audio/visual feedback on snap.

[screenshot of a modular assembly setup using sockets] 
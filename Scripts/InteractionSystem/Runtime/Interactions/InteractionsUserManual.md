# Shababeek Interactions System - User Manual

This manual explains how to use the Shababeek Interactions system from the perspective of a Unity developer working in the Inspector. The system provides a complete framework for creating interactive objects in VR that can be grabbed, manipulated, and used.

## Table of Contents

1. [System Overview](#system-overview)
2. [Core Components](#core-components)
3. [Interactables](#interactables)
4. [Interactors](#interactors)
5. [Grab Strategies](#grab-strategies)
6. [Editor Tools](#editor-tools)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

## System Overview

The Shababeek Interactions system is built around three core concepts:

- **Interactables**: Objects that can be interacted with (grabbed, used, etc.)
- **Interactors**: Components that detect and manage interactions (usually attached to hands)
- **Grab Strategies**: Different ways objects can be grabbed and manipulated

The system automatically handles:
- Hover detection and visual feedback
- Selection through button presses
- Secondary actions (use/activate)
- Physics-based or transform-based grabbing
- Event dispatching for custom behaviors

## Core Components

### InteractionState Enum

Defines the three possible states of an interaction:

- **None**: No interaction is currently active
- **Hovering**: An interactor is hovering over the object
- **Selected**: An interactor has selected the object and can perform actions

### InteractionHand Enum

Specifies which hands can interact with an object:

- **Left**: Only the left hand can interact
- **Right**: Only the right hand can interact
- **Both**: Both hands can interact (default)

### InteractorUnityEvent

A UnityEvent wrapper that automatically passes the interactor reference when invoked. Use this in the inspector to create callbacks that receive the hand that triggered the interaction.

## Interactables

### InteractableBase

The base class for all interactive objects. Provides the foundation for hover, selection, and activation interactions.

#### Inspector Settings

**Interaction Settings:**
- **Interaction Hand**: Which hands can interact with this object (Left, Right, or Both)
- **Selection Button**: The button that triggers selection (Grip or Trigger)

**Interaction Events:**
- **On Selected**: Event raised when this object is selected
- **On Deselected**: Event raised when this object is deselected
- **On Hover Start**: Event raised when hovering begins
- **On Hover End**: Event raised when hovering ends
- **On Use Started**: Event raised when the secondary button is pressed while selected
- **On Use Ended**: Event raised when the secondary button is released while selected

**Runtime State (Read-Only):**
- **Is Selected**: Whether this object is currently selected
- **Current Interactor**: The interactor currently interacting with this object
- **Current State**: The current interaction state (None, Hovering, or Selected)
- **Is Using**: Whether this object is currently being used

#### Usage

1. Inherit from `InteractableBase` in your custom interactable classes
2. Override the abstract methods:
   - `Select()`: Handle selection logic
   - `DeSelected()`: Handle deselection logic
   - `StartHover()`: Handle hover start
   - `EndHover()`: Handle hover end
   - `UseStarted()`: Handle secondary button press
   - `UseEnded()`: Handle secondary button release (optional)

3. Set up events in the inspector to trigger custom behaviors

### Grabable

A component that enables objects to be grabbed and held by VR hands. Manages the grabbing process with smooth animations and pose constraints.

#### Inspector Settings

**Grab Settings:**
- **Hide Hand**: Whether to hide the hand model when this object is grabbed
- **Tweener**: The VariableTweener component for smooth animations (auto-added if not present)

#### Required Components

- **PoseConstrainter**: Manages hand positioning and constraints
- **VariableTweener**: Handles smooth grab animations

#### Usage

1. Add the `Grabable` component to any object you want to be grabbable
2. Ensure the object has a `PoseConstrainter` component
3. The system automatically handles grab/ungrab logic
4. Use the `RightHandRelativePosition` and `LeftHandRelativePosition` properties to customize grab positions

#### Public Properties

- **RightHandRelativePosition**: Transform for the right hand's grab position
- **LeftHandRelativePosition**: Transform for the left hand's grab position
- **GetRightHandTarget()**: Returns target position and rotation for right hand
- **GetLeftHandTarget()**: Returns target position and rotation for left hand

### Switch

A physical switch component that responds to trigger interactions. Automatically rotates based on interaction direction and raises events.

#### Inspector Settings

**Events:**
- **On Up**: Event raised when switch moves to up position
- **On Down**: Event raised when switch moves to down position
- **On Hold**: Event raised when switch is held in a position

**Switch Configuration:**
- **Switch Body**: The transform that rotates during interaction
- **Rotation Axis**: Axis around which the switch rotates (X, Y, or Z)
- **Detection Axis**: Axis used to detect interaction direction
- **Up Rotation**: Rotation angle in degrees for the up position
- **Down Rotation**: Rotation angle in degrees for the down position
- **Rotate Speed**: Speed of rotation animation in degrees per second
- **Angle Threshold**: Angle threshold in degrees for direction detection
- **Stay In Position**: Whether the switch stays in position instead of returning to neutral
- **Starting Position**: Starting position when the scene starts (Off, Neutral, or On)

**Debug:**
- **Direction**: Current direction of the switch (read-only)

#### Usage

1. Add the `Switch` component to a GameObject
2. Configure the rotation and detection axes
3. Set up the rotation angles and speed
4. Connect events in the inspector to trigger custom behaviors
5. The switch automatically detects trigger interactions and rotates accordingly

#### Public Methods

- **ResetSwitch()**: Resets to neutral position (respects stayInPosition setting)
- **ForceResetSwitch()**: Forces reset to neutral regardless of settings
- **GetSwitchState()**: Returns current state (true=up, false=down, null=neutral)
- **GetCurrentRotation()**: Returns current rotation as Vector3
- **SetPosition(StartingPosition)**: Sets switch to specific position

### VRButton

A simple button component that responds to trigger interactions.

#### Inspector Settings

- **On Press**: Event raised when the button is pressed
- **On Release**: Event raised when the button is released

### Throwable

A component that makes objects throwable when released from a grab.

#### Inspector Settings

- **Throw Force**: Multiplier for throw force
- **Angular Velocity**: Angular velocity applied when thrown

### DrawerInteractable

A component for creating drawers that can be pulled open and closed.

#### Inspector Settings

- **Drawer Transform**: The transform that moves during drawer operation
- **Open Position**: Local position when drawer is fully open
- **Closed Position**: Local position when drawer is fully closed
- **Movement Speed**: Speed of drawer movement
- **On Open**: Event raised when drawer opens
- **On Close**: Event raised when drawer closes

### LeverInteractable

A component for creating levers that can be pulled in different directions.

#### Inspector Settings

- **Lever Transform**: The transform that rotates during lever operation
- **Max Rotation**: Maximum rotation angle in degrees
- **Movement Speed**: Speed of lever movement
- **On Value Changed**: Event raised when lever value changes (0-1)

### JoystickInteractable

A component for creating joysticks that can be moved in 2D space.

#### Inspector Settings

- **Joystick Transform**: The transform that moves during joystick operation
- **Max Distance**: Maximum distance the joystick can move
- **Dead Zone**: Minimum distance required to register movement
- **On Value Changed**: Event raised when joystick value changes (Vector2)

### WheelInteractable

A component for creating wheels that can be rotated.

#### Inspector Settings

- **Wheel Transform**: The transform that rotates during wheel operation
- **Max Rotation**: Maximum rotation angle in degrees
- **Movement Speed**: Speed of wheel rotation
- **On Value Changed**: Event raised when wheel value changes (0-1)

### TurretInteractable

A component for creating turrets that can be aimed in 3D space.

#### Inspector Settings

- **Turret Base**: The base transform that rotates horizontally
- **Turret Head**: The head transform that rotates vertically
- **Max Horizontal Rotation**: Maximum horizontal rotation angle
- **Max Vertical Rotation**: Maximum vertical rotation angle
- **Movement Speed**: Speed of turret movement
- **On Aim Changed**: Event raised when turret aim changes (Vector2)

## Interactors

### InteractorBase

The base class for all interactors. Handles interaction detection, state management, and button mapping.

#### Inspector Settings

**Runtime State (Read-Only):**
- **Current Interactable**: The currently hovered or selected interactable
- **Is Interacting**: Whether this interactor is currently interacting

#### Required Components

- **Hand**: Provides input and pose information

#### Public Properties

- **AttachmentPoint**: Transform where grabbed objects are attached
- **HandIdentifier**: Whether this is the left or right hand
- **Hand**: Reference to the Hand component
- **CurrentInteractable**: The current interactable being interacted with

#### Public Methods

- **ToggleHandModel(bool)**: Shows or hides the hand model
- **OnSelect()**: Called when selecting an interactable
- **OnDeSelect()**: Called when deselecting an interactable

### TriggerInteractor

A trigger-based interactor that detects direct collisions with objects.

#### Inspector Settings

- **Trigger Collider**: The collider used for interaction detection
- **Interaction Range**: Maximum distance for interactions

### RaycastInteractor

A raycast-based interactor that can interact with objects at a distance.

#### Inspector Settings

- **Ray Origin**: Transform that serves as the ray origin
- **Ray Direction**: Direction of the raycast
- **Max Distance**: Maximum raycast distance
- **Layer Mask**: Layers to include in raycast

## Grab Strategies

### GrabStrategy

Abstract base class for implementing different grab strategies.

#### Public Methods

- **Initialize(InteractorBase)**: Sets up the strategy for a specific interactor
- **Grab(Grabable, InteractorBase)**: Performs the grab action
- **UnGrab(Grabable, InteractorBase)**: Performs the ungrab action

### RigidBodyGrabStrategy

Grab strategy for objects with Rigidbody components. Makes the rigidbody kinematic while grabbed.

#### Usage

Automatically selected when a Grabable object has a Rigidbody component.

### TransformGrabStrategy

Grab strategy for objects without Rigidbody components. Maintains exact position when released.

#### Usage

Automatically selected when a Grabable object has no Rigidbody component.

## Editor Tools

### SwitchEditor

Custom editor for the Switch component that provides:

- **Visual Configuration**: Easy setup of rotation and detection axes
- **Gizmo Visualization**: Scene view visualization of switch behavior
- **Event Management**: Simple event setup and testing

[Insert screenshot of SwitchEditor in action]

### JoystickInteractableEditor

Custom editor for the JoystickInteractable component that provides:

- **Movement Range Visualization**: Visual representation of joystick movement area
- **Dead Zone Configuration**: Easy setup of dead zone parameters
- **Testing Tools**: In-editor testing of joystick behavior

[Insert screenshot of JoystickInteractableEditor in action]

### LeverInteractableEditor

Custom editor for the LeverInteractable component that provides:

- **Rotation Range Visualization**: Visual representation of lever movement
- **Speed Configuration**: Easy adjustment of movement parameters
- **Event Testing**: Test lever events in the editor

[Insert screenshot of LeverInteractableEditor in action]

## Best Practices

### Setting Up Interactables

1. **Inherit from InteractableBase**: Always inherit from the base class for custom interactables
2. **Use Events**: Connect inspector events rather than hardcoding behaviors
3. **Validate in OnValidate**: Add validation logic to ensure proper configuration
4. **Use ReadOnly for Runtime State**: Mark runtime-only fields as read-only

### Performance Considerations

1. **Minimize Update Calls**: Only use Update for components that need per-frame updates
2. **Efficient Collision Detection**: Use appropriate collider types and layers
3. **Event Optimization**: Avoid expensive operations in event callbacks
4. **Object Pooling**: Consider object pooling for frequently created/destroyed objects

### VR Best Practices

1. **Comfortable Interaction Distances**: Design interactions for comfortable arm reach
2. **Clear Visual Feedback**: Provide clear visual cues for interactive elements
3. **Haptic Feedback**: Use haptic feedback to enhance interaction feel
4. **Accessibility**: Ensure interactions work for different hand sizes and abilities

### Inspector Organization

1. **Use Headers**: Group related fields with headers
2. **Logical Ordering**: Arrange fields in logical order (configuration, events, runtime state)
3. **Tooltips**: Provide clear, helpful tooltips for all fields
4. **Validation**: Show warnings and errors clearly in the inspector

## Troubleshooting

### Common Issues

**Object Not Grabbable:**
- Ensure the object has a `Grabable` component
- Check that it has a `PoseConstrainter` component
- Verify the object has appropriate colliders

**Events Not Firing:**
- Check that events are properly connected in the inspector
- Verify the interaction state is correct
- Ensure the interactable is properly configured

**Physics Issues:**
- Check layer collision settings
- Verify rigidbody configuration
- Ensure proper collider setup

**Performance Problems:**
- Check for unnecessary Update calls
- Verify event callback efficiency
- Monitor physics object count

### Debug Tools

1. **Scene Gizmos**: Many components provide visual gizmos for debugging
2. **Runtime State Display**: Check runtime state fields in the inspector
3. **Event Logging**: Add Debug.Log calls to event methods
4. **Physics Debugging**: Use Unity's physics debugging tools

### Getting Help

1. **Check the Console**: Look for error messages and warnings
2. **Review Inspector Settings**: Verify all required fields are set
3. **Test in Isolation**: Test components individually before combining
4. **Check Dependencies**: Ensure all required components are present

## Conclusion

The Shababeek Interactions system provides a powerful and flexible framework for creating interactive VR experiences. By following this manual and the established patterns, you can quickly create engaging interactive objects that respond naturally to user input.

For more advanced usage patterns or specific questions, refer to the code examples or contact the development team.

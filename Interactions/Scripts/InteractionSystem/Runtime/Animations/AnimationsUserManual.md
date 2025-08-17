# Shababeek Animations System - User Manual

This manual explains how to use the Shababeek Animations system from the perspective of a Unity developer working in the Inspector. The system provides a sophisticated hand animation framework using Unity's Playable Graph API for VR hand interactions.

## Table of Contents

1. [System Overview](#system-overview)
2. [Core Components](#core-components)
3. [Hand Data System](#hand-data-system)
4. [Pose System](#pose-system)
5. [Constraint System](#constraint-system)
6. [Playable Mixers](#playable-mixers)
7. [Best Practices](#best-practices)
8. [Troubleshooting](#troubleshooting)

## System Overview

The Shababeek Animations system is built around four core concepts:

- **HandPoseController**: Manages hand animations and pose transitions
- **HandData**: Contains pose definitions and hand configuration
- **Pose System**: Defines static and dynamic hand poses
- **Constraint System**: Controls hand positioning and visibility during interactions

The system automatically handles:
- Smooth pose transitions using Unity's Playable Graph
- Real-time finger control for dynamic poses
- Hand positioning and constraints during interactions
- Avatar mask management for finger-specific animations

## Core Components

### HandPoseController

The main component that controls hand pose animations and manages the Playable Graph system.

#### Inspector Settings

**Hand Configuration:**
- **Hand Data**: The HandData asset containing all pose definitions and finger configurations

**Finger Values (Read-Only):**
- **Fingers Array**: Array of finger curl values (0 = extended, 1 = curled) for Thumb, Index, Middle, Ring, Pinky

**Runtime State (Read-Only):**
- **Current Pose Index**: Index of the currently active pose in the HandData

#### Required Components

- **VariableTweener**: Handles smooth transitions between poses
- **Animator**: Provides the animation system connection
- **Hand**: Provides input and pose information

#### Usage

1. Add the `HandPoseController` component to a hand GameObject
2. Assign a `HandData` asset in the inspector
3. The system automatically initializes the Playable Graph and poses
4. Use the `Pose` property to switch between poses
5. Use the `Constrains` property to apply pose constraints

#### Public Properties

- **Pose**: Sets the current pose index
- **Constrains**: Sets pose constraints for the hand
- **CurrentPoseIndex**: Gets or sets the current pose with animation blending
- **HandData**: Gets or sets the HandData asset
- **Graph**: Gets the Playable Graph instance
- **Poses**: Gets the list of all available poses

#### Public Methods

- **Initialize()**: Manually initializes the animation system
- **UpdateGraphVariables()**: Updates the animation graph and finger values

### HandData

A ScriptableObject that contains all hand pose data, avatar masks, and prefab references.

#### Inspector Settings

**Visual Preview:**
- **Preview Sprite**: Preview image for this hand, shown in setup wizard and UI

**Description:**
- **Description**: Text description of this hand data asset for organization purposes

**Hand Prefabs:**
- **Left Hand Prefab**: The left hand prefab with HandPoseController component
- **Right Hand Prefab**: The right hand prefab with HandPoseController component

**Pose Configuration:**
- **Default Pose**: The default pose that will be used when no specific pose is selected
- **Custom Poses**: List of custom poses that can be selected and used by the hand

**Avatar Masks:**
- **Hand Avatar Mask Container**: Container for avatar masks for each finger of the hand

#### Usage

1. Create a new HandData asset via `Create > Shababeek > Interaction System > Hand Data`
2. Configure the default pose and add custom poses
3. Set up avatar masks for each finger
4. Assign hand prefabs for left and right hands
5. Use this asset in HandPoseController components

#### Public Properties

- **DefaultPose**: Gets the default pose data
- **LeftHandPrefab**: Gets the left hand prefab
- **RightHandPrefab**: Gets the right hand prefab
- **Poses**: Returns an array of all poses, including the default pose at index 0
- **Description**: Gets the description of this hand data asset

#### Indexers

- **this[int i]**: Gets the avatar mask for a finger by numeric index (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky)
- **this[FingerName i]**: Gets the avatar mask for a finger by finger name

## Pose System

### PoseData

Represents a single hand pose with animation clips and type information.

#### Inspector Settings

**Animation Clips:**
- **Open**: The animation clip for when the hand is fully open (no buttons are pressed)
- **Closed**: The animation clip for when the hand is fully closed (all buttons are pressed)

**Pose Configuration:**
- **Name**: The name of the pose. If empty, it will be derived from the animation clips
- **Type**: The type of the pose: static (single pose) or dynamic (fingers will follow a value between two poses)

#### Usage

1. Create PoseData instances within HandData assets
2. Assign open and closed animation clips
3. Set the pose type (static or dynamic)
4. Optionally set a custom name

#### Public Properties

- **Name**: Gets or sets the pose name (auto-generated if not set)
- **OpenAnimationClip**: Gets the open hand animation clip
- **ClosedAnimationClip**: Gets the closed hand animation clip
- **Type**: Gets the current pose type

#### Public Methods

- **SetPosNameIfEmpty(string)**: Sets the pose name if it is currently empty
- **SetType(PoseType)**: Sets the type of the pose

#### PoseType Enum

- **Dynamic**: Dynamic pose that allows real-time control between open and closed states
- **Static**: Static pose that plays a predefined animation clip

## Constraint System

### PoseConstrainter

Component that constrains hand poses during interactions, providing pose constraints, transform positioning, and hand visibility control.

#### Inspector Settings

**Constraint Configuration:**
- **Constraint Type**: The type of constraint to apply to hands during interaction
- **Use Smooth Transitions**: Whether to use smooth transitions when positioning hands
- **Transition Speed**: Speed of smooth transitions (units per second)

**Pose Constraints:**
- **Left Pose Constraints**: Constraints for the left hand's pose during interactions
- **Right Pose Constraints**: Constraints for the right hand's pose during interactions

**Hand Positioning:**
- **Left Hand Positioning**: Positioning data for the left hand relative to the interactable
- **Right Hand Positioning**: Positioning data for the right hand relative to the interactable

**Runtime State (Read-Only):**
- **Parent**: The parent transform for this constraint system

#### Required Components

- **HandPoseController**: Manages the hand animations
- **Hand**: Provides hand input and pose information

#### Usage

1. Add the `PoseConstrainter` component to interactable objects
2. Configure the constraint type and positioning data
3. The system automatically applies constraints during interactions
4. Use `ApplyConstraints()` and `RemoveConstraints()` methods

#### Public Properties

- **Parent**: Gets or sets the parent transform for this constraint system
- **LeftPoseConstrains**: Gets the pose constraints for the left hand
- **RightPoseConstrains**: Gets the pose constraints for the right hand
- **ConstraintType**: Gets the type of constraint to apply
- **UseSmoothTransitions**: Gets whether smooth transitions are enabled
- **TransitionSpeed**: Gets the transition speed

#### Public Methods

- **ApplyConstraints(Hand)**: Applies pose constraints and visibility control to the specified hand
- **RemoveConstraints(Hand)**: Removes all pose constraints and restores hand visibility
- **GetTargetHandTransform(HandIdentifier)**: Gets the target position and rotation for the specified hand
- **GetRelativeTargetHandTransform(Transform, HandIdentifier)**: Gets the target hand transform relative to a specified parent
- **GetPoseConstraints(HandIdentifier)**: Gets the pose constraints for the specified hand

### HandConstrainType Enum

Defines the different types of hand constraints that can be applied during interactions.

- **HideHand**: Hides the hand model during interaction
- **FreeHand**: Allows the hand to move freely without any pose constraints
- **Constrained**: Applies specific pose constraints to the hand during interaction

### HandPositioning Struct

Represents the positioning data for a hand relative to an interactable.

- **Position Offset**: Position offset for the hand relative to the interactable
- **Rotation Offset**: Rotation offset for the hand relative to the interactable

## Playable Mixers

### IPose Interface

Interface for pose objects that can control finger animations.

#### Public Members

- **this[int finger]**: Sets the finger curl value for the specified finger index
- **Name**: Gets the name of this pose

### StaticPose

A pose that does not allow access to fingers individually. Plays a predefined animation clip.

#### Usage

Automatically created by the system for poses with `PoseType.Static`.

#### Public Properties

- **Mixer**: Gets the animation clip playable for this static pose
- **Name**: Gets the name of this static pose

### DynamicPose

A pose that allows real-time control of individual finger positions between open and closed states.

#### Usage

Automatically created by the system for poses with `PoseType.Dynamic`.

#### Public Properties

- **Name**: Gets the name of this dynamic pose

#### Internal Properties

- **PoseMixer**: Gets the pose mixer playable that manages all finger layers

### FingerAnimationMixer

Mixes between two different states of a finger (open and closed).

#### Inspector Settings

**Finger Animation (Read-Only):**
- **Weight**: The current weight between open (0) and closed (1) finger states

#### Public Properties

- **Weight**: Gets or sets the weight between open and closed finger states
- **Mixer**: Gets the layer mixer playable that manages the finger animation blending

## Best Practices

### Setting Up Hand Animations

1. **Create HandData Assets**: Always create HandData assets for each hand type
2. **Use Avatar Masks**: Create avatar masks for each finger to ensure proper animation isolation
3. **Pose Organization**: Organize poses logically (default, grab, point, etc.)
4. **Animation Quality**: Use high-quality animation clips for smooth hand movements

### Performance Considerations

1. **Playable Graph Management**: The system automatically manages Playable Graph lifecycle
2. **Avatar Mask Optimization**: Use appropriate avatar masks to limit animation scope
3. **Pose Transitions**: Use smooth transitions for better user experience
4. **Memory Management**: Dispose of unused HandData assets when no longer needed

### VR Best Practices

1. **Natural Hand Movement**: Design poses that feel natural in VR
2. **Smooth Transitions**: Use smooth transitions between poses to avoid jarring movements
3. **Constraint Design**: Design constraints that enhance rather than hinder interaction
4. **Performance Monitoring**: Monitor frame rates during complex hand animations

### Inspector Organization

1. **Use Headers**: Group related fields with headers
2. **Logical Ordering**: Arrange fields in logical order (configuration, constraints, positioning)
3. **Tooltips**: Provide clear, helpful tooltips for all fields
4. **Validation**: Show warnings and errors clearly in the inspector

## Troubleshooting

### Common Issues

**Hand Not Animating:**
- Ensure the HandData asset is assigned
- Check that the Animator component is present
- Verify that VariableTweener component is attached
- Check console for initialization errors

**Pose Constraints Not Working:**
- Verify the PoseConstrainter component is attached
- Check that constraint types are properly configured
- Ensure hand positioning data is set correctly
- Verify that the Hand component is present

**Performance Issues:**
- Check the number of active poses
- Monitor Playable Graph complexity
- Verify avatar mask configurations
- Check for unnecessary Update calls

**Animation Glitches:**
- Ensure animation clips are properly configured
- Check avatar mask settings
- Verify pose type configurations
- Check for conflicting animations

### Debug Tools

1. **Scene Gizmos**: Many components provide visual gizmos for debugging
2. **Runtime State Display**: Check runtime state fields in the inspector
3. **Console Logging**: Look for error messages and warnings
4. **Playable Graph Debugging**: Use Unity's Playable Graph debugging tools

### Getting Help

1. **Check the Console**: Look for error messages and warnings
2. **Review Inspector Settings**: Verify all required fields are set
3. **Test in Isolation**: Test components individually before combining
4. **Check Dependencies**: Ensure all required components are present

## Advanced Usage

### Custom Pose Creation

1. **Create Animation Clips**: Design open and closed hand animations
2. **Set Up Avatar Masks**: Create finger-specific avatar masks
3. **Configure PoseData**: Set up pose data with proper type and clips
4. **Add to HandData**: Include the pose in your HandData asset

### Dynamic Pose Control

1. **Real-time Finger Control**: Use the finger indexers for real-time control
2. **Smooth Transitions**: Leverage the built-in tweening system
3. **Constraint Integration**: Combine with pose constraints for natural movement
4. **Performance Optimization**: Monitor performance during complex animations

### Constraint System Extension

1. **Custom Constraints**: Extend the constraint system for specific needs
2. **Positioning Logic**: Implement custom positioning algorithms
3. **Transition Systems**: Create custom transition systems
4. **Integration**: Integrate with other interaction systems

## Conclusion

The Shababeek Animations system provides a powerful and flexible framework for creating realistic hand animations in VR. By following this manual and the established patterns, you can quickly create engaging hand interactions that respond naturally to user input.

For more advanced usage patterns or specific questions, refer to the code examples or contact the development team.

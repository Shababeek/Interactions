# User Manual

## Overview

The Shababeek Interaction System provides a comprehensive solution for creating VR/AR interactions with a focus on hand presence, pose constraints, and smooth user experiences. This manual covers all aspects of using the system effectively.

## Table of Contents

1. [System Concepts](#system-concepts)
2. [Basic Interactions](#basic-interactions)
3. [Pose Constraints](#pose-constraints)
4. [Feedback Systems](#feedback-systems)
5. [Sequencing](#sequencing)
6. [Variables and Events](#variables-and-events)
7. [Advanced Features](#advanced-features)
8. [Best Practices](#best-practices)

## System Concepts

### **Core Philosophy**

The system is built around several key concepts:

- **Hand-First Design**: Everything is designed around natural hand presence and interactions
- **ScriptableObject-Driven**: Data and configuration are separated from logic
- **Event-Driven**: Components communicate through events rather than direct references
- **Modular Architecture**: Each system can be used independently or together

### **Key Components**

#### **Interactables**
Objects that can be interacted with by hands or other interactors.

#### **Interactors**
Components that detect and initiate interactions (hands, raycasters, etc.).

#### **Pose Constraints**
Systems that control how hands behave during interactions.

#### **Feedback Systems**
Visual, audio, and haptic feedback for user interactions.

#### **Sequences**
Step-by-step processes with conditional logic.

## Basic Interactions

### **Creating Simple Interactables**

#### **Step 1: Create the Object**
1. Create a GameObject in your scene
2. Add a Collider component for interaction detection
3. Add an InteractableBase component

#### **Step 2: Configure Basic Settings**
```csharp
// The InteractableBase component provides:
- Interaction Point: Transform for interaction positioning
- Events: OnSelected, OnDeselected, OnHoverStart, OnHoverEnd, OnActivated
- State Management: IsSelected, CurrentInteractor, CurrentState
```

#### **Step 3: Add Visual Feedback**
1. Add a FeedbackSystem component
2. Configure material, audio, or haptic feedback
3. Connect to interaction events

[insert screenshot of basic interactable setup in Inspector]

### **Using Pre-built Interactables**

The system includes several pre-built interactable types:

#### **VRButton**
Simple button with press detection:
```csharp
// Events available:
- OnButtonDown: Fired when button is pressed
- OnButtonUp: Fired when button is released

// UniRx Observables:
- OnButtonDown: Observable for button press
- OnButtonUp: Observable for button release
```

#### **Switch**
Toggle switch functionality:
```csharp
// Properties:
- IsOn: Current switch state
- OnValueChanged: Event fired when state changes
```

#### **Grabable**
Basic grab and throw functionality:
```csharp
// Features:
- Physics-based grabbing
- Throw mechanics
- Socket integration
```

### **Advanced Interactables**

#### **DrawerInteractable**
Sliding drawer with constraints:
```csharp
// Configuration:
- Local Start/End: Movement range
- Return to Original: Auto-return behavior
- Return Speed: Speed of auto-return
- OnMoved: Event fired during movement
- OnLimitReached: Event fired at limits
```

#### **LeverInteractable**
Rotatable lever with angle constraints:
```csharp
// Configuration:
- Min/Max: Rotation angle limits
- Rotation Axis: Axis of rotation
- Return to Original: Auto-return behavior
- OnLeverChanged: Event fired during rotation
```

#### **JoystickInteractable**
2D joystick with circular constraints:
```csharp
// Configuration:
- Max Radius: Movement radius limit
- Return to Center: Auto-center behavior
- Return Speed: Speed of auto-center
- OnJoystickMoved: Event fired during movement
```

#### **TurretInteractable**
3D rotatable turret with axis constraints:
```csharp
// Configuration:
- Limit X/Y/Z Rotation: Per-axis rotation limits
- Min/Max Angles: Angle limits for each axis
- Return to Original: Auto-return behavior
- OnRotationChanged: Event fired during rotation
```

#### **WheelInteractable**
Continuous rotation wheel:
```csharp
// Configuration:
- OnWheelRotated: Event fired during rotation
- Continuous rotation without limits
```

## Pose Constraints

### **Unified Pose Constraint System**

The Unified Pose Constraint System provides centralized hand pose management.

#### **Hand Constraint Types**

##### **Hide Hand**
Completely hide the real hand during interaction:
```csharp
// Use when:
- You want to show only the object being manipulated
- The real hand would interfere with the interaction
- You're using fake hands for precise positioning
```

##### **Free Hand**
Allow natural hand movement:
```csharp
// Use when:
- You want the hand to move naturally
- No specific pose constraints are needed
- The interaction doesn't require precise hand positioning
```

##### **Constrained**
Apply specific pose constraints:
```csharp
// Use when:
- You need precise hand positioning
- The interaction requires specific hand poses
- You want to show a controlled hand appearance
```

#### **Configuration**

##### **Basic Setup**
1. Add UnifiedPoseConstraintSystem component
2. Set Constraint Type (Hide, Free, Constrained)
3. Configure hand positioning for left and right hands

##### **Smooth Transitions**
```csharp
// Enable smooth transitions:
- Use Smooth Transitions: Enable/disable
- Transition Speed: Speed of hand movement (units/second)
- Higher speeds = faster transitions
- Lower speeds = smoother, more natural movement
```

##### **Hand Positioning**
```csharp
// Position and rotation offsets:
- Position Offset: Vector3 offset from object
- Rotation Offset: Vector3 rotation offset
- Use scene view handles for visual positioning
- Values are stored as local coordinates
```

[insert screenshot of Unified Pose Constraint System configuration]

#### **Pose Selection**

##### **Using HandData**
1. Create HandData asset with poses
2. Assign HandData to Config or Hand components
3. Select poses from dropdown in constraint system

##### **Static vs Dynamic Poses**
```csharp
// Static Poses:
- Fixed hand position and rotation
- No finger constraint controls
- Better performance
- Use for simple, fixed hand positions

// Dynamic Poses:
- Configurable finger constraints
- Real-time finger control
- More complex setup
- Use for detailed hand control
```

#### **Fake Hand Management**

When using Constrained hand type, the system can create fake hands:

```csharp
// Fake hand behavior:
- Created dynamically when needed
- Cached for performance
- Positioned based on constraint settings
- Real hand is hidden during interaction
```

### **Advanced Pose Configuration**

#### **HandData Setup**
1. Create HandData asset
2. Assign left and right hand prefabs
3. Configure poses array with PoseData assets

#### **PoseData Configuration**
```csharp
// PoseData structure:
- PoseName: Descriptive name for the pose
- PoseType: Static or Dynamic
- HandConstraints: Wrist and finger rotations
- FingerConstraints: Individual finger bend values
```

#### **Finger Constraints**
```csharp
// Available fingers:
- Thumb: Thumb finger control
- Index: Index finger control
- Middle: Middle finger control
- Ring: Ring finger control
- Pinky: Pinky finger control

// Constraint values:
- 0.0: Fully extended
- 1.0: Fully bent
- Intermediate values for partial bending
```

## Feedback Systems

### **Unified Feedback System**

The FeedbackSystem provides a unified approach to all types of feedback.

#### **Feedback Types**

##### **Material Feedback**
Visual material changes:
```csharp
// Configuration:
- Target Material: Material to modify
- Property Name: Shader property to change
- Start Value: Initial property value
- End Value: Target property value
- Transition Duration: Time for transition
```

##### **Haptic Feedback**
Controller vibration:
```csharp
// Configuration:
- Haptic Type: Type of haptic feedback
- Intensity: Vibration intensity (0-1)
- Duration: Duration of haptic feedback
- Frequency: Vibration frequency
```

##### **Audio Feedback**
Sound effects:
```csharp
// Configuration:
- Audio Clip: Sound to play
- Volume: Audio volume (0-1)
- Pitch: Audio pitch modification
- Spatial Blend: 2D vs 3D audio
```

##### **Animation Feedback**
Animation-based feedback:
```csharp
// Configuration:
- Target Animator: Animator component
- Trigger Name: Animation trigger
- Animation Clip: Direct animation clip
- Transition Duration: Animation transition time
```

##### **SFX Feedback**
Special effects:
```csharp
// Configuration:
- Particle System: Particle effect to trigger
- Effect Duration: How long effect lasts
- Effect Scale: Size of the effect
- Auto Destroy: Auto-cleanup of effects
```

#### **Configuration**

##### **Adding Feedback**
1. Add FeedbackSystem component
2. Click "Add Feedback" button
3. Select feedback type
4. Configure parameters

##### **Event Binding**
```csharp
// Available events:
- OnSelected: When object is selected
- OnDeselected: When object is deselected
- OnHoverStart: When hovering starts
- OnHoverEnd: When hovering ends
- OnActivated: When object is activated
```

##### **Multiple Feedback**
```csharp
// Combine feedback types:
- Material + Audio: Visual and sound feedback
- Haptic + Animation: Touch and movement feedback
- Multiple Audio: Layered sound effects
- Particle + Audio: Visual and audio effects
```

[insert screenshot of Feedback System with multiple feedback types]

## Sequencing

### **Linear Sequences**

Simple step-by-step processes.

#### **Creating a Sequence**
1. Create Sequence asset (Create → Shababeek → Sequence)
2. Add steps to the sequence
3. Configure step conditions and transitions

#### **Sequence Configuration**
```csharp
// Step properties:
- Step Name: Descriptive name
- Auto Complete: Whether step completes automatically
- Allow Manual Completion: Can be completed manually
- Transitions: Conditions for moving to next step
```

#### **Step Events**
```csharp
// Available events:
- OnStepStarted: When step begins
- OnStepCompleted: When step finishes
- OnStepFailed: When step fails
- OnStepCancelled: When step is cancelled
```

### **Branching Sequences**

Complex sequences with conditional logic.

#### **Creating a Branching Sequence**
1. Create BranchingSequence asset
2. Add BranchingStep assets as sub-assets
3. Configure transitions between steps

#### **Variable Management**
```csharp
// Variable types:
- BoolVariable: True/false conditions
- IntVariable: Integer values
- FloatVariable: Floating-point values
- TextVariable: String values

// Variable operations:
- SetBool/GetBool: Boolean operations
- SetInt/GetInt: Integer operations
- SetFloat/GetFloat: Float operations
```

#### **Transition Conditions**
```csharp
// Condition types:
- Boolean: True/false checks
- Numeric: Greater than, less than, equals
- String: Text comparison
- Custom: User-defined conditions

// Comparison operators:
- Equals: Exact match
- Not Equals: Different values
- Greater Than: Numeric comparison
- Less Than: Numeric comparison
```

[insert screenshot of Branching Sequence Editor]

### **Sequence Behavior**

Runtime component for executing sequences.

#### **Basic Usage**
```csharp
// Add to GameObject:
- Add SequenceBehaviour component
- Assign Sequence asset
- Call StartSequence() to begin
```

#### **Control Methods**
```csharp
// Available methods:
- StartSequence(): Begin sequence execution
- StopSequence(): Stop current sequence
- RestartSequence(): Restart from beginning
- PauseSequence(): Pause execution
- ResumeSequence(): Resume execution
```

## Variables and Events

### **Scriptable Variables**

Observable, serializable variables for data-driven design.

#### **Variable Types**

##### **Basic Types**
```csharp
// BoolVariable: True/false values
- Value: Boolean value
- OnValueChanged: Event when value changes

// IntVariable: Integer values
- Value: Integer value
- Min/Max: Value constraints
- OnValueChanged: Event when value changes

// FloatVariable: Floating-point values
- Value: Float value
- Min/Max: Value constraints
- OnValueChanged: Event when value changes

// TextVariable: String values
- Value: String value
- OnValueChanged: Event when value changes
```

##### **Complex Types**
```csharp
// QuaternionVariable: Rotation values
- Value: Quaternion rotation
- OnValueChanged: Event when value changes

// ColorVariable: Color values
- Value: Color value
- OnValueChanged: Event when value changes

// GameObjectVariable: GameObject references
- Value: GameObject reference
- OnValueChanged: Event when value changes

// TransformVariable: Transform references
- Value: Transform reference
- OnValueChanged: Event when value changes

// Vector3Variable: 3D vector values
- Value: Vector3 value
- OnValueChanged: Event when value changes
```

#### **Variable References**

Type-safe references to ScriptableVariables:
```csharp
// Reference types:
- BoolReference: Reference to BoolVariable
- IntReference: Reference to IntVariable
- FloatReference: Reference to FloatVariable
- TextReference: Reference to TextVariable
- QuaternionReference: Reference to QuaternionVariable
- ColorReference: Reference to ColorVariable
- GameObjectReference: Reference to GameObjectVariable
```

#### **Usage Patterns**

##### **Direct Usage**
```csharp
// Create and use variables:
var healthVariable = ScriptableObject.CreateInstance<FloatVariable>();
healthVariable.Value = 100f;
healthVariable.OnValueChanged.AddListener(OnHealthChanged);
```

##### **UI Binding**
```csharp
// Bind to UI elements:
var uiBinder = GetComponent<VariableToUIBinder>();
uiBinder.BindVariable(healthVariable, healthText);
```

##### **Sequence Integration**
```csharp
// Use in sequences:
branchingSequence.SetBool("isPlayerAlive", true);
var playerHealth = branchingSequence.GetFloat("playerHealth");
```

### **Game Events**

Decoupled event system for loose coupling.

#### **Creating Events**
1. Create GameEvent asset (Create → Shababeek → Game Event)
2. Configure event name and description
3. Use Raise() method to trigger event

#### **Event Listeners**
```csharp
// GameEventListener component:
- Listen to multiple GameEvents
- Configure Unity Events for each event
- Automatic event management
```

#### **Event Usage**
```csharp
// Raising events:
gameEvent.Raise();

// Adding listeners:
gameEvent.AddListener(OnEventRaised);

// Removing listeners:
gameEvent.RemoveListener(OnEventRaised);
```

#### **Object Lifecycle Events**
```csharp
// ObjectLifecycleEvents component:
- OnObjectEnabled: When object is enabled
- OnObjectDisabled: When object is disabled
- GameEvents: Trigger GameEvents on lifecycle
- UnityEvents: Trigger UnityEvents on lifecycle
```

## Advanced Features

### **Custom Interactables**

Creating your own interactable types.

#### **Basic Custom Interactable**
```csharp
public class CustomInteractable : InteractableBase
{
    protected override bool Select()
    {
        // Custom selection logic
        Debug.Log("Custom interactable selected!");
        return true; // Return true if selection was successful
    }
    
    protected override void DeSelected()
    {
        // Custom deselection logic
        Debug.Log("Custom interactable deselected!");
    }
}
```

#### **Constrained Custom Interactable**
```csharp
public class CustomConstrainedInteractable : ConstrainedInteractableBase
{
    protected override void HandleObjectMovement()
    {
        // Implement object-specific movement
        // This is called during interaction
    }
    
    protected override void HandleObjectDeselection()
    {
        // Implement cleanup logic
        // This is called when interaction ends
    }
}
```

### **Custom Feedback**

Creating custom feedback types.

#### **Custom Feedback Data**
```csharp
public class CustomFeedbackData : FeedbackData
{
    [SerializeField] private string customParameter;
    
    public override void ApplyFeedback()
    {
        // Implement custom feedback logic
        Debug.Log($"Applying custom feedback: {customParameter}");
    }
}
```

### **Custom Variables**

Creating custom variable types.

#### **Custom Scriptable Variable**
```csharp
public class CustomVariable : ScriptableVariable<CustomType>
{
    // Custom variable implementation
    // Inherits all base functionality
}
```

### **Socket System**

Advanced object connection system.

#### **Basic Socket**
```csharp
// Socket component:
- Accepts socketable objects
- Provides connection points
- Manages connection states
```

#### **Socketable Objects**
```csharp
// Socketable component:
- Can be connected to sockets
- Provides connection data
- Manages connection behavior
```

#### **Multi-Socket**
```csharp
// MultiSocket component:
- Multiple connection points
- Complex connection logic
- Advanced state management
```

## Best Practices

### **Performance Optimization**

#### **Hand Management**
- Use static poses when possible
- Cache frequently used fake hands
- Optimize hand model complexity
- Enable smooth transitions only when needed

#### **Event Management**
- Remove event listeners when not needed
- Use object pooling for frequently created objects
- Cache component references
- Avoid excessive event firing

#### **Memory Management**
- Reuse ScriptableObject assets
- Clean up fake hands properly
- Dispose of UniRx subscriptions
- Use appropriate variable types

### **User Experience**

#### **Hand Interactions**
- Provide clear visual feedback
- Use appropriate constraint types
- Test with actual hand models
- Ensure smooth transitions

#### **Feedback Design**
- Combine multiple feedback types
- Use appropriate feedback timing
- Consider user comfort and accessibility
- Test feedback in target environment

#### **Sequence Design**
- Keep sequences simple and clear
- Provide clear progress indicators
- Allow for user error recovery
- Test sequences thoroughly

### **Development Workflow**

#### **Asset Organization**
- Use descriptive names for all assets
- Organize assets in logical folders
- Follow consistent naming conventions
- Document complex configurations

#### **Testing**
- Test interactions with actual hand models
- Validate pose constraints in different scenarios
- Test performance with multiple objects
- Verify feedback systems work correctly

#### **Debugging**
- Use provided debug tools
- Check component references
- Validate asset assignments
- Monitor performance metrics

### **Code Quality**

#### **Naming Conventions**
- Private fields: `_fieldName` (underscore prefix)
- Serialized fields: `fieldName` (camelCase)
- Public properties: `PropertyName` (PascalCase)
- Methods: `MethodName` (PascalCase)

#### **Documentation**
- Use XML documentation for public members
- Add tooltips for serialized fields
- Document complex logic
- Provide usage examples

#### **Error Handling**
- Validate inputs and references
- Provide meaningful error messages
- Handle edge cases gracefully
- Log important events for debugging

## Troubleshooting

### **Common Issues**

#### **Hand Not Appearing**
- Check HandData assignment in Config
- Verify hand prefab assignments
- Ensure proper layer settings
- Check Hand component configuration

#### **Pose Constraints Not Working**
- Verify Unified Pose Constraint System is added
- Check Constraint Type setting
- Ensure HandData is properly configured
- Validate pose data assignments

#### **Feedback Not Triggering**
- Verify Feedback System component is added
- Check event assignments
- Ensure feedback parameters are set
- Validate target component references

#### **Performance Issues**
- Use static poses when possible
- Optimize hand model complexity
- Enable smooth transitions only when needed
- Monitor memory usage and cleanup

### **Debug Tools**

#### **Config Editor**
- Validate system configuration
- Check layer assignments
- Verify asset references
- Test automatic setup

#### **Unified Pose Constraint Editor**
- Debug pose constraints
- Test hand positioning
- Validate pose selections
- Check smooth transitions

#### **Feedback System Editor**
- Test feedback configurations
- Validate event connections
- Check parameter settings
- Debug feedback timing

#### **Variable Drawer**
- Verify variable assignments
- Check reference validity
- Test variable connections
- Debug value changes

### **Getting Help**

#### **Documentation**
- Check this User Manual for detailed workflows
- Review Component Reference for API details
- Consult System Overview for architecture
- Read Quick Start Guide for basic setup

#### **Editor Tools**
- Use interactive scene view manipulation
- Leverage real-time preview and debugging
- Utilize visual sequence and constraint editors
- Monitor performance and memory usage

#### **Best Practices**
- Follow established naming conventions
- Use provided debug tools
- Test with actual hand models
- Optimize for performance and UX 
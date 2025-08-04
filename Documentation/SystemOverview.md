# System Overview

## Architecture Overview

The Shababeek Interaction System is built around a modular, ScriptableObject-driven architecture that separates concerns and promotes reusability. The system consists of several interconnected subsystems that work together to provide comprehensive VR/AR interaction capabilities.

## Core Systems

### **1. Scriptable System**
**Location**: `Assets/Shababeek/Interactions/Scripts/Core/Runtime/ScriptableSystem/`

The foundation of the entire system, providing data-driven architecture through ScriptableObjects.

#### **Variables**
- **ScriptableVariable<T>**: Base class for observable, serializable variables
- **Type-Specific Variables**: BoolVariable, IntVariable, FloatVariable, TextVariable, etc.
- **Reference Types**: BoolReference, IntReference, FloatReference, etc.
- **Complex Types**: QuaternionVariable, ColorVariable, GameObjectVariable, TransformVariable, Vector3Variable

#### **Events**
- **GameEvent**: Observable, serializable events for decoupled event-driven logic
- **GameEventListener**: Component for listening to multiple GameEvents
- **ObjectLifecycleEvents**: Component for triggering events on object lifecycle

#### **Utility**
- **VariableToUIBinder**: Binds variables to UI for live updates
- **Attributes**: MinMaxAttribute, ReadOnly for enhanced editor experience

### **2. Interaction System**
**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Interactions/`

The core interaction framework that handles all object-hand interactions.

#### **Interactors**
- **InteractorBase**: Base class for all interactors (hands, raycasters, triggers)
- **TriggerInteractor**: Collider-based interaction detection
- **RaycastInteractor**: Raycast-based interaction detection
- **GestureSetter**: Gesture-based interaction system

#### **Interactables**
- **InteractableBase**: Base class for all interactable objects
- **ConstrainedInteractableBase**: Abstract base for interactables with pose constraints
- **Specific Interactables**:
  - `Grabable`: Basic grab and throw functionality
  - `DrawerInteractable`: Sliding drawer with constraints
  - `LeverInteractable`: Rotatable lever with angle limits
  - `JoystickInteractable`: 2D joystick with circular constraints
  - `TurretInteractable`: 3D rotatable turret with axis constraints
  - `WheelInteractable`: Continuous rotation wheel
  - `VRButton`: Simple button with press detection
  - `Switch`: Toggle switch functionality
  - `Throwable`: Enhanced throwing with physics

#### **Sockets**
- **AbstractSocket**: Base socket system
- **Socket**: Basic socket implementation
- **MultiSocket**: Multiple socket support
- **Socketable**: Objects that can be socketed

### **3. Unified Pose Constraint System**
**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Animations/Constraints/`

Advanced hand pose management with smooth transitions and fake hand support.

#### **Core Components**
- **UnifiedPoseConstraintSystem**: Centralized pose constraint management
- **HandConstrainType**: Enum for hand behavior (Hide, Free, Constrained)
- **HandPositioning**: Struct for position/rotation offsets
- **IPoseConstrainer**: Interface for pose constraint systems

#### **Features**
- **Smooth Transitions**: Configurable lerp-based hand positioning
- **Fake Hand Management**: Dynamic creation and caching of fake hands
- **Vector-Based Positioning**: No Transform dependencies
- **Multi-Hand Support**: Left and right hand configuration

### **4. Hand & Pose System**
**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Animations/`

Comprehensive hand management with pose blending and animation.

#### **Core Components**
- **Hand**: Represents a VR hand, manages input, pose, and constraints
- **HandPoseController**: Controls hand pose blending and animation
- **HandData**: ScriptableObject for hand configuration and poses
- **PoseData**: Individual pose configuration with constraints

#### **Features**
- **Static Poses**: Fixed poses without finger constraints
- **Dynamic Poses**: Poses with finger constraint capabilities
- **Pose Blending**: Smooth transitions between poses
- **Finger Animation**: Individual finger control and animation

### **5. Feedback System**
**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Interactions/Interactables/Feedback/`

Unified feedback system for haptics, audio, and visual cues.

#### **Core Components**
- **FeedbackSystem**: Centralized feedback management
- **FeedbackData**: Base class for all feedback types
- **Specific Feedback Types**:
  - `MaterialFeedbackData`: Visual material changes
  - `HapticFeedbackData`: Controller vibration feedback
  - `AnimationFeedbackData`: Animation-based feedback
  - `AudioFeedbackData`: Sound effect feedback
  - `SFXFeedbackData`: Special effects feedback

#### **Features**
- **Unified Interface**: Single system for all feedback types
- **Event-Driven**: Triggered by interaction events
- **Configurable**: Customizable feedback parameters
- **Performance Optimized**: Efficient feedback processing

### **6. Sequencing System**
**Location**: `Assets/Shababeek/Interactions/Scripts/SequencingSystem/Runtime/`

Step-by-step interaction system with branching logic support.

#### **Core Components**
- **Sequence**: Linear sequence system
- **BranchingSequence**: Visual branching sequence system
- **BranchingStep**: Individual steps in branching sequences
- **StepEventListener**: Listen to sequence events
- **SequenceBehaviour**: Runtime component for sequence execution

#### **Features**
- **Linear Sequences**: Step-by-step processes
- **Branching Logic**: Conditional step execution
- **Variable Management**: Type-safe variable system (Bool, Int, Float)
- **Visual Editor**: Drag-and-drop sequence creation

### **7. Input System**
**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Core/Input/`

Flexible input management with multiple input source support.

#### **Core Components**
- **ButtonObservable**: UniRx-based button state management
- **XRButton**: Simple button with press detection
- **InputManagerBase**: Base class for input management
- **AxisBasedInputManager**: Axis-based input handling
- **NewInputSystemBasedInputManager**: Unity's new input system integration

#### **Features**
- **UniRx Integration**: Reactive programming support
- **Multiple Input Sources**: XR, keyboard, axis-based input
- **Observable Events**: Reactive button state changes
- **Extensible**: Easy to add new input sources

### **8. Camera System**
**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Core/`

VR camera management and positioning.

#### **Core Components**
- **CameraRig**: Manages camera positioning and movement
- **PhysicsHandFollower**: Physics-based hand following

#### **Features**
- **Smooth Following**: Configurable camera following
- **Offset Support**: Customizable camera positioning
- **VR Optimized**: Designed for VR camera requirements

## System Interactions

### **Data Flow**
```
Input System → Interactors → Interactables → Feedback System
     ↓              ↓              ↓              ↓
Hand System → Pose Constraints → Hand Data → UI Updates
     ↓              ↓              ↓              ↓
Scriptable Variables → Game Events → Sequencing → Config
```

### **Component Dependencies**
```
InteractableBase
├── InteractorBase
├── FeedbackSystem
├── UnifiedPoseConstraintSystem (if constrained)
└── HandData (for pose constraints)

ConstrainedInteractableBase
├── UnifiedPoseConstraintSystem (required)
├── Hand (for fake hand creation)
└── HandData (for pose selection)

UnifiedPoseConstraintSystem
├── HandData (for pose configuration)
├── HandPoseController (for pose application)
└── Hand (for constraint application)
```

## Configuration System

### **Config Asset**
**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Core/Config.cs`

Central configuration for system-wide settings.

#### **Key Features**
- **Layer Management**: Automatic layer creation and assignment
- **Hand Data Assignment**: System-wide hand configuration
- **Input Settings**: Global input configuration
- **Editor Integration**: Automatic setup and validation

### **Editor Tools**
- **ConfigEditor**: Custom editor with automatic layer setup
- **VariableDrawer**: Property drawer for ScriptableVariable assets
- **GameEventListenerEditor**: Multi-event listener management
- **UnifiedPoseConstraintSystemEditor**: Interactive pose constraint editing

## Performance Considerations

### **Optimization Strategies**
1. **Cached Fake Hands**: Reuse fake hand instances
2. **Vector-Based Calculations**: Reduce Transform overhead
3. **Event-Driven Architecture**: Efficient event propagation
4. **ScriptableObject Caching**: Reuse configuration assets
5. **Conditional Updates**: Only update when necessary

### **Memory Management**
1. **Object Pooling**: For frequently created objects
2. **Asset References**: Proper ScriptableObject management
3. **Event Cleanup**: Proper event listener disposal
4. **Component Caching**: Cache component references

## Extension Points

### **Custom Interactables**
```csharp
public class CustomInteractable : InteractableBase
{
    protected override bool Select()
    {
        // Custom selection logic
        return true;
    }
}
```

### **Custom Interactors**
```csharp
public class CustomInteractor : InteractorBase
{
    protected override void OnInteractionDetected(InteractableBase interactable)
    {
        // Custom interaction detection
    }
}
```

### **Custom Feedback**
```csharp
public class CustomFeedbackData : FeedbackData
{
    [SerializeField] private string customParameter;
    
    public override void ApplyFeedback()
    {
        // Custom feedback implementation
    }
}
```

### **Custom Variables**
```csharp
public class CustomVariable : ScriptableVariable<CustomType>
{
    // Custom variable implementation
}
```

## Best Practices

### **1. Architecture**
- Use ScriptableObjects for data-driven design
- Implement interfaces for extensibility
- Follow the established naming conventions
- Use events for loose coupling

### **2. Performance**
- Cache frequently accessed components
- Use appropriate constraint types
- Optimize hand model complexity
- Implement proper cleanup

### **3. User Experience**
- Provide smooth transitions
- Use appropriate feedback types
- Test with actual hand models
- Validate pose constraints

### **4. Development**
- Use the provided editor tools
- Follow the documentation guidelines
- Test across different hand types
- Validate system integration

## Troubleshooting Guide

### **Common Issues**
1. **Hand Not Positioning**: Check HandData assignment and pose configuration
2. **Performance Issues**: Verify caching and optimization settings
3. **Editor Not Responding**: Check component references and asset validity
4. **Events Not Firing**: Verify GameEvent assignments and listener connections

### **Debug Tools**
- **Config Editor**: Validate system configuration
- **Unified Pose Constraint Editor**: Debug pose constraints
- **Feedback System Editor**: Test feedback configurations
- **Variable Drawer**: Verify variable assignments

## Future Enhancements

### **Planned Features**
1. **Advanced Sequencing**: More complex branching logic
2. **Enhanced Editor Tools**: More interactive editing capabilities
3. **Performance Monitoring**: Built-in performance analysis tools
4. **Additional Hand Types**: Support for more hand models
5. **Network Support**: Multiplayer interaction capabilities

### **Extension Opportunities**
1. **Custom Input Systems**: Integration with new input methods
2. **Advanced Physics**: Enhanced physics-based interactions
3. **AI Integration**: AI-driven interaction behaviors
4. **Analytics**: Interaction analytics and metrics 
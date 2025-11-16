# Shababeek InteractionSystem Core - User Manual

## Overview

The InteractionSystem Core provides the foundational components for VR hand interactions in Unity. This system handles hand tracking, physics-based hand movement, input management, and camera rig setup. This manual explains how to use these systems from the Unity Inspector and through code.

## Table of Contents

1. [Configuration System](#configuration-system)
2. [Camera Rig System](#camera-rig-system)
3. [Hand System](#hand-system)
4. [Physics Hand Follower](#physics-hand-follower)
5. [Finger Visualizer](#finger-visualizer)
6. [Input System](#input-system)
7. [Best Practices](#best-practices)
8. [Examples](#examples)

---

## Configuration System

The configuration system is the central hub that manages all settings for the interaction system.

### Config Asset

**What it does:** Central configuration asset containing all system settings, input mappings, and layer configurations.

**How to use:**
1. Create a new Config asset: `Right-click → Create → Shababeek → Interactions → Config`
2. Configure the asset in the inspector
3. Assign it to your CameraRig component

**Inspector Settings:**

#### Hand Configuration
- **Hand Data**: ScriptableObject containing hand poses, prefabs, and avatar masks

#### Layer Configuration
- **Left Hand Layer**: Layer for the left hand (prevents self-collision)
- **Right Hand Layer**: Layer for the right hand (prevents self-collision)
- **Interactable Layer**: Layer for objects that can be grabbed
- **Player Layer**: Layer for the player character

#### Input Manager Settings
- **Input Type**: Choose between InputManager (legacy) or InputSystem (modern)
- **Left Hand Actions**: Input action references for the left hand
- **Right Hand Actions**: Input action references for the right hand

#### Old Input Manager Settings
- **Left/Right Hand Input**: Axis names and button mappings for legacy input system
- **Debug Keys**: Keyboard keys for testing without VR hardware

#### Hand Physics Settings
- **Hand Mass**: Mass of hand physics objects (higher = more stable, lower = more responsive)
- **Linear Damping**: How quickly hand movement is reduced
- **Angular Damping**: How quickly hand rotation is reduced

**Code Example:**
```csharp
// Reference the config asset
[SerializeField] private Config interactionConfig;

// Access layer information
int leftHandLayer = interactionConfig.LeftHandLayer;
int rightHandLayer = interactionConfig.RightHandLayer;

// Get input manager
var inputManager = interactionConfig.InputManager;

// Check input type
if (interactionConfig.InputType == InputManagerType.InputSystem)
{
    // Use new input system
}
```

---

## Camera Rig System

The CameraRig component manages the XR environment and coordinates hand interactions.

### CameraRig Component

**What it does:** Main component for managing the XR camera rig, hand initialization, and layer management.

**How to use:**
1. Add the `CameraRig` component to your main camera rig GameObject
2. Assign the Config asset
3. Configure hand pivots and interactor types
4. The component automatically initializes hands and layers on startup

**Inspector Settings:**

#### Core Configuration
- **Config**: Reference to the Config asset
- **Initialize Hands**: Whether to automatically create hands on startup
- **Initialize Layers**: Whether to automatically assign layers on startup

#### Tracking Configuration
- **Tracking Method**: Transform-based (performant) or Physics-based (realistic)

#### Hand Pivots
- **Left Hand Pivot**: Transform defining the left hand position relative to the camera rig
- **Right Hand Pivot**: Transform defining the right hand position relative to the camera rig

#### Interactor Configuration
- **Left Hand Interactor Type**: Trigger (direct collision) or Ray (distance-based)
- **Right Hand Interactor Type**: Trigger (direct collision) or Ray (distance-based)

#### Camera Configuration
- **Offset Object**: Transform for camera position and height offset
- **XR Camera**: The XR camera component (auto-found if not assigned)
- **Camera Height**: Height offset in world units (default: 1 unit)
- **Align Rig Forward**: Whether to align with tracking origin on startup

#### Layer Management
- **Custom Layer Assignments**: Array of specific objects and their layer assignments

**Code Example:**
```csharp
// Get the camera rig component
var cameraRig = GetComponent<CameraRig>();

// Access hand prefabs
var leftHandPrefab = cameraRig.LeftHandPrefab;
var rightHandPrefab = cameraRig.RightHandPrefab;

// Access configuration
var config = cameraRig.Config;
```

**Setup Workflow:**
1. **Create Camera Rig**: Add CameraRig component to main camera GameObject
2. **Assign Config**: Drag your Config asset to the Config field
3. **Set Hand Pivots**: Create empty GameObjects for left and right hand positions
4. **Configure Interactors**: Choose Trigger or Ray interactor types
5. **Set Camera Offset**: Create child GameObject for camera positioning
6. **Test**: The system automatically initializes hands and layers

---

## Hand System

The Hand component provides the interface for accessing hand input and managing hand behavior.

### Hand Component

**What it does:** Manages hand input, pose constraints, and visual representation for a single hand.

**How to use:**
1. Automatically added by CameraRig during initialization
2. Access hand input and manage hand behavior through this component
3. Subscribe to button state changes and access finger values

**Inspector Settings:**
- **Hand**: Whether this is the left or right hand
- **Config**: Reference to the global configuration
- **Hand Model**: GameObject to show/hide (auto-assigned if not set)

**Code Example:**
```csharp
// Get the hand component
var hand = GetComponent<Hand>();

// Subscribe to button state changes
hand.OnTriggerTriggerButtonStateChange.Subscribe(state => 
{
    if (state == VRButtonState.Pressed)
        Debug.Log("Trigger pressed!");
});

hand.OnGripButtonStateChange.Subscribe(state => 
{
    if (state == VRButtonState.Pressed)
        Debug.Log("Grip pressed!");
});

// Get finger curl values
float indexCurl = hand[FingerName.Index];    // 0 = extended, 1 = curled
float thumbCurl = hand[0];                   // Using numeric index
float middleCurl = hand[2];                  // Middle finger

// Toggle hand visibility
hand.ToggleRenderer(false);  // Hide hand
hand.ToggleRenderer(true);   // Show hand

// Apply pose constraints
hand.Constrain(someConstraintSystem);

// Remove constraints
hand.Unconstrain(someConstraintSystem);
```

**Finger Indices:**
- **0**: Thumb
- **1**: Index
- **2**: Middle
- **3**: Ring
- **4**: Pinky

---

## Physics Hand Follower

The PhysicsHandFollower component provides smooth, physics-based hand movement.

### PhysicsHandFollower Component

**What it does:** Makes hands smoothly follow target transforms using physics, with configurable smoothing and deadzones.

**How to use:**
1. Automatically added by CameraRig for physics-based tracking
2. Configure smoothing and deadzone settings in the inspector
3. The component automatically handles hand following and physics

**Inspector Settings:**

#### Target Settings
- **Target**: Transform for the hand to follow (usually hand pivot in camera rig)

#### Velocity Settings
- **Max Velocity**: Maximum hand movement speed (units/second)
- **Max Distance**: Distance before teleporting (prevents hands from getting stuck)
- **Min Distance**: Minimum distance before following (reduces jitter)

#### Smoothing Settings
- **Position Smoothing**: How smooth position changes are (lower = smoother but slower)
- **Rotation Smoothing**: How smooth rotation changes are (lower = smoother but slower)
- **Velocity Damping**: How quickly hand movement decelerates
- **Angular Velocity Damping**: How quickly hand rotation decelerates

#### Advanced Settings
- **Use Deadzone**: Whether to ignore very small movements
- **Position Deadzone**: Minimum position change to respond to
- **Rotation Deadzone**: Minimum rotation change to respond to (degrees)

**Code Example:**
```csharp
// Get the follower component
var follower = GetComponent<PhysicsHandFollower>();

// Set a new target
follower.Target = newTargetTransform;

// The component automatically handles the rest
```

**Performance Tips:**
- **Lower smoothing values** = smoother but slower movement
- **Higher damping values** = more responsive but potentially jittery
- **Use deadzones** to reduce unnecessary physics calculations
- **Adjust max velocity** based on your VR setup and performance requirements

---

## Finger Visualizer

The FingerVisualizer component identifies specific fingers in the hand hierarchy.

### FingerVisualizer Component

**What it does:** Marks GameObjects as representing specific fingers for debugging and identification.

**How to use:**
1. Add this component to finger bone GameObjects
2. Set the finger type in the inspector
3. Other systems can identify which finger a GameObject represents

**Inspector Settings:**
- **Finger**: The specific finger this GameObject represents

**Code Example:**
```csharp
// Get the finger visualizer
var visualizer = GetComponent<FingerVisualizer>();

// Check which finger this is
if (visualizer.Finger == FingerName.Index)
{
    // Handle index finger logic
}

// Set finger type programmatically
visualizer.Finger = FingerName.Thumb;
```

---

## Input System

The input system provides unified access to hand input regardless of the underlying input method.

### Input Manager Types

#### InputManager (Legacy)
- Uses Unity's legacy Input Manager
- Configure axis names and button mappings in Config asset
- Provides debug keyboard keys for testing

#### InputSystem (Modern)
- Uses Unity's new Input System
- Configure InputAction references in Config asset
- More flexible and performant

### Input Access

**Button States:**
```csharp
// Subscribe to button changes
hand.OnTriggerTriggerButtonStateChange.Subscribe(state => 
{
    switch (state)
    {
        case VRButtonState.Pressed:
            Debug.Log("Trigger pressed");
            break;
        case VRButtonState.Released:
            Debug.Log("Trigger released");
            break;
        case VRButtonState.Held:
            Debug.Log("Trigger held");
            break;
    }
});
```

**Finger Values:**
```csharp
// Get individual finger values
float indexCurl = hand[FingerName.Index];
float thumbCurl = hand[FingerName.Thumb];

// Check if finger is curled
bool isIndexCurled = indexCurl > 0.5f;
bool isThumbCurled = thumbCurl > 0.5f;

// Get all finger values
for (int i = 0; i < 5; i++)
{
    float curl = hand[i];
    Debug.Log($"Finger {i}: {curl}");
}
```

---

## Best Practices

### Configuration
1. **Create Config First**: Always create and configure the Config asset before setting up the camera rig
2. **Layer Management**: Use different layers for hands, interactables, and player to prevent unwanted collisions
3. **Input System Choice**: Use InputSystem for new projects, InputManager for legacy compatibility

### Camera Rig Setup
1. **Hand Pivots**: Position hand pivots where you want hands to appear relative to the camera
2. **Interactor Types**: Use Trigger for direct interaction, Ray for distance-based interaction
3. **Physics vs Transform**: Use Physics-based for realistic interactions, Transform-based for performance

### Hand Management
1. **Input Subscriptions**: Always unsubscribe from input events when components are destroyed
2. **Finger Access**: Use the indexer properties for clean finger value access
3. **Pose Constraints**: Apply constraints when interacting with objects, remove when done

### Performance
1. **Smoothing Values**: Start with default smoothing values and adjust based on performance
2. **Deadzones**: Use deadzones to reduce unnecessary physics calculations
3. **Layer Optimization**: Keep hands and interactables on separate layers for efficient collision detection

---

## Examples

### Complete Hand Interaction Setup

```csharp
public class HandInteractionExample : MonoBehaviour
{
    [SerializeField] private Hand leftHand;
    [SerializeField] private Hand rightHand;
    
    private void Start()
    {
        // Subscribe to input events
        leftHand.OnTriggerTriggerButtonStateChange.Subscribe(OnLeftTrigger);
        rightHand.OnTriggerTriggerButtonStateChange.Subscribe(OnRightTrigger);
        
        leftHand.OnGripButtonStateChange.Subscribe(OnLeftGrip);
        rightHand.OnGripButtonStateChange.Subscribe(OnRightGrip);
    }
    
    private void OnLeftTrigger(VRButtonState state)
    {
        if (state == VRButtonState.Pressed)
        {
            Debug.Log("Left trigger pressed");
            // Handle left trigger logic
        }
    }
    
    private void OnRightTrigger(VRButtonState state)
    {
        if (state == VRButtonState.Pressed)
        {
            Debug.Log("Right trigger pressed");
            // Handle right trigger logic
        }
    }
    
    private void OnLeftGrip(VRButtonState state)
    {
        if (state == VRButtonState.Pressed)
        {
            Debug.Log("Left grip pressed");
            // Handle left grip logic
        }
    }
    
    private void OnRightGrip(VRButtonState state)
    {
        if (state == VRButtonState.Pressed)
        {
            Debug.Log("Right grip pressed");
            // Handle right grip logic
        }
    }
    
    private void Update()
    {
        // Check finger states
        float leftIndexCurl = leftHand[FingerName.Index];
        float rightIndexCurl = rightHand[FingerName.Index];
        
        if (leftIndexCurl > 0.8f && rightIndexCurl > 0.8f)
        {
            Debug.Log("Both index fingers are curled");
        }
    }
}
```

### Custom Hand Behavior

```csharp
public class CustomHandBehavior : MonoBehaviour
{
    [SerializeField] private Hand hand;
    [SerializeField] private float curlThreshold = 0.5f;
    
    private void Start()
    {
        // Subscribe to grip changes
        hand.OnGripButtonStateChange.Subscribe(OnGripChanged);
    }
    
    private void OnGripChanged(VRButtonState state)
    {
        if (state == VRButtonState.Pressed)
        {
            // Check finger curl when grip is pressed
            CheckFingerCurl();
        }
    }
    
    private void CheckFingerCurl()
    {
        var fingerNames = new[] { FingerName.Thumb, FingerName.Index, FingerName.Middle, FingerName.Ring, FingerName.Pinky };
        
        foreach (var finger in fingerNames)
        {
            float curl = hand[finger];
            if (curl > curlThreshold)
            {
                Debug.Log($"{finger} is curled ({curl:F2})");
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up subscriptions
        if (hand != null)
        {
            // Note: UniRx handles cleanup automatically in most cases
        }
    }
}
```

### Physics Hand Configuration

```csharp
public class PhysicsHandConfigurator : MonoBehaviour
{
    [SerializeField] private PhysicsHandFollower leftHandFollower;
    [SerializeField] private PhysicsHandFollower rightHandFollower;
    
    [Header("Smoothing Presets")]
    [SerializeField] private float smoothPreset = 0.15f;
    [SerializeField] private float responsivePreset = 0.3f;
    [SerializeField] private float jitteryPreset = 0.5f;
    
    public void ApplySmoothPreset()
    {
        ApplyPreset(smoothPreset);
    }
    
    public void ApplyResponsivePreset()
    {
        ApplyPreset(responsivePreset);
    }
    
    public void ApplyJitteryPreset()
    {
        ApplyPreset(jitteryPreset);
    }
    
    private void ApplyPreset(float smoothing)
    {
        if (leftHandFollower != null)
        {
            // Access private fields through reflection or make them public
            // This is just an example of the concept
        }
        
        if (rightHandFollower != null)
        {
            // Apply same settings to right hand
        }
    }
}
```

---

## Troubleshooting

### Common Issues

1. **Hands not appearing**: Check that HandData is assigned in Config and hand prefabs exist
2. **Input not working**: Verify InputManager is properly configured and InputActions are assigned
3. **Physics hands unstable**: Reduce hand mass and increase damping values
4. **Hands jittery**: Increase smoothing values and enable deadzones
5. **Layer conflicts**: Ensure hands, interactables, and player are on different layers

### Debug Tips

1. **Check Console**: Look for error messages about missing components or references
2. **Verify References**: Ensure all required components and assets are assigned
3. **Test Input**: Use debug keys when testing without VR hardware
4. **Monitor Performance**: Use Unity Profiler to identify performance bottlenecks
5. **Layer Visualization**: Use Unity's Layer Collision Matrix to verify layer settings

---

This manual covers the core functionality of the Shababeek InteractionSystem. For more advanced usage patterns or specific questions, refer to the code examples or contact the development team.

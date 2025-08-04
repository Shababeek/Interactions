# Hand Posing and Constraints Manual

## Overview

The Hand Posing and Constraints system provides sophisticated control over VR hand animations, finger movements, and pose constraints during interactions. This system uses Unity's Playable Graph API for efficient animation blending and supports both static poses (pre-defined animations) and dynamic poses (real-time finger control).

## Core Components

### **HandPoseController**

**Menu Location**: `Component > Shababeek > Animations > Hand Pose Controller`

**What it does**: Controls the pose and finger animations of a VR hand using Unity's Playable Graph system.

**How it works**: Manages hand pose transitions, finger curl values, and constraint application through a sophisticated animation mixing system. It implements the IPoseable interface and works with HandData to manage multiple pose states.

**Inspector Properties**:

- **Hand Data** (HandData)
  - **What it does**: The HandData object containing pose definitions and finger configurations
  - **Default**: None
  - **When to use**: Assign the HandData asset that defines your hand's poses

- **Fingers** (float[]) [Hidden in Inspector]
  - **What it does**: Array of finger curl values (0 = extended, 1 = curled)
  - **Range**: 0.0 - 1.0 for each finger
  - **Default**: [0, 0, 0, 0, 0] (all fingers extended)
  - **When to use**: Automatically managed by the system

- **Current Pose Index** (int) [Hidden in Inspector]
  - **What it does**: Index of the currently active pose
  - **Default**: 0
  - **When to use**: Automatically managed by the system

**Setup Example**:
1. Add the HandPoseController component to your hand GameObject
2. Assign a HandData asset in the Hand Data field
3. Ensure the hand has an Animator component
4. The system will automatically initialize the animation graph

### **HandData**

**Menu Location**: `Assets > Create > Shababeek > Interaction System > Hand Data`

**What it does**: ScriptableObject that contains all hand pose data, avatar masks, and prefab references for a hand.

**How it works**: Defines the available poses, their types (static or dynamic), finger joint mappings, and provides avatar masks for each finger.

**Inspector Properties**:

- **Preview Sprite** (Sprite)
  - **What it does**: Preview image for this hand, shown in setup wizard and UI
  - **Default**: None
  - **When to use**: Set to show a preview of the hand in editors

- **Left Hand Prefab** (HandPoseController) [Hidden in Inspector]
  - **What it does**: Reference to the left hand prefab
  - **Default**: None
  - **When to use**: Automatically set by the system

- **Right Hand Prefab** (HandPoseController) [Hidden in Inspector]
  - **What it does**: Reference to the right hand prefab
  - **Default**: None
  - **When to use**: Automatically set by the system

- **Default Pose** (PoseData) [Hidden in Inspector]
  - **What it does**: The default pose configuration
  - **Default**: Dynamic pose type
  - **When to use**: Automatically configured

- **Poses** (List<PoseData>) [Hidden in Inspector]
  - **What it does**: List of custom poses for this hand
  - **Default**: Empty list
  - **When to use**: Add custom poses through the editor

- **Hand Avatar Mask Container** (HandAvatarMaskContainer) [Hidden in Inspector]
  - **What it does**: Contains avatar masks for each finger
  - **Default**: None
  - **When to use**: Automatically configured

**Setup Example**:
1. Right-click in Project window
2. Select Create > Shababeek > Interaction System > Hand Data
3. Configure the default pose and add custom poses
4. Assign avatar masks for each finger
5. Set the preview sprite

### **PoseData**

**What it does**: Represents a single hand pose, including its name, animation clips, and type.

**Inspector Properties**:

- **Open** (AnimationClip)
  - **What it does**: The animation clip for when the hand is fully open (no buttons pressed)
  - **Default**: None
  - **When to use**: Set to the open hand animation

- **Closed** (AnimationClip)
  - **What it does**: The animation clip for when the hand is fully closed (all buttons pressed)
  - **Default**: None
  - **When to use**: Set to the closed hand animation

- **Name** (string)
  - **What it does**: The name of the pose
  - **Default**: Empty (derived from animation clips)
  - **When to use**: Set a custom name or let it auto-generate

- **Type** (PoseType)
  - **What it does**: The type of the pose (Static or Dynamic)
  - **Options**: Static, Dynamic
  - **Default**: Dynamic
  - **When to use**: Static for single poses, Dynamic for finger control

### **UnifiedPoseConstraintSystem**

**Menu Location**: `Component > Shababeek > Interactions > Unified Pose Constraint System`

**What it does**: Unified system for constraining hand poses during interactions.

**How it works**: Provides pose constraints, transform positioning, and hand visibility control. Movement strategy (object to hand vs hand to object) is handled by individual interactables.

**Inspector Properties**:

- **Constraint Type** (HandConstrainType)
  - **What it does**: The type of constraint to apply to hands during interaction
  - **Options**: HideHand, FreeHand, Constrained
  - **Default**: Constrained
  - **When to use**: 
    - HideHand: Hide the hand during interaction
    - FreeHand: No constraints applied
    - Constrained: Apply pose constraints

- **Use Smooth Transitions** (bool)
  - **What it does**: Whether to use smooth transitions when positioning hands
  - **Default**: false
  - **When to use**: Enable for smooth hand movement

- **Transition Speed** (float)
  - **What it does**: Speed of smooth transitions (units per second)
  - **Range**: 0.1 - 50.0
  - **Default**: 10.0
  - **When to use**: Adjust based on desired transition smoothness

- **Left Pose Constraints** (PoseConstrains)
  - **What it does**: Constraints for the left hand's pose during interactions
  - **Default**: Free constraints
  - **When to use**: Configure finger constraints for left hand

- **Right Pose Constraints** (PoseConstrains)
  - **What it does**: Constraints for the right hand's pose during interactions
  - **Default**: Free constraints
  - **When to use**: Configure finger constraints for right hand

- **Left Hand Positioning** (HandPositioning)
  - **What it does**: Positioning data for the left hand relative to the interactable
  - **Default**: Zero offset
  - **When to use**: Set position and rotation offsets for left hand

- **Right Hand Positioning** (HandPositioning)
  - **What it does**: Positioning data for the right hand relative to the interactable
  - **Default**: Zero offset
  - **When to use**: Set position and rotation offsets for right hand

**Setup Example**:
1. Add the UnifiedPoseConstraintSystem component to your interactable
2. Set the Constraint Type (HideHand, FreeHand, or Constrained)
3. Configure pose constraints for left and right hands
4. Set hand positioning offsets if needed
5. Enable smooth transitions if desired

## Constraint Types

### **HandConstrainType**

- **HideHand**: Completely hides the hand during interaction
- **FreeHand**: No pose constraints applied - hand moves freely
- **Constrained**: Applies pose constraints to limit finger movement

### **PoseConstrains**

**What it does**: Defines constraints for each finger's pose during interactions.

**Inspector Properties**:

- **Target Pose Index** (int)
  - **What it does**: Index of the target pose to apply
  - **Default**: 0
  - **When to use**: Set to the desired pose index

- **Index Finger Limits** (FingerConstraints)
  - **What it does**: Constraints for the index finger
  - **Default**: Free constraints
  - **When to use**: Configure index finger movement limits

- **Middle Finger Limits** (FingerConstraints)
  - **What it does**: Constraints for the middle finger
  - **Default**: Free constraints
  - **When to use**: Configure middle finger movement limits

- **Ring Finger Limits** (FingerConstraints)
  - **What it does**: Constraints for the ring finger
  - **Default**: Free constraints
  - **When to use**: Configure ring finger movement limits

- **Pinky Finger Limits** (FingerConstraints)
  - **What it does**: Constraints for the pinky finger
  - **Default**: Free constraints
  - **When to use**: Configure pinky finger movement limits

- **Thumb Finger Limits** (FingerConstraints)
  - **What it does**: Constraints for the thumb
  - **Default**: Free constraints
  - **When to use**: Configure thumb movement limits

### **FingerConstraints**

**What it does**: Defines constraints for a single finger's pose.

**Inspector Properties**:

- **Locked** (bool)
  - **What it does**: Whether the finger is locked in its current pose
  - **Default**: false
  - **When to use**: Enable to prevent finger movement

- **Min** (float)
  - **What it does**: Minimum value for the finger pose
  - **Range**: 0.0 - 1.0
  - **Default**: 0.0
  - **When to use**: Set the minimum curl value

- **Max** (float)
  - **What it does**: Maximum value for the finger pose
  - **Range**: 0.0 - 1.0
  - **Default**: 1.0
  - **When to use**: Set the maximum curl value

### **HandPositioning**

**What it does**: Represents the positioning data for a hand relative to an interactable.

**Inspector Properties**:

- **Position Offset** (Vector3)
  - **What it does**: Position offset for the hand relative to the interactable
  - **Default**: Zero
  - **When to use**: Set position offset for hand placement

- **Rotation Offset** (Vector3)
  - **What it does**: Rotation offset for the hand relative to the interactable
  - **Default**: Zero
  - **When to use**: Set rotation offset for hand orientation

## Advanced Configuration

### **Pose Types**

**Static Poses**:
- Pre-defined animation clips
- No real-time finger control
- Good for specific hand gestures

**Dynamic Poses**:
- Real-time finger control
- Values range from 0 (extended) to 1 (curled)
- Good for responsive hand interactions

### **Finger Control**

```csharp
// Get a reference to the hand pose controller
HandPoseController poseController = GetComponent<HandPoseController>();

// Set individual finger values (0 = extended, 1 = curled)
poseController[FingerName.Thumb] = 0.5f;    // Half curled thumb
poseController[FingerName.Index] = 0.8f;    // Mostly curled index
poseController[FingerName.Middle] = 0.3f;   // Slightly curled middle
poseController[FingerName.Ring] = 0.0f;     // Extended ring
poseController[FingerName.Pinky] = 0.1f;    // Slightly curled pinky

// Or use numeric indices (0=Thumb, 1=Index, 2=Middle, 3=Ring, 4=Pinky)
poseController[0] = 0.5f; // Thumb
poseController[1] = 0.8f; // Index
```

### **Pose Constraints**

```csharp
// Create finger constraints
FingerConstraints thumbConstraints = new FingerConstraints(false, 0.2f, 0.8f);
FingerConstraints indexConstraints = new FingerConstraints(true, 0.5f, 0.5f);

// Create pose constraints
PoseConstrains poseConstraints = new PoseConstrains
{
    targetPoseIndex = 1,
    thumbFingerLimits = thumbConstraints,
    indexFingerLimits = indexConstraints
};

// Apply constraints to hand
Hand hand = GetComponent<Hand>();
hand.Constrain(poseConstraints);
```

### **Hand Positioning**

```csharp
// Get the unified pose constraint system
UnifiedPoseConstraintSystem constraintSystem = GetComponent<UnifiedPoseConstraintSystem>();

// Get target transform for left hand
var (position, rotation) = constraintSystem.GetTargetHandTransform(HandIdentifier.Left);

// Apply positioning
Transform leftHand = GetComponent<Transform>();
leftHand.position = position;
leftHand.rotation = rotation;
```

## Troubleshooting

### **Common Issues**

**Hand not animating:**
- Check that HandData is assigned to HandPoseController
- Verify Animator component is present
- Ensure animation clips are properly configured
- Check that the Playable Graph is initialized

**Finger constraints not working:**
- Verify UnifiedPoseConstraintSystem is attached
- Check constraint type is set to "Constrained"
- Ensure finger constraint values are within valid ranges
- Test with different constraint configurations

**Hand positioning issues:**
- Check HandPositioning offset values
- Verify transform hierarchy is correct
- Test with different positioning configurations
- Ensure smooth transitions are properly configured

**Performance issues:**
- Limit the number of active pose controllers
- Use appropriate constraint types
- Optimize animation clip complexity
- Monitor Playable Graph performance

### **VR-Specific Issues**

**Hand tracking problems:**
- Check HandPoseController initialization
- Verify HandData configuration
- Test with different VR devices
- Ensure proper VR input system setup

**Animation blending issues:**
- Check pose transition configurations
- Verify animation clip compatibility
- Test with different pose types
- Monitor animation graph performance

## Code Integration

While this manual focuses on Unity Editor usage, here's how to work with hand posing in code:

```csharp
// Get a reference to the hand pose controller
HandPoseController poseController = GetComponent<HandPoseController>();

// Set finger values
poseController[FingerName.Thumb] = 0.5f;
poseController[FingerName.Index] = 0.8f;

// Change pose
poseController.Pose = 1; // Set to pose index 1

// Apply constraints
PoseConstrains constraints = new PoseConstrains();
poseController.Constrains = constraints;

// Get hand data
HandData handData = poseController.HandData;
List<IPose> poses = poseController.Poses;
```

### **Custom Pose Controller**

```csharp
using UnityEngine;
using Shababeek.Interactions.Animations;

public class CustomPoseController : MonoBehaviour, IPoseable
{
    [SerializeField] private float[] fingerValues = new float[5];
    
    public float this[int index]
    {
        get => fingerValues[index];
        set => fingerValues[index] = Mathf.Clamp01(value);
    }
    
    public float this[FingerName index]
    {
        get => this[(int)index];
        set => this[(int)index] = value;
    }
    
    public int Pose { set { /* Custom pose logic */ } }
    public PoseConstrains Constrains { set { /* Custom constraint logic */ } }
}
```

### **Custom Constraint System**

```csharp
using UnityEngine;
using Shababeek.Interactions.Animations.Constraints;

public class CustomConstraintSystem : MonoBehaviour, IPoseConstrainer
{
    [SerializeField] private PoseConstrains leftConstraints;
    [SerializeField] private PoseConstrains rightConstraints;
    
    public PoseConstrains LeftPoseConstrains => leftConstraints;
    public PoseConstrains RightPoseConstrains => rightConstraints;
    
    public Transform LeftHandTransform { get; set; }
    public Transform RightHandTransform { get; set; }
    public Transform PivotParent => transform;
    public bool HasChanged => transform.hasChanged;
    
    public void ApplyConstraints(Hand hand)
    {
        // Custom constraint application logic
        Debug.Log($"Applying constraints to {hand.name}");
    }
}
``` 
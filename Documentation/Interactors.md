# Interactors Manual

## Overview

Interactors are the components that detect and interact with interactable objects. They represent the user's input method - whether that's hands, raycasters, or other input devices. The Shababeek Interaction System provides two main types of interactors to handle different interaction scenarios.

## Common Requirements

All interactors require these components to function properly:

- **HandPoseController** (or anything that inhert IPosable) - controls the hand pose
- **Hand** - Required component for all interactors
- **Collider** - For physical interaction detection (for TriggerInteractor)
- **LineRenderer** - For visualization of raycast (for RayInteractor)


## Common Inspector Properties

All interactors share these base properties in the Inspector:

### **Current Interactable** (InteractableBase) [Read-only]
- **What it does**: The interactable object currently being interacted with
- **Default**: None
- **When to use**: Read-only property to check current interaction

### **Is Interacting** (bool) [Read-only]
- **What it does**: Whether this interactor is currently interacting with an object
- **Default**: false
- **When to use**: Read-only property to check interaction state

### **Attachment Point** (Transform) [Read-only]
- **What it does**: The attachment point transform for objects held by this interactor
- **Default**: Automatically created child GameObject
- **When to use**: Used by Grabable objects to attach to the interactor

### **Hand Identifier** (HandIdentifier) [Read-only]
- **What it does**: The hand identifier (left or right) for this interactor
- **Default**: Based on the Hand component
- **When to use**: Read-only property to identify which hand this represents

### **Hand** (Hand) [Read-only]
- **What it does**: The Hand component associated with this interactor
- **Default**: Automatically assigned
- **When to use**: Read-only property to access hand functionality

## Interactor Types

### **TriggerInteractor**

**Menu Location**: `Component > Shababeek > Interactors > Trigger Interactor`

**What it does**: Uses Unity's trigger collider system to detect interactions with interactable objects.

**How it works**: Detects when objects enter and exit the trigger area, automatically managing hover states and interaction transitions. Designed to work with colliders and does not require direct input from the user.

**Inspector Properties**:

- **Current Collider** (Collider) [Read-only]
  - **What it does**: The collider currently being interacted with
  - **Default**: None
  - **When to use**: Read-only property for debugging

**Setup Example**:
1. Add a Collider component to your hand GameObject
2. Set the Collider to "Is Trigger"
3. Add the TriggerInteractor component
4. The interactor will automatically detect interactable objects
5. Test trigger-based interactions

**Common Uses**:
- Hand-based interactions
- Proximity-based interactions
- Automatic hover detection

### **RaycastInteractor**

**Menu Location**: `Component > Shababeek > Interactors > Raycast Interactor`

**What it does**: Uses raycasting to interact with objects at a distance, useful for UI interactions or when hands can't reach objects.

**How it works**: Shoots a ray from the interactor's position and direction, detecting the first object it hits. Can interact with objects through the raycast without physical contact. Includes a LineRenderer for visualization.

**Inspector Properties**:

- **Max Raycast Distance** (float)
  - **What it does**: Maximum distance the raycast can reach
  - **Range**: 0.1 - 100.0
  - **Default**: 10.0
  - **When to use**: Set based on your interaction needs

- **Raycast Layer Mask** (LayerMask)
  - **What it does**: Layers that the raycast can hit
  - **Default**: Everything (-1)
  - **When to use**: Set to limit which objects can be raycast against

- **Raycast Origin** (Transform)
  - **What it does**: Point where the ray starts
  - **Default**: None (uses interactor's transform)
  - **When to use**: Set to a specific point like a controller tip

- **Line Renderer** (LineRenderer)
  - **What it does**: The LineRenderer component for visual feedback
  - **Default**: None (auto-created if not assigned)
  - **When to use**: Assign a custom LineRenderer or let it auto-create

- **Line Material** (Material)
  - **What it does**: Material for the visual ray
  - **Default**: None
  - **When to use**: Set to make the ray visible and styled

- **Line Color** (Color)
  - **What it does**: Color of the line renderer
  - **Default**: White
  - **When to use**: Set the visual color of the ray

- **Line Width** (float)
  - **What it does**: Width of the line renderer
  - **Range**: 0.001 - 0.1
  - **Default**: 0.01
  - **When to use**: Adjust for better visibility

- **Show Line Renderer** (bool)
  - **What it does**: Whether to display a visual ray in the scene
  - **Default**: true
  - **When to use**: Enable for debugging or visual feedback

- **Hit Point** (Vector3) [Read-only]
  - **What it does**: The point where the raycast hit
  - **Default**: Zero
  - **When to use**: Read-only property for debugging

- **Is Hitting** (bool) [Read-only]
  - **What it does**: Whether the raycast is currently hitting an object
  - **Default**: false
  - **When to use**: Read-only property for debugging

**Setup Example**:
1. Add the RaycastInteractor component to your controller or camera
2. Set the Raycast Origin to the controller tip or camera
3. Configure raycast distance and layer mask
4. Set up line renderer settings for visual feedback
5. Test raycast interactions

**Common Uses**:
- UI interactions
- Distant object selection
- Pointer-based interactions
- VR controller interactions

## Advanced Configuration

### **Hand Component Integration**

All interactors require a Hand component:

1. **Hand Component**:
   - Automatically assigned when adding an interactor
   - Provides hand identifier (Left/Right)
   - Manages input button states

2. **Input Button Integration**:
   - Interactors automatically subscribe to hand button events
   - Uses the interactable's SelectionButton setting
   - Supports both Grip and Trigger buttons
3- HandPoseController

### **Layer Management**


Set up proper layer organization:

1. **Create Interaction Layers**:
   - Interactable layer for objects
   - UI layer for interface elements
   - Hand layer for hand models

2. **Configure Layer Masks**:
   - Set appropriate layer masks on raycast interactors
   - Ensure proper interaction detection

3. **Test Layer Interactions**:
   - Verify interactors can detect intended objects
   - Check that unwanted interactions are prevented

### **Performance Optimization**

Optimize interactor performance:

1. **Trigger Interactor Optimization**:
   - Use appropriate collider sizes
   - Limit the number of trigger colliders
   - Use distance checking for better performance

2. **Raycast Interactor Optimization**:
   - Set reasonable max raycast distances
   - Use appropriate layer masks
   - Limit raycast frequency if needed

3. **Efficient Input Handling**:
   - Interactors automatically handle input through Hand component
   - No manual input management required

## Troubleshooting

### **Common Issues**

**Interactor not detecting objects:**
- Check that objects have InteractableBase components
- Verify layer masks and layer settings
- Ensure proper collider setup for TriggerInteractor
- Check that Hand component is present

**Input not responding:**
- Verify Hand component is properly configured
- Check that interactables have correct SelectionButton settings
- Test with different input devices
- Ensure proper VR setup

**Performance issues:**
- Reduce raycast distance for RaycastInteractor
- Limit the number of trigger colliders
- Use appropriate layer masks
- Optimize collider sizes

### **VR-Specific Issues**

**Hand tracking problems:**
- Check Hand component configuration
- Verify hand identifier settings
- Test with different VR devices
- Ensure proper VR input system setup

**Controller input issues:**
- Verify Hand component button mappings
- Check interactable SelectionButton settings
- Test with different controllers
- Update VR input system if needed

### **Visual Feedback Issues**

**Line renderer not visible:**
- Check Show Line Renderer setting
- Verify Line Material assignment
- Test Line Color and Width settings
- Ensure LineRenderer component is enabled

**Trigger interactions not working:**
- Verify collider is set to "Is Trigger"
- Check collider size and position
- Test with different interactable types
- Ensure proper layer setup

## Code Integration

While this manual focuses on Unity Editor usage, here's how to work with interactors in code:

```csharp
// Get a reference to an interactor
InteractorBase interactor = GetComponent<InteractorBase>();

// Check if it's currently interacting
if (interactor.IsInteracting)
{
    Debug.Log("Currently interacting!");
}

// Get the current interactable
InteractableBase currentInteractable = interactor.currentInteractable;

// Get hand information
HandIdentifier handId = interactor.HandIdentifier;
Hand hand = interactor.Hand;

// Toggle hand model visibility
interactor.ToggleHandModel(false); // Hide hand
interactor.ToggleHandModel(true);  // Show hand
```

For specific interactor types:

```csharp
// For RaycastInteractor
RaycastInteractor raycastInteractor = GetComponent<RaycastInteractor>();
raycastInteractor.SetMaxDistance(15.0f);
raycastInteractor.SetLineColor(Color.red);
raycastInteractor.SetLineRendererVisibility(true);

// For TriggerInteractor
TriggerInteractor triggerInteractor = GetComponent<TriggerInteractor>();
// TriggerInteractor has no public properties to configure
```

### **Custom Interactor Creation**

```csharp
using UnityEngine;
using Shababeek.Interactions;

public class CustomInteractor : InteractorBase
{
    [SerializeField] private float customDetectionRange = 5f;
    
    protected override void OnHoverStart()
    {
        // Custom hover start logic
        Debug.Log("Custom hover started!");
        base.OnHoverStart();
    }
    
    protected override void OnHoverEnd()
    {
        // Custom hover end logic
        Debug.Log("Custom hover ended!");
        base.OnHoverEnd();
    }
    
    // Override OnSelect and OnDeSelect if needed
    public override void OnSelect()
    {
        Debug.Log("Custom select!");
        base.OnSelect();
    }
    
    public override void OnDeSelect()
    {
        Debug.Log("Custom deselect!");
        base.OnDeSelect();
    }
}
``` 
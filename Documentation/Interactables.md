# Interactables Manual

## Overview

All interactable objects in the Shababeek Interaction System inherit from `InteractableBase`. This base class provides the core interaction functionality that allows objects to respond to hand interactions, raycasting, and other input methods.

## Common Requirements

All interactables require these components to function properly:

- **Collider** - For physical interaction detection
- **InteractableBase** (or derived class) - Core interaction logic
- **Rigidbody** - For physics-based interactions (optional for some types)

## Common Inspector Properties

All interactables share these base properties in the Inspector:

### **Interaction Hand** (InteractionHand)
- **What it does**: Specifies which hands can interact with this object (Left, Right, or Both)
- **Default**: Both 
- **When to use**: Set to limit interaction to specific hands
- **Example**: Set to Left only for left-handed tools

### **Selection Button** (XRButton)
- **What it does**: The button that triggers selection of this interactable (Grip or Trigger)
- **Default**: Grip
- **When to use**: Choose based on the object's interaction style
- **Example**: Use Trigger for precise interactions, Grip for grab interactions

### **On Selected** (InteractorUnityEvent)
- **What it does**: Event raised when this interactable is selected by an interactor
- **Default**: Empty
- **Common uses**: Play selection sounds, change materials, show UI hints

### **On Deselected** (InteractorUnityEvent)
- **What it does**: Event raised when this interactable is deselected by an interactor
- **Default**: Empty
- **Common uses**: Restore original materials, hide UI hints, stop effects

### **On Hover Start** (InteractorUnityEvent)
- **What it does**: Event raised when an interactor starts hovering over this interactable
- **Default**: Empty
- **Common uses**: Show tooltips, highlight the object, play hover sounds

### **On Hover End** (InteractorUnityEvent)
- **What it does**: Event raised when an interactor stops hovering over this interactable
- **Default**: Empty
- **Common uses**: Hide tooltips, remove highlights, stop hover effects

### **On Activated** (InteractorUnityEvent)
- **What it does**: Event raised when the alternative button is pressed while the object is being interacted with
- **Default**: Empty
- **Common uses**: Perform the main action, play activation sounds, trigger animations

**Read-only Properties:**
- **Is Selected** (bool) - Whether this object is currently selected
- **Current Interactor** (InteractorBase) - The interactor currently interacting with this object
- **Current State** (InteractionState) - The current interaction state

## Interactable Types

### **Grabable**

**Menu Location**: `Component > Shababeek > Interactables > Grabable`
**What it does**: Allows objects to be picked up and held by hands. The object follows the hand's movement and rotation.
**How it works**: When selected, the object becomes a child of the hand and follows its transform. When deselected, it can either return to its original position or be thrown if the Throwable Component is added.
**Inspector Properties**:

- **Hide Hand** (bool)[deprecated use UnifedPsoeConstraints insteead]
  - **What it does**: Whether to hide the hand model when this object is grabbed
  - **Default**: false
  - **When to use**: Enable for objects that should hide the hand during interaction

- **Tweener** (VariableTweener)
  - **What it does**: The tweener component used for smooth grab animations
  - **Default**: Creates a new Tweeener when the game starts
  - **When to use**: Assign for smooth grab transitions

**Setup Example**:
1. Add a Collider to your object
2. Add a Rigidbody (if you want physics)
3. Add the Grabable component
4. Add a FeedbackSystem component for effects (sounds, particles, etc.)
5. Configure the hide hand and tweener settings

### **Throwable**

**Menu Location**: `Component > Shababeek > Interactables > Throwable`
**What it does**: Designed to work with Grabable, and enable better throwing. Objects can be picked up and thrown with realistic physics.
**How it works**: Objects follow the hand while held, and when released, they maintain the hand's velocity and can be affected by gravity and air resistance.
**Inspector Properties**:

- **Velocity Sample Count** (int)
  - **What it does**: Number of velocity samples to average for throw calculation
  - **Range**: 1 - 50
  - **Default**: 10
  - **When to use**: Higher values provide smoother throws but use more memory

- **Throw Multiplier** (float)
  - **What it does**: Multiplier applied to the calculated throw velocity
  - **Range**: 0.1 - 5.0
  - **Default**: 1.0
  - **When to use**: Values > 1 increase throw force, < 1 decrease it

- **Enable Angular Velocity** (bool)
  - **What it does**: Whether to apply angular velocity (rotation) to the thrown object
  - **Default**: true
  - **When to use**: Enable for objects that should spin when thrown

- **Angular Velocity Multiplier** (float)
  - **What it does**: Multiplier applied to the calculated angular velocity
  - **Range**: 0.0 - 5.0
  - **Default**: 1.0
  - **When to use**: Controls how much the object spins when thrown

- **On Throw End** (Vector3UnityEvent)
  - **What it does**: Event raised when the object is thrown, providing the final throw velocity
  - **Default**: Empty
  - **Common uses**: Play throw sounds, trigger effects

**Setup Example**:
1. Add a Collider (SphereCollider works well for balls)
2. Add a Rigidbody with appropriate mass
3. Add the Grabable component first
4. Add the Throwable component
5. Configure physics properties based on your object

### **LeverInteractable**

**Menu Location**: `Component > Shababeek > Interactables > Lever Interactable`

**What it does**: Creates a lever that can be pulled to different positions.

**How it works**: The lever can be pulled to different angles, triggering events at specific thresholds. It can be spring-loaded to return to a default position.

**Inspector Properties**:

- **Interactable Object** (Transform)
  - **What it does**: The visual part of the lever object that rotates
  - **Default**: None
  - **When to use**: Set to the lever's visual mesh or child object

- **Min** (float)
  - **What it does**: Minimum rotation angle in degrees
  - **Range**: -180.0 - -1.0
  - **Default**: -40.0
  - **When to use**: Set the lower limit of lever movement

- **Max** (float)
  - **What it does**: Maximum rotation angle in degrees
  - **Range**: 1.0 - 180.0
  - **Default**: 40.0
  - **When to use**: Set the upper limit of lever movement

- **Rotation Axis** (RotationAxis)
  - **What it does**: The axis around which the lever rotates
  - **Options**: Right, Up, Forward
  - **Default**: Right
  - **When to use**: Set based on your lever's orientation

- **Return To Original** (bool)
  - **What it does**: Whether the lever returns to its original position when released
  - **Default**: false
  - **When to use**: Enable for spring-loaded levers

- **On Lever Changed** (FloatUnityEvent)
  - **What it does**: Events triggered when the player moves the lever, passing the value between 0-1 of how much it's pulled
  - **Default**: Empty
  - **Common uses**: Trigger different actions at specific lever positions

**Setup Example**:
1. Create a lever visual object as a child
2. Add a Collider to the lever handle
3. Add the LeverInteractable component
4. Set the Interactable Object to the visual object
5. Configure angle limits and rotation axis
6. Set up lever change events

**Use provided Example**:
- `GameObject -> Shababeek -> MakeLever` when no object is selected will create a sample Lever
- `GameObject -> Shababeek -> MakeLever` when an object is selected will convert the object into a lever, placing the selected object inside the rotating part of the lever

### **DrawerInteractable**

**Menu Location**: `Component > Shababeek > Interactables > Drawer Interactable`

**What it does**: Allows the player to pull or push a drawer along a single axis, simulating realistic sliding motion.

**How it works**: The drawer can be grabbed and moved between a closed and open position, with optional limits and events for different states.

**Inspector Properties**:

- **Interactable Object** (Transform)
  - **What it does**: The visual part of the drawer that slides
  - **Default**: None
  - **When to use**: Set to the drawer's mesh or child object that should move

- **Local Start** (Vector3)
  - **What it does**: The minimum (closed) position in local coordinates
  - **Default**: (0, 0, 0)
  - **When to use**: Set to the fully closed position value

- **Local End** (Vector3)
  - **What it does**: The maximum (open) position in local coordinates
  - **Default**: (0, 0, 1)
  - **When to use**: Set to the fully open position value

- **Return To Original** (bool)
  - **What it does**: Whether the drawer returns to its original position when released
  - **Default**: false
  - **When to use**: Enable for self-closing drawers, or sliders

- **Return Speed** (float)
  - **What it does**: Speed at which the drawer returns to original position
  - **Range**: 0.1 - 20.0
  - **Default**: 5.0
  - **When to use**: Adjust for faster or slower return animation

- **On Moved** (UnityEvent)
  - **What it does**: Events triggered as the drawer moves
  - **Default**: Empty
  - **Common uses**: Play sounds, trigger animations, update UI

- **On Limit Reached** (UnityEvent)
  - **What it does**: Events triggered when the drawer reaches open/closed limits
  - **Default**: Empty
  - **Common uses**: Play limit sounds, trigger effects

- **On Value Changed** (FloatUnityEvent)
  - **What it does**: Events triggered as the drawer moves, passing a value between 0 (closed) and 1 (open)
  - **Default**: Empty
  - **Common uses**: Update UI, trigger progressive effects

**Setup Example**:
1. Create a drawer visual object as a child of your main object
2. Add a Collider to the drawer handle or front
3. Add the DrawerInteractable component to the parent object
4. Assign the Interactable Object to the sliding drawer mesh
5. Set Local Start/Local End to match your model
6. Configure events for open/close as needed

**Use provided Example**:
- `GameObject -> Shababeek -> MakeDrawer` when no object is selected will create a sample Drawer
- `GameObject -> Shababeek -> MakeDrawer` when an object is selected will convert the object into a drawer, placing the selected object as the sliding part

### **VRButton**

**Menu Location**: `Component > Shababeek > Interactables > VR Button`

**What it does**: Creates pressable buttons that can trigger actions when pressed.

**How it works**: Buttons can be pressed down and will either stay pressed or spring back up. They can trigger different events for press, release, and hold states.

**Inspector Properties**:

- **Button** (Transform)
  - **What it does**: The transform of the button visual element that moves during press
  - **Default**: None
  - **When to use**: Set to the button's visual mesh or child object

- **Normal Position** (Vector3)
  - **What it does**: The normal (unpressed) position of the button
  - **Default**: (0, 0.5, 0)
  - **When to use**: Set the rest position of the button

- **Pressed Position** (Vector3)
  - **What it does**: The pressed position of the button (how far it moves when pressed)
  - **Default**: (0, 0.2, 0)
  - **When to use**: Set how far the button moves when pressed

- **Press Speed** (float)
  - **What it does**: Speed of the button press animation
  - **Range**: 1.0 - 50.0
  - **Default**: 10.0
  - **When to use**: Adjust for faster or slower press animation

- **Cool Down Time** (float)
  - **What it does**: Cooldown time between button clicks to prevent rapid-fire activation
  - **Range**: 0.0 - 2.0
  - **Default**: 0.2
  - **When to use**: Increase to prevent accidental rapid clicking

- **On Click** (UnityEvent)
  - **What it does**: Events triggered when button is clicked
  - **Default**: Empty
  - **Common uses**: Play click sounds, trigger actions, show feedback

- **On Button Down** (UnityEvent)
  - **What it does**: Events triggered when button is pressed down
  - **Default**: Empty
  - **Common uses**: Play press sounds, trigger actions

- **On Button Up** (UnityEvent)
  - **What it does**: Events triggered when button is released
  - **Default**: Empty
  - **Common uses**: Play release sounds, complete actions

**Setup Example**:
1. Create a button visual object as a child
2. Add a Collider to the button surface
3. Add the VRButton component
4. Set the Button Transform to the visual object
5. Configure press positions and speed
6. Set up click, button down, and button up events

### **Switch**

**Menu Location**: `Component > Shababeek > Interactables > Switch`

**What it does**: Creates toggleable switches that can be turned on/off by interaction.

**How it works**: Each interaction toggles the switch state. The switch can trigger different events for on/off states and can be locked to prevent interaction.

**Inspector Properties**:

- **Switch Body** (Transform)
  - **What it does**: The transform of the switch body that rotates during interaction
  - **Default**: None
  - **When to use**: Set to the switch's visual mesh or child object

- **Up Rotation** (float)
  - **What it does**: Rotation angle in degrees for the up position
  - **Range**: -180.0 - 180.0
  - **Default**: 20.0
  - **When to use**: Set the rotation for the "on" position

- **Down Rotation** (float)
  - **What it does**: Rotation angle in degrees for the down position
  - **Range**: -180.0 - 180.0
  - **Default**: -20.0
  - **When to use**: Set the rotation for the "off" position

- **Rotate Speed** (float)
  - **What it does**: Speed of the rotation animation
  - **Range**: 1.0 - 50.0
  - **Default**: 10.0
  - **When to use**: Adjust for faster or slower rotation

- **On Up** (UnityEvent)
  - **What it does**: Events triggered when the switch is moved to the up position
  - **Default**: Empty
  - **Common uses**: Turn on lights, start machines, play activation sounds

- **On Down** (UnityEvent)
  - **What it does**: Events triggered when the switch is moved to the down position
  - **Default**: Empty
  - **Common uses**: Turn off lights, stop machines, play deactivation sounds

- **On Hold** (UnityEvent)
  - **What it does**: Events triggered when the switch is held in a position
  - **Default**: Empty
  - **Common uses**: Continuous actions, sustained effects

**Setup Example**:
1. Add a Collider to your switch object
2. Add the Switch component
3. Set up visual feedback (materials, animations) using the UnityEvents
4. Configure the rotation angles and speed
5. Test the toggle behavior in play mode

### **SpawningInteractable**

**Menu Location**: `Component > Shababeek > Interactables > Spawning Interactable`

**What it does**: Creates an interactable that spawns other interactables when interacted with.

**How it works**: When selected, it instantiates a prefab and automatically transfers the interaction to the newly spawned object.

**Inspector Properties**:

- **Prefab** (Grabable)
  - **What it does**: The Grabable prefab to spawn when this interactable is selected
  - **Default**: None
  - **When to use**: Set to the object you want to spawn
  - **Example**: Set to a Grabable prefab like a tool or weapon

**Setup Example**:
1. Create a Grabable prefab that you want to spawn
2. Add the SpawningInteractable component to a GameObject
3. Assign the Grabable prefab to the Prefab field
4. Add a Collider to the spawning object
5. Test the spawning behavior in play mode

**Common Uses**:
- Weapon racks that spawn weapons
- Tool boxes that spawn tools
- Item dispensers
- Ammunition racks

### **TurretInteractable**

**Menu Location**: `Component > Shababeek > Interactables > Turret Interactable`

**What it does**: Allows for constrained rotation around multiple axes, like turrets, dials, or security cameras.

**How it works**: Provides smooth rotation control with configurable limits for X, Y, and Z axes, with optional return-to-original behavior.

**Inspector Properties**:

- **Limit X Rotation** (bool)
  - **What it does**: Whether to limit rotation around the X axis (pitch)
  - **Default**: true
  - **When to use**: Enable to constrain pitch movement

- **Min X Angle** (float)
  - **What it does**: Minimum allowed X rotation angle in degrees
  - **Range**: -180.0 - 180.0
  - **Default**: -90.0
  - **When to use**: Set the lower pitch limit

- **Max X Angle** (float)
  - **What it does**: Maximum allowed X rotation angle in degrees
  - **Range**: -180.0 - 180.0
  - **Default**: 90.0
  - **When to use**: Set the upper pitch limit

- **Limit Y Rotation** (bool)
  - **What it does**: Whether to limit rotation around the Y axis (yaw)
  - **Default**: true
  - **When to use**: Enable to constrain yaw movement

- **Min Y Angle** (float)
  - **What it does**: Minimum allowed Y rotation angle in degrees
  - **Range**: -180.0 - 180.0
  - **Default**: -180.0
  - **When to use**: Set the lower yaw limit

- **Max Y Angle** (float)
  - **What it does**: Maximum allowed Y rotation angle in degrees
  - **Range**: -180.0 - 180.0
  - **Default**: 180.0
  - **When to use**: Set the upper yaw limit

- **Limit Z Rotation** (bool)
  - **What it does**: Whether to limit rotation around the Z axis (roll)
  - **Default**: false
  - **When to use**: Enable to constrain roll movement

- **Min Z Angle** (float)
  - **What it does**: Minimum allowed Z rotation angle in degrees
  - **Range**: -180.0 - 180.0
  - **Default**: -45.0
  - **When to use**: Set the lower roll limit

- **Max Z Angle** (float)
  - **What it does**: Maximum allowed Z rotation angle in degrees
  - **Range**: -180.0 - 180.0
  - **Default**: 45.0
  - **When to use**: Set the upper roll limit

- **Return To Original** (bool)
  - **What it does**: Whether the turret should return to its original rotation when deselected
  - **Default**: false
  - **When to use**: Enable for turrets that should reset position

- **Return Speed** (float)
  - **What it does**: Speed at which the turret returns to its original rotation (degrees per second)
  - **Range**: 1.0 - 100.0
  - **Default**: 30.0
  - **When to use**: Adjust for faster or slower return animation

**Setup Example**:
1. Add a Collider to your turret object
2. Add the TurretInteractable component
3. Configure rotation limits for each axis
4. Set up return behavior if needed
5. Test the turret rotation in play mode

## Advanced Configuration

### **Physics Setup**

For physics-based interactables (Grabable, Throwable):

1. Add a Rigidbody component
2. Set appropriate mass for the object type
3. Configure drag and angular drag for realistic movement
4. Set collision detection mode if needed

### **Feedback Integration**

Connect interactables to the feedback system:

1. Add a FeedbackSystem component to your interactable
2. Configure appropriate feedback types (haptic, audio, visual)
3. Connect UnityEvents to feedback triggers
4. Test feedback in VR to ensure proper feel

## Event Subscription Examples

### **Subscribing to Interactable Events**

```csharp
// Get a reference to an interactable
InteractableBase interactable = GetComponent<InteractableBase>();

// Subscribe to selection events
interactable.OnSelected.Subscribe(interactor => {
    Debug.Log($"Object selected by {interactor.name}!");
});

// Subscribe to deselection events
interactable.OnDeselected.Subscribe(interactor => {
    Debug.Log($"Object deselected by {interactor.name}!");
});

// Subscribe to hover events
interactable.OnHoverStarted.Subscribe(interactor => {
    Debug.Log($"Object hovered by {interactor.name}!");
});

interactable.OnHoverEnded.Subscribe(interactor => {
    Debug.Log($"Object hover ended by {interactor.name}!");
});

// Subscribe to activation events
interactable.OnActivated.Subscribe(interactor => {
    Debug.Log($"Object activated by {interactor.name}!");
});
```

### **Subscribing to Specific Interactable Events**

```csharp
// For LeverInteractable
LeverInteractable lever = GetComponent<LeverInteractable>();
lever.OnLeverChanged.Subscribe(value => {
    Debug.Log($"Lever value: {value}"); // value is between 0 and 1
});

// For DrawerInteractable
DrawerInteractable drawer = GetComponent<DrawerInteractable>();
drawer.OnValueChanged.Subscribe(value => {
    Debug.Log($"Drawer value: {value}"); // value is between 0 and 1
});

// For Throwable
Throwable throwable = GetComponent<Throwable>();
throwable.OnThrowEnd.Subscribe(velocity => {
    Debug.Log($"Object thrown with velocity: {velocity}");
});
```

### **Using UniRx Observables**

```csharp
// Get the interactable
InteractableBase interactable = GetComponent<InteractableBase>();

// Subscribe to UniRx observables
IDisposable selectedSubscription = interactable.OnSelected.Subscribe(interactor => {
    Debug.Log($"Object selected by {interactor.name} via UniRx!");
});

IDisposable deselectedSubscription = interactable.OnDeselected.Subscribe(interactor => {
    Debug.Log($"Object deselected by {interactor.name} via UniRx!");
});

// Dispose subscriptions when done (important to prevent memory leaks)
selectedSubscription.Dispose();
deselectedSubscription.Dispose();

// Or use AddTo(this) for automatic disposal when the component is destroyed
interactable.OnSelected
    .Subscribe(interactor => Debug.Log("Selected!"))
    .AddTo(this);
```

## Creating Custom Interactables

### **Basic Custom Interactable**

```csharp
using UnityEngine;
using Shababeek.Interactions;

public class CustomInteractable : InteractableBase
{
    [SerializeField] private string customMessage = "Custom interaction!";
    
    protected override void Activate()
    {
        // Called when the alternative button is pressed while selected
        Debug.Log($"Activated: {customMessage}");
    }
    
    protected override void StartHover()
    {
        // Called when an interactor starts hovering
        Debug.Log("Started hovering");
    }
    
    protected override void EndHover()
    {
        // Called when an interactor stops hovering
        Debug.Log("Ended hovering");
    }
    
    protected override bool Select()
    {
        // Called when an interactor selects this object
        // Return true to prevent default selection behavior
        Debug.Log($"Selected: {customMessage}");
        return false; // Allow default selection
    }
    
    protected override void DeSelected()
    {
        // Called when an interactor deselects this object
        Debug.Log("Deselected");
    }
}
```

### **Custom Constrained Interactable**

```csharp
using UnityEngine;
using Shababeek.Interactions;

public class CustomConstrainedInteractable : ConstrainedInteractableBase
{
    [SerializeField] private float rotationSpeed = 90f; // degrees per second
    
    protected override void Activate()
    {
        // Custom activation logic
    }
    
    protected override void StartHover()
    {
        // Custom hover start logic
    }
    
    protected override void EndHover()
    {
        // Custom hover end logic
    }
    
    protected override void HandleObjectMovement()
    {
        if (!IsSelected) return;
        
        // Custom movement logic
        Vector3 direction = CurrentInteractor.transform.position - transform.position;
        float angle = Vector3.SignedAngle(transform.forward, direction, transform.up);
        
        // Apply rotation
        interactableObject.Rotate(0, angle * rotationSpeed * Time.deltaTime, 0);
    }
    
    protected override void HandleObjectDeselection()
    {
        // Custom deselection logic
        // Reset position, play sound, etc.
    }
}
```

### **Custom Interactable with Events**

```csharp
using UnityEngine;
using UnityEngine.Events;
using Shababeek.Interactions;

public class CustomEventInteractable : InteractableBase
{
    [Header("Custom Events")]
    [SerializeField] private UnityEvent onCustomAction;
    [SerializeField] private UnityEvent<float> onValueChanged;
    
    [Header("Settings")]
    [SerializeField] private float interactionValue = 0f;
    
    protected override void Activate()
    {
        // Trigger custom action
        onCustomAction?.Invoke();
        
        // Update value and trigger event
        interactionValue = Mathf.Clamp01(interactionValue + 0.1f);
        onValueChanged?.Invoke(interactionValue);
    }
    
    protected override bool Select()
    {
        Debug.Log("Custom interactable selected!");
        return false;
    }
    
    protected override void DeSelected()
    {
        Debug.Log("Custom interactable deselected!");
    }
    
    // Public method to reset the interaction value
    public void ResetValue()
    {
        interactionValue = 0f;
        onValueChanged?.Invoke(interactionValue);
    }
}
```

## Troubleshooting

### **Common Issues**

**Object not responding to interaction:**
- Check that the object has a Collider
- Verify the object is on the correct layer
- Ensure the InteractableBase component is attached
- Check that an Interactor is present in the scene

**Physics behaving unexpectedly:**
- Adjust Rigidbody mass and drag values
- Check collision detection settings
- Verify collider size and shape
- Test with different physics materials

**Events not triggering:**
- Verify UnityEvents are properly connected
- Check that the correct event is being used
- Test with simple debug logs first
- Ensure no errors in the console

### **Performance Tips**

- Use appropriate collider types (BoxCollider for simple shapes)
- Limit the number of active interactables in large scenes
- Use object pooling for frequently created/destroyed objects
- Optimize feedback systems for VR performance


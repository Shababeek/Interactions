# Config Asset  Central System Configuration

## Overview

The **Config** asset is the brain of the Shababeek Interaction System. It's a ScriptableObject that stores all system-wide settings in one place, ensuring every component in your project uses the same configuration.

**Think of it as:** A central control panel for your VR interaction system.

**You only need one** Config asset per project (though you can create multiple for different scenarios).

**Key Responsibilities:**
-  **Layer Management** - Defines which layers hands and objects use
-  **Hand Configuration** - Select which hand will be used in your project
-  **Input Setup** - Manages controller input (new Input System or legacy)
-  **Physics Settings** - Controls hand physics behavior
-  **System References** - Automatically creates and manages system objects

---

## Quick Start

### Creating Your First Config

**Recommended: Use the Setup Wizard** *
1. Right-click in hierarchy or menu: **Shababeek > Setup Wizard**
2. Follow the on-screen steps
3. The wizard creates and configures everything automatically

**Manual Creation:**
1. Right-click in Project window
2. **Create > Shababeek > Interactions > Config**
3. Name it "InteractionConfig" (or similar)
4. Configure settings (see sections below)

![CreateConfig.png](../Images/CreateConfig.png)

### Where to Assign It

The Config asset is assigned in the **CameraRig** component, if you don't assign it nothing will work
Once assigned, all hands  and interactables automatically use this configuration.

---

## Inspector Reference

### Hand Configuration

#### Hand Data

**Field**: Hand Data *HandData Asset (see HandData documentation)*
**Purpose**: References the hand models, poses, and finger Masks.
**Required**: Yes (system won't work without it)

**What it does:**
- Provides hand prefabs for Use system wide
- Stores available hand poses (Default, Fist, Pointing, custom)
- Contains finger animation data and avatar masks

**Setup:**
Follow instructions in the *HandData Documentation (coming soon)*

**Troubleshooting:**
-  **"Hands not showing"**  Check HandData is assigned and has prefabs
-  **"Poses not working"**  Verify HandData has animation clips assigned

---

### Layer Configuration

Layers prevent unwanted collisions (like hands colliding with themselves), this should be automatic once you select the layers.


| Field                  | Type | Purpose | Default |
|------------------------|------|---------|---------|
| **Left Hand Layer**    | LayerMask | Layer assigned to the left hand GameObject | Create a "LeftHand" layer |
| **Right Hand Layer**   | LayerMask | Layer assigned to the right hand GameObject | Create a "RightHand" layer |
| **Interactable Layer** | LayerMask | Layer for all grabbable/interactable objects | Create an "Interactable" layer |
| **Player Layer**       | LayerMask | Layer for the player body/rig | Use Unity's "Player" layer or create custom |

**Why Layers Matter:**
- Hands won't collide with themselves (left hand vs left hand body)
- Hands won't collide with player body
- Clear separation between interactable and non-interactable objects
- Optimizes physics calculations

**Recommended Layer Setup:**

| Layer Name | Number | Used For | Collides With |
|------------|--------|----------|---------------|
| LeftHand | 8 | Left hand objects | Interactable, Environment |
| RightHand | 9 | Right hand objects | Interactable, Environment |
| Interactable | 10 | Grabbable objects | LeftHand, RightHand, Environment |
| Player | 11 | Player body | Environment (NOT hands) |

**Setup Steps:**
1. Go to **Edit > Project Settings > Tags and Layers**
2. Create the four layers listed above
3. Go to **Edit > Project Settings > Physics**
4. Configure **Layer Collision Matrix**:
 -  LeftHand can collide with Interactable
 -  RightHand can collide with Interactable
 -  LeftHand cannot collide with Player
 -  RightHand cannot collide with Player
 -  LeftHand cannot collide with LeftHand
 -  RightHand cannot collide with RightHand

<!-- TODO: Add PhysicsLayerMatrix.png -->
*Physics Layer Matrix configuration*

**Note:** The Setup Wizard can configure this automatically!

---

### Input Manager Settings

Choose between Unity's new Input System (recommended) or legacy Input Manager.

#### Input Type

| Property | Value |
|----------|-------|
| **Type** | Enum |
| **Options** | InputSystem, InputManager |
| **Default** | InputSystem |
| **Recommendation** | Use InputSystem for new projects |

**InputSystem (Modern - Recommended):**
- More flexible and powerful
- Better VR controller support
- Easier to customize
- Required by most modern XR plugins

**InputManager (Legacy):**
- Unity's old input system
- Uses axis names and buttons
- Good for older projects or specific requirements

#### Left/Right Hand Actions (Input System Mode)

| Property | Value |
|----------|-------|
| **Field** | Left Hand Actions / Right Hand Actions |
| **Type** | HandInputActions struct |
| **Visible** | Only when Input Type = InputSystem |

These reference InputAction assets from Unity's Input System.

**Structure:**

| Action | Description |
|--------|-------------|
| **Thumb** | Thumb button (usually A/X button) |
| **Index** | Index finger trigger |
| **Middle** | Middle finger curl |
| **Ring** | Ring finger curl |
| **Pinky** | Pinky finger curl |
| **Grip** | Grip button (side button) |
| **Trigger** | Trigger button |
| **Primary** | Primary button (A/X) |
| **Secondary** | Secondary button (B/Y) |

**Setup:**
1. Create an Input Actions asset: **Create > Input Actions**
2. Define actions for each hand (grip, trigger, fingers, etc.)
3. Assign the actions to these fields

**Quick Setup:**
The Setup Wizard can create default input actions automatically.

---

### Old Input Manager Settings (Legacy Mode)

| Property | Value |
|----------|-------|
| **Field** | Old Input Settings |
| **Type** | OldInputManagerSettings struct |
| **Visible** | Only when Input Type = InputManager |

Contains axis and button names for Unity's legacy input system.

**Settings:**

| Hand | Setting | Example | Description |
|------|---------|---------|-------------|
| **Left** | leftTriggerAxis | "Shababeek_Left_Trigger" | Axis name for trigger |
| **Left** | leftGripAxis | "Shababeek_Left_Grip" | Axis name for grip button |
| **Left** | leftPrimaryButton | "Shababeek_Left_PrimaryButton" | Button name for primary button |
| **Left** | leftSecondaryButton | "Shababeek_Left_SecondaryButton" | Button name for secondary button |
| **Left** | Debug keys | "z", "x", "c" | Keyboard testing keys |
| **Right** | rightTriggerAxis | "Shababeek_Right_Trigger" | Axis name for trigger |
| **Right** | rightGripAxis | "Shababeek_Right_Grip" | Axis name for grip button |
| **Right** | rightPrimaryButton | "Shababeek_Right_PrimaryButton" | Button name for primary button |
| **Right** | rightSecondaryButton | "Shababeek_Right_SecondaryButton" | Button name for secondary button |
| **Right** | Debug keys | "m", "n", "b" | Keyboard testing keys |

**Default Values:**
The system provides default axis names following the "Shababeek_" prefix convention.

**Setup:**
1. Go to **Edit > Project Settings > Input Manager**
2. Create axes with names matching the fields above
3. Or use the "Create Input Axes" button in Config inspector (if available)

**Debug Keys:**
These allow testing without VR hardware:
- Set keyboard keys (e.g., "z" for left grip)
- Press keys in Play Mode to simulate controller buttons
- Useful for development and testing

---

### Editor UI Settings

#### Feedback System Style Sheet

| Property | Value |
|----------|-------|
| **Type** | StyleSheet |
| **Purpose** | Customizes the visual appearance of feedback system UI |
| **Optional** | Yes (has default styling) |

This is for customizing the editor's UI for the feedback system. Most users don't need to change this.

---

### Hand Physics Settings

Control how hands interact with physics objects.

#### Hand Mass

| Property | Value |
|----------|-------|
| **Type** | float |
| **Default** | 30.0 |
| **Range** | 0.1 - 100 |
| **Purpose** | Mass of hand physics objects |

**What it affects:**
- How hands push objects
- How stable hands are when grabbing
- How hands react to collisions

**Guidelines:**
- **Low (1-10):** Responsive but can be shaky
- **Medium (20-40):** Balanced (recommended)
- **High (50-100):** Stable but less responsive

**Adjust if:**
- [OK] Hands feel too floaty  Increase mass
- [OK] Hands vibrate when holding objects  Increase mass
- [OK] Hands move too slowly  Decrease mass
- [OK] Hands push objects too weakly  Increase mass

#### Linear Damping

| Property | Value |
|----------|-------|
| **Type** | float |
| **Default** | 5.0 |
| **Range** | 0 - 20 |
| **Purpose** | How quickly hand movement slows down |

**Think of it as:** Air resistance for hand movement.

**Guidelines:**
- **Low (0-2):** Hands glide, less damping
- **Medium (3-8):** Natural feel (recommended)
- **High (9-20):** Hands stop quickly, more damping

**Adjust if:**
- [OK] Hands move too much after you stop  Increase damping
- [OK] Hands feel sluggish  Decrease damping

#### Angular Damping

| Property | Value |
|----------|-------|
| **Type** | float |
| **Default** | 1.0 |
| **Range** | 0 - 10 |
| **Purpose** | How quickly hand rotation slows down |
Purpose: How quickly hand rotation slows down
```

**Think of it as:** Air resistance for hand rotation.

**Guidelines:**
- **Low (0-0.5):** Hands spin freely
- **Medium (1-3):** Natural rotation (recommended)
- **High (4-10):** Rotation stops quickly

**Adjust if:**
- [OK] Hands rotate too much when you twist your wrist  Increase damping
- [OK] Rotation feels stiff  Decrease damping

---

### Hand Following Settings

#### Follower Preset

| Property | Value |
|----------|-------|
| **Type** | Enum |
| **Options** | Gentle, Standard, Responsive, Aggressive, Custom |
| **Default** | Standard |
| **Purpose** | Controls how hands follow controller movement |

**Presets Explained:**

| Preset | Feel | Use Case |
|--------|------|----------|
| **Gentle** | Smooth, floaty | Casual experiences, comfort-focused |
| **Standard** | Balanced | General VR interactions (recommended) |
| **Responsive** | Snappy, direct | Action games, precise interactions |
| **Aggressive** | Very tight tracking | Competitive games, minimal latency |
| **Custom** | Your settings | Fine-tuned control |

**What it affects:**
- Position smoothing (how quickly hands move to controller position)
- Rotation smoothing (how quickly hands rotate to controller rotation)
- Max distance/angle (limits on how far hands can be from controllers)

**Adjust if:**
- [OK] Hands lag behind controllers  Use Responsive or Aggressive
- [OK] Hands feel jittery  Use Gentle or Standard
- [OK] Need precise control for specific game  Use Custom

#### Custom Follower Settings

| Property | Value |
|----------|-------|
| **Type** | PhysicsFollowerSettings |
| **Visible** | Only when Follower Preset = Custom |
| **Purpose** | Fine-tune hand following behavior |

**Available Settings:**
- Position smoothing factors
- Rotation smoothing factors
- Maximum follow distance
- Maximum rotation angle
- Force multipliers

See [Physics Hand Follower documentation] for detailed settings explanation.

---

### System References (Read-Only)

These fields are managed automatically - you don't need to set them.

#### Game Manager

| Property | Value |
|----------|-------|
| **Type** | GameObject |
| **Purpose** | GameObject that hosts the input manager |
| **Auto-created** | Yes, when needed |

This is automatically created in your scene when the Config is initialized. Don't delete it!

#### Input Manager

| Property | Value |
|----------|-------|
| **Type** | InputManagerBase |
| **Purpose** | Component that reads controller input |
| **Auto-created** | Yes, based on Input Type |

Automatically created based on your Input Type selection:
- `InputSystem`  Creates `NewInputSystemBasedInputManager`
- `InputManager`  Creates `AxisBasedInputManager`

---

## Configuration Workflows

### Initial Setup (Beginner)

**Option 1: Setup Wizard (Easiest) ***

1. **Right-click** in hierarchy or menu bar
2. Select **Shababeek > Setup Wizard**
3. Follow these steps:
    - **Step 1:** Choose input type (Input System recommended)
    - **Step 2:** Wizard creates Config asset automatically
    - **Step 3:** Wizard creates HandData asset
    - **Step 4:** Wizard sets up layers
    - **Step 5:** Wizard configures physics layer matrix
    - **Step 6:** Wizard creates/configures CameraRig
4. **Done!** Everything is configured

**Option 2: Manual Setup**

1. **Create Config asset:**
    - Right-click in Project > Create > Shababeek > Interactions > Config

2. **Configure Hand Data:**
    - Create HandData: Create > Shababeek > Interaction System > Hand Data
    - Set up hand prefabs and poses
    - Assign to Config's Hand Data field

3. **Configure Layers:**
    - Edit > Project Settings > Tags and Layers
    - Create: LeftHand (8), RightHand (9), Interactable (10), Player (11)
    - Assign in Config

4. **Configure Physics:**
    - Edit > Project Settings > Physics
    - Set up Layer Collision Matrix

5. **Configure Input:**
    - Choose Input Type
    - Set up Input Actions (Input System) or Axis names (Input Manager)

6. **Assign to CameraRig:**
    - Add CameraRig component to your XR Origin
    - Drag Config into the Config field

---

### Common Configuration Patterns

#### Pattern 1: Standard VR Setup (Most Common)
```
Input Type: InputSystem
Hand Mass: 30
Linear Damping: 5
Angular Damping: 1
Follower Preset: Standard

Layers:
- LeftHand: Layer 8
- RightHand: Layer 9
- Interactable: Layer 10
- Player: Layer 11
```

**Use for:** General VR experiences, most games

#### Pattern 2: Comfort-Focused Experience
```
Input Type: InputSystem
Hand Mass: 20
Linear Damping: 8
Angular Damping: 2
Follower Preset: Gentle

Same layer setup as Pattern 1
```

**Use for:** Casual games, experiences targeting comfort-sensitive users

#### Pattern 3: Action/Competitive Game
```
Input Type: InputSystem
Hand Mass: 40
Linear Damping: 3
Angular Damping: 0.5
Follower Preset: Responsive

Same layer setup as Pattern 1
```

**Use for:** Action games, competitive VR, rhythm games

#### Pattern 4: Legacy Project
```
Input Type: InputManager
Hand Mass: 30
Linear Damping: 5
Angular Damping: 1
Follower Preset: Standard

Same layer setup as Pattern 1
+ Configure Old Input Settings with axis names
```

**Use for:** Updating older projects, specific input requirements

---

## Validation & Troubleshooting

### Config Inspector Validation

The Config inspector shows validation status for:
-  **HandData assigned** - Required for system to work
-  **Layers configured** - Checks if layers are set
-  **Physics matrix valid** - Verifies layer collision settings
-  **Input configured** - Checks if input is set up

**Validation Indicators:**

```
Green box: "All systems configured correctly"
Yellow box: "Some settings may need attention"
Red box: "Critical configuration missing"
```

### Common Issues

#### Issue: "HandData is not assigned"
**Symptoms:** Red validation error in inspector  
**Solution:**
1. Create HandData asset if you don't have one
2. Drag HandData into Hand Data field
3. Validation should turn green

#### Issue: "Layers are not configured"
**Symptoms:** Yellow/red validation warning  
**Solution:**
1. Go to Edit > Project Settings > Tags and Layers
2. Create the four required layers
3. Assign them in Config
4. Use Setup Wizard to configure automatically

#### Issue: "Physics layer collision matrix invalid"
**Symptoms:** Yellow warning about layer collisions  
**Solution:**
1. Go to Edit > Project Settings > Physics
2. Uncheck collisions between:
    - LeftHand  LeftHand
    - RightHand  RightHand
    - LeftHand  Player
    - RightHand  Player
3. Check collisions between:
    - LeftHand  Interactable
    - RightHand  Interactable

#### Issue: "Hands not responding to input"
**Symptoms:** Hands visible but don't move/respond to buttons  
**Solutions:**

**If using Input System:**
- Check Input Actions are assigned in Config
- Verify Input System package is installed
- Check that Input Actions are enabled
- Ensure controller is connected and detected

**If using Input Manager:**
- Check Old Input Settings are configured
- Go to Edit > Project Settings > Input Manager
- Verify axis names match Config settings
- Try debug keys to test without VR hardware

#### Issue: "Hands feel wrong (too slow/fast/jittery)"
**Symptoms:** Hand movement doesn't feel natural  
**Solutions:**
- Adjust Hand Mass (higher = more stable)
- Adjust Linear Damping (higher = less movement)
- Change Follower Preset (Gentle/Standard/Responsive)
- Use Custom preset for fine control

#### Issue: "Hands collide with player body"
**Symptoms:** Hands stop when near body  
**Solution:**
1. Check Player Layer is assigned in Config
2. Go to Physics settings
3. Disable collisions: LeftHand/RightHand  Player

#### Issue: "Multiple Config assets causing conflicts"
**Symptoms:** Inconsistent behavior across scene  
**Solution:**
- Use only ONE Config asset per scene
- All CameraRigs should reference the same Config
- Remove or disable extra Config assets

---

## Scripting API

### Accessing the Config

```csharp
using Shababeek.Interactions.Core;

public class MyComponent : MonoBehaviour
{
    [SerializeField] private Config config;
    
    void Start()
    {
        // Access from CameraRig
        var cameraRig = GetComponent<CameraRig>();
        config = cameraRig.Config;
        
        // Or find in scene (not recommended - assign in inspector instead)
        config = FindObjectOfType<CameraRig>().Config;
    }
}
```

### Reading Configuration

```csharp
// Hand Data
HandData handData = config.HandData;

// Layers
int leftHandLayer = config.LeftHandLayer;
int rightHandLayer = config.RightHandLayer;
int interactableLayer = config.InteractableLayer;
int playerLayer = config.PlayerLayer;

// Input
InputManagerType inputType = config.InputType;
InputManagerBase inputManager = config.InputManager;

// Physics
float handMass = config.HandMass;
float linearDamping = config.HandLinearDamping;
float angularDamping = config.HandAngularDamping;

// Follower settings
PhysicsFollowerSettings followerSettings = config.FollowerSettings;
```

### Input System Access

```csharp
// Get input manager
var inputManager = config.InputManager;

// Access hand input
var leftHand = inputManager.GetHandInputManager(HandIdentifier.Left);
var rightHand = inputManager.GetHandInputManager(HandIdentifier.Right);

// Read button states
bool leftGripPressed = leftHand.Grip;
bool rightTriggerPressed = rightHand.Trigger;

// Read finger curl values
float leftIndex = leftHand.Index;
float rightThumb = rightHand.Thumb;
```

### Layer Utilities

```csharp
// Check if object is on interactable layer
bool isInteractable = gameObject.layer == config.InteractableLayer;

// Set object to interactable layer
gameObject.layer = config.InteractableLayer;

// Check layer collision
bool canCollide = Physics.GetIgnoreLayerCollision(
    config.LeftHandLayer, 
    config.InteractableLayer
);
```

### Runtime Modification (Advanced)

**Note:** Changing Config at runtime is NOT recommended. Config is meant for design-time setup. However, if needed:

```csharp
// Change physics settings (affects new hands created after this)
// This won't affect already-created hands
config.HandMass = 50f;
config.LinearDamping = 10f;

// Switch input type (requires scene reload)
config.InputType = Config.InputManagerType.InputManager;
// Must reinitialize the input system after this
```

**Better approach:** Create multiple Config assets for different scenarios and swap them in CameraRig.

---

## Advanced Topics

### Multiple Config Assets

You can create multiple Config assets for different scenarios:

**Example Use Cases:**
- **Development Config** - Debug keys enabled, lower physics quality
- **Production Config** - VR input only, higher physics quality
- **Testing Config** - Specific input setup for automated testing

**How to use:**
1. Create multiple Config assets
2. Assign different configs to CameraRig in different scenes
3. Or swap configs at runtime (requires reinitializing)

**Caution:** Only one Config should be active per scene.

### Custom Input Manager

You can create your own input manager:

```csharp
using Shababeek.Interactions.Core;

public class CustomInputManager : InputManagerBase
{
    public override void Initialize(Config config)
    {
        // Your custom initialization
    }
    
    protected override void CreateInputManager(HandIdentifier handId)
    {
        // Create custom hand input manager
    }
}
```

Then modify Config to use your custom manager.

### Config as a Prefab

You can make Config a prefab asset for reuse across projects:

1. Create and configure Config
2. Create a "Prefabs" folder in your Assets
3. Drag Config into the folder (it's already a ScriptableObject asset)
4. Copy this Config to other projects
5. Adjust for project-specific settings

**What transfers:**
-  Layer numbers (may need adjustment)
-  Physics settings
-  Input type choice
-  Input Actions (need to be recreated per-project)
-  HandData (need to be recreated or copied separately)

---

## Best Practices

### Do's [OK]

**Use the Setup Wizard** - Saves time and avoids errors  
**Create Config early** - Before setting up any interactions  
**Assign descriptive layer names** - Makes debugging easier  
**Test with default settings first** - Customize only if needed  
**Use Input System** - Better for modern VR projects  
**Keep one Config per project** - Avoid configuration conflicts  
**Document custom settings** - Note why you changed defaults  
**Version control Config** - Include in your repository

### Don'ts

**Don't skip layer setup** - Leads to collision issues  
**Don't modify Config at runtime** - Causes unpredictable behavior  
**Don't delete the Game Manager GameObject** - Breaks input  
**Don't set extreme physics values** - Can make hands unstable  
**Don't use multiple Configs in one scene** - Causes conflicts  
**Don't forget to assign HandData** - System won't work  
**Don't ignore validation warnings** - They indicate real issues

### Performance Tips

1. **Physics settings:** Start with defaults, adjust only if needed
2. **Follower preset:** "Standard" is optimized for most cases
3. **Hand mass:** Higher values are more stable but more expensive
4. **Damping:** Higher values reduce physics calculations
5. **Layer collisions:** Minimize enabled layer pairs in Physics settings

---

## Related Documentation

- **[Setup Wizard](SetupWizard.md)** - Automated configuration tool
- **[HandData & Poses](../PoseSystem/readme.md)** - Setting up hand models
- **[CameraRig](CameraRig.md)** - VR camera setup
- **[Input System](InputSystem.md)** - Controller input configuration
- **[Quick Start Guide](../GettingStarted/QuickStart.md)** - First-time setup

---

## FAQ

**Q: Do I need to create a Config for every scene- **  
A: No! Create one Config asset and reference it in every scene's CameraRig.

**Q: Can I have different settings for different hands- **  
A: Hand physics settings apply to both hands. For different input per hand, configure Left/Right Hand Actions separately.

**Q: What if I want to change settings during gameplay- **  
A: Config is designed for design-time setup. For runtime changes, expose specific settings as parameters on your components rather than modifying Config directly.

**Q: Should I use Input System or Input Manager- **  
A: Use **Input System** for new projects. It's more flexible, better supported, and required by modern XR plugins.

**Q: Can I copy Config between projects- **  
A: Yes, but you'll need to reassign HandData and recreate Input Actions for the new project.

**Q: What happens if I don't assign layers- **  
A: The system will still work, but you'll have collision issues (hands colliding with themselves, etc.). Always configure layers!

**Q: How do I test without VR hardware- **  
A: Use Input Manager mode and set debug keys in Old Input Settings. Press those keys to simulate controller buttons.

**Q: Can I have multiple Config assets- **  
A: Yes, but only one should be active per scene. Useful for dev/production configs or different game modes.

---

**Last Updated:** October 2025  
**Component Version:** Shababeek Interactions 1.0+
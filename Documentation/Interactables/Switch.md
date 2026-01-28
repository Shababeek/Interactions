# Switch — Physical Toggle Component

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Interactables > Switch
> **Use For:** Physical switches that rotate when touched
> **Requires:** Collider component (set as Trigger)

---

## What It Does

The **Switch** component creates a physical toggle switch that responds to trigger collisions. When a hand or finger enters the switch's trigger volume, the switch rotates based on the direction of approach, firing events for on/off states.

**Perfect for:**
- ✅ Light switches
- ✅ Power toggles
- ✅ Circuit breakers
- ✅ Any physical flip-switch mechanism

**Don't use for:**
- ❌ Push buttons (use VRButton instead)
- ❌ Objects that need to be grabbed (use Grabable)
- ❌ Continuous controls like dimmers (use Lever)

---

## Quick Example

> **Goal:** Create a light switch that toggles a room light

![Switch Toggle](../Images/switch-toggle.gif)

1. Create a switch model with a pivot point
2. Add Switch component
3. Add a Trigger collider
4. Wire On Up/On Down events to light.enabled

---

## Inspector Reference

![Switch Inspector](../Images/switch-inspector-labeled.png)

### Events

#### On Up
Fires when the switch moves to the **up/on** position.

**Common uses:**
- Turn on lights
- Enable systems
- Play "click on" sound
- Start machinery

#### On Down
Fires when the switch moves to the **down/off** position.

**Common uses:**
- Turn off lights
- Disable systems
- Play "click off" sound
- Stop machinery

#### On Hold
Fires while the switch is held in a position.

**Common uses:**
- Continuous effects while held
- Charging mechanics

---

### Switch Configuration

#### Switch Body
The transform that visually rotates when the switch is toggled.

**What to assign:** The child object that represents the physical switch lever/toggle.

> 💡 **Tip:** Create your switch with a parent (for the base) and child (for the moving part). Assign the child here.

---

#### Rotation Axis
The local axis around which the switch rotates.

| Value | Rotation |
|-------|----------|
| **X** | Rotates around local X axis (pitch) |
| **Y** | Rotates around local Y axis (yaw) |
| **Z** | Rotates around local Z axis (roll) |

**Default:** Z

Choose based on your switch's orientation and desired movement direction.

---

#### Detection Axis
The axis used to determine which direction the hand is approaching from.

| Value | Detection Direction |
|-------|---------------------|
| **X** | Left/Right approach |
| **Y** | Up/Down approach |
| **Z** | Front/Back approach |

**Default:** X

The switch rotates away from the approaching hand based on this detection.

---

#### Up Rotation / Down Rotation
Rotation angles (in degrees) for the on and off positions.

| Setting | Description | Default |
|---------|-------------|---------|
| **Up Rotation** | Angle when switch is "on" | 20° |
| **Down Rotation** | Angle when switch is "off" | -20° |

Adjust these to match your switch model's range of motion.

---

#### Rotate Speed
How fast the switch animates between positions (degrees per second factor).

| Value | Speed |
|-------|-------|
| **5** | Slow, deliberate |
| **10** | Normal (default) |
| **20** | Fast, snappy |

---

#### Angle Threshold
Minimum angle (in degrees) the hand must be offset from center before the switch activates.

**Purpose:** Prevents accidental toggles when hand is directly in front.

**Default:** 5°

Increase if switch is too sensitive; decrease if it's hard to trigger.

---

#### Stay In Position
When enabled, the switch stays where it was toggled instead of returning to neutral.

| Value | Behavior |
|-------|----------|
| **Checked** | Switch stays at on/off position after hand leaves |
| **Unchecked** | Switch returns to neutral when hand leaves |

**Default:** Unchecked (returns to neutral)

**Use Stay In Position for:**
- Permanent toggles (light switches)
- State-based controls

**Use return to neutral for:**
- Momentary switches
- Spring-loaded toggles

---

#### Starting Position
Initial position of the switch when the scene starts.

| Value | Position |
|-------|----------|
| **Off** | Starts in down/off position |
| **Neutral** | Starts in middle position |
| **On** | Starts in up/on position |

**Default:** Neutral

---

### Debug

#### Direction (Read-Only)
Shows the current switch direction during play mode.

| Value | Meaning |
|-------|---------|
| **Up (1)** | Switch is in on position |
| **Down (-1)** | Switch is in off position |
| **None (0)** | Switch is in neutral/transitioning |

---

## Adding to Your Scene

### Step 1: Create Switch Structure

1. Create an empty GameObject, name it "LightSwitch"
2. Add a child 3D object for the base (cube scaled flat)
3. Add another child for the toggle lever (small cube or cylinder)
4. Position the lever where it should pivot

```
LightSwitch (empty)
├── Base (cube, scaled to switch plate)
└── Lever (small cube, positioned as toggle)
```

### Step 2: Add Components

1. Select the parent "LightSwitch" object
2. **Add Component > Switch**
3. **Add Component > Box Collider**
4. Check **Is Trigger** on the collider
5. Size the collider to cover the interaction area

### Step 3: Configure Switch

1. Drag the **Lever** child into the **Switch Body** field
2. Set **Rotation Axis** to match your lever orientation
3. Set **Detection Axis** based on approach direction
4. Adjust **Up/Down Rotation** angles
5. Enable **Stay In Position** for a toggle switch

### Step 4: Wire Events

1. Expand **On Up** event
2. Click **+**
3. Drag your light GameObject
4. Select **Light > enabled** (or a custom method)
5. Check the checkbox (to enable light on "up")

Repeat for **On Down** with the checkbox unchecked.

![Switch Event Wiring](../Images/switch-event-wiring.png)

---

## Common Workflows

### How To: Create a Basic Light Switch

> **Goal:** Toggle a light on/off
> **Time:** ~3 minutes

#### Setup
```
Rotation Axis: Z
Detection Axis: X
Up Rotation: 25
Down Rotation: -25
Stay In Position: ✓ (checked)
Starting Position: Off
```

#### Events
- **On Up** → Light.enabled = true
- **On Down** → Light.enabled = false

---

### How To: Create a Momentary Switch

> **Goal:** Switch only active while held
> **Time:** ~2 minutes

#### Setup
```
Stay In Position: ☐ (unchecked)
Starting Position: Neutral
```

#### Events
- **On Up** → Activate effect
- **On Down** → Activate different effect
- (Effect stops when hand leaves and switch returns to neutral)

---

### How To: Create a Three-Position Switch

> **Goal:** Off / Neutral / On positions
> **Time:** ~3 minutes

For a three-position effect, combine the starting position with events:

#### Setup
```
Stay In Position: ✓ (checked)
Starting Position: Neutral
```

#### Logic
- Neutral = Initial state
- On Up = Position 1 (up)
- On Down = Position 2 (down)

Wire events to your game logic that tracks all three states.

---

### How To: Add Sound Effects

> **Goal:** Click sound when switch toggles
> **Time:** ~2 minutes

1. Add **AudioSource** to switch object
2. Assign click sound to AudioClip
3. Uncheck **Play On Awake**
4. Wire both **On Up** and **On Down** to AudioSource.Play()

For different sounds per direction:
1. Add two AudioSource components
2. Assign different clips
3. Wire **On Up** to first, **On Down** to second

---

## Tips & Best Practices

💡 **Position collider carefully**
The trigger collider determines where the player can interact. Make it slightly larger than the visual switch for easier use.

💡 **Test axis orientation**
If the switch doesn't respond correctly to hand direction, try different Detection Axis values.

💡 **Use gizmos for debugging**
The Switch draws helpful gizmos in Scene view showing rotation limits and detection direction.

💡 **Consider haptic feedback**
Add vibration when switch toggles for better VR feel (use Feedback System).

⚠️ **Common Mistake:** Collider not set as Trigger
Switch uses OnTriggerEnter/Exit. Ensure **Is Trigger** is checked on the collider.

⚠️ **Common Mistake:** Switch Body not assigned
The lever won't rotate if Switch Body field is empty. Always assign the moving part.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Switch doesn't respond | Collider not a trigger | Check **Is Trigger** on collider |
| Lever doesn't move | Switch Body not assigned | Assign the moving transform to Switch Body |
| Wrong rotation direction | Axis misconfigured | Try different Rotation Axis values |
| Switch always triggers the same direction | Detection Axis wrong | Adjust Detection Axis to match approach |
| Switch too sensitive | Threshold too low | Increase Angle Threshold |
| Can't reach the switch | Collider too small | Enlarge the trigger collider |
| Switch returns when I don't want it to | Stay In Position off | Enable Stay In Position |

---

## Scripting API

### Properties

```csharp
// Get/set the switch body transform
switch.SwitchBody = leverTransform;

// Get/set stay in position behavior
switch.StayInPosition = true;

// Get/set starting position
switch.StartingPosition = StartingPosition.On;
```

### Methods

```csharp
// Reset switch to neutral
switch.ResetSwitch();

// Force reset (ignores StayInPosition)
switch.ForceResetSwitch();

// Get current state
bool? state = switch.GetSwitchState();
// true = up, false = down, null = neutral

// Get current rotation
Vector3 rotation = switch.GetCurrentRotation();

// Set position programmatically
switch.SetPosition(StartingPosition.On);
```

### Example: Reading Switch State

```csharp
public class SwitchReader : MonoBehaviour
{
    [SerializeField] private Switch mySwitch;

    void Update()
    {
        bool? state = mySwitch.GetSwitchState();

        if (state == true)
            Debug.Log("Switch is ON");
        else if (state == false)
            Debug.Log("Switch is OFF");
        else
            Debug.Log("Switch is NEUTRAL");
    }
}
```

---

## Related Documentation

- [VRButton](VRButton.md) — Press-style buttons
- [Lever](ConstrainedInteractables.md#lever) — Grabbable rotation control
- [Feedback System](../Systems/FeedbackSystem.md) — Add haptic/audio feedback
- [Quick Start Guide](../GettingStarted/QuickStart.md) — Basic setup

---

**Last Updated:** January 2026
**Component Version:** 1.0.0

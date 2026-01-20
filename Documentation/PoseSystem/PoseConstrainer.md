# PoseConstrainer — Hand Pose Control

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Pose Constrainer
> **Use For:** Controlling hand appearance and position during interactions
> **Required By:** Grabable, ConstrainedInteractableBase, and derived components

---

## What It Does

The **PoseConstrainer** component controls how the player's hand looks and where it positions when interacting with an object. It defines the hand pose (finger positions), hand placement (position and rotation relative to the object), and the constraint behavior.

**Think of it as:** A blueprint for how hands should grip this specific object.

**Key Responsibilities:**
- **Hand Positioning** — Where the hand appears relative to the object
- **Finger Poses** — Individual finger curl and lock settings
- **Constraint Behavior** — Whether hand snaps to pose, moves freely, or hides
- **Smooth Transitions** — Optional animation when grabbing

---

## When to Use

PoseConstrainer is **automatically added** when you create:
- Grabable objects (via right-click menu)
- Any ConstrainedInteractable (Lever, Drawer, Joystick, Wheel)

You typically **configure** PoseConstrainer rather than add it manually.

**Configure PoseConstrainer when you want to:**
- Create a custom hand grip for a tool (hammer, screwdriver)
- Position hands precisely on a weapon (gun grip, sword handle)
- Make hands wrap naturally around irregular shapes
- Hide hands for special effects
- Add polish with smooth grab transitions

---

## Inspector Reference

[PLACEHOLDER_SCREENSHOT: Full PoseConstrainer Inspector with numbered sections]

### Constraint Configuration

#### Constraint Type
Controls how hands behave during interaction.

| Value | Behavior | Use Case |
|-------|----------|----------|
| **Constrained** | Hand snaps to defined pose and position | Most objects — tools, weapons, props |
| **FreeHand** | Hand follows naturally, no pose override | Organic shapes, clothing, soft objects |
| **HideHand** | Hand model becomes invisible | Magic items, portals, special effects |

**Default:** Constrained

**Visual Example:**

[PLACEHOLDER_GIF: Side-by-side comparison of Constrained vs FreeHand vs HideHand]

---

#### Use Smooth Transitions
When enabled, hands animate smoothly to the grab position instead of snapping instantly.

| Value | Behavior |
|-------|----------|
| **Checked** | Hand smoothly moves to target position over time |
| **Unchecked** | Hand instantly snaps to target position |

**Default:** Unchecked

> 💡 **Tip:** Enable smooth transitions for a more polished feel, especially on tools and weapons where the grip matters.

---

#### Transition Speed
How fast the hand moves to the target position (only visible when Smooth Transitions is enabled).

| Value | Feel |
|-------|------|
| **5** | Slow, deliberate transition |
| **10** | Balanced (recommended) |
| **20** | Quick, snappy transition |

**Default:** 10
**Range:** 1 - 50

---

### Pose Constraints

Pose Constraints define how each finger behaves when holding the object. There are separate settings for **Left** and **Right** hands.

[PLACEHOLDER_SCREENSHOT: Pose Constraints section expanded]

#### Target Pose Index
Which base pose to use from the HandData asset.

| Index | Typical Pose |
|-------|--------------|
| **0** | Open hand (default) |
| **1** | Fist |
| **2** | Pointing |
| **3+** | Custom poses (project-dependent) |

This selects the starting animation; finger constraints modify it further.

---

#### Finger Constraints

Each finger (Thumb, Index, Middle, Ring, Pinky) has individual constraint settings:

##### Locked
When checked, the finger stays fixed at the **Min** value and ignores player input.

| Value | Behavior |
|-------|----------|
| **Checked** | Finger locked at Min value |
| **Unchecked** | Finger can move between Min and Max based on input |

**Use locked fingers for:**
- Fingers that must stay in a specific position (e.g., trigger finger on a gun)
- Creating static poses (e.g., pointing gesture)

##### Min (Range: 0-1)
Minimum curl amount for the finger.

| Value | Position |
|-------|----------|
| **0** | Fully straight/extended |
| **0.5** | Half curled |
| **1** | Fully curled/closed |

##### Max (Range: 0-1)
Maximum curl amount when the player grips (only used when not locked).

The actual finger position interpolates between Min and Max based on controller grip input.

---

### Hand Positioning

Defines where each hand appears relative to the object when grabbed. Separate settings for **Left** and **Right** hands.

[PLACEHOLDER_SCREENSHOT: Hand Positioning section with position/rotation fields]

#### Position Offset
Local position offset from the object's transform origin.

| Axis | Direction |
|------|-----------|
| **X** | Left (-) / Right (+) |
| **Y** | Down (-) / Up (+) |
| **Z** | Back (-) / Forward (+) |

**Default:** (0, 0, 0) — hand at object center

#### Rotation Offset
Local rotation offset in Euler angles.

| Axis | Rotation |
|------|----------|
| **X** | Pitch (tilt forward/back) |
| **Y** | Yaw (turn left/right) |
| **Z** | Roll (rotate clockwise/counter-clockwise) |

**Default:** (0, 0, 0) — no rotation offset

> 💡 **Tip:** Use Play mode to adjust these values in real-time and see immediate results!

---

## Common Workflows

### How To: Create a Custom Grip for a Tool

> **Goal:** Make a hammer with a natural grip pose
> **Time:** ~5 minutes

#### Step 1: Set Up the Object
1. Import or create your hammer model
2. Make it grabbable: Right-click → Shababeek → Make Into → Grabable
3. PoseConstrainer is automatically added

#### Step 2: Position the Hand
1. Enter **Play mode**
2. Grab the hammer in VR
3. Note where the hand appears vs. where it should be
4. Exit Play mode

Now adjust the positioning:
1. Select the hammer
2. Find **PoseConstrainer** component
3. Expand **Right Hand Positioning** (assuming right-handed grip)
4. Adjust **Position Offset**:
   - Move X to shift hand left/right on handle
   - Move Y to shift hand up/down
   - Move Z to shift hand forward/back
5. Adjust **Rotation Offset**:
   - Rotate to match natural grip angle

#### Step 3: Configure Finger Pose
For a hammer grip, you want fingers wrapped around the handle:

```
Thumb:  Locked=true, Min=0.6 (wrapped around)
Index:  Locked=true, Min=0.8 (tightly curled)
Middle: Locked=true, Min=0.9 (tightly curled)
Ring:   Locked=true, Min=0.9 (tightly curled)
Pinky:  Locked=true, Min=0.9 (tightly curled)
```

#### Step 4: Test and Refine
1. Enter Play mode
2. Grab the hammer
3. Check if grip looks natural
4. Adjust and repeat until satisfied

[PLACEHOLDER_GIF: Before/after of hammer grip configuration]

✅ **Result:** A hammer with a natural-looking power grip!

---

### How To: Create a Pointing Pose

> **Goal:** Object held with index finger extended (like a magic wand)
> **Time:** ~3 minutes

#### Finger Configuration
```
Thumb:  Locked=false, Min=0.3, Max=0.6 (relaxed)
Index:  Locked=true, Min=0 (straight - pointing)
Middle: Locked=true, Min=0.8 (curled around object)
Ring:   Locked=true, Min=0.8 (curled)
Pinky:  Locked=true, Min=0.8 (curled)
```

This creates a pose where the index finger stays extended while other fingers grip the object.

[PLACEHOLDER_SCREENSHOT: Hand in pointing pose holding wand]

---

### How To: Configure Two-Handed Objects

> **Goal:** Object that can be held differently by each hand
> **Time:** ~5 minutes

Some objects (like a rifle) need different hand poses for left and right grips.

#### Step 1: Configure Dominant Hand (Right)
Set up the main grip (e.g., on the trigger/handle):
```
Right Hand Positioning:
  Position: (0, -0.05, 0.1)  // Shifted to grip area
  Rotation: (0, 0, 0)

Right Pose Constraints:
  Index:  Locked=false, Min=0, Max=0.3 (trigger finger)
  Others: Locked=true, Min=0.8 (grip fingers)
```

#### Step 2: Configure Support Hand (Left)
Set up the forward grip (e.g., on the barrel/foregrip):
```
Left Hand Positioning:
  Position: (0, -0.02, 0.25)  // Forward on the object
  Rotation: (0, 90, 0)        // Rotated to grip angle

Left Pose Constraints:
  All fingers: Locked=true, Min=0.7 (support grip)
```

> 💡 **Note:** Two-handed grabbing requires additional setup on the Grabable component to allow both hands.

---

### How To: Hide Hands for Special Effects

> **Goal:** Hands disappear when grabbing a magical orb
> **Time:** ~1 minute

#### Configuration
1. Select the magical orb object
2. Find PoseConstrainer
3. Set **Constraint Type** to **HideHand**

That's it! When the player grabs the orb, their hand model will become invisible.

**Use cases:**
- Magic items that absorb the hand
- Portals or dimensional objects
- Ghost/spirit interactions
- Situations where hand clipping is unavoidable

---

### How To: Create Smooth Grab Transitions

> **Goal:** Hand smoothly animates to grab position for polish
> **Time:** ~1 minute

#### Configuration
1. Select your object
2. Find PoseConstrainer
3. Check **Use Smooth Transitions**
4. Set **Transition Speed** to desired value:
   - **5-8**: Slow, dramatic grabs
   - **10-15**: Natural, balanced (recommended)
   - **20+**: Quick, snappy grabs

[PLACEHOLDER_GIF: Comparison of instant vs smooth grab transition]

---

## Preset Configurations

### Power Grip (Hammer, Bat)
```
All Fingers: Locked=true, Min=0.85
Thumb: Locked=true, Min=0.6
Position: Centered on handle
```

### Pinch Grip (Pen, Small Objects)
```
Thumb: Locked=true, Min=0.4
Index: Locked=true, Min=0.5
Middle/Ring/Pinky: Locked=true, Min=0.3
Position: Near object tip
```

### Trigger Grip (Gun, Drill)
```
Index: Locked=false, Min=0, Max=0.3 (for trigger animation)
Others: Locked=true, Min=0.8
Thumb: Locked=true, Min=0.5
```

### Open Palm (Orb, Ball)
```
All Fingers: Locked=false, Min=0.2, Max=0.6
Position: Centered under object
```

### Relaxed Hold (Book, Tablet)
```
All Fingers: Locked=false, Min=0.3, Max=0.7
Thumb: Locked=false, Min=0.2, Max=0.5
```

---

## Tips & Best Practices

💡 **Test in VR, not just Scene view**
Hand poses feel different in VR. What looks good in the editor may feel wrong when you're actually holding it.

💡 **Start with locked fingers**
It's easier to create a static pose first, then unlock fingers that need to animate.

💡 **Use smooth transitions for important objects**
Key items like weapons, tools, and story objects benefit from polished grab animations.

💡 **Mirror poses when possible**
If left and right grips should be symmetric, copy values and flip X position/rotation.

💡 **Consider hand size variation**
Not all players have the same hand size. Test with different people if possible.

⚠️ **Common Mistake:** Setting Min > Max
If Min is greater than Max, the finger will behave unexpectedly. Always ensure Min ≤ Max.

⚠️ **Common Mistake:** Forgetting to configure both hands
If your object can be grabbed by either hand, configure both Left and Right settings.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Hand clips through object | Position offset wrong | Adjust Position Offset to move hand outside object |
| Fingers look unnatural | Curl values too extreme | Use values between 0.3-0.8 for natural poses |
| Hand snaps weirdly | Smooth transitions off | Enable Use Smooth Transitions |
| Only one hand works | Only configured one side | Configure both Left and Right pose constraints |
| Hand doesn't appear at all | Constraint Type = HideHand | Change to Constrained or FreeHand |
| Pose doesn't apply | Missing PoseConstrainer | Ensure component is attached and enabled |
| Grab feels laggy | Transition speed too low | Increase Transition Speed value |

---

## Technical Details

### How It Works

When an object is selected (grabbed):

1. **PoseConstrainer.ApplyConstraints()** is called
2. Based on Constraint Type:
   - **HideHand**: Hand renderer is disabled
   - **FreeHand**: No pose changes applied
   - **Constrained**: Hand moves to target position and pose is applied
3. If smooth transitions enabled, hand animates over time
4. Finger poses are applied via the hand's animation system
5. On deselect, **RemoveConstraints()** restores hand to default state

### Performance Considerations

- PoseConstrainer has minimal performance impact
- Smooth transitions add negligible overhead
- Finger animations are handled by Unity's animation system
- No per-frame allocations

### Compatibility

- **Unity Version:** 2021.3+
- **Required Components:** Automatically added with Grabable/ConstrainedInteractables
- **Works With:** All interactable types that inherit from InteractableBase

---

## Scripting API

### Key Properties

```csharp
// Get constraint type
HandConstrainType type = poseConstrainer.ConstraintType;

// Check if smooth transitions enabled
bool smooth = poseConstrainer.UseSmoothTransitions;

// Get transition speed
float speed = poseConstrainer.TransitionSpeed;

// Get pose constraints for a hand
PoseConstrains leftPose = poseConstrainer.LeftPoseConstrains;
PoseConstrains rightPose = poseConstrainer.RightPoseConstrains;
```

### Applying Constraints Manually

```csharp
// Apply constraints to a hand
poseConstrainer.ApplyConstraints(hand);

// Remove constraints
poseConstrainer.RemoveConstraints(hand);

// Get target hand transform
var (position, rotation) = poseConstrainer.GetTargetHandTransform(HandIdentifier.Right);
```

### Accessing Finger Constraints

```csharp
// Get constraints for specific finger
PoseConstrains pose = poseConstrainer.RightPoseConstrains;
FingerConstraints indexFinger = pose.indexFingerLimits;

// Check if finger is locked
bool isLocked = indexFinger.locked;

// Get min/max values
float min = indexFinger.min;
float max = indexFinger.max;

// Calculate constrained value from input (0-1)
float constrainedValue = indexFinger.GetConstrainedValue(inputValue);
```

---

## Related Documentation

- [Grabable](../Interactables/Grabable.md) — Uses PoseConstrainer for hand grips
- [Constrained Interactables](../Interactables/ConstrainedInteractables.md) — Lever, Drawer, etc.
- [Hand System](../Interactors/Hand.md) — Hand components and animation
- [Quick Start Guide](../GettingStarted/QuickStart.md) — Basic PoseConstrainer setup

---

**Last Updated:** January 2026
**Component Version:** 1.0.0

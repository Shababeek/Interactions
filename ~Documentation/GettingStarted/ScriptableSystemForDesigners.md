# Scriptable System — Designer's Guide

> **No coding required!** This guide shows how to connect interactive objects to visual feedback using drag-and-drop in the Inspector.

---

## The Big Picture

```
┌─────────────────┐      ┌──────────────┐      ┌─────────────────┐
│   INPUT         │ ──▶  │   VARIABLE   │ ──▶  │   OUTPUT        │
│   (Lever, etc.) │      │   (Asset)    │      │   (Binder)      │
└─────────────────┘      └──────────────┘      └─────────────────┘

   Player pulls         FloatVariable         Door rotates,
   a lever              stores 0-1            light dims, etc.
```

**Variables** are the glue between inputs and outputs. They're assets that live in your Project folder.

> **Naming note:** In ReactiveVars these output components are called **Binders**. In InteractionSystem, interaction-specific bridges are documented as **Drivers**.

---

## Example 1: Lever Controls a Door

> **Goal:** Pulling a lever opens a door

### What You Need

| Item | Type | Purpose |
|------|------|---------|
| Lever | LeverInteractable | Player input |
| FloatVariable | Asset | Stores lever position (0-1) |
| Door | GameObject | Visual output |
| NumericalRotationBinder | Component | Rotates door based on variable |

### Setup Steps

**1. Create the Variable**
- Right-click in Project → **Create > Shababeek > Scriptable System > Variables > FloatVariable**
- Name it "LeverValue"

**2. Connect the Lever**
- Select your Lever object
- Find the **On Value Changed** event
- Drag "LeverValue" asset to the event
- Select **FloatVariable > Value** (the setter)

```
Lever Inspector:
┌─────────────────────────────────────┐
│ On Value Changed (float)            │
│ ┌─────────────────────────────────┐ │
│ │ LeverValue    FloatVariable.Value│ │
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

**3. Connect the Door**
- Select your Door object
- Add Component → **Shababeek > Scriptable System > Numerical Rotation Binder**
- Drag "LeverValue" to the Variable field
- Set Min Angle: 0, Max Angle: 90

```
Door Inspector:
┌─────────────────────────────────────┐
│ Numerical Rotation Binder           │
│ ├─ Variable: [LeverValue]           │
│ ├─ Min Value: 0                     │
│ ├─ Max Value: 1                     │
│ ├─ Rotation Axis: Y                 │
│ ├─ Min Angle: 0                     │
│ ├─ Max Angle: 90                    │
│ └─ Smooth Rotation: ✓               │
└─────────────────────────────────────┘
```

**Result:** Lever at 0% = door closed. Lever at 100% = door open 90°.

---

## Example 2: Lever Controls Light Brightness

> **Goal:** Dimmer switch controls a light

### Setup

**1. Same LeverValue variable** (reuse it!)

**2. On the Light object:**
- Create a script-free setup using Animation

OR use a simple MonoBehaviour:

```
Light Inspector:
┌─────────────────────────────────────┐
│ Light Intensity Binder (custom)     │
│ └─ Variable: [LeverValue]           │
└─────────────────────────────────────┘
```

**Tip:** One variable can drive multiple things! The lever can open a door AND dim a light simultaneously.

---

## Example 3: Joystick Controls Character Direction

> **Goal:** VR joystick input moves a character

### What You Need

| Item | Type | Purpose |
|------|------|---------|
| Controller | XR Controller | Player input |
| Vector2Variable | Asset | Stores joystick XY |
| Character | GameObject | Has Rigidbody |
| RigidbodyVelocityBinder | Component | Moves character |

### Setup Steps

**1. Create the Variable**
- Create → **Vector2Variable** named "MoveInput"

**2. Connect the Joystick**
- On your input handler, output joystick values to "MoveInput"

```
Input Handler:
┌─────────────────────────────────────┐
│ On Joystick Move (Vector2)          │
│ └─ MoveInput    Vector2Variable.Value│
└─────────────────────────────────────┘
```

**3. Connect to Character**
- Add **Rigidbody Velocity Binder** to character
- Set Physics Mode: Rigidbody2D (or 3D)
- Drag "MoveInput" to Vector2 Variable
- Set Multiplier: 5 (movement speed)

```
Character Inspector:
┌─────────────────────────────────────┐
│ Rigidbody Velocity Binder           │
│ ├─ Physics Mode: Rigidbody2D        │
│ ├─ Application Mode: Velocity       │
│ ├─ Vector2 Variable: [MoveInput]    │
│ ├─ Multiplier: 5                    │
│ └─ Continuous: ✓                    │
└─────────────────────────────────────┘
```

---

## Example 4: Health Bar UI

> **Goal:** Enemy health displays on a UI bar

### What You Need

| Item | Type | Purpose |
|------|------|---------|
| IntVariable | Asset | Stores health (0-100) |
| UI Image | Filled type | Visual bar |
| NumericalFillBinder | Component | Updates fill amount |
| ColorVariable | Asset | Bar color |
| ColorImageBinder | Component | Updates color |

### Setup Steps

**1. Create Variables**
- Create **IntVariable** named "EnemyHealth" (default: 100)
- Create **ColorVariable** named "HealthBarColor"

**2. Enemy Script writes to variable**
```
Enemy takes damage → EnemyHealth.Value -= damage
```

**3. UI Image setup**
- Set Image Type: **Filled**
- Add **Numerical Fill Binder**:
  - Variable: EnemyHealth
  - Min Value: 0
  - Max Value: 100

**4. Color gradient (optional)**
- Add **Color Image Binder**:
  - Color Variable: HealthBarColor
  - Smooth Transition: ✓

```
Health Bar Inspector:
┌─────────────────────────────────────┐
│ Image (Type: Filled)                │
├─────────────────────────────────────┤
│ Numerical Fill Binder               │
│ ├─ Variable: [EnemyHealth]          │
│ ├─ Min Value: 0                     │
│ ├─ Max Value: 100                   │
│ └─ Smooth Fill: ✓                   │
├─────────────────────────────────────┤
│ Color Image Binder                  │
│ ├─ Color Variable: [HealthBarColor] │
│ └─ Smooth Transition: ✓             │
└─────────────────────────────────────┘
```

---

## Example 5: Drawer Opens Panel

> **Goal:** Pulling a drawer slides a UI panel

### Setup

**1. Create FloatVariable** "DrawerValue"

**2. Connect Drawer**
- Drawer's On Value Changed → DrawerValue.Value

**3. UI Panel with Numerical Position Binder**
- Variable: DrawerValue
- Start Position: (off-screen)
- End Position: (on-screen)
- Use Curve for easing

```
UI Panel Inspector:
┌─────────────────────────────────────┐
│ Numerical Position Binder           │
│ ├─ Variable: [DrawerValue]          │
│ ├─ Start Position: (-500, 0, 0)     │
│ ├─ End Position: (0, 0, 0)          │
│ ├─ Min Value: 0                     │
│ ├─ Max Value: 1                     │
│ └─ Curve: [EaseOutBack]             │
└─────────────────────────────────────┘
```

---

## Example 6: Dial Controls Gauge

> **Goal:** Rotating a dial moves a gauge needle

### Setup

**1. Create FloatVariable** "DialValue"

**2. Dial Interactable**
- On Value Changed → DialValue.Value

**3. Gauge Needle with Numerical Rotation Binder**
- Variable: DialValue
- Min Angle: -120
- Max Angle: 120
- Rotation Axis: Z

Both the dial and needle rotate in sync!

---

## Example 7: Button Changes Color Theme

> **Goal:** Pressing a button changes UI colors

### Setup

**1. Create ColorVariable** "ThemeColor"

**2. Button**
- On Click → ThemeColor.Value = [new color]

**3. Multiple UI elements with Color Image Binder**
- All reference the same ThemeColor
- When button clicked, everything updates!

```
Multiple Objects:
┌────────────┐  ┌────────────┐  ┌────────────┐
│ Header     │  │ Button BG  │  │ Panel      │
│ ColorImage │  │ ColorImage │  │ ColorImage │
│ Binder     │  │ Binder     │  │ Binder     │
│    ↓       │  │    ↓       │  │    ↓       │
└────────────┘  └────────────┘  └────────────┘
       │              │              │
       └──────────────┼──────────────┘
                      ▼
              [ThemeColor]
```

---

## Variable Container — Organize Related Variables

Group related variables in one asset:

**1. Create Variable Container**
- Create → **Variable Container** named "PlayerStats"

**2. Add Variables**
- Click + to add: Health (Int), Stamina (Float), Speed (Float)

**3. Access in Binders**
- Reference individual variables from the container
- Or access via code: `playerStats.Get<IntVariable>("Health")`

```
PlayerStats Container:
┌─────────────────────────────────────┐
│ Variables                           │
│ ├─ Health      [Int]     100        │
│ ├─ Stamina     [Float]   100.0      │
│ ├─ Speed       [Float]   5.0        │
│ └─ PlayerName  [Text]    "Hero"     │
└─────────────────────────────────────┘
```

---

## Quick Reference: What Binder Do I Need?

| I want to change... | Use this Binder | Variable Type |
|---------------------|-----------------|---------------|
| Object rotation | NumericalRotationBinder | Int/Float |
| Object position | TransformBinder | Vector3 |
| Object between 2 positions | NumericalPositionBinder | Int/Float |
| Object scale | TransformBinder | Vector3/Float |
| UI fill amount | NumericalFillBinder | Int/Float |
| Sprite color | ColorSpriteBinder | Color |
| UI Image color | ColorImageBinder | Color |
| Text color | ColorTextMeshProBinder | Color |
| Text content | TextMeshProBinder | Any |
| Rigidbody velocity | RigidbodyVelocityBinder | Vector2/Vector3 |
| Object facing direction | TransformBinder (Direction2D) | Vector2 |

---

## Quick Reference: Common Interactable Outputs

| Interactable | Output Event | Variable Type |
|--------------|--------------|---------------|
| Lever | On Value Changed | Float (0-1) |
| Drawer | On Value Changed | Float (0-1) |
| Dial/Knob | On Value Changed | Float (0-1) |
| Button | On Click | — (use Event) |
| Grabable | On Selected/Deselected | Bool |
| Joystick | On Value Changed | Vector2 |

---

## Tips for Designers

💡 **One Variable, Many Outputs**
A single variable can drive multiple binders. One lever can open a door, play a sound, and change lighting.

💡 **Reuse Variables**
If two things should stay in sync, they should use the same variable.

💡 **Name Variables Clearly**
Use names like "MainDoorOpen" not "var1". You'll thank yourself later.

💡 **Test with Inspector**
Select a variable asset and change its value in the Inspector to test binders without playing.

💡 **Check Scene References**
Select a variable to see all objects using it in the current scene.

💡 **Use Smooth Interpolation**
Enable smooth/lerp options on binders for polished visuals.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Nothing happens when I move the lever | Check that On Value Changed event is connected to the variable |
| Binder doesn't update | Make sure variable is assigned in the binder |
| Fill bar doesn't work | Set Image Type to "Filled" |
| Wrong rotation axis | Change Rotation Axis in binder settings |
| Movement is jerky | Enable Smooth option on the binder |
| Can't find my variable | Use the search bar in the object picker |

---

## Summary Workflow

```
1. CREATE variable asset (FloatVariable, etc.)
        ↓
2. CONNECT interactable's event to variable
   (On Value Changed → Variable.Value)
        ↓
3. ADD binder component to output object
        ↓
4. ASSIGN same variable to binder
        ↓
5. CONFIGURE binder settings (min/max, axis, etc.)
        ↓
6. TEST and tweak!
```

---

**Last Updated:** January 2026

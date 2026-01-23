# Binders — Connect Variables to Components

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Scriptable System > [BinderName]
> **Use For:** Automatically updating components when variables change
> **Requires:** UniRx, ScriptableVariable

---

## What It Does

**Binders** sync ScriptableVariable values to Unity components automatically. When the variable changes, the bound component updates — no code required.

---

## Available Binders

### Transform & Position Binders

| Binder | Variable Type | Updates |
|--------|--------------|---------|
| **TransformBinder** | Vector3/Quaternion/Vector2 | Position, Rotation, Scale |
| **FloatLerpPositionBinder** | Float (0-1) | Position between two points |
| **Vector2SpaceBinder** | Vector2 | Position within bounded space |

### Physics Binders

| Binder | Variable Type | Updates |
|--------|--------------|---------|
| **Rigidbody3DBinder** | Vector3/Vector2/Float | 3D Rigidbody velocity/forces |
| **Rigidbody2DBinder** | Vector2/Float | 2D Rigidbody velocity/forces |
| **AngularVelocityBinder** | Vector3/Vector2/Float | Angular velocity/torque |

### UI & Visual Binders

| Binder | Variable Type | Updates |
|--------|--------------|---------|
| **TextMeshProBinder** | Any | TMP_Text content |
| **ColorTextMeshProBinder** | Color | TMP_Text color |
| **NumericalFillBinder** | Int/Float | Image fillAmount |
| **NumericalRotationBinder** | Int/Float | Transform rotation |
| **NumericalPositionBinder** | Int/Float | Position between two points |
| **ColorSpriteBinder** | Color | SpriteRenderer color |
| **ColorImageBinder** | Color | UI Image color |
| **AudioSourceBinder** | Audio | AudioSource settings |

### Interactable-to-Variable Binders

| Binder | Source | Outputs |
|--------|--------|---------|
| **LeverToVariableBinder** | LeverInteractable | Float (normalized, angle) |
| **WheelToVariableBinder** | WheelInteractable | Float (normalized, angle) |
| **JoystickToVariableBinder** | JoystickInteractable | Vector2, Float (x, y) |
| **DrawerToVariableBinder** | DrawerInteractable | Float, Bool, Events |
| **InteractableEventBinder** | InteractableBase | GameEvents, BoolVariables |

---

## Transform Binder

Unified binder for position, rotation, and scale.

### Position Settings

| Setting | Description |
|---------|-------------|
| **Bind Position** | Enable position binding |
| **Position Variable** | Vector3Variable to follow |
| **Use Local Position** | Local vs world space |
| **Position Offset** | Offset from variable |

### Rotation Settings

| Setting | Description |
|---------|-------------|
| **Bind Rotation** | Enable rotation binding |
| **Rotation Mode** | Euler, Quaternion, or Direction2D |
| **Euler Variable** | Vector3Variable for euler angles |
| **Quaternion Variable** | QuaternionVariable for rotation |
| **Direction Variable** | Vector2Variable for 2D direction |
| **Direction Plane** | XY, XZ, or YZ |
| **Angle Offset** | Additional angle offset |

### Scale Settings

| Setting | Description |
|---------|-------------|
| **Bind Scale** | Enable scale binding |
| **Scale Mode** | Vector3 or Uniform |
| **Scale Vector** | Vector3Variable for scale |
| **Uniform Scale** | Int/Float for uniform scale |
| **Base Scale** | Multiplier for uniform mode |

### Interpolation

| Setting | Description |
|---------|-------------|
| **Smooth** | Enable interpolation |
| **Speed** | Interpolation speed |

---

## Rigidbody 3D Binder

Binds various input types to 3D Rigidbody velocity or forces. Replaces the old RigidbodyVelocityBinder with more flexible input modes.

### Input Modes

| Mode | Variables | Description |
|------|-----------|-------------|
| **Vector3** | Vector3Variable | Direct 3D velocity |
| **Vector2XY** | Vector2Variable | X→X, Y→Y (Z=0) |
| **Vector2XZ** | Vector2Variable | X→X, Y→Z (Y=0) — common for ground movement |
| **Vector2YZ** | Vector2Variable | X→Y, Y→Z (X=0) |
| **Vector2PlusFloat** | Vector2Variable + FloatVariable | XZ plane from Vector2, Y from Float |
| **FloatDirection** | FloatVariable + Direction | Float magnitude along specified direction |

### Application Modes

| Mode | Description |
|------|-------------|
| **SetVelocity** | Directly sets rb.linearVelocity |
| **AddForce** | Adds force (ForceMode.Force) |
| **AddAcceleration** | Adds acceleration (ForceMode.Acceleration) |

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Rigidbody** | Target Rigidbody | Auto-detect |
| **Input Mode** | How to compose velocity | Vector3 |
| **Application Mode** | How to apply to rigidbody | SetVelocity |
| **Use Local Space** | Transform direction to local space | false |
| **Multiplier** | Scale the velocity | 1.0 |
| **Continuous** | Apply every FixedUpdate | true |

### Example

```csharp
// Character controller with separate horizontal and vertical movement
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private Vector2Variable moveInput;    // From joystick
    [SerializeField] private FloatVariable verticalInput;  // From jump/fly

    void Update()
    {
        moveInput.Value = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"));

        verticalInput.Value = Input.GetKey(KeyCode.Space) ? 5f : 0f;
    }
}
// Configure Rigidbody3DBinder with Vector2PlusFloat mode
```

---

## Rigidbody 2D Binder

Binds variables to Rigidbody2D velocity or forces.

### Input Modes

| Mode | Variables | Description |
|------|-----------|-------------|
| **Vector2** | Vector2Variable | Direct 2D velocity |
| **TwoFloats** | FloatVariable x2 | Separate X and Y floats |
| **FloatDirection** | FloatVariable + Direction | Float magnitude along direction |

### Application Modes

| Mode | Description |
|------|-------------|
| **SetVelocity** | Directly sets rb.linearVelocity |
| **AddForce** | Adds force (ForceMode2D.Force) |
| **AddForceImpulse** | Adds impulse (ForceMode2D.Impulse) |

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Rigidbody 2D** | Target Rigidbody2D | Auto-detect |
| **Input Mode** | How to compose velocity | Vector2 |
| **Application Mode** | How to apply to rigidbody | SetVelocity |
| **Use Local Space** | Transform to local space | false |
| **Multiplier** | Scale the velocity | 1.0 |
| **Continuous** | Apply every FixedUpdate | true |

---

## Angular Velocity Binder

Binds variables to Rigidbody angular velocity or applies as torque. Works with both 3D and 2D rigidbodies.

### Input Modes

| Mode | Variables | Description |
|------|-----------|-------------|
| **Vector3** | Vector3Variable | Full 3D angular velocity |
| **Vector2XY** | Vector2Variable | X→X, Y→Y rotation |
| **Vector2XZ** | Vector2Variable | X→X, Y→Z rotation |
| **FloatSingleAxis** | FloatVariable + Axis | Single axis rotation |

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Rigidbody 3D** | Target 3D Rigidbody | Auto-detect |
| **Rigidbody 2D** | Target 2D Rigidbody | Auto-detect |
| **Input Mode** | How to compose angular velocity | Vector3 |
| **Rotation Axis** | Axis for FloatSingleAxis mode | Up |
| **Set Velocity** | true=set velocity, false=add torque | true |
| **Use Local Space** | Transform to local space | false |
| **Multiplier** | Scale the angular velocity | 1.0 |
| **Continuous** | Apply every FixedUpdate | true |

---

## Float Lerp Position Binder

Moves an object between two positions based on a float value (0-1). Supports direct, velocity-based, and smooth interpolation modes.

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Float Input** | FloatVariable (0-1 range) | None |
| **Target** | Transform to move | Self |
| **Use Local Space** | Local vs world space | true |
| **Start Position** | Position at value 0 | (0,0,0) |
| **End Position** | Position at value 1 | (0,0,1) |

### Movement Modes

| Mode | Description |
|------|-------------|
| **Direct** | Instant position update |
| **Velocity** | Move at fixed speed toward target |
| **SmoothDamp** | Smooth interpolation to target |

### Additional Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Velocity Speed** | Speed for Velocity mode | 2.0 |
| **Smooth Time** | Time for SmoothDamp mode | 0.1 |
| **Easing Curve** | Animation curve for easing | Linear |

### API

```csharp
binder.SetStartPosition(Vector3 pos);
binder.SetEndPosition(Vector3 pos);
binder.SetPositions(Vector3 start, Vector3 end);
binder.SetValue(float value);
binder.SnapToTarget();  // Instantly move to target
```

---

## Vector2 Space Binder

Maps a Vector2 input to object movement within bounded space (rectangle or circle). Ideal for joystick-to-cursor or joystick-to-object mapping.

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Input Variable** | Vector2Variable (-1 to 1) | None |
| **Target** | Transform to move | Self |
| **Center Position** | Center of movement area | (0,0,0) |
| **Use Local Space** | Local vs world space | true |

### Bounds Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Bounds Type** | Rectangle or Circle | Rectangle |
| **Rectangle Size** | Width and height for Rectangle | (1,1) |
| **Circle Radius** | Radius for Circle | 0.5 |
| **Plane** | XY, XZ, or YZ | XY |

### Movement Modes

| Mode | Description |
|------|-------------|
| **Direct** | Input maps directly to position |
| **Velocity** | Input acts as velocity within bounds |
| **SmoothDamp** | Smooth interpolation to target |

### Additional Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Velocity Multiplier** | Speed for Velocity mode | 5.0 |
| **Smooth Time** | Time for SmoothDamp mode | 0.1 |

### API

```csharp
binder.SetCenter(Vector3 center);
binder.ResetToCenter();  // Return to center position
```

### Gizmos

When selected, draws the bounds area (rectangle or circle) in the scene view.

---

## Interactable-to-Variable Binders

These binders connect VR interactable outputs to scriptable variables, enabling the Scriptable System to react to user interactions.

### Lever To Variable Binder

Binds a LeverInteractable's output to float variables.

| Setting | Description |
|---------|-------------|
| **Lever** | Source LeverInteractable |
| **Normalized Output** | FloatVariable for 0-1 value |
| **Angle Output** | FloatVariable for actual angle |
| **Invert Output** | Flip the output direction |
| **Output Multiplier** | Scale the output values |

### Wheel To Variable Binder

Binds a WheelInteractable's output to float variables.

| Setting | Description |
|---------|-------------|
| **Wheel** | Source WheelInteractable |
| **Normalized Output** | FloatVariable for -1 to 1 value |
| **Angle Output** | FloatVariable for actual angle |
| **Invert Output** | Flip the output direction |
| **Output Multiplier** | Scale the output values |

### Joystick To Variable Binder

Binds a JoystickInteractable's output to Vector2 or separate float variables.

| Setting | Description |
|---------|-------------|
| **Joystick** | Source JoystickInteractable |
| **Vector2 Output** | Vector2Variable for combined output |
| **X Output** | FloatVariable for X axis only |
| **Y Output** | FloatVariable for Y axis only |
| **Invert X/Y** | Flip individual axes |
| **Deadzone** | Minimum input magnitude | 0.1 |
| **Output Multiplier** | Scale the output values |

### Drawer To Variable Binder

Binds a DrawerInteractable's output to variables and events.

| Setting | Description |
|---------|-------------|
| **Drawer** | Source DrawerInteractable |
| **Position Output** | FloatVariable for 0-1 position |
| **Is Open Output** | BoolVariable for open state |
| **On Opened Event** | GameEvent raised when opened |
| **On Closed Event** | GameEvent raised when closed |
| **Invert Output** | Flip position direction |
| **Open Threshold** | Position considered "open" | 0.9 |

### Interactable Event Binder

Binds any InteractableBase's interaction events to GameEvents and BoolVariables.

| Setting | Description |
|---------|-------------|
| **Interactable** | Source InteractableBase |
| **On Selected Event** | GameEvent for selection |
| **On Deselected Event** | GameEvent for deselection |
| **On Hover Start Event** | GameEvent for hover start |
| **On Hover End Event** | GameEvent for hover end |
| **On Use Start Event** | GameEvent for use start |
| **On Use End Event** | GameEvent for use end |
| **Is Selected Variable** | BoolVariable tracking selection |
| **Is Hovered Variable** | BoolVariable tracking hover |
| **Is Using Variable** | BoolVariable tracking use |

---

## Numerical Position Binder

Moves object between two positions based on a numeric value.

### Settings

| Setting | Description |
|---------|-------------|
| **Variable** | Int/Float variable |
| **Start Position** | Position at min value |
| **End Position** | Position at max value |
| **Min Value** | Value mapping to start |
| **Max Value** | Value mapping to end |
| **Curve** | Animation curve for easing |

### Context Menu

- **Set Start Position** — Save current position as start
- **Set End Position** — Save current position as end
- **Preview Start** — Move to start position
- **Preview End** — Move to end position

---

## Color TextMeshPro Binder

Binds a ColorVariable to TextMeshPro text color.

### Settings

| Setting | Description |
|---------|-------------|
| **Color Variable** | ColorVariable to bind |
| **Text Component** | TMP_Text target |
| **Smooth** | Enable interpolation |
| **Speed** | Color lerp speed |
| **Include Alpha** | Update alpha channel |

---

## Numerical Fill Binder

Binds any numeric variable to a UI Image's fill amount.

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Variable** | Int or Float variable | None |
| **Min Value** | Maps to 0% fill | 0 |
| **Max Value** | Maps to 100% fill | 100 |
| **Invert Fill** | Reverse direction | false |
| **Smooth Fill** | Animate changes | false |

---

## Numerical Rotation Binder

Maps numeric value to rotation angle.

### Settings

| Setting | Description | Default |
|---------|-------------|Her
| **Variable** | Int or Float | None |
| **Min/Max Value** | Value range | 0-100 |
| **Min/Max Angle** | Angle range | 0-360 |
| **Rotation Axis** | X, Y, or Z | Z |
| **Use Shortest Path** | Angle interpolation | true |

---

## Color Sprite Binder

Binds ColorVariable to SpriteRenderer color.

### Settings

| Setting | Description |
|---------|-------------|
| **Color Variable** | ColorVariable |
| **Smooth Transition** | Animate changes |
| **Transition Speed** | Lerp speed |
| **Include Alpha** | Update alpha |

---

## Color Image Binder

Same as Color Sprite Binder but for UI Image.

---

## Scene References

When you select any ScriptableVariable or GameEvent asset, the inspector shows a **Scene References** panel listing all components in open scenes that reference it.

- Click **→** to select and ping the GameObject
- Click **Refresh** to rescan
- Shows component path and type

---

## Tips

💡 **Use TransformBinder** — Unified component for all transform needs.

💡 **Separate 2D and 3D** — Use Rigidbody2DBinder for 2D games, Rigidbody3DBinder for 3D.

💡 **Input Modes** — Choose the right input mode for your control scheme. Vector2XZ is common for ground movement.

💡 **Continuous vs Manual** — Set `continuous=false` and call `Apply()` manually for precise control.

💡 **Animation Curves** — NumericalPositionBinder and FloatLerpPositionBinder support curves for easing.

💡 **Interactable Binders** — Use these to connect VR interactions to the Scriptable System without custom code.

⚠️ **Filled Image** — NumericalFillBinder requires Image.Type = Filled.

⚠️ **Physics Timing** — Physics binders apply in FixedUpdate. For manual control, disable continuous mode.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Transform not updating | Binding disabled | Enable the specific bind toggle |
| 2D rotation wrong | Wrong plane | Change Direction Plane |
| Rigidbody jittery | Wrong timing | Use continuous mode |
| Position snaps | No smoothing | Enable Smooth option |
| No response to interactable | Missing reference | Assign the interactable in inspector |
| Physics not applying | Wrong rigidbody | Check 2D vs 3D binder |
| Bounded movement off-center | Wrong center | Set Center Position correctly |

---

**Last Updated:** January 2026
**Component Version:** 1.2.0

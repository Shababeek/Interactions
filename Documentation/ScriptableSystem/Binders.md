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
| **NumericalRotationBinder** | Int/Float | Transform rotation (direct mapping) |
| **NumericalRotationSpeedBinder** | Int/Float | Rotation speed (continuous) |
| **NumericalPositionBinder** | Int/Float | Position between two points (direct) |
| **NumericalPositionSpeedBinder** | Int/Float | Movement speed between two points |
| **NumericalScaleBinder** | Int/Float | Transform scale |
| **NumericalMaterialBinder** | Int/Float | Shader property values |
| **ColorSpriteBinder** | Color | SpriteRenderer color |
| **ColorImageBinder** | Color | UI Image color |
| **AudioSourceBinder** | Audio | AudioSource settings |
| **SliderBinder** | Int/Float | UI Slider value (bidirectional) |
| **CanvasGroupBinder** | Float/Bool | CanvasGroup alpha, interactability |
| **LightBinder** | Float/Color/Bool | Light intensity, color, range, enabled |
| **CameraBinder** | Float/Color/Bool/Vector2 | FOV, ortho size, clip planes, background |

### State & Toggle Binders

| Binder | Variable Type | Updates |
|--------|--------------|---------|
| **BoolToggleBinder** | Bool | Enable/disable GameObjects, Components |
| **AnimatorBinder** | Bool/Float/Int/Event | Animator parameters (comprehensive) |
| **EventAnimatorBinder** | GameEvent | Animator parameters (event-based) |

### Interactable-to-Variable Binders

| Binder | Source | Outputs |
|--------|--------|---------|
| **LeverToVariableBinder** | LeverInteractable | Float (normalized, angle) |
| **WheelToVariableBinder** | WheelInteractable | Float (normalized, angle) |
| **DialToVariableBinder** | DialInteractable | Int (step), Float (normalized, angle) |
| **JoystickToVariableBinder** | JoystickInteractable | Vector2, Float (x, y) |
| **DrawerToVariableBinder** | DrawerInteractable | Float, Bool, Events |
| **InteractableEventBinder** | InteractableBase | GameEvents, BoolVariables |
| **InteractableToBoolBinder** | InteractableBase | BoolVariables for Hovered/Selected/Used |
| **InteractableToEventBinder** | InteractableBase | GameEvents for all interaction events |

### Socket-to-Variable Binders

| Binder | Source | Outputs |
|--------|--------|---------|
| **SocketToBoolBinder** | Socket | BoolVariable for HasObject, GameEvents |
| **SocketToEventBinder** | Socket | GameEvents for Insert/Remove |
| **SocketableToBoolBinder** | Socketable | BoolVariable for IsSocketed |
| **SocketableToEventBinder** | Socketable | GameEvents for Socketed/Unsocketed |

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

### Dial To Variable Binder

Binds a DialInteractable's discrete step output to variables and events.

| Setting | Description |
|---------|-------------|
| **Step Variable** | IntVariable for current step (0-based) |
| **Normalized Variable** | FloatVariable for 0-1 value |
| **Angle Variable** | FloatVariable for current angle |
| **On Step Changed Event** | GameEvent raised when step changes |
| **On Step Confirmed Event** | GameEvent raised when step is confirmed (on release) |
| **Step Events** | Array of GameEvents for specific steps |

**Features:**
- Outputs step index as integer
- Normalized value (0-1) for progress indicators
- Per-step events for specific actions (e.g., step 0 = Event A, step 1 = Event B)

**Use cases:**
- Combination locks (each step is a digit)
- Rotary selectors (mode selection)
- Safe dials with discrete positions

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

### Interactable To Bool Binder

Simplified binder focusing only on BoolVariables for interaction states.

| Setting | Description |
|---------|-------------|
| **Hovered Variable** | BoolVariable set true when hovered |
| **Selected Variable** | BoolVariable set true when selected/grabbed |
| **Used Variable** | BoolVariable set true during use |
| **Reset On Disable** | Reset all to false when disabled |

**Use cases:**
- UI indicators showing interaction state
- Enabling features based on selection
- Analytics tracking of interactions

### Interactable To Event Binder

Simplified binder focusing only on GameEvents for interaction events.

| Setting | Description |
|---------|-------------|
| **On Hover Start Event** | GameEvent raised when hover starts |
| **On Hover End Event** | GameEvent raised when hover ends |
| **On Selected Event** | GameEvent raised when selected |
| **On Deselected Event** | GameEvent raised when deselected |
| **On Use Start Event** | GameEvent raised when use starts |
| **On Use End Event** | GameEvent raised when use ends |

**Use cases:**
- Triggering game logic when objects are grabbed
- Playing effects through event listeners
- Updating UI/analytics when interactions occur

---

## Socket Binders

These binders connect Socket System events to the Scriptable System.

### Socket To Bool Binder

Binds a Socket's state to BoolVariables and GameEvents.

| Setting | Description |
|---------|-------------|
| **Socket** | Source Socket component |
| **Has Object Variable** | BoolVariable true when socket contains object |
| **On Inserted Event** | GameEvent raised when object inserted |
| **On Removed Event** | GameEvent raised when object removed |

**Use cases:**
- Light indicator when slot is filled
- Trigger puzzle logic when all sockets filled
- Play sound effects on socket events

### Socket To Event Binder

Simplified version focusing only on GameEvents.

| Setting | Description |
|---------|-------------|
| **Socket** | Source Socket component |
| **On Inserted Event** | GameEvent raised on insert |
| **On Removed Event** | GameEvent raised on remove |

### Socketable To Bool Binder

Binds a Socketable object's state to BoolVariable.

| Setting | Description |
|---------|-------------|
| **Socketable** | Source Socketable component |
| **Is Socketed Variable** | BoolVariable true when in a socket |
| **On Socketed Event** | GameEvent raised when socketed |
| **On Unsocketed Event** | GameEvent raised when unsocketed |

**Use cases:**
- Battery shows "installed" indicator
- Key disappears when inserted in lock
- Tool shows different state when holstered

### Socketable To Event Binder

Simplified version focusing only on GameEvents.

| Setting | Description |
|---------|-------------|
| **Socketable** | Source Socketable component |
| **On Socketed Event** | GameEvent raised when socketed |
| **On Unsocketed Event** | GameEvent raised when unsocketed |

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

Maps numeric value directly to rotation angle.

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Variable** | Int or Float | None |
| **Min/Max Value** | Value range | 0-100 |
| **Min/Max Angle** | Angle range | 0-360 |
| **Rotation Axis** | X, Y, or Z | Z |
| **Use Shortest Path** | Angle interpolation | true |

---

## Numerical Rotation Speed Binder

Maps numeric value to rotation **speed** instead of direct angle. Perfect for continuous rotation controlled by input.

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Variable** | Int or Float | None |
| **Min Value** | Maps to max negative speed | -1 |
| **Max Value** | Maps to max positive speed | 1 |
| **Max Speed** | Rotation speed in degrees/sec | 180 |
| **Dead Zone** | Values within this range = no rotation | 0.01 |
| **Rotation Axis** | X, Y, or Z | Z |
| **Use Local Rotation** | Local vs world space | true |
| **Limit Rotation** | Enable min/max angle limits | false |
| **Min Angle** | Minimum angle (if limited) | -180 |
| **Max Angle** | Maximum angle (if limited) | 180 |
| **Smooth Acceleration** | Gradual speed changes | false |
| **Acceleration Rate** | Speed change rate | 10 |

### Use Cases

- **Steering wheel**: Input controls rotation speed, not position
- **Rotating platforms**: Hold button = platform rotates
- **Camera turret**: Joystick controls rotation speed
- **Valve handles**: Continuous turning while held

### Example

```csharp
// Joystick X controls rotation speed
joystickVariable.Value = Input.GetAxis("Horizontal");
// Object rotates left/right based on input
// Release = rotation stops (value returns to 0)
```

---

## Numerical Position Speed Binder

Maps numeric value to movement **speed** between two positions. Unlike NumericalPositionBinder which maps value to position directly, this maps value to velocity.

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Variable** | Int or Float | None |
| **Start Position** | Position at T=0 | Vector3.zero |
| **End Position** | Position at T=1 | Vector3.zero |
| **Use Local Position** | Local vs world space | true |
| **Min Value** | Maps to max speed toward start | -1 |
| **Max Value** | Maps to max speed toward end | 1 |
| **Max Speed** | Movement speed in units/sec | 2 |
| **Dead Zone** | Values within this = no movement | 0.01 |
| **Clamp To Endpoints** | Stop at start/end (vs wrap) | true |
| **Smooth Acceleration** | Gradual speed changes | false |
| **Acceleration Rate** | Speed change rate | 10 |

### Context Menu

- **Set Start Position** — Save current position as start
- **Set End Position** — Save current position as end
- **Preview Start** — Move to start position
- **Preview End** — Move to end position

### Use Cases

- **Sliding doors**: Button held = door moves, released = stops
- **Elevator platforms**: Up/down input controls movement
- **Conveyor belts**: Speed control
- **Throttle-controlled mechanisms**

### Public API

```csharp
binder.CurrentSpeed     // Current speed in units/sec
binder.CurrentT         // Position as 0-1 value
binder.CurrentPosition  // Current world/local position
binder.SetPositionImmediate(float t)  // Jump to position
binder.GoToStart()      // Jump to start
binder.GoToEnd()        // Jump to end
binder.GoToCenter()     // Jump to center
```

---

## Numerical Scale Binder

Maps numeric value to object scale.

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Variable** | Int or Float | None |
| **Min Value** | Value for min scale | 0 |
| **Max Value** | Value for max scale | 1 |
| **Scale Mode** | Uniform, PerAxis, XOnly, YOnly, ZOnly | Uniform |
| **Min Scale** | Scale at min value | (0.5, 0.5, 0.5) |
| **Max Scale** | Scale at max value | (1, 1, 1) |
| **Smooth** | Enable interpolation | false |
| **Speed** | Interpolation speed | 5 |
| **Curve** | Animation curve | Linear |

### Scale Modes

| Mode | Description |
|------|-------------|
| **Uniform** | All axes scale equally |
| **PerAxis** | Each axis interpolates independently |
| **XOnly** | Only X axis scales |
| **YOnly** | Only Y axis scales |
| **ZOnly** | Only Z axis scales |

---

## Numerical Material Binder

Maps numeric value to shader properties (float, color, vector).

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Variable** | Int or Float | None |
| **Renderer** | Target Renderer | Auto-detect |
| **Material Index** | Which material to modify | 0 |
| **Property Name** | Shader property name | "_Intensity" |
| **Property Type** | Float, Color, or Vector | Float |
| **Min Value** | Input value for min output | 0 |
| **Max Value** | Input value for max output | 1 |

#### For Float Properties
| **Min Float** | Output at min value | 0 |
| **Max Float** | Output at max value | 1 |

#### For Color Properties
| **Min Color** | Color at min value | Black |
| **Max Color** | Color at max value | White |

#### For Vector Properties
| **Min Vector** | Vector at min value | (0,0,0,0) |
| **Max Vector** | Vector at max value | (1,1,1,1) |

### Use Cases

- **Dissolve effects**: Value controls dissolve amount
- **Emission intensity**: Health controls glow
- **Color transitions**: Temperature controls material color
- **Shader animations**: Script-driven visual effects

---

## Slider Binder

Binds a numeric variable to a UI Slider with bidirectional support.

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Variable** | Int or Float variable | None |
| **Binding Mode** | OneWayToSlider, OneWayToVariable, TwoWay | TwoWay |
| **Use Value Mapping** | Map variable range to slider range | false |
| **Min Variable Value** | Variable value for slider min | 0 |
| **Max Variable Value** | Variable value for slider max | 100 |
| **Round To Int** | Round values to whole numbers | false |

### Binding Modes

| Mode | Description |
|------|-------------|
| **OneWayToSlider** | Variable changes update slider only |
| **OneWayToVariable** | Slider changes update variable only |
| **TwoWay** | Both directions sync |

### Use Cases

- **Volume controls**: Float variable bound to volume slider
- **Settings panels**: Variables driving UI sliders
- **Progress indicators**: Read-only display of variable values

---

## Canvas Group Binder

Binds variables to CanvasGroup properties (alpha, interactability, raycasts).

### Settings

| Setting | Description |
|---------|-------------|
| **Alpha Variable** | Float variable for alpha (0-1) |
| **Use Alpha Mapping** | Map variable range to 0-1 |
| **Smooth Alpha** | Animate alpha changes |
| **Alpha Speed** | Animation speed |
| **Interactable Variable** | Bool for interactability |
| **Blocks Raycasts Variable** | Bool for blocking raycasts |
| **Ignore Parent Groups Variable** | Bool for ignoring parent |

### Methods

| Method | Description |
|--------|-------------|
| `FadeIn()` | Animate to alpha 1 |
| `FadeOut()` | Animate to alpha 0 |
| `Show()` | Full visibility + interactable |
| `Hide()` | Invisible + non-interactable |

### Use Cases

- **UI panels**: Fade in/out based on game state
- **Menu visibility**: Hide/show menus with variables
- **Loading screens**: Alpha controlled by progress

---

## Light Binder

Binds variables to Light component properties.

### Settings

#### Intensity Binding
| Setting | Description |
|---------|-------------|
| **Intensity Variable** | Float for intensity |
| **Use Intensity Mapping** | Map variable range to intensity range |
| **Min/Max Intensity Value** | Variable range |
| **Min/Max Intensity** | Output intensity range |

#### Color Binding
| Setting | Description |
|---------|-------------|
| **Color Variable** | ColorVariable for light color |

#### Range Binding (Point/Spot)
| Setting | Description |
|---------|-------------|
| **Range Variable** | Float for light range |
| **Use Range Mapping** | Map variable range to light range |

#### Spot Angle Binding (Spot only)
| Setting | Description |
|---------|-------------|
| **Spot Angle Variable** | Float for spot angle |

#### Enabled Binding
| Setting | Description |
|---------|-------------|
| **Enabled Variable** | Bool to control on/off |
| **Invert Enabled** | Flip the logic |

#### Animation
| Setting | Description |
|---------|-------------|
| **Smooth Changes** | Animate property changes |
| **Smooth Speed** | Animation speed |

### Use Cases

- **Day/night cycle**: Intensity from time variable
- **Health indicator lights**: Color changes with health
- **Flashlight**: Enabled by bool variable
- **Alert lights**: Pulsing controlled by float

---

## Camera Binder

Binds variables to Camera component properties.

### Settings

#### Field of View
| Setting | Description |
|---------|-------------|
| **FOV Variable** | Float for field of view (perspective) |
| **Use FOV Mapping** | Map variable range to FOV range |
| **Min/Max FOV Value** | Variable range |
| **Min/Max FOV** | Output FOV range (1-179) |

#### Orthographic Size
| Setting | Description |
|---------|-------------|
| **Ortho Size Variable** | Float for orthographic size |
| **Use Ortho Mapping** | Map variable range |

#### Clip Planes
| Setting | Description |
|---------|-------------|
| **Near Clip Variable** | Float for near clip plane |
| **Far Clip Variable** | Float for far clip plane |

#### Background
| Setting | Description |
|---------|-------------|
| **Background Color Variable** | ColorVariable for background |

#### Viewport
| Setting | Description |
|---------|-------------|
| **Viewport Position Variable** | Vector2 for viewport (x, y) |
| **Viewport Size Variable** | Vector2 for viewport (width, height) |

#### Other
| Setting | Description |
|---------|-------------|
| **Depth Variable** | Float for camera depth |
| **Enabled Variable** | Bool to enable/disable camera |
| **Use Target Texture Variable** | Bool to toggle render texture |

### Use Cases

- **Zoom controls**: FOV controlled by scroll/input
- **Camera shake**: Viewport position wobble
- **Split-screen**: Dynamic viewport sizing
- **Cinematic effects**: Animated clip planes for fog/reveal

---

## Bool Toggle Binder

Binds BoolVariable to enable/disable GameObjects, Components, or Renderers.

### Settings

| Setting | Description |
|---------|-------------|
| **Variable** | BoolVariable to bind |
| **Toggle Mode** | What to toggle |
| **Invert** | Flip the logic |

### Toggle Modes

| Mode | Description |
|------|-------------|
| **GameObject** | Enable/disable GameObjects |
| **Behaviour** | Enable/disable Behaviour components |
| **Collider** | Enable/disable Colliders |
| **Renderer** | Enable/disable Renderers |

### Target Arrays

| Setting | Description |
|---------|-------------|
| **Game Objects** | List of GameObjects to toggle |
| **Behaviours** | List of Behaviours to toggle |
| **Colliders** | List of Colliders to toggle |
| **Renderers** | List of Renderers to toggle |

### Example

Variable = true → All targets enabled
Variable = false → All targets disabled
(Inverted if **Invert** is checked)

---

## Animator Binder

Comprehensive binder that connects ScriptableVariables directly to Animator parameters.

### Bool Bindings

Bind BoolVariables directly to Animator bool parameters.

| Setting | Description |
|---------|-------------|
| **Variable** | BoolVariable to bind |
| **Parameter Name** | Animator bool parameter name |
| **Invert** | Invert the bool value |

### Float Bindings

Bind numeric variables to Animator float parameters with optional remapping.

| Setting | Description |
|---------|-------------|
| **Variable** | Int or Float variable |
| **Parameter Name** | Animator float parameter name |
| **Multiplier** | Scale the value |
| **Use Remapping** | Enable input→output range mapping |
| **Input Min/Max** | Source value range |
| **Output Min/Max** | Target value range |
| **Clamp Output** | Clamp to min/max |
| **Use Damping** | Smooth parameter changes |
| **Damp Time** | Damping duration |

### Int Bindings

Bind numeric variables to Animator integer parameters.

| Setting | Description |
|---------|-------------|
| **Variable** | Int or Float variable |
| **Parameter Name** | Animator int parameter name |
| **Offset** | Value added to output |
| **Clamp Output** | Clamp to min/max |
| **Min/Max Value** | Clamp range |

### Trigger Bindings

Bind GameEvents to Animator triggers.

| Setting | Description |
|---------|-------------|
| **Game Event** | Event that fires the trigger |
| **Parameter Name** | Animator trigger parameter name |
| **Reset First** | Reset trigger before setting (prevents queuing) |

### Options

| Setting | Description |
|---------|-------------|
| **Continuous Update** | Update every frame (for smooth blending) |

### Use Cases

- **Player state machine**: Variables control animation states
- **Blend trees**: Float variables for blend weights
- **Combat**: Events trigger attack animations
- **UI animations**: Menu state controls animator

### Comparison: AnimatorBinder vs EventAnimatorBinder

| Feature | AnimatorBinder | EventAnimatorBinder |
|---------|---------------|---------------------|
| Bool via variable | ✅ BoolVariable | ❌ Events only |
| Float/Int | ✅ Direct binding | ✅ Variable binding |
| Triggers | ✅ GameEvent | ✅ GameEvent |
| Remapping | ✅ Full support | ❌ Multiplier only |
| Damping | ✅ Built-in | ❌ None |
| Continuous mode | ✅ Yes | ❌ No |

Use **AnimatorBinder** when you want direct variable-to-parameter binding.
Use **EventAnimatorBinder** for simple event-based trigger/bool control.

---

## Event Animator Binder

Binds GameEvents and ScriptableVariables to Animator parameters.

### Settings

| Setting | Description |
|---------|-------------|
| **Animator** | Target Animator |

### Event Bindings

Add bindings for GameEvents that trigger Animator parameters:

| Setting | Description |
|---------|-------------|
| **Event** | GameEvent to listen to |
| **Parameter Name** | Animator parameter name |
| **Parameter Type** | Trigger, Bool, Int, Float |
| **Bool Value** | Value to set (for Bool type) |
| **Int Value** | Value to set (for Int type) |
| **Float Value** | Value to set (for Float type) |

### Variable Bindings

Add bindings for Variables that sync to Animator parameters:

| Setting | Description |
|---------|-------------|
| **Variable** | Any ScriptableVariable |
| **Parameter Name** | Animator parameter name |
| **Parameter Type** | Bool, Int, Float |

### Use Cases

- **Door open event** → Trigger "Open" animation
- **Health variable** → Sync to "Health" float parameter
- **Is grounded bool** → Sync to "Grounded" bool parameter

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

💡 **Speed vs Direct Binders** — Use `NumericalRotationSpeedBinder` when input should control rotation speed (steering wheel), use `NumericalRotationBinder` when input should control rotation angle directly (dial).

💡 **Socket Binders** — Connect socket events to your game logic without writing socket-specific code.

💡 **Bool Toggle for UI** — Use `BoolToggleBinder` to show/hide UI elements based on game state.

⚠️ **Filled Image** — NumericalFillBinder requires Image.Type = Filled.

⚠️ **Physics Timing** — Physics binders apply in FixedUpdate. For manual control, disable continuous mode.

⚠️ **Material Binder Property Names** — Ensure property names match your shader exactly (case-sensitive).

⚠️ **Animator Parameter Names** — Ensure parameter names match your Animator Controller exactly (case-sensitive).

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
**Component Version:** 1.5.0

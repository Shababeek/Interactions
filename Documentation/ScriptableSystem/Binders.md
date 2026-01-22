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

| Binder | Variable Type | Updates |
|--------|--------------|---------|
| **TransformBinder** | Vector3/Quaternion/Vector2 | Position, Rotation, Scale |
| **TextMeshProBinder** | Any | TMP_Text content |
| **ColorTextMeshProBinder** | Color | TMP_Text color |
| **NumericalFillBinder** | Int/Float | Image fillAmount |
| **NumericalRotationBinder** | Int/Float | Transform rotation |
| **NumericalPositionBinder** | Int/Float | Position between two points |
| **ColorSpriteBinder** | Color | SpriteRenderer color |
| **ColorImageBinder** | Color | UI Image color |
| **RigidbodyVelocityBinder** | Vector2/Vector3 | Rigidbody velocity/acceleration |
| **AudioSourceBinder** | Audio | AudioSource settings |

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

## Rigidbody Velocity Binder

Binds Vector2/Vector3 to Rigidbody velocity or applies as acceleration.

### Settings

| Setting | Description |
|---------|-------------|
| **Physics Mode** | Rigidbody3D or Rigidbody2D |
| **Application Mode** | Velocity or Acceleration |
| **Vector3 Variable** | For 3D physics |
| **Vector2 Variable** | For 2D physics |
| **Plane** | XY, XZ, YZ for 2D mapping |
| **Multiplier** | Scale the velocity |
| **Continuous** | Apply every FixedUpdate |

### Example

```csharp
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Vector2Variable moveInput;

    void Update()
    {
        moveInput.Value = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"));
    }
}
```

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
|---------|-------------|---------|
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

💡 **Continuous vs Manual** — Set `continuous=false` on RigidbodyVelocityBinder and call `Apply()` manually for precise control.

💡 **Animation Curves** — NumericalPositionBinder supports curves for easing.

⚠️ **Filled Image** — NumericalFillBinder requires Image.Type = Filled.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Transform not updating | Binding disabled | Enable the specific bind toggle |
| 2D rotation wrong | Wrong plane | Change Direction Plane |
| Rigidbody jittery | Wrong timing | Use continuous mode |
| Position snaps | No smoothing | Enable Smooth option |

---

**Last Updated:** January 2026
**Component Version:** 1.1.0

# Variable Container — Group Multiple Variables in One Asset

> **Quick Reference**
> **Menu Path:** Assets > Create > Shababeek > Scriptable System > Variables > Variable Container
> **Use For:** Organizing related variables into a single asset
> **Requires:** ScriptableVariable types

---

## What It Does

**Variable Container** is a ScriptableObject that holds multiple named variables as sub-assets within a single .asset file. This provides:

- ✅ **Organization:** Group related variables together (player stats, game settings)
- ✅ **Single Reference:** One asset to manage instead of many
- ✅ **Dynamic Types:** Add any variable type via dropdown (populated by reflection)
- ✅ **Inline Editing:** Edit variable values directly in the container inspector
- ✅ **Code Access:** Easily retrieve variables by name at runtime

---

## Quick Example

> **Goal:** Create a container for player stats
> **Time:** ~3 minutes

[PLACEHOLDER_GIF: Creating container and adding variables]

1. **Create the Container:**
   - Right-click in Project > **Create > Shababeek > Scriptable System > Variables > Variable Container**
   - Name it "PlayerStats"

2. **Add Variables:**
   - Click the **+** button
   - Select from the dropdown: Int, Float, Text, etc.
   - Rename variables: "Health", "MaxHealth", "Speed", "Gold"
   - Set initial values

3. **Use in Code:**
   ```csharp
   public class Player : MonoBehaviour
   {
       [SerializeField] private VariableContainer stats;

       void Start()
       {
           var health = stats.Get<IntVariable>("Health");
           var speed = stats.Get<FloatVariable>("Speed");

           health.OnValueChanged.Subscribe(OnHealthChanged);
       }
   }
   ```

---

## Inspector Reference

[PLACEHOLDER_SCREENSHOT: VariableContainer inspector with multiple variables]

### Variables List

A reorderable list showing all variables in this container. Each entry displays:

| Column | Description |
|--------|-------------|
| **Name** | Editable name (stored as the sub-asset name) |
| **Type** | Variable type (Int, Float, Bool, etc.) |
| **Value** | Inline editable value field |

### Add Button (+)

Click to show a dropdown menu with all available variable types:

**Categories:**
- **Primitives:** Int, Float, Bool, Text
- **Vectors:** Vector2, Vector2Int, Vector3, Quaternion
- **Graphics:** Color, Gradient, AnimationCurve
- **Other:** GameObject, Transform, AudioClip, LayerMask, Enum

The dropdown is populated via reflection, so custom variable types automatically appear.

### Remove Button (-)

Removes the selected variable from the container and deletes its sub-asset.

⚠️ **Warning:** This permanently deletes the variable. References to it will become null.

### Utility Buttons

| Button | Description |
|--------|-------------|
| **Cleanup Nulls** | Removes any null references from the list |
| **Raise All** | Triggers events on all variables (useful for UI refresh) |
| **Reset All** | Resets all variables to default values (with confirmation) |

---

## API Reference

### Getting Variables

```csharp
// Get typed variable by name
IntVariable health = container.Get<IntVariable>("Health");

// Get any variable by name
ScriptableVariable anyVar = container.Get("Health");

// Try-get pattern (null-safe)
if (container.TryGet<FloatVariable>("Speed", out var speed))
{
    Debug.Log(speed.Value);
}

// Check existence
if (container.Has("Gold"))
{
    // ...
}
```

### Getting Multiple Variables

```csharp
// Get all variables
IReadOnlyList<ScriptableVariable> all = container.Variables;

// Get all of a specific type
foreach (var intVar in container.GetAll<IntVariable>())
{
    Debug.Log($"{intVar.name}: {intVar.Value}");
}

// Get all numeric variables
foreach (var numVar in container.GetAllNumerical())
{
    Debug.Log($"Numeric: {numVar.AsFloat}");
}

// Get all variable names
IEnumerable<string> names = container.GetNames();
```

### Indexer Access

```csharp
// Access by index
ScriptableVariable first = container[0];

// Count
int count = container.Count;
```

### Utility Methods

```csharp
// Reset all variables to default values
container.ResetAll();

// Raise events on all variables (refresh UI)
container.RaiseAll();
```

---

## Common Workflows

### How To: Create Player Stats Container

> **Goal:** Set up a complete player stats system
> **Time:** ~5 minutes

1. Create **Variable Container** named "PlayerStats"

2. Add variables:

   | Name | Type | Default |
   |------|------|---------|
   | Health | Int | 100 |
   | MaxHealth | Int | 100 |
   | Speed | Float | 5.0 |
   | Gold | Int | 0 |
   | PlayerName | Text | "Player" |

3. Create player script:
   ```csharp
   public class PlayerController : MonoBehaviour
   {
       [SerializeField] private VariableContainer stats;
       private IntVariable _health;
       private FloatVariable _speed;

       void Start()
       {
           _health = stats.Get<IntVariable>("Health");
           _speed = stats.Get<FloatVariable>("Speed");
       }

       void TakeDamage(int damage)
       {
           _health.Add(-damage);
           _health.Clamp(0, stats.Get<IntVariable>("MaxHealth").Value);
       }
   }
   ```

---

### How To: Game Settings Container

> **Goal:** Centralize game settings
> **Time:** ~3 minutes

1. Create **Variable Container** named "GameSettings"

2. Add settings:

   | Name | Type | Default |
   |------|------|---------|
   | MasterVolume | Float | 1.0 |
   | MusicVolume | Float | 0.8 |
   | SFXVolume | Float | 1.0 |
   | Difficulty | Int | 1 |
   | InvertY | Bool | false |

3. Reference in settings menu:
   ```csharp
   public class SettingsMenu : MonoBehaviour
   {
       [SerializeField] private VariableContainer settings;
       [SerializeField] private Slider volumeSlider;

       void Start()
       {
           var volume = settings.Get<FloatVariable>("MasterVolume");
           volumeSlider.value = volume.Value;
           volumeSlider.onValueChanged.AddListener(v => volume.Value = v);
       }
   }
   ```

---

### How To: Level Data Container

> **Goal:** Store level-specific data
> **Time:** ~3 minutes

1. Create **Variable Container** for each level: "Level1Data", "Level2Data"

2. Add common variables:

   | Name | Type | Description |
   |------|------|-------------|
   | LevelName | Text | Display name |
   | TimeLimit | Float | Level time limit |
   | TargetScore | Int | Score to complete |
   | Completed | Bool | Has been completed |
   | BestTime | Float | Best completion time |

3. Load dynamically:
   ```csharp
   public class LevelManager : MonoBehaviour
   {
       public void LoadLevel(VariableContainer levelData)
       {
           var timeLimit = levelData.Get<FloatVariable>("TimeLimit").Value;
           var targetScore = levelData.Get<IntVariable>("TargetScore").Value;
           // ...
       }
   }
   ```

---

## Tips & Best Practices

💡 **Name Variables Descriptively**
Use clear names like "PlayerHealth" not just "Health" — the container already provides context.

💡 **Cache Variable References**
Get variables once in Start/Awake and cache them for performance.

💡 **Group by Context**
Create separate containers for different systems: PlayerStats, GameSettings, AudioSettings.

💡 **Use Reset for New Game**
Call `ResetAll()` when starting a new game to restore default values.

💡 **Raise After Loading**
Call `RaiseAll()` after loading a scene to ensure all UI updates with current values.

⚠️ **Common Mistake:** Modifying container at runtime
Don't add/remove variables at runtime — the list is for editor-time configuration.

⚠️ **Common Mistake:** Same name different types
Avoid having multiple variables with the same name but different types.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Variable not found | Name mismatch | Check exact spelling and casing |
| Get returns null | Wrong type | Verify the variable type matches |
| Sub-assets missing | Asset corruption | Re-create the container |
| Dropdown empty | No variable types | Ensure variable classes compile |
| Changes not saved | Forgot to save | Save project after making changes |

---

## Related Documentation

- [Scriptable Variables](ScriptableVariables.md) — Individual variable types
- [Binders](Binders.md) — Connect variables to components
- [Quick Start Guide](../GettingStarted/QuickStart.md) — Basic setup

---

**Last Updated:** January 2026
**Component Version:** 1.0.0

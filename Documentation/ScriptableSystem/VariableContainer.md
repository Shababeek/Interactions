# Variable Container — Group Multiple Variables and Events in One Asset

> **Quick Reference**
> **Menu Path:** Assets > Create > Shababeek > Scriptable System > Variables > Variable Container
> **Use For:** Organizing related variables and events into a single asset
> **Requires:** ScriptableVariable types, GameEvent

---

## What It Does

**Variable Container** is a ScriptableObject that holds multiple named variables and events as sub-assets within a single .asset file. This provides:

- ✅ **Organization:** Group related variables and events together (player stats, game settings)
- ✅ **Single Reference:** One asset to manage instead of many
- ✅ **Dynamic Types:** Add any variable or event type via dropdown (populated by reflection)
- ✅ **Inline Editing:** Edit variable values directly in the container inspector
- ✅ **Auto-Naming:** Sub-assets are automatically named with container prefix (ContainerName_variableName)
- ✅ **Code Access:** Easily retrieve variables and events by name at runtime

---

## Quick Example

> **Goal:** Create a container for player stats with variables and events
> **Time:** ~3 minutes

![Creating a Variable Container](../Images/variable-container-create.gif)

1. **Create the Container:**
   - Right-click in Project > **Create > Shababeek > Scriptable System > Variables > Variable Container**
   - Name it "PlayerStats"

2. **Add Variables:**
   - In the **Variables** section, click the **+** button
   - Select from the dropdown: Int, Float, Text, etc.
   - Rename variables: "Health", "MaxHealth", "Speed", "Gold"
   - Set initial values

3. **Add Events:**
   - In the **Events** section, click the **+** button
   - Select "GameEvent" from the dropdown
   - Rename events: "OnDeath", "OnLevelUp", "OnDamaged"

4. **Use in Code:**
   ```csharp
   public class Player : MonoBehaviour
   {
       [SerializeField] private VariableContainer stats;

       private IntVariable _health;
       private GameEvent _onDeath;

       void Start()
       {
           _health = stats.Get<IntVariable>("Health");
           _onDeath = stats.GetEvent("OnDeath");

           _health.OnValueChanged.Subscribe(OnHealthChanged);
       }

       void OnHealthChanged(int newHealth)
       {
           if (newHealth <= 0)
               _onDeath.Raise();
       }
   }
   ```

---

## Inspector Reference

![Variable Container Inspector](../Images/variable-container-inspector.png)

### Variables Section

A reorderable list showing all variables in this container. Each entry displays:

| Column | Description |
|--------|-------------|
| **Name** | Editable name (stored as the sub-asset name with container prefix) |
| **Type** | Variable type (Int, Float, Bool, etc.) |
| **Value** | Inline editable value field |

### Events Section

A reorderable list showing all GameEvents in this container. Each entry displays:

| Column | Description |
|--------|-------------|
| **Name** | Editable name (stored as the sub-asset name with container prefix) |
| **Type** | Always "GameEvent" |

### Add Button (+)

Click to show a dropdown menu with all available types:

**Variable Categories:**
- **Primitives:** Int, Float, Bool, Text
- **Vectors:** Vector2, Vector2Int, Vector3, Quaternion
- **Graphics:** Color, Gradient, AnimationCurve
- **Other:** GameObject, Transform, AudioClip, LayerMask, Enum

**Event Types:**
- GameEvent (with parameterized variants if available)

The dropdown is populated via reflection, so custom variable types automatically appear.

### Remove Button (-)

Removes the selected variable or event from the container and deletes its sub-asset.

⚠️ **Warning:** This permanently deletes the asset. References to it will become null.

### Naming Convention

Sub-assets are automatically named with the container name as a prefix:
- Container: "PlayerStats"
- Variable named "Health" → Asset name: "PlayerStats_Health"
- Event named "OnDeath" → Asset name: "PlayerStats_OnDeath"

This prevents naming conflicts when using multiple containers.

### Utility Buttons

| Button | Description |
|--------|-------------|
| **Cleanup Nulls** | Removes any null references from the lists |
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

### Getting Events

```csharp
// Get event by name
GameEvent onDeath = container.GetEvent("OnDeath");

// Check existence
if (container.HasEvent("OnLevelUp"))
{
    // ...
}

// Get all events
IReadOnlyList<GameEvent> allEvents = container.Events;
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
// Access variable by index
ScriptableVariable first = container[0];

// Counts
int variableCount = container.Count;
int eventCount = container.EventCount;
```

### Utility Methods

```csharp
// Reset all variables to default values
container.ResetAll();

// Raise events on all variables (refresh UI)
container.RaiseAll();
```

---

## Persistence (Save/Load)

Variable Containers support saving and loading all variable values to JSON files. This enables save systems, settings persistence, and state serialization.

### Quick Save/Load

```csharp
// Save to default location (Application.persistentDataPath/VariableContainers/)
container.Save();  // Uses container name as filename

// Load from default location
container.Load();

// Check if save exists
if (container.SaveExists())
{
    container.Load();
}

// Delete save file
container.DeleteSave();
```

### Custom File Paths

```csharp
// Save to specific file
container.SaveToFile("/path/to/save.json");

// Load from specific file
container.LoadFromFile("/path/to/save.json");

// Get the default save path
string path = container.GetDefaultSavePath();
// Returns: "{persistentDataPath}/VariableContainers/{containerName}.json"
```

### Supported Variable Types

| Type | Serialization |
|------|---------------|
| IntVariable | Integer string |
| FloatVariable | Float string (invariant culture) |
| BoolVariable | "True"/"False" |
| TextVariable | Raw string |
| Vector2Variable | JSON object |
| Vector3Variable | JSON object |
| QuaternionVariable | JSON object |
| ColorVariable | JSON object |
| Vector2IntVariable | JSON object |
| StringListVariable | JSON array |

### Save File Format

```json
{
    "containerName": "PlayerStats",
    "savedAt": "2026-01-28 14:30:45",
    "variables": [
        { "name": "Health", "type": "IntVariable", "value": "85" },
        { "name": "Speed", "type": "FloatVariable", "value": "5.5" },
        { "name": "PlayerName", "type": "TextVariable", "value": "Hero" }
    ]
}
```

### Example: Game Save System

```csharp
public class SaveManager : MonoBehaviour
{
    [SerializeField] private VariableContainer playerStats;
    [SerializeField] private VariableContainer gameProgress;
    [SerializeField] private VariableContainer settings;

    public void SaveGame()
    {
        playerStats.Save("player_save.json");
        gameProgress.Save("progress_save.json");
    }

    public void LoadGame()
    {
        if (playerStats.SaveExists("player_save.json"))
        {
            playerStats.Load("player_save.json");
            gameProgress.Load("progress_save.json");
            playerStats.RaiseAllVariables(); // Refresh UI
        }
    }

    public void SaveSettings()
    {
        settings.Save("settings.json");
    }

    public void LoadSettings()
    {
        settings.Load("settings.json");
    }

    public void NewGame()
    {
        playerStats.ResetAllVariables();
        gameProgress.ResetAllVariables();
    }
}
```

### Example: Auto-Save on Application Quit

```csharp
public class AutoSave : MonoBehaviour
{
    [SerializeField] private VariableContainer[] containersToSave;

    void OnApplicationQuit()
    {
        foreach (var container in containersToSave)
        {
            container.Save();
        }
    }

    void Start()
    {
        foreach (var container in containersToSave)
        {
            if (container.SaveExists())
            {
                container.Load();
            }
        }
    }
}
```

### Notes on Persistence

- **Reference types not saved**: GameObjectVariable, TransformVariable, etc. cannot be serialized
- **Events not saved**: GameEvents don't have values to persist
- **Missing variables**: When loading, variables not found in container are skipped with a warning
- **Type mismatches**: If a variable type changed since save, loading will fail for that variable
- **Thread safety**: Save/Load operations are not thread-safe

---

## Common Workflows

### How To: Create Player Stats Container

> **Goal:** Set up a complete player stats system with variables and events
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

3. Add events:

   | Name | Description |
   |------|-------------|
   | OnDeath | Raised when health reaches 0 |
   | OnDamaged | Raised when taking damage |
   | OnHealed | Raised when healing |
   | OnLevelUp | Raised on level up |

4. Create player script:
   ```csharp
   public class PlayerController : MonoBehaviour
   {
       [SerializeField] private VariableContainer stats;
       private IntVariable _health;
       private FloatVariable _speed;
       private GameEvent _onDeath;
       private GameEvent _onDamaged;

       void Start()
       {
           _health = stats.Get<IntVariable>("Health");
           _speed = stats.Get<FloatVariable>("Speed");
           _onDeath = stats.GetEvent("OnDeath");
           _onDamaged = stats.GetEvent("OnDamaged");
       }

       public void TakeDamage(int damage)
       {
           _health.Add(-damage);
           _health.Clamp(0, stats.Get<IntVariable>("MaxHealth").Value);
           _onDamaged.Raise();

           if (_health.Value <= 0)
               _onDeath.Raise();
       }
   }
   ```

---

### How To: Game Settings Container

> **Goal:** Centralize game settings with events for changes
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

3. Add events:

   | Name | Description |
   |------|-------------|
   | OnSettingsChanged | Raised when any setting changes |
   | OnVolumeChanged | Raised when volume settings change |

4. Reference in settings menu:
   ```csharp
   public class SettingsMenu : MonoBehaviour
   {
       [SerializeField] private VariableContainer settings;
       [SerializeField] private Slider volumeSlider;

       private GameEvent _onVolumeChanged;

       void Start()
       {
           var volume = settings.Get<FloatVariable>("MasterVolume");
           _onVolumeChanged = settings.GetEvent("OnVolumeChanged");

           volumeSlider.value = volume.Value;
           volumeSlider.onValueChanged.AddListener(v =>
           {
               volume.Value = v;
               _onVolumeChanged.Raise();
           });
       }
   }
   ```

---

### How To: Level Data Container

> **Goal:** Store level-specific data with completion events
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

3. Add events:

   | Name | Description |
   |------|-------------|
   | OnLevelStart | Raised when level starts |
   | OnLevelComplete | Raised when level completed |
   | OnNewBestTime | Raised when new best time achieved |

4. Load dynamically:
   ```csharp
   public class LevelManager : MonoBehaviour
   {
       public void LoadLevel(VariableContainer levelData)
       {
           var timeLimit = levelData.Get<FloatVariable>("TimeLimit").Value;
           var targetScore = levelData.Get<IntVariable>("TargetScore").Value;
           var onStart = levelData.GetEvent("OnLevelStart");

           onStart.Raise();
           // ...
       }
   }
   ```

---

## Tips & Best Practices

💡 **Name Variables Descriptively**
Use clear names like "Health" not "HP" — the container prefix provides additional context.

💡 **Cache Variable References**
Get variables and events once in Start/Awake and cache them for performance.

💡 **Group by Context**
Create separate containers for different systems: PlayerStats, GameSettings, AudioSettings.

💡 **Use Reset for New Game**
Call `ResetAll()` when starting a new game to restore default values.

💡 **Raise After Loading**
Call `RaiseAll()` after loading a scene to ensure all UI updates with current values.

💡 **Events for Side Effects**
Use events to trigger side effects (sounds, particles, UI updates) rather than coupling systems directly.

💡 **Container Prefix**
The automatic naming (ContainerName_variableName) helps identify assets at a glance in the Project view.

⚠️ **Common Mistake:** Modifying container at runtime
Don't add/remove variables at runtime — the lists are for editor-time configuration.

⚠️ **Common Mistake:** Same name different types
Avoid having multiple variables with the same name but different types.

⚠️ **Common Mistake:** Forgetting to cache
Calling `Get<T>()` every frame is wasteful — cache references in Start/Awake.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Variable not found | Name mismatch | Check exact spelling and casing |
| Get returns null | Wrong type | Verify the variable type matches |
| Sub-assets missing | Asset corruption | Re-create the container |
| Dropdown empty | No variable types | Ensure variable classes compile |
| Changes not saved | Forgot to save | Save project after making changes |
| Event not in dropdown | Wrong event type | Ensure GameEvent class is available |
| Name shows full path | Expected behavior | Container prefix is intentional |

---

## Related Documentation

- [Scriptable Variables](ScriptableVariables.md) — Individual variable types
- [Scriptable System Window](ScriptableSystemWindow.md) — Debug and monitoring tool
- [Binders](Binders.md) — Connect variables to components
- [Game Events](GameEvents.md) — Event system documentation
- [Quick Start Guide](../GettingStarted/QuickStart.md) — Basic setup

---

**Last Updated:** January 2026
**Component Version:** 1.2.0

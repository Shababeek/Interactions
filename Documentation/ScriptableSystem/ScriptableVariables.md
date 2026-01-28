# Scriptable Variables — Reactive Data Containers

> **Quick Reference**
> **Menu Path:** Assets > Create > Shababeek > Scriptable System > Variables
> **Use For:** Storing and sharing data across scenes and components
> **Requires:** UniRx package

---

## What It Does

The **Scriptable Variables System** provides reactive, asset-based data containers that can be shared across your entire project. Unlike regular variables, Scriptable Variables:

- ✅ Persist as assets (survive scene changes)
- ✅ Can be referenced by multiple components
- ✅ Trigger events when values change (reactive)
- ✅ Decouple systems (no direct references needed)
- ✅ Easy to debug and modify in the Inspector

**Key Benefits:**
- **Scene Independence:** Variables exist as assets, so data persists across scenes
- **Reactive Updates:** Subscribe to changes with UniRx for automatic UI updates
- **Designer Friendly:** Non-programmers can create and tweak values in the Inspector
- **Testable:** Easy to mock and test individual systems

---

## Available Variable Types

| Type | Description | Example Use |
|------|-------------|-------------|
| **IntVariable** | Integer numbers | Health, Score, Level |
| **FloatVariable** | Decimal numbers | Speed, Timer, Progress |
| **BoolVariable** | True/False values | IsAlive, IsPaused, HasKey |
| **TextVariable** | String text | Player Name, Messages |
| **StringListVariable** | List of strings | Dialogue options, Tags, Inventory names |
| **Vector2Variable** | 2D vectors | Direction, Position 2D |
| **Vector3Variable** | 3D vectors | Position, Velocity |
| **ColorVariable** | RGBA colors | UI Theme, Effects |
| **GameObjectVariable** | Object references | Player, Target |
| **TransformVariable** | Transform references | Camera Target |
| **AudioClipVariable** | Audio clips | Current Music |

---

## Quick Example

> **Goal:** Create a health system that updates UI automatically
> **Time:** ~5 minutes

![Creating Health Variable and UI Connection](../Images/scriptable-variable-health-ui.gif)

1. **Create the Variable:**
   - Right-click in Project > **Create > Shababeek > Scriptable System > Variables > IntVariable**
   - Name it "PlayerHealth"
   - Set initial value to 100

2. **Reference in Player Script:**
   ```csharp
   public class PlayerHealth : MonoBehaviour
   {
       [SerializeField] private IntVariable health;

       public void TakeDamage(int damage)
       {
           health.Add(-damage);
           health.Clamp(0, 100);
       }
   }
   ```

3. **Connect to UI:**
   - Add **Numerical Fill Binder** to your health bar Image
   - Assign the "PlayerHealth" variable
   - Set Min: 0, Max: 100

The health bar now updates automatically when the variable changes!

---

## Numerical Variables

IntVariable and FloatVariable inherit from `NumericalVariable<T>`, providing common numeric operations:

### Common Operations

```csharp
// All these work with both IntVariable and FloatVariable
numericVar.Add(10);           // Add to current value
numericVar.Subtract(5);       // Subtract from current value
numericVar.Multiply(2);       // Multiply current value
numericVar.Divide(2);         // Divide current value
numericVar.Clamp(0, 100);     // Clamp between min/max

// Normalized value (useful for UI)
float normalized = numericVar.GetNormalized(0, 100); // Returns 0-1

// Set from normalized
numericVar.SetFromNormalized(0.5f, 0, 100); // Sets to 50

// Interpolation
numericVar.LerpTo(targetValue, t);
numericVar.MoveTowards(targetValue, maxDelta);
```

### INumericalVariable Interface

Both IntVariable and FloatVariable implement `INumericalVariable`, enabling generic numeric operations:

```csharp
// Works with any numeric variable
void ProcessNumber(INumericalVariable numVar)
{
    float value = numVar.AsFloat;      // Get as float
    int intValue = numVar.AsInt;       // Get as int
    numVar.SetFromFloat(42.5f);        // Set from float
}
```

---

## String List Variable

`StringListVariable` stores a dynamic list of strings, useful for dialogue systems, inventories, or any collection of text.

### Common Operations

```csharp
// Adding items
stringList.Add("New Item");
stringList.AddUnique("Only Once");     // Won't add duplicates
stringList.AddRange(new[] { "A", "B" });

// Removing items
stringList.Remove("Item Name");
stringList.RemoveAt(0);
stringList.Clear();

// Querying
bool has = stringList.Contains("Item");
int index = stringList.IndexOf("Item");
int count = stringList.Count;

// Accessing
string first = stringList.First;
string last = stringList.Last;
string item = stringList[2];        // By index

// Random
string random = stringList.GetRandom();
string popped = stringList.PopRandom(); // Get and remove

// Manipulation
stringList.Sort();
stringList.Reverse();
stringList.Shuffle();

// Conversion
string joined = stringList.Join(", ");
string[] array = stringList.ToArray();
List<string> copy = stringList.ToList();
```

### Use Cases

- **Dialogue options**: Store available responses
- **Inventory lists**: Track item names
- **Tag systems**: Dynamic list of tags
- **Quest objectives**: List of tasks
- **High scores**: Player names

---

## Variable References

Use `VariableReference<T>` to allow designers to choose between a constant value or a variable reference:

```csharp
public class Enemy : MonoBehaviour
{
    // Can be set to either a constant or reference a variable
    [SerializeField] private IntReference damage;
    [SerializeField] private FloatReference speed;

    void Attack()
    {
        // Works the same regardless of whether it's constant or variable
        player.TakeDamage(damage.Value);
    }
}
```

### NumericalReference

`NumericalReference` works with any numeric variable (IntVariable or FloatVariable):

```csharp
public class ProgressBar : MonoBehaviour
{
    [SerializeField] private NumericalReference value;
    [SerializeField] private NumericalReference maxValue;

    void Update()
    {
        float fill = value.Value / maxValue.Value;
        // ...
    }
}
```

---

## Subscribing to Changes

Use UniRx to react to value changes:

```csharp
public class HealthUI : MonoBehaviour
{
    [SerializeField] private IntVariable health;
    [SerializeField] private Text healthText;

    private CompositeDisposable _disposable = new();

    void OnEnable()
    {
        // Subscribe to changes
        health.OnValueChanged
            .Subscribe(value => healthText.text = $"HP: {value}")
            .AddTo(_disposable);

        // Also trigger immediately with current value
        healthText.text = $"HP: {health.Value}";
    }

    void OnDisable()
    {
        _disposable.Dispose();
    }
}
```

---

## Creating Custom Variable Types

Extend `ScriptableVariable<T>` to create your own types:

```csharp
[CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/PlayerDataVariable")]
public class PlayerDataVariable : ScriptableVariable<PlayerData>
{
    // Add custom methods if needed
    public void ResetToDefault()
    {
        Value = new PlayerData { health = 100, gold = 0 };
    }
}

[Serializable]
public struct PlayerData
{
    public int health;
    public int gold;
}
```

---

## Tips & Best Practices

💡 **Use Descriptive Names**
Name variables clearly: "PlayerHealth" not "Health", "EnemySpawnRate" not "Rate".

💡 **One Responsibility**
Each variable should represent one piece of data. Don't pack multiple values together.

💡 **Initialize on Start**
Reset variables at game start if they should have fresh values each session.

💡 **Use Variable References**
Prefer `IntReference` over `IntVariable` in components to allow constant values when appropriate.

💡 **Group Related Variables**
Use Variable Containers to organize related variables (e.g., all player stats in one container).

⚠️ **Common Mistake:** Forgetting to dispose subscriptions
Always use `.AddTo(disposable)` or manually dispose in OnDisable.

⚠️ **Common Mistake:** Modifying variables during serialization
Don't modify variable values in constructors or field initializers.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Value doesn't update | Not calling Value setter | Use `variable.Value = x` not `variable.value = x` |
| UI doesn't update | Missing subscription | Subscribe to OnValueChanged |
| Subscription error on disable | Not disposing | Use CompositeDisposable and dispose in OnDisable |
| Value resets on play | Editor value not saved | Click on variable asset, modify value, and save project |
| Variable is null | Not assigned in Inspector | Drag the variable asset to the field |

---

## Related Documentation

- [Variable Container](VariableContainer.md) — Group multiple variables
- [Scriptable System Window](ScriptableSystemWindow.md) — Debug and monitoring tool
- [Binders](Binders.md) — Connect variables to components
- [Quick Start Guide](../GettingStarted/QuickStart.md) — Basic setup

---

**Last Updated:** January 2026
**Component Version:** 1.0.0

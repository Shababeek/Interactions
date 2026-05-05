# Shababeek Core System - User Manual

## Overview

The Shababeek Core system provides foundational components for tweening, events, and variables that can be used throughout your Unity project. This manual explains how to use these systems from the Unity Inspector and through code.

## Table of Contents

1. [Tween System](#tween-system)
2. [Scriptable System](#scriptable-system)
   - [Events](#events)
   - [Variables](#variables)
   - [Utility Attributes](#utility-attributes)
3. [Best Practices](#best-practices)
4. [Examples](#examples)

---

## Tween System

The Tween System provides smooth animations for values and transforms over time.

### VariableTweener Component

**What it does:** Manages multiple tweening animations automatically.

**How to use:**
1. Add the `VariableTweener` component to any GameObject
2. Configure the `Tween Scale` in the inspector (higher values = faster animations)
3. The component automatically updates all registered tweenables each frame

**Inspector Settings:**
- **Tween Scale**: Controls the speed of all animations (default: 12f)

### TweenableFloat

**What it does:** Smoothly animates float values between start and target values.

**How to use:**
```csharp
// Get the tweener component
var tweener = GetComponent<VariableTweener>();

// Create a tweenable float
var floatTween = new TweenableFloat(tweener, 
    onChange: value => Debug.Log($"Value: {value}"), // Optional callback
    rate: 2f,                                       // Animation speed
    value: 0f                                       // Initial value
);

// Start animating to a new value
floatTween.Value = 10f;

// Listen for completion
floatTween.OnFinished += () => Debug.Log("Animation complete!");
```

**Inspector Integration:**
- The `Value` property automatically starts tweening when set
- Use `OnChange` event for real-time updates
- Use `OnFinished` event for completion callbacks

### TransformTweenable

**What it does:** Smoothly animates a Transform's position and rotation to match a target Transform.

**How to use:**
```csharp
// Create a transform tween
var transformTween = new TransformTweenable();

// Initialize with source and target transforms
transformTween.Initialize(myTransform, targetTransform);

// Listen for completion
transformTween.OnTweenComplete += () => Debug.Log("Transform animation complete!");

// Add to tweener for automatic updates
tweener.AddTweenable(transformTween);
```

**Inspector Integration:**
- Must be initialized with source and target transforms
- Automatically removes itself from the tweener when complete

---

## Scriptable System

The Scriptable System provides ScriptableObject-based events and variables for data-driven design.

### Events

#### GameEvent

**What it does:** Base class for events that can be raised and observed.

**How to use:**
1. Create a new ScriptableObject asset: `Right-click → Create → Shababeek → Scriptable System → Events → GameEvent`
2. Connect Unity events in the inspector
3. Raise the event from code

**Inspector Settings:**
- **On Raised**: Connect Unity events that will be triggered when the event is raised

**Code Example:**
```csharp
// Reference the event asset
[SerializeField] private GameEvent myEvent;

// Raise the event
myEvent.Raise();
```

#### GameEvent<T> (Generic Events)

**What it does:** Events that can carry data payloads.

**How to use:**
1. Create a derived class for your specific data type
2. Implement the `DefaultValue` property
3. Use the `Raise(T data)` method to pass data

**Code Example:**
```csharp
// Create a float event
[CreateAssetMenu(menuName = "Shababeek/Events/FloatEvent")]
public class FloatEvent : GameEvent<float>
{
    protected override float DefaultValue => 0f;
}

// Usage
floatEvent.Raise(42f);
```

#### GameEventListener

**What it does:** Listens to GameEvents and triggers Unity events in response.

**How to use:**
1. Add the `GameEventListener` component to a GameObject
2. In the inspector, add GameEvents to the list
3. Connect Unity events for each GameEvent
4. The component automatically subscribes when enabled

**Inspector Settings:**
- **Game Event List**: Array of GameEvent + UnityEvent pairs
  - **Game Event**: The event to listen to
  - **On Game Event Raised**: Unity event to trigger when the GameEvent is raised

#### ObjectLifecycleEvents

**What it does:** Automatically raises events when a GameObject is enabled or disabled.

**How to use:**
1. Add the `ObjectLifecycleEvents` component to any GameObject
2. Connect GameEvents and UnityEvents in the inspector
3. Events are automatically raised when the object's lifecycle changes

**Inspector Settings:**
- **Game Events On Enable**: Array of GameEvents to raise when enabled
- **Game Events On Disable**: Array of GameEvents to raise when disabled
- **On Object Enabled**: Unity event to trigger when enabled
- **On Object Disabled**: Unity event to trigger when disabled

### Variables

#### ScriptableVariable<T>

**What it does:** Base class for variables that can be observed and raised as events.

**How to use:**
1. Create derived classes for specific types (FloatVariable, IntVariable, etc.)
2. Set values through code or inspector
3. Subscribe to value changes using UniRx

**Inspector Settings:**
- **Value**: The current value stored by the variable

**Code Example:**
```csharp
// Subscribe to value changes
floatVariable.OnValueChanged.Subscribe(value => Debug.Log($"Value changed to: {value}"));

// Set a new value (triggers events automatically)
floatVariable.Value = 42f;
```

#### FloatVariable

**What it does:** Scriptable variable for float values with full mathematical operator support.

**How to use:**
1. Create a new ScriptableObject asset: `Right-click → Create → Shababeek → Scriptable System → Variables → FloatVariable`
2. Use mathematical operators directly with float values
3. Subscribe to value changes

**Inspector Settings:**
- **Value**: The current float value

**Code Example:**
```csharp
var var1 = ScriptableObject.CreateInstance<FloatVariable>();
var var2 = ScriptableObject.CreateInstance<FloatVariable>();

var1.Value = 5f;
var2.Value = 3f;

float result = var1 + var2;        // result = 8f
bool isEqual = var1 == 5f;         // isEqual = true
float product = var1 * var2;       // product = 15f
var1++;                           // Increments var1.Value to 6f
```

**Available Operators:**
- **Comparison**: `==`, `!=` (with both FloatVariable and float)
- **Arithmetic**: `+`, `-`, `*`, `/` (with both FloatVariable and float)
- **Increment/Decrement**: `++`, `--`

#### VariableReference<T>

**What it does:** References that can point to either a ScriptableVariable or use a constant value.

**How to use:**
1. Use in place of direct ScriptableVariable references
2. Provides flexibility between dynamic and static values
3. Automatically handles null checks

**Inspector Settings:**
- **Use Constant**: Toggle between variable reference and constant value
- **Variable**: Reference to a ScriptableVariable asset
- **Constant Value**: Direct value when not using a variable

### Utility Attributes

#### ReadOnly

**What it does:** Makes a serialized field read-only in the Inspector.

**How to use:**
```csharp
[SerializeField]
[ReadOnly]
private float calculatedValue; // This will be read-only in the Inspector
```

**Inspector Behavior:**
- Field is visible but cannot be edited
- Useful for calculated or runtime-only values

#### MinMax

**What it does:** Creates a slider in the Inspector with configurable min/max limits.

**How to use:**
```csharp
[SerializeField]
[MinMax(0f, 100f)]
private float health; // Will show as a slider from 0 to 100

[SerializeField]
[MinMax(0f, 1f, true, true)]
private float alpha; // Will show as a slider with editable range and debug values
```

**Inspector Settings:**
- **Min Limit**: Minimum value for the slider
- **Max Limit**: Maximum value for the slider
- **Show Edit Range**: Allows editing of min/max values
- **Show Debug Values**: Shows debug information

---

## Best Practices

### Tween System
1. **Reuse Tweenables**: Don't create new TweenableFloat instances every frame
2. **Proper Cleanup**: Tweenables are automatically removed when complete
3. **Rate Tuning**: Adjust the rate parameter for different animation speeds
4. **Event Usage**: Use OnChange for real-time updates, OnFinished for completion

### Event System
1. **Asset Organization**: Keep GameEvents in organized folders
2. **Event Naming**: Use descriptive names for your events
3. **Unsubscribe**: UniRx subscriptions are automatically managed by GameEventListener
4. **Inspector Connections**: Use the inspector to connect events when possible

### Variable System
1. **Type Safety**: Use the appropriate variable type for your data
2. **Operator Overloading**: Take advantage of mathematical operators for clean code
3. **Value Change Events**: Subscribe to OnValueChanged for reactive programming
4. **Asset References**: Store variables as assets for easy reuse across scenes

### Performance
1. **Tween Scale**: Adjust the VariableTweener's scale based on your needs
2. **Event Frequency**: Avoid raising events every frame unless necessary
3. **Variable Updates**: Variables automatically trigger events when values change
4. **Memory Management**: UniRx subscriptions are automatically disposed

---

## Examples

### Complete Tweening Example

```csharp
public class TweeningExample : MonoBehaviour
{
    [SerializeField] private VariableTweener tweener;
    [SerializeField] private Transform targetTransform;
    
    private TweenableFloat healthTween;
    private TransformTweenable movementTween;
    
    void Start()
    {
        // Create health tween
        healthTween = new TweenableFloat(tweener, 
            onChange: value => UpdateHealthUI(value),
            rate: 3f,
            value: 100f
        );
        
        // Create movement tween
        movementTween = new TransformTweenable();
        movementTween.Initialize(transform, targetTransform);
        movementTween.OnTweenComplete += OnMovementComplete;
        
        // Add to tweener
        tweener.AddTweenable(movementTween);
    }
    
    public void TakeDamage(float damage)
    {
        float newHealth = healthTween.Value - damage;
        healthTween.Value = Mathf.Max(0f, newHealth);
    }
    
    private void UpdateHealthUI(float health)
    {
        // Update UI elements
    }
    
    private void OnMovementComplete()
    {
        Debug.Log("Reached target position!");
    }
}
```

### Complete Event System Example

```csharp
public class EventSystemExample : MonoBehaviour
{
    [SerializeField] private GameEvent playerDeathEvent;
    [SerializeField] private FloatEvent healthChangedEvent;
    [SerializeField] private FloatVariable playerHealth;
    
    void Start()
    {
        // Subscribe to health changes
        playerHealth.OnValueChanged.Subscribe(OnHealthChanged);
        
        // Subscribe to death event
        playerDeathEvent.OnRaised.Subscribe(_ => OnPlayerDeath());
    }
    
    private void OnHealthChanged(float newHealth)
    {
        // Raise health changed event
        healthChangedEvent.Raise(newHealth);
        
        // Check for death
        if (newHealth <= 0f)
        {
            playerDeathEvent.Raise();
        }
    }
    
    private void OnPlayerDeath()
    {
        Debug.Log("Player has died!");
        // Handle death logic
    }
}
```

### Complete Variable System Example

```csharp
public class VariableSystemExample : MonoBehaviour
{
    [SerializeField] private FloatVariable playerSpeed;
    [SerializeField] private FloatVariable playerHealth;
    [SerializeField] private FloatVariable playerDamage;
    
    void Start()
    {
        // Subscribe to speed changes
        playerSpeed.OnValueChanged.Subscribe(OnSpeedChanged);
        
        // Use mathematical operators
        float totalDamage = playerDamage * 1.5f;
        bool isHealthy = playerHealth > 50f;
    }
    
    private void OnSpeedChanged(float newSpeed)
    {
        // Update movement speed
        GetComponent<CharacterController>().speed = newSpeed;
    }
    
    public void ApplyDamage()
    {
        // Calculate damage with variables
        float finalDamage = playerDamage + (playerHealth * 0.1f);
        playerHealth.Value -= finalDamage;
    }
}
```

---

## Troubleshooting

### Common Issues

1. **Tweens not working**: Ensure VariableTweener component is attached and enabled
2. **Events not firing**: Check that GameEventListener is enabled and properly configured
3. **Variables not updating**: Verify that the Value property is being set, not just assigned
4. **Performance issues**: Reduce Tween Scale or event frequency

### Debug Tips

1. **Use Debug.Log**: Add logging to OnChange and OnFinished events
2. **Check Inspector**: Verify all references are properly set
3. **Validate Events**: Use the inspector to test event connections
4. **Monitor Performance**: Use Unity Profiler to identify bottlenecks

---

This manual covers the core functionality of the Shababeek Core system. For more advanced usage patterns or specific questions, refer to the code examples or contact the development team.

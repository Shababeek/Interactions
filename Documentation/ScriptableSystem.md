# Scriptable System Manual

## Overview

The Scriptable System provides a powerful framework for creating scriptable variables and events that can be shared across scenes and components. It uses UniRx for reactive programming and supports various data types with automatic event raising when values change.

## Core Components

### **ScriptableVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > ScriptableVariable`

**What it does**: Base class for scriptable variables that can be observed and raised as game events.

**How it works**: Provides a reactive variable system where changes automatically raise events. Implements IObservable<T> for UniRx integration and inherits from GameEvent for event system compatibility.

**Inspector Properties**:

- **Value** (T) [Generic]
  - **What it does**: The current value of the variable
  - **Default**: Default value for type T
  - **When to use**: Set the initial value for the variable

**Setup Example**:
1. Right-click in Project window
2. Select Create > Shababeek > Scriptable System > Variables > ScriptableVariable
3. Set the initial value
4. Use in components that need reactive variables

### **GameEvent**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Events > GameEvent`

**What it does**: Base class for game events that can be raised and observed without data.

**How it works**: Provides a simple event system that can be raised and subscribed to. Uses UnityEvents for editor integration and UniRx for reactive programming.

**Inspector Properties**:

- **On Raised** (UnityEvent)
  - **What it does**: Event raised when the event is triggered
  - **Default**: Empty
  - **When to use**: Add Unity events to respond to this game event

**Setup Example**:
1. Right-click in Project window
2. Select Create > Shababeek > Scriptable System > Events > GameEvent
3. Add Unity events in the On Raised field
4. Raise the event from code or other components

## Variable Types

### **BoolVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > BoolVariable`

**What it does**: Scriptable variable that stores a boolean value with reactive updates.

**Inspector Properties**:

- **Value** (bool)
  - **What it does**: The boolean value of the variable
  - **Default**: false
  - **When to use**: Set the initial boolean value

**Usage Example**:
```csharp
// Subscribe to value changes
boolVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"Bool changed to: {value}"))
    .AddTo(this);

// Set the value
boolVariable.Value = true;
```

### **IntVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > IntVariable`

**What it does**: Scriptable variable that stores an integer value with reactive updates.

**Inspector Properties**:

- **Value** (int)
  - **What it does**: The integer value of the variable
  - **Default**: 0
  - **When to use**: Set the initial integer value

**Usage Example**:
```csharp
// Subscribe to value changes
intVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"Int changed to: {value}"))
    .AddTo(this);

// Set the value
intVariable.Value = 42;
```

### **FloatVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > FloatVariable`

**What it does**: Scriptable variable that stores a float value with reactive updates and arithmetic operators.

**Inspector Properties**:

- **Value** (float)
  - **What it does**: The float value of the variable
  - **Default**: 0.0f
  - **When to use**: Set the initial float value

**Usage Example**:
```csharp
// Subscribe to value changes
floatVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"Float changed to: {value}"))
    .AddTo(this);

// Set the value
floatVariable.Value = 3.14f;

// Use arithmetic operators
float result = floatVariable + 10f;
floatVariable++;
```

### **StringVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > StringVariable`

**What it does**: Scriptable variable that stores a string value with reactive updates.

**Inspector Properties**:

- **Value** (string)
  - **What it does**: The string value of the variable
  - **Default**: Empty string
  - **When to use**: Set the initial string value

**Usage Example**:
```csharp
// Subscribe to value changes
stringVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"String changed to: {value}"))
    .AddTo(this);

// Set the value
stringVariable.Value = "Hello World";
```

### **Vector3Variable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > Vector3Variable`

**What it does**: Scriptable variable that stores a Vector3 value with reactive updates.

**Inspector Properties**:

- **Value** (Vector3)
  - **What it does**: The Vector3 value of the variable
  - **Default**: Vector3.zero
  - **When to use**: Set the initial Vector3 value

**Usage Example**:
```csharp
// Subscribe to value changes
vector3Variable.OnValueChanged
    .Subscribe(value => Debug.Log($"Vector3 changed to: {value}"))
    .AddTo(this);

// Set the value
vector3Variable.Value = new Vector3(1, 2, 3);
```

### **ColorVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > ColorVariable`

**What it does**: Scriptable variable that stores a Color value with reactive updates.

**Inspector Properties**:

- **Value** (Color)
  - **What it does**: The Color value of the variable
  - **Default**: Color.white
  - **When to use**: Set the initial Color value

**Usage Example**:
```csharp
// Subscribe to value changes
colorVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"Color changed to: {value}"))
    .AddTo(this);

// Set the value
colorVariable.Value = Color.red;
```

### **QuaternionVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > QuaternionVariable`

**What it does**: Scriptable variable that stores a Quaternion value with reactive updates.

**Inspector Properties**:

- **Value** (Quaternion)
  - **What it does**: The Quaternion value of the variable
  - **Default**: Quaternion.identity
  - **When to use**: Set the initial Quaternion value

**Usage Example**:
```csharp
// Subscribe to value changes
quaternionVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"Quaternion changed to: {value}"))
    .AddTo(this);

// Set the value
quaternionVariable.Value = Quaternion.Euler(0, 90, 0);
```

### **TransformVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > TransformVariable`

**What it does**: Scriptable variable that stores a Transform reference with reactive updates.

**Inspector Properties**:

- **Value** (Transform)
  - **What it does**: The Transform reference of the variable
  - **Default**: null
  - **When to use**: Set the initial Transform reference

**Usage Example**:
```csharp
// Subscribe to value changes
transformVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"Transform changed to: {value?.name}"))
    .AddTo(this);

// Set the value
transformVariable.Value = someGameObject.transform;
```

### **GameObjectVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > GameObjectVariable`

**What it does**: Scriptable variable that stores a GameObject reference with reactive updates and utility methods.

**Inspector Properties**:

- **Value** (GameObject)
  - **What it does**: The GameObject reference of the variable
  - **Default**: null
  - **When to use**: Set the initial GameObject reference

**Usage Example**:
```csharp
// Subscribe to value changes
gameObjectVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"GameObject changed to: {value?.name}"))
    .AddTo(this);

// Set the value
gameObjectVariable.Value = someGameObject;

// Use utility methods
gameObjectVariable.SetActive(true);
var component = gameObjectVariable.GetComponent<Renderer>();
bool hasComponent = gameObjectVariable.HasComponent<Collider>();
Transform transform = gameObjectVariable.Transform;
bool isActive = gameObjectVariable.IsActive;
```

### **AudioClipVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > AudioClipVariable`

**What it does**: Scriptable variable that stores an AudioClip reference with reactive updates.

**Inspector Properties**:

- **Value** (AudioClip)
  - **What it does**: The AudioClip reference of the variable
  - **Default**: null
  - **When to use**: Set the initial AudioClip reference

**Usage Example**:
```csharp
// Subscribe to value changes
audioClipVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"AudioClip changed to: {value?.name}"))
    .AddTo(this);

// Set the value
audioClipVariable.Value = someAudioClip;
```

### **TextVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > TextVariable`

**What it does**: Scriptable variable that stores a string value with reactive updates, designed for text content.

**Inspector Properties**:

- **Value** (string)
  - **What it does**: The string value of the variable
  - **Default**: Empty string
  - **When to use**: Set the initial text value

**Usage Example**:
```csharp
// Subscribe to value changes
textVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"Text changed to: {value}"))
    .AddTo(this);

// Set the value
textVariable.Value = "New text content";
```

### **EnumVariable**

**Menu Location**: `Assets > Create > Shababeek > Scriptable System > Variables > EnumVariable`

**What it does**: Scriptable variable that stores an enum value with reactive updates.

**Inspector Properties**:

- **Value** (int)
  - **What it does**: The enum value of the variable (stored as int)
  - **Default**: 0
  - **When to use**: Set the initial enum value

**Usage Example**:
```csharp
// Subscribe to value changes
enumVariable.OnValueChanged
    .Subscribe(value => Debug.Log($"Enum changed to: {value}"))
    .AddTo(this);

// Set the value
enumVariable.Value = (int)MyEnum.SomeValue;
```

## Variable References

### **VariableReference**

**What it does**: Base class for variable references that can point to either a ScriptableVariable or use a constant value.

**How it works**: Provides type-safe variable handling with UniRx integration. Can reference a ScriptableVariable or use a constant value.

**Inspector Properties**:

- **Use Constant** (bool)
  - **What it does**: Whether to use a constant value instead of a variable reference
  - **Default**: false
  - **When to use**: Enable to use constant value

- **Variable** (ScriptableVariable)
  - **What it does**: Reference to a ScriptableVariable
  - **Default**: None
  - **When to use**: Assign when Use Constant is false

- **Constant Value** (T) [Generic]
  - **What it does**: Constant value to use when Use Constant is true
  - **Default**: Default value for type T
  - **When to use**: Set when Use Constant is true

### **BoolReference**

**What it does**: A reference that can point to either a BoolVariable or use a constant boolean value.

**Inspector Properties**:

- **Use Constant** (bool)
  - **What it does**: Whether to use a constant value instead of a variable reference
  - **Default**: false
  - **When to use**: Enable to use constant value

- **Variable** (BoolVariable)
  - **What it does**: Reference to a BoolVariable
  - **Default**: None
  - **When to use**: Assign when Use Constant is false

- **Constant Value** (bool)
  - **What it does**: Constant boolean value to use when Use Constant is true
  - **Default**: false
  - **When to use**: Set when Use Constant is true

### **IntReference**

**What it does**: A reference that can point to either an IntVariable or use a constant integer value.

**Inspector Properties**:

- **Use Constant** (bool)
  - **What it does**: Whether to use a constant value instead of a variable reference
  - **Default**: false
  - **When to use**: Enable to use constant value

- **Variable** (IntVariable)
  - **What it does**: Reference to an IntVariable
  - **Default**: None
  - **When to use**: Assign when Use Constant is false

- **Constant Value** (int)
  - **What it does**: Constant integer value to use when Use Constant is true
  - **Default**: 0
  - **When to use**: Set when Use Constant is true

### **FloatReference**

**What it does**: A reference that can point to either a FloatVariable or use a constant float value.

**Inspector Properties**:

- **Use Constant** (bool)
  - **What it does**: Whether to use a constant value instead of a variable reference
  - **Default**: false
  - **When to use**: Enable to use constant value

- **Variable** (FloatVariable)
  - **What it does**: Reference to a FloatVariable
  - **Default**: None
  - **When to use**: Assign when Use Constant is false

- **Constant Value** (float)
  - **What it does**: Constant float value to use when Use Constant is true
  - **Default**: 0.0f
  - **When to use**: Set when Use Constant is true

## Event System

### **GameEventListener**

**Menu Location**: `Component > Shababeek > Scriptable System > Game Event Listener`

**What it does**: Listens to game events and raises Unity events accordingly.

**How it works**: Provides a bridge between the ScriptableObject-based GameEvent system and Unity's event system. Supports multiple GameEvents with their associated Unity events.

**Inspector Properties**:

- **Game Event List** (List<GameEventWithEvents>)
  - **What it does**: List of game events to listen to with their associated Unity events
  - **Default**: Empty list
  - **When to use**: Add game events and their corresponding Unity events

**Setup Example**:
1. Add the GameEventListener component to a GameObject
2. Add game events to the Game Event List
3. Configure Unity events for each game event
4. The component will automatically listen and respond to game events

### **VariableToUIBinder**

**Menu Location**: `Component > Shababeek > Scriptable System > Variable To UI Binder`

**What it does**: Binds scriptable variables to UI elements for automatic updates.

**How it works**: Automatically updates UI elements when scriptable variables change. Supports various UI components like Text, Image, Slider, etc.

**Inspector Properties**:

- **Variable** (ScriptableVariable)
  - **What it does**: The scriptable variable to bind to UI
  - **Default**: None
  - **When to use**: Assign the variable to bind

- **UI Component** (Component)
  - **What it does**: The UI component to update
  - **Default**: None
  - **When to use**: Assign the UI component to update

**Setup Example**:
1. Add the VariableToUIBinder component to a GameObject
2. Assign a ScriptableVariable to the Variable field
3. Assign a UI component to the UI Component field
4. The UI will automatically update when the variable changes

## Advanced Configuration

### **Reactive Programming**

**UniRx Integration**:
- All variables implement IObservable<T>
- Automatic event raising on value changes
- Reactive programming patterns supported

**Event Subscription**:
```csharp
// Subscribe to variable changes
variable.OnValueChanged
    .Subscribe(value => Debug.Log($"Value changed to: {value}"))
    .AddTo(this);

// Subscribe to game events
gameEvent.OnRaised
    .Subscribe(_ => Debug.Log("Event raised!"))
    .AddTo(this);
```

### **Variable Operations**

**Arithmetic Operations** (FloatVariable):
- Addition, subtraction, multiplication, division
- Increment and decrement operators
- Comparison operators

**Utility Methods** (GameObjectVariable):
- SetActive(bool)
- GetComponent<T>()
- HasComponent<T>()
- Transform property
- IsActive property

### **Type Safety**

**Generic Constraints**:
- Type-safe variable references
- Compile-time type checking
- Automatic type conversion

**Reference Safety**:
- Null-safe operations
- Automatic null checking
- Safe default values

## Troubleshooting

### **Common Issues**

**Variable not updating:**
- Check that the variable reference is properly assigned
- Verify event subscriptions are active
- Ensure the variable is being set correctly
- Test with direct value assignment

**Events not firing:**
- Verify GameEventListener is properly configured
- Check that Unity events are assigned
- Ensure game events are being raised
- Test with manual event raising

**UI not updating:**
- Check VariableToUIBinder configuration
- Verify UI component assignment
- Ensure variable changes are being detected
- Test with direct UI updates

**Type mismatches:**
- Verify variable types match expected types
- Check generic constraints
- Ensure proper type conversion
- Test with explicit type casting

### **Performance Issues**

**Memory leaks:**
- Properly dispose of UniRx subscriptions
- Use AddTo(this) for automatic disposal
- Avoid circular references
- Clean up event subscriptions

**Event overhead:**
- Limit the number of active subscriptions
- Use appropriate event filtering
- Optimize event frequency
- Monitor event system performance

## Code Integration

While this manual focuses on Unity Editor usage, here's how to work with scriptable variables in code:

```csharp
// Get a reference to a scriptable variable
FloatVariable floatVar = GetComponent<FloatVariable>();

// Subscribe to value changes
floatVar.OnValueChanged
    .Subscribe(value => Debug.Log($"Float changed to: {value}"))
    .AddTo(this);

// Set the value
floatVar.Value = 42.0f;

// Raise the variable (triggers events)
floatVar.Raise();
```

### **Variable References**

```csharp
// Get a reference to a variable reference
FloatReference floatRef = GetComponent<FloatReference>();

// Get the current value (from variable or constant)
float currentValue = floatRef.Value;

// Set the value (if using variable)
if (!floatRef.UseConstant)
{
    floatRef.Variable.Value = 10.0f;
}
```

### **Game Events**

```csharp
// Get a reference to a game event
GameEvent gameEvent = GetComponent<GameEvent>();

// Subscribe to the event
gameEvent.OnRaised
    .Subscribe(_ => Debug.Log("Event raised!"))
    .AddTo(this);

// Raise the event
gameEvent.Raise();
```

### **Custom Variable Types**

```csharp
using UnityEngine;
using Shababeek.Core;

[CreateAssetMenu(menuName = "Shababeek/Scriptable System/Variables/CustomVariable")]
public class CustomVariable : ScriptableVariable<CustomType>
{
    // Custom logic can be added here
    public void CustomMethod()
    {
        Debug.Log($"Custom method called with value: {Value}");
    }
}

// Custom type
[System.Serializable]
public struct CustomType
{
    public string name;
    public int value;
    
    public override string ToString()
    {
        return $"{name}: {value}";
    }
}
```

### **Event Integration**

```csharp
// Subscribe to multiple variables
Observable.CombineLatest(
    boolVariable.OnValueChanged,
    intVariable.OnValueChanged,
    floatVariable.OnValueChanged
)
.Subscribe(tuple => {
    var (boolVal, intVal, floatVal) = tuple;
    Debug.Log($"All variables changed: {boolVal}, {intVal}, {floatVal}");
})
.AddTo(this);

// Filter events
gameEvent.OnRaised
    .Where(_ => someCondition)
    .Subscribe(_ => Debug.Log("Filtered event!"))
    .AddTo(this);
``` 
# ScriptableVariable<T> — Manual

## Purpose
A ScriptableObject-based variable that can be observed for changes and used for decoupled data flow in Unity. Supports all primitive types and custom types.

## Usage
- Create via the Unity menu (e.g., Create > Shababeek > Scriptable System > Variables > FloatVariable).
- Assign as a reference in components to share data between objects.
- Subscribe to value changes using UniRx.

## Inspector Properties
- **value** (T): The current value of the variable.

## Key Members
- `T Value` — Get/set the value. Setting triggers observers.
- `IObservable<T> OnValueChanged` — Subscribe to value changes.
- `void Raise()` — Notify observers of the current value.
- `void Raise(T data)` — Notify observers with a specific value.

## Example
```csharp
public FloatVariable myFloat;

void Start() {
    myFloat.OnValueChanged.Subscribe(val => Debug.Log($"Value changed: {val}"));
    myFloat.Value = 5f;
}
```

---

# ScriptableVariable (base class)

- Abstract base for all ScriptableVariable<T> types.
- Inherits from GameEvent. 
# Shababeek Interaction System — Scripting Reference

## Introduction
This reference covers the main classes, interfaces, and scripting patterns for extending and customizing the Shababeek Interaction System.

## Key Classes & Interfaces
- `InteractableBase` — Base class for all interactable objects.
- `InteractorBase` — Base class for all interactors (hands, raycasters, triggers).
- `ScriptableVariable<T>` — Observable, serializable variable.
- `GameEvent<T>` — Observable, serializable event.
- `FeedbackSystem` — Add haptic, audio, or visual feedback to interactions.
- `Config` — Central ScriptableObject for system-wide settings.

## Example Usage

### Creating a Custom Interactable
```csharp
using Shababeek.Interactions;

public class MyCustomInteractable : InteractableBase {
    protected override void Activate() {
        // Custom activation logic
    }
    protected override void StartHover() {
        // Custom hover logic
    }
    protected override void EndHover() {
        // Custom hover end logic
    }
    protected override bool Select() {
        // Custom selection logic
        return false;
    }
    protected override void DeSelected() {
        // Custom deselection logic
    }
}
```

### Listening to a GameEvent
```csharp
using Shababeek.Core;
using UniRx;

public class EventListener : MonoBehaviour {
    public GameEvent myEvent;
    void Start() {
        myEvent.OnRaised.Subscribe(_ => Debug.Log("Event raised!"));
    }
}
```

### Using a ScriptableVariable
```csharp
using Shababeek.Core;
using UniRx;

public class VariableWatcher : MonoBehaviour {
    public FloatVariable myFloat;
    void Start() {
        myFloat.OnValueChanged.Subscribe(val => Debug.Log($"Value changed: {val}"));
    }
}
```

## More
- See the User Manual and Getting Started guide for additional details and advanced usage. 
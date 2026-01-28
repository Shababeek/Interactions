# Scriptable System Window — Debug & Monitoring Tool

> **Quick Reference**
> **Menu Path:** Shababeek > Scriptable System Window
> **Use For:** Viewing, debugging, and testing ScriptableVariables and GameEvents
> **Requires:** Editor only

---

## What It Does

The **Scriptable System Window** is an editor tool that provides a centralized view of all ScriptableVariables and GameEvents in your project. It helps you:

- ✅ View all variables referenced in the current scene
- ✅ Monitor variable values at runtime
- ✅ Edit variable values during play mode
- ✅ Fire GameEvents for testing
- ✅ Quickly locate variables in the Project window
- ✅ Filter and search through large collections

**Key Benefits:**
- **Debugging:** See all reactive data in one place during runtime
- **Testing:** Fire events manually to test game logic
- **Organization:** Variables grouped by their parent asset (VariableContainer)
- **Efficiency:** Quick search and type filtering

---

## Opening the Window

**Menu:** `Shababeek > Scriptable System Window`

The window can be docked anywhere in your Unity layout.

---

## Window Layout

### Toolbar (Top Row)

| Element | Description |
|---------|-------------|
| **Variables Tab** | Shows all ScriptableVariables |
| **Events Tab** | Shows all GameEvents |
| **Search Field** | Filter items by name (case-insensitive) |
| **Clear Button (✕)** | Reset search filter |

### Filter Bar (Second Row)

| Element | Description |
|---------|-------------|
| **Type Dropdown** | Filter variables by type (Int, Float, Bool, etc.) |
| **Scene Refs Only** | Toggle between scene-referenced items and all project items |
| **Refresh Button** | Manually refresh the data |

---

## Variables Tab

Displays all ScriptableVariables, organized by their parent asset.

### Variable Entry

Each variable row shows:

| Element | Description |
|---------|-------------|
| **→ Button** | Select and ping the variable in Project window |
| **Name** | Variable asset name |
| **[Type]** | Variable type (Int, Float, Bool, etc.) |
| **Value Field** | Current value (editable at runtime) |
| **● Indicator** | Shows if referenced in current scene |

### Grouping

Variables are grouped by their parent asset:
- **📦 Container Name** — Variables inside a VariableContainer
- **Standalone Assets** — Individual variable assets not in a container

Click the **Select** button on a group header to select the parent asset.

### Editing Values at Runtime

During play mode, you can directly edit variable values:
- **Int/Float:** Type new numbers
- **Bool:** Toggle checkbox
- **String:** Type new text
- **Color:** Use color picker
- **Vector2/Vector3:** Displayed as read-only

Changes are applied immediately and trigger reactive updates.

---

## Events Tab

Displays all GameEvents in the project (excluding ScriptableVariables which inherit from GameEvent).

### Event Entry

Each event row shows:

| Element | Description |
|---------|-------------|
| **→ Button** | Select and ping the event in Project window |
| **Name** | Event asset name |
| **Fire Button** | Raise the event (runtime only) |
| **● Indicator** | Shows if referenced in current scene |

### Firing Events

During play mode, click the **Fire** button to manually raise any GameEvent. This is useful for:
- Testing event listeners
- Triggering sequences manually
- Debugging event-driven logic

---

## Filtering & Search

### Search

Type in the search field to filter by name. Search is case-insensitive and matches partial names.

**Examples:**
- "health" matches "PlayerHealth", "EnemyHealth", "HealthPickup"
- "spawn" matches "SpawnEvent", "EnemySpawned"

### Type Filter (Variables Tab)

Filter variables by their data type:

| Filter | Matches |
|--------|---------|
| All | All variable types |
| Int | IntVariable |
| Float | FloatVariable |
| Bool | BoolVariable |
| Text | TextVariable (strings) |
| Vector2 | Vector2Variable, Vector2IntVariable |
| Vector3 | Vector3Variable |
| Color | ColorVariable |
| Other | All other types (Gradient, AnimationCurve, etc.) |

### Scene Refs Only Toggle

- **Enabled (default):** Shows only variables/events that are referenced by components in the currently loaded scene(s)
- **Disabled:** Shows all variables/events in the entire project

This is helpful for:
- Focusing on relevant data during debugging
- Finding orphaned variables (toggle off and compare)
- Understanding what data a scene uses

---

## Auto-Refresh Behavior

The window automatically refreshes when:
- The hierarchy changes (objects added/removed)
- Play mode starts or stops
- You manually click the Refresh button

A cooldown prevents excessive refreshes during rapid changes.

---

## Use Cases

### Debugging Runtime State

1. Open the window during play mode
2. Enable "Scene Refs Only" to focus on active data
3. Watch values update in real-time as you play
4. Edit values to test edge cases

### Testing Event Flow

1. Switch to the Events tab
2. Find the event you want to test
3. Click **Fire** to trigger it
4. Observe the game's response

### Finding Variable References

1. Search for a variable name
2. Click **→** to select it in the Project window
3. Use Unity's "Find References In Scene" for detailed usage

### Organizing Variables

1. Toggle off "Scene Refs Only" to see all variables
2. Identify variables that aren't grouped in containers
3. Consider organizing related variables into VariableContainers

---

## Tips & Best Practices

💡 **Dock the window** next to your Game view for easy runtime monitoring.

💡 **Use search** with type filters together for precise filtering.

💡 **Fire events** to test individual behaviors without playing through the whole game.

💡 **Check the ● indicator** to quickly see which items are actually used in your scene.

💡 **Use containers** — Variables grouped in VariableContainers are easier to manage in the window.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| No variables shown | "Scene Refs Only" enabled with no references | Toggle off "Scene Refs Only" or add references to scene |
| Values not editable | Not in play mode | Enter play mode to edit runtime values |
| Fire button grayed out | Not in play mode | Enter play mode to fire events |
| Variable missing | Not a ScriptableVariable asset | Ensure it inherits from ScriptableVariable |
| Groups not showing | Variables are standalone | Use VariableContainers to group related variables |

---

## Related Documentation

- [Scriptable Variables](ScriptableVariables.md) — Creating and using variables
- [Variable Container](VariableContainer.md) — Grouping variables together
- [Binders](Binders.md) — Connecting variables to components

---

**Last Updated:** January 2026
**Component Version:** 1.0.0

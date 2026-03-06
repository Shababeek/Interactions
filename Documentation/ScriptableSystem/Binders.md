# Interaction Binders — Connect Interactions to Reactive Variables

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Scriptable System > [BinderName]
> **Use For:** Connecting interactables and sockets to the reactive variable system
> **Requires:** com.shababeek.reactivevars package, ScriptableVariable

---

## What It Does

These binders connect VR interaction system outputs (levers, wheels, buttons, sockets, etc.) to the ScriptableVariable and GameEvent system from the **com.shababeek.reactivevars** package. When an interaction occurs, the binder automatically updates reactive variables and fires events.

This enables:
- Tutorial systems that respond to interactions
- UI that updates when objects are grabbed
- Game logic triggered by socket insertions
- Complex sequences coordinated through reactive variables

**For a complete list of generic binders** (Transform, Physics, UI, Animator, Light, Camera, etc.), see the **com.shababeek.reactivevars** package documentation.

---

## Interactable-to-Variable Binders

These binders connect constrained interactables (rotatable/movable objects) to numeric variables.

### Lever To Variable Binder

Binds a LeverInteractable's output to float variables.

| Setting | Description |
|---------|-------------|
| **Lever** | Source LeverInteractable |
| **Normalized Output** | FloatVariable for 0-1 value |
| **Angle Output** | FloatVariable for actual angle in degrees |
| **Invert Output** | Flip the output direction |
| **Output Multiplier** | Scale the output values |

**Use Case:** Pull a lever to open a door — normalized output controls door rotation angle.

---

### Wheel To Variable Binder

Binds a WheelInteractable's output to float variables.

| Setting | Description |
|---------|-------------|
| **Wheel** | Source WheelInteractable |
| **Normalized Output** | FloatVariable for -1 to 1 value |
| **Angle Output** | FloatVariable for actual angle in degrees |
| **Invert Output** | Flip the output direction |
| **Output Multiplier** | Scale the output values |

**Use Case:** Rotate a valve — output controls valve opening and connected effects.

---

### Dial To Variable Binder

Binds a DialInteractable's discrete step output to variables and events.

| Setting | Description |
|---------|-------------|
| **Dial** | Source DialInteractable |
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

**Use Cases:**
- Combination locks (each step is a digit)
- Rotary selectors (mode selection)
- Safe dials with discrete positions

---

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

**Use Case:** Control a vehicle with a joystick — output maps to movement input.

---

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

**Use Case:** Detect when a drawer opens to trigger lighting or UI updates.

---

## Generic Interactable Binders

These binders work with any InteractableBase-derived component.

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

**Use Cases:**
- Play sound when object is grabbed
- Highlight UI when hovered
- Update animation state on use

---

### Interactable To Bool Binder

Simplified binder focusing only on BoolVariables for interaction states.

| Setting | Description |
|---------|-------------|
| **Interactable** | Source InteractableBase |
| **Hovered Variable** | BoolVariable set true when hovered |
| **Selected Variable** | BoolVariable set true when selected/grabbed |
| **Used Variable** | BoolVariable set true during use |
| **Reset On Disable** | Reset all to false when disabled |

**Use Cases:**
- UI indicators showing interaction state
- Enabling features based on selection
- Analytics tracking of interactions

---

### Interactable To Event Binder

Simplified binder focusing only on GameEvents for interaction events.

| Setting | Description |
|---------|-------------|
| **Interactable** | Source InteractableBase |
| **On Hover Start Event** | GameEvent raised when hover starts |
| **On Hover End Event** | GameEvent raised when hover ends |
| **On Selected Event** | GameEvent raised when selected |
| **On Deselected Event** | GameEvent raised when deselected |
| **On Use Start Event** | GameEvent raised when use starts |
| **On Use End Event** | GameEvent raised when use ends |

**Use Cases:**
- Triggering game logic when objects are grabbed
- Playing effects through event listeners
- Updating UI/analytics when interactions occur

---

## Socket Binders

These binders connect Socket System events to the reactive variable system.

### Socket To Bool Binder

Binds a Socket's state to BoolVariables and GameEvents.

| Setting | Description |
|---------|-------------|
| **Socket** | Source Socket component |
| **Has Object Variable** | BoolVariable true when socket contains object |
| **On Inserted Event** | GameEvent raised when object inserted |
| **On Removed Event** | GameEvent raised when object removed |

**Use Cases:**
- Light indicator when slot is filled
- Trigger puzzle logic when all sockets filled
- Play sound effects on socket events

---

### Socket To Event Binder

Simplified version focusing only on GameEvents.

| Setting | Description |
|---------|-------------|
| **Socket** | Source Socket component |
| **On Inserted Event** | GameEvent raised on insert |
| **On Removed Event** | GameEvent raised on remove |

---

### Socketable To Bool Binder

Binds a Socketable object's state to BoolVariable.

| Setting | Description |
|---------|-------------|
| **Socketable** | Source Socketable component |
| **Is Socketed Variable** | BoolVariable true when in a socket |
| **On Socketed Event** | GameEvent raised when socketed |
| **On Unsocketed Event** | GameEvent raised when unsocketed |

**Use Cases:**
- Battery shows "installed" indicator
- Key disappears when inserted in lock
- Tool shows different state when holstered

---

### Socketable To Event Binder

Simplified version focusing only on GameEvents.

| Setting | Description |
|---------|-------------|
| **Socketable** | Source Socketable component |
| **On Socketed Event** | GameEvent raised when socketed |
| **On Unsocketed Event** | GameEvent raised when unsocketed |

---

## Common Workflows

### Tutorial with Variable Feedback

1. Create a FloatVariable "LeverPosition"
2. Add **LeverToVariableBinder** to your lever
3. Create a UI progress bar bound to "LeverPosition" (using a generic binder from ReactiveVars)
4. Create a Sequence that advances when "LeverPosition" reaches 1.0

---

### Puzzle Lock System

1. Create IntVariables for each dial: "Dial1", "Dial2", "Dial3"
2. Add **DialToVariableBinder** to each dial
3. Create a Sequence that checks if all dials match target values
4. When all match, unlock the door

---

### Drawer Auto-Close Detection

1. Create a BoolVariable "DrawerOpen"
2. Add **DrawerToVariableBinder** to the drawer
3. Bind UI element visibility to "DrawerOpen"
4. Create sound effect triggered by "OnClosed" event

---

## Tips

💡 **Use normalized outputs** for UI progress bars and visual feedback.

💡 **Use angle outputs** for calculations and constraints.

💡 **Interactable Event Binder** is the most versatile — use it for any interactable.

💡 **Socket binders** work great with puzzle mechanics — check HasObject to validate solutions.

💡 **Per-step dial events** enable combo-lock patterns without custom code.

💡 **Invert Output** simplifies reversed controls (e.g., lever up = close instead of open).

⚠️ **Variable Types Must Match** — Ensure FloatVariable, IntVariable, etc. match the binder output type.

⚠️ **Deadzone Tuning** — For joysticks, adjust deadzone if input feels too sensitive.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Variable not updating | Binding disabled or missing reference | Check Inspector, assign variable |
| Values wrong direction | Output direction inverted | Toggle Invert Output |
| No socket events firing | Socket/Socketable references missing | Verify both references in inspector |
| Dial events not firing | Step events array empty or wrong length | Create array matching number of steps |
| Joystick output noisy | Deadzone too low | Increase deadzone value |

---

## Related Documentation

- **[Interaction System Overview](../../UserManual.md)** — Core concepts
- **[Socket System](../SocketSystem/SocketSystem.md)** — Socket and Socketable components
- **[Constrained Interactables](../Interactables/ConstrainedInteractables.md)** — Lever, Wheel, Dial, Joystick, Drawer
- **[ReactiveVars Package](https://github.com/Shababeek/ReactiveVars)** — Complete documentation on variables, events, and generic binders
- **[Interaction Sequences](SequencingSystem.md)** — Using binders with tutorials

---

**Last Updated:** March 2026
**Component Version:** 1.5.0

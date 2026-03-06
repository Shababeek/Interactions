# Interaction Sequences — Interaction-Specific Actions

> **Quick Reference**
> **Create Sequence:** See com.shababeek.reactivevars package documentation
> **Actions:** GrabHoldAction, SocketAction, SocketableAction, InsertionAction, LeverAction, DrawerAction, WheelAction, JoystickAction, DialAction, GazeAction, TimerAction, ControllerButtonAction, InteractionAction
> **Use For:** Tutorials and guided workflows using interaction-specific actions

---

## What It Does

This document covers the **interaction-specific sequence actions** available in the Shababeek Interaction System. These actions work with the Sequencing System from the **com.shababeek.reactivevars** package.

For complete documentation on the core Sequencing System (Sequence, Step, SequenceBehaviour, BranchingSequence, Graph View), see the **com.shababeek.reactivevars** package documentation.

**This document covers:**
- Actions that detect and respond to interactions
- Actions for hand presence and gestures
- Actions for timing and controller input
- Practical examples combining interactions with sequences

---

## Interaction-Specific Action Types

### GrabHoldAction

Detects when an object is grabbed and held.

| Setting | Description |
|---------|-------------|
| **Grabable** | The Grabable component to monitor |
| **Require Hand** | Only count grabs by specific hand (Left/Right/Either) |

**Behavior:**
- Completes immediately when object is grabbed
- Optionally waits for release to re-trigger

**Use Case:** "Pick up the wrench and hold it" — step completes when wrench is grabbed.

---

### InteractionAction

Completes when a specific interaction event occurs on any InteractableBase.

| Setting | Description |
|---------|-------------|
| **Interactable Object** | Object to monitor |
| **Interaction Type** | Selection, Activation, Hover, Deselection |

| Type | Description |
|------|-------------|
| **Selection** | Object is grabbed or selected |
| **Activation** | Object is used (trigger pressed while holding) |
| **Hover** | Hand enters interaction range |
| **Deselection** | Object is released |

**Use Case:** "Select the red button to continue" or "Press the trigger to activate the device".

---

### SocketAction

Completes when an object is placed into a socket.

| Setting | Description |
|---------|-------------|
| **Socket** | The socket to monitor |

**Behavior:**
- Completes immediately when any object is inserted
- Triggers on any insertion (use SocketableAction for specific objects)

**Use Case:** "Insert the key into the lock" — completes when any object is placed in the lock socket.

---

### SocketableAction

Completes when a specific socketable object is placed in a socket.

| Setting | Description |
|---------|-------------|
| **Socketable** | The specific object to monitor |

**Behavior:**
- Completes only when this specific object is inserted
- Fails if a different object is inserted first

**Use Case:** "Insert the battery into the correct slot" — completes only for the specific battery.

---

### InsertionAction

Completes when an object is inserted (deprecated in favor of SocketAction/SocketableAction, but still available).

| Setting | Description |
|---------|-------------|
| **Interactable** | Object that should be inserted |

---

### Constrained Interactable Actions

These actions monitor constrained interactables (levers, wheels, dials, drawers, joysticks) reaching specific positions.

### LeverAction

Waits for a lever to reach a specific angle or normalized position.

| Setting | Description |
|---------|-------------|
| **Lever** | The LeverInteractable to monitor |
| **Target Normalized** | Target position 0-1 (0 = start, 1 = fully extended) |
| **Tolerance** | Allowed deviation from target (0-1) |
| **Wait For Release** | Step completes only after reaching target AND releasing |

**Use Case:** "Pull the lever all the way down" — monitor until normalized position reaches 1.0.

---

### WheelAction

Waits for a wheel to reach a specific rotation angle.

| Setting | Description |
|---------|-------------|
| **Wheel** | The WheelInteractable to monitor |
| **Target Angle** | Target rotation in degrees |
| **Tolerance** | Allowed angle deviation |
| **Direction** | Forward, Backward, or Either |

**Use Case:** "Rotate the valve 90 degrees clockwise" — monitor until target angle is reached.

---

### DialAction

Waits for a dial to reach a specific step.

| Setting | Description |
|---------|-------------|
| **Dial** | The DialInteractable to monitor |
| **Target Step** | The step index to reach (0-based) |
| **Wait For Confirmation** | Complete only after user releases (confirms selection) |

**Use Case:** "Set the dial to position 3" — monitor until step 3 is selected and confirmed.

---

### DrawerAction

Waits for a drawer to reach a specific state.

| Setting | Description |
|---------|-------------|
| **Drawer** | The DrawerInteractable to monitor |
| **Target State** | Open, Closed, or Specific Position |
| **Position** | Target position if "Specific Position" is selected |
| **Tolerance** | Allowed position deviation |

**Use Case:** "Open the desk drawer" — completes when drawer crosses open threshold.

---

### JoystickAction

Waits for a joystick input to reach specific thresholds.

| Setting | Description |
|---------|-------------|
| **Joystick** | The JoystickInteractable to monitor |
| **Target Direction** | Direction to reach (or magnitude threshold) |
| **Threshold** | Minimum input magnitude |

**Use Case:** "Move the joystick to the left" — detects input in specific direction.

---

## Hand Presence Actions

### GazeAction

Completes when player looks at a specific collider or object.

| Setting | Description |
|---------|-------------|
| **Target Collider** | Collider to gaze at |
| **Duration** | Time to look at target (0 = instant) |
| **Camera** | Camera used for raycast (auto-detect) |

**Use Case:** "Look at the instruction panel" — triggers when player gazes at a specific object.

---

## Input & Timing Actions

### TimerAction

Waits for a specified duration.

| Setting | Description |
|---------|-------------|
| **Duration** | Time to wait in seconds |
| **Auto Start** | Begin counting immediately when step starts |

**Use Case:** "Wait 5 seconds before continuing" — pause sequence for UI animations, dialogue, etc.

---

### ControllerButtonAction

Completes when a specific controller button is pressed.

| Setting | Description |
|---------|-------------|
| **Config** | XR configuration reference |
| **Hand** | Left, Right, or Either |
| **Button** | Trigger or Grip |
| **Press Type** | Down, Up, or Held |

**Use Case:** "Press the trigger to continue" — wait for user to press a button.

---

## Combining Actions in Sequences

A single step can have multiple actions. The step completes when **any** of the conditions are met (OR logic). To require multiple conditions, create separate steps.

### Example: Tutorial Sequence

```
Step 1: "Welcome" (TimerAction - 2 seconds)
Step 2: "Look at the tool" (GazeAction - look at wrench)
Step 3: "Pick up the tool" (GrabHoldAction - grab wrench)
Step 4: "Pull the lever" (LeverAction - reach position 1.0)
Step 5: "Well done!" (TimerAction - 2 seconds)
```

### Example: Multi-Step Assembly

```
Step 1: "Grab Part A" (GrabHoldAction - Part A)
Step 2: "Insert Part A into Slot 1" (SocketableAction - Part A into Slot 1)
Step 3: "Grab Part B" (GrabHoldAction - Part B)
Step 4: "Insert Part B into Slot 2" (SocketableAction - Part B into Slot 2)
Step 5: "Unlock the door" (ControllerButtonAction - Press trigger)
Step 6: "Complete!" (TimerAction - 2 seconds)
```

### Example: Dial Combination Lock

```
Step 1: "Set dial 1 to 5" (DialAction - Target step 5, wait for confirmation)
Step 2: "Set dial 2 to 3" (DialAction - Target step 3, wait for confirmation)
Step 3: "Set dial 3 to 7" (DialAction - Target step 7, wait for confirmation)
Step 4: "Success!" (TimerAction - 2 seconds, play unlock sound)
```

---

## Best Practices

💡 **Use Normalized Outputs** — LeverAction and WheelAction work better with normalized positions (0-1) for intuitive targets.

💡 **Add Tolerance** — Always include a small tolerance (0.05-0.1) to prevent user frustration with exact positioning.

💡 **Combine with Audio** — Use TimerAction with audio clips to provide narration between interaction steps.

💡 **Fallback Timers** — Add a long TimerAction as a fallback to advance if user gets stuck (good UX).

💡 **Clear Instructions** — Step audio should match action requirements ("Pull the lever down" not just "Interact").

💡 **Wait For Confirmation** — For dial/joystick steps, use confirmation waits so users feel control.

💡 **Socket Validation** — Use SocketableAction for specific objects in puzzles; use SocketAction for optional insertions.

⚠️ **Action Timing** — GazeAction requires a camera reference; auto-detection works if camera is tagged.

⚠️ **Collider Requirements** — GazeAction, SocketAction, and SocketableAction need proper colliders and references.

⚠️ **Button Mapping** — Verify ControllerButtonAction button names match your input configuration.

---

## Common Patterns

### Guided Assembly

1. InteractionAction (Selection) - "Grab the part"
2. GazeAction - "Look at the slot"
3. SocketableAction - "Insert the part"
4. Repeat for each part
5. ControllerButtonAction - "Press trigger to activate"

### Valve Sequence

1. GazeAction - "Look at the valve"
2. InteractionAction (Selection) - "Grab the wheel"
3. WheelAction - "Turn clockwise 90 degrees"
4. ControllerButtonAction - "Release when ready"

### Lock Tutorial

1. TimerAction - "Welcome to the lock tutorial"
2. DialAction - "Set dial 1 to position 5"
3. DialAction - "Set dial 2 to position 3"
4. DialAction - "Set dial 3 to position 7"
5. SocketAction - "Insert the key"
6. TimerAction - "Lock opened! Well done!"

---

## Interaction System Integration

These actions integrate seamlessly with all Interaction System components:

| Component | Compatible Actions |
|-----------|-------------------|
| **Grabable** | GrabHoldAction, InteractionAction (Selection/Activation) |
| **Throwable** | GrabHoldAction, InteractionAction |
| **Switch** | InteractionAction (Selection/Activation) |
| **VRButton** | InteractionAction (Activation) |
| **Lever** | LeverAction, InteractionAction (Selection) |
| **Wheel** | WheelAction, InteractionAction (Selection) |
| **Dial** | DialAction, InteractionAction (Selection) |
| **Drawer** | DrawerAction, InteractionAction (Selection) |
| **Joystick** | JoystickAction, InteractionAction (Selection) |
| **Socket** | SocketAction, SocketableAction |
| **Hand** | GazeAction, ControllerButtonAction |

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Action won't complete | Target condition impossible | Verify target value is achievable |
| GazeAction not working | Camera not found or no collider | Assign camera and verify collider exists |
| SocketAction fires immediately | Object already in socket | Clear socket before starting step |
| DialAction misses target | Tolerance too strict | Increase tolerance value |
| ControllerButtonAction unresponsive | Wrong button name or hand | Check Config input bindings |
| Multiple actions compete | No action fires correctly | Simplify to one action per step |

---

## Related Documentation

- **[Interaction System Overview](../../UserManual.md)** — System architecture
- **[Constrained Interactables](../Interactables/ConstrainedInteractables.md)** — Lever, Wheel, Dial, Drawer, Joystick
- **[Socket System](../SocketSystem/SocketSystem.md)** — Socket and Socketable components
- **[Interaction Binders](Binders.md)** — Connecting interactions to reactive variables
- **[Feedback System](FeedbackSystem.md)** — Adding haptics and audio to sequences
- **[ReactiveVars Package](https://github.com/Shababeek/ReactiveVars)** — Core Sequencing System, Sequence, Step, SequenceBehaviour, BranchingSequence

---

**Last Updated:** March 2026
**Component Version:** 1.5.0

# Sequencing System — Tutorials and Guided Workflows

> **Quick Reference**
> **Create Sequence:** Right-click > Create > Shababeek > Sequencing > Sequence
> **Behaviour Component:** Add Component > SequenceBehaviour
> **Use For:** Step-by-step tutorials, training sequences, guided experiences

---

## What It Does

The **Sequencing System** provides a framework for creating ordered sequences of steps that guide users through interactive experiences. Each sequence contains multiple steps, and each step can have actions that control when the step completes.

**Perfect for:**
- ✅ VR tutorials and onboarding
- ✅ Training simulations
- ✅ Guided assembly tasks
- ✅ Interactive storytelling
- ✅ Step-by-step procedures

---

## Core Concepts

### Sequence
A **Sequence** is a ScriptableObject asset that contains an ordered list of steps. Sequences execute steps one at a time in order.

### Step
A **Step** represents a single stage in a sequence. Each step can:
- Play audio when started
- Wait for specific actions to complete
- Trigger Unity events on start and completion

### Action
An **Action** defines the condition for completing a step. Examples: wait for interaction, detect gesture, wait for timer.

---

## Quick Example

> **Goal:** Create a "Pick up the tool" tutorial step

<!-- TODO: Add sequence-tutorial.gif -->
*Step-by-step sequence showing welcome, gaze, interaction, and completion steps*

```
Sequence: "Tool Tutorial"
├── Step 1: "Welcome" (TimerAction - 2 seconds)
├── Step 2: "Look at tool" (GazeAction)
├── Step 3: "Pick up tool" (InteractionAction - Selection)
└── Step 4: "Well done!" (TimerAction - 2 seconds)
```

---

## Creating Sequences

### Step 1: Create a Sequence Asset

1. Right-click in Project window
2. Select **Create > Shababeek > Sequencing > Sequence**
3. Name your sequence (e.g., "TutorialSequence")

**Sequence Settings:**
| Setting | Description |
|---------|-------------|
| **Pitch** | Audio pitch multiplier (0.1-2.0) |
| **Volume** | Audio volume level (0-1) |

### Step 2: Add SequenceBehaviour

1. Create an empty GameObject in your scene
2. **Add Component > SequenceBehaviour**
3. Assign your Sequence asset to the **Sequence** field

**SequenceBehaviour Settings:**
| Setting | Description |
|---------|-------------|
| **Sequence** | The sequence asset to execute |
| **Star On Awake** | Start automatically when scene loads |
| **On Sequence Started** | Event raised when sequence begins |
| **On Sequence Completed** | Event raised when sequence finishes |

### Step 3: Configure Steps

Steps are created and configured through the Sequence Editor window. Each step can have:
- Audio clip to play
- Actions that must complete
- Events to trigger on start/complete

---

## Step Configuration

Each step has these settings:

### Audio Settings
| Setting | Description |
|---------|-------------|
| **Audio Clip** | Audio to play when step starts |
| **Audio Only** | Complete step when audio finishes |
| **Audio Delay** | Delay before starting audio (seconds) |
| **Override Pitch** | Use custom pitch for this step |
| **Pitch** | Custom pitch value (0.1-2.0) |

### Behavior Settings
| Setting | Description |
|---------|-------------|
| **Can Be Finished Before Started** | Allow pre-completion |

### Events
| Event | When Fired |
|-------|------------|
| **On Started** | When step begins |
| **On Completed** | When step completes |

---

## Action Types

### ActivatingAction
Completes when an interactable is used.

| Setting | Description |
|---------|-------------|
| **Interactable Object** | The interactable to monitor |

**Use Case:** "Pick up the wrench to continue"

---

### AnimationAction
Triggers animation and waits for completion.

| Setting | Description |
|---------|-------------|
| **Animation Trigger Name** | Animator parameter name |
| **Animator** | The Animator component |

**Important:** Call `AnimationEnded()` from an Animation Event to complete the step.

---

### TimerAction
Waits for a specified duration.

| Setting | Description |
|---------|-------------|
| **Time** | Duration in seconds |
| **Start On Enable** | Auto-start when enabled |
| **On Complete** | Event when timer finishes |

**Use Case:** "Wait 5 seconds before continuing"

---

### GazeAction
Completes when player looks at a collider.

**Use Case:** "Look at the instruction panel"

---

### InteractionAction
Completes when a specific interaction occurs.

| Setting | Description |
|---------|-------------|
| **Interactable Object** | Object to monitor |
| **Interaction Type** | Selection, Activation, Hover, etc. |

**Use Case:** "Select the red button to continue"

---

### GestureAction
Completes when hand gestures are detected.

| Setting | Description |
|---------|-------------|
| **Gestures** | List of gestures to detect |
| **Gesture Type** | Fist, Open Hand, Pointing, Thumbs Up, etc. |
| **Target Hand** | Left or Right |
| **Hold Duration** | Time gesture must be held |
| **Tolerance** | Detection sensitivity (0-1) |
| **Require All Gestures** | All must be detected |

**Use Case:** "Make a fist with your right hand"

---

### InsertionAction
Completes when object is placed correctly.

| Setting | Description |
|---------|-------------|
| **Interactable** | Object to insert |

**Use Case:** "Insert the key into the lock"

---

### ControllerButtonAction
Completes when controller button is pressed.

| Setting | Description |
|---------|-------------|
| **Config** | XR configuration reference |
| **Hand** | Left or Right controller |
| **Button** | Trigger or Grip |

**Use Case:** "Press the trigger to continue"

---

### TriggerAction
Completes when object enters a trigger.

| Setting | Description |
|---------|-------------|
| **Object Tag** | Tag of object to detect (optional) |
| **On Trigger Enter** | Event when object enters |

**Use Case:** "Place the item in the container"

---

## Event Listeners

### StepEventListener
Listens to step events and triggers Unity events.

| Setting | Description |
|---------|-------------|
| **Step List** | Steps to monitor with events |

**Use Case:** Show UI elements when specific steps start.

### MultiStepListener
Listens to multiple steps simultaneously.

| Setting | Description |
|---------|-------------|
| **Steps** | Array of steps to monitor |
| **On Started** | Event when any step starts |
| **On Ended** | Event when any step completes |

---

## Common Patterns

### Tutorial Flow
```
Step 1: TimerAction (2s) - "Welcome to the tutorial"
Step 2: GazeAction - "Look at the control panel"
Step 3: InteractionAction (Selection) - "Select the tool"
Step 4: InteractionAction (Activation) - "Use the tool"
Step 5: TimerAction (3s) - "Great job! Complete"
```

### Training Sequence
```
Step 1: AnimationAction - Demonstrate technique
Step 2: GestureAction - User replicates gesture
Step 3: InteractionAction - User performs action
Step 4: Validation + completion audio
```

### Multi-Object Assembly
```
Step 1: InteractionAction (Part A - Selection)
Step 2: InsertionAction (Part A into Slot 1)
Step 3: InteractionAction (Part B - Selection)
Step 4: InsertionAction (Part B into Slot 2)
```

---

## Best Practices

💡 **One Sequence Per Tutorial**
Keep sequences focused and manageable.

💡 **Meaningful Names**
Name sequences and steps descriptively for easy maintenance.

💡 **Clear Audio**
Use clear, concise audio instructions.

💡 **Step Granularity**
Break complex tasks into smaller steps.

💡 **Visual Feedback**
Combine actions with UI feedback (highlights, arrows).

💡 **Timer Fallbacks**
Add timers as backup completion for stuck users.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Step won't complete | Verify action conditions are achievable |
| Audio not playing | Check AudioClip is assigned, verify volume |
| Actions not triggering | Verify target references are assigned |
| Sequence not starting | Check Star On Awake or call StartQuest() |

---

## Scripting API

### Starting a Sequence
```csharp
SequenceBehaviour behaviour = GetComponent<SequenceBehaviour>();
behaviour.StartQuest();
```

### Subscribing to Events
```csharp
sequence.OnRaisedData
    .Where(status => status == SequenceStatus.Completed)
    .Subscribe(_ => Debug.Log("Sequence completed"))
    .AddTo(this);
```

### Manually Completing a Step
```csharp
step.CompleteStep();
```

### Custom Actions
```csharp
public class MyCustomAction : AbstractSequenceAction
{
    [SerializeField] private float customValue;

    protected override void OnStepStatusChanged(SequenceStatus status)
    {
        if (status == SequenceStatus.Started)
        {
            // Your logic here
            // Call Step.CompleteStep() when condition is met
        }
    }
}
```

---

## FAQ

**Can I skip steps?**
Not directly. Use `step.CompleteStep()` programmatically.

**Can steps run in parallel?**
No, steps run sequentially. Use separate sequences for parallel execution.

**How do I reset a sequence?**
Disable and re-enable SequenceBehaviour, or call `sequence.Begin()`.

**Can I modify sequences at runtime?**
Sequences are ScriptableObjects. Clone them if runtime changes are needed.

---

## Related Documentation

- [Socket System](../SocketSystem/SocketSystem.md) — Validate insertions
- [Feedback System](FeedbackSystem.md) — Add feedback to steps
- [Grabable](../Interactables/Grabable.md) — Interaction detection
- [Quick Start Guide](../GettingStarted/QuickStart.md) — Basic setup

---

**Last Updated:** January 2026
**Component Version:** 1.0.0

# Sequencing System - User Manual

## Overview

The Sequencing System provides a framework for creating ordered sequences of steps that guide users through interactive experiences. Each sequence contains multiple steps, and each step can have actions that control when the step completes.

## Table of Contents

1. [Core Concepts](#core-concepts)
2. [Creating Sequences](#creating-sequences)
3. [Steps and Actions](#steps-and-actions)
4. [Action Types](#action-types)
5. [Event Listeners](#event-listeners)
6. [Best Practices](#best-practices)

---

## Core Concepts

### Sequence
A **Sequence** is a ScriptableObject asset that contains an ordered list of steps. Sequences execute steps one at a time in order.

**Key Features:**
- Audio support with pitch and volume control
- Step progression tracking
- Event notifications for sequence start/completion

### Step
A **Step** represents a single stage in a sequence. Each step can:
- Play audio when started
- Wait for specific actions to complete
- Trigger Unity events on start and completion
- Override sequence audio settings

### Actions
**Actions** are components that define the conditions for completing a step. Examples include:
- Waiting for an interaction
- Detecting a gesture
- Waiting for a timer
- Monitoring variable values

---

## Creating Sequences

### Step 1: Create a Sequence Asset

1. Right-click in the Project window
2. Select `Create → Shababeek → Sequencing → Sequence`
3. Name your sequence (e.g., "TutorialSequence")

**Inspector Settings:**
- **Pitch**: Audio pitch multiplier (0.1-2.0)
- **Volume**: Audio volume level (0-1)

### Step 2: Add a SequenceBehaviour to Your Scene

1. Create an empty GameObject in your scene
2. Add the `SequenceBehaviour` component
3. Assign your Sequence asset to the `Sequence` field

**Inspector Settings:**
- **Sequence**: The sequence asset to execute
- **Star On Awake**: Start sequence automatically when scene loads
- **On Sequence Started**: Unity event raised when sequence begins
- **On Sequence Completed**: Unity event raised when sequence finishes

### Step 3: Configure Steps (In Editor)

Steps are created and configured through the Sequence Editor window. Each step appears as a node that you can configure with:
- Audio clip to play
- Actions that must complete
- Events to trigger

---

## Steps and Actions

### Step Configuration

Each step in a sequence has the following settings:

**Audio Settings:**
- **Audio Clip**: Audio to play when step starts
- **Audio Only**: Complete step when audio finishes
- **Audio Delay**: Delay before starting audio (seconds)
- **Override Pitch**: Use custom pitch for this step
- **Pitch**: Custom pitch value (0.1-2.0)

**Behavior Settings:**
- **Can Be Finished Before Started**: Allow pre-completion

**Events:**
- **On Started**: Unity event when step begins
- **On Completed**: Unity event when step completes

---

## Action Types

### ActivatingAction
Completes the step when an interactable object is used.

**Inspector Settings:**
- **Interactable Object**: The interactable to monitor

**Use Case:** "Pick up the wrench to continue"

### AnimationAction
Triggers an animation and waits for it to complete.

**Inspector Settings:**
- **Animation Trigger Name**: Parameter name in animator
- **Animator**: The Animator component to control

**Use Case:** "Play door opening animation"

**Important:** Call `AnimationEnded()` from an Animation Event to complete the step.

### TimerAction
Waits for a specified duration.

**Inspector Settings:**
- **Time**: Duration in seconds
- **Start On Enable**: Auto-start when enabled
- **On Complete**: Event when timer finishes

**Use Case:** "Wait 5 seconds before continuing"

### GazeAction
Completes when player looks at a collider.

**Use Case:** "Look at the instruction panel"

### InteractionAction
Completes when a specific interaction occurs.

**Inspector Settings:**
- **Interactable Object**: Object to monitor
- **Interaction Type**: Type of interaction (Selection, Activation, Hover, etc.)

**Use Case:** "Select the red button to continue"

### GestureAction
Completes when specific hand gestures are detected.

**Inspector Settings:**
- **Gestures**: List of gestures to detect
  - **Gesture Type**: Fist, Open Hand, Pointing, Thumbs Up, etc.
  - **Target Hand**: Left or Right
  - **Hold Duration**: Time gesture must be held
  - **Require Hold**: Whether gesture must be held
  - **Tolerance**: Detection sensitivity (0-1)
- **Require All Gestures**: All gestures must be detected
- **Check Interval**: How often to check (seconds)
- **Continuous Check**: Keep checking until detected

**Use Case:** "Make a fist with your right hand"

### InsertionAction
Completes when an object is placed in the correct location.

**Inspector Settings:**
- **Interactable**: Object to insert

**Use Case:** "Insert the key into the lock"

### ControllerButtonAction
Completes when a controller button is pressed.

**Inspector Settings:**
- **Config**: XR configuration reference
- **Hand**: Left or Right controller
- **Button**: Trigger or Grip

**Use Case:** "Press the trigger to continue"

### TriggerAction
Completes when an object enters a trigger collider.

**Inspector Settings:**
- **Object Tag**: Tag of object to detect (optional)
- **On Trigger Enter**: Event when object enters

**Use Case:** "Place the item in the container"

---

## Event Listeners

### StepEventListener
Listens to step events and triggers Unity events.

**Inspector Settings:**
- **Step List**: List of steps to monitor
  - **Step**: The step to listen to
  - **On Step Started**: Event when step starts
  - **On Step Completed**: Event when step completes

**Use Case:** Show UI elements when specific steps start

### MultiStepListener
Listens to multiple steps simultaneously.

**Inspector Settings:**
- **Steps**: Array of steps to monitor
- **On Started**: Event when any step starts
- **On Ended**: Event when any step completes

**Use Case:** Toggle a group of GameObjects based on step status

---

## Best Practices

### Sequence Organization
1. **One Sequence Per Tutorial**: Keep sequences focused and manageable
2. **Meaningful Names**: Name sequences and steps descriptively
3. **Audio Guidelines**: Use clear, concise audio instructions
4. **Step Granularity**: Break complex tasks into smaller steps

### Audio Management
1. **Consistent Volume**: Use sequence-level volume for consistency
2. **Pitch Variation**: Override pitch for emphasis on important steps
3. **Audio Delays**: Add small delays to prevent overlap
4. **Audio-Only Steps**: Use for narration-only steps

### Action Usage
1. **Single Responsibility**: Each action should have one clear purpose
2. **Timer Fallbacks**: Consider adding timers as backup completion methods
3. **Visual Feedback**: Combine actions with UI feedback
4. **Testing**: Test each step thoroughly in isolation

### Performance
1. **Event Cleanup**: Listeners automatically dispose subscriptions
2. **Audio Management**: Sequence reuses a single AudioSource
3. **Action Efficiency**: Actions only run when their step is active

### Error Handling
1. **Null Checks**: Always check for null references in custom actions
2. **Fallback Logic**: Provide alternate completion paths
3. **Debug Logging**: Use Debug.Log to track sequence progress during development

---

## Common Patterns

### Tutorial Flow
```
Step 1: TimerAction (2s) - "Welcome to the tutorial"
Step 2: GazeAction - "Look at the control panel"
Step 3: InteractionAction (Selection) - "Select the tool"
Step 4: InteractionAction (Activation) - "Use the tool on the workpiece"
Step 5: TimerAction (3s) - "Great job! Tutorial complete"
```

### Training Sequence
```
Step 1: AnimationAction - Demonstrate technique
Step 2: GestureAction - User replicates gesture
Step 3: InteractionAction - User performs action
Step 4: Validation via custom logic
Step 5: Completion audio and next sequence trigger
```

### Multi-Object Interaction
```
Step 1: InteractionAction (Object A - Selection)
Step 2: InsertionAction (Object A into Slot 1)
Step 3: InteractionAction (Object B - Selection)
Step 4: InsertionAction (Object B into Slot 2)
```

---

## Troubleshooting

### Step Won't Complete
- Check that action conditions are achievable
- Verify action component is attached to GameObject with StepEventListener
- Ensure audio-only steps have audio clips assigned
- Check console for error messages

### Audio Not Playing
- Verify AudioClip is assigned to step
- Check sequence volume and pitch settings
- Ensure AudioSource is not muted in scene
- Check if audio delay is too long

### Actions Not Triggering
- Verify the action's target references are assigned
- Check that interactables have required components
- Ensure step is actually started (check status in inspector)
- Verify StepEventListener is attached if required

### Sequence Not Starting
- Check `Star On Awake` is enabled if using automatic start
- Verify sequence asset is assigned to SequenceBehaviour
- Call `StartQuest()` method if starting manually
- Check that sequence has steps defined

---

## Scripting Reference

### Starting a Sequence
```csharp
SequenceBehaviour sequenceBehaviour = GetComponent<SequenceBehaviour>();
sequenceBehaviour.StartQuest();
```

### Subscribing to Sequence Events
```csharp
sequence.OnRaisedData
    .Where(status => status == SequenceStatus.Started)
    .Subscribe(_ => Debug.Log("Sequence started"))
    .AddTo(this);

sequence.OnRaisedData
    .Where(status => status == SequenceStatus.Completed)
    .Subscribe(_ => Debug.Log("Sequence completed"))
    .AddTo(this);
```

### Subscribing to Step Events
```csharp
step.OnRaisedData
    .Where(status => status == SequenceStatus.Started)
    .Subscribe(_ => Debug.Log("Step started"))
    .AddTo(this);
```

### Manually Completing a Step
```csharp
step.CompleteStep();
```

### Playing Custom Audio in Sequence
```csharp
AudioPlayerInSequence audioPlayer = GetComponent<AudioPlayerInSequence>();
audioPlayer.Play();
```

---

## Advanced Usage

### Custom Actions
Create custom actions by extending `AbstractSequenceAction`:

```csharp
public class MyCustomAction : AbstractSequenceAction
{
    [Tooltip("Custom parameter")]
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

### Conditional Branching
Use VariableAction to create conditional sequences based on ScriptableVariables.

### Dynamic Sequence Loading
Load different sequences based on user progress or choices:
```csharp
public Sequence[] sequences;
public SequenceBehaviour sequenceBehaviour;

public void LoadSequence(int index)
{
    sequenceBehaviour.sequence = sequences[index];
    sequenceBehaviour.StartQuest();
}
```

---

## Inspector Reference

### SequenceBehaviour
- **Sequence**: Sequence asset to execute
- **Star On Awake**: Auto-start on load
- **On Sequence Started**: Event on start
- **On Sequence Completed**: Event on complete

### Sequence Asset
- **Pitch**: Audio pitch (0.1-2.0)
- **Volume**: Audio volume (0-1)
- **Steps**: Configured in Sequence Editor

### Step (In Sequence Editor)
- **Audio Clip**: Audio to play
- **Can Be Finished Before Started**: Pre-completion toggle
- **Audio Only**: Complete on audio finish
- **Audio Delay**: Start delay (seconds)
- **Override Pitch**: Custom pitch toggle
- **Pitch**: Custom pitch value
- **On Started**: Start event
- **On Completed**: Complete event

---

## FAQ

**Q: Can I skip steps in a sequence?**  
A: Not directly. You can complete steps programmatically using `step.CompleteStep()`.

**Q: Can steps run in parallel?**  
A: No, steps run sequentially. Use separate sequences for parallel execution.

**Q: How do I reset a sequence?**  
A: Disable and re-enable the SequenceBehaviour component, or call `sequence.Begin()`.

**Q: Can I use the same sequence multiple times?**  
A: Yes, sequences can be reused and restarted.

**Q: How do I add custom data to steps?**  
A: Create custom action scripts that extend AbstractSequenceAction.

**Q: Can I modify sequences at runtime?**  
A: Sequences are ScriptableObjects and modifications persist. Clone them if runtime changes are needed.

---

## Examples

See the ExampleScene folder for complete working examples of:
- Basic tutorial sequences
- Complex multi-step interactions
- Custom actions
- Event listener patterns



# Sequence System Manual

## Overview

The Sequence System provides a sophisticated framework for creating and managing sequential interactions, tutorials, and guided experiences in VR. It supports both linear sequences and complex branching logic with audio playback, step management, and analytics tracking.

## Core Components

### **Sequence**

**Menu Location**: `Assets > Create > Shababeek > Sequencing > Sequence`

**What it does**: A linear sequence of steps that execute in order with audio playback support.

**How it works**: Manages a list of steps that execute sequentially. Each step can have audio clips, events, and completion conditions. The sequence handles audio source management and step transitions.

**Inspector Properties**:

- **Pitch** (float)
  - **What it does**: Audio pitch for the sequence's audio source
  - **Range**: 0.1 - 2.0
  - **Default**: 1.0
  - **When to use**: Adjust for different audio playback speeds

- **Volume** (float)
  - **What it does**: Audio volume for the sequence's audio source
  - **Range**: 0.0 - 1.0
  - **Default**: 0.5
  - **When to use**: Set the overall volume for sequence audio

- **Steps** (List<Step>) [Hidden in Inspector]
  - **What it does**: List of steps in this sequence
  - **Default**: Empty list
  - **When to use**: Automatically managed by the editor

- **Current Step Index** (int) [Read-only]
  - **What it does**: Index of the currently active step
  - **Default**: 0
  - **When to use**: Read-only property for debugging

**Setup Example**:
1. Right-click in Project window
2. Select Create > Shababeek > Sequencing > Sequence
3. Add steps to the sequence
4. Configure audio settings
5. Test the sequence in play mode

### **Step**

**What it does**: Represents a single step in a sequence with audio playback and completion logic.
**How it works**: Each step can play audio clips, trigger events, and complete based on various conditions. Steps are managed by their parent sequence.

**Inspector Properties**:

- **Audio Clip** (AudioClip)
  - **What it does**: The audio clip to play during this step
  - **Default**: None
  - **When to use**: Set to provide audio feedback for the step

- **Can Be Finished Before Started** (bool)
  - **What it does**: Whether the user can complete the step before it starts
  - **Default**: false
  - **When to use**: Enable for steps that can be pre-completed

- **Audio Only** (bool)
  - **What it does**: Whether the step completes when the audio finishes playing
  - **Default**: false
  - **When to use**: Enable for audio-only steps

- **Audio Delay** (float)
  - **What it does**: Delay before playing the audio clip (in seconds)
  - **Range**: 0.0 - 10.0
  - **Default**: 0.1
  - **When to use**: Add delay for timing synchronization

- **On Started** (UnityEvent)
  - **What it does**: Event raised when the step starts
  - **Default**: Empty
  - **When to use**: Add actions to perform when step begins

- **On Completed** (UnityEvent)
  - **What it does**: Event raised when the step completes
  - **Default**: Empty
  - **When to use**: Add actions to perform when step ends

- **Override Pitch** (bool)
  - **What it does**: Whether to override the sequence's pitch for this step
  - **Default**: false
  - **When to use**: Enable for custom pitch per step

- **Pitch** (float)
  - **What it does**: Custom pitch for this step's audio
  - **Range**: 0.1 - 2.0
  - **Default**: 1.0
  - **When to use**: Set when Override Pitch is enabled

### **SequenceBehaviour**

**Menu Location**: `Component > Shababeek > Sequencing > Sequence Behaviour`

**What it does**: Manages the execution of sequences in the scene with lifecycle management and analytics tracking.

**How it works**: Handles sequence startup, timing, event subscriptions, and provides Unity events for sequence lifecycle. Can automatically start sequences and track analytics.

**Inspector Properties**:

- **Sequence** (Sequence)
  - **What it does**: The sequence to be executed by this behaviour
  - **Default**: None
  - **When to use**: Assign the sequence asset to execute

- **Start On Space** (bool) [Hidden in Inspector]
  - **What it does**: Whether to start the sequence when spacebar is pressed
  - **Default**: false
  - **When to use**: Enable for manual testing

- **Started** (bool) [Read-only]
  - **What it does**: Indicates whether the sequence has been started
  - **Default**: false
  - **When to use**: Read-only property for debugging

- **Star On Awake** (bool)
  - **What it does**: Whether to automatically start the sequence on awake
  - **Default**: false
  - **When to use**: Enable for automatic sequence startup

- **Delay** (float) [Hidden in Inspector]
  - **What it does**: Delay before starting the sequence (in seconds)
  - **Range**: 0.0 - 60.0
  - **Default**: 0.0
  - **When to use**: Add delay when Star On Awake is enabled

- **On Sequence Started** (UnityEvent)
  - **What it does**: Event raised when the sequence starts
  - **Default**: Empty
  - **When to use**: Add actions to perform when sequence begins

- **On Sequence Completed** (UnityEvent)
  - **What it does**: Event raised when the sequence completes
  - **Default**: Empty
  - **When to use**: Add actions to perform when sequence ends

- **Steps** (List<StepEventPair>) [Hidden in Inspector]
  - **What it does**: Internal list of step event pairs for the sequence
  - **Default**: Empty list
  - **When to use**: Automatically managed by the system

- **Step Listeners** (List<StepEventListener>) [Hidden in Inspector]
  - **What it does**: Internal list of step event listeners for the sequence
  - **Default**: Empty list
  - **When to use**: Automatically managed by the system

- **Listener** (bool)
  - **What it does**: Whether to enable analytics tracking for this sequence
  - **Default**: false
  - **When to use**: Enable for analytics data collection

**Setup Example**:
1. Add the SequenceBehaviour component to a GameObject
2. Assign a Sequence asset to the Sequence field
3. Configure startup settings (Star On Awake, Delay)
4. Add Unity events for sequence lifecycle
5. Test the sequence execution

### **BranchingSequence**

**Menu Location**: `Assets > Create > Shababeek > Sequencing > Branching Sequence`

**What it does**: A branching sequence that supports multiple paths and transitions based on conditions.

**How it works**: Similar to behavior trees and state machines, with visual elements for clear editing. Supports complex branching logic while maintaining the drag-and-drop ScriptableObject workflow.

**Inspector Properties**:

- **Allow Parallel Execution** (bool)
  - **What it does**: Whether to allow multiple active steps at once
  - **Default**: false
  - **When to use**: Enable for parallel step execution

- **Auto Advance** (bool)
  - **What it does**: Whether to automatically advance to the next step when current step completes
  - **Default**: true
  - **When to use**: Enable for automatic progression

- **Steps** (List<BranchingStep>)
  - **What it does**: All steps in this branching sequence
  - **Default**: Empty list
  - **When to use**: Add branching steps to the sequence

- **Bool Variables** (List<BoolReference>)
  - **What it does**: Boolean variables that can be used in transition conditions
  - **Default**: Empty list
  - **When to use**: Add boolean variables for branching logic

- **Int Variables** (List<IntReference>)
  - **What it does**: Integer variables that can be used in transition conditions
  - **Default**: Empty list
  - **When to use**: Add integer variables for branching logic

- **Float Variables** (List<FloatReference>)
  - **What it does**: Float variables that can be used in transition conditions
  - **Default**: Empty list
  - **When to use**: Add float variables for branching logic

- **Step Positions X** (List<float>) [Hidden in Inspector]
  - **What it does**: Visual positions for editor layout (X coordinate)
  - **Default**: Empty list
  - **When to use**: Automatically managed by the editor

- **Step Positions Y** (List<float>) [Hidden in Inspector]
  - **What it does**: Visual positions for editor layout (Y coordinate)
  - **Default**: Empty list
  - **When to use**: Automatically managed by the editor

- **Active Steps** (List<BranchingStep>) [Read-only]
  - **What it does**: Currently active steps (read-only)
  - **Default**: Empty list
  - **When to use**: Read-only property for debugging

- **Execution Path** (List<string>) [Read-only]
  - **What it does**: Current execution path (read-only)
  - **Default**: Empty list
  - **When to use**: Read-only property for debugging

**Setup Example**:
1. Right-click in Project window
2. Select Create > Shababeek > Sequencing > Branching Sequence
3. Add branching steps and configure transitions
4. Add control variables for branching logic
5. Configure parallel execution settings

### **BranchingSequenceBehaviour**

**Menu Location**: `Component > Shababeek > Sequencing > Branching Sequence Behaviour`

**What it does**: Manages the execution of branching sequences in the scene.

**How it works**: Similar to SequenceBehaviour but designed for branching sequences with complex logic and multiple execution paths.

**Inspector Properties**:

- **Branching Sequence** (BranchingSequence)
  - **What it does**: The branching sequence to be executed
  - **Default**: None
  - **When to use**: Assign the branching sequence asset

- **Auto Start** (bool)
  - **What it does**: Whether to automatically start the sequence
  - **Default**: false
  - **When to use**: Enable for automatic startup

- **Start Delay** (float)
  - **What it does**: Delay before starting the sequence
  - **Range**: 0.0 - 60.0
  - **Default**: 0.0
  - **When to use**: Add delay when Auto Start is enabled

## Sequence Actions

### **AbstractSequenceAction**

**What it does**: Base class for sequence actions that can be attached to steps.

**How it works**: Provides the foundation for creating custom actions that respond to step status changes.

**Inspector Properties**:

- **Step Event Listener** (StepEventListener)
  - **What it does**: Reference to the step event listener component
  - **Default**: None
  - **When to use**: Automatically assigned

### **VariableAction**

**Menu Location**: `Assets > Create > Shababeek > Sequencing > Actions > VariableAction`

**What it does**: Action that can set, check, increment, or decrement scriptable variables.

**How it works**: Supports various operations on scriptable variables with comparison logic for conditional execution.

**Inspector Properties**:

- **Variable Reference** (ScriptableVariable)
  - **What it does**: The scriptable variable to operate on
  - **Default**: None
  - **When to use**: Assign the variable to modify

- **Operation** (VariableOperation)
  - **What it does**: The operation to perform on the variable
  - **Options**: Set, Check, Increment, Decrement
  - **Default**: Set
  - **When to use**: Choose the operation type

- **Comparison Type** (ComparisonType)
  - **What it does**: Type of comparison for check operations
  - **Options**: Equals, NotEquals, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual, Contains, StartsWith
  - **Default**: Equals
  - **When to use**: Set when Operation is Check

- **String Value** (string) [Hidden in Inspector]
  - **What it does**: String value for string variables
  - **Default**: Empty
  - **When to use**: Set for string operations

- **Float Value** (float) [Hidden in Inspector]
  - **What it does**: Float value for float variables
  - **Default**: 0.0
  - **When to use**: Set for float operations

- **Int Value** (int) [Hidden in Inspector]
  - **What it does**: Integer value for int variables
  - **Default**: 0
  - **When to use**: Set for int operations

- **Bool Value** (bool) [Hidden in Inspector]
  - **What it does**: Boolean value for bool variables
  - **Default**: false
  - **When to use**: Set for bool operations

- **Vector3 Value** (Vector3) [Hidden in Inspector]
  - **What it does**: Vector3 value for Vector3 variables
  - **Default**: Zero
  - **When to use**: Set for Vector3 operations

- **Quaternion Value** (Quaternion) [Hidden in Inspector]
  - **What it does**: Quaternion value for Quaternion variables
  - **Default**: Identity
  - **When to use**: Set for Quaternion operations

- **Color Value** (Color) [Hidden in Inspector]
  - **What it does**: Color value for Color variables
  - **Default**: White
  - **When to use**: Set for Color operations

### **SequenceOrderAction**

**Menu Location**: `Assets > Create > Shababeek > Sequencing > Actions > SequenceOrderAction`

**What it does**: Action that tracks the order of interactions and validates them against a predefined sequence.

**How it works**: Monitors interactions with interactables and validates that they occur in the correct order. Supports timeout, skip options, and reset functionality.

**Inspector Properties**:

- **Ordered Interactions** (List<OrderedInteraction>)
  - **What it does**: List of interactions that must occur in order
  - **Default**: Empty list
  - **When to use**: Define the required interaction sequence

- **Reset On Wrong Order** (bool)
  - **What it does**: Whether to reset progress when wrong interaction occurs
  - **Default**: true
  - **When to use**: Enable for strict order enforcement

- **Allow Skip** (bool)
  - **What it does**: Whether to allow skipping interactions
  - **Default**: false
  - **When to use**: Enable for flexible progression

- **Timeout Duration** (float)
  - **What it does**: Time limit for completing the sequence
  - **Range**: 0.0 - 300.0
  - **Default**: 30.0
  - **When to use**: Set when Use Timeout is enabled

- **Use Timeout** (bool)
  - **What it does**: Whether to use timeout for sequence completion
  - **Default**: false
  - **When to use**: Enable for time-limited sequences

## Advanced Configuration

### **Step Event Listeners**

**StepEventListener**:
- Listens to individual step events
- Can trigger actions when specific steps start or complete
- Supports custom event handling

**MultiStepListener**:
- Listens to multiple steps simultaneously
- Can track progress across multiple steps
- Supports complex event combinations

### **Audio Management**

**Audio Source Management**:
- Sequences automatically create audio sources
- Each step can override pitch settings
- Supports audio-only step completion

**Audio Timing**:
- Configurable audio delays per step
- Automatic audio completion detection
- Pitch and volume control

### **Analytics Integration**

**Event Tracking**:
- Automatic sequence start/complete tracking
- Step transition monitoring
- Performance metrics collection

**Custom Analytics**:
- Custom event data collection
- Timing information tracking
- User interaction analysis

## Troubleshooting

### **Common Issues**

**Sequence not starting:**
- Check that SequenceBehaviour is properly configured
- Verify Sequence asset is assigned
- Ensure Star On Awake or manual start is enabled
- Check for missing dependencies

**Steps not progressing:**
- Verify step completion conditions
- Check audio completion settings
- Ensure proper event subscriptions
- Test step completion logic

**Audio not playing:**
- Check Audio Clip assignments
- Verify audio source configuration
- Test pitch and volume settings
- Ensure audio files are properly imported

**Branching logic not working:**
- Verify transition conditions
- Check variable assignments
- Test condition evaluation
- Ensure proper step connections

### **Performance Issues**

**Sequence performance:**
- Limit the number of active steps
- Optimize audio playback
- Use appropriate step completion conditions
- Monitor analytics overhead

**Memory management:**
- Properly dispose of event subscriptions
- Clean up audio sources
- Manage step lifecycle correctly
- Avoid memory leaks in custom actions

## Code Integration

While this manual focuses on Unity Editor usage, here's how to work with sequences in code:

```csharp
// Get a reference to a sequence
Sequence sequence = GetComponent<Sequence>();

// Start the sequence
sequence.Begin();

// Check sequence status
if (sequence.Started)
{
    Debug.Log("Sequence is running!");
}

// Get current step
Step currentStep = sequence.CurrentStep;
if (currentStep != null)
{
    Debug.Log($"Current step: {currentStep.name}");
}

// Complete current step
sequence.CurrentStep.CompleteStep();
```

### **Branching Sequence Control**

```csharp
// Get a reference to a branching sequence
BranchingSequence branchingSequence = GetComponent<BranchingSequence>();

// Set control variables
branchingSequence.SetBool("IsCompleted", true);
branchingSequence.SetInt("StepCount", 5);
branchingSequence.SetFloat("Progress", 0.75f);

// Get variable values
bool isCompleted = branchingSequence.GetBool("IsCompleted");
int stepCount = branchingSequence.GetInt("StepCount");
float progress = branchingSequence.GetFloat("Progress");

// Activate a step
BranchingStep step = branchingSequence.Steps[0];
branchingSequence.ActivateStep(step);
```

### **Custom Sequence Actions**

```csharp
using UnityEngine;
using Shababeek.Sequencing;

public class CustomSequenceAction : AbstractSequenceAction
{
    [SerializeField] private string customMessage = "Custom action executed!";
    
    protected override void OnStepStatusChanged(SequenceStatus status)
    {
        if (status == SequenceStatus.Started)
        {
            Debug.Log(customMessage);
            // Custom logic when step starts
        }
        else if (status == SequenceStatus.Completed)
        {
            Debug.Log("Step completed!");
            // Custom logic when step completes
        }
    }
}
```

### **Event Subscription**

```csharp
// Subscribe to sequence events
sequence.OnRaisedData
    .Where(status => status == SequenceStatus.Started)
    .Subscribe(_ => Debug.Log("Sequence started!"))
    .AddTo(this);

sequence.OnRaisedData
    .Where(status => status == SequenceStatus.Completed)
    .Subscribe(_ => Debug.Log("Sequence completed!"))
    .AddTo(this);

// Subscribe to branching sequence events
branchingSequence.OnStepTransitioned
    .Subscribe(transition => {
        Debug.Log($"Transitioned from {transition.fromStep.Name} to {transition.toStep.Name}");
    })
    .AddTo(this);

branchingSequence.OnBranchTaken
    .Subscribe(branch => {
        Debug.Log($"Branch taken: {branch.reason}");
    })
    .AddTo(this);
``` 
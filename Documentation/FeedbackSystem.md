# Feedback System Manual

## Overview

The Feedback System provides unified feedback management for interactions, including haptic feedback, audio feedback, visual feedback, and animation feedback. It allows you to create rich, multi-sensory experiences that respond to user interactions.

## Common Requirements

All feedback components require these elements to function properly:
- **InteractableBase** - Required component for all feedback systems
## Core Components

### **FeedbackSystem**

**Menu Location**: `Component > Shababeek > Feedback > Feedback System`

**What it does**: Unified feedback management component that coordinates multiple feedback effects.

**How it works**: Manages a list of feedback effects and applies them based on interaction events. Automatically subscribes to interactable events and triggers appropriate feedback for each interaction type.

**Inspector Properties**:

- **Feedbacks** (List<FeedbackData>)
  - **What it does**: List of feedback effects to apply
  - **Default**: Empty list
  - **When to use**: Add feedback effects for different interaction types
  - **Example**: Add haptic, audio, and visual feedback for grab interactions

**Setup Example**:
1. Add the FeedbackSystem component to your interactable
2. Add feedback effects to the Feedbacks list via code or editor
3. Configure each feedback type's settings
4. Test feedback in VR

## Feedback Types

### **HapticFeedback**

**What it does**: Provides tactile feedback through VR controller vibration.

**How it works**: Sends haptic impulses to VR controllers to create tactile sensations that match the interaction. Automatically detects which hand is interacting and sends haptic feedback to the appropriate controller.

**Inspector Properties**:

- **Feedback Name** (string)
  - **What it does**: Name of this feedback component
  - **Default**: "Feedback"
  - **When to use**: Set a descriptive name for organization

- **Hover Amplitude** (float)
  - **What it does**: Strength of haptic feedback when hovering
  - **Range**: 0.0 - 1.0
  - **Default**: 0.3
  - **When to use**: Adjust based on desired tactile intensity for hover

- **Hover Duration** (float)
  - **What it does**: Duration of haptic feedback when hovering
  - **Range**: 0.01 - 2.0
  - **Default**: 0.1
  - **When to use**: Set based on hover interaction type

- **Select Amplitude** (float)
  - **What it does**: Strength of haptic feedback when selecting
  - **Range**: 0.0 - 1.0
  - **Default**: 0.5
  - **When to use**: Adjust for selection feedback intensity

- **Select Duration** (float)
  - **What it does**: Duration of haptic feedback when selecting
  - **Range**: 0.01 - 2.0
  - **Default**: 0.2
  - **When to use**: Set based on selection interaction type

- **Activate Amplitude** (float)
  - **What it does**: Strength of haptic feedback when activating
  - **Range**: 0.0 - 1.0
  - **Default**: 1.0
  - **When to use**: Adjust for activation feedback intensity

- **Activate Duration** (float)
  - **What it does**: Duration of haptic feedback when activating
  - **Range**: 0.01 - 2.0
  - **Default**: 0.3
  - **When to use**: Set based on activation interaction type

**Setup Example**:
1. Add HapticFeedback to the FeedbackSystem's feedbacks list
2. Configure amplitude and duration for each interaction type
3. Test haptic feedback in VR
4. Adjust values for optimal tactile feel

### **AudioFeedback**

**What it does**: Provides audio feedback through sound effects.

**How it works**: Plays audio clips based on interaction events, providing auditory feedback that enhances the user experience. Automatically manages AudioSource component and supports spatial audio.

**Inspector Properties**:

- **Feedback Name** (string)
  - **What it does**: Name of this feedback component
  - **Default**: "Feedback"
  - **When to use**: Set a descriptive name for organization

- **Audio Source** (AudioSource)
  - **What it does**: Audio source to play feedback sounds
  - **Default**: None (auto-created if not assigned)
  - **When to use**: Set to a specific AudioSource or let it auto-create

- **Hover Clip** (AudioClip)
  - **What it does**: Audio clip to play when hovering starts
  - **Default**: None
  - **When to use**: Add hover sound effects

- **Hover Exit Clip** (AudioClip)
  - **What it does**: Audio clip to play when hovering ends
  - **Default**: None
  - **When to use**: Add hover exit sound effects

- **Select Clip** (AudioClip)
  - **What it does**: Audio clip to play when object is selected
  - **Default**: None
  - **When to use**: Add selection sound effects

- **Deselect Clip** (AudioClip)
  - **What it does**: Audio clip to play when object is deselected
  - **Default**: None
  - **When to use**: Add deselection sound effects

- **Activate Clip** (AudioClip)
  - **What it does**: Audio clip to play when object is activated
  - **Default**: None
  - **When to use**: Add activation sound effects

- **Hover Volume** (float)
  - **What it does**: Volume for hover audio clips
  - **Range**: 0.0 - 1.0
  - **Default**: 0.5
  - **When to use**: Adjust hover sound volume

- **Select Volume** (float)
  - **What it does**: Volume for selection audio clips
  - **Range**: 0.0 - 1.0
  - **Default**: 0.7
  - **When to use**: Adjust selection sound volume

- **Activate Volume** (float)
  - **What it does**: Volume for activation audio clips
  - **Range**: 0.0 - 1.0
  - **Default**: 1.0
  - **When to use**: Adjust activation sound volume

- **Use Spatial Audio** (bool)
  - **What it does**: Whether to use spatial audio (3D positioning)
  - **Default**: true
  - **When to use**: Enable for 3D audio, disable for 2D audio

- **Randomize Pitch** (bool)
  - **What it does**: Whether to randomize pitch for audio clips
  - **Default**: false
  - **When to use**: Enable to add variety to repeated sounds

- **Pitch Randomization** (float)
  - **What it does**: Amount of pitch randomization
  - **Range**: 0.0 - 0.5
  - **Default**: 0.1
  - **When to use**: Set when Randomize Pitch is enabled

**Setup Example**:
1. Add AudioFeedback to the FeedbackSystem's feedbacks list
2. Assign audio clips for different interaction types
3. Configure volume and spatial audio settings
4. Test audio feedback in play mode

### **MaterialFeedback**

**What it does**: Provides visual feedback through material color changes.

**How it works**: Changes the color of renderers to provide visual feedback, such as highlighting objects when hovered or changing color when selected. Automatically detects renderers in children.

**Inspector Properties**:

- **Feedback Name** (string)
  - **What it does**: Name of this feedback component
  - **Default**: "Material Feedback"
  - **When to use**: Set a descriptive name for organization

- **Renderers** (Renderer[])
  - **What it does**: The renderers to apply material changes to
  - **Default**: None (auto-detected from children)
  - **When to use**: Set specific renderers or let it auto-detect

- **Color Property Name** (string)
  - **What it does**: Name of the color property in the material shader
  - **Default**: "_Color"
  - **When to use**: Set to match your material's color property

- **Hover Color** (Color)
  - **What it does**: Color to use when hovering
  - **Default**: Yellow
  - **When to use**: Set hover highlight color

- **Select Color** (Color)
  - **What it does**: Color to use when selected
  - **Default**: Green
  - **When to use**: Set selection highlight color

- **Activate Color** (Color)
  - **What it does**: Color to use when activated
  - **Default**: Red
  - **When to use**: Set activation highlight color

- **Color Multiplier** (float)
  - **What it does**: Multiplier for hover color (applied to original color)
  - **Range**: 0.0 - 2.0
  - **Default**: 0.3
  - **When to use**: Adjust hover color intensity

**Setup Example**:
1. Add MaterialFeedback to the FeedbackSystem's feedbacks list
2. Configure renderers and color property name
3. Set colors for different interaction states
4. Test visual feedback in play mode

### **AnimationFeedback**

**What it does**: Provides feedback through animator parameter changes.

**How it works**: Triggers animator parameters to provide visual feedback, such as object movement, scaling, or other animated effects. Automatically detects Animator component.

**Inspector Properties**:

- **Feedback Name** (string)
  - **What it does**: Name of this feedback component
  - **Default**: "Feedback"
  - **When to use**: Set a descriptive name for organization

- **Animator** (Animator)
  - **What it does**: The animator to control animations
  - **Default**: None (auto-detected)
  - **When to use**: Set to specific Animator or let it auto-detect

- **Hover Bool Name** (string)
  - **What it does**: Name of the boolean parameter for hover state
  - **Default**: "Hovered"
  - **When to use**: Set to match your animator's hover parameter

- **Select Trigger Name** (string)
  - **What it does**: Name of the trigger parameter for selection
  - **Default**: "Selected"
  - **When to use**: Set to match your animator's select trigger

- **Deselect Trigger Name** (string)
  - **What it does**: Name of the trigger parameter for deselection
  - **Default**: "Deselected"
  - **When to use**: Set to match your animator's deselect trigger

- **Activated Trigger Name** (string)
  - **What it does**: Name of the trigger parameter for activation
  - **Default**: "Activated"
  - **When to use**: Set to match your animator's activate trigger

**Setup Example**:
1. Add an Animator component to your object
2. Create animations and parameters for different states
3. Add AnimationFeedback to the FeedbackSystem's feedbacks list
4. Set parameter names to match your animator
5. Test animations in play mode

## Advanced Configuration

### **Feedback Combinations**

Create rich feedback experiences by combining multiple feedback types:

1. **Grab Interaction**:
   - HapticFeedback for tactile response
   - AudioFeedback for grab sound
   - MaterialFeedback for visual highlight
   - AnimationFeedback for object movement

2. **Button Press**:
   - HapticFeedback for button feel
   - AudioFeedback for click sound
   - MaterialFeedback for press visual
   - AnimationFeedback for press animation

3. **Hover Effect**:
   - AudioFeedback for hover sound
   - MaterialFeedback for highlight
   - AnimationFeedback for hover animation

### **Feedback Timing**

Configure feedback timing for optimal user experience:

1. **Immediate Feedback**:
   - Use for selection and activation
   - Keep feedback duration short
   - Ensure responsive feel

2. **Sustained Feedback**:
   - Use for ongoing interactions
   - Consider user comfort
   - Avoid overwhelming feedback

3. **Delayed Feedback**:
   - Use for complex interactions
   - Provide clear feedback cues
   - Maintain user engagement

### **Performance Optimization**

Optimize feedback for performance:

1. **Audio Optimization**:
   - Use compressed audio formats
   - Limit concurrent audio sources
   - Implement audio pooling for repeated sounds

2. **Material Optimization**:
   - Use efficient shaders
   - Limit material changes per frame
   - Cache material references

3. **Animation Optimization**:
   - Use efficient animation controllers
   - Limit animator parameter changes
   - Optimize animation assets

## Troubleshooting

### **Common Issues**

**Haptic feedback not working:**
- Check VR controller setup
- Verify haptic amplitude settings
- Test with different VR devices
- Ensure haptic support is enabled

**Audio feedback not playing:**
- Check AudioSource assignment
- Verify audio clip assignments
- Test audio source settings
- Check volume and pitch settings

**Material feedback not visible:**
- Verify renderer assignment
- Check material property names
- Test material compatibility
- Ensure proper lighting setup

**Animation feedback not triggering:**
- Check Animator assignment
- Verify animation parameter names
- Test animation controller setup
- Ensure animations are properly configured

### **VR-Specific Issues**

**Haptic feedback problems:**
- Test with different VR controllers
- Check haptic API compatibility
- Verify controller input mapping
- Test haptic intensity settings

**Audio feedback in VR:**
- Check spatial audio settings
- Verify audio source positioning
- Test with VR audio plugins
- Ensure proper audio routing

### **Performance Issues**

**Feedback causing lag:**
- Reduce concurrent feedback effects
- Optimize audio file sizes
- Use efficient material shaders
- Limit animation complexity

**Memory usage issues:**
- Cache material references
- Limit audio clip memory usage
- Optimize animation assets
- Monitor feedback system overhead

## Code Integration

While this manual focuses on Unity Editor usage, here's how to work with feedback in code:

```csharp
// Get feedback system
FeedbackSystem feedbackSystem = GetComponent<FeedbackSystem>();

// Add feedback components
feedbackSystem.AddFeedback(new HapticFeedback());
feedbackSystem.AddFeedback(new AudioFeedback());
feedbackSystem.AddFeedback(new MaterialFeedback());
feedbackSystem.AddFeedback(new AnimationFeedback());

// Remove feedback
var feedbacks = feedbackSystem.GetFeedbacks();
if (feedbacks.Count > 0)
{
    feedbackSystem.RemoveFeedback(feedbacks[0]);
}

// Clear all feedbacks
feedbackSystem.ClearFeedbacks();
```

For individual feedback types:

```csharp
// Haptic feedback
HapticFeedback haptic = new HapticFeedback();
haptic.hoverAmplitude = 0.8f;
haptic.hoverDuration = 0.2f;
feedbackSystem.AddFeedback(haptic);

// Audio feedback
AudioFeedback audio = new AudioFeedback();
audio.hoverVolume = 0.7f;
audio.selectClip = someAudioClip;
feedbackSystem.AddFeedback(audio);

// Material feedback
MaterialFeedback material = new MaterialFeedback();
material.hoverColor = Color.yellow;
material.selectColor = Color.green;
feedbackSystem.AddFeedback(material);

// Animation feedback
AnimationFeedback animation = new AnimationFeedback();
animation.hoverBoolName = "IsHovered";
animation.selectTriggerName = "Select";
feedbackSystem.AddFeedback(animation);
```

### **Custom Feedback Types**

```csharp
using UnityEngine;
using Shababeek.Interactions.Feedback;

public class CustomFeedback : FeedbackData
{
    [SerializeField] private string customParameter = "Custom";
    
    public override void OnHoverStarted(InteractorBase interactor)
    {
        Debug.Log($"Custom feedback: Hover started by {interactor.name}");
        // Custom hover logic
    }
    
    public override void OnSelected(InteractorBase interactor)
    {
        Debug.Log($"Custom feedback: Selected by {interactor.name}");
        // Custom selection logic
    }
    
    public override void OnActivated(InteractorBase interactor)
    {
        Debug.Log($"Custom feedback: Activated by {interactor.name}");
        // Custom activation logic
    }
    
    public override bool IsValid()
    {
        return true; // Add validation logic as needed
    }
}
``` 
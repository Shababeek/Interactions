# Feedback System — Haptic, Audio, Visual, and Animation Feedback

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Feedback > Feedback System
> **Use For:** Adding multi-sensory feedback to interactions
> **Requires:** InteractableBase component (any interactable)

---

## What It Does

The **Feedback System** provides unified feedback management for interactions. It responds to hover, select, and activate events by triggering various feedback types: haptic vibration, audio sounds, visual material changes, and animations.

**Key Benefits:**
- ✅ One component for all feedback types
- ✅ Automatically responds to interaction events
- ✅ Mix and match feedback types per object
- ✅ No scripting required for common setups

---

## Feedback Types

| Type | What It Does | Best For |
|------|--------------|----------|
| **Material** | Changes object colors/materials | Visual highlight on hover/select |
| **Animation** | Triggers animator parameters | Character reactions, UI animations |
| **Haptic** | Controller vibration | Physical confirmation of interactions |
| **Audio** | Plays sound effects | Click sounds, interaction audio |

---

## Quick Example

> **Goal:** Object highlights when hovered and vibrates when grabbed

[PLACEHOLDER_GIF: Object with visual highlight and controller vibrating]

1. Add Feedback System to any interactable
2. Add Material Feedback → Configure hover color
3. Add Haptic Feedback → Configure vibration on select

---

## Inspector Reference

[PLACEHOLDER_SCREENSHOT: FeedbackSystem Inspector with expanded feedbacks]

### Feedback Configuration

#### Feedbacks List
A list of all feedback components attached to this system. Each feedback type is added as an entry in this list.

**To add feedback:**
1. Click **Add Feedback** button (or configure via code)
2. Configure the new feedback type's settings
3. Repeat for additional feedback types

Each feedback entry shows:
- **Feedback Name** — Display label
- **Enabled** — Toggle to temporarily disable
- **Type-specific settings** — Varies by feedback type

---

## Material Feedback

Changes material colors in response to interaction events.

[PLACEHOLDER_SCREENSHOT: MaterialFeedback settings expanded]

### Settings

#### Renderers
Array of Renderer components whose materials will be modified.

**Auto-detection:** If left empty, automatically finds all renderers in children.

#### Color Property Name
The shader property to modify.

| Value | Use For |
|-------|---------|
| **_Color** | Standard shader, most materials |
| **_BaseColor** | URP/HDRP Lit shader |
| **_EmissionColor** | Emission effects |

**Default:** "_Color"

#### Hover Color
Color applied when hand hovers over object.

**Default:** Yellow

#### Select Color
Color applied when object is grabbed/selected.

**Default:** Green

#### Activate Color
Color applied when object is used (trigger pressed while held).

**Default:** Red

#### Color Multiplier
How strongly the hover effect is applied (multiplied with original color).

**Default:** 0.3

Lower values = subtle effect; Higher values = stronger effect.

### How It Works
- **Hover Start** → Multiplies original color by multiplier
- **Hover End** → Restores original color
- **Select** → Sets select color
- **Deselect** → Restores original color
- **Activate** → Sets activate color

---

## Animation Feedback

Triggers Animator parameters based on interaction events.

[PLACEHOLDER_SCREENSHOT: AnimationFeedback settings expanded]

### Settings

#### Animator
The Animator component to control.

**Auto-detection:** If not assigned, looks for Animator on same object.

#### Hover Bool Name
Bool parameter set true during hover.

**Default:** "Hovered"

**Animator setup:** Create a bool parameter with this name.

#### Select Trigger Name
Trigger parameter fired when selected.

**Default:** "Selected"

#### Deselect Trigger Name
Trigger parameter fired when deselected.

**Default:** "Deselected"

#### Activated Trigger Name
Trigger parameter fired when activated.

**Default:** "Activated"

### How It Works
- **Hover Start** → Sets bool to true
- **Hover End** → Sets bool to false
- **Select** → Fires select trigger
- **Deselect** → Fires deselect trigger
- **Activate** → Fires activate trigger

### Animator Controller Setup
Your Animator Controller needs parameters matching the names above:

```
Parameters:
- Hovered (Bool)
- Selected (Trigger)
- Deselected (Trigger)
- Activated (Trigger)

States:
- Idle
- Hovered (transition when Hovered = true)
- Selected (transition on Selected trigger)
```

[PLACEHOLDER_SCREENSHOT: Example Animator Controller with interaction states]

---

## Haptic Feedback

Sends vibration impulses to VR controllers.

[PLACEHOLDER_SCREENSHOT: HapticFeedback settings expanded]

### Settings

#### Hover Amplitude / Duration
Vibration strength (0-1) and length (seconds) for hover events.

**Defaults:** 0.3 amplitude, 0.1 seconds

Subtle feedback when hand approaches object.

#### Select Amplitude / Duration
Vibration for grab/select events.

**Defaults:** 0.5 amplitude, 0.2 seconds

Medium feedback confirming grab.

#### Activate Amplitude / Duration
Vibration for use/activate events.

**Defaults:** 1.0 amplitude, 0.3 seconds

Strong feedback for triggering actions.

### Amplitude Guide

| Value | Feel |
|-------|------|
| **0.1-0.3** | Subtle, light tap |
| **0.4-0.6** | Medium, noticeable |
| **0.7-0.9** | Strong, attention-getting |
| **1.0** | Maximum vibration |

### Duration Guide

| Value | Feel |
|-------|------|
| **0.05-0.1s** | Quick tick |
| **0.1-0.2s** | Normal pulse |
| **0.3-0.5s** | Extended vibration |

---

## Audio Feedback

Plays sound effects for interaction events.

[PLACEHOLDER_SCREENSHOT: AudioFeedback settings expanded]

### Settings

#### Audio Source
The AudioSource component used for playback.

**Auto-creation:** If not assigned, creates one automatically.

#### Sound Clips

| Clip | When Played |
|------|-------------|
| **Hover Clip** | Hand enters hover range |
| **Hover Exit Clip** | Hand leaves hover range |
| **Select Clip** | Object grabbed |
| **Deselect Clip** | Object released |
| **Activate Clip** | Trigger pressed while holding |

#### Volume Settings

| Setting | Range | Default |
|---------|-------|---------|
| **Hover Volume** | 0-1 | 0.5 |
| **Select Volume** | 0-1 | 0.7 |
| **Activate Volume** | 0-1 | 1.0 |

#### Use Spatial Audio
When enabled, sounds are 3D positioned (louder when closer).

**Default:** Enabled

#### Randomize Pitch
When enabled, slightly varies pitch each time for variety.

**Default:** Disabled

#### Pitch Randomization
Amount of pitch variation (± this value).

**Default:** 0.1

---

## Adding Feedback to Your Objects

### Step 1: Ensure Object Has Interactable

Feedback System requires an interactable component:
- Grabable
- Any ConstrainedInteractable (Lever, Drawer, etc.)
- Custom InteractableBase implementation

### Step 2: Add Feedback System

1. Select your interactable object
2. **Add Component > Feedback System**
3. The component auto-detects the interactable

### Step 3: Configure Feedbacks

Add desired feedback types via inspector or code:

**Via Inspector:**
Click "Add Feedback" and select type, then configure settings.

**Via Code:**
```csharp
var feedbackSystem = GetComponent<FeedbackSystem>();

// Add material feedback
var materialFeedback = new MaterialFeedback();
materialFeedback.hoverColor = Color.yellow;
feedbackSystem.AddFeedback(materialFeedback);

// Add haptic feedback
var hapticFeedback = new HapticFeedback();
hapticFeedback.selectAmplitude = 0.7f;
feedbackSystem.AddFeedback(hapticFeedback);
```

---

## Common Workflows

### How To: Highlight on Hover

> **Goal:** Object glows when hand approaches
> **Time:** ~2 minutes

1. Add **Feedback System** to object
2. Add **Material Feedback**
3. Set **Hover Color** to desired highlight
4. Adjust **Color Multiplier** (0.5 for subtle, 1.0 for bright)

---

### How To: Full Interactive Feedback

> **Goal:** Complete feedback (visual, audio, haptic)
> **Time:** ~5 minutes

1. Add **Feedback System**
2. Add **Material Feedback**:
   - Hover: Yellow
   - Select: Green
   - Activate: Red

3. Add **Audio Feedback**:
   - Assign hover/select/activate clips
   - Adjust volumes

4. Add **Haptic Feedback**:
   - Hover: 0.2, 0.1s
   - Select: 0.5, 0.2s
   - Activate: 0.8, 0.15s

---

### How To: Button Click Feedback

> **Goal:** Satisfying button press feel
> **Time:** ~3 minutes

1. Add **Feedback System** to button
2. Add **Audio Feedback**:
   - Select Clip: "button_click.wav"
   - Select Volume: 0.8

3. Add **Haptic Feedback**:
   - Select: 0.7, 0.1s (strong, quick)

4. Add **Material Feedback** (optional):
   - Select Color: Brighter version of button color

---

### How To: Ambient Hover Sound

> **Goal:** Object hums when hand is near
> **Time:** ~3 minutes

1. Add **Audio Feedback**
2. Set **Hover Clip** to a looping hum sound
3. Add **AudioSource** manually, set **Loop = true**
4. In Audio Feedback, assign this AudioSource

The sound will start on hover and stop on hover exit.

---

## Tips & Best Practices

💡 **Less is more with haptics**
Constant strong vibration causes fatigue. Use subtle hover feedback and stronger select/activate.

💡 **Use consistent colors**
Establish a color language: Yellow = hover, Green = grabbed, Red = activated.

💡 **Layer feedback types**
Combining haptic + audio + visual creates rich, satisfying interactions.

💡 **Test without visuals**
Can players understand interactions through haptic alone? Good feedback should work multi-sensory.

💡 **Vary by importance**
Important objects can have stronger feedback; background props can be subtle.

⚠️ **Common Mistake:** No interactable
Feedback System requires an InteractableBase. Add Grabable or similar first.

⚠️ **Common Mistake:** Audio clips not assigned
Feedback will silently fail if clips are null. Assign or check your audio clips.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| No feedback at all | Interactable missing | Add Grabable or similar |
| Material doesn't change | Renderers array empty | Assign renderers or let it auto-detect |
| Wrong color property | Shader uses different name | Check shader for correct property name |
| No vibration | Haptic not added | Add HapticFeedback to the list |
| Sound doesn't play | AudioClip not assigned | Assign sound clips to Audio Feedback |
| Animation doesn't trigger | Parameter names wrong | Match animator parameter names exactly |

---

## Scripting API

### Adding/Removing Feedback

```csharp
FeedbackSystem system = GetComponent<FeedbackSystem>();

// Add feedback
var feedback = new MaterialFeedback();
system.AddFeedback(feedback);

// Remove feedback
system.RemoveFeedback(feedback);

// Clear all
system.ClearFeedbacks();

// Get all feedbacks
List<FeedbackData> feedbacks = system.GetFeedbacks();
```

### Creating Custom Feedback

Extend `FeedbackData` to create custom feedback types:

```csharp
[Serializable]
public class ParticleFeedback : FeedbackData
{
    [SerializeField] public ParticleSystem particles;

    public ParticleFeedback()
    {
        feedbackName = "Particle Feedback";
    }

    public override void OnHoverStarted(InteractorBase interactor)
    {
        if (!enabled || particles == null) return;
        particles.Play();
    }

    public override void OnHoverEnded(InteractorBase interactor)
    {
        if (!enabled || particles == null) return;
        particles.Stop();
    }

    public override bool IsValid()
    {
        return base.IsValid() && particles != null;
    }
}
```

### Event Timing

Feedback events fire in this order:

1. **OnHoverStarted** — Hand enters interaction range
2. **OnSelected** — Object grabbed
3. **OnActivated** — Use button pressed (while holding)
4. **OnDeselected** — Object released
5. **OnHoverEnded** — Hand leaves interaction range

---

## Related Documentation

- [Grabable](../Interactables/Grabable.md) — Primary interactable for feedback
- [Constrained Interactables](../Interactables/ConstrainedInteractables.md) — Lever, Drawer feedback
- [PoseConstrainer](../PoseSystem/PoseConstrainer.md) — Hand positioning
- [Quick Start Guide](../GettingStarted/QuickStart.md) — Basic setup

---

**Last Updated:** January 2026
**Component Version:** 1.0.0

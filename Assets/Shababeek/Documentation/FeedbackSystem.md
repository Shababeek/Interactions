# FeedbackSystem — Manual

## Overview
The FeedbackSystem component manages haptic, audio, and visual feedback for interactable objects. Attach it to any interactable to provide user feedback during interactions.

[screenshot of FeedbackSystem component in the Inspector]

## How to Add & Configure
- Add the FeedbackSystem component to a GameObject with an Interactable (e.g., Grabable).
- In the Inspector, add feedback entries (haptic, audio, visual, etc.).
- Configure each feedback type as needed.

[screenshot of adding feedbacks to the FeedbackSystem]

## Inspector Properties
- **feedbacks** (List<FeedbackData>): List of feedbacks to trigger on interaction events. Add via the Inspector.

## Types of Feedback Supported
- **Haptic Feedback:** Trigger controller vibration or haptic pulses.
- **Audio Feedback:** Play sounds on interaction events.
- **Visual Feedback:** Trigger visual effects (e.g., highlight, particle systems).

[screenshot of configuring haptic, audio, and visual feedbacks]

## Usage Tips
- Combine multiple feedback types for richer user experience.
- Use FeedbackSystem on all major interactables for consistent feedback.
- Test feedback on target hardware for best results. 
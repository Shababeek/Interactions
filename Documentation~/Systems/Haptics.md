# Haptic Patterns — Reusable Controller Vibration

> **Quick Reference**
> **Create:** Project window > Create > Shababeek > Interactions > Haptic Pattern
> **Use For:** Giving clicks, detents and impacts a consistent feel across a project

---

## What It Does

A **Haptic Pattern** is an asset that describes a vibration: a strength curve over a duration. Instead of typing an amplitude and a duration on every component, you tune a feel once ("click", "detent", "thud") and assign the same asset wherever it is needed. Change the asset and every control that uses it changes with it.

---

## Creating a Pattern

1. Project window > **Create > Shababeek > Interactions > Haptic Pattern**.
2. Select the asset. Use a preset button (**Click**, **Detent**, **Thud**, **Buzz**) as a starting point, or edit the values.
3. With a headset connected, press **Test Left** or **Test Right** to feel it.

Four ready-made patterns ship in `Data/HapticPatterns/`: `Haptic_Click`, `Haptic_Detent`, `Haptic_Thud`, `Haptic_Buzz`.

### Inspector Reference

| Field | What it does |
|---|---|
| **Amplitude** | Curve of vibration strength over the pattern. Both axes run 0 to 1. |
| **Duration** | Total length in seconds. |
| **Strength** | Overall multiplier for the curve (0 to 1). Use it to make a softer copy of a pattern without redrawing the curve. |

---

## Where Patterns Are Used

| Component | Slots |
|---|---|
| [Feedback System](FeedbackSystem.md#haptic-feedback) — Haptic Feedback | Hover, Select, Activate |
| [Dial](../Interactables/ConstrainedInteractables.md#dial-interactable), [Slider](../Interactables/ConstrainedInteractables.md#slider-interactable), [Toggle Switch](../Interactables/ToggleSwitch.md) | Step haptic |
| [VRButton](../Interactables/VRButton.md), [VR Toggle Button](../Interactables/AdditionalInteractables.md#vr-toggle-button) | Press / toggle |
| [Shifting Rack Socket](../SocketSystem/AdvancedSockets.md#shifting-rack-socket) | Slot change, insert, reject |
| [Elastic Tether](../Interactables/AdditionalInteractables.md#elastic-tether) | Slip |

Where a component still shows separate **Amplitude** and **Duration** fields, an assigned pattern takes priority over them.

---

## Scripting API

```csharp
// Play on the hand that is interacting
interactor.PlayHapticPattern(pattern);

// A single impulse without an asset
interactor.SendHapticImpulse(0.5f, 0.1f);
```

A Haptic Pattern is also a ReactiveVars scriptable variable, so it can be observed or swapped at runtime like any other variable.

> The older **Haptic Driver** component is deprecated. Use the Feedback System's Haptic Feedback instead.

---

## Related Documentation

- [Feedback System](FeedbackSystem.md) — Haptic, audio and visual feedback on interaction events

---

**Last Updated:** November 2026

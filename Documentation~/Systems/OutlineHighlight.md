# Outline Highlight — Hover, Selection and Grab Hints

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Interactable Outline Feedback
> **Config:** Project window > Create > Shababeek > Interactions > Outline Feedback Config
> **Use For:** Showing players what they can grab and what they are holding

---

## What It Does

**Interactable Outline Feedback** draws an outline around an interactable:

- when a hand **hovers** it,
- in a different colour or width while it is **selected** (held),
- and optionally as a slow **pulsing hint** while idle, to advertise that the object can be grabbed.

The look is not set per object. It comes from one shared **Outline Feedback Config** asset, so the whole project stays consistent and can be restyled in one place.

---

## Setup

1. Select an object that has an interactable (Grabable, Lever, and so on).
2. Add **Interactable Outline Feedback**. An outline renderer is found on the object or created for you.
3. Press Play and hover the object.

The project-wide config is the asset at `Resources/OutlineFeedbackConfig`. The package ships one, plus a lighter `OutlineFeedbackConfig_Quest` variant.

---

## Outline Feedback Config — Inspector Reference

| Section | Fields | What they do |
|---|---|---|
| **General** | Feedback Enabled | Switches all outline feedback off for every component using this config. |
| **Hover** | Enabled, Color, Width, Mode | The outline shown while a hand is near. |
| **Selected** | Enabled, Color, Width, Mode | The outline shown while held. Untick Enabled to hide the outline once grabbed. |
| **Grab Hint (Idle)** | Enabled, Color, Mode, Hint Min Width, Hint Max Width, Hint Pulse Speed | The pulsing idle outline. Enabled here is the project default. |
| **Per-Render-Pipeline Overrides** | Label, Pipeline Name Contains, Pipeline Assets, Config | "While this render pipeline is active, use that config instead." Read only on the root asset. The first matching rule wins. |

**Mode** chooses what is drawn: the outline only where the object is visible, only where it is hidden, both, or a silhouette.

### Different looks per platform

To use a cheaper outline on Quest, add a rule under **Per-Render-Pipeline Overrides** on the root config: set **Pipeline Name Contains** to part of your mobile Render Pipeline Asset's name (for example `Quest`) and assign the alternative config. Matching by name keeps the rule portable between projects; assigning the asset directly is exact but ties the config to that asset.

---

## Controlling Hints From Script

```csharp
// Turn the idle grab hint on or off for one object
GetComponent<InteractableOutlineFeedback>().SetGrabHint(true);
```

Useful in tutorials: enable the hint on the object the player should pick up next.

---

## Demo Outline Feedback

**Demo Outline Feedback (Config-Free)** does the same job with its colours and widths stored on the component instead of in the shared config. It is meant for self-contained demo scenes. For a real project prefer the config-driven component.

---

## Other Highlight Options

- [Feedback System](FeedbackSystem.md) — **Material Feedback** tints the object's material on hover and select, and can be combined with scale, audio and haptics.

---

## Credits

The outline renderer is a modified copy of QuickOutline by Chris Nolet (MIT). See `ThirdPartyNotices.md` in the asset's root folder.

---

**Last Updated:** November 2026

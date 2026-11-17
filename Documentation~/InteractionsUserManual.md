# Interactions Overview

How interactables and interactors work together, the settings and events every interactable shares, and how to write your own. For a specific component, go to its manual from the [documentation index](README.md).

## Table of Contents

1. [The Model](#the-model)
2. [Shared Interactable Settings](#shared-interactable-settings)
3. [Shared Events](#shared-events)
4. [Interactable Families](#interactable-families)
5. [Interactors](#interactors)
6. [What Happens During a Grab](#what-happens-during-a-grab)
7. [Writing Your Own Interactable](#writing-your-own-interactable)
8. [Troubleshooting](#troubleshooting)

---

## The Model

- An **interactable** is a component on an object the player can act on (Grabable, Lever, Dial, and so on).
- An **interactor** is a component on a hand that finds interactables and tells them what the hand is doing.

An interactable is always in one of three states:

| State | Meaning |
|---|---|
| **None** | No hand is near |
| **Hovering** | A hand is close enough to select it |
| **Selected** | A hand is holding or operating it |

While selected, the other button on the controller **uses** the object (fires a gun, toggles a flashlight). Using is an action on a selected object, not a separate state.

Buttons and finger-pressed switches ([VRButton](Interactables/VRButton.md), [VR Toggle Button](Interactables/AdditionalInteractables.md#vr-toggle-button), [Switch](Interactables/Switch.md)) are different: they react to a finger physically touching them and do not go through hover and select.

---

## Shared Interactable Settings

Every interactable that derives from the common base shows these in the Inspector.

| Field | What it does |
|---|---|
| **Interaction Hand** | Which hands may interact: Left, Right or both. |
| **Selection Button** | The button that selects the object: Grip, Trigger or Any. The other button becomes the use button. |
| **Layer Behavior** | **Match Hand**: while held, the object moves to the holding hand's layer so it does not collide with that hand. **Keep Original**: the layer is left alone. |

Read-only debug fields show **Is Selected**, **Current Interactor**, **Current State** and **Is Using** in Play mode.

---

## Shared Events

Each event passes the interactor (the hand) involved. All are available as UnityEvents in the Inspector and as observables in code.

| Event | Fires when |
|---|---|
| **On Hover Start / On Hover End** | A hand comes within reach / leaves |
| **On Selected / On Deselected** | The object is grabbed / released |
| **On Use Started / On Use Ended** | The use button is pressed / released while held |
| **On Thumb Pressed / Released** | The thumb button is pressed / released while held |
| **On A Pressed / Released**, **On B Pressed / Released** | The A or B face button is pressed / released while held |

```csharp
var grabable = GetComponent<Grabable>();
grabable.OnSelected.Subscribe(interactor => Debug.Log($"Grabbed by {interactor.HandIdentifier}"));
grabable.OnUseStarted.Subscribe(_ => Fire());
```

---

## Interactable Families

| Family | Components | Manual |
|---|---|---|
| Free grab | Grabable, Two Handed Grabable | [Grabable](Interactables/Grabable.md), [Two Handed Grabable](Interactables/TwoHandedGrabable.md) |
| Rotary, continuous | Lever, Wheel, Joystick, Door, Door Handle | [Constrained Interactables](Interactables/ConstrainedInteractables.md), [Additional Interactables](Interactables/AdditionalInteractables.md#door-and-door-handle) |
| Rotary, stepped | Dial, Toggle Switch | [Dial](Interactables/ConstrainedInteractables.md#dial-interactable), [Toggle Switch](Interactables/ToggleSwitch.md) |
| Linear | Drawer, Slider | [Constrained Interactables](Interactables/ConstrainedInteractables.md) |
| Special | Elastic Tether, Spawning Interactable | [Additional Interactables](Interactables/AdditionalInteractables.md) |
| Finger-pressed | VRButton, VR Toggle Button, Switch | [VRButton](Interactables/VRButton.md), [Switch](Interactables/Switch.md) |

Constrained interactables keep their moving part under a generated **interactableObject** child and show a posed hand on the control while it is held. Wrap only the moving part (the lever arm, the drawer box), not the whole assembly.

---

## Interactors

| Component | Finds objects by | Manual |
|---|---|---|
| **Trigger Interactor** | Proximity to the hand | [Interactors](Interactors/Interactors.md#triggerinteractor) |
| **Raycast Interactor** | A ray from the hand | [Interactors](Interactors/Interactors.md#raycastinteractor) |
| **Distance Grabber** | Aim and flick, in addition to one of the above | [Distance Grab](Interactables/DistanceGrab.md) |

Hand input comes from an input provider (controller, hand tracking or a recording). See [Input providers and locomotion](Core/InputAndLocomotion.md).

---

## What Happens During a Grab

When a [Grabable](Interactables/Grabable.md) is selected:

1. The hand takes the pose set on the object's [PoseConstrainer](PoseSystem/PoseConstrainer.md).
2. If the object has a Rigidbody, it is made kinematic while held.
3. The object moves smoothly to the hand and is attached to it.
4. If **Can Be Thrown** is on, the object's velocity is tracked.

When it is released: the pose is removed, the object is detached, and with **Can Be Thrown** on it becomes a normal physics body and is thrown with the tracked velocity. With **Can Be Thrown** off, the Rigidbody returns to the kinematic setting it had before the grab.

A [Socketable](SocketSystem/SocketSystem.md) object released near a matching socket snaps into it instead.

---

## Writing Your Own Interactable

Derive from `InteractableBase` for a new kind of object, or from `ConstrainedInteractableBase` for a control that moves along a path and shows a posed hand.

```csharp
using Shababeek.Interactions;

public class Pullable : InteractableBase
{
    // Return true to refuse the selection.
    protected override bool Select()
    {
        return false;
    }

    protected override void DeSelected() { }

    // Optional hooks
    protected override void StartHover() { }
    protected override void EndHover() { }
    protected override void UseStarted() { }
    protected override void UseEnded() { }
}
```

For a constrained control, implement `HandleObjectMovement(Vector3 target)` (called while held, with the hand position), `HandleObjectDeselection()` and `HandleReturnToOriginalPosition()`. The existing bases do most of the work for common shapes: `RotaryLeverBase` (single-axis lever), `RotaryInteractableBase` (wheel-like rotation) and `LinearInteractableBase` (start-to-end slide). See the [architecture map](Analysis/ArchitectureMap.md).

Often you do not need a new interactable. A small component beside an existing one that listens to its events is simpler; [Gun Firing and Blade Hit](Interactables/Weapons.md) are built that way.

---

## Troubleshooting

| Problem | Check |
|---|---|
| Object cannot be grabbed | It needs a Collider. **Interaction Hand** must include the hand. The hand and object layers must be allowed to overlap (see [Config](Core/config.md)). |
| Wrong button grabs | **Selection Button** on the interactable. |
| Events do not fire | Use and button events only fire while the object is selected. |
| Hand passes through or pushes the held object | **Layer Behavior** should be Match Hand. |
| A constrained control's visuals do not move | The visuals must be under the **interactableObject** child. |

---

**Last Updated:** November 2026

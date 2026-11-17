# Documentation Status

What is documented, where, and what is still missing. Updated November 2026 against the current source.

Back to the [documentation index](README.md).

---

## Coverage by component

| Area | Components | Manual | State |
|---|---|---|---|
| Grab | Grabable, Throwable settings | [Grabable](Interactables/Grabable.md) | Complete |
| Grab | TwoHandedGrabable | [Two Handed Grabable](Interactables/TwoHandedGrabable.md) | New, no images |
| Grab | DistanceGrabber, DistanceGrabbable | [Distance Grab](Interactables/DistanceGrab.md) | New, no images |
| Weapons | GunFiring, Bullet, BladeHit, Target, PistolInteractable | [Weapons](Interactables/Weapons.md) | New, no images |
| Touch | VRButton | [VRButton](Interactables/VRButton.md) | Complete |
| Touch | VRToggleButton | [Additional Interactables](Interactables/AdditionalInteractables.md#vr-toggle-button) | New, reference only |
| Touch | Switch | [Switch](Interactables/Switch.md) | Complete |
| Constrained | Lever, Drawer, Joystick, Wheel, Dial | [Constrained Interactables](Interactables/ConstrainedInteractables.md) | Complete |
| Constrained | Slider | [Constrained Interactables](Interactables/ConstrainedInteractables.md#slider-interactable) | Short; needs a full Inspector reference |
| Constrained | ToggleSwitchInteractable | [Toggle Switch](Interactables/ToggleSwitch.md) | Complete |
| Constrained | DoorInteractable, DoorHandleInteractable, ElasticTetherInteractable | [Additional Interactables](Interactables/AdditionalInteractables.md) | New, reference only |
| Helpers | SpawningInteractable, ReturnToOrigin, StepValueDisplay, StepLabelHighlighter | [Additional Interactables](Interactables/AdditionalInteractables.md) | New, reference only |
| Hands | Hand, TriggerInteractor, RaycastInteractor | [Interactors](Interactors/Interactors.md) | Complete |
| Hands | Config | [Config asset](Core/config.md) | Input sections rewritten November 2026 |
| Hands | CameraRig, PhysicsHandFollower | [Interaction Core](Core/InteractionCoreUserManual.md) | Needs review: still presents the legacy input manager |
| Hands | Input providers, CameraRigLocomotion | [Input providers and locomotion](Core/InputAndLocomotion.md) | New |
| Poses | PoseConstrainer | [PoseConstrainer](PoseSystem/PoseConstrainer.md) | Complete; Auto-Fit Fingers button and grab points not yet covered |
| Poses | HandData, HandPoseController | [Animations and poses](PoseSystem/AnimationsUserManual.md) | Older page, needs review |
| Feedback | FeedbackSystem | [Feedback System](Systems/FeedbackSystem.md) | Complete; haptic section updated for patterns |
| Feedback | HapticPattern | [Haptics](Systems/Haptics.md) | New |
| Feedback | InteractableOutlineFeedback, OutlineFeedbackConfig, DemoOutlineFeedback | [Outline Highlight](Systems/OutlineHighlight.md) | New, no images |
| Sockets | Socket, Socketable | [Socket System](SocketSystem/SocketSystem.md) | Complete |
| Sockets | SocketMask, MultiSocket, GridMultiSocket, TransformArrayMultiSocket, ShiftingRackSocket, InventoryGridSocket and helpers, Fastener, FastenerTool | [Advanced Sockets](SocketSystem/AdvancedSockets.md) | New, reference only |
| Tools | InteractionRecorder, InteractionRecordingPlayer | [Interaction Recording](Systems/InteractionRecording.md) | New |
| Data flow | Variable writers, event and socket drivers | [Interaction Drivers](ScriptableSystem/Drivers.md) | Complete; Slider, Toggle Switch and Switch writers added |
| Data flow | Sequence actions | [Interaction Sequences](Systems/SequencingSystem.md) | Complete |

---

## Not documented yet

| Component | Note |
|---|---|
| HandFingerRig and Auto-Fit Fingers To Surface | Described only in the [design notes](ProceduralGrabPosing-Design.md). Needs a section in the PoseConstrainer manual. |
| InteractionZoneVisualizer | No manual. |
| EyelidEffect | No manual. |
| KinematicHandFollower | One line in the source; mention in the Interaction Core manual. |
| GestureSetter, GestureVariable | Older gesture components; decide whether they stay before documenting. |
| SFXFeedback, MaterialHighlighter, HapticDriver, Animinteractable | Older standalone feedback components, superseded by the Feedback System. |

---

## Known gaps in existing pages

- **Images.** 28 screenshots and GIFs are referenced but not captured; see [ImagesTodo.md](ImagesTodo.md). The haptic feedback screenshot in the Feedback System manual predates pattern slots and should be retaken.
- **Interaction Core manual** describes the legacy input manager and keyboard debug keys as current options.
- **ScriptableSystem pages** duplicate material from the ReactiveVars package and can drift from it.
- **Hierarchy menus** cover only the original interactables, so manuals for newer components can only say "Add Component".

---

## Conventions

- Unity Manual style: what it does, setup, Inspector reference, workflows, troubleshooting, then a short scripting section.
- Inspector field names are written as they appear in the Inspector, in bold.
- Relative links only; anchors are the heading text in lower case with spaces as hyphens.
- Run `node Documentation~/validate-links.mjs` from the package root before publishing.

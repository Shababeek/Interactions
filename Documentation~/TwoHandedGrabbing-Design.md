# Two-Handed Grabbing — Design

*2026-06-12 — decisions: promote secondary on primary release; look-rotation solve; Grabable first.*

## Principle

The single-interactor state machine stays authoritative and untouched for the primary hand.
The second hand attaches through a separate, narrow **secondary channel** that never calls
`OnStateChanged` — so none of the recently-hardened select/deselect logic changes behavior.

## Layers

### 1. PoseConstrainer: per-hand active grab points (done)

`_activeGrabPointIndex` split into left/right. Each hand resolves its own nearest MultiPoint
grip (excluding the other hand's claimed point), `RemoveConstraints` clears only its own hand.
Also fixes the standing two-hands-corrupt-each-other bug (M-ed11).

### 2. InteractableBase: secondary contract (3 virtuals, default = refuse)

```csharp
public virtual bool CanAcceptSecondaryInteractor(InteractorBase interactor) => false;
public virtual bool TrySecondarySelect(InteractorBase interactor) => false;
public virtual void SecondaryDeselect(InteractorBase interactor) { }
```

### 3. InteractorBase: secondary-hold channel

- `StartHover`: when the target is selected but accepts a secondary, subscribe the selection
  button stream only — **no** `OnStateChanged(Hovering)` (that would deselect the primary).
- Button **Down** while target selected by another hand → `TrySecondarySelect(this)`;
  on success `isInteracting = true` + `_isSecondaryHold = true` (blocks further detection).
- Button **Up** / `DeSelect()` / `OnDisable→Release` during a secondary hold → route through a
  secondary branch in `DeSelect()` that calls `SecondaryDeselect` and never touches the
  interactable's state machine.
- `PromoteSecondaryToPrimary()`: clears the secondary flags and runs a normal `Select()`.
  Works because the recent fixes already make programmatic selection sound (button stream
  subscription on commit, abort-safe isInteracting).

### 4. TwoHandedGrabable : Grabable

- Accepts one secondary while selected.
- `TrySecondarySelect`: applies pose constraints for that hand (per-hand grab points give each
  hand the right pose), cancels the grab tween, unparents the object, starts the solve.
- **Solve (LateUpdate, smoothed exp-decay):** position from the primary grip, rotation aims the
  object's grip axis (primary→secondary anchors, from MultiPoint grab points or per-hand
  HandPositioning offsets) along the world primary→secondary hand line, roll from the primary
  hand's up. Extracted to static `TwoHandSolver` for unit testing.
- `SecondaryDeselect`: removes that hand's constraints, re-attaches the object to the primary
  attachment point (the one-hand path resumes exactly as before).
- **Primary releases while secondary holds:** normal `DeSelected` runs (throw suppressed via a
  `SuppressThrow` hook), then next frame the secondary interactor is promoted via a normal
  `Select()` — the object snaps to the remaining hand and all release semantics work because
  it is now an ordinary single-hand grab.

### Grabable changes (extensibility only)

`InitializeAttachmentPointTransform`, `AttachToHand` become protected; new protected
`CancelGrabTween()`; new `protected virtual bool SuppressThrow => false` consulted before
`ApplyThrow` in `DeSelected`.

## Later phases

- **Two-hand wheels:** per-hand angle deltas summed in RotaryInteractableBase (uses layer 1+2+3).
- **Pose blending/procedural fit per hand:** ties into procedural posing Phase 2/3.

## Known v1 limitations

- One frame between primary release and secondary promotion (object briefly free; kinematic
  state restored then re-applied). Imperceptible in practice; revisit if it shows.
- Solve assumes ConstraintTransform is rotationally aligned with the object root.
- Secondary hand's activation (use/thumb) inputs are not forwarded — primary owns Use.

# Procedural Grab Posing — Design Proposal

*2026-06-11 — draft for review, no code yet*

## Goal

Fingers wrap onto the held object's surface automatically, eliminating most manual per-object
pose authoring. Works with both pose systems (legacy playable-mixer and muscle-based) because it
operates purely on the existing 0–1 finger-weight space.

## Key insight from the pipeline

Every pose path already funnels through one point:

```
PullFingersFromHand():  this[i] = _constrains[i].constraints.GetConstrainedValue(_hand[i])
```

A fitted "max curl before surface contact" per finger is just a tighter `max` in that formula.
Procedural posing therefore needs no changes to PoseData, clips, muscles, or mixers — it is a
**per-finger clamp computed from geometry**, fed in next to the authored constraints.

## Architecture (3 pieces)

### 1. `HandFingerRig` (new component on hand prefabs)

Serialized per-finger references: tip transform, mid-phalanx transform, finger radius.
Editor button auto-populates from the humanoid avatar (`Animator.GetBoneTransform`) with a
name-based fallback for non-humanoid rigs. This is the only new requirement on hand prefabs and
removes the "no fingertip access at runtime" gap.

### 2. `FingerArcBaker` + `FingerArcs` (baked curl trajectories)

For a given HandPoseController + pose index: step each finger weight 0→1 in K samples (K≈12),
evaluate the graph/muscles out-of-band, record **hand-local** tip and mid-phalanx positions per
sample. Result (`FingerArcs`) is cached per hand prefab + pose — baked once (startup or
on-demand), so grab-time fitting never evaluates the animation graph.

### 3. `PoseFitSolver` (static, stateless)

Inputs: `FingerArcs`, the hand's target grab pose (from existing `HandPositioning` /
`GetTargetHandTransform`), and the target object's colliders.
For each finger: sphere-cast along the baked arc (segment by segment, radius = finger radius)
against **only the object's colliders** (`Collider.Raycast`, no scene-wide physics queries).
First hit → fitted curl `t` + contact point/normal. No hit → finger keeps authored max.
Cost: ~5 fingers × ≤12 segment casts against a handful of colliders — trivially cheap, no GC.

## Two consumption modes

### A. Editor: "Auto-Fit Fingers" (authoring killer)

Button in `PoseConstrainerEditor` while editing a hand (the preview hand is already posed at the
grab position): run baker+solver, write results into the serialized `FingerConstraints.min/max`
(per hand, per grab point in MultiPoint). Contact points drawn as gizmos. Designer can still
hand-tweak afterwards — output is plain authored data, zero runtime cost, fully inspectable.

### B. Runtime: fit at grab time (for uncurated/dynamic objects)

Opt-in `[RequireComponent(typeof(InteractableBase))]` component `ProceduralGrabPose`:
on Select, posed-hand solve → `HandPoseController.SetProceduralClamps(float[5] maxes)`;
on Deselect → `ClearProceduralClamps()`. Inside `PullFingersFromHand`:

```csharp
float v = _constrains[i].constraints.GetConstrainedValue(_hand[i]);
if (_proceduralClampsActive) v = Mathf.Min(v, _proceduralMax[i]);
this[i] = v;
```

One-shot at grab (objects are rigid while held); public `Refit()` for edge cases.
Works identically for fake hands (ConstrainedInteractableBase) since they use the same
HandPoseController — deferred to a later phase to keep scope tight.

## Phases

1. **HandFingerRig + FingerArcBaker + PoseFitSolver + editor Auto-Fit button.**
   Highest value (kills authoring cost), no runtime risk, exercises the whole stack.
2. **Runtime fitting for Grabable** via `ProceduralGrabPose` + the clamp layer in
   HandPoseController.
3. **Fake hands / constrained interactables + MultiPoint** (per-grab-point runtime fit) —
   lands after the two-handed prerequisite work on PoseConstrainer.

## Risks / mitigations

- **Out-of-band graph evaluation during bake** — same technique the PoseConstrainerEditor
  preview already uses (`UpdateGraphVariables` + manual evaluate); muscle mode needs one
  `ApplyMuscleWrites` per sample. Bake is offline/startup, never per-grab.
- **Thumb** — its arc is the least "curl-like"; if sphere-casts misbehave, fall back to
  authored constraints for thumb only (per-finger enable mask in the solver).
- **Concave/mesh colliders** — `Collider.Raycast` on non-convex MeshColliders works for
  raycasts (unlike ClosestPoint); segment casts handle this correctly.
- **Hand scale** — arcs are hand-local; transformed by the wrapper's world matrix at solve
  time, so the existing scale-compensation work is respected.

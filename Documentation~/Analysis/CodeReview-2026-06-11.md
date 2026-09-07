# Interaction System Code Review — 2026-06-11

Full-system review: interactables, interactors, core/input, sockets, feedback, highlight, drivers/writers, sequencing, pose/animation, editors. All findings verified against source. Paths relative to `InteractionSystem/Scripts/`.

---

## Critical — state machine lock-ups & broken features

### C1. RaycastInteractor clears CurrentInteractable while selected → object can never be released
`InteractionSystem/Runtime/Interactions/Interactors/RaycastInteractor.cs:108-121`
`PerformRaycast` runs every Update with no `IsInteracting` guard (TriggerInteractor has one). If the ray drifts off a grabbed object, `CurrentInteractable` is overwritten; button-up then sees null and `DeSelect()` never runs — `isInteracting` stays true forever, the hand is dead.
**Fix:** `if (IsInteracting) return;` at the top of `PerformRaycast()`.

### C2. Destroyed interactable while held/hovered → interactor permanently dead
`InteractorBase.cs:224-243` + `TriggerInteractor.cs:35`
`HandleInteractionStateChanged` early-returns on fake-null `currentInteractable`, so `isInteracting` is never reset and `TriggerInteractor.Update` is blocked forever. No OnDestroy notification path from interactable → interactor.
**Fix:** treat null `currentInteractable` while `isInteracting` as a forced deselect (reset flag, dispose subscriptions, null the field).

### C3. Cancelled selection (`Select()` returns true) strands `isInteracting = true`
`InteractorBase.cs:166-188` — `isInteracting` is set unconditionally even when the interactable aborts selection (state stays Hovering). Button-up requires `Selected` to deselect → same lock-up as C2. Combine with M-int1 below (InteractableBase leaves stale state on abort).
**Fix:** only set `isInteracting` if `currentInteractable.CurrentState == InteractionState.Selected` after the call.

### C4. FeedbackSystem never initializes feedbacks — MaterialFeedback is always dead
`Interactions/Interactables/Feedback/FeedbackSystem.cs:31-40`
Chicken-and-egg: `Initialize()` is only called if `IsValid()`, but `MaterialFeedback.IsValid()` requires `_isInitialized`, which only `Initialize()` sets. Material feedback never works; AudioFeedback/AnimationFeedback/ScaleFeedback auto-resolve paths also never run. Same bug in `AddFeedback()` (156-164).
**Fix:** call `Initialize` unconditionally; use `IsValid()` only when dispatching events.

### C5. CameraRig: stray `return;` makes HandPivotUpdater auto-add unreachable
`Core/CameraRig.cs:411-414` — `return; updater = gameObject.AddComponent<HandPivotUpdater>();` — if the rig has no updater, hand pivots are never driven. Compiler flags CS0162.
**Fix:** remove the stray `return;`.

### C6. CameraRig interactor setup: NRE + duplicate interactors
`Core/CameraRig.cs:485-496` — Trigger branch NREs if prefab lacks TriggerInteractor; Ray branch adds a *second* TriggerInteractor and leaves the prefab's original enabled, so both interactors fight over the same interactable state machine.
**Fix:** GetComponent-or-add symmetrically; disable existing rather than adding duplicates.

### C7. Socketable.Insert: null pivot crashes mid-state-change and kills socketing permanently
`Sockets/Socketable.cs:137-156` — `TransformArrayMultiSocket`, `GridMultiSocket`, and `ShiftingRackSocket` can all return null from `Insert` even when `CanSocket()` passed. `LerpToPosition(null)` throws *after* `IsSocketed = true` and `onSocketed` fired; the exception terminates the UniRx `OnDeselected` chain, so this object can never socket again.
**Fix:** check `t == null` before mutating any state.

### C8. HandPoseController: PlayableGraph leak + per-frame NRE
`Animations/HandPoseController.cs:136-152, 180-208, 353-368`
- `OnEnable → InitializeGraph` creates a new graph without disposing the previous; no `OnDisable` (only OnDestroy). `[ExecuteAlways]` means every enable cycle / editor selection leaks a live graph. `HandPoseControllerEditor.OnEnable` calls `Initialize()` **twice**, compounding it.
- If `handData` is null, `Update` → `UpdateGraphVariables` → `InitializeGraph` throws NRE every frame (first line dereferences `handData`).
**Fix:** `DisposeGraph()` at top of `InitializeGraph` + add `OnDisable`; guard null `handData`; remove duplicate editor Initialize.

### C9. EyelidEffect.Blink kills its own sequence — close phase never plays
`Core/EyelidEffect.cs:92-111` — `Close()`/`Open()` each call `Kill()` and reassign `_activeSequence` internally, so building Blink destroys itself as it appends.
**Fix:** extract tween builders that don't touch `_activeSequence`.

---

## High

### H1. Joystick Y normalized by X range (copy-paste) — CONFIRMED
`JoystickInteractable.cs:236-237` — `normalizedAngle.y /= Mathf.Abs(xRotationRange.x - xRotationRange.y);` should use `zRotationRange`. Also inconsistent with `NormalizedRotation` (InverseLerp 0..1) — dividing by full span gives ±0.5 at limits. Pick one convention.

### H2. Hover button-subscription leak → wrong button selects
`InteractorBase.cs:144-161` — both early returns in `EndHover` skip `DisposeHoverSubscription()`, and `StartHover` overwrites `_hoverSubscriber` without disposing. Scenario: hand A hovers X, hand B grabs X, A moves away → leaked subscription; A's next hover can be selected by the *old* object's button.
**Fix:** dispose at the top of `StartHover` and before every `EndHover` return.

### H3. ReturnWhenDisabled loop spams events every frame
`ConstrainedInteractableBase.cs:307-324` — `while (!enabled)` calls `HandleReturnToOriginalPosition()` every frame ignoring `IsReturning`; completion branches in RotaryLeverBase/Dial/Slider re-fire `onStepConfirmed`/`OnReturnComplete` every frame once at target.
**Fix:** gate on `IsReturning`, exit when done.

### H4. VRButton.OnTriggerExit fires click for any collider
`VRButton.cs:195-202` — Exit checks none of the Enter-side guards (mask, enabled, cooldown). Any unrelated collider exiting while down fires `onClick`. Unity also delivers trigger messages to disabled behaviours.
**Fix:** store the pressing collider on Enter, release only on match; check `enabled`.

### H5. Rotary grab state never initialized for FreeHand/HideHand constraint types
`RotaryInteractableBase.cs:96-122` + `ConstrainedInteractableBase.cs:84-95` — `_previousHandAngle`/`_grabRotationOffset`/twist reference only init in `PositionFakeHand`, which only runs for Constrained/MultiPoint. FreeHand/HideHand wheels get a large first-frame angle jump from stale state.
**Fix:** move grab-state init into a hook that runs on every Select.

### H6. RotaryLeverBase / Joystick: authored pre-tilt double-counted
`RotaryLeverBase.cs:44-49, 129-140`; `JoystickInteractable.cs:99-103` — `_originalRotation` caches the tilted pose AND `currentAngle` is measured from it, so first apply renders 2× the authored angle.
**Fix:** strip measured angle from `_originalRotation` or treat authored pose as 0.

### H7. Socketable.ReturnWithTween accumulates handlers + leaks ReturnTarget GOs
`Socketable.cs:242-278`; root cause `TransformTweenable.Initialize` never clears `OnTweenComplete`. Nth return fires N stale closures; `ForceReturn` leaks the `_ReturnTarget` GameObject and its handler.
**Fix:** clear event in Initialize (or store/unsubscribe delegate like Grabable does); destroy target GO in ForceReturn.

### H8. MultiSocket.Remove pools whatever transform is the socketable's parent
`Sockets/MultiSocket.cs:93-104` — no check it's actually one of this socket's pivots; can reparent the *hand attachment point* under the socket and add it to the pivot pool. LIFO pop hands the contaminated pivot to the next insert.
**Fix:** track pivot-per-socketable in a dictionary (like ShiftingRackSocket), only pool transforms the socket created.

### H9. HandTrackingInputProvider stops the shared XRHandSubsystem
`Core/Input/HandTrackingInputProvider.cs:171-179` — disabling one hand's provider calls `Stop()` on the global subsystem, killing the other hand; the other provider restarts it → stop/start fight.
**Fix:** don't stop a subsystem you don't own.

### H10. SocketableAction Unsocketed/Returned are state polls, not transitions
`SequencingSystem/Runtime/Actions/SocketableAction.cs:57-74` — "Unsocketed" completes on frame 1 if the object simply isn't socketed; "Returned" is the same check with `Skip(1)` — completes on frame 2 of idle state, detects nothing.
**Fix:** track previous state and complete on transition, or subscribe to actual events.

### H11. MaterialHighlighter (deprecated) wipes captured colors — restores black
`Feedback/MaterialHighlighter.cs:42-49` — `_color` re-allocated right after being filled; hover-end restores transparent black. Also `renderers ??=` fallback never triggers (Unity deserializes empty array, not null). Delete the second allocation or delete the class if truly deprecated.

### H12. MuscleBasedDynamicPose blends inverted vs its procedural sibling
`Animations/MuscleBased/MuscleBasedDynamicPose.cs:66-79` — `Lerp(_closedMuscles, _openMuscles, w)` vs procedural's `Lerp(open, closed, w)`. The comment admits a rig-specific sign hack. For correctly authored clips this class is inverted.
**Fix:** swap the lerp and fix the offending rig's clips, or make inversion a serialized option.

---

## Medium

### Interactables
- **M-int1.** Aborted selection leaves `currentState = Hovering` + stale `currentInteractor`; `SpawningInteractable.Select` reenters `OnStateChanged(None)` causing double hover-end events. `InteractableBase.cs:233-257`.
- **M-int2.** `OnValidate` creates GameObjects/reparents (`InteractableBase.cs:432-483`) — throws during prefab import/isolation; defer via `EditorApplication.delayCall` or move to Awake/Reset.
- **M-int3.** `JoystickInteractable.OnValidate` is `private`, hides the virtual base, never calls base — Joystick alone skips hierarchy validation. `JoystickInteractable.cs:272`.
- **M-int4.** Dial wrap-around snaps the long way: motion uses `Mathf.Lerp` but completion uses `DeltaAngle`. Use `LerpAngle`. `DialInteractable.cs:134-147`.
- **M-int5.** Drawer `onOpened`/`onClosed` fire every frame inside the epsilon zone — no edge detection. `DrawerInteractable.cs:42-50`.
- **M-int6.** Grabable never restores original parent — released objects go to scene root (breaks containers/moving platforms). Cache parent in Select, restore in DeSelected. `Grabable.cs:102`.
- **M-int7.** Public API typo `Constrainter` → `Constrainer` (`InteractableBase.cs:60-66`, used in Grabable ×5). Note: the *class* `PoseConstrainer` is spelled correctly; only the property/field is wrong. Indexer `this[HandIdentifier]` ignores its argument — remove or document.
- **M-int8.** `ConstrainedInteractableBase.InteractableObject` hides base property with different semantics (base does per-call `Find()`); duplicate `PoseConstrainer` caches (base lazy property + derived field assigned in Select). Consolidate.
- **M-int9.** `GetWorldAxis` doesn't mirror `GetLocalAxis` negation for Right/Up — Dial/Wheel gizmos rotate opposite to the object. `RotaryInteractableBase.cs:57-66 vs 167-175`.
- **M-int10.** Collider/layer cache from Awake goes stale; destroyed collider while held → NRE; layers snapshotted *after* `onSelected.Invoke`. `InteractableBase.cs:263-296`.
- **M-int11.** `DestroyImmediate` of scale-compensator children in OnDestroy — destroys user content if only the component is removed; use `Destroy` and only destroy self-created objects. `InteractableBase.cs:543-553`.

### Interactors / Core / Input
- **M-core1.** TriggerInteractor throttle timer never resets — `distanceCheckInterval` is dead, overlap runs every frame. `TriggerInteractor.cs:31-37`.
- **M-core2.** `DeSelect()` fires spurious StartHover+EndHover pair every release — feedback flashes. `InteractorBase.cs:193-204`.
- **M-core3.** TriggerInteractor measures distance to `GetComponentInChildren<Collider>()` instead of the actual overlap hit; wrong ranking for multi-collider objects, garbage for concave meshes, per-candidate alloc. Use `overlapResults[i].ClosestPoint`. `TriggerInteractor.cs:65, 83-87`.
- **M-core4.** RaycastInteractor selects through walls (ignores closer non-interactable hits), unsorted capped buffer, never checks `CanInteract(Hand)`. `RaycastInteractor.cs:88-106`.
- **M-core5.** PhysicsHandFollower: NaN angular velocity from `ToAngleAxis` near identity; unclamped linear velocity on teleport. `Core/PhysicsHandFollower.cs:41-49`.
- **M-core6.** ControllerInputProvider: null actions read as `?? 0.5f` → exceeds 0.2 thresholds → buttons permanently "pressed". Default to 0. `Core/Input/ControllerInputProvider.cs:96-100`.
- **M-core7.** Controller hand-matching heuristics can bind the wrong controller (name-contains fallbacks, "first of 2 is left"). Prefer `XRController.leftHand/rightHand`. `ControllerInputProvider.cs:180-195`.
- **M-core8.** `Config.SetHandProvider` writes scene MonoBehaviours into serialized SO fields → dangling Missing refs in the asset after play mode. Keep runtime overrides in the non-serialized cache. `Core/Config.cs:199-211`.
- **M-core9.** `NewInputSystemBasedInputManager` Enable/Disable dereference actions without null checks (Update uses `?.`). `Core/Input/NewInputSystemBasedInputManager.cs:23-55`.
- **M-core10.** GestureSetter: missing truth-table case leaves stale gesture; no null guard on `gestureVariable`. `Interactors/GestureSetter.cs:34-55`.

### Sockets / Feedback / Drivers / Sequencing
- **M-soc1.** Destroyed socketables permanently leak socket capacity (null occupants skipped but counted; no Socketable OnDestroy cleanup). `ShiftingRackSocket.cs:299-303`, GridMultiSocket, TransformArrayMultiSocket.
- **M-soc2.** Public `Socketable.Insert` bypasses category mask (`CanSocket(this)` never consulted). `Socketable.cs:139`.
- **M-soc3.** `ForceReturn`/`ReturnToOriginalState` teleport to world origin when `shouldReturnToLastSocket` is false (initial pose only captured conditionally). Capture unconditionally in Awake. `Socketable.cs:112-117, 363, 397`.
- **M-soc4.** `returnDuration` serialized + drawn in editor but never read — dead knob. `Socketable.cs:25`.
- **M-soc5.** `GridMultiSocket.OnValidate` rebuilds pivots at runtime, orphaning old GOs and un-tracking socketed objects. `GridMultiSocket.cs:174-177`.
- **M-soc6.** `TransformArrayMultiSocket.ForceSocketToSlot` desyncs Socketable state (no events, no IsSocketed). `TransformArrayMultiSocket.cs:175-189`.
- **M-soc7.** Debug `P`-key insert bypasses all bookkeeping; on by default. Gate behind `#if UNITY_EDITOR`. `Socketable.cs:184-200`.
- **M-soc8.** Log spam: `verboseSocketLogs` defaults true; ShiftingRackSocket has ~9 unconditional Debug.Logs.
- **M-soc9.** Accidental nested namespace `Shababeek.Interactions.Shababeek.Interactions` in `MultiSocket.cs:5-10` — forces bizarre usings in GridMultiSocket/ShiftingRackSocket.
- **M-fb1.** `MaterialFeedback.hoverColor` is dead — hover applies `original * colorMultiplier` instead. `FeedbackSystem.cs:300, 357`.
- **M-fb2.** MaterialFeedback leaks material instances (`renderers[i].material` per access, never destroyed). Use MaterialPropertyBlock. `FeedbackSystem.cs:334+`.
- **M-drv1.** Lever/Wheel writers: `invertOutput` means "mirror in 0-1" for normalized but "negate" for angle; multiplier breaks documented 0-1 contract. `LeverVariableWriter.cs:45-54`, `WheelVariableWriter.cs:50-55`.
- **M-seq1.** TimerAction ignores step lifecycle — `startOnEnable` can pre-complete unstarted steps; never stopped on Inactive/Completed (zombie timers). `Actions/TimerAction.cs:28-57`.
- **M-seq2.** ControllerButtonAction silently ignores `XRButton.Any` — step can never complete. `Actions/ControllerButtonAction.cs:41-58`.
- **M-seq3.** InsertionAction: NRE on unassigned interactable; never calls `CompleteStep()` — occupies a step it can't finish. `Actions/InsertionAction.cs:18-26`.
- **M-seq4.** `AbstractSequenceAction.OnEnable` NREs when `step` unassigned — affects all 12 action types. Add guard + clear log. (ReactiveVars `AbstractSequenceAction.cs:47`.)
- **M-seq5.** DrawerAction resets `_currentValue = 0` on step start regardless of real drawer position — false completes with `targetValue` near 0. Initialize from the drawer. `Actions/DrawerAction.cs:116-124` (same pattern in JoystickAction).

### Editors / Setup
- **M-ed1.** `ConstrainedInteractableEditor` references nonexistent `_snapDistance` — CONFIRMED; "Snap Distance" silently never renders for all 7 constrained interactables. Delete or add the field. `ConstrainedInteractableEditor.cs:19,48`.
- **M-ed2.** `PoseConstrainerEditor` NREs every repaint when no Config asset exists. Guard `_config == null` in `DrawHandConstraints`. `PoseConstraintEditor.cs:384`.
- **M-ed3.** `PoseConstraintPropertyDrawer` height omits the pose-dropdown lines — last finger row overflows. Unused `hasPose` field was meant for this. `HandConstraintEditor.cs:144-149`.
- **M-ed4.** `HandPoseControllerEditor`: pose index written without Undo/SetDirty; `DoFingerHandle` contains a dead while-loop; discarded change check in OnSceneGUI. `HandPoseControllerEditor.cs:87-129`.
- **M-ed5.** Layer-creation loop off-by-one (reads index 32 on a 32-element array; skips slot 6) — duplicated in **three** files: `SetupChoiceStep.cs:170-190`, `LayersInputStep.cs:88-108`, `InteractionSystemLoader.cs:254-275`.
- **M-ed6.** `InteractionSystemLoader.InitializeConfigFile`: stores **bitmask** where layer **index** expected — masked only because it searches the wrong type name (`Shababeek.Interactions.Runtime.Config` doesn't exist). Two bugs hiding each other. Also `InitializeInputManager` destructively truncates user input axes. `InteractionSystemLoader.cs:235-292`.
- **M-ed7.** OpenXR check uses assembly name `Unity.XR.OpenXR` instead of package name `com.unity.xr.openxr` → always fails → `SetupChoiceStep` re-installs with a main-thread `Thread.Sleep` loop (frozen editor). `SetupChoiceStep.cs:273-308`, `DependenciesCheckStep.cs:90`.
- **M-ed8.** Wizard "Try Again" skips a step (calls `NextStep()` from `OnStepExit`, which the wizard follows with its own `_step++`). 3 files. `LayersInputStep.cs:224` etc.
- **M-ed9.** Hardcoded paths `Assets/Shababeek/...` don't match actual install `Assets/_Shababeek/Hands/...` — asset postprocessor never triggers; wizard creates assets in a foreign folder. `ShababeekSetupWizard.cs:26,211`, `FeedbackDrawerBase.cs:32`.
- **M-ed10.** `HandData.Poses` **getter** mutates serialized asset (forces Name/Type on defaultPose) without dirtying; `HandDataEditor` mutates a struct copy returned by value — no-op dead code. `HandData.cs:104,123-127`, `HandDataEditor.cs:29-30`.
- **M-ed11.** `PoseConstrainer` shares one `_activeGrabPointIndex` across both hands — second hand's MultiPoint grab corrupts the first's. Track per HandIdentifier. `PoseConstrainer.cs:94,170-189`.

---

## Low (selected)

- `SpawningInteractable`/`DebugInteractable` lack `[AddComponentMenu]`; Spawning copies position but not rotation, no prefab null check. (Joystick/Lever **do** have the attribute — earlier note outdated.)
- `ConstrainedInteractableBase.cs:65` — guard `CurrentInteractor` destruction: `if (IsSelected && CurrentInteractor)`.
- Joystick plane projection blows up when hand goes below pivot (`Mathf.Max(0.01f, handLocal.y)` on negative y). `JoystickInteractable.cs:166`.
- `Switch.ApplyAngle` euler read-modify-write drifts with multi-axis rotation; compose from cached original + AngleAxis instead. `Switch.cs:147-158`. Switch fires no initial-state event in Start.
- Trigger filtering inconsistent: Switch ignores trigger colliders, VRButton/VRToggleButton accept them; default `maskName` differs ("" vs "tip").
- `Quaternion.Lerp` → `Slerp` for pose transitions. `ConstrainedInteractableBase.cs:57`.
- `Outline.Awake` no null guard on `Resources.Load` (use `EnsureRuntimeResources`); static `registeredMeshes` never pruned; comment claims `[ExecuteAlways]` but attribute absent; `InteractableOutlineFeedback` is in the global namespace.
- `SFXFeedback` toggles evaluated once at Awake; `audioSource.Play()` cuts previous clip — use `PlayOneShot`. No `OnUseEnded` hook in FeedbackData.
- `SliderVariableWriter`: no initial value written on enable (Dial/Lever do); uses `NumericalVariable<int>` vs siblings' `IntVariable`; missing class XML doc.
- `SocketableToBoolDriver` stale after non-grab unsocket; `SocketToBoolDriver` wrong for multi-sockets; `SocketableToEventDriver` lacks unsocketed event.
- GazeAction caches `Camera.main` in Awake — dead if rig spawns later. Dead fields: `GrabHoldAction._currentInteractor`, `LeverAction._wasAtTarget`, `Socketable._nextNoHitLogTime`.
- Interactor misc: `Hand.OnAnyButtonStateChange` doc says thumb but merges only trigger/grip; `handModel?.gameObject` C#-null-conditional on UnityEngine.Object; `KeyboardbasedInput` uses legacy Input API; `XRButtonObserver` appears dead — delete; `RigVisualizer` adds components inside OnDrawGizmos.
- Editor misc: unconditional `Undo.RecordObject` every scene-GUI event (Lever/Joystick/Switch/ToggleSwitch editors); `GameObjectMenuHelpers` dead `typeof(T) == null` validation, destructive no-Undo camera deletion, hardcoded URP shader; `FingerColliderSetup` computes direction it never uses, no Undo registration; `FeedbackSystemEditor` mutates runtime list directly despite `[CanEditMultipleObjects]`; `FeedBackDataDrawer` labels every type "Animation … Settings"; `PhysicsHandManagerMenu.cs` is an empty file; `IPosabale.cs` filename typo (interface is `IPoseable`); `ITransfrormConstrainer` misspelled, orphaned, wrong namespace — delete.

## Documentation/tooltip gaps (per project conventions)

Public members missing XML docs and serialized fields missing tooltips, concentrated in: `VRInteractionZoneVisualizer` (~25 public props), `EyelidEffect`, `Gesture`, `HandIdentifier`, `ShiftingRackSocket` (haptics fields + 6 public members), `Socketable.Insert/OnSocketedAsObservable`, `HandPoseController.fingers/currentPoseIndex`, `PoseConstrains`/`HandConstraints` (XML present, `[Tooltip]` absent), `InteractableBase.InteractionHand` + indexer, sequencing `HoldProgress` props, `SocketAction.socketEventType`/`SocketableAction.eventType`.

## What's in good shape

UniRx hygiene in Drivers/Writers and sequencing actions (OnEnable-new/OnDisable-dispose) is consistent. Grabable's throw pipeline (FixedUpdate sampling, kinematic restore ordering, ring-buffer averaging, ToAngleAxis wrap) is correct. Grabable correctly unsubscribes its tween callback. The fake-hand collider disable and uniform scale compensation from the previous cleanup pass hold up.

## Suggested fix order

1. **C1–C3 + H2** — interactor state machine (lock-ups + subscription leak). One file cluster, highest player impact.
2. **C4** — FeedbackSystem init (material feedback is fully broken).
3. **C7, H7, H8** — socket state corruption.
4. **C8 + M-ed* (5)** — HandPoseController graph leak/NRE + editor double-init.
5. **C5, C6** — CameraRig setup.
6. **H1, H3–H6** — interactable behavior bugs.
7. **H10, M-seq*** — sequencing correctness.
8. Setup wizard cluster (M-ed5–M-ed9) — these mostly affect first-run experience.

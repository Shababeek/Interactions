# Pre-publish Code Review — 2026-09-06

Quick pass over `Assets/_Shababeek/Hands` (InteractionSystem + ReactiveVars submodules, ~64k lines excluding UniRx) ahead of the Asset Store update. Every item below was checked against the current source. Paths are relative to the `Hands/` root. Line numbers are current as of this pass.

The June review (`InteractionSystem/Documentation~/Analysis/CodeReview-2026-06-11.md`) is also re-checked at the end: 11 of its items are fixed, most of the rest are still open.

---

## Blockers — will not compile or will break on import for some customers

### B1. `GetEntityId()` / `EntityId` require Unity 6000.3+, but package.json says `"unity": "6.0"`
`InteractionSystem/Scripts/InteractionSystem/Runtime/Interactions/Interactables/ElasticTetherInteractable.cs:733`
`InteractionSystem/Scripts/InteractionSystem/Runtime/Animations/ProceduralPosing/FingerArcCache.cs:12,19`
The whole `Shababeek.Interactions` assembly fails to compile on 6000.0–6000.2. Either replace with `GetInstanceID()` (int key in the dictionary tuple) or bump the declared minimum Unity version in both package.json files and the store listing.

### B2. ReactiveVars sequencing has a hard, unguarded dependency on the Input System
`ReactiveVars/Runtime/SequencingSystem/Core/Core/SequenceBehaviour.cs:7,38,41,143`
`ReactiveVars/Runtime/SequencingSystem/Core/Core/BranchingSequenceBehaviour.cs:5,36,128`
`using UnityEngine.InputSystem;`, `Key` fields and `Keyboard.current` are unconditional, while the InputAction drivers are correctly wrapped in `#if REACTIVE_VARS_INPUT_SYSTEM` and `com.unity.inputsystem` is not in ReactiveVars' package.json. Any project without the Input System package fails to compile. Wrap the debug-key fields and `Update()` in `#if REACTIVE_VARS_INPUT_SYSTEM`.

### B3. `VariableDebugOverlay` uses legacy `Input.GetKeyDown`
`ReactiveVars/Runtime/ScriptableSystem/Utility/VariableDebugOverlay.cs:57`
Throws `InvalidOperationException` every frame when Active Input Handling is "Input System Package (New)" — the exact configuration the package's own drivers target (and what most XR projects use). Guard with `#if ENABLE_LEGACY_INPUT_MANAGER` / `#elif ENABLE_INPUT_SYSTEM` or expose a public `Toggle()` and drop key polling. Same concern for `Socketable.DebugKeyHandling` (see H7) and `KeyboardbasedInput.cs`.

### B4. `Outline` and two feedback classes are in the global namespace under the QuickOutline class name
`InteractionSystem/Scripts/InteractionSystem/Runtime/Highlight/Outline.cs:15`, `InteractableOutlineFeedback.cs:15`, `DemoOutlineFeedback.cs:16`
Any customer who already has QuickOutline in their project (very common) gets a duplicate-type compile error on import. Move into `Shababeek.Interactions.Highlight` and rename `Outline`. Also: QuickOutline (MIT, Chris Nolet) is not listed in `ThirdPartyNotices.md` — the header of the file still carries his copyright, so the notice is required.

### B5. ReactiveVars `package.json` dependencies won't resolve
`ReactiveVars/package.json:8-9`
A git URL (`com.neuecc.unirx`) is not a valid package-to-package dependency (UPM only resolves git URLs from the project manifest), and `com.unity.textmeshpro@3.0.6` doesn't exist in Unity 6 (TMP lives in `com.unity.ugui@2.0.0`; the asmdef already references it by GUID). This only matters if ReactiveVars is ever installed as a UPM package rather than dropped under Assets, but it's a one-line fix: `"com.unity.ugui": "2.0.0"`, drop the git entry, add `com.unity.inputsystem` if you keep B2 as a hard dependency.

### B6. Stray `return;` in CameraRig (June C5 — still open)
`InteractionSystem/Scripts/InteractionSystem/Runtime/Core/CameraRig.cs:486`
`return; updater = gameObject.AddComponent<HandPivotUpdater>();` — if the rig prefab lacks a `HandPivotUpdater`, hand pivots are never driven. Compiler warns CS0162. Delete the `return;`.

---

## High — customer-visible bugs

### H1. Console spam is on by default in shipping code
- `Sockets/Fastener.cs:39` and `Sockets/FastenerTool.cs:62` — `verboseLogs = true` by default; logs every engage/disengage flicker and a turn line every 0.25 s.
- `Sockets/ShiftingRackSocket.cs:241,248,256,297,347,363,370,507` — 8 unconditional `Debug.Log($"[Rack:{name}] …")` (line 363 also does a LINQ `string.Join` allocation per call). The `verboseSocketLogs` field was removed but the logs stayed; `Socketable._nextNoHitLogTime` is now dead.
- `Sockets/BackpackItemGrabber.cs:59,79` — unconditional log per grip.
- `ReactiveVars/Runtime/SequencingSystem/Core/Core/Sequence.cs:78`, `BranchingSequence.cs:110` — `Debug.Log("starting sequence{name}")` (missing space) on every `Begin()`.
- `ReactiveVars/Runtime/ScriptableSystem/Variables/VariableContainer.cs:226,251,312` — logs on every save/load/delete.
- `ReactiveVars/Editor/VariableDrawer.cs:80` — `Debug.Log(property.objectReferenceValue)` on every inspector build for every `ScriptableVariable<>` field.
- `Weapons/Bullet.cs:89` — `Debug.DrawLine` every physics step per live bullet.
Default the flags to false, gate the rest behind a serialized `verboseLogging` bool or `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.

### H2. ReactiveVars `operator ==` breaks Unity null checks on every variable type
`ReactiveVars/Runtime/ScriptableSystem/Variables/FloatVariable.cs:39-56` — same pattern in all 18 `Variables/*Variable.cs`.
The overload uses `ReferenceEquals(a, null)`, so `myFloatVar == null` is **false** for an unassigned/destroyed reference in the editor (Unity fake-null), and `a == b` compares by value while `Equals` is by reference. Every `variable != null` guard in the binders (e.g. `CanvasGroupBinder:86`, `RigidbodyPropertyBinder:55-111`, `CameraBinder:129`) silently passes for missing references. Remove the `==`/`!=` overloads on the SO types (keep arithmetic operators and `Equals`), or make them defer to `(UnityEngine.Object)a == null`.

### H3. ScriptableVariable values persist into the asset after play mode
`ReactiveVars/Runtime/ScriptableSystem/Variables/ScriptableVariable.cs:55-64`, `Utility/VariableResetter.cs:18,23`
`Value` writes straight to the serialized field; the only mitigation (`VariableResetter`) snapshots in `Start()` (after other `Awake`/`OnEnable` writers), restores in `OnDestroy` via `SetValue` (raises events during teardown), and stores reference types like `List<string>` by reference so they're never actually restored. Add an `initialValue` field with a reset on play-mode enter (editor asmdef `playModeStateChanged`) or `[RuntimeInitializeOnLoadMethod]`; make the resetter capture in `Awake` and deep-copy. This is the #1 support question for every SO-variable asset on the store.

### H4. Sequence runtime state is serialized into the asset
`ReactiveVars/Runtime/SequencingSystem/Core/Core/SequenceNode.cs:24-25`
`status` and the scene `AudioSource` are `[SerializeField]` on the ScriptableObject — after play mode `Sequence.Started` stays true and the asset carries a dangling scene reference (dirty asset, inspector shows "Restart"). Make both `[NonSerialized]`. Related: `Sequence.cs:88,214`, `BranchingSequence.cs:119,252`, `Step.cs:64,143` — `initialized` bool is only reset on SO `Awake/OnEnable`, so after a scene change destroys `{name}_AudioObject` the next `Begin()` skips recreation and throws `MissingReferenceException`; `Reset()` also never destroys the old audio object (one leaked per restart). Test `audioObject == null` instead of a bool and destroy it in `Reset()`.

### H5. SequenceBehaviour subscriptions stack on every enable
`ReactiveVars/Runtime/SequencingSystem/Core/Core/SequenceBehaviour.cs:79,85,162,172`, `BranchingSequenceBehaviour.cs:71,81,140,150`
Subscribed in `OnEnable`, disposed with `.AddTo(this)` (destroy-scoped) → each disable/enable cycle adds another subscription and `onSequenceStarted/Completed` fire N times. Use a `CompositeDisposable` disposed in `OnDisable`, like the binders do. Also `async void BeginSequence` (93-100) calls `sequence.Begin()` after the await even if the behaviour was destroyed — pass `destroyCancellationToken`.

### H6. FastenerTool leaves the real hand hidden when dropped mid-operation
`Sockets/FastenerTool.cs:363-373`
`OnDeselected` only clears button flags; `StopOperate` runs next `Update`, by which time `InteractableBase` has nulled `CurrentInteractor` (`InteractableBase.cs:244`), so `ToggleHandModel(true)` never runs. Cache the interactor in `SpawnFakeHand` and restore from that, or call `StopOperate()` from the `OnDeselected` handler. `OnDisable` (line 119) has the same gap — call `StopOperate()` there too.

### H7. `Socketable` debug key insert ships in runtime code (June M-soc7 — still open)
`Sockets/Socketable.cs:255-271`
`DebugKeyHandling()` is not `#if UNITY_EDITOR`, uses legacy `Input.GetKeyDown` (see B3), sets `IsSocketed = true` before the `socket.Insert` null check and skips `onSocketed`/`_lastSocket`. Gate the whole method.

### H8. InventoryGridSocket scale-to-fit is silently undone
`Sockets/InventoryGridSocket.cs:392` vs `Sockets/Socketable.cs:281`
`Insert` sets the fit scale, then `Socketable.LerpToPosition` writes `transform.localScale = _initialLocalScale` right after, so items never shrink into the grid. Have `AbstractSocket.Insert` return a desired scale / flag and skip the reset for scale-managing sockets. Related: destroyed items keep their packer cells reserved forever and `GetItemAtWorldPosition` hands a fake-null `Socketable` to `BackpackItemGrabber.cs:63` (`MissingReferenceException`) — prune null keys.

### H9. Bullet pass-through applied too late
`Weapons/Bullet.cs:105-112`
`Physics.IgnoreCollision` is called in `OnCollisionEnter`, i.e. after PhysX has already resolved the contact — first shot per bullet/collider pair bounces off the shooter's hand or gun. Have `GunFiring` pass the gun/hand colliders into `Launch` and ignore before setting velocity (or rely on the layer matrix). Also `:120-122` reads `_body.linearVelocity` after the collision resolved, so on a bounce the impact direction points away from the target — use `-collision.relativeVelocity`.

### H10. `ReturnToOrigin` fights sockets
`Interactables/ReturnToOrigin.cs:62,117-118`
`ScheduleReturn` fires on every deselect, including release into a socket; `returnDelay` later it re-parents the item to origin while `Socketable.IsSocketed` stays true (phantom occupant). Skip the return when the object's `Socketable` is socketed, or cancel on socket events.

### H11. Still-open June items with real player impact
See the regression table at the end for evidence lines. In severity order: **H7** (Socketable return-tween handler accumulation + `_ReturnTarget` GO leak — root cause `TransformTweenable.Initialize` never clears `OnTweenComplete`), **H8** (MultiSocket.Remove pools the hand attachment point), **H3** (ReturnWhenDisabled per-frame spam loop), **H5** (rotary grab state not initialised for FreeHand/HideHand), **H6** (Lever/Joystick authored pre-tilt double-counted), **H9** (HandTrackingInputProvider stops the shared XRHandSubsystem), **H10** (SocketableAction Unsocketed/Returned are polls), **H1** (Joystick Y normalised by X range — `JoystickInteractable.cs:298`, one-token fix), **H4** (VRButton exit still fires for unrelated colliders unless `handsOnly`), **H11** (MaterialHighlighter restores black), **H12** (MuscleBasedDynamicPose inverted lerp).

---

## Medium

### Interaction System
- `Sockets/BackpackItemGrabber.cs:23` — `FindObjectsOfType<TriggerInteractor>()` is obsolete (CS0618) and runs once in `Start`; interactors spawned later never subscribe. Use `FindObjectsByType<T>(FindObjectsSortMode.None)` and expose `Register(interactor)`.
- `Highlight/Outline.cs:96,192` — `Resources.Load` with no null guard (the material does ship under `InteractionSystem/Resources/Materials/OutlineFill.mat`, so this is defensive only — route through `EnsureRuntimeResources()`); `renderer.materials = materials` instantiates a copy per renderer per attach and `DetachMaterials` only restores `sharedMaterials` → material instance leak per hover/select cycle.
- `Weapons/Target.cs:218-232` — `KnockdownRoutine` awaits with `destroyCancellationToken` but never catches `OperationCanceledException`; destroying a target mid-animation logs an unhandled exception. Wrap like `ReturnToOrigin` does.
- `DistanceGrab/DistanceGrabber.cs:313-315` — `TryHandOff` overwrites `_interactor.CurrentInteractable` without ending the current hover → object stuck in `Hovering` with a stale interactor. `:108-111` — `OnEnable` reads `_interactor.Hand` which is set in `InteractorBase.Awake`; NRE if component order puts DistanceGrabber first.
- `Core/CameraRigLocomotion.cs:65-68` — `OnDisable` calls `Disable()` on assigned `InputActionReference`s, switching the action off for every other consumer of the shared asset. Only disable actions this component created.
- `Weapons/GunFiring.cs:131` — `OnDestroy` clears the pool while bullets are in flight; they later `Release` into an orphaned pool. `:78` tooltip says `onDryFire` fires on cooldown, but the cooldown path (`:151`) returns silently.
- `Weapons/BladeHit.cs:124` — `_lastHitTimes` grows unbounded (destroyed colliders never removed).
- `Core/Recording/InteractionRecorder.cs:85` — `_eventSubscriptions` never disposed if destroyed mid-recording. `InteractionRecordingPlayer.cs:47` — public property named `Time` shadows `UnityEngine.Time`.
- `Sockets/SocketMaskRegistry.cs:335` — `<see cref="UnityEditor.EditorGUI.MaskField…">` in a runtime assembly (CS1574 under doc generation).
- June mediums still open (evidence in the table): M-int1, M-int6 (Grabable never restores parent), M-int7 (`Constrainter` typo is public API — rename before this version ships, since renaming later breaks customers), M-core1, M-core6 (null actions read as 0.5 → buttons permanently pressed), M-core8, M-soc1, M-soc5, M-fb2, M-seq3, M-seq4, M-ed1, M-ed2, M-ed5, M-ed6, M-ed7 (main-thread `Thread.Sleep` loop in the setup wizard), M-ed9 (hardcoded `Assets/Shababeek/` paths — the install path is `Assets/_Shababeek/Hands/`; `SocketMaskDrawer.cs:11` already uses the real path, the wizard and `FeedbackDrawerBase.cs:32` don't).

### ReactiveVars
- `Variables/ScriptableVariable.cs:97-108` — `Raise(T data)` calls `Raise()` (emits current value) then emits `data` → two notifications, first with the wrong value.
- `SequencingSystem/Actions/AbstractSequenceAction.cs:47`, `Core/Core/MultiStepListener.cs:57`, `Actions/MultiConditionAction.cs:67` — `step.OnRaised` dereferenced in `OnEnable` with no null check (all 12 action types NRE on an unassigned step).
- `SequencingSystem/Core/Core/Sequence.cs:100` — `Begin()` on a zero-step sequence throws `ArgumentOutOfRangeException`.
- `Utility/TimerDriver.cs:49,133` — `autoStart` (default true) with no variable → NRE in `OnEnable` right after the "not assigned" warning.
- `Editor/SequencingSystem/SequenceEditor.cs:102,118,134` — steps added/removed by mutating `sequence.Steps` directly, `RemoveObjectFromAsset` with no Undo and no `SetDirty`; `OnInspectorGUI` never calls `serializedObject.Update()`; "Create Sequence in Scene" creates GameObjects without `Undo.RegisterCreatedObjectUndo`.
- `Editor/VariableContainerEditor.cs:310,350` — `Undo.RecordObject` + `DestroyImmediate(variable, true)` leaves Undo pointing at a destroyed sub-asset. Use `Undo.DestroyObjectImmediate`. `:213,257` — value edits / "Raise" invoke `UnityEvent` listeners in edit mode (`GameEventEditor:25` correctly gates on `Application.isPlaying`).
- `Editor/VariableDrawer.cs:95` — "Find Asset" sets `objectReferenceValue` without `ApplyModifiedProperties` → never persisted; unused `isShared` toggle; namespace is `Shababeek.Interactions.Editors` (copy-paste).
- `Editor/GameEventListenerEditor.cs:78`, `Editor/SequencingSystem/StepEventListenerEditor.cs:80`, `BranchingSequenceEditor.cs:257` — nested editors from `CreateEditor` never destroyed (leak per selection change; `BranchingSequenceGraphWindow:258` does it right).
- `Tests/Shababeek.ReactiveVars.Tests.asmdef:17-24` — `precompiledReferences` and `defineConstraints` each duplicated. Also: `UNITY_INCLUDE_TESTS` is always defined under `Assets/`, so if the store build ships `Tests/`, the 13 ReactiveVars + 23 InteractionSystem test files compile into the customer's project and appear in their Test Runner. Exclude `Tests/` from the .unitypackage (or move them behind a `Tests~` folder).

---

## Packaging / listing hygiene

- **`ThirdPartyNotices.md` is out of date.** It lists hand models that aren't in this folder (Robot Clown, Sci-fi, Viking) and omits IndustrialGloves; it claims a "Clown Nose Sound" `.wav` that doesn't exist here (no audio files at all in the package); it does not mention QuickOutline (B4). The example scene's `Models/` folder has `VRHUMAN VR Essentials Vol 1`, `Oculus Rift Controllers`, `Oculus Controller R`, `Assault Rifle`, `Pistol`, etc. — if any of those are third-party downloads, the store reviewer will want their licences listed here (or the models dropped from the example scene).
- `InteractionSystem/package.json` — `"description": ""`, `displayName` has a trailing space, `"unity": "6.0"` is not a valid UPM version string (use `"6000.0"` like ReactiveVars — or `"6000.3"` per B1), `author.url` lacks a scheme, `com.unity.inputsystem: 1.0.0` is far below what Unity 6 ships (1.11+). Neither package has a `CHANGELOG.md`.
- Both packages are still `"version": "1.0.0"` — bump before publishing.
- `ReactiveVars/Runtime/SequencingSystem/Core/Core/Sequence.cs:8` — `InternalsVisibleTo("Shababeek.GoodMorning")` is a leftover from the game project.
- `ReactiveVars` `AddComponentMenu` paths split between `"ReactiveVars/…"` (34 files) and `"Shababeek/ReactiveVars/…"` (32 files) — components appear under two top-level menus.
- `ScriptableVariable.cs:27` — `[CreateAssetMenu]` on the open generic base yields a menu entry that can't create an asset.
- Stray files that will get packaged: `.gitmodules.swp` (root), four `materials.mtl.bak` / `doorway.mtl.bak` under `ExampleScene/Models/`, `ExampleScene/Blend Files~/ControlRoomKit.blend1` (ignored by git and Unity, but still on disk).
- `InteractionSystem/Scripts/.../Runtime/Interactions/Interactables/DebugInteractable.cs` (5 logs) and `ExampleScene/Scripts/DrillGunHandler.cs` (2 logs) — fine if intentional demo helpers, but consider whether `DebugInteractable` belongs in the runtime assembly of a paid asset.
- `GameEvent.cs:31`, `SequenceNode.cs:27` — `private new readonly` hides nothing → CS0109 warnings visible to every customer on import.

---

## Documentation / tooltip gaps (project convention: public members get short XML docs, serialized fields get tooltips)

**InteractionSystem** — worst files: `InteractionZoneVisualizer.cs` (~24 public props undocumented, 4 fields without tooltip — unchanged since June), `InventoryGridSocket.cs` (~13 public members), `OutlineFeedbackConfig.cs` (~9 props + `OutlineState` fields), `ShiftingRackSocket.cs` (~9), `InteractionRecording.cs` (~8), `ItemFootprint.cs` (~7, plus public `dimensions` field duplicating the `Dimensions` property), `GridPacker.cs` (~6), `SocketMask.cs` (~6), then a handful each in `FastenerTool.cs`, `Fastener.cs`, `GunFiring.cs`, `BladeHit.cs`, `Bullet.cs`, `CameraRigLocomotion.cs`, `BackpackItemGrabber.cs` (no class doc), `Outline.cs`.

**ReactiveVars** — roughly 294 public members without XML docs, concentrated in `VariableContainer.cs` (~30) and the operator groups of `IntVariable`/`FloatVariable` (~22 each), `LayerMaskVariable` (~18), `Vector2/Vector3/EnumVariable` (~17 each), `Vector2Int/ColorVariable` (~16), `BoolVariable` (~15) — `<inheritdoc/>` on operator groups would clear most of it cheaply. Roughly 152 serialized fields without `[Tooltip]`, worst in `TransformFollowerBinder.cs` (10), `TransformVariable.cs` (9), `BranchingSequence.cs` (8), `VariableContainer.cs` and `GradientSamplerBinder.cs` (7 each), `NumericalAudioBinder.cs` (6).

---

## June 2026 review — regression status

Fixed: C1, C2, C3, C4, C6, C7, C8, C9, H2, M-soc2, M-ed11.

| ID | Status | Location | Evidence |
|---|---|---|---|
| C5 | open | `Core/CameraRig.cs:486` | `return; updater = gameObject.AddComponent<HandPivotUpdater>();` |
| H1 | open | `JoystickInteractable.cs:298` | `normalizedAngle.y /= Mathf.Abs(xRotationRange.x - xRotationRange.y);` |
| H3 | open | `ConstrainedInteractableBase.cs:454-458` | `while (!enabled) { await …; HandleReturnToOriginalPosition(); }` no `IsReturning` gate |
| H4 | partial | `VRButton.cs:275-288` | Exit checks `Accepts(other)` which is `true` unless `handsOnly`; no pressing-collider match |
| H5 | open | `RotaryInteractableBase.cs:131-157` | grab-state init only inside `PositionFakeHand` (Constrained/MultiPoint only) |
| H6 | open | `RotaryLeverBase.cs:63-65,162`; `JoystickInteractable.cs:134-135,246` | `_originalRotation` cached from tilted pose, then `* AngleAxis(currentAngle)` |
| H7 | open | `Socketable.cs:339,477-490`; `ReactiveVars/…/TransformTweenable.cs:43-63` | `OnTweenComplete +=` each return, never cleared; `_ReturnTarget` never destroyed in `ForceReturn` |
| H8 | open | `MultiSocket.cs:96-101` | pools `socketable.transform.parent` unconditionally |
| H9 | open | `HandTrackingInputProvider.cs:175-178` | `handSubsystem.Stop()` on the shared subsystem in OnDisable |
| H10 | open | `SequencingSystem/Runtime/Actions/SocketableAction.cs:57-74` | `EveryUpdate().Where(!IsSocketed).Take(1)` / `.Skip(1)` — polls, not transitions |
| H11 | open | `Feedback/MaterialHighlighter.cs:42,49` | `_color = new Color[…]` re-allocated after fill |
| H12 | open | `MuscleBased/MuscleBasedDynamicPose.cs:77` | `Lerp(_closedMuscles, _openMuscles, w)` inverted vs procedural sibling |
| M-int1 | open | `InteractableBase.cs:266-282`; `SpawningInteractable.cs:25` | aborted select leaves `Hovering` + stale interactor |
| M-int6 | open | `Grabable.cs:128` | `transform.SetParent(null, true)` — parent never restored |
| M-int7 | open | `InteractableBase.cs:73,193` | `public PoseConstrainer Constrainter`; indexer ignores argument |
| M-core1 | open | `TriggerInteractor.cs:33-34` | `_timeSinceLastColliderUpdate` never reset |
| M-core6 | open | `ControllerInputProvider.cs:116-120` | `?? 0.5f` ×5 vs 0.2 thresholds |
| M-core8 | open | `Core/Config.cs:203,208` | scene MonoBehaviour written to `[SerializeField]` SO field |
| M-soc1 | open | `Socketable.cs` (no OnDestroy); `TransformArrayMultiSocket.cs:93`; `GridMultiSocket.cs:129`; `ShiftingRackSocket.cs:355,374` | occupancy never decremented on destroy |
| M-soc5 | open | `GridMultiSocket.cs:174-177` | OnValidate rebuilds pivots at runtime |
| M-soc7 | open | `Socketable.cs:255-271` | `DebugKeyHandling` not editor-gated |
| M-soc8 | partial | `ShiftingRackSocket.cs:241…507` | flag removed, 8 logs remain |
| M-fb2 | open | `FeedbackSystem.cs:325-400` | `renderers[i].material.SetColor` — instances never destroyed |
| M-seq3 | open | `Actions/InsertionAction.cs:128,157-159` | NRE on unassigned; `CompleteStep()` never called |
| M-seq4 | open | `ReactiveVars/…/AbstractSequenceAction.cs:47` | no null guard on `step` |
| M-ed1 | open | `ConstrainedInteractableEditor.cs:19,48-49` | `FindProperty("_snapDistance")` — field doesn't exist |
| M-ed2 | open | `PoseConstraintEditor.cs:410` | `_config.HandData` with `_config` possibly null |
| M-ed5 | open | `SetupChoiceStep.cs:170-177`; `LayersInputStep.cs:88-94`; `InteractionSystemLoader.cs:254-266` | `index++` before read → reads 32, skips 6 |
| M-ed6 | open | `InteractionSystemLoader.cs:235,242-245,287-292` | wrong type name; bitmask into index; truncates input axes |
| M-ed7 | open | `SetupChoiceStep.cs:273-276,301`; `DependenciesCheckStep.cs:90,253` | assembly name passed to `FindForPackageName`; `Thread.Sleep` loop |
| M-ed9 | open | `ShababeekSetupWizard.cs:26,211,227`; `FeedbackDrawerBase.cs:32` | `Assets/Shababeek/…` paths |

---

## Suggested order

1. **B1–B6** — compile/import blockers; B1, B2, B4 and B6 are each a few lines.
2. **H1** (log spam) and **H7** (debug key) — cheap, and the first thing a reviewer or customer sees in the console.
3. **H2–H5** — ReactiveVars core correctness (null checks, asset persistence, serialized runtime state, subscription stacking). These generate support tickets.
4. **H6, H8–H10** — new interactables (FastenerTool, InventoryGrid, Bullet, ReturnToOrigin).
5. **H11 / June carry-overs** — at minimum June H1 (one token), H7, H8, C5, M-core6, M-int7 (public API rename — do it now, not after customers depend on `Constrainter`).
6. Packaging: ThirdPartyNotices, package.json fields, version bump, strip `Tests/` and stray files from the store build.
7. Tooltips/XML docs — batch job, `<inheritdoc/>` on operator groups first.

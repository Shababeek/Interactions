# Interaction Recording — Record and Replay a VR Session

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Interaction Recorder / Interaction Recording Player
> **Use For:** Testing without a headset, repeatable bug reproduction, demos and attract loops

---

## What It Does

The **Interaction Recorder** captures a play session: head and hand poses, finger curls, and button presses. It saves them as an **Interaction Recording** asset. The **Interaction Recording Player** replays that asset through the Camera Rig as if a player were wearing the headset, so objects are grabbed, levers pulled and buttons pressed exactly as they were recorded.

Because playback goes through the normal input path, everything downstream (interactors, interactables, feedback, sequences) behaves as it does with live input.

**Perfect for:**
- Iterating on a scene without putting the headset on each time
- Reproducing a bug the same way every run
- Showing an interaction running in a trailer or kiosk build

**Limits:** thumbstick movement is not recorded, so locomotion is not replayed. Poses are stored relative to the Camera Rig, so a recording replays correctly wherever the rig is placed, but the scene objects must be where they were when it was recorded.

---

## Recording

1. Add **Interaction Recorder** to the Camera Rig (or a child of it).
2. Enter Play mode.
3. In the recorder's Inspector press **Start Recording**, perform the interaction, then press **Stop Recording**.

The recording is saved as an asset in the output folder.

### Interaction Recorder — Inspector

| Field | What it does |
|---|---|
| **Camera Rig** | The rig whose head and hands are recorded. Found in parents if left empty. |
| **Sample Rate** | Pose and finger samples per second. Higher is smoother and larger. Default 30. |
| **Record On Start** | Begin recording as soon as Play starts. |
| **Output Folder** | Folder under Assets where recordings are saved. Default `Assets/Recordings`. |
| **Recording Name** | Base file name for saved recordings. |

Saving assets happens in the Editor.

---

## Playing Back

1. Add **Interaction Recording Player** to the Camera Rig.
2. Assign the **Recording**.
3. Add the head and hand tracking components (for example Tracked Pose Drivers) to **Tracking To Disable During Playback**, so live tracking does not fight the replay.
4. Enter Play mode and press **Play**, or tick **Play On Start**.

### Interaction Recording Player — Inspector

| Field | What it does |
|---|---|
| **Camera Rig** | The rig to drive. Found in parents if left empty. |
| **Recording** | The recording to replay. |
| **Loop** | Restart from the beginning at the end. Default on. |
| **Play On Start** | Begin replaying as soon as Play starts. |
| **Playback Speed** | Speed multiplier. 1 is real time. |
| **Tracking To Disable During Playback** | Components switched off while replaying and restored afterwards. |

Stopping playback restores live input.

---

## The Recording Asset

Selecting a recording shows its sample rate, sample count and duration. **Show Raw Data** expands the stored tracks. Recordings are ordinary assets and can be kept in version control alongside the scene they belong to.

---

## Scripting API

```csharp
recorder.StartRecording();
InteractionRecording clip = recorder.StopRecording();

player.Recording = clip;
player.Play();
player.Stop();
```

`PlaybackHandInputProvider` is the input provider the player installs for each hand during playback. You do not normally add it yourself.

---

## Troubleshooting

| Problem | Check |
|---|---|
| Hands jitter or snap back during playback | Live tracking is still running. Add the tracking components to **Tracking To Disable During Playback**. |
| Hands replay but nothing is grabbed | The scene has changed since the recording, or objects have moved. Re-record. |
| Recording is not saved | Recording assets are written in the Editor, into **Output Folder**. |

---

## Related Documentation

- [Input providers and locomotion](../Core/InputAndLocomotion.md) — How hand input reaches the interactors
- [Interaction Core manual](../Core/InteractionCoreUserManual.md) — Camera Rig and hands

---

**Last Updated:** November 2026

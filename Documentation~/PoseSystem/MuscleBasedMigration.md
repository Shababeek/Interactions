# Muscle-Based Hand Pose (Editor Migration)

Use this when moving a **Hand Data** asset from the legacy bone-based pose pipeline to **Muscle Based** (humanoid muscle sampling).

---

## In the Editor

1. Select your **Hand Data** asset (`Create → Shababeek → … → Hand Data`, or use the asset referenced from **Config**).
2. Open the Inspector and set **Pose System** to **Muscle Based**.
3. Ensure each **hand prefab** used by that asset has a **Humanoid** Animator avatar (not Generic-only).
4. For each pose clip referenced on that Hand Data, confirm **Rig → Animation Type = Humanoid** where clips drive poses.

![Hand Data Inspector — Pose System set to Muscle Based](../Images/Pose_Constrainer/hand_data_pose_system_toggle.png)

---

## Play Mode checks

- Both hands appear with rest pose and respond to grab/use.
- Watch the Console for errors from **HandPoseController** / humanoid sampling after changing clips.

---

## More detail

Authoring notes for clips, limits, and hierarchy expectations live alongside animation code; if you need a full technical migration checklist, keep it in sync with `HandData` and **Muscle Based** types under `Scripts/…/Animations/MuscleBased/`.

---

**Last Updated:** May 2026

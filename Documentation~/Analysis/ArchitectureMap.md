# Architecture Map (Editor-Oriented)

This page summarizes how the Interaction System is wired **from the Unity Editor** and how pieces talk at runtime.

---

## What you set up first

1. **Create a Config asset** — `Assets` context menu → **Create → Shababeek → Interactions → Config** (see [Config](../Core/config.md)).
2. **Initialize the scene** — Hierarchy context menu → **Shababeek → Initialize Scene** to add **Camera Rig**, hands, and interactors (see [Quick Start](../GettingStarted/QuickStart.md)).
3. **Assign Config on Camera Rig** if it is not auto-linked.

![Suggested screenshot: hierarchy with Camera Rig and Config assignment](../Images/General/architecture_config_and_camera_rig.png)

---

## Runtime flow (mental model)

```
Config + Camera Rig
       → Hand (per side) + input
       → Interactor (Trigger / Raycast)
       → Interactable (Grabable, constrained types, Switch, VRButton, …)
       → Optional: FeedbackSystem, Sockets, PoseConstrainer
       → Optional: Interaction Drivers → ReactiveVars variables / events
```

For deeper Inspector detail on rigs, hands, and input, see **[Interaction Core manual](../Core/InteractionCoreUserManual.md)**.

---

## Extension points

| Goal | Where to start |
|------|----------------|
| New grab/manipulation type | Subclass **InteractableBase** |
| New detection mode | Subclass **InteractorBase** |
| Custom socket rules | Subclass **AbstractSocket** |
| Data / UI binding | **Interaction Drivers** + ReactiveVars binders ([Drivers](../ScriptableSystem/Drivers.md)) |

---

**Last Updated:** May 2026

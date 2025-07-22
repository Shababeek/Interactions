# Shababeek Interaction System — Component Reference

This document lists all core components (MonoBehaviours, ScriptableObjects, and key classes) in the system. Each entry will link to a detailed manual for that component.

## Core Components Index

- [Config](Config.md): Central configuration ScriptableObject for the system.
- [Hand](Hand.md): Represents a VR hand, manages input, pose, and visuals.
- [InteractorBase](InteractorBase.md): Base class for all interactors (hands, raycasters, triggers).
- [InteractableBase](InteractableBase.md): Base class for all interactable objects.
- [FeedbackSystem](FeedbackSystem.md): Unified feedback for haptics, audio, and visuals.
- [ScriptableVariable<T>](ScriptableVariable.md): Observable, serializable variable.
- [GameEvent<T>](GameEvent.md): Observable, serializable event.
- [GameEventListener](GameEventListener.md): Listens and responds to GameEvents.
- [VariableToUIBinder](VariableToUIBinder.md): Binds ScriptableVariables to UI.
- [HandData](HandData.md): ScriptableObject for hand pose and animation data.
- [HandPoseController](HandPoseController.md): Controls hand pose blending and animation.
- [InputManagerBase](InputManagerBase.md): Abstracts input sources.
- [VariableTweener](VariableTweener.md): Manages and updates tweenable objects.
- [Sequence, SequenceNode, Step, StepEventListener, MultiStepListener](Sequencing.md): Sequencing system for step-based actions.
- [Throwable, Grabable, Switch, TurretInteractable, etc.](Interactables.md): Example interactables.

For each component, see its dedicated file for detailed documentation. 
# InteractorBase — Manual

## Purpose
InteractorBase is the base class for all interactors in the system (e.g., hands, raycasters, triggers). It manages interaction states, the current interactable, and provides an attachment point for held objects.

## Usage
- Inherit from InteractorBase to create custom interactors.
- Add to a GameObject (usually a hand) to enable interaction with interactables.

## Inspector Properties
- **currentInteractable** (InteractableBase, ReadOnly): The interactable currently being interacted with.
- **isInteracting** (bool, ReadOnly): Whether the interactor is currently interacting with something.

## Key Members
- `Transform AttachmentPoint`: The attachment point for held objects.
- `HandIdentifier HandIdentifier`: The hand identifier (left or right) for this interactor.
- `Hand Hand`: The Hand component associated with this interactor.
- `void ToggleHandModel(bool enable)`: Show or hide the hand model.

## Example Setup
1. Add an InteractorBase-derived component (e.g., TriggerInteractor, RaycastInteractor) to your hand GameObject.
2. The interactor will automatically manage interaction states and attachment points.
3. Use the AttachmentPoint to parent grabbed objects. 
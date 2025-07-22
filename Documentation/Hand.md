# Hand — Manual

## Purpose
The Hand MonoBehaviour represents a VR hand in the interaction system. It manages input, pose constraints, and visual representation, serving as the central hub for hand-related functionality.

## Usage
- Add the Hand component to a GameObject representing a hand in your scene.
- Assign the required properties in the Inspector (hand identifier, config, hand model).
- Used by interactors and interactables to determine hand state and input.

[screenshot of Hand component Inspector with hand, config, and handModel fields]

## Inspector Properties
- **hand** (HandIdentifier): Identifies whether this hand is the left or right hand in the VR system. ("Identifies whether this hand is the left or right hand in the VR system.")
- **config** (Config): Reference to the global configuration asset containing input manager and system settings.
- **handModel** (GameObject): The hand model GameObject that will be shown or hidden based on interaction state.

## Key Members
- `HandIdentifier HandIdentifier`: Gets or sets the hand identifier (Left or Right).
- `Config Config`: Reference to the configuration asset.
- `GameObject HandModel`: The hand model for this hand.

## Example Setup
1. Add a Hand component to your left and right hand GameObjects.
2. Assign the `hand` property to Left or Right as appropriate.
3. Assign the shared Config asset to `config`.
4. Assign a hand model prefab to `handModel`.

[screenshot of two hand GameObjects in the hierarchy, each with a Hand component] 
# Config — Manual

## Purpose
The Config ScriptableObject holds all configuration settings for the Shababeek Interaction System, including hand data, input, and layer assignments. It centralizes system-wide settings for easy management.

## Usage
- Create via the Unity menu: Create > Shababeek > Interactions > Config.
- Assign in the Inspector to set up hand data, layers, and input settings.
- Reference this asset in your scene to ensure all components use the same configuration.

[screenshot of Config asset Inspector with all properties visible]

## Inspector Properties
- **handData** (HandData): ScriptableObject containing hand pose and animation data. ("You need to create a HandData ScriptableObject for all hands you want to use in the interaction system.")
- **leftHandLayer** (LayerMask): Layer for the left hand, used for physics interactions. ("Needed to make sure the hand does not interact with itself.")
- **rightHandLayer** (LayerMask): Layer for the right hand, used for physics interactions. ("Needed to make sure the hand does not interact with itself.")
- **interactableLayer** (LayerMask): Layer for interactable objects, used for physics interactions.
- **playerLayer** (LayerMask): Layer for the player.
- **inputType** (InputManagerType): Type of input manager to use (e.g., InputSystem). ("Type of input manager to use for the interaction system. Options are Unity Axis")
- **leftHandActions** (HandInputActions): Input actions for the left hand.
- **rightHandActions** (HandInputActions): Input actions for the right hand.
- **feedbackSystemStyleSheet** (StyleSheet): Editor UI style sheet for feedback system.
- **handMass** (float): Mass of the hand for physics simulation.
- **linearDamping** (float): Linear damping for hand physics.
- **angularDamping** (float): Angular damping for hand physics.

## Example Setup
1. Create a Config asset in your project.
2. Assign a HandData asset to `handData`.
3. Set the appropriate layers for hands, interactables, and player.
4. Choose the input manager type and assign input actions.
5. Adjust hand mass and damping as needed for your experience.

[screenshot of assigning HandData and layers in the Config Inspector] 
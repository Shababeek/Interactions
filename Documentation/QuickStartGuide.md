# Quick Start Guide

## Overview

This guide will walk you through setting up a basic VR interaction scene with the Shababeek Interaction System. You'll learn how to initialize the scene, configure hands, create interactable objects, set up feedback, and create sequences.

## Prerequisites

- Unity 6.0 LTS or newer
- OpenXR package installed and configured
- VR headset and controllers (for testing)

## Step 1: Initialize the Scene

### Adding the Camera Rig

1. **Open Unity** and create a new 3D scene
2. **Initialize the scene** with Shababeek components:
   - Go to `Shababeek > Initialize Scene` in the top menu
   - This automatically adds the necessary camera rig and basic setup
   - The camera rig includes XR Origin, XR Interaction Manager, and basic VR setup
[insert screenshot of Unity menu bar showing Shababeek menu]
### What Gets Added

The initialization adds:
- **Camera Rig** - A prefab containing the XR Origin and basic VR setup
- This includes the main camera, XR Origin, and basic VR components needed for interaction

## Step 2: Configure Hand Data

### Setting Up Hand Models

1. **Open the Config asset**:
   - Find `Assets/Shababeek/Interactions/Data/config.asset`
   - Double-click to open it in the Inspector
   - choose a differnt hand by clickong on HandData and selecting another hand

2. **Configure Hand Data**:
   - In the **Hand Data** section, you'll see options for different hand models
   - **Left Hand Prefab** and **Right Hand Prefab** - Assign your preferred hand models
   - **Default Pose** - Set the default hand pose
   - **Poses** - Add custom poses for different interactions

3. **Available Hand Models**:
   - **Biker Hands** - Realistic hands with tattoos
   - **Cartoon Gloves** - Stylized cartoon hands
   - **Robot Clown** - Mechanical/robotic hands
   - **Sci-fi Hands** - Futuristic hands
   - **Viking Hands** - Fantasy-style hands

### Hand Pose Configuration

1. **Create Custom Poses**:
   - In the Hand Data, expand the **Poses** section
   - Click **Add Pose** to create a new pose
   - Set the **Name** (e.g., "Grab", "Point", "ThumbsUp")
   - Assign **Open Animation Clip** and **Closed Animation Clip**

2. **Configure Finger Constraints**:
   - Each pose can have finger-specific constraints
   - Set **Min** and **Max** values for finger movement
   - Use **Locked** to completely restrict finger movement

## Step 3: Creating Interactable Objects

### Making a Grabable Object

1. **Create the object**:
   - Right-click in the Hierarchy
   - Select `Create > Shababeek > Interactables > Grabable`
   - This creates a GameObject with all necessary components

2. **What components are added**:
   - **Grabable** - Main interaction component
   - **InteractableBase** - Base interaction functionality
   - **Rigidbody** - For physics-based grabbing
   - **Collider** - For interaction detection
   - **FeedbackSystem** - For haptic/audio feedback

3. **Configure the Grabable**:
   - **Interaction Hand** - Set to Left, Right, or Both
   - **Selection Button** - Choose which button triggers selection
   - **Grab Point** - Set the position where the object is grabbed
   - **Throw Force** - Adjust how strongly the object is thrown

### Making a Lever

1. **Create the lever**:
   - Right-click in the Hierarchy
   - Select `Create > Shababeek > Interactables > Lever Interactable`

2. **What components are added**:
   - **LeverInteractable** - Lever-specific interaction logic
   - **InteractableBase** - Base interaction functionality
   - **HingeJoint** - For lever rotation
   - **Rigidbody** - For physics
   - **Collider** - For interaction detection

3. **Configure the Lever**:
   - **Lever Axis** - Set the rotation axis (X, Y, or Z)
   - **Min Angle** and **Max Angle** - Set rotation limits
   - **Current Value** - Read-only, shows current lever position
   - **On Lever Changed** - Event triggered when lever moves

### Making a Drawer

1. **Create the drawer**:
   - Right-click in the Hierarchy
   - Select `Create > Shababeek > Interactables > Drawer Interactable`

2. **What components are added**:
   - **DrawerInteractable** - Drawer-specific interaction logic
   - **InteractableBase** - Base interaction functionality
   - **ConfigurableJoint** - For sliding movement
   - **Rigidbody** - For physics
   - **Collider** - For interaction detection

3. **Configure the Drawer**:
   - **Drawer Axis** - Set the sliding direction (X, Y, or Z)
   - **Min Distance** and **Max Distance** - Set sliding limits
   - **Current Value** - Read-only, shows current drawer position
   - **On Value Changed** - Event triggered when drawer moves

### Making a Button

1. **Create the button**:
   - Right-click in the Hierarchy
   - Select `Create > Shababeek > Interactables > VR Button`

2. **What components are added**:
   - **VRButton** - Button-specific interaction logic
   - **InteractableBase** - Base interaction functionality
   - **Rigidbody** - For physics
   - **Collider** - For interaction detection

3. **Configure the Button**:
   - **Press Distance** - How far the button can be pressed
   - **Press Force** - How much force is required to press
   - **On Pressed** - Event triggered when button is pressed
   - **On Released** - Event triggered when button is released

## Step 4: Setting Up Feedback

### Adding Haptic Feedback

1. **Select your interactable object**
2. **In the Inspector**, find the **FeedbackSystem** component
3. **Add HapticFeedback**:
   - Click the **+** button in the Feedbacks list
   - Select **HapticFeedback** from the dropdown
   - Configure the haptic settings:
     - **Hover Amplitude**: 0.3 (subtle hover feedback)
     - **Select Amplitude**: 0.5 (medium selection feedback)
     - **Activate Amplitude**: 1.0 (strong activation feedback)

### Adding Audio Feedback

1. **Add AudioFeedback** to the same FeedbackSystem
2. **Configure audio clips**:
   - **Hover Clip** - Sound when hovering over the object
   - **Select Clip** - Sound when selecting the object
   - **Activate Clip** - Sound when activating the object
3. **Set volumes** for each interaction type
4. **Enable Spatial Audio** for 3D positioning

### Adding Visual Feedback

1. **Add MaterialFeedback** to the FeedbackSystem
2. **Configure colors**:
   - **Hover Color** - Yellow (for hover highlight)
   - **Select Color** - Green (for selection highlight)
   - **Activate Color** - Red (for activation highlight)
3. **Set Color Property Name** to "_Color" (or your material's color property)

## Step 5: Using Unity Events

### Setting Up Event Responses

1. **Select your interactable object**
2. **In the Inspector**, find the **InteractableBase** component
3. **Configure events**:
   - **On Selected** - Triggered when object is selected
   - **On Deselected** - Triggered when object is deselected
   - **On Hover Start** - Triggered when hovering begins
   - **On Hover End** - Triggered when hovering ends
   - **On Activated** - Triggered when object is activated

### Adding Event Actions

1. **Click the + button** in any event field
2. **Add common actions**:
   - **Set Active** - Show/hide GameObjects
   - **Set Position** - Move objects
   - **Set Rotation** - Rotate objects
   - **Set Scale** - Scale objects
   - **Play Animation** - Trigger animations
   - **Play Audio** - Play sound effects

### Example: Making a Light Switch

1. **Create a VR Button** for the switch
2. **Add a Light** component to a GameObject
3. **In the button's On Activated event**:
   - Click **+** to add an event
   - Select the Light GameObject
   - Choose **Light > Enabled**
   - Set it to **True** to turn on the light
4. **In the button's On Deactivated event**:
   - Add another event
   - Set the Light's **Enabled** to **False** to turn off the light

## Step 6: Creating Sequences

### Setting Up a Basic Sequence

1. **Create a Sequence**:
   - Right-click in the Project window
   - Select `Create > Shababeek > Sequencing > Sequence`

2. **Add Steps** to the sequence:
   - Click **Add Step** in the sequence
   - Configure each step:
     - **Audio Clip** - Voice over or sound effect
     - **Audio Delay** - Delay before playing audio
     - **Audio Only** - Complete step when audio finishes
     - **On Started** - Events when step begins
     - **On Completed** - Events when step ends

### Adding Voice Over

1. **Import your audio files** into the project
2. **In each step**, assign the **Audio Clip**
3. **Configure audio settings**:
   - **Audio Delay** - Wait time before playing
   - **Audio Only** - Enable to auto-complete when audio finishes
   - **Pitch** - Adjust playback speed if needed

### Using Sequence Actions

1. **Add actions** to your sequence steps:
   - **VariableAction** - Set/check scriptable variables
   - **SequenceOrderAction** - Track interaction order
   - **ActivatingAction** - Activate other objects

2. **VariableAction example**:
   - Add VariableAction to a step
   - Set **Operation** to "Set"
   - Assign a **ScriptableVariable** (BoolVariable, IntVariable, etc.)
   - Set the **Value** to change the variable

3. **SequenceOrderAction example**:
   - Add SequenceOrderAction to a step
   - Configure **Ordered Interactions**
   - Set the order of interactions required
   - Enable **Reset On Wrong Order** for strict validation

### Running the Sequence

1. **Add SequenceBehaviour** to a GameObject in the scene
2. **Assign your Sequence** to the **Sequence** field
3. **Configure startup**:
   - **Star On Awake** - Start automatically
   - **Delay** - Wait time before starting
4. **Add Unity Events**:
   - **On Sequence Started** - Events when sequence begins
   - **On Sequence Completed** - Events when sequence ends

## Step 7: Advanced Configuration

### Pose Constraints

1. **Select an interactable object**
2. **Add PoseConstrains** component
3. **Configure constraints**:
   - **Constraint Type** - HideHand, FreeHand, or Constrained
   - **Target Pose Index** - Which pose to use
   - **Finger Constraints** - Individual finger settings
   - **Hand Positioning** - Position and rotation offsets

### Hand Positioning

1. **In PoseConstrains**, expand **Hand Positioning**
2. **Set Position Offset** - Where the hand should be positioned
3. **Set Rotation Offset** - How the hand should be rotated
4. **Use Smooth Transitions** - Enable for smooth hand movement

### Creating Custom Interactables

1. **Create a new script** that inherits from `InteractableBase`
2. **Override interaction methods**:
   ```csharp
   public override void OnSelected(InteractorBase interactor)
   {
       // Custom selection logic
   }
   ```
3. **Add the component** to your GameObject
4. **Configure in the Inspector** like other interactables

## Step 8: Testing and Debugging

### Testing in VR

1. **Enter Play Mode** in Unity
2. **Put on your VR headset**
3. **Test interactions**:
   - Hover over objects to see feedback
   - Select objects with your controller
   - Activate objects with the trigger button
   - Test sequences and voice over

### Common Issues and Solutions

**Hands not appearing**:
- Check Hand Data configuration in config.asset
- Verify hand prefabs are assigned
- Ensure XR Origin is properly set up

**Interactions not working**:
- Check that objects have InteractableBase components
- Verify Interaction Layer settings
- Ensure controllers are properly configured

**Audio not playing**:
- Check AudioSource assignments
- Verify audio clips are assigned
- Test volume and pitch settings

**Sequences not starting**:
- Check SequenceBehaviour configuration
- Verify sequence asset is assigned
- Test Star On Awake and Delay settings

## Next Steps

Now that you have a basic understanding, explore these advanced features:

- **Branching Sequences** - Create complex interaction flows
- **Scriptable Variables** - Create shared state across scenes
- **Custom Feedback Types** - Create your own feedback systems
- **Advanced Hand Posing** - Create complex hand animations
- **Performance Optimization** - Optimize for VR performance

## Tips for Best Practices

1. **Use descriptive names** for all objects and components
2. **Test frequently** in VR to ensure good user experience
3. **Optimize audio files** for VR (compressed formats)
4. **Use appropriate haptic feedback** - not too strong or weak
5. **Design for accessibility** - consider different user needs
6. **Document your sequences** - keep track of complex flows
7. **Reuse components** - create prefabs for common interactions 
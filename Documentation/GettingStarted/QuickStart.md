# Getting Started with Shababeek Interactions — 10-Minute Quick Start

Welcome to the Shababeek Interaction System! This guide will get you from zero to grabbing your first VR object in about 10 minutes.

---

## What You'll Learn

By the end of this guide, you'll have:
- ✅ A VR scene set up with hands
- ✅ An object you can pick up and throw
- ✅ Sound effects when grabbing
- ✅ Understanding of the core concepts

**Time Required:** ~10 minutes  
**Prerequisites:** Unity 2021.3 or newer, VR headset (Quest, PCVR, etc.)

---

## Step 1: Install the Package (2 minutes)

### Option A: Unity Package Manager (Recommended)
1. Open your Unity project
2. **Window > Package Manager**
3. Click **+** > **Add package from git URL**
4. Enter: `https://github.com/YourRepo/Shababeek-Interactions.git`
5. Click **Add**
![AddFromPackageManager.png](../Images/AddFromPackageManager.png)
### Option B: Download and Import
1. Download the `.unitypackage` from [releases page](https://github.com/Shababeek/Interactions/releases)
2. **Assets > Import Package > Custom Package**
3. Select the downloaded file
4. Click **Import**

### Verify Installation
Check that you see `Shababeek` folder in your Project window:
```
Assets/
└── Shababeek/
    ├── Interactions/
    ├── Core/
    └── Utilities/
```


## Step 2: Create the Config Asset (1 minute), or use the one provided in the asset

The Config asset is the brain of the interaction system. You only create this once per project.

### Use the wizard to create it 
1. Context menu: **Shababeek > Setup wizard**
2. follow the steps on screen
### Create it manually
1. **Right click**-> Shababeek->Inteataction System Config 
![CreateConfig.png](../Images/CreateConfig.png)

### Basic Setup
The Config comes with good defaults, but verify these settings:

#### Essential Settings (in Inspector)
- **Interaction Layers:** Set to a dedicated layers (create them if needed)
- **Hand Data:** Assign the default HandData asset from the list
![ConfigFile.png](../Images/ConfigFile.png)

> 💡 **Tip:** use default settings from the setup wizard and then edit it.

✅ **Checkpoint:** Config asset created and set up!

---

## Step 3: Set Up Your VR Scene (3 minutes)

### Add XR Origin
If you don't already have a VR setup:

1. `GameObject > Shababeek > Initialize scene` or `context menu->Shababeek-> Initialize scene` 
   - This Deletes the main camera 
   - creates Camera Rig object with some other object inside


### configure CameraRig
1. if the config file is not added automatically, drag it to the camera Rig
![CameraRig.png](../Images/CameraRig.png)


   
✅ **Checkpoint:** VR hands are set up and ready to interact!( press play and see it in action)

---

## Step 4: Make Your First Grabbable Object (2 minutes)

Time to create something you can pick up!

### Create a Simple Cube
1. **GameObject > 3D Object > Cube**
2. Name it "Grabbable Cube"
3. Position it in front of the player (use Scene view)


### Make It Grabbable
1. Right-click on your Cube: `Shababeek-> Make into-> Grabable`
2. That's it! This will add:
    - a PoseConstrainer component(for hand positioning)
    - a Grabable component 
![MakeGrababel.png](../Images/MakeGrababel.png)
![Grabable.png](../Images/Grabable.png)
### Set the Layer
**optional:** Your object needs to be on the Interactable layer!

1. At the top of Inspector, click **Layer** dropdown
2. Select **Interactable** (or whatever you set in Config)


✅ **Checkpoint:** You have a grabbable cube!

---

## Step 5: Test It! (1 minute)

Time to try it in VR!

### Press Play
1. Put on your VR headset
2. Click the **Play** button in Unity
3. Reach out toward the cube
4. Press the **Grip** button (side button on your controller)
5. The cube attaches to your hand!
6. Move your hand around - the cube follows
7. Release Grip - the cube drops

## Step 7 configure the Hand Pose


### Troubleshooting Quick Fixes
**Can't see hands?**
- Check Hand component has HandData assigned
- Verify hand prefabs exist in HandData asset

**Can't grab cube?**
- Make sure the selected Input method is enabled in player settings
- Change the Input method in Config file
- Make sure cube has a Collider


✅ **Checkpoint:** You successfully grabbed an object in VR!

---

## Step 6: Add Grab Sound (2 minutes)

Make it feel more real with audio feedback!

### Prepare Audio
1. Import a sound effect (or use Unity's built-in sound)
2. For this example, we'll use a simple "click" sound

### Add Audio Source
1. Select your Grabbable Cube
2. **Add Component > Audio Source**
3. **Un check** "Play On Awake"
4. Drag your sound file into **AudioClip** field

### Wire Up the Event
1. On the **Grabable** component, find the **Events** section
2. Expand **On Selected**
3. Click the **+** button
4. Drag your **Grabbable Cube** into the **Object** field
5. Click the function dropdown: **AudioSource > Play()**


### Test It Again!
Press Play, put on headset, grab the cube - you should hear your sound!


✅ **Checkpoint:** Your object now has audio feedback!

---

## 🎉 Congratulations!

You've created your first VR interaction in under 10 minutes! Here's what you accomplished:

- ✅ Installed Shababeek Interactions
- ✅ Set up VR hands with interactors
- ✅ Created a grabbable physics object
- ✅ Added audio feedback
- ✅ Tested in VR

---

## Next Steps: Learn More

Now that you have the basics, explore these topics:

### Beginner
- 📚 [Grabable](../Components/Grabable.md) - Deep dive into grabbable objects
- 📚 [Throwable](../Components/Throwable.md) - Simulating throw physics
- 📚 [Complex interactables](../Components/constrainedInteractables.md) - Joysticks, levers, drawers,...
- 📚 [Buttons](../Components/Switch.md) - Learn about switches and buttons
- 📚 [Switches](../Components/Switch.md) - Learn about switches and buttons
- 📚 [Events System](../Core/Events.md) - Make things happen when you interact

### Intermediate
- 📚 [Hand Poses](../HandPoses/README.md) - Custom hand grips for different objects
- 📚 [Feedback System](../Feedback/README.md) - Haptics, visuals, and audio
- 📚 [Sockets System](../Sockets/Readme.md)
### Advanced
- 📚 [Sequencing System](../Sequencing/README.md) - Create step-by-step tutorials
- 📚 [Custom Interactables](../Advanced/CustomInteractables.md) - Build your own components
- 📚 [Scriptable Variables](../Core/ScriptableVariables.md) - Data flow and game state

---

## Common Next Tasks

### I Want To...

**...make a light switch**
→ Use the [Switch Component](../Components/Switch.md) 

**...make objects snap into place**
→ Use [Sockets System](../Sockets/Readme.md)

**...show UI when hovering over objects**
→ Use the **On Hover Start** event on InteractableBase

**...make a flashlight that turns on when held**
→ Follow the [Flashlight Tutorial](../Tutorials/Flashlight.md)

**...create a puzzle where objects must be placed in order**
→ Learn about the [Sequencing System](../Sequencing/README.md)

---

## 🆘 Need Help?

### Quick Troubleshooting

**Nothing works at all**
1. Check Console for errors
2. Verify Config asset is assigned to Hand components
3. Make sure InteractionLayers match in Config and on your objects

**Hands don't show up**
- HandData must have hand prefabs assigned
- Check Hand component's "Hand Model" field

**Can see hands but can't grab**
- Object must be on Interactable layer
- Object must have a Collider (not set to Trigger)
- Controllers must have TriggerInteractor components

**Objects fly away when grabbed**
- Lower the Mass on the Rigidbody
- Check for collisions with other objects

### Get Support
- **Documentation:** [Full Component Reference](../ComponentReference.md)
- **Examples:** Check `Shababeek/Interactions/Examples/` scenes
- **Discord:** [Join our community](link)
- **GitHub Issues:** [Report bugs](link)
- **Email:** support@shababeek.com

---

## Tips for Your First Project

💡 **Start Simple**
Don't try to build everything at once. Master one interaction type, then add more.

💡 **Test in VR Frequently**
What looks good in the Scene view can feel different in VR. Test often!

💡 **Use Prefabs**
Once you have a working grabbable object, make it a prefab to reuse.

💡 **Organize Your Layers**
Create clear layer names (Interactable, UI, Terrain) and use them consistently.

💡 **Watch the Examples**
The example scenes in `Shababeek/Interactions/Examples/` show best practices.

---

## What You Learned

| Concept | What It Does |
|---------|-------------|
| **Config Asset** | Central settings for the entire interaction system |
| **Hand Component** | Represents a VR hand with visual model and interactions |
| **Interactors** | Components that detect and interact with objects (TriggerInteractor, RaycastInteractor) |
| **Grabable** | Makes objects pickable and throwable |
| **Events** | Trigger actions when interactions happen (On Selected, On Hover, etc.) |
| **Layers** | Control what can interact with what |

---

## Ready for More?

You're now ready to explore the full power of Shababeek Interactions!

**Next Recommended Reading:**
1. [Component Overview](../ComponentOverview.md) - See all available components
2. [Interactables Deep Dive](../Components/Interactables.md) - Master Interactable objects
3. [Core Concepts](../CoreConcepts.md) - Understand the architecture

**Or Jump to a Tutorial:**
- [Making a VR Flashlight](../Tutorials/Flashlight.md)
- [Creating a Physics Puzzle](../Tutorials/PhysicsPuzzle.md)
- [Building an Inventory System](../Tutorials/Inventory.md)

---

Happy developing! 🚀

**Last Updated:** 25th October 2025
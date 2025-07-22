# Hand Data & Pose System — Manual

## How to Import and Set Up a New Hand

### 1. Import the 3D Hand Model
- Import your hand 3D model (FBX or similar) into the Unity project.
- Ensure the model’s rig is set to “Humanoid” or “Generic” as appropriate.
- Check that the bone hierarchy matches your intended finger structure.
[screenshot of imported hand model in the Project window]

### 2. Create Single-Frame Animation Clips for Each Pose
- In your 3D modeling or animation software (e.g., Blender, Maya), or in Unity directly pose the hand in the required positions:
  - **Fully Open** (required)
  - **Fist** (required)
  - (Optional) Additional poses: pointing, pinching, etc.
- Export each pose as a single-frame animation clip (FBX or compatible format), or create the poses using Unity's Animation system instead.
- Import these animation clips into Unity.
[screenshot of imported single-frame animation clips in the Project window]

### 3. Create Avatar Masks for Each Finger
- For each finger, create an Avatar Mask (right-click in Project > Create > Avatar Mask).
- In each mask, enable only the bones for the corresponding finger.
- Name each mask clearly (e.g., "ThumbMask", "IndexMask").
[screenshot of Avatar Mask Inspector for a finger]

### 4. Cofiguring the Poses 
- Drag the animations for Fist and Fully Opern into the HandDataAsset
- For each imported single-frame animation(except the Fist and fully Open), create a PoseData asset.
- Assign the animation clip to the PoseData asset.
- Create PoseData assets for at least the fully open and fist poses (required), and optionally for any additional poses.

[screenshot The Poses Part in the editor]

### 5. Create the HandData Asset and Assign Poses/Masks/Prefabs
- Create a HandData asset (Create > Shababeek > Interaction System > Hand Data).
- In the HandData Inspector, assign your PoseData assets and Avatar Masks to the appropriate fields.
[screenshot of HandData Inspector with poses and masks assigned]

### 6. Create the Hand Prefabs and Link Everything
- Drag the imported hand model into the scene and add any required components (e.g., colliders, PoseConstrainer).
- Create a prefab from the configured hand object for both left and right hands.
- In each hand prefab, set the PoseConstrainer component to reference the HandData asset.
- In the HandData asset, assign the left and right hand prefabs to their respective fields.
[screenshot of hand prefab Inspector with PoseConstrainer referencing HandData]
[screenshot of HandData asset with left/right prefabs assigned]

### 7. Assign HandData in the Config or Hand Component
- In your Config asset or directly on the Hand component, assign the HandData asset.
- This ensures the system uses your new hand setup for pose blending and constraints.
[screenshot of Config or Hand Inspector with HandData assigned]

### 8. Test and Refine
- Placve the Prefab in the scene
- Choose the desired Pose
- For Dynamic Poses, Move the slider for each finger betweeen 1 and 0
- Tweak pose data, avatar masks, or prefab setup as needed for natural, immersive hand presence.
[screenshot of hand in-game with blended pose applied] 
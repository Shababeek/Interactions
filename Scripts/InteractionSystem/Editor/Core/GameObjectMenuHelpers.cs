using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Shababeek.Interactions.Editors
{
    public class GameObjectMenuHelpers : Editor
    {
        // Constants for consistent sizing
        // Lever - A handle that rotates around a pivot point
        private const float LeverBaseRadius = 0.15f;
        private const float LeverBaseHeight = 0.05f;
        private const float LeverARMWidth = 0.05f;
        private const float LeverARMHeight = 0.05f;
        private const float LeverARMLength = 0.4f;
        private const float LeverHandleRadius = 0.08f;
        private const float LeverHandleHeight = 0.15f;
        
        // Drawer - A box that slides in and out
        private const float DrawerFrameWidth = 0.6f;
        private const float DrawerFrameHeight = 0.3f;
        private const float DrawerFrameDepth = 0.5f;
        private const float DrawerFrameThickness = 0.02f;
        private const float DrawerBoxWidth = 0.56f;
        private const float DrawerBoxHeight = 0.26f;
        private const float DrawerBoxDepth = 0.48f;
        private const float DrawerHandleWidth = 0.15f;
        private const float DrawerHandleHeight = 0.05f;
        private const float DrawerHandleDepth = 0.05f;
        
        private const float JoystickBaseRadius = 0.3f;
        private const float JoystickBaseHeight = 0.1f;
        private const float JoystickBodyHeight = 0.15f;
        private const float JoystickBarrelRadius = 0.05f;
        private const float JoystickBarrelLength = 0.4f;
        
        private const float ButtonScale = 0.5f;
        private const float ButtonHeight = 0.25f;
        private const float ButtonYOffset = 0.5f;
        private const float ButtonTriggerY = 0.2f;
        private const float ButtonBodyScale = 0.2f;
        
        // Switch - A toggle switch with a flat base and small lever
        private const float SwitchPanelWidth = 0.2f;
        private const float SwitchPanelHeight = 0.02f;
        private const float SwitchPanelDepth = 0.15f;
        private const float SwitchToggleWidth = 0.08f;
        private const float SwitchToggleHeight = 0.03f;
        private const float SwitchToggleDepth = 0.04f;
        private const float SwitchToggleY = 0.025f;

        // ========== CREATE MENU ==========
        
        [MenuItem("GameObject/Shababeek/Create/Lever", priority = 2)]
        public static void CreateLever()
        {
            GameObject leverObject = new GameObject("Lever");
            
            // Create static base
            var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            baseObj.name = "Base";
            baseObj.localScale = new Vector3(LeverBaseRadius, LeverBaseHeight, LeverBaseRadius);
            baseObj.localPosition = Vector3.zero;
            baseObj.localRotation = Quaternion.Euler(90, 0, 0);
            baseObj.parent = leverObject.transform;
            
            // Create moving parts (arm and handle) - oriented vertically
            GameObject movingParts = new GameObject("MovingParts");
            movingParts.transform.parent = leverObject.transform;
            movingParts.transform.localPosition = new Vector3(0, LeverBaseHeight, 0);
            
            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            arm.name = "Arm";
            arm.localScale = new Vector3(LeverARMWidth, LeverARMLength, LeverARMHeight);
            arm.localPosition = new Vector3(0, LeverARMLength / 2, 0);
            arm.parent = movingParts.transform;
            
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            handle.name = "Handle";
            handle.localScale = new Vector3(LeverHandleRadius, LeverHandleHeight, LeverHandleRadius);
            handle.localPosition = new Vector3(0, LeverARMLength, 0);
            handle.parent = movingParts.transform;
            
            // Initialize the constrained interactable with only the moving parts
            var leverTransform = leverObject.transform;
            InitializeConstrainedInteractable<LeverInteractable>(leverTransform, movingParts);
            
            Selection.activeGameObject = leverObject;
        }

        [MenuItem("GameObject/Shababeek/Create/Lever", true)]
        private static bool ValidateCreateLever()
        {
            return ValidateRequiredComponents();
        }

        [MenuItem("GameObject/Shababeek/Create/Drawer", priority = 3)]
        public static void CreateDrawer()
        {
            GameObject drawerObject = new GameObject("Drawer");
            
            // Create static frame
            var frame = new GameObject("Frame").transform;
            frame.parent = drawerObject.transform;
            frame.localPosition = Vector3.zero;
            
            var frameBottom = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            frameBottom.name = "Bottom";
            frameBottom.localScale = new Vector3(DrawerFrameWidth, DrawerFrameThickness, DrawerFrameDepth);
            frameBottom.localPosition = Vector3.zero;
            frameBottom.parent = frame;
            
            var frameLeft = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            frameLeft.name = "Left";
            frameLeft.localScale = new Vector3(DrawerFrameThickness, DrawerFrameHeight, DrawerFrameDepth);
            frameLeft.localPosition = new Vector3(-DrawerFrameWidth / 2 + DrawerFrameThickness / 2, DrawerFrameHeight / 2, 0);
            frameLeft.parent = frame;
            
            var frameRight = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            frameRight.name = "Right";
            frameRight.localScale = new Vector3(DrawerFrameThickness, DrawerFrameHeight, DrawerFrameDepth);
            frameRight.localPosition = new Vector3(DrawerFrameWidth / 2 - DrawerFrameThickness / 2, DrawerFrameHeight / 2, 0);
            frameRight.parent = frame;
            
            var frameTop = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            frameTop.name = "Top";
            frameTop.localScale = new Vector3(DrawerFrameWidth, DrawerFrameThickness, DrawerFrameDepth);
            frameTop.localPosition = new Vector3(0, DrawerFrameHeight, 0);
            frameTop.parent = frame;
            
            // Create moving parts (drawer box and handle)
            GameObject movingParts = new GameObject("MovingParts");
            movingParts.transform.parent = drawerObject.transform;
            movingParts.transform.localPosition = new Vector3(0, DrawerFrameHeight / 2, 0);
            
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            box.name = "DrawerBox";
            box.localScale = new Vector3(DrawerBoxWidth, DrawerBoxHeight, DrawerBoxDepth);
            box.localPosition = Vector3.zero;
            box.parent = movingParts.transform;
            
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            handle.name = "Handle";
            handle.localScale = new Vector3(DrawerHandleWidth, DrawerHandleHeight, DrawerHandleDepth);
            handle.localPosition = new Vector3(0, 0, DrawerBoxDepth / 2 + DrawerHandleDepth / 2);
            handle.parent = box;
            
            // Initialize the constrained interactable with only the moving parts
            var drawerTransform = drawerObject.transform;
            InitializeConstrainedInteractable<DrawerInteractable>(drawerTransform, movingParts);
            
            Selection.activeGameObject = drawerObject;
        }

        [MenuItem("GameObject/Shababeek/Create/Drawer", true)]
        private static bool ValidateCreateDrawer()
        {
            return ValidateRequiredComponents();
        }

        [MenuItem("GameObject/Shababeek/Create/Joystick", priority = 4)]
        public static void CreateJoystick()
        {
            GameObject joystickObject = new GameObject("Joystick");
            
            // Create static base
            var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            baseObj.name = "Base";
            baseObj.localScale = new Vector3(JoystickBaseRadius, JoystickBaseHeight, JoystickBaseRadius);
            baseObj.localPosition = Vector3.zero;
            baseObj.parent = joystickObject.transform;
            
            // Create moving parts (stick and knob)
            GameObject movingParts = new GameObject("MovingParts");
            movingParts.transform.parent = joystickObject.transform;
            movingParts.transform.localPosition = Vector3.zero;
            
            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            stick.name = "Stick";
            stick.localScale = new Vector3(JoystickBarrelRadius, JoystickBarrelLength, JoystickBarrelRadius);
            stick.localPosition = new Vector3(0, JoystickBarrelLength, 0);
            stick.parent = movingParts.transform;
            
            var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            knob.name = "Knob";
            knob.localScale = Vector3.one * JoystickBodyHeight;
            knob.localPosition = new Vector3(0, JoystickBarrelLength * 2, 0);
            knob.parent = movingParts.transform;
            
            // Initialize the constrained interactable with only the moving parts
            var joystickTransform = joystickObject.transform;
            InitializeConstrainedInteractable<JoystickInteractable>(joystickTransform, movingParts);
            
            Selection.activeGameObject = joystickObject;
        }

        [MenuItem("GameObject/Shababeek/Create/Joystick", true)]
        private static bool ValidateCreateJoystick()
        {
            return ValidateRequiredComponents();
        }

        [MenuItem("GameObject/Shababeek/Create/Button", priority = 5)]
        public static void CreateButton()
        {
            GameObject buttonObject = CreateButtonGeometry();
            Selection.activeGameObject = buttonObject;
        }

        [MenuItem("GameObject/Shababeek/Create/Button", true)]
        private static bool ValidateCreateButton()
        {
            return ValidateRequiredComponents();
        }

        [MenuItem("GameObject/Shababeek/Create/Switch", priority = 6)]
        public static void CreateSwitch()
        {
            GameObject switchObject = CreateSwitchGeometry();
            Selection.activeGameObject = switchObject;
        }

        [MenuItem("GameObject/Shababeek/Create/Switch", true)]
        private static bool ValidateCreateSwitch()
        {
            return ValidateRequiredComponents();
        }

        // ========== MAKE INTO MENU ==========

        [MenuItem("GameObject/Shababeek/Make Into/Grabbable", priority = 100)]
        public static void MakeIntoGrabbable()
        {
            var obj = Selection.activeGameObject;
            if (obj == null)
            {
                Debug.LogWarning("No object selected. Please select an object to make grabbable.");
                return;
            }

            if (obj.GetComponent<Grabable>())
            {
                Debug.LogWarning("Object already has a Grabable component.");
                return;
            }
            
            obj.AddComponent<Grabable>();
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/Shababeek/Make Into/Grabbable", true)]
        private static bool ValidateMakeIntoGrabbable()
        {
            return ValidateRequiredComponents() && Selection.activeGameObject != null;
        }

        [MenuItem("GameObject/Shababeek/Make Into/Throwable", priority = 101)]
        public static void MakeIntoThrowable()
        {
            var obj = Selection.activeGameObject;
            if (obj == null)
            {
                Debug.LogWarning("No object selected. Please select an object to make throwable.");
                return;
            }

            if (obj.GetComponent<Throwable>())
            {
                Debug.LogWarning("Object already has a Throwable component.");
                return;
            }
            
            if (!obj.GetComponent<Rigidbody>())
                obj.AddComponent<Rigidbody>().isKinematic = true;
            if (!obj.GetComponent<Grabable>())
                obj.AddComponent<Grabable>();
            obj.AddComponent<Throwable>();
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/Shababeek/Make Into/Throwable", true)]
        private static bool ValidateMakeIntoThrowable()
        {
            return ValidateRequiredComponents() && Selection.activeGameObject != null;
        }

        [MenuItem("GameObject/Shababeek/Make Into/Lever", priority = 102)]
        public static void MakeIntoLever()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No object selected. Please select an object to convert into a lever.");
                return;
            }
            
            if (IsInteractable(selectedObject))
            {
                Debug.LogError("Object is already interactable");
                return;
            }

            var leverObject = new GameObject("Lever").transform;
            leverObject.transform.position = selectedObject.transform.position;
            InitializeConstrainedInteractable<LeverInteractable>(leverObject, selectedObject);
            Selection.activeGameObject = leverObject.gameObject;
        }

        [MenuItem("GameObject/Shababeek/Make Into/Lever", true)]
        private static bool ValidateMakeIntoLever()
        {
            if (!ValidateRequiredComponents()) return false;
            var selectedObject = Selection.activeGameObject;
            return selectedObject != null && !IsInteractable(selectedObject);
        }

        [MenuItem("GameObject/Shababeek/Make Into/Drawer", priority = 103)]
        public static void MakeIntoDrawer()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No object selected. Please select an object to convert into a drawer.");
                return;
            }
            
            if (IsInteractable(selectedObject))
            {
                Debug.LogError("Object is already interactable");
                return;
            }

            var drawerObject = new GameObject("Drawer").transform;
            drawerObject.transform.position = selectedObject.transform.position;
            InitializeConstrainedInteractable<DrawerInteractable>(drawerObject, selectedObject);
            Selection.activeGameObject = drawerObject.gameObject;
        }

        [MenuItem("GameObject/Shababeek/Make Into/Drawer", true)]
        private static bool ValidateMakeIntoDrawer()
        {
            if (!ValidateRequiredComponents()) return false;
            var selectedObject = Selection.activeGameObject;
            return selectedObject != null && !IsInteractable(selectedObject);
        }

        [MenuItem("GameObject/Shababeek/Make Into/Joystick", priority = 104)]
        public static void MakeIntoJoystick()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogWarning("No object selected. Please select an object to convert into a joystick.");
                return;
            }
            
            if (IsInteractable(selectedObject))
            {
                Debug.LogError("Object is already interactable");
                return;
            }

            var joystickObject = new GameObject("Joystick").transform;
            joystickObject.transform.position = selectedObject.transform.position;
            InitializeConstrainedInteractable<JoystickInteractable>(joystickObject, selectedObject);
            Selection.activeGameObject = joystickObject.gameObject;
        }

        [MenuItem("GameObject/Shababeek/Make Into/Joystick", true)]
        private static bool ValidateMakeIntoJoystick()
        {
            if (!ValidateRequiredComponents()) return false;
            var selectedObject = Selection.activeGameObject;
            return selectedObject != null && !IsInteractable(selectedObject);
        }
        
        // ========== SCENE INITIALIZATION ==========
        
        [MenuItem("GameObject/Shababeek/Initialize CameraRig", priority = 0)]
        [MenuItem("Shababeek/Initialize Scene", priority = 0)]
        public static void InitializeScene()
        {
            Debug.Log("Initializing Shababeek XR Scene...");
            
            // Clean up existing cameras and rigs
            DestroyOldRigAndCamera();
            
            // Load and instantiate the CameraRig prefab
            var cameraRig = Resources.Load<CameraRig>("CameraRig");
            if (cameraRig != null)
            {
                var instantiatedRig = Instantiate<CameraRig>(cameraRig);
                Resources.UnloadAsset(cameraRig);
                
                // Select the new camera rig in the hierarchy
                Selection.activeGameObject = instantiatedRig.gameObject;
                
                Debug.Log("Scene initialized successfully! CameraRig has been created and selected.");
                Debug.Log("Note: All previous cameras have been removed to prevent conflicts with XR setup.");
            }
            else
            {
                Debug.LogError("Failed to load CameraRig prefab from Resources. Please ensure the CameraRig prefab exists in the Resources folder.");
            }
        }

        [MenuItem("GameObject/Shababeek/Initialize CameraRig", true)]
        [MenuItem("Shababeek/Initialize Scene", true)]
        private static bool ValidateInitializeScene()
        {
            // Check if CameraRig prefab exists in Resources
            var cameraRig = Resources.Load<CameraRig>("CameraRig");
            return cameraRig != null;
        }

        private static void DestroyOldRigAndCamera()
        {
            // First, destroy any existing CameraRig
            var rig = Object.FindFirstObjectByType<CameraRig>();
            if (rig) 
            {
                Object.DestroyImmediate(rig.gameObject);
                Debug.Log("Destroyed existing CameraRig");
            }
            
            // Destroy all cameras in the scene to ensure clean setup
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                // Log which camera is being destroyed for debugging
                Debug.Log($"Destroying camera: {camera.name} (Tag: {camera.tag})");
                Object.DestroyImmediate(camera.gameObject);
            }
            
            if (cameras.Length > 0)
            {
                Debug.Log($"Destroyed {cameras.Length} camera(s) to prepare for XR setup");
            }
        }

        // ========== GEOMETRY CREATION HELPERS ==========
        // These are only used for "Make Into" functions now

        private static T InitializeConstrainedInteractable<T>(Transform parentTransform, GameObject movingParts) where T : ConstrainedInteractableBase
        {
            try
            {
                var constrainedInteractable = parentTransform.gameObject.AddComponent<T>();
                var interactableObject = InitializeInteractableObject(movingParts.transform);
                interactableObject.parent = parentTransform;
                if (constrainedInteractable.InteractableObject )
                    DestroyImmediate(constrainedInteractable.InteractableObject.gameObject);
                constrainedInteractable.InteractableObject = interactableObject;
                constrainedInteractable.ValidateAndCreateHierarchy();
                return constrainedInteractable;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize {typeof(T).Name}: {e.Message}");
                throw;
            }
        }

        private static GameObject CreateDrawerGeometry()
        {
            // This is now only used for "Make Into" - creates just the moving parts
            GameObject movingParts = new GameObject("Drawer");
            
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            box.name = "DrawerBox";
            box.localScale = new Vector3(DrawerBoxWidth, DrawerBoxHeight, DrawerBoxDepth);
            box.localPosition = Vector3.zero;
            box.parent = movingParts.transform;
            
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            handle.name = "Handle";
            handle.localScale = new Vector3(DrawerHandleWidth, DrawerHandleHeight, DrawerHandleDepth);
            handle.localPosition = new Vector3(0, 0, DrawerBoxDepth / 2 + DrawerHandleDepth / 2);
            handle.parent = box;
            
            return movingParts;
        }

        private static GameObject CreateJoystickGeometry()
        {
            // This is now only used for "Make Into" - creates just the moving parts
            GameObject movingParts = new GameObject("Joystick");
            
            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            stick.name = "Stick";
            stick.localScale = new Vector3(JoystickBarrelRadius, JoystickBarrelLength, JoystickBarrelRadius);
            stick.localPosition = new Vector3(0, JoystickBarrelLength, 0);
            stick.parent = movingParts.transform;
            
            var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            knob.name = "Knob";
            knob.localScale = Vector3.one * JoystickBodyHeight;
            knob.localPosition = new Vector3(0, JoystickBarrelLength * 2, 0);
            knob.parent = movingParts.transform;
            
            return movingParts;
        }

        private static GameObject CreateLeverGeometry()
        {
            // This is now only used for "Make Into" - creates just the moving parts
            GameObject movingParts = new GameObject("Lever");
            
            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            arm.name = "Arm";
            arm.localScale = new Vector3(LeverARMWidth, LeverARMLength, LeverARMHeight);
            arm.localPosition = new Vector3(0, LeverARMLength / 2, 0);
            arm.parent = movingParts.transform;
            
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            handle.name = "Handle";
            handle.localScale = new Vector3(LeverHandleRadius, LeverHandleHeight, LeverHandleRadius);
            handle.localPosition = new Vector3(0, LeverARMLength, 0);
            handle.parent = movingParts.transform;
            
            return movingParts;
        }
        
        private static GameObject CreateButtonGeometry()
        {
            var buttonObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            var buttonBody = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            
            buttonBody.name = "Button";
            
            // Set up the button body
            var trigger = buttonBody.gameObject.AddComponent<BoxCollider>();
            trigger.center = Vector3.up * ButtonTriggerY;
            trigger.isTrigger = true;
            
            // Set up the button object
            buttonObject.transform.parent = buttonBody.transform;
            buttonObject.localScale = new Vector3(ButtonScale, ButtonHeight, ButtonScale);
            buttonObject.localPosition = Vector3.up * ButtonYOffset;
            
            // Add the VRButton component
            var button = buttonBody.gameObject.AddComponent<VRButton>();
            button.Button = buttonObject.transform;
            
            // Scale the button body
            buttonBody.localScale = Vector3.one * ButtonBodyScale;
            
            return buttonBody.gameObject;
        }
        
        private static GameObject CreateSwitchGeometry()
        {
            GameObject switchObject = new GameObject("Switch");
            
            // Panel/base (flat surface the switch sits on)
            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            panel.name = "Panel";
            panel.localScale = new Vector3(SwitchPanelWidth, SwitchPanelHeight, SwitchPanelDepth);
            panel.localPosition = Vector3.zero;
            panel.parent = switchObject.transform;
            
            // Toggle switch (small rectangular piece that flips)
            var toggle = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            toggle.name = "Toggle";
            toggle.localScale = new Vector3(SwitchToggleWidth, SwitchToggleHeight, SwitchToggleDepth);
            toggle.localPosition = new Vector3(0, SwitchToggleY, 0);
            toggle.parent = switchObject.transform;
            
            // Add a trigger collider to the switch for interaction detection
            var trigger = switchObject.AddComponent<BoxCollider>();
            trigger.center = new Vector3(0, SwitchToggleY, 0);
            trigger.size = new Vector3(SwitchPanelWidth, SwitchToggleHeight * 3, SwitchPanelDepth);
            trigger.isTrigger = true;
            
            // Add the Switch component
            var switchComponent = switchObject.AddComponent<Switch>();
            switchComponent.SwitchBody = toggle;
            
            return switchObject;
        }

        // ========== VALIDATION HELPERS ==========

        private static bool IsInteractable(GameObject obj)
        {
            return obj && obj.GetComponent<InteractableBase>();
        }

        private static bool ValidateRequiredComponents()
        {
            // Check if required components are available
            var requiredTypes = new System.Type[]
            {
                typeof(Grabable),
                typeof(Throwable),
                typeof(LeverInteractable),
                typeof(DrawerInteractable),
                typeof(JoystickInteractable),
                typeof(VRButton),
                typeof(Switch)
            };

            foreach (var type in requiredTypes)
            {
                if (type == null)
                {
                    Debug.LogError($"Required component type {type} is not available. Please ensure all Shababeek components are properly imported.");
                    return false;
                }
            }

            return true;
        }

        private static Transform InitializeInteractableObject(Transform obj)
        {
            var interactableObject = new GameObject("interactableObject").transform;
            interactableObject.position = obj.position;
            interactableObject.localScale = Vector3.one;
            obj.transform.parent = interactableObject;
            return interactableObject;
        }
    }
}
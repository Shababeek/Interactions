using Shababeek.Interactions;
using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Shababeek.Interactions.Editors
{
    public class GameObjectMenuHelpers : Editor
    {
        [MenuItem("GameObject/Shababeek/MakeGrabable", priority = 0)]
        public static void MakeInteractable()
        {
            var obj = Selection.activeGameObject;
            if (obj == null)
                obj = new GameObject("grabable object");

            if (obj.GetComponent<Grabable>()) return;
            //obj.AddComponent<Rigidbody>();
            obj.AddComponent<Grabable>();
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/Shababeek/MakeGrabable", true)]
        private static bool ValidateMakeInteractable()
        {
            return ValidateRequiredComponents();
        }

        [MenuItem("GameObject/Shababeek/MakeThrowable", priority = 1)]
        public static void MakeThrowable()
        {
            var obj = Selection.activeGameObject;
            if (obj == null)
                obj = new GameObject("Throwable object");

            if (obj.GetComponent<Throwable>()) return;
            obj.AddComponent<Rigidbody>().isKinematic = true;
            obj.AddComponent<Grabable>();
            obj.AddComponent<Throwable>();
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/Shababeek/MakeThrowable", true)]
        private static bool ValidateMakeThrowable()
        {
            return ValidateRequiredComponents();
        }

        [MenuItem("GameObject/Shababeek/MakeLever", priority = 4)]
        public static void MakeLever()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (IsInteractable(selectedObject))
            {
                Debug.LogError("Object is already interactable");
                return;
            }

            if (selectedObject == null)
            {
                selectedObject = CreateLever();
            }

            var leverObject = new GameObject(selectedObject.name).transform;
            leverObject.transform.position = selectedObject.transform.position;
            InitializeConstrainedInteractable<LeverInteractable>(leverObject, selectedObject);
            Selection.activeGameObject = leverObject.gameObject;
        }

        [MenuItem("GameObject/Shababeek/MakeLever", true)]
        private static bool ValidateMakeLever()
        {
            if (!ValidateRequiredComponents()) return false;
            var selectedObject = Selection.activeGameObject;
            return selectedObject == null || !IsInteractable(selectedObject);
        }

        [MenuItem("GameObject/Shababeek/MakeDrawer", priority = 3)]
        public static void MakeDrawer()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (IsInteractable(selectedObject))
            {
                Debug.LogError("Object is already interactable");
                return;
            }

            if (selectedObject == null)
            {
                selectedObject = CreateDrawer();
            }

            var drawerObject = new GameObject(selectedObject.name).transform;
            InitializeConstrainedInteractable<DrawerInteractable>(drawerObject, selectedObject);
            Selection.activeGameObject = drawerObject.gameObject;
        }

        [MenuItem("GameObject/Shababeek/MakeDrawer", true)]
        private static bool ValidateMakeDrawer()
        {
            if (!ValidateRequiredComponents()) return false;
            var selectedObject = Selection.activeGameObject;
            return selectedObject == null || !IsInteractable(selectedObject);
        }

        [MenuItem("GameObject/Shababeek/MakeTurret", priority = 5)]
        public static void MakeTurret()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (IsInteractable(selectedObject))
            {
                Debug.LogError("Object is already interactable");
                return;
            }

            if (selectedObject == null)
            {
                selectedObject = CreateTurret();
            }

            var turretObject = new GameObject(selectedObject.name).transform;
            InitializeConstrainedInteractable<JoystickInteractable>(turretObject, selectedObject);
            Selection.activeGameObject = turretObject.gameObject;
        }

        [MenuItem("GameObject/Shababeek/MakeTurret", true)]
        private static bool ValidateMakeTurret()
        {
            if (!ValidateRequiredComponents()) return false;
            var selectedObject = Selection.activeGameObject;
            return selectedObject == null || !IsInteractable(selectedObject);
        }
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

        private static GameObject CreateLever()
        {
            GameObject selectedObject;
            selectedObject = new GameObject("Lever");
            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            stick.parent = selectedObject.transform;
            stick.localScale = new Vector3(.1f, .2f, .1f);
            stick.localPosition = new Vector3(0, .2f, 0);
            var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            knob.name = "knob";
            knob.transform.parent = selectedObject.transform;
            knob.transform.localScale = Vector3.one * .15f;
            knob.transform.localPosition = new Vector3(0, .45f, 0);
            return selectedObject;
        }

        private static T InitializeConstrainedInteractable<T>(Transform interactableTransform, GameObject selectedObject) where T : ConstrainedInteractableBase
        {
            try
            {
                interactableTransform.transform.position = selectedObject.transform.position;
                var constrainedInteractable = interactableTransform.gameObject.AddComponent<T>();
                var interactableObject = InitializeInteractableObject(selectedObject.transform);
                interactableObject.parent = interactableTransform;
                constrainedInteractable.InteractableObject = interactableObject;
                constrainedInteractable.Initialize();
                return constrainedInteractable;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize {typeof(T).Name}: {e.Message}");
                throw;
            }
        }

        private static GameObject CreateDrawer()
        {
            GameObject selectedObject;
            selectedObject = new GameObject("Drawer");
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            body.localScale = new Vector3(.4f, .05f, .5f);
            body.localPosition = new Vector3(0, 0, 0);
            body.transform.parent = selectedObject.transform;
            var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            knob.name = "knob";
            knob.transform.parent = selectedObject.transform;
            knob.transform.localScale = Vector3.one * .1f;
            knob.transform.localPosition = new Vector3(0, 0, .25f);
            return selectedObject;
        }

        private static GameObject CreateTurret()
        {
            GameObject selectedObject;
            selectedObject = new GameObject("Turret");
            
            // Base
            var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            baseObj.name = "Base";
            baseObj.localScale = new Vector3(.3f, .1f, .3f);
            baseObj.localPosition = Vector3.zero;
            baseObj.parent = selectedObject.transform;
            
            // Turret body
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            body.name = "Body";
            body.localScale = new Vector3(.2f, .15f, .3f);
            body.localPosition = new Vector3(0, .125f, 0);
            body.parent = selectedObject.transform;
            
            // Gun barrel
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            barrel.name = "Barrel";
            barrel.localScale = new Vector3(.05f, .4f, .05f);
            barrel.localPosition = new Vector3(0, .125f, .2f);
            barrel.localRotation = Quaternion.Euler(90, 0, 0);
            barrel.parent = selectedObject.transform;
            
            return selectedObject;
        }

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
                typeof(VRButton)
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
            interactableObject.position = interactableObject.position;
            interactableObject.localScale = Vector3.one;
            obj.transform.parent = interactableObject;
            return interactableObject;
        }

        [MenuItem("GameObject/Shababeek/Button", priority = 9)]
        public static void MakeButton()
        {
            var buttonObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            var buttonBody = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            
            buttonBody.name = "Button";
            
            // Set up the button body
            var trigger = buttonBody.gameObject.AddComponent<BoxCollider>();
            trigger.center = Vector3.up * .2f;
            trigger.isTrigger = true;
            
            // Set up the button object
            buttonObject.transform.parent = buttonBody.transform;
            buttonObject.localScale = new Vector3(.5f, .25f, .5f);
            buttonObject.localPosition = Vector3.up * .5f;
            
            // Add the VRButton component
            var button = buttonBody.gameObject.AddComponent<VRButton>();
            button.Button = buttonObject.transform;
            
            // Scale the button body
            buttonBody.localScale = Vector3.one / 10;
            
            // Select the created button
            Selection.activeGameObject = buttonBody.gameObject;
        }

        [MenuItem("GameObject/Shababeek/Button", true)]
        private static bool ValidateMakeButton()
        {
            return ValidateRequiredComponents();
        }
    }
}
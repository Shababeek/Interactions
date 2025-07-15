using Kandooz.InteractionSystem.Interactions;
using UnityEditor;
using UnityEngine;

namespace Kandooz.InteractionSystem.Core
{
    public class GameObjectMenuHelpers : Editor
    {
        [MenuItem("GameObject/Kandooz/MakeGrabable", priority = 0)]
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

        [MenuItem("GameObject/Kandooz/MakeThrowable", priority = 1)]
        public static void MakeThrowable()
        {
            var obj = Selection.activeGameObject;
            if (obj == null)
                obj = new GameObject("Grabable object");

            if (obj.GetComponent<Throwable>()) return;
            obj.AddComponent<Rigidbody>().isKinematic = true;
            obj.AddComponent<Grabable>();
            obj.AddComponent<Throwable>();
            Selection.activeGameObject = obj;
        }

        [MenuItem("GameObject/Kandooz/MakeLever", priority = 4)]
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

        [MenuItem("GameObject/Kandooz/MakeDrawer", priority = 3)]
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
            InitializeConstrainedInteractable<LinearlyConstrainedInteractable>(drawerObject, selectedObject);
            Selection.activeGameObject = drawerObject.gameObject;
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
            interactableTransform.transform.position = selectedObject.transform.position;
            var constrainedInteractable = interactableTransform.gameObject.AddComponent<T>();
            var interactableObject = InitializeInteractableObject(selectedObject.transform);
            interactableObject.parent = interactableTransform;
            constrainedInteractable.InteractableObject = interactableObject;
            constrainedInteractable.Initialize();
            return constrainedInteractable;
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

        private static bool IsInteractable(GameObject obj)
        {
            return obj && obj.GetComponent<InteractableBase>();
        }

        private static Transform InitializeInteractableObject(Transform obj)
        {
            var interactableObject = new GameObject("interactableObject").transform;
            interactableObject.position = interactableObject.position;
            interactableObject.localScale = Vector3.one;
            obj.transform.parent = interactableObject;
            return interactableObject;
        }

        [MenuItem("GameObject/Kandooz/Button", priority = 9)]
        public static void MakeButton()
        {
            var buttonObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            var buttonBody = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            buttonObject.GetComponent<MeshRenderer>().material.color = Color.red;
            buttonBody.name = "Button";
            var trigger = buttonBody.gameObject.AddComponent<BoxCollider>();
            trigger.center = Vector3.up * .2f;
            trigger.isTrigger = true;
            buttonObject.transform.parent = buttonBody.transform;
            buttonObject.localScale = new Vector3(.5f, .25f, .5f);
            buttonObject.localPosition = Vector3.up * .5f;
            var button = buttonBody.gameObject.AddComponent<VRButton>();
            button.Button = buttonObject.transform;
            buttonBody.localScale = Vector3.one / 10;
        }
    }
}
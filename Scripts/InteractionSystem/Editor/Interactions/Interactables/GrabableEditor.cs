using Shababeek.Interactions;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Grabable))]
    [CanEditMultipleObjects]
    public class GrabableEditor : InteractableBaseEditor
    {
        private SerializedProperty _tweenerProp;
        private SerializedProperty _canBeThrownProp;
        private SerializedProperty _throwableProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _tweenerProp = serializedObject.FindProperty("tweener");
            _canBeThrownProp = serializedObject.FindProperty("canBeThrown");
            _throwableProp = serializedObject.FindProperty("throwable");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Component that enables objects to be grabbed and held by VR hands. Manages the grab process, hand positioning, and smooth transitions between grab states. Optionally tracks throw velocity on release when a Rigidbody is present.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Grabable Settings", EditorStyles.boldLabel);

            if (_tweenerProp != null)
                EditorGUILayout.PropertyField(_tweenerProp,
                    new GUIContent("Tweener", "The tweener component used for smooth grab animations."));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Throwing", EditorStyles.boldLabel);

            if (_canBeThrownProp != null)
                EditorGUILayout.PropertyField(_canBeThrownProp,
                    new GUIContent("Can Be Thrown",
                        "When enabled, the grabable tracks velocity while held and applies a throw on release. Requires a Rigidbody to actually launch."));

            if (_canBeThrownProp != null && _canBeThrownProp.boolValue && _throwableProp != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_throwableProp,
                    new GUIContent("Throwable", "Throw tracking settings."), true);
                EditorGUI.indentLevel--;

                if (((Grabable)target).GetComponent<Rigidbody>() == null)
                {
                    EditorGUILayout.HelpBox(
                        "Can Be Thrown is enabled but there is no Rigidbody on this GameObject. Throwing will be skipped at runtime.",
                        MessageType.Warning);
                }
            }
        }

        protected override void DrawCustomEvents()
        {
            // Throwable's onThrowEnd event is drawn as part of the Throwable property in DrawCustomProperties.
        }
    }
}

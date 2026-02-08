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

        protected override void OnEnable()
        {
            base.OnEnable();
            _tweenerProp = base.serializedObject.FindProperty("tweener");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Component that enables objects to be grabbed and held by VR hands. Manages the grabbing process, hand positioning, and smooth transitions between grab states using pose constraints and tweening.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Grabable Settings", EditorStyles.boldLabel);

            if (_tweenerProp != null)
                EditorGUILayout.PropertyField(_tweenerProp, new GUIContent("Tweener", "The tweener component used for smooth grab animations."));
        }

        protected override void DrawCustomEvents()
        {
            //Nothing to Do, no custom Events
        }
    }
}
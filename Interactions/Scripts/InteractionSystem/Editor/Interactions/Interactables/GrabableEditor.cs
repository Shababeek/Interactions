using Shababeek.Interactions;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Grabable))]
    [CanEditMultipleObjects]
    public class GrabableEditor : InteractableBaseEditor
    {
        // Editable properties
        private SerializedProperty _hideHandProp;
        private SerializedProperty _tweenerProp;


        protected override void OnEnable()
        {
            base.OnEnable();

            _hideHandProp = base.serializedObject.FindProperty("hideHand");
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

            if (_hideHandProp != null)
                EditorGUILayout.PropertyField(_hideHandProp, new GUIContent("Hide Hand", "Whether to hide the hand model when this object is grabbed."));

            if (_tweenerProp != null)
                EditorGUILayout.PropertyField(_tweenerProp, new GUIContent("Tweener", "The tweener component used for smooth grab animations."));
        }
        
    }
}
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(ConstrainedInteractableBase))]
    public abstract class ConstrainedInteractableEditor : InteractableBaseEditor
    {
        private SerializedProperty _interactableObjectProp;
        private SerializedProperty _snapDistanceProp;
        private SerializedProperty _returnSpeedProp;
        private SerializedProperty _returnWhenDeselectedProb;

        protected override void OnEnable()
        {
            base.OnEnable();
            _interactableObjectProp = base.serializedObject.FindProperty("interactableObject");
            _snapDistanceProp = base.serializedObject.FindProperty("_snapDistance");
            
            _returnSpeedProp = base.serializedObject.FindProperty("returnSpeed");
            _returnWhenDeselectedProb = base.serializedObject.FindProperty("returnWhenDeselected");

        }
        
        

        protected override void DrawCustomProperties()
        {
            if (_interactableObjectProp != null)
                EditorGUILayout.PropertyField(_interactableObjectProp, new GUIContent("Interactable Object", "The object that will be moved by the drawer"));
            if (_snapDistanceProp != null)
                EditorGUILayout.PropertyField(_snapDistanceProp, new GUIContent("Snap Distance", "Distance threshold for snapping to positions"));
            if (_returnWhenDeselectedProb != null)
                EditorGUILayout.PropertyField(_returnWhenDeselectedProb, new GUIContent("Return To Original", "Whether the drawer returns to its original position when released"));
            if (_returnSpeedProp != null && _returnWhenDeselectedProb is { boolValue: true })
                EditorGUILayout.PropertyField(_returnSpeedProp, new GUIContent("Return Speed", "Speed of return animation"));
        }
    }
}
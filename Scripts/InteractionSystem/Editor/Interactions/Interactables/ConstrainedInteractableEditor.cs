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
        
        

        private void DrawNonUniformScaleWarning()
        {
            var targetTransform = ((Component)target).transform;
            var scale = targetTransform.localScale;
            if (!Mathf.Approximately(scale.x, scale.y) || !Mathf.Approximately(scale.y, scale.z))
            {
                EditorGUILayout.HelpBox(
                    "Non-uniform scale detected. Constrained interactables require uniform scaling " +
                    "(e.g. 2,2,2 not 2,1,3) for correct hand positioning and visuals. " +
                    "Apply non-uniform proportions to child meshes instead.",
                    MessageType.Warning);
            }
        }

        protected override void DrawCustomProperties()
        {
            DrawNonUniformScaleWarning();

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
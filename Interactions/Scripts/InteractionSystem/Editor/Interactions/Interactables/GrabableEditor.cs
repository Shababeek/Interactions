using Shababeek.Interactions;
using UnityEditor;

namespace Shababeek.InteractionSystem.Interactions.Editors
{
    [CustomEditor(typeof(Grabable))]
    [CanEditMultipleObjects]
    public class GrabableEditor : Editor
    {
        private bool showEvents = true;

        // Editable properties
        private SerializedProperty hideHandProp;
        private SerializedProperty tweenerProp;
        private SerializedProperty interactionHandProp;

        private SerializedProperty selectionButtonProp;

        // Events
        private SerializedProperty onSelectedProp;
        private SerializedProperty onDeselectedProp;
        private SerializedProperty onHoverStartProp;
        private SerializedProperty onHoverEndProp;

        private SerializedProperty onActivatedProp;

        // Read-only
        private SerializedProperty isSelectedProp;
        private SerializedProperty currentInteractorProp;
        private SerializedProperty currentStateProp;

        private void OnEnable()
        {
            hideHandProp = serializedObject.FindProperty("hideHand");
            tweenerProp = serializedObject.FindProperty("tweener");
            interactionHandProp = serializedObject.FindProperty("interactionHand");
            selectionButtonProp = serializedObject.FindProperty("selectionButton");
            onSelectedProp = serializedObject.FindProperty("onSelected");
            onDeselectedProp = serializedObject.FindProperty("onDeselected");
            onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            onActivatedProp = serializedObject.FindProperty("onActivated");
            // Read-only fields 
            isSelectedProp = serializedObject.FindProperty("isSelected");
            currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            currentStateProp = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "The Grabable component allows objects to be picked up and manipulated by interactors. Configure the options below. [insert screenshot here]",
                MessageType.Info
            );
            serializedObject.Update();
            // Editable properties
            if (hideHandProp != null)
                EditorGUILayout.PropertyField(hideHandProp);
            if (tweenerProp != null)
                EditorGUILayout.PropertyField(tweenerProp);
            if (interactionHandProp != null)
                EditorGUILayout.PropertyField(interactionHandProp);
            if (selectionButtonProp != null)
                EditorGUILayout.PropertyField(selectionButtonProp);
            // Events foldout
            showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(showEvents, "Events");
            if (showEvents)
            {
                if (onSelectedProp != null)
                    EditorGUILayout.PropertyField(onSelectedProp);
                if (onDeselectedProp != null)
                    EditorGUILayout.PropertyField(onDeselectedProp);
                if (onHoverStartProp != null)
                    EditorGUILayout.PropertyField(onHoverStartProp);
                if (onHoverEndProp != null)
                    EditorGUILayout.PropertyField(onHoverEndProp);
                if (onActivatedProp != null)
                    EditorGUILayout.PropertyField(onActivatedProp);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            // Read-only fields at the end
            EditorGUI.BeginDisabledGroup(true);
            if (isSelectedProp != null)

                EditorGUILayout.PropertyField(isSelectedProp);


            if (currentInteractorProp != null)

                EditorGUILayout.PropertyField(currentInteractorProp);


            if (currentStateProp != null)

                EditorGUILayout.PropertyField(currentStateProp);


            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
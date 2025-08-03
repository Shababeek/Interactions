using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(WheelInteractable))]
    [CanEditMultipleObjects]
    public class WheelInteractableEditor : Editor
    {
        // Events
        private SerializedProperty onWheelRotatedProp;
        private SerializedProperty onSelectedProp;
        private SerializedProperty onDeselectedProp;
        private SerializedProperty onHoverStartProp;
        private SerializedProperty onHoverEndProp;
        private SerializedProperty onActivatedProp;
        // Read-only
        private SerializedProperty currentRotationProp;
        private SerializedProperty isSelectedProp;
        private SerializedProperty currentInteractorProp;
        private SerializedProperty currentStateProp;
        // Editable properties
        private SerializedProperty interactionHandProp;
        private SerializedProperty selectionButtonProp;
        private SerializedProperty interactableObjectProp;
        private SerializedProperty snapDistanceProp;
        private bool showEvents = true;

        protected  void OnEnable()
        {
            onWheelRotatedProp = serializedObject.FindProperty("onWheelRotated");
            onSelectedProp = serializedObject.FindProperty("onSelected");
            onDeselectedProp = serializedObject.FindProperty("onDeselected");
            onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            onActivatedProp = serializedObject.FindProperty("onActivated");
            currentRotationProp = serializedObject.FindProperty("currentRotation");
            isSelectedProp = serializedObject.FindProperty("isSelected");
            currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            currentStateProp = serializedObject.FindProperty("currentState");
            interactionHandProp = serializedObject.FindProperty("interactionHand");
            selectionButtonProp = serializedObject.FindProperty("selectionButton");
            interactableObjectProp = serializedObject.FindProperty("interactableObject");
            snapDistanceProp = serializedObject.FindProperty("snapDistance");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "The wheel will rotate based on hand position and track full rotations.\n\n" +
                "Pose constraints are automatically handled by the UnifiedPoseConstraintSystem component (automatically added). Use it to configure hand poses and positioning.",
                MessageType.Info
            );
            serializedObject.Update();
            // Editable properties
            if (interactionHandProp != null)
                EditorGUILayout.PropertyField(interactionHandProp);
            if (selectionButtonProp != null)
                EditorGUILayout.PropertyField(selectionButtonProp);
            if (interactableObjectProp != null)
                EditorGUILayout.PropertyField(interactableObjectProp, new GUIContent("Interactable Object"));
            if (snapDistanceProp != null)
                EditorGUILayout.PropertyField(snapDistanceProp);
            // Events foldout
            showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(showEvents, "Events");
            if (showEvents)
            {
                if (onWheelRotatedProp != null)
                    EditorGUILayout.PropertyField(onWheelRotatedProp);
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
            if (currentRotationProp != null)
                EditorGUILayout.PropertyField(currentRotationProp);
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
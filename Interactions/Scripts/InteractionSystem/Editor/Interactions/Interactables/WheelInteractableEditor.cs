using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(WheelInteractable))]
    [CanEditMultipleObjects]
    public class WheelInteractableEditor : Editor
    {
        // Events
        private SerializedProperty _onWheelRotatedProp;
        private SerializedProperty _onSelectedProp;
        private SerializedProperty _onDeselectedProp;
        private SerializedProperty _onHoverStartProp;
        private SerializedProperty _onHoverEndProp;
        private SerializedProperty _onActivatedProp;
        // Read-only
        private SerializedProperty _currentRotationProp;
        private SerializedProperty _isSelectedProp;
        private SerializedProperty _currentInteractorProp;
        private SerializedProperty _currentStateProp;
        // Editable properties
        private SerializedProperty _interactionHandProp;
        private SerializedProperty _selectionButtonProp;
        private SerializedProperty _interactableObjectProp;
        private SerializedProperty _snapDistanceProp;
        private bool _showEvents = true;

        protected  void OnEnable()
        {
            _onWheelRotatedProp = serializedObject.FindProperty("onWheelRotated");
            _onSelectedProp = serializedObject.FindProperty("onSelected");
            _onDeselectedProp = serializedObject.FindProperty("onDeselected");
            _onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            _onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            _onActivatedProp = serializedObject.FindProperty("onActivated");
            _currentRotationProp = serializedObject.FindProperty("currentRotation");
            _isSelectedProp = serializedObject.FindProperty("isSelected");
            _currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            _currentStateProp = serializedObject.FindProperty("currentState");
            _interactionHandProp = serializedObject.FindProperty("interactionHand");
            _selectionButtonProp = serializedObject.FindProperty("selectionButton");
            _interactableObjectProp = serializedObject.FindProperty("interactableObject");
            _snapDistanceProp = serializedObject.FindProperty("snapDistance");
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
            if (_interactionHandProp != null)
                EditorGUILayout.PropertyField(_interactionHandProp);
            if (_selectionButtonProp != null)
                EditorGUILayout.PropertyField(_selectionButtonProp);
            if (_interactableObjectProp != null)
                EditorGUILayout.PropertyField(_interactableObjectProp, new GUIContent("Interactable Object"));
            if (_snapDistanceProp != null)
                EditorGUILayout.PropertyField(_snapDistanceProp);
            // Events foldout
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                if (_onWheelRotatedProp != null)
                    EditorGUILayout.PropertyField(_onWheelRotatedProp);
                if (_onSelectedProp != null)
                    EditorGUILayout.PropertyField(_onSelectedProp);
                if (_onDeselectedProp != null)
                    EditorGUILayout.PropertyField(_onDeselectedProp);
                if (_onHoverStartProp != null)
                    EditorGUILayout.PropertyField(_onHoverStartProp);
                if (_onHoverEndProp != null)
                    EditorGUILayout.PropertyField(_onHoverEndProp);
                if (_onActivatedProp != null)
                    EditorGUILayout.PropertyField(_onActivatedProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            // Read-only fields at the end
            EditorGUI.BeginDisabledGroup(true);
            if (_currentRotationProp != null)
                EditorGUILayout.PropertyField(_currentRotationProp);
            if (_isSelectedProp != null)
                EditorGUILayout.PropertyField(_isSelectedProp);
            if (_currentInteractorProp != null)
                EditorGUILayout.PropertyField(_currentInteractorProp);
            if (_currentStateProp != null)
                EditorGUILayout.PropertyField(_currentStateProp);
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
} 
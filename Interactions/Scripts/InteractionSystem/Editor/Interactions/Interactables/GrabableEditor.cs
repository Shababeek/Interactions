using Shababeek.Interactions;
using UnityEditor;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Grabable))]
    [CanEditMultipleObjects]
    public class GrabableEditor : Editor
    {
        private bool _showEvents = true;

        // Editable properties
        private SerializedProperty _hideHandProp;
        private SerializedProperty _tweenerProp;
        private SerializedProperty _interactionHandProp;

        private SerializedProperty _selectionButtonProp;

        // Events
        private SerializedProperty _onSelectedProp;
        private SerializedProperty _onDeselectedProp;
        private SerializedProperty _onHoverStartProp;
        private SerializedProperty _onHoverEndProp;

        private SerializedProperty _onActivatedProp;

        // Read-only
        private SerializedProperty _isSelectedProp;
        private SerializedProperty _currentInteractorProp;
        private SerializedProperty _currentStateProp;

        private void OnEnable()
        {
            _hideHandProp = serializedObject.FindProperty("hideHand");
            _tweenerProp = serializedObject.FindProperty("tweener");
            _interactionHandProp = serializedObject.FindProperty("interactionHand");
            _selectionButtonProp = serializedObject.FindProperty("selectionButton");
            _onSelectedProp = serializedObject.FindProperty("onSelected");
            _onDeselectedProp = serializedObject.FindProperty("onDeselected");
            _onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            _onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            _onActivatedProp = serializedObject.FindProperty("onActivated");
            // Read-only fields 
            _isSelectedProp = serializedObject.FindProperty("isSelected");
            _currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            _currentStateProp = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "The Grabable component allows objects to be picked up and manipulated by interactors. Configure the options below. [insert screenshot here]",
                MessageType.Info
            );
            serializedObject.Update();
            // Editable properties
            if (_hideHandProp != null)
                EditorGUILayout.PropertyField(_hideHandProp);
            if (_tweenerProp != null)
                EditorGUILayout.PropertyField(_tweenerProp);
            if (_interactionHandProp != null)
                EditorGUILayout.PropertyField(_interactionHandProp);
            if (_selectionButtonProp != null)
                EditorGUILayout.PropertyField(_selectionButtonProp);
            // Events foldout
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
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
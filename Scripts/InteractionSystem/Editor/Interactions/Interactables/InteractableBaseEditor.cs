using UnityEngine;
using UnityEditor;

namespace Shababeek.Interactions.Editors
{

    public abstract class InteractableBaseEditor : Editor
    {
        // Common serialized properties for all interactables
        private SerializedProperty _interactionHandProp;
        private SerializedProperty _selectionButtonProp;

        // Common event properties
        private SerializedProperty _onSelectedProp;
        private SerializedProperty _onDeselectedProp;
        private SerializedProperty _onHoverStartProp;
        private SerializedProperty _onHoverEndProp;
        private SerializedProperty _onUseStartedProp;
        private SerializedProperty _onUseEndedProp;
        private SerializedProperty _onThumbPressedProp;
        private SerializedProperty _onThumbReleasedProp;

        // Common read-only properties
        private SerializedProperty _isSelectedProp;
        private SerializedProperty _currentInteractorProp;
        private SerializedProperty _currentStateProp;

        // UI state
        private bool _showEvents = true;
        private bool _showDebug = true;

        protected virtual void OnEnable()
        {
            // Find common serialized properties
            _interactionHandProp = serializedObject.FindProperty("interactionHand");
            _selectionButtonProp = serializedObject.FindProperty("selectionButton");
            
            _onSelectedProp = serializedObject.FindProperty("onSelected");
            _onDeselectedProp = serializedObject.FindProperty("onDeselected");
            _onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            _onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            _onUseStartedProp = serializedObject.FindProperty("onUseStarted");
            _onUseEndedProp = serializedObject.FindProperty("onUseEnded");
            _onThumbPressedProp = serializedObject.FindProperty("onThumbPressed");
            _onThumbReleasedProp = serializedObject.FindProperty("onThumbReleased");

            _isSelectedProp = serializedObject.FindProperty("isSelected");
            _currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            _currentStateProp = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawCustomHeader();
            EditorGUILayout.Space();
            DrawImportantSettings();
            DrawInteractionSettings();
            EditorGUILayout.Space();
            DrawCustomProperties();
            EditorGUILayout.Space();
            DrawEvents();
            EditorGUILayout.Space();

            DrawCommonDebugInfo();
            DrawCustomDebugInfo();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the custom header/info section for the interactable.
        /// Override this method to provide specific information about the interactable type.
        /// </summary>
        protected virtual void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "This is an interactable object that can be interacted with by interactors.",
                MessageType.Info);
        }

        /// <summary>
        /// Draws the common interaction settings section.
        /// </summary>
        protected virtual void DrawInteractionSettings()
        {
            EditorGUILayout.LabelField("Interaction Settings", EditorStyles.boldLabel);

            if (_interactionHandProp != null)
            {
                EditorGUILayout.PropertyField(_interactionHandProp, new GUIContent("Interaction Hand", "Specifies which hands can interact with this object (Left, Right, or Both)."));
            }

            if (_selectionButtonProp != null)
            {
                EditorGUILayout.PropertyField(_selectionButtonProp, new GUIContent("Selection Button", "The button that triggers selection of this interactable (Grip or Trigger)."));
            }
        }

        /// <summary>
        /// Draws custom properties specific to the derived interactable type.
        /// Override this method to add custom property fields.
        /// </summary>
        protected virtual void DrawCustomProperties()
        {
            // Override in derived classes to add custom properties
        }

        protected virtual void DrawEvents()
        {
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                DrawCustomEvents();
                DrawBaseEvents();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawBaseEvents()
        {
            if (_onSelectedProp != null)
                EditorGUILayout.PropertyField(_onSelectedProp);

            if (_onDeselectedProp != null)
                EditorGUILayout.PropertyField(_onDeselectedProp);

            if (_onHoverStartProp != null)
                EditorGUILayout.PropertyField(_onHoverStartProp);

            if (_onHoverEndProp != null)
                EditorGUILayout.PropertyField(_onHoverEndProp);

            if (_onUseStartedProp != null)
                EditorGUILayout.PropertyField(_onUseStartedProp);

            if (_onUseEndedProp != null)
                EditorGUILayout.PropertyField(_onUseEndedProp);

            if (_onThumbPressedProp != null)
                EditorGUILayout.PropertyField(_onThumbPressedProp);

            if (_onThumbReleasedProp != null)
                EditorGUILayout.PropertyField(_onThumbReleasedProp);
        }

        protected abstract void DrawCustomEvents();

        protected virtual void DrawCommonDebugInfo()
        {
            _showDebug = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebug, "Debug Information");
            if (_showDebug)
            {
                EditorGUI.BeginDisabledGroup(true);

                if (_isSelectedProp != null)
                    EditorGUILayout.PropertyField(_isSelectedProp, new GUIContent("Is Selected", "Indicates whether this interactable is currently selected."));

                if (_currentInteractorProp != null)
                    EditorGUILayout.PropertyField(_currentInteractorProp, new GUIContent("Current Interactor", "The interactor that is currently interacting with this object."));

                if (_currentStateProp != null)
                    EditorGUILayout.PropertyField(_currentStateProp, new GUIContent("Current State", "The current interaction state of this interactable."));

                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Draws custom debug information specific to the derived interactable type.
        /// Override this method to add custom debug fields.
        /// </summary>
        protected virtual void DrawCustomDebugInfo()
        {
            // Override in derived classes to add custom debug information
        }

        /// <summary>
        /// Helper method to safely find and display a property field.
        /// </summary>
        /// <param name="property">The serialized property to display</param>
        /// <param name="label">The label for the property field</param>
        /// <param name="tooltip">Optional tooltip for the property</param>
        protected void SafePropertyField(SerializedProperty property, string label, string tooltip = null)
        {
            if (property != null)
            {
                var content = tooltip != null ? new GUIContent(label, tooltip) : new GUIContent(label);
                EditorGUILayout.PropertyField(property, content);
            }
        }

        /// <summary>
        /// Helper method to safely find and display a property field with custom content.
        /// </summary>
        /// <param name="property">The serialized property to display</param>
        /// <param name="content">The GUIContent for the property field</param>
        protected void SafePropertyField(SerializedProperty property, GUIContent content)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, content);
            }
        }

        protected virtual void DrawImportantSettings()
        {
            // Override in derived classes to add important settings
        }
    }
}

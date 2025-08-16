using UnityEngine;
using UnityEditor;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Base editor class for all interactable objects.
    /// Provides common functionality for displaying interaction settings, events, and debug information.
    /// </summary>
    public abstract class InteractableBaseEditor : Editor
    {
        // Common serialized properties for all interactables
        protected SerializedProperty _interactionHandProp;
        protected SerializedProperty _selectionButtonProp;

        // Common event properties
        protected SerializedProperty _onSelectedProp;
        protected SerializedProperty _onDeselectedProp;
        protected SerializedProperty _onHoverStartProp;
        protected SerializedProperty _onHoverEndProp;
        protected SerializedProperty _onUseStartedProp;
        protected SerializedProperty _onUseEndedProp;

        // Common read-only properties
        protected SerializedProperty _isSelectedProp;
        protected SerializedProperty _currentInteractorProp;
        protected SerializedProperty _currentStateProp;

        // UI state
        protected bool _showEvents = true;
        protected bool _showDebug = true;

        protected virtual void OnEnable()
        {
            // Find common serialized properties
            _interactionHandProp = serializedObject.FindProperty("interactionHand");
            _selectionButtonProp = serializedObject.FindProperty("selectionButton");

            // Find common event properties
            _onSelectedProp = serializedObject.FindProperty("onSelected");
            _onDeselectedProp = serializedObject.FindProperty("onDeselected");
            _onHoverStartProp = serializedObject.FindProperty("onHoverStart");
            _onHoverEndProp = serializedObject.FindProperty("onHoverEnd");
            _onUseStartedProp = serializedObject.FindProperty("onUseStarted");
            _onUseEndedProp = serializedObject.FindProperty("onUseEnded");

            // Find common read-only properties
            _isSelectedProp = serializedObject.FindProperty("isSelected");
            _currentInteractorProp = serializedObject.FindProperty("currentInteractor");
            _currentStateProp = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // Draw custom header/info
            DrawCustomHeader();
            EditorGUILayout.Space();
            DrawImportantSettings();
            // Draw interaction settings
            DrawInteractionSettings();

            EditorGUILayout.Space();

            // Draw custom properties (implemented by derived classes)
            DrawCustomProperties();

            EditorGUILayout.Space();

            // Draw common events
            DrawCommonEvents();

            EditorGUILayout.Space();

            // Draw common debug information
            DrawCommonDebugInfo();

            // Draw custom debug info (implemented by derived classes)
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

        /// <summary>
        /// Draws the common events section.
        /// </summary>
        protected virtual void DrawCommonEvents()
        {
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                if (_onSelectedProp != null)
                    EditorGUILayout.PropertyField(_onSelectedProp, new GUIContent("On Selected", "Event raised when this interactable is selected by an interactor."));

                if (_onDeselectedProp != null)
                    EditorGUILayout.PropertyField(_onDeselectedProp, new GUIContent("On Deselected", "Event raised when this interactable is deselected by an interactor."));

                if (_onHoverStartProp != null)
                    EditorGUILayout.PropertyField(_onHoverStartProp, new GUIContent("On Hover Start", "Event raised when an interactor starts hovering over this interactable."));

                if (_onHoverEndProp != null)
                    EditorGUILayout.PropertyField(_onHoverEndProp, new GUIContent("On Hover End", "Event raised when an interactor stops hovering over this interactable."));

                if (_onUseStartedProp != null)
                    EditorGUILayout.PropertyField(_onUseStartedProp, new GUIContent("On Use Started", "Event raised when the secondary button is pressed while selected."));

                if (_onUseEndedProp != null)
                    EditorGUILayout.PropertyField(_onUseEndedProp, new GUIContent("On Use Ended", "Event raised when the secondary button is released while selected."));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Draws the common debug information section.
        /// </summary>
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

using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for the WheelInteractable component with enhanced visualization and interactive editing.
    /// </summary>
    [CustomEditor(typeof(WheelInteractable))]
    [CanEditMultipleObjects]
    public class WheelInteractableEditor : Editor
    {
        private WheelInteractable _wheelComponent;

        private SerializedProperty _rotationAxis;

        private SerializedProperty _returnToStart;
        private SerializedProperty _returnSpeed;
        
        private SerializedProperty _interactionHand;
        private SerializedProperty _selectionButton;
        private SerializedProperty _interactableObject;

        // Events
        private SerializedProperty _onWheelAngleChanged;
        private SerializedProperty _onWheelRotated;
        private SerializedProperty _onSelected;
        private SerializedProperty _onDeselected;
        private SerializedProperty _onHoverStart;
        private SerializedProperty _onHoverEnd;
        private SerializedProperty _onActivated;

        // Debug
        private SerializedProperty _currentAngle;
        private SerializedProperty _currentRotation;
        private SerializedProperty _isSelected;
        private SerializedProperty _currentInteractor;
        private SerializedProperty _currentState;

        private bool _showEvents = true;
        private void OnEnable()
        {
            _wheelComponent = (WheelInteractable)target;

            _rotationAxis = serializedObject.FindProperty("rotationAxis");

            _returnToStart = serializedObject.FindProperty("returnToStart");
            _returnSpeed = serializedObject.FindProperty("returnSpeed");

            _interactionHand = serializedObject.FindProperty("interactionHand");
            _selectionButton = serializedObject.FindProperty("selectionButton");
            _interactableObject = serializedObject.FindProperty("interactableObject");

            _onWheelAngleChanged = serializedObject.FindProperty("onWheelAngleChanged");
            _onWheelRotated = serializedObject.FindProperty("onWheelRotated");
            _onSelected = serializedObject.FindProperty("onSelected");
            _onDeselected = serializedObject.FindProperty("onDeselected");
            _onHoverStart = serializedObject.FindProperty("onHoverStart");
            _onHoverEnd = serializedObject.FindProperty("onHoverEnd");
            _onActivated = serializedObject.FindProperty("onActivated");

            _currentAngle = serializedObject.FindProperty("currentAngle");
            _currentRotation = serializedObject.FindProperty("currentRotation");
            _isSelected = serializedObject.FindProperty("isSelected");
            _currentInteractor = serializedObject.FindProperty("currentInteractor");
            _currentState = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.HelpBox(
                "Configure the wheel rotation axis. Visual gizmos will show in the scene view.",
                MessageType.Info);
            
            // Rotation limits disabled for now
            //DrawPresets();

            EditorGUILayout.Space();

            // Base Interaction Settings
            EditorGUILayout.LabelField("Interaction Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_interactionHand);
            EditorGUILayout.PropertyField(_selectionButton);
            EditorGUILayout.PropertyField(_interactableObject);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_rotationAxis);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_returnToStart);
            if (_returnToStart.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_returnSpeed);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            
            

            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                DrawEventsInspector();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_currentAngle);
            EditorGUILayout.PropertyField(_currentRotation);
            EditorGUILayout.PropertyField(_isSelected);
            EditorGUILayout.PropertyField(_currentInteractor);
            EditorGUILayout.PropertyField(_currentState);
            GUI.enabled = true;

            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Wheel"))
                {
                    _wheelComponent.ResetWheel();
                }

                if (GUILayout.Button("Set to 0°"))
                {
                    _wheelComponent.SetWheelAngle(0);
                }

                EditorGUILayout.EndHorizontal();

                // Angle slider
                var currentAngle = _wheelComponent.CurrentAngle;
                var newAngle = EditorGUILayout.Slider("Set Angle", currentAngle, -720f, 720f);
                if (!Mathf.Approximately(newAngle, currentAngle))
                {
                    _wheelComponent.SetWheelAngle(newAngle);
                }
            }

            // Visual Guide
            DrawVisualGuide();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEventsInspector()
        {
            EditorGUILayout.PropertyField(_onWheelAngleChanged);
            EditorGUILayout.PropertyField(_onWheelRotated);
            EditorGUILayout.PropertyField(_onSelected);
            EditorGUILayout.PropertyField(_onDeselected);
            EditorGUILayout.PropertyField(_onHoverStart);
            EditorGUILayout.PropertyField(_onHoverEnd);
            EditorGUILayout.PropertyField(_onActivated);
        }


        /// <summary>
        /// Draws quick preset buttons for common wheel configurations.
        /// </summary>
        private void DrawPresets()
        {
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Steering Wheel\n(spring back)"))
            {
                _returnToStart.boolValue = true;
                _returnSpeed.floatValue = 180f;
            }

            if (GUILayout.Button("Control Knob\n(stays in place)"))
            {
                _returnToStart.boolValue = false;
                _returnSpeed.floatValue = 90f;
            }

            if (GUILayout.Button("Valve Wheel\n(free rotation)"))
            {
                _returnToStart.boolValue = false;
                _returnSpeed.floatValue = 90f;
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a visual guide showing the gizmo colors and meanings.
        /// </summary>
        private void DrawVisualGuide()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene View Visual Guide", EditorStyles.boldLabel);

            var originalColor = GUI.color;

            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("█", GUILayout.Width(12));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Rotation Axis", GUILayout.Width(80));

            GUI.color = Color.cyan;
            EditorGUILayout.LabelField("█", GUILayout.Width(12));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Reference Direction", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.green;
            EditorGUILayout.LabelField("█", GUILayout.Width(12));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Current Rotation", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            GUI.color = originalColor;
        }

        /// <summary>
        /// Draws handles in the scene view for interactive configuration.
        /// </summary>
        private void OnSceneGUI()
        {
            // TODO: Limits visualization and editing
            return;
            if (_wheelComponent == null || _wheelComponent.InteractableObject == null) return;

            var wheelTransform = _wheelComponent.InteractableObject.transform;
            var position = wheelTransform.position;
            var rotationAxisVector = GetRotationAxisVector();
            var referenceVector = GetReferenceVector();
            var radius = HandleUtility.GetHandleSize(position) * 1.2f;

            return;
            
        }

        /// <summary>
        /// Gets the rotation axis vector based on the configured rotation axis.
        /// </summary>
        private Vector3 GetRotationAxisVector()
        {
            var axis = (Axis)_rotationAxis.enumValueIndex;
            return axis switch
            {
                Axis.X => _wheelComponent.InteractableObject.transform.right,
                Axis.Y => _wheelComponent.InteractableObject.transform.up,
                Axis.Z => _wheelComponent.InteractableObject.transform.forward,
                _ => _wheelComponent.InteractableObject.transform.forward
            };
        }

        /// <summary>
        /// Gets the reference vector based on the configured rotation axis.
        /// </summary>
        private Vector3 GetReferenceVector()
        {
            var axis = (Axis)_rotationAxis.enumValueIndex;
            return axis switch
            {
                Axis.X => _wheelComponent.InteractableObject.transform.forward,
                Axis.Y => _wheelComponent.InteractableObject.transform.forward,
                Axis.Z => _wheelComponent.InteractableObject.transform.up,
                _ => _wheelComponent.InteractableObject.transform.up
            };
        }
    }
}
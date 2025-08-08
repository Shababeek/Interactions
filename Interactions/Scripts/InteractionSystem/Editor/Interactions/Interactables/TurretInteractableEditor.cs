using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for the TurretInteractable component with enhanced visualization and interactive editing.
    /// </summary>
    [CustomEditor(typeof(JoystickInteractable))]
    [CanEditMultipleObjects]
    public class JoystickInteractableEditor : Editor
    {
        private JoystickInteractable _joystickComponent;
        
        // Rotation Limits
        private SerializedProperty _limitXRotation;
        private SerializedProperty _minXAngle;
        private SerializedProperty _maxXAngle;
        private SerializedProperty _limitZRotation;
        private SerializedProperty _minZAngle;
        private SerializedProperty _maxZAngle;
            
        // Return Behavior
        private SerializedProperty _returnToOriginal;
        private SerializedProperty _returnSpeed;
        
        private SerializedProperty _interactionHand;
        private SerializedProperty _selectionButton;
        private SerializedProperty _interactableObject;
        
        // Events
        private SerializedProperty _onRotationChanged;
        private SerializedProperty _onXRotationChanged;
        private SerializedProperty _onZRotationChanged;
        private SerializedProperty _onSelected;
        private SerializedProperty _onDeselected;
        private SerializedProperty _onHoverStart;
        private SerializedProperty _onHoverEnd;
        private SerializedProperty _onActivated;
        
        // Debug
        private SerializedProperty _currentRotation;
        private SerializedProperty _normalizedRotation;
        private SerializedProperty _isSelected;
        private SerializedProperty _currentInteractor;
        private SerializedProperty _currentState;

        private bool _showEvents = true;
        private bool _showRotationLimits = true;
        private bool _showMovementSettings = true;
        private static bool _editRotationLimits = false;

        private void OnEnable()
        {
            _joystickComponent = (JoystickInteractable)target;
            
            // Rotation Limits
            _limitXRotation = serializedObject.FindProperty("limitXRotation");
            _minXAngle = serializedObject.FindProperty("minXAngle");
            _maxXAngle = serializedObject.FindProperty("maxXAngle");
            _limitZRotation = serializedObject.FindProperty("limitZRotation");
            _minZAngle = serializedObject.FindProperty("minZAngle");
            _maxZAngle = serializedObject.FindProperty("maxZAngle");

            _returnToOriginal = serializedObject.FindProperty("returnToOriginal");
            _returnSpeed = serializedObject.FindProperty("returnSpeed");
            
            _interactionHand = serializedObject.FindProperty("interactionHand");
            _selectionButton = serializedObject.FindProperty("selectionButton");
            _interactableObject = serializedObject.FindProperty("interactableObject");
            
            _onRotationChanged = serializedObject.FindProperty("onRotationChanged");
            _onXRotationChanged = serializedObject.FindProperty("onXRotationChanged");
            _onZRotationChanged = serializedObject.FindProperty("onZRotationChanged");
            _onSelected = serializedObject.FindProperty("onSelected");
            _onDeselected = serializedObject.FindProperty("onDeselected");
            _onHoverStart = serializedObject.FindProperty("onHoverStart");
            _onHoverEnd = serializedObject.FindProperty("onHoverEnd");
            _onActivated = serializedObject.FindProperty("onActivated");
            
            _currentRotation = serializedObject.FindProperty("currentRotation");
            _normalizedRotation = serializedObject.FindProperty("normalizedRotation");
            _isSelected = serializedObject.FindProperty("isSelected");
            _currentInteractor = serializedObject.FindProperty("currentInteractor");
            _currentState = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Header
            EditorGUILayout.HelpBox(
                "Turret-style interactable with X (pitch) and Z (roll) axis rotation.\n\n" +
                "Hand Movement Mapping:\n" +
                "• Hand up/down → X-axis rotation (pitch)\n" +
                "• Hand left/right → Z-axis rotation (roll)\n\n" +
                "Use the edit button to interactively adjust rotation limits in the scene view.",
                MessageType.Info);
            
            // Edit button
            DrawEditButton();
            
            // Quick Presets
            DrawPresets();
            
            EditorGUILayout.Space();
            
            // Base Interaction Settings
            EditorGUILayout.LabelField("Interaction Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_interactionHand);
            EditorGUILayout.PropertyField(_selectionButton);
            EditorGUILayout.PropertyField(_interactableObject);
            
            EditorGUILayout.Space();
            
            // Rotation Limits Section
            _showRotationLimits = EditorGUILayout.BeginFoldoutHeaderGroup(_showRotationLimits, "Rotation Limits");
            if (_showRotationLimits)
            {
                EditorGUI.indentLevel++;
                
                // X Rotation (Pitch)
                EditorGUILayout.PropertyField(_limitXRotation, new GUIContent("Limit X Rotation (Pitch)"));
                if (_limitXRotation.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(_minXAngle, new GUIContent("Min Pitch (°)", "Minimum pitch angle (down)"));
                    EditorGUILayout.PropertyField(_maxXAngle, new GUIContent("Max Pitch (°)", "Maximum pitch angle (up)"));
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(5);
                
                // Z Rotation (Roll)
                EditorGUILayout.PropertyField(_limitZRotation, new GUIContent("Limit Z Rotation (Roll)"));
                if (_limitZRotation.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(_minZAngle, new GUIContent("Min Roll (°)", "Minimum roll angle (left)"));
                    EditorGUILayout.PropertyField(_maxZAngle, new GUIContent("Max Roll (°)", "Maximum roll angle (right)"));
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Return Behavior", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_returnToOriginal, new GUIContent("Return to Original"));
            if (_returnToOriginal.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_returnSpeed, new GUIContent("Return Speed"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                EditorGUILayout.PropertyField(_onRotationChanged, new GUIContent("On Rotation Changed (Vector2)"));
                EditorGUILayout.PropertyField(_onXRotationChanged, new GUIContent("On X Rotation Changed (Pitch)"));
                EditorGUILayout.PropertyField(_onZRotationChanged, new GUIContent("On Z Rotation Changed (Roll)"));
                EditorGUILayout.PropertyField(_onSelected);
                EditorGUILayout.PropertyField(_onDeselected);
                EditorGUILayout.PropertyField(_onHoverStart);
                EditorGUILayout.PropertyField(_onHoverEnd);
                EditorGUILayout.PropertyField(_onActivated);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.Space();
            
            // Debug section
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_currentRotation, new GUIContent("Current Rotation (X=pitch, Z=roll)"));
            EditorGUILayout.PropertyField(_normalizedRotation, new GUIContent("Normalized Rotation"));
            EditorGUILayout.PropertyField(_isSelected);
            EditorGUILayout.PropertyField(_currentInteractor);
            EditorGUILayout.PropertyField(_currentState);
            GUI.enabled = true;
            
            // Runtime controls
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset to Original"))
                {
                    _joystickComponent.ResetToOriginal();
                }
                if (GUILayout.Button("Set to Center"))
                {
                    _joystickComponent.SetRotation(0, 0);
                }
                EditorGUILayout.EndHorizontal();
                
                // Rotation sliders
                var currentRot = _joystickComponent.CurrentRotation;
                float minX = _joystickComponent.LimitXRotation ? _joystickComponent.MinXAngle : -90f;
                float maxX = _joystickComponent.LimitXRotation ? _joystickComponent.MaxXAngle : 90f;
                float minZ = _joystickComponent.LimitZRotation ? _joystickComponent.MinZAngle : -90f;
                float maxZ = _joystickComponent.LimitZRotation ? _joystickComponent.MaxZAngle : 90f;
                
                float newX = EditorGUILayout.Slider("Set X Rotation (Pitch)", currentRot.x, minX, maxX);
                float newZ = EditorGUILayout.Slider("Set Z Rotation (Roll)", currentRot.y, minZ, maxZ);
                
                if (!Mathf.Approximately(newX, currentRot.x) || !Mathf.Approximately(newZ, currentRot.y))
                {
                    _joystickComponent.SetRotation(newX, newZ);
                }
            }
            
            // Visual Guide
            DrawVisualGuide();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(_editRotationLimits ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prevColor = GUI.color;
            if (_editRotationLimits)
                GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                _editRotationLimits = !_editRotationLimits;
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Draws quick preset buttons for common turret configurations.
        /// </summary>
        private void DrawPresets()
        {
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Security Camera\n(±30°, spring back)"))
            {
                _limitXRotation.boolValue = true;
                _minXAngle.floatValue = -30f;
                _maxXAngle.floatValue = 30f;
                _limitZRotation.boolValue = true;
                _minZAngle.floatValue = -30f;
                _maxZAngle.floatValue = 30f;
                _returnToOriginal.boolValue = true;
                _returnSpeed.floatValue = 3f;
            }
            
            if (GUILayout.Button("Gun Turret\n(±45°, responsive)"))
            {
                _limitXRotation.boolValue = true;
                _minXAngle.floatValue = -45f;
                _maxXAngle.floatValue = 45f;
                _limitZRotation.boolValue = true;
                _minZAngle.floatValue = -45f;
                _maxZAngle.floatValue = 45f;
                _returnToOriginal.boolValue = false;
                _returnSpeed.floatValue = 5f;
            }
            
            if (GUILayout.Button("Radar Dish\n(±60°, smooth)"))
            {
                _limitXRotation.boolValue = true;
                _minXAngle.floatValue = -60f;
                _maxXAngle.floatValue = 60f;
                _limitZRotation.boolValue = true;
                _minZAngle.floatValue = -60f;
                _maxZAngle.floatValue = 60f;
                _returnToOriginal.boolValue = false;
                _returnSpeed.floatValue = 2f;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Joystick\n(±30°, spring back)"))
            {
                _limitXRotation.boolValue = true;
                _minXAngle.floatValue = -30f;
                _maxXAngle.floatValue = 30f;
                _limitZRotation.boolValue = true;
                _minZAngle.floatValue = -30f;
                _maxZAngle.floatValue = 30f;
                _returnToOriginal.boolValue = true;
                _returnSpeed.floatValue = 8f;
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
            GUI.color = Color.red;
            EditorGUILayout.LabelField("█", GUILayout.Width(12));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("X-Axis Limits (Pitch)", GUILayout.Width(120));
            
            GUI.color = Color.blue;
            EditorGUILayout.LabelField("█", GUILayout.Width(12));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Z-Axis Limits (Roll)", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("█", GUILayout.Width(12));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Turret Center", GUILayout.Width(120));
            
            GUI.color = Color.green;
            EditorGUILayout.LabelField("█", GUILayout.Width(12));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Current Direction", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(1f, 0.5f, 0f);
            EditorGUILayout.LabelField("█", GUILayout.Width(12));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Dead Zone", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            GUI.color = originalColor;
        }

        /// <summary>
        /// Draws handles in the scene view for interactive configuration.
        /// </summary>
        private void OnSceneGUI()
        {
            if (_joystickComponent == null || _joystickComponent.InteractableObject == null) return;
            
            var turretTransform = _joystickComponent.InteractableObject.transform;
            var position = turretTransform.position;
            var radius = HandleUtility.GetHandleSize(position) * 1.2f;
            
            // Draw rotation limit visualization
            if (_editRotationLimits)
            {
                DrawInteractiveRotationLimits(position, radius);
            }
        }

        private void DrawInteractiveRotationLimits(Vector3 position, float radius)
        {
            // Draw X rotation limits (pitch) - Red
            if (_joystickComponent.LimitXRotation)
            {
                DrawAxisLimits(position, radius, _joystickComponent.transform.right, _joystickComponent.transform.forward,
                    _minXAngle, _maxXAngle, Color.red, "X (Pitch)");
            }
            
            // Draw Z rotation limits (roll) - Blue  
            if (_joystickComponent.LimitZRotation)
            {
                DrawAxisLimits(position, radius, _joystickComponent.transform.forward, _joystickComponent.transform.up,
                    _minZAngle, _maxZAngle, Color.blue, "Z (Roll)");
            }
        }

        private void DrawAxisLimits(Vector3 position, float radius, Vector3 axis, Vector3 reference, 
            SerializedProperty minAngleProp, SerializedProperty maxAngleProp, Color color, string axisName)
        {
            var minAngle = minAngleProp.floatValue;
            var maxAngle = maxAngleProp.floatValue;
            
            var minRotation = Quaternion.AngleAxis(minAngle, axis);
            var maxRotation = Quaternion.AngleAxis(maxAngle, axis);
            var minDir = minRotation * reference;
            var maxDir = maxRotation * reference;
            var minPos = position + minDir * radius;
            var maxPos = position + maxDir * radius;
            
            Handles.color = new Color(color.r, color.g, color.b, 0.2f);
            Handles.DrawSolidArc(position, axis, minDir, maxAngle - minAngle, radius);
            Handles.color = color;
            Handles.DrawWireArc(position, axis, minDir, maxAngle - minAngle, radius);
            Handles.Label(minPos + minDir * 0.1f, $"Min {axisName} ({minAngle:F1}°)");
            Handles.Label(maxPos + maxDir * 0.1f, $"Max {axisName} ({maxAngle:F1}°)");
            
            Undo.RecordObject(_joystickComponent, $"Edit {axisName} Rotation Limits");
            
            Handles.color = color;
            EditorGUI.BeginChangeCheck();
            var newMinPos = Handles.FreeMoveHandle(minPos, HandleUtility.GetHandleSize(minPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                var from = reference;
                var to = (newMinPos - position).normalized;
                var newMinAngle = Vector3.SignedAngle(from, to, axis);
                minAngleProp.floatValue = Mathf.Clamp(newMinAngle, -90f, maxAngleProp.floatValue - 1f);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_joystickComponent);
            }
            
            EditorGUI.BeginChangeCheck();
            var newMaxPos = Handles.FreeMoveHandle(maxPos, HandleUtility.GetHandleSize(maxPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                var from = reference;
                var to = (newMaxPos - position).normalized;
                var newMaxAngle = Vector3.SignedAngle(from, to, axis);
                maxAngleProp.floatValue = Mathf.Clamp(newMaxAngle, minAngleProp.floatValue + 1f, 90f);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_joystickComponent);
            }
            
            Handles.color = Color.white;
        }
    }
} 
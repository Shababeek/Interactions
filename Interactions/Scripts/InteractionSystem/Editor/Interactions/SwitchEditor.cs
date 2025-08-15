using UnityEngine;
using UnityEditor;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for the Switch component with enhanced visualization and presets.
    /// </summary>
    [CustomEditor(typeof(Switch))]
    public class SwitchEditor : Editor
    {
        private Switch switchComponent;
        
        // Serialized properties
        private SerializedProperty _onUp;
        private SerializedProperty _onDown;
        private SerializedProperty _onHold;
        private SerializedProperty _switchBody;
        private SerializedProperty _rotationAxis;
        private SerializedProperty _detectionAxis;
        private SerializedProperty _upRotation;
        private SerializedProperty _downRotation;
        private SerializedProperty _rotateSpeed;
        private SerializedProperty _angleThreshold;
        private SerializedProperty _stayInPosition;
        private SerializedProperty _startingPosition;
        private SerializedProperty _direction;
        
        // UI state
        private bool _showEvents = true;
        private static bool _editSwitchRange = false;
        
        private void OnEnable()
        {
            switchComponent = (Switch)target;
            
            // Find serialized properties
            _onUp = serializedObject.FindProperty("onUp");
            _onDown = serializedObject.FindProperty("onDown");
            _onHold = serializedObject.FindProperty("onHold");
            _switchBody = serializedObject.FindProperty("switchBody");
            _rotationAxis = serializedObject.FindProperty("rotationAxis");
            _detectionAxis = serializedObject.FindProperty("detectionAxis");
            _upRotation = serializedObject.FindProperty("upRotation");
            _downRotation = serializedObject.FindProperty("downRotation");
            _rotateSpeed = serializedObject.FindProperty("rotateSpeed");
            _angleThreshold = serializedObject.FindProperty("angleThreshold");
            _stayInPosition = serializedObject.FindProperty("stayInPosition");
            _startingPosition = serializedObject.FindProperty("startingPosition");
            _direction = serializedObject.FindProperty("direction");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Check for property changes to update switch body rotation
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.HelpBox(
                "Configure the switch rotation and detection axes. Visual gizmos will show in the scene view.\n\n" +
                "Use the edit button to interactively adjust rotation angles in the scene view.",
                MessageType.Info);
            DrawEditButton();
            DrawPresets();
            
            EditorGUILayout.Space();
            #region SwitchAxe

            
            EditorGUILayout.LabelField("Switch Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_switchBody);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_rotationAxis, new GUIContent("Rotation Axis", "The axis around which the switch rotates"));
            EditorGUILayout.PropertyField(_detectionAxis, new GUIContent("Detection Axis", "The axis used to detect interaction direction"));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_upRotation, new GUIContent("Up Rotation (°)", "Rotation angle for up position"));
            EditorGUILayout.PropertyField(_downRotation, new GUIContent("Down Rotation (°)", "Rotation angle for down position"));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_rotateSpeed, new GUIContent("Rotate Speed", "Speed of rotation animation"));
            EditorGUILayout.PropertyField(_angleThreshold, new GUIContent("Angle Threshold (°)", "Minimum angle for direction detection"));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(_stayInPosition, new GUIContent("Stay In Position", "When enabled, the switch will stay in its current position instead of returning to neutral when the trigger exits"));
            
            EditorGUILayout.PropertyField(_startingPosition, new GUIContent("Starting Position", "The starting position of the switch when the scene starts"));
            #endregion
            EditorGUILayout.Space();
            #region eventFoldOut
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                EditorGUILayout.PropertyField(_onUp);
                EditorGUILayout.PropertyField(_onDown);
                EditorGUILayout.PropertyField(_onHold);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_direction);
            GUI.enabled = true;
            
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Switch"))
                {
                    switchComponent.ResetSwitch();
                }
                
                if (GUILayout.Button("Force Reset Switch"))
                {
                    switchComponent.ForceResetSwitch();
                }
                
                var state = switchComponent.GetSwitchState();
                var stateText = state switch
                {
                    true => "UP",
                    false => "DOWN", 
                    null => "NEUTRAL"
                };
                EditorGUILayout.LabelField($"Current State: {stateText}");
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox(
                    "Reset Switch: Respects the 'Stay In Position' setting\n" +
                    "Force Reset Switch: Always resets to neutral regardless of settings",
                    MessageType.Info);
            }
            
            // Visual Guide
            EditorGUILayout.Space();
            DrawVisualGuide();
            
            // Check if any properties changed and update switch body rotation if needed
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UpdateSwitchBodyRotation(_upRotation.floatValue, _downRotation.floatValue);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        

        private void DrawEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(_editSwitchRange ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prevColor = GUI.color;
            if (_editSwitchRange)
                GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                _editSwitchRange = !_editSwitchRange;
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }
        
    
        private void DrawPresets()
        {
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
                         if (GUILayout.Button("Wall Switch\n(Z-axis rotation)"))
             {
                 _rotationAxis.enumValueIndex = (int)Axis.Z;
                 _detectionAxis.enumValueIndex = (int)Axis.X;
                 _upRotation.floatValue = 20f;
                 _downRotation.floatValue = -20f;
             }
             
             if (GUILayout.Button("Panel Toggle\n(Y-axis rotation)"))
             {
                 _rotationAxis.enumValueIndex = (int)Axis.Y;
                 _detectionAxis.enumValueIndex = (int)Axis.X;
                 _upRotation.floatValue = 30f;
                 _downRotation.floatValue = -30f;
             }
             
             if (GUILayout.Button("Lever Switch\n(X-axis rotation)"))
             {
                 _rotationAxis.enumValueIndex = (int)Axis.X;
                 _detectionAxis.enumValueIndex = (int)Axis.Z;
                 _upRotation.floatValue = 45f;
                 _downRotation.floatValue = -45f;
             }
            
            EditorGUILayout.EndHorizontal();
        }
        

        private void DrawVisualGuide()
        {
            EditorGUILayout.LabelField("Scene View Gizmos", EditorStyles.boldLabel);
            
            var originalColor = GUI.color;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Rotation axis
            GUI.color = Color.yellow;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("█", GUILayout.Width(20));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Yellow: Rotation Axis (switch pivots around this)");
            EditorGUILayout.EndHorizontal();
            
            // Detection direction
            GUI.color = Color.cyan;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("█", GUILayout.Width(20));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Cyan: Detection Direction (interaction detection)");
            EditorGUILayout.EndHorizontal();
            
            // Up rotation
            GUI.color = Color.green;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("█", GUILayout.Width(20));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Green: Up Position Range (when selected)");
            EditorGUILayout.EndHorizontal();
            
            // Down rotation
            GUI.color = Color.red;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("█", GUILayout.Width(20));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Red: Down Position Range (when selected)");
            EditorGUILayout.EndHorizontal();
            
            // Detection area
            GUI.color = new Color(1f, 0.5f, 0f, 1f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("█", GUILayout.Width(20));
            GUI.color = originalColor;
            EditorGUILayout.LabelField("Orange: Detection Area & Angle Threshold");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            GUI.color = originalColor;
        }
        private void OnSceneGUI()
        {
            if (switchComponent == null || switchComponent.SwitchBody == null) return;
            
            var switchTransform = switchComponent.SwitchBody;
            var position = switchTransform.position;
            var rotationAxisVector = GetRotationAxisVector();
            var detectionVector = GetDetectionVector();
            var radius = HandleUtility.GetHandleSize(position) * 0.8f;
            
            // Get current rotation values
            var upAngle = _upRotation.floatValue;
            var downAngle = _downRotation.floatValue;
            
            // Calculate world positions for up/down
            var upRotationQ = Quaternion.AngleAxis(upAngle, rotationAxisVector);
            var downRotationQ = Quaternion.AngleAxis(downAngle, rotationAxisVector);
            var upDir = upRotationQ * detectionVector;
            var downDir = downRotationQ * detectionVector;
            var upPos = position + upDir * radius;
            var downPos = position + downDir * radius;
            
            // Always draw arc for visual feedback
            Handles.color = new Color(0f, 1f, 0f, 0.2f);
            Handles.DrawSolidArc(position, rotationAxisVector, downDir, upAngle - downAngle, radius);
            Handles.color = Color.green;
            Handles.DrawWireArc(position, rotationAxisVector, downDir, upAngle - downAngle, radius);
            
            // Draw labels
            Handles.Label(upPos + upDir * 0.1f, $"UP ({upAngle:F1}°)");
            Handles.Label(downPos + downDir * 0.1f, $"DOWN ({downAngle:F1}°)");
            
            if (!_editSwitchRange) return;
            
            // Record undo for interactive editing
            Undo.RecordObject(switchComponent, "Edit Switch Angles");
            
            // Draw and move up handle (green circle)
            Handles.color = Color.green;
            EditorGUI.BeginChangeCheck();
            var newUpPos = Handles.FreeMoveHandle(upPos, HandleUtility.GetHandleSize(upPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                var from = detectionVector;
                var to = (newUpPos - position).normalized;
                var newUpAngle = Vector3.SignedAngle(from, to, rotationAxisVector);
                _upRotation.floatValue = Mathf.Clamp(newUpAngle, _downRotation.floatValue + 1f, 180f);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(switchComponent);
                
                // Update the switch body rotation to reflect the new angle
                UpdateSwitchBodyRotation(newUpAngle, _downRotation.floatValue);
            }
            
            // Draw and move down handle (red circle)
            Handles.color = Color.red;
            EditorGUI.BeginChangeCheck();
            var newDownPos = Handles.FreeMoveHandle(downPos, HandleUtility.GetHandleSize(downPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                var from = detectionVector;
                var to = (newDownPos - position).normalized;
                var newDownAngle = Vector3.SignedAngle(from, to, rotationAxisVector);
                _downRotation.floatValue = Mathf.Clamp(newDownAngle, -180f, _upRotation.floatValue - 1f);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(switchComponent);
                
                // Update the switch body rotation to reflect the new angle
                UpdateSwitchBodyRotation(_upRotation.floatValue, newDownAngle);
            }
            
            Handles.color = Color.white;
        }
        
        /// <summary>
        /// Updates the switch body rotation to reflect the current angle values.
        /// </summary>
        private void UpdateSwitchBodyRotation(float upAngle, float downAngle)
        {
            if (switchComponent == null || switchComponent.SwitchBody == null) return;
            
            // Get the current starting position to determine which angle to apply
            var startingPosition = (StartingPosition)_startingPosition.enumValueIndex;
            float targetAngle = 0;
            
            switch (startingPosition)
            {
                case StartingPosition.On:
                    targetAngle = upAngle;
                    break;
                case StartingPosition.Off:
                    targetAngle = downAngle;
                    break;
                case StartingPosition.Neutral:
                default:
                    targetAngle = (upAngle + downAngle) / 2f; // Calculate middle position
                    break;
            }
            
            // Apply the rotation to the switch body
            var currentRotation = switchComponent.SwitchBody.localRotation.eulerAngles;
            var rotationAxis = (Axis)_rotationAxis.enumValueIndex;
            
            var newRotation = rotationAxis switch
            {
                Axis.X => new Vector3(targetAngle, currentRotation.y, currentRotation.z),
                Axis.Y => new Vector3(currentRotation.x, targetAngle, currentRotation.z),
                Axis.Z => new Vector3(currentRotation.x, currentRotation.y, targetAngle),
                _ => new Vector3(currentRotation.x, currentRotation.y, targetAngle)
            };
            
            switchComponent.SwitchBody.localRotation = Quaternion.Euler(newRotation);
        }
                 private Vector3 GetRotationAxisVector()
         {
             var axis = (Axis)_rotationAxis.enumValueIndex;
             return axis switch
             {
                 Axis.X => switchComponent.SwitchBody.right,
                 Axis.Y => switchComponent.SwitchBody.up,
                 Axis.Z => switchComponent.SwitchBody.forward,
                 _ => switchComponent.SwitchBody.forward
             };
         }
        
                 private Vector3 GetDetectionVector()
         {
             var axis = (Axis)_detectionAxis.enumValueIndex;
             return axis switch
             {
                 Axis.X => switchComponent.SwitchBody.right,
                 Axis.Y => switchComponent.SwitchBody.up,
                 Axis.Z => switchComponent.SwitchBody.forward,
                 _ => switchComponent.SwitchBody.right
             };
         }
    }
}
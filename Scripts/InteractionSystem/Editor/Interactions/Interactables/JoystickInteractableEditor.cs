using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for the JoystickInteractable component with enhanced visualization and interactive editing.
    /// </summary>
    [CustomEditor(typeof(JoystickInteractable))]
    [CanEditMultipleObjects]
    public class JoystickInteractableEditor : InteractableBaseEditor
    {
        private JoystickInteractable _joystickComponent;
        private SerializedProperty _limitXRotationProp;
        private SerializedProperty _limitZRotationProp;
        private SerializedProperty _minXAngle;
        private SerializedProperty _maxXAngle;
        private SerializedProperty _minZAngle;
        private SerializedProperty _maxZAngle;
        private SerializedProperty _returnToOriginalProp;
        private SerializedProperty _returnSpeedProp;
        private SerializedProperty _dampingProp;
        private SerializedProperty _interactableObjectProp;
        private SerializedProperty _onJoystickChangedProp;
        private SerializedProperty _onJoystickReleasedProp;
        private SerializedProperty _currentXAngleProp;
        private SerializedProperty _currentZAngleProp;


        private static bool _editJoystickRange = false;

        private void OnEnable()
        {
            base.OnEnable();

            _joystickComponent = (JoystickInteractable)target;
            _limitXRotationProp = base.serializedObject.FindProperty("limitXRotation");
            _limitZRotationProp = base.serializedObject.FindProperty("limitZRotation");
            _minXAngle = base.serializedObject.FindProperty("minXAngle");
            _maxXAngle = base.serializedObject.FindProperty("maxXAngle");
            _minZAngle = base.serializedObject.FindProperty("minZAngle");
            _maxZAngle = base.serializedObject.FindProperty("maxZAngle");
            _returnToOriginalProp = base.serializedObject.FindProperty("returnToOriginal");
            _returnSpeedProp = base.serializedObject.FindProperty("returnSpeed");
            _dampingProp = base.serializedObject.FindProperty("damping");
            _interactableObjectProp = base.serializedObject.FindProperty("interactableObject");
            _onJoystickChangedProp = base.serializedObject.FindProperty("onJoystickChanged");
            _onJoystickReleasedProp = base.serializedObject.FindProperty("onJoystickReleased");
            _currentXAngleProp = base.serializedObject.FindProperty("currentXAngle");
            _currentZAngleProp = base.serializedObject.FindProperty("currentZAngle");
            _interactableObjectProp = base.serializedObject.FindProperty("interactableObject");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Turret-style interactable that allows constrained rotation around X (pitch) and Z (roll) axes. Provides smooth rotation control with configurable limits and return-to-original behavior.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Joystick Settings", EditorStyles.boldLabel);
            
            if (_interactableObjectProp != null)
                EditorGUILayout.PropertyField(_interactableObjectProp, new GUIContent("Interactable Object", "The visual part of the joystick object that rotates"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Rotation Limits", EditorStyles.boldLabel);
            
            if (_minXAngle != null && _maxXAngle != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_minXAngle, new GUIContent("X Min (°)"));
                EditorGUILayout.PropertyField(_maxXAngle, new GUIContent("X Max (°)"));
                EditorGUILayout.EndHorizontal();
            }
            
            if (_minZAngle != null && _maxZAngle != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_minZAngle, new GUIContent("Z Min (°)"));
                EditorGUILayout.PropertyField(_maxZAngle, new GUIContent("Z Max (°)"));
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Behavior", EditorStyles.boldLabel);
            
            if (_returnToOriginalProp != null)
                EditorGUILayout.PropertyField(_returnToOriginalProp, new GUIContent("Return To Original", "Whether the joystick returns to its original position when released"));
            
            if (_returnSpeedProp != null)
                EditorGUILayout.PropertyField(_returnSpeedProp, new GUIContent("Return Speed", "Speed of return animation (degrees per second)"));
            
            if (_dampingProp != null)
                EditorGUILayout.PropertyField(_dampingProp, new GUIContent("Damping", "Damping factor for smooth movement (0-1)"));
            
        }

           protected override void DrawImportantSettings()
        {
            DrawEditButton();
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Joystick Events", EditorStyles.boldLabel);
            
            if (_onJoystickChangedProp != null)
                EditorGUILayout.PropertyField(_onJoystickChangedProp, new GUIContent("On Joystick Changed", "Event raised when the joystick position changes"));
            
            if (_onJoystickReleasedProp != null)
                EditorGUILayout.PropertyField(_onJoystickReleasedProp, new GUIContent("On Joystick Released", "Event raised when the joystick is released"));
            
            EditorGUILayout.Space();

        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Joystick Debug", EditorStyles.boldLabel);
            
            if (_currentXAngleProp != null)
                EditorGUILayout.PropertyField(_currentXAngleProp, new GUIContent("Current X Angle", "Current X-axis rotation angle in degrees"));
            
            if (_currentZAngleProp != null)
                EditorGUILayout.PropertyField(_currentZAngleProp, new GUIContent("Current Z Angle", "Current Z-axis rotation angle in degrees"));
        }

        private void DrawEditButton()
        {
            EditorGUILayout.Space();
            var icon = EditorGUIUtility.IconContent(_editJoystickRange ? "d_EditCollider" : "EditCollider");
            var iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };
            Color prevColor = GUI.color;
            if (_editJoystickRange)
                GUI.color = Color.green;
            if (GUILayout.Button(icon, iconButtonStyle))
                _editJoystickRange = !_editJoystickRange;
            GUI.color = prevColor;
            EditorGUILayout.Space();
        }

        private void OnSceneGUI()
        {
            
            if (_joystickComponent == null || _joystickComponent.InteractableObject == null) return;
            
            var position = _joystickComponent.InteractableObject.position;
            var radius = HandleUtility.GetHandleSize(position) * 0.8f;
            
            // Draw joystick visualization
            Handles.color = new Color(0.3f, 0.7f, 1f, 0.2f);
            Handles.DrawSolidDisc(position, Vector3.up, radius);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(position, Vector3.up, radius);
            
            // Draw rotation limits
            if (_editJoystickRange)
            {
                DrawInteractiveRotationLimits(position, radius);
            }
            
            Handles.color = Color.white;
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

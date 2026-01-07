using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for JoystickInteractable with MinMaxSlider controls and enhanced visualization.
    /// </summary>
    [CustomEditor(typeof(JoystickInteractable))]
    [CanEditMultipleObjects]
    public class JoystickInteractableEditor : ConstrainedInteractableEditor
    {
        private JoystickInteractable _joystickComponent;
        private SerializedProperty _projectionPlaneHeightProp;
        private SerializedProperty _projectionMethodProp;
        private SerializedProperty _xRotationRangeProp;
        private SerializedProperty _zRotationRangeProp;
        private SerializedProperty _onRotationChangedProp;
        private SerializedProperty _currentRotationProp;
        private SerializedProperty _normalizedRotationProp;

        private static bool _editJoystickRange = false;
        
        private const float MaxAngleLimit = 85f;

        protected override void OnEnable()
        {
            base.OnEnable();

            _joystickComponent = (JoystickInteractable)target;
            _projectionPlaneHeightProp = serializedObject.FindProperty("projectionPlaneHeight");
            _projectionMethodProp = serializedObject.FindProperty("projectionMethod");
            _xRotationRangeProp = serializedObject.FindProperty("xRotationRange");
            _zRotationRangeProp = serializedObject.FindProperty("zRotationRange");
            _onRotationChangedProp = serializedObject.FindProperty("onRotationChanged");
            _currentRotationProp = serializedObject.FindProperty("currentRotation");
            _normalizedRotationProp = serializedObject.FindProperty("normalizedRotation");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Joystick-style interactable with constrained pitch/yaw rotation. Uses plane projection for natural, intuitive control.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Joystick Settings", EditorStyles.boldLabel);
            
            base.DrawCustomProperties();
            
            if (_projectionPlaneHeightProp != null)
            {
                EditorGUILayout.PropertyField(_projectionPlaneHeightProp, 
                    new GUIContent("Projection Plane Height", "Height of the projection plane above the joystick base"));
            }
            
            if (_projectionMethodProp != null)
            {
                EditorGUILayout.PropertyField(_projectionMethodProp, 
                    new GUIContent("Projection Method", "How hand position is projected onto the control plane"));
                
                // Show helpful description based on selection
                var method = (JoystickProjectionMethod)_projectionMethodProp.enumValueIndex;
                string helpText = method == JoystickProjectionMethod.DirectionProjection
                    ? "Direction Projection: Only horizontal hand movement affects angle (arcade stick feel)"
                    : "Plane Intersection: Hand height also affects angle (realistic joystick feel)";
                EditorGUILayout.HelpBox(helpText, MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Rotation Limits", EditorStyles.boldLabel);
            
            DrawRotationRangeSlider("X Rotation (Pitch)", _xRotationRangeProp);
            DrawRotationRangeSlider("Z Rotation (Yaw)", _zRotationRangeProp);
        }

        protected override void DrawImportantSettings()
        {
            DrawEditButton();
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Joystick Events", EditorStyles.boldLabel);
            
            if (_onRotationChangedProp != null)
            {
                EditorGUILayout.PropertyField(_onRotationChangedProp, 
                    new GUIContent("On Rotation Changed", "Event fired when joystick rotation changes"));
            }
            
            EditorGUILayout.Space();
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Joystick Debug", EditorStyles.boldLabel);
            
            if (_currentRotationProp != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_currentRotationProp, 
                    new GUIContent("Current Rotation", "Current rotation angles (X=pitch, Z=yaw) in degrees"));
                EditorGUI.EndDisabledGroup();
            }
            
            if (_normalizedRotationProp != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_normalizedRotationProp, 
                    new GUIContent("Normalized Rotation", "Normalized rotation values (0-1)"));
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawRotationRangeSlider(string label, SerializedProperty rangeProp)
        {
            if (rangeProp == null) return;

            Vector2 range = rangeProp.vector2Value;
            float minValue = range.x;
            float maxValue = range.y;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            // Min value field
            EditorGUILayout.LabelField("Min:", GUILayout.Width(35));
            minValue = EditorGUILayout.FloatField(minValue, GUILayout.Width(50));
            
            // MinMaxSlider
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, -MaxAngleLimit, MaxAngleLimit);
            
            // Max value field
            EditorGUILayout.LabelField("Max:", GUILayout.Width(35));
            maxValue = EditorGUILayout.FloatField(maxValue, GUILayout.Width(50));
            
            EditorGUILayout.EndHorizontal();
            
            // Display range
            EditorGUILayout.LabelField($"Range: {minValue:F1}° to {maxValue:F1}°", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
            
            // Apply clamping
            minValue = Mathf.Clamp(minValue, -MaxAngleLimit, maxValue - 1f);
            maxValue = Mathf.Clamp(maxValue, minValue + 1f, MaxAngleLimit);
            
            rangeProp.vector2Value = new Vector2(minValue, maxValue);
            
            EditorGUILayout.Space(2);
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
            
            // Draw joystick base visualization
            Handles.color = new Color(0.3f, 0.7f, 1f, 0.2f);
            Handles.DrawSolidDisc(position, Vector3.up, radius);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(position, Vector3.up, radius);
            
            // Draw projection plane
            DrawProjectionPlaneHandle(position, radius);
            
            // Draw rotation limits
            if (_editJoystickRange)
            {
                DrawInteractiveRotationLimits(position, radius);
            }
            
            Handles.color = Color.white;
        }

        private void DrawProjectionPlaneHandle(Vector3 position, float radius)
        {
            Transform pivot = _joystickComponent.InteractableObject;
            float planeHeight = _joystickComponent.ProjectionPlaneHeight;
            Vector3 planeCenter = pivot.position + pivot.up * planeHeight;
            
            // Draw plane
            Handles.color = new Color(0f, 1f, 1f, 0.1f);
            Handles.DrawSolidDisc(planeCenter, pivot.up, radius);
            Handles.color = new Color(0f, 1f, 1f, 0.5f);
            Handles.DrawWireDisc(planeCenter, pivot.up, radius);
            
            // Draw support line
            Handles.color = Color.cyan;
            Handles.DrawLine(pivot.position, planeCenter);
            
            // Draw plane height handle
            EditorGUI.BeginChangeCheck();
            Vector3 newPlaneCenter = Handles.Slider(planeCenter, pivot.up, 
                HandleUtility.GetHandleSize(planeCenter) * 0.15f, Handles.ConeHandleCap, 0.01f);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_joystickComponent, "Adjust Projection Plane Height");
                float newHeight = Vector3.Dot(newPlaneCenter - pivot.position, pivot.up);
                _projectionPlaneHeightProp.floatValue = Mathf.Max(0.01f, newHeight);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_joystickComponent);
            }
        }

        private void DrawInteractiveRotationLimits(Vector3 position, float radius)
        {
            // Draw X rotation limits (pitch) - Red
            DrawAxisLimits(position, radius, _joystickComponent.transform.right, _joystickComponent.transform.forward,
                _xRotationRangeProp, Color.red, "X (Pitch)");
            
            // Draw Z rotation limits (yaw) - Blue  
            DrawAxisLimits(position, radius, _joystickComponent.transform.forward, _joystickComponent.transform.up,
                _zRotationRangeProp, Color.blue, "Z (Yaw)");
        }

        private void DrawAxisLimits(Vector3 position, float radius, Vector3 axis, Vector3 reference, 
            SerializedProperty rangeProp, Color color, string axisName)
        {
            Vector2 range = rangeProp.vector2Value;
            float minAngle = range.x;
            float maxAngle = range.y;
            
            var minRotation = Quaternion.AngleAxis(minAngle, axis);
            var maxRotation = Quaternion.AngleAxis(maxAngle, axis);
            var minDir = minRotation * reference;
            var maxDir = maxRotation * reference;
            var minPos = position + minDir * radius;
            var maxPos = position + maxDir * radius;
            
            // Draw arc
            Handles.color = new Color(color.r, color.g, color.b, 0.2f);
            Handles.DrawSolidArc(position, axis, minDir, maxAngle - minAngle, radius);
            Handles.color = color;
            Handles.DrawWireArc(position, axis, minDir, maxAngle - minAngle, radius);
            
            // Draw labels
            Handles.Label(minPos + minDir * 0.1f, $"Min {axisName} ({minAngle:F1}°)");
            Handles.Label(maxPos + maxDir * 0.1f, $"Max {axisName} ({maxAngle:F1}°)");
            
            Undo.RecordObject(_joystickComponent, $"Edit {axisName} Rotation Limits");
            
            // Min handle
            Handles.color = color;
            EditorGUI.BeginChangeCheck();
            var newMinPos = Handles.FreeMoveHandle(minPos, HandleUtility.GetHandleSize(minPos) * 0.08f, 
                Vector3.zero, Handles.DotHandleCap);
            
            if (EditorGUI.EndChangeCheck())
            {
                var from = reference;
                var to = (newMinPos - position).normalized;
                var newMinAngle = Vector3.SignedAngle(from, to, axis);
                range.x = Mathf.Clamp(newMinAngle, -MaxAngleLimit, range.y - 1f);
                rangeProp.vector2Value = range;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_joystickComponent);
            }
            
            // Max handle
            EditorGUI.BeginChangeCheck();
            var newMaxPos = Handles.FreeMoveHandle(maxPos, HandleUtility.GetHandleSize(maxPos) * 0.08f, 
                Vector3.zero, Handles.DotHandleCap);
            
            if (EditorGUI.EndChangeCheck())
            {
                var from = reference;
                var to = (newMaxPos - position).normalized;
                var newMaxAngle = Vector3.SignedAngle(from, to, axis);
                range.y = Mathf.Clamp(newMaxAngle, range.x + 1f, MaxAngleLimit);
                rangeProp.vector2Value = range;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_joystickComponent);
            }
            
            Handles.color = Color.white;
        }
    }
}
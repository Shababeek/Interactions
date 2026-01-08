using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for WheelInteractable with enhanced visualization and rotation limits.
    /// </summary>
    [CustomEditor(typeof(WheelInteractable))]
    [CanEditMultipleObjects]
    public class WheelInteractableEditor : ConstrainedInteractableEditor
    {
        private WheelInteractable _wheelComponent;
        private SerializedProperty _rotationAxisProp;
        private SerializedProperty _limitRotationsProp;
        private SerializedProperty _rotationLimitsProp;
        private SerializedProperty _onWheelAngleChangedProp;
        private SerializedProperty _onWheelRotatedProp;
        private SerializedProperty _currentAngleProp;
        private SerializedProperty _totalRotationsProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            _wheelComponent = (WheelInteractable)target;
            _rotationAxisProp = serializedObject.FindProperty("rotationAxis");
            _limitRotationsProp = serializedObject.FindProperty("limitRotations");
            _rotationLimitsProp = serializedObject.FindProperty("rotationLimits");
            _onWheelAngleChangedProp = serializedObject.FindProperty("onWheelAngleChanged");
            _onWheelRotatedProp = serializedObject.FindProperty("onWheelRotated");
            _currentAngleProp = serializedObject.FindProperty("currentAngle");
            _totalRotationsProp = serializedObject.FindProperty("totalRotations");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Wheel-style interactable that tracks rotation around a single axis. Provides smooth wheel rotation tracking with optional rotation limits.",
                MessageType.Info
            );
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Wheel Settings", EditorStyles.boldLabel);

            base.DrawCustomProperties();

            if (_rotationAxisProp != null)
            {
                EditorGUILayout.PropertyField(_rotationAxisProp,
                    new GUIContent("Rotation Axis", "The axis around which the wheel rotates"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Rotation Limits", EditorStyles.boldLabel);

            if (_limitRotationsProp != null)
            {
                EditorGUILayout.PropertyField(_limitRotationsProp,
                    new GUIContent("Limit Rotations", "Enable rotation limits (min/max number of rotations)"));
            }

            if (_limitRotationsProp != null && _limitRotationsProp.boolValue)
            {
                DrawRotationLimitsSlider();
            }
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Wheel Events", EditorStyles.boldLabel);

            if (_onWheelAngleChangedProp != null)
            {
                EditorGUILayout.PropertyField(_onWheelAngleChangedProp,
                    new GUIContent("On Wheel Angle Changed", "Event fired when wheel rotates (provides current angle)"));
            }

            if (_onWheelRotatedProp != null)
            {
                EditorGUILayout.PropertyField(_onWheelRotatedProp,
                    new GUIContent("On Wheel Rotated", "Event fired when wheel completes full rotation (provides rotation count)"));
            }

            EditorGUILayout.Space();
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Wheel Debug", EditorStyles.boldLabel);

            if (_currentAngleProp != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_currentAngleProp,
                    new GUIContent("Current Angle", "Current rotation angle in degrees"));
                EditorGUI.EndDisabledGroup();
            }

            if (_totalRotationsProp != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_totalRotationsProp,
                    new GUIContent("Total Rotations", "Total number of rotations (can be fractional)"));
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawRotationLimitsSlider()
        {
            if (_rotationLimitsProp == null) return;

            Vector2 limits = _rotationLimitsProp.vector2Value;
            float minValue = limits.x;
            float maxValue = limits.y;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Rotation Range", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();

            // Min value field
            EditorGUILayout.LabelField("Min:", GUILayout.Width(35));
            minValue = EditorGUILayout.FloatField(minValue, GUILayout.Width(50));

            // MinMaxSlider
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, -10f, 10f);

            // Max value field
            EditorGUILayout.LabelField("Max:", GUILayout.Width(35));
            maxValue = EditorGUILayout.FloatField(maxValue, GUILayout.Width(50));

            EditorGUILayout.EndHorizontal();

            // Display range
            EditorGUILayout.LabelField($"Range: {minValue:F1} to {maxValue:F1} rotations", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();

            // Apply clamping
            minValue = Mathf.Min(minValue, maxValue - 0.1f);
            maxValue = Mathf.Max(maxValue, minValue + 0.1f);

            _rotationLimitsProp.vector2Value = new Vector2(minValue, maxValue);

            EditorGUILayout.Space(2);
        }

        private void OnSceneGUI()
        {
            if (_wheelComponent == null || _wheelComponent.InteractableObject == null) return;

            var position = _wheelComponent.InteractableObject.position;
            var rotationAxisVector = _wheelComponent.GetRotationAxisVector();
            var radius = HandleUtility.GetHandleSize(position) * 0.8f;

            // Draw wheel disc
            Handles.color = new Color(0.3f, 0.7f, 1f, 0.2f);
            Handles.DrawSolidDisc(position, rotationAxisVector, radius);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(position, rotationAxisVector, radius);

            // Draw rotation axis line
            Handles.color = Color.yellow;
            Handles.DrawLine(position - rotationAxisVector * radius * 0.5f, 
                           position + rotationAxisVector * radius * 0.5f);

            // Draw reference direction
            var reference = _wheelComponent.GetWorldReferenceVector();
            Handles.color = Color.green;
            Handles.DrawLine(position, position + reference * radius * 0.5f);
            Handles.Label(position + reference * radius * 0.6f, "Reference");

            // Draw rotation limits if enabled
            if (_wheelComponent.LimitRotations)
            {
                DrawRotationLimitsHandles(position, rotationAxisVector, reference, radius);
            }

            Handles.color = Color.white;
        }

        private void DrawRotationLimitsHandles(Vector3 position, Vector3 axis, Vector3 reference, float radius)
        {
            Vector2 limits = _wheelComponent.RotationLimits;

            // Draw min limit arc
            Handles.color = new Color(1f, 0.3f, 0.3f, 0.3f);
            float minAngle = limits.x * 360f;
            Handles.DrawSolidArc(position, axis, reference, minAngle, radius);
            Handles.color = Color.red;
            Handles.DrawWireArc(position, axis, reference, minAngle, radius);

            // Draw max limit arc
            Handles.color = new Color(0.3f, 0.3f, 1f, 0.3f);
            float maxAngle = limits.y * 360f;
            Handles.DrawSolidArc(position, axis, reference, maxAngle, radius);
            Handles.color = Color.blue;
            Handles.DrawWireArc(position, axis, reference, maxAngle, radius);

            // Labels
            var minRot = Quaternion.AngleAxis(minAngle, axis);
            var maxRot = Quaternion.AngleAxis(maxAngle, axis);
            var minDir = minRot * reference;
            var maxDir = maxRot * reference;

            Handles.Label(position + minDir * radius * 1.1f, $"Min ({limits.x:F1})");
            Handles.Label(position + maxDir * radius * 1.1f, $"Max ({limits.y:F1})");
        }
    }
}
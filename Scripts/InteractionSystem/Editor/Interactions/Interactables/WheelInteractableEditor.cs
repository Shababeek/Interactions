using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(WheelInteractable))]
    [CanEditMultipleObjects]
    public class WheelInteractableEditor : RotaryInteractableEditor
    {
        private WheelInteractable _wheel;
        private SerializedProperty _maxRotationsProp;
        private SerializedProperty _onAngleChangedProp;
        private SerializedProperty _onNormalizedChangedProp;
        private SerializedProperty _normalizedValueProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _wheel = (WheelInteractable)target;

            _maxRotationsProp = serializedObject.FindProperty("maxRotations");
            _onAngleChangedProp = serializedObject.FindProperty("onAngleChanged");
            _onNormalizedChangedProp = serializedObject.FindProperty("onNormalizedChanged");
            _normalizedValueProp = serializedObject.FindProperty("normalizedValue");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Wheel interactable for steering wheels and similar continuous rotators.\n" +
                "Returns to zero on release.",
                MessageType.Info);
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Wheel Settings", EditorStyles.boldLabel);
            base.DrawCustomProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation Limits", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_maxRotationsProp,
                new GUIContent("Max Rotations", "Maximum turns in each direction"));

            float maxDegrees = _maxRotationsProp.floatValue * 360f;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Max Angle (°)", maxDegrees);
            EditorGUI.EndDisabledGroup();
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onAngleChangedProp,
                new GUIContent("On Angle Changed", "Fires with current angle in degrees"));
            EditorGUILayout.PropertyField(_onNormalizedChangedProp,
                new GUIContent("On Normalized Changed", "Fires with -1 to 1 value"));
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            base.DrawCustomDebugInfo();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_normalizedValueProp, new GUIContent("Normalized Value"));
            EditorGUI.EndDisabledGroup();
        }

        private void OnSceneGUI()
        {
            if (_wheel == null || _wheel.InteractableObject == null) return;

            var pos = _wheel.InteractableObject.position;
            var axis = _wheel.GetWorldAxis();
            var size = HandleUtility.GetHandleSize(pos);

            Handles.color = new Color(0.3f, 0.7f, 1f, 0.15f);
            Handles.DrawSolidDisc(pos, axis, size * 0.6f);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(pos, axis, size * 0.6f);

            Handles.color = Color.yellow;
            Handles.DrawLine(pos - axis * size * 0.3f, pos + axis * size * 0.3f);
        }
    }
}

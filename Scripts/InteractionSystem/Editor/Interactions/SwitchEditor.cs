using UnityEngine;
using UnityEditor;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for the two-state Switch component with scene-view angle handles.
    /// </summary>
    [CustomEditor(typeof(Switch))]
    public class SwitchEditor : Editor
    {
        private Switch _switch;

        private SerializedProperty _onTurnedOn;
        private SerializedProperty _onTurnedOff;
        private SerializedProperty _onStateChanged;
        private SerializedProperty _switchBody;
        private SerializedProperty _rotationAxis;
        private SerializedProperty _detectionAxis;
        private SerializedProperty _onAngle;
        private SerializedProperty _offAngle;
        private SerializedProperty _rotateSpeed;
        private SerializedProperty _angleThreshold;
        private SerializedProperty _startOn;
        private SerializedProperty _currentState;

        private bool _showEvents = true;
        private static bool _editAngles = false;

        private void OnEnable()
        {
            _switch = (Switch)target;

            _onTurnedOn = serializedObject.FindProperty("onTurnedOn");
            _onTurnedOff = serializedObject.FindProperty("onTurnedOff");
            _onStateChanged = serializedObject.FindProperty("onStateChanged");
            _switchBody = serializedObject.FindProperty("switchBody");
            _rotationAxis = serializedObject.FindProperty("rotationAxis");
            _detectionAxis = serializedObject.FindProperty("detectionAxis");
            _onAngle = serializedObject.FindProperty("onAngle");
            _offAngle = serializedObject.FindProperty("offAngle");
            _rotateSpeed = serializedObject.FindProperty("rotateSpeed");
            _angleThreshold = serializedObject.FindProperty("angleThreshold");
            _startOn = serializedObject.FindProperty("startOn");
            _currentState = serializedObject.FindProperty("currentState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Two-state switch. The side a finger approaches from decides on vs off. " +
                "Use the edit button to drag the on/off angles in the scene view.",
                MessageType.Info);

            DrawEditButton();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Switch Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_switchBody);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_rotationAxis, new GUIContent("Rotation Axis", "The axis around which the switch rotates"));
            EditorGUILayout.PropertyField(_detectionAxis, new GUIContent("Detection Axis", "The axis used to detect which side the hand approaches from"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_onAngle, new GUIContent("On Angle (°)", "Rotation angle for the on position"));
            EditorGUILayout.PropertyField(_offAngle, new GUIContent("Off Angle (°)", "Rotation angle for the off position"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_rotateSpeed, new GUIContent("Rotate Speed", "Speed of the rotation animation"));
            EditorGUILayout.PropertyField(_angleThreshold, new GUIContent("Angle Threshold (°)", "Minimum approach angle before the switch flips"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(_startOn, new GUIContent("Start On", "State the switch starts in when the scene loads"));

            EditorGUILayout.Space();
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                EditorGUILayout.PropertyField(_onTurnedOn);
                EditorGUILayout.PropertyField(_onTurnedOff);
                EditorGUILayout.PropertyField(_onStateChanged);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(_currentState, new GUIContent("Current State", "True when the switch is on"));

            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Turn On")) _switch.SetState(true);
                if (GUILayout.Button("Turn Off")) _switch.SetState(false);
                if (GUILayout.Button("Toggle")) _switch.Toggle();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField($"State: {(_switch.IsOn ? "ON" : "OFF")}");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEditButton()
        {
            var icon = EditorGUIUtility.IconContent(_editAngles ? "d_EditCollider" : "EditCollider");
            var style = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32,
                fixedHeight = 24,
                padding = new RectOffset(2, 2, 2, 2)
            };

            var prevColor = GUI.color;
            if (_editAngles) GUI.color = Color.green;
            if (GUILayout.Button(icon, style)) _editAngles = !_editAngles;
            GUI.color = prevColor;
        }

        private void OnSceneGUI()
        {
            if (_switch == null || _switch.SwitchBody == null) return;

            var body = _switch.SwitchBody;
            var position = body.position;
            var axis = GetRotationAxisVector();
            var detection = GetDetectionVector();
            var radius = HandleUtility.GetHandleSize(position) * 0.8f;

            var onA = _onAngle.floatValue;
            var offA = _offAngle.floatValue;

            var onDir = Quaternion.AngleAxis(onA, axis) * detection;
            var offDir = Quaternion.AngleAxis(offA, axis) * detection;
            var onPos = position + onDir * radius;
            var offPos = position + offDir * radius;

            Handles.color = new Color(0f, 1f, 0f, 0.15f);
            Handles.DrawSolidArc(position, axis, offDir, onA - offA, radius);
            Handles.color = Color.green;
            Handles.DrawWireArc(position, axis, offDir, onA - offA, radius);
            Handles.Label(onPos + onDir * 0.1f, $"ON ({onA:F1}°)");
            Handles.Label(offPos + offDir * 0.1f, $"OFF ({offA:F1}°)");

            if (!_editAngles) return;

            Undo.RecordObject(_switch, "Edit Switch Angles");

            EditorGUI.BeginChangeCheck();
            var newOn = Handles.FreeMoveHandle(onPos, HandleUtility.GetHandleSize(onPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                var angle = Vector3.SignedAngle(detection, (newOn - position).normalized, axis);
                _onAngle.floatValue = Mathf.Clamp(angle, -180f, 180f);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            var newOff = Handles.FreeMoveHandle(offPos, HandleUtility.GetHandleSize(offPos) * 0.08f, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                var angle = Vector3.SignedAngle(detection, (newOff - position).normalized, axis);
                _offAngle.floatValue = Mathf.Clamp(angle, -180f, 180f);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private Vector3 GetRotationAxisVector()
        {
            var body = _switch.SwitchBody;
            return (Axis)_rotationAxis.enumValueIndex switch
            {
                Axis.X => body.right,
                Axis.Y => body.up,
                Axis.Z => body.forward,
                _ => body.forward
            };
        }

        private Vector3 GetDetectionVector()
        {
            var body = _switch.SwitchBody;
            return (Axis)_detectionAxis.enumValueIndex switch
            {
                Axis.X => body.right,
                Axis.Y => body.up,
                Axis.Z => body.forward,
                _ => body.right
            };
        }
    }
}

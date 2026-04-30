using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(DialInteractable))]
    [CanEditMultipleObjects]
    public class DialInteractableEditor : RotaryInteractableEditor
    {
        private DialInteractable _dial;
        private SerializedProperty _numberOfStepsProp;
        private SerializedProperty _startingStepProp;
        private SerializedProperty _totalAngleProp;
        private SerializedProperty _wrapAroundProp;
        private SerializedProperty _hapticOnStepProp;
        private SerializedProperty _hapticAmplitudeProp;
        private SerializedProperty _hapticDurationProp;
        private SerializedProperty _onStepChangedProp;
        private SerializedProperty _onStepConfirmedProp;
        private SerializedProperty _currentStepProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _dial = (DialInteractable)target;

            _numberOfStepsProp = serializedObject.FindProperty("numberOfSteps");
            _startingStepProp = serializedObject.FindProperty("startingStep");
            _totalAngleProp = serializedObject.FindProperty("totalAngle");
            _wrapAroundProp = serializedObject.FindProperty("wrapAround");
            _hapticOnStepProp = serializedObject.FindProperty("hapticOnStep");
            _hapticAmplitudeProp = serializedObject.FindProperty("hapticAmplitude");
            _hapticDurationProp = serializedObject.FindProperty("hapticDuration");
            _onStepChangedProp = serializedObject.FindProperty("onStepChanged");
            _onStepConfirmedProp = serializedObject.FindProperty("onStepConfirmed");
            _currentStepProp = serializedObject.FindProperty("currentStep");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Rotary dial with discrete steps (combination lock, selector switch).\n" +
                "Always snaps to the nearest step on release.",
                MessageType.Info);
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Dial Settings", EditorStyles.boldLabel);
            base.DrawCustomProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Steps", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_numberOfStepsProp, new GUIContent("Number Of Steps"));
            EditorGUILayout.PropertyField(_startingStepProp, new GUIContent("Starting Step"));
            EditorGUILayout.PropertyField(_totalAngleProp, new GUIContent("Total Angle (°)"));
            EditorGUILayout.PropertyField(_wrapAroundProp, new GUIContent("Wrap Around"));

            int steps = Mathf.Max(1, _numberOfStepsProp.intValue);
            float anglePerStep = _totalAngleProp.floatValue / steps;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Angle Per Step (°)", anglePerStep);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Haptics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hapticOnStepProp, new GUIContent("Haptic On Step"));
            if (_hapticOnStepProp.boolValue)
            {
                EditorGUILayout.PropertyField(_hapticAmplitudeProp, new GUIContent("Amplitude"));
                EditorGUILayout.PropertyField(_hapticDurationProp, new GUIContent("Duration (s)"));
            }
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onStepChangedProp,
                new GUIContent("On Step Changed", "Fires with new step index when the step changes"));
            EditorGUILayout.PropertyField(_onStepConfirmedProp,
                new GUIContent("On Step Confirmed", "Fires with step index when snap completes"));
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            base.DrawCustomDebugInfo();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_currentStepProp, new GUIContent("Current Step"));
            EditorGUI.EndDisabledGroup();
        }

        private void OnSceneGUI()
        {
            if (_dial == null || _dial.InteractableObject == null) return;

            var pos = _dial.InteractableObject.position;
            var axis = _dial.GetWorldAxis();
            var size = HandleUtility.GetHandleSize(pos);

            Handles.color = new Color(0.3f, 0.7f, 1f, 0.12f);
            Handles.DrawSolidDisc(pos, axis, size * 0.6f);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(pos, axis, size * 0.6f);

            Handles.color = Color.yellow;
            Handles.DrawLine(pos - axis * size * 0.3f, pos + axis * size * 0.3f);

            int steps = Mathf.Max(1, _numberOfStepsProp.intValue);
            float anglePerStep = _totalAngleProp.floatValue / steps;

            var t = _dial.InteractableObject;
            Vector3 reference = t.right;
            if (Vector3.Dot(axis.normalized, Vector3.right) > 0.9f) reference = t.forward;

            for (int i = 0; i < steps; i++)
            {
                float angle = i * anglePerStep;
                var rot = Quaternion.AngleAxis(angle, axis);
                Vector3 dir = rot * reference;
                Handles.color = (Application.isPlaying && i == _currentStepProp.intValue) ? Color.green : Color.cyan;
                Handles.DrawLine(pos, pos + dir * size * 0.6f);
            }
        }
    }
}

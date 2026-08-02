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
        private SerializedProperty _offsetAngleProp;
        private SerializedProperty _wrapAroundProp;
        private SerializedProperty _hapticOnStepProp;
        private SerializedProperty _hapticAmplitudeProp;
        private SerializedProperty _hapticDurationProp;
        private SerializedProperty _hapticPatternProp;
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
            _offsetAngleProp = serializedObject.FindProperty("offsetAngle");
            _wrapAroundProp = serializedObject.FindProperty("wrapAround");
            _hapticOnStepProp = serializedObject.FindProperty("hapticOnStep");
            _hapticAmplitudeProp = serializedObject.FindProperty("hapticAmplitude");
            _hapticDurationProp = serializedObject.FindProperty("hapticDuration");
            _hapticPatternProp = serializedObject.FindProperty("hapticPattern");
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
            EditorGUILayout.PropertyField(_offsetAngleProp,
                new GUIContent("Offset Angle (°)", "Angle where step 0 sits, relative to the authored rotation."));
            EditorGUILayout.PropertyField(_wrapAroundProp, new GUIContent("Wrap Around"));

            float offset = _offsetAngleProp.floatValue;
            float anglePerStep = AnglePerStep();
            float lastStepAngle = offset + anglePerStep * (StepCount() - 1);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Angle Per Step (°)", anglePerStep);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.HelpBox(
                _wrapAroundProp.boolValue
                    ? $"Wrapping: steps run {offset:0.##}° → {lastStepAngle:0.##}° and loop back to {offset:0.##}°."
                    : $"Clamped: step 0 at {offset:0.##}°, last step at {lastStepAngle:0.##}° " +
                      $"(full {_totalAngleProp.floatValue:0.##}° sweep).",
                MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Haptics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hapticOnStepProp, new GUIContent("Haptic On Step"));
            if (_hapticOnStepProp.boolValue)
            {
                EditorGUILayout.PropertyField(_hapticPatternProp, new GUIContent("Pattern (optional)"));
                EditorGUI.BeginDisabledGroup(_hapticPatternProp.objectReferenceValue != null);
                EditorGUILayout.PropertyField(_hapticAmplitudeProp, new GUIContent("Amplitude"));
                EditorGUILayout.PropertyField(_hapticDurationProp, new GUIContent("Duration (s)"));
                EditorGUI.EndDisabledGroup();
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

        /// <summary>Steps drawn in the scene view, mirroring <see cref="DialInteractable.NumberOfSteps"/>.</summary>
        private int StepCount() => Mathf.Max(2, _numberOfStepsProp.intValue);

        /// <summary>
        /// Must match <see cref="DialInteractable.AnglePerStep"/>: a wrapping dial divides the sweep
        /// into N gaps, a clamped one into N-1 so the last step lands exactly on Total Angle.
        /// </summary>
        private float AnglePerStep()
        {
            int steps = StepCount();
            return _wrapAroundProp.boolValue
                ? _totalAngleProp.floatValue / steps
                : _totalAngleProp.floatValue / Mathf.Max(1, steps - 1);
        }

        private void OnSceneGUI()
        {
            if (_dial == null || _dial.InteractableObject == null) return;

            var pos = _dial.InteractableObject.position;
            var axis = _dial.GetSignedWorldAxis();
            var size = HandleUtility.GetHandleSize(pos);
            float radius = size * 0.6f;

            Handles.color = new Color(0.3f, 0.7f, 1f, 0.12f);
            Handles.DrawSolidDisc(pos, axis, radius);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(pos, axis, radius);

            Handles.color = Color.yellow;
            Handles.DrawLine(pos - axis * size * 0.3f, pos + axis * size * 0.3f);

            int steps = StepCount();
            float anglePerStep = AnglePerStep();
            float offset = _offsetAngleProp.floatValue;

            var t = _dial.InteractableObject;
            Vector3 reference = t.right;
            if (Vector3.Dot(axis.normalized, Vector3.right) > 0.9f) reference = t.forward;

            // Sweep wedge: from the offset through the full travel, so a 180° dial reads as a half circle.
            Vector3 sweepStart = Quaternion.AngleAxis(offset, axis) * reference;
            Handles.color = new Color(1f, 0.8f, 0.2f, 0.15f);
            Handles.DrawSolidArc(pos, axis, sweepStart, _totalAngleProp.floatValue, radius);

            for (int i = 0; i < steps; i++)
            {
                float angle = offset + i * anglePerStep;
                var rot = Quaternion.AngleAxis(angle, axis);
                Vector3 dir = rot * reference;
                Handles.color = (Application.isPlaying && i == _currentStepProp.intValue) ? Color.green : Color.cyan;
                Handles.DrawLine(pos, pos + dir * radius);
                Handles.Label(pos + dir * radius * 1.1f, i.ToString());
            }
        }
    }
}

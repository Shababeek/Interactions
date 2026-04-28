using UnityEditor;
using UnityEngine;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(SliderInteractable))]
    [CanEditMultipleObjects]
    public class SliderInteractableEditor : ConstrainedInteractableEditor
    {
        private SliderInteractable _slider;
        private SerializedProperty _localStartProp;
        private SerializedProperty _localEndProp;
        private SerializedProperty _numberOfStepsProp;
        private SerializedProperty _startingStepProp;
        private SerializedProperty _hapticOnStepProp;
        private SerializedProperty _hapticAmplitudeProp;
        private SerializedProperty _hapticDurationProp;
        private SerializedProperty _onStepChangedProp;
        private SerializedProperty _onStepConfirmedProp;
        private SerializedProperty _onMovedProp;
        private SerializedProperty _currentStepProp;
        private SerializedProperty _currentNormalizedProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            _slider = (SliderInteractable)target;

            _localStartProp = serializedObject.FindProperty("localStart");
            _localEndProp = serializedObject.FindProperty("localEnd");
            _numberOfStepsProp = serializedObject.FindProperty("numberOfSteps");
            _startingStepProp = serializedObject.FindProperty("startingStep");
            _hapticOnStepProp = serializedObject.FindProperty("hapticOnStep");
            _hapticAmplitudeProp = serializedObject.FindProperty("hapticAmplitude");
            _hapticDurationProp = serializedObject.FindProperty("hapticDuration");
            _onStepChangedProp = serializedObject.FindProperty("onStepChanged");
            _onStepConfirmedProp = serializedObject.FindProperty("onStepConfirmed");
            _onMovedProp = serializedObject.FindProperty("onMoved");
            _currentStepProp = serializedObject.FindProperty("currentStep");
            _currentNormalizedProp = serializedObject.FindProperty("currentNormalized");
        }

        protected override void DrawCustomHeader()
        {
            EditorGUILayout.HelpBox(
                "Linear slider with discrete steps (volume fader, multi-position switch).\n" +
                "Define start/end in local space; the handle moves along that line.\n" +
                "Always snaps to the nearest step on release.",
                MessageType.Info);
        }

        protected override void DrawCustomProperties()
        {
            EditorGUILayout.LabelField("Slider Settings", EditorStyles.boldLabel);
            base.DrawCustomProperties();

            EditorGUILayout.PropertyField(_localStartProp, new GUIContent("Local Start", "Handle position at step 0."));
            EditorGUILayout.PropertyField(_localEndProp, new GUIContent("Local End", "Handle position at the last step."));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Steps", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_numberOfStepsProp, new GUIContent("Number Of Steps"));
            EditorGUILayout.PropertyField(_startingStepProp, new GUIContent("Starting Step"));

            int steps = Mathf.Max(2, _numberOfStepsProp.intValue);
            Vector3 rail = _localEndProp.vector3Value - _localStartProp.vector3Value;
            float railLength = rail.magnitude;
            float distancePerStep = steps > 1 ? railLength / (steps - 1) : 0f;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Distance Per Step", distancePerStep);
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
            // EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onStepChangedProp,
                new GUIContent("On Step Changed", "Fires with new step index when the step changes"));
            EditorGUILayout.PropertyField(_onStepConfirmedProp,
                new GUIContent("On Step Confirmed", "Fires with step index when snap completes"));
            EditorGUILayout.PropertyField(_onMovedProp,
                new GUIContent("On Moved", "Fires continuously with normalized position (0–1)"));
        }

        protected override void DrawCustomDebugInfo()
        {
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_currentStepProp, new GUIContent("Current Step"));
            EditorGUILayout.PropertyField(_currentNormalizedProp, new GUIContent("Current Normalized"));
            EditorGUI.EndDisabledGroup();
        }

        private void OnSceneGUI()
        {
            if (_slider == null || _slider.InteractableObject == null) return;

            Transform tr = _slider.transform;
            Vector3 worldStart = tr.TransformPoint(_localStartProp.vector3Value);
            Vector3 worldEnd = tr.TransformPoint(_localEndProp.vector3Value);
            float size = HandleUtility.GetHandleSize(Vector3.Lerp(worldStart, worldEnd, 0.5f));

            Handles.color = Color.green;
            Handles.DrawLine(worldStart, worldEnd);

            Handles.color = Color.red;
            Handles.SphereHandleCap(0, worldStart, Quaternion.identity, size * 0.05f, EventType.Repaint);
            Handles.SphereHandleCap(0, worldEnd, Quaternion.identity, size * 0.05f, EventType.Repaint);

            int steps = Mathf.Max(2, _numberOfStepsProp.intValue);
            for (int i = 0; i < steps; i++)
            {
                float t = steps > 1 ? (float)i / (steps - 1) : 0f;
                Vector3 pos = Vector3.Lerp(worldStart, worldEnd, t);
                bool active = Application.isPlaying && i == _currentStepProp.intValue;
                Handles.color = active ? Color.green : Color.cyan;
                Handles.SphereHandleCap(0, pos, Quaternion.identity, size * 0.035f, EventType.Repaint);
            }

            if (Application.isPlaying)
            {
                Handles.color = Color.yellow;
                Vector3 handlePos = _slider.InteractableObject.position;
                Handles.SphereHandleCap(0, handlePos, Quaternion.identity, size * 0.045f, EventType.Repaint);
            }
        }
    }
}

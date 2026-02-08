using UnityEditor;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Custom editor for SequenceBehaviour providing conditional property display and runtime controls.
    /// </summary>
    [CustomEditor(typeof(SequenceBehaviour))]
    public class SequenceBehaviourEditor : Editor
    {
        private SerializedProperty _sequence;
        private SerializedProperty _startOnAwake;
        private SerializedProperty _delay;
        private SerializedProperty _onSequenceStarted;
        private SerializedProperty _onSequenceCompleted;
        private SerializedProperty _enableDebugControls;
        private SerializedProperty _nextStepKey;
        private SerializedProperty _previousStepKey;
        private SerializedProperty _enableAnalytics;
        private SerializedProperty _started;

        private void OnEnable()
        {
            _sequence = serializedObject.FindProperty("sequence");
            _startOnAwake = serializedObject.FindProperty("startOnAwake");
            _delay = serializedObject.FindProperty("delay");
            _onSequenceStarted = serializedObject.FindProperty("onSequenceStarted");
            _onSequenceCompleted = serializedObject.FindProperty("onSequenceCompleted");
            _enableDebugControls = serializedObject.FindProperty("enableDebugControls");
            _nextStepKey = serializedObject.FindProperty("nextStepKey");
            _previousStepKey = serializedObject.FindProperty("previousStepKey");
            _enableAnalytics = serializedObject.FindProperty("enableAnalytics");
            _started = serializedObject.FindProperty("started");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var behaviour = (SequenceBehaviour)target;

            EditorGUILayout.PropertyField(_sequence);
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(_startOnAwake);
            if (_startOnAwake.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_delay);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_onSequenceStarted);
            EditorGUILayout.PropertyField(_onSequenceCompleted);

            EditorGUILayout.Space(8);
            EditorGUILayout.PropertyField(_enableDebugControls);
            if (_enableDebugControls.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_nextStepKey, new GUIContent("Next Step Key"));
                EditorGUILayout.PropertyField(_previousStepKey, new GUIContent("Previous Step Key"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_enableAnalytics);

            EditorGUILayout.Space(8);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_started);
            GUI.enabled = true;

            if (Application.isPlaying && behaviour.sequence != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (!behaviour.Started)
                {
                    if (GUILayout.Button("Start Sequence"))
                    {
                        behaviour.StartSequence();
                    }
                }
                else
                {
                    if (GUILayout.Button("Skip Step"))
                    {
                        behaviour.SkipCurrentStep();
                    }
                    if (GUILayout.Button("Previous Step"))
                    {
                        behaviour.GoToPreviousStep();
                    }
                    if (GUILayout.Button("Restart"))
                    {
                        behaviour.RestartSequence();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (behaviour.Started && behaviour.sequence.CurrentStep != null)
                {
                    EditorGUILayout.HelpBox(
                        $"Current Step: {behaviour.sequence.CurrentStep.name}",
                        MessageType.Info);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

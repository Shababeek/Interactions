using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(InteractionRecording))]
    public class InteractionRecordingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var recording = (InteractionRecording)target;

            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField("Sample Rate (Hz)", recording.SampleRate);
                EditorGUILayout.IntField("Pose Samples", recording.SampleCount);
                EditorGUILayout.FloatField("Duration (s)", recording.Duration);
                EditorGUILayout.IntField("Left Button Events", recording.LeftHand?.inputEvents?.Length ?? 0);
                EditorGUILayout.IntField("Right Button Events", recording.RightHand?.inputEvents?.Length ?? 0);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Show Raw Data"))
            {
                _showRaw = !_showRaw;
            }
            if (_showRaw)
            {
                DrawDefaultInspector();
            }
        }

        private bool _showRaw;
    }
}

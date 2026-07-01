using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(InteractionRecorder))]
    public class InteractionRecorderEditor : Editor
    {
        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var recorder = (InteractionRecorder)target;
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play mode to record an interaction session.", MessageType.Info);
                return;
            }

            if (recorder.IsRecording)
            {
                EditorGUILayout.HelpBox($"Recording… {recorder.ElapsedTime:F1}s, {recorder.SampleCount} samples.", MessageType.Warning);
                if (GUILayout.Button("Stop Recording"))
                {
                    recorder.StopRecording();
                }
            }
            else
            {
                if (GUILayout.Button("Start Recording"))
                {
                    recorder.StartRecording();
                }
                if (recorder.LastRecording != null)
                {
                    EditorGUILayout.ObjectField("Last Recording", recorder.LastRecording, typeof(InteractionRecording), false);
                }
            }
        }
    }
}

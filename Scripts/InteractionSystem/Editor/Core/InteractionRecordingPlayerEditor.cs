using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(InteractionRecordingPlayer))]
    public class InteractionRecordingPlayerEditor : Editor
    {
        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var player = (InteractionRecordingPlayer)target;
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play mode to replay a recording. Assign any head/hand TrackedPoseDrivers to 'Tracking To Disable During Playback' so they don't fight the replayed poses.", MessageType.Info);
                return;
            }

            if (player.Recording == null)
            {
                EditorGUILayout.HelpBox("Assign a recording to replay.", MessageType.Warning);
                return;
            }

            if (player.IsPlaying)
            {
                EditorGUILayout.HelpBox($"Playing… {player.Time:F1}s / {player.Recording.Duration:F1}s.", MessageType.Warning);
                if (GUILayout.Button("Stop")) player.Stop();
            }
            else if (GUILayout.Button("Play"))
            {
                player.Play();
            }
        }
    }
}

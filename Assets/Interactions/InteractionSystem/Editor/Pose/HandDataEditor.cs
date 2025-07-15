using UnityEditor;

namespace Kandooz.InteractionSystem.Animations.Editors
{
    [CustomEditor(typeof(HandPoseData))]
    [CanEditMultipleObjects]
    public class HandDataEditor : Editor
    {
        HandPoseData data;

        private void OnEnable()
        {
            data = (HandPoseData)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default Pose  animations");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultPose").FindPropertyRelative("open"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultPose").FindPropertyRelative("closed"));
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("poses"));
            serializedObject.ApplyModifiedProperties();
            data.DefaultPose.SetPosNameIfEmpty("default");
            data.DefaultPose.SetType(PoseData.PoseType.Dynamic);

        }
    }
}
using Shababeek.Interactions.Animations;
using UnityEditor;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(HandData))]
    [CanEditMultipleObjects]
    public class HandDataEditor : Editor
    {
        private HandData data;

        private void OnEnable()
        {
            data = (HandData)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawHandPrefabEditor(data.LeftHandPrefab, "leftHandPrefab");
            DrawHandPrefabEditor(data.RightHandPrefab, "rightHandPrefab");
            DrawAvatarMasnksEditor();
            DrawAnimationPoseEditor();

            serializedObject.ApplyModifiedProperties();
            data.DefaultPose.SetPosNameIfEmpty("default");
            data.DefaultPose.SetType(PoseData.PoseType.Dynamic);
        }

        private void DrawAvatarMasnksEditor()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Finger Masks");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handAvatarMaskContainer").FindPropertyRelative("thumb"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handAvatarMaskContainer").FindPropertyRelative("index"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handAvatarMaskContainer").FindPropertyRelative("middle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handAvatarMaskContainer").FindPropertyRelative("ring"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handAvatarMaskContainer").FindPropertyRelative("pinky"));
            EditorGUI.indentLevel--;
        }

        private void DrawAnimationPoseEditor()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default Pose  animations");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultPose").FindPropertyRelative("open"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultPose").FindPropertyRelative("closed"));
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("poses"));
        }

        private void DrawHandPrefabEditor(HandPoseController handObject, string handVariableName)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(handVariableName));
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) handObject.HandData = data;
        }
    }
}
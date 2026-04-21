using Shababeek.Interactions.Animations;
using UnityEditor;
using UnityEngine;

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

            bool muscleBased = data.PoseSystem == HandPoseSystem.MuscleBased;

            DrawHandPrefabSection(muscleBased);
            if (!muscleBased) DrawAvatarMasksEditor();
            DrawAnimationPoseEditor(muscleBased);

            serializedObject.ApplyModifiedProperties();
            data.DefaultPose.SetPosNameIfEmpty("default");
            data.DefaultPose.SetType(PoseData.PoseType.Dynamic);
        }

        private void DrawHandPrefabSection(bool muscleBased)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hand Prefabs", EditorStyles.boldLabel);

            if (muscleBased)
            {
                EditorGUILayout.HelpBox(
                    "Muscle-Based mode: drag a HandPoseController prefab whose root Animator uses a Humanoid avatar. " +
                    "Finger AvatarMasks are ignored in this mode.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Legacy Bone-Based mode: drag a HandPoseController prefab whose skeleton matches the finger AvatarMasks below.",
                    MessageType.Info);
            }

            DrawHandPrefabField(
                data.LeftHandPrefab,
                "leftHandPrefab",
                muscleBased ? "Left Humanoid Hand Prefab" : "Left Hand Prefab",
                muscleBased);

            DrawHandPrefabField(
                data.RightHandPrefab,
                "rightHandPrefab",
                muscleBased ? "Right Humanoid Hand Prefab" : "Right Hand Prefab",
                muscleBased);
        }

        private void DrawHandPrefabField(HandPoseController handObject, string propertyName, string label, bool requireHumanoid)
        {
            var property = serializedObject.FindProperty(propertyName);
            var tooltip = requireHumanoid
                ? "Drag a HandPoseController prefab whose root Animator has a Humanoid avatar."
                : "Drag a HandPoseController prefab matching the finger AvatarMasks.";

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck() && handObject != null)
            {
                handObject.HandData = data;
            }

            if (requireHumanoid) ValidateHumanoidPrefab(property);
        }

        private static void ValidateHumanoidPrefab(SerializedProperty property)
        {
            var controller = property.objectReferenceValue as HandPoseController;
            if (controller == null) return;

            var animator = controller.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                EditorGUILayout.HelpBox(
                    $"'{controller.name}' has no Animator. Muscle-Based mode requires an Animator with a Humanoid avatar.",
                    MessageType.Warning);
                return;
            }

            if (animator.avatar == null || !animator.avatar.isHuman)
            {
                EditorGUILayout.HelpBox(
                    $"'{controller.name}' Animator does not use a Humanoid avatar. Muscle-Based mode will fail to initialize at runtime.",
                    MessageType.Error);
            }
        }

        private void DrawAvatarMasksEditor()
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

        private void DrawAnimationPoseEditor(bool muscleBased)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default Pose  animations");
            if (muscleBased)
            {
                EditorGUILayout.HelpBox(
                    "Leave Open and Closed empty to use a procedural neutral-to-fist default. " +
                    "Assign clips to override with a hand-authored gesture.",
                    MessageType.None);
            }
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultPose").FindPropertyRelative("open"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultPose").FindPropertyRelative("closed"));
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("poses"));
        }
    }
}

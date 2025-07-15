using Kandooz.InteractionSystem.Core;
using UnityEditor;
using UnityEngine;
namespace Kandooz.InteractionSystem.Animations
{
    [CustomEditor(typeof(HandPoseController))]
    public class HandAnimationControllerEditor : Editor
    {
        HandPoseController controller;
        void OnEnable()
        {
            controller = (HandPoseController)target;
            if (controller.HandData)
            {
                controller.Initialize();
            }
            EditorApplication.update += Update;

            controller.Initialize();
        }
        void OnDisable()
        {
            EditorApplication.update -= Update;
        }
        void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                if (controller.Graph.IsValid())
                {

                    controller.UpdateGraphVariables();
                }
                else
                {
                    controller.Initialize();
                }

            }
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DisplayHandEditor(serializedObject);
            serializedObject.ApplyModifiedProperties();

        }

        private void DisplayHandEditor(SerializedObject serializedObject)
        {

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handData"));
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                controller.Initialize();
            }
            if (!controller.HandData)
            {
                EditorGUILayout.LabelField("Please select a handData object or create one");
                return;
            }
            DrawCurrentPoseEditor(serializedObject);
            if (!IsStaticPose())
            {
                DrawFingerEditor(serializedObject);
            }
        }
        private bool IsStaticPose()
        {
            var currentPose = controller.Poses[controller.CurrentPoseIndex];
            var isStatic = currentPose.GetType() == typeof(StaticPose);
            return isStatic;
        }
        private void DrawCurrentPoseEditor(SerializedObject serializedObject)
        {
            var currentPose = serializedObject.FindProperty("currentPoseIndex");
            var poses = controller.Poses;
            string[] list = new string[poses.Count];
            for (int i = 0; i < list.Length; i++)
            {

                list[i] = poses[i].Name;
            }
            controller.CurrentPoseIndex =
                EditorGUILayout.Popup(new GUIContent("pose"), currentPose.intValue, list);
        }
        private static void DrawFingerEditor(SerializedObject serializedObject)
        {
            var fingers = serializedObject.FindProperty("fingers");
            for (int i = 0; i < fingers.arraySize; i++)
            {
                var fingerWeight = fingers.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(fingerWeight, new GUIContent(((FingerName)i).ToString()));
            }
        }
        Vector3 last = Vector3.one * 10000;
        protected virtual void OnSceneGUI()
        {
            EditorGUI.BeginChangeCheck();
            var fingerPositions = controller.GetComponentsInChildren<FingerVisualizer>();
            foreach (var finger in fingerPositions)
            {
                var position = finger.transform.position;
                var rotation = controller.transform.rotation;
                var value = Mathf.Max( controller[finger.finger],0.0001f);
                value= DoFingerHandle(position, rotation,value);
                if (value < 0.0002)
                {
                    value = 0;
                }
                value = Mathf.Clamp01(value);
                controller[finger.finger] = value;
            }
            EditorGUI.EndChangeCheck();
        }

        private float DoFingerHandle(Vector3 position, Quaternion rotation,float value=1)
        {
            Handles.color = Color.green*.7f;
            var result = Handles.ScaleSlider(value, position, controller.transform.right, rotation, .05f, 2f);
            while(Mathf.Abs(result-value)>.00001 && Mathf.Abs(result - value) < .07)
            {
                value = (value) * 10;
            }
            return result;
        }
    }
}
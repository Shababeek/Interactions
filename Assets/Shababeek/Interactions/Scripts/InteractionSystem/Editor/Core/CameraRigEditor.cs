using System;
using Shababeek.Interactions.Core;
using UnityEditor;

namespace Shababeek.InteractionSystem.Core.Editors
{
    [CustomEditor(typeof(CameraRig))]
    public class CameraRigEditor : Editor
    {
        private CameraRig rig;

        private void OnEnable()
        {
            rig = (CameraRig)target;
            var hands = rig.GetComponentsInChildren<Hand>();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (rig.Config == null)
            {
                EditorGUILayout.LabelField("Select a config file to continue");
                return;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("initializeHands"));

            if (serializedObject.FindProperty("initializeHands").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("handTrackingMethod"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("leftHandPivot"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rightHandPivot"));
                // Expose interactor type selection
                EditorGUILayout.PropertyField(serializedObject.FindProperty("leftHandInteractorType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rightHandInteractorType"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("initializeLayers"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
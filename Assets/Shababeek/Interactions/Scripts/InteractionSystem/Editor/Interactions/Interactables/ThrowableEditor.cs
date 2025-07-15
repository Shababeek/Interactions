using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Throwable))]
    [CanEditMultipleObjects]
    public class ThrowableEditor : Editor
    {
        private SerializedProperty velocitySampleCountProp;
        private SerializedProperty throwMultiplierProp;
        private SerializedProperty enableAngularVelocityProp;
        private SerializedProperty angularVelocityMultiplierProp;
        
        // Events
        private SerializedProperty onThrowEndProp;
        
        // Debug
        private SerializedProperty isBeingThrownProp;
        private SerializedProperty currentVelocityProp;
        private SerializedProperty lastThrowVelocityProp;
        
        private bool showEvents = true;
        private bool showDebug = true;

        protected void OnEnable()
        {
            velocitySampleCountProp = serializedObject.FindProperty("velocitySampleCount");
            throwMultiplierProp = serializedObject.FindProperty("throwMultiplier");
            enableAngularVelocityProp = serializedObject.FindProperty("enableAngularVelocity");
            angularVelocityMultiplierProp = serializedObject.FindProperty("angularVelocityMultiplier");
            
            onThrowEndProp = serializedObject.FindProperty("onThrowEnd");
            
            isBeingThrownProp = serializedObject.FindProperty("isBeingThrown");
            currentVelocityProp = serializedObject.FindProperty("currentVelocity");
            lastThrowVelocityProp = serializedObject.FindProperty("lastThrowVelocity");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "The Throwable component allows objects to be thrown with realistic physics based on hand movement.",
                MessageType.Info
            );
            
            serializedObject.Update();
            
            // Throw Settings
            EditorGUILayout.LabelField("Throw Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(velocitySampleCountProp, new GUIContent("Velocity Sample Count"));
            EditorGUILayout.PropertyField(throwMultiplierProp, new GUIContent("Throw Multiplier"));
            EditorGUILayout.PropertyField(enableAngularVelocityProp, new GUIContent("Enable Angular Velocity"));
            
            if (enableAngularVelocityProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(angularVelocityMultiplierProp, new GUIContent("Angular Velocity Multiplier"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Events
            showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(showEvents, "Events");
            if (showEvents)
            {
                EditorGUILayout.PropertyField(onThrowEndProp, new GUIContent("On Throw End"));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Debug Information
            showDebug = EditorGUILayout.BeginFoldoutHeaderGroup(showDebug, "Debug Information");
            if (showDebug)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(isBeingThrownProp, new GUIContent("Is Being Thrown"));
                EditorGUILayout.PropertyField(currentVelocityProp, new GUIContent("Current Velocity"));
                EditorGUILayout.PropertyField(lastThrowVelocityProp, new GUIContent("Last Throw Velocity"));
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
} 
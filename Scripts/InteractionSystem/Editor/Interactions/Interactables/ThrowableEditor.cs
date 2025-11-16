using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Throwable))]
    [CanEditMultipleObjects]
    public class ThrowableEditor : Editor
    {
        private SerializedProperty _velocitySampleCountProp;
        private SerializedProperty _throwMultiplierProp;
        private SerializedProperty _enableAngularVelocityProp;
        private SerializedProperty _angularVelocityMultiplierProp;
        
        private SerializedProperty _onThrowEndProp;
        
        private SerializedProperty _isBeingThrownProp;
        private SerializedProperty _currentVelocityProp;
        private SerializedProperty _lastThrowVelocityProp;
        
        private bool _showEvents = true;
        private bool _showDebug = true;

        protected void OnEnable()
        {
            _velocitySampleCountProp = serializedObject.FindProperty("velocitySampleCount");
            _throwMultiplierProp = serializedObject.FindProperty("throwMultiplier");
            _enableAngularVelocityProp = serializedObject.FindProperty("enableAngularVelocity");
            _angularVelocityMultiplierProp = serializedObject.FindProperty("angularVelocityMultiplier");
            
            _onThrowEndProp = serializedObject.FindProperty("onThrowEnd");
            
            _isBeingThrownProp = serializedObject.FindProperty("isBeingThrown");
            _currentVelocityProp = serializedObject.FindProperty("currentVelocity");
            _lastThrowVelocityProp = serializedObject.FindProperty("lastThrowVelocity");
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
            EditorGUILayout.PropertyField(_velocitySampleCountProp, new GUIContent("Velocity Sample Count"));
            EditorGUILayout.PropertyField(_throwMultiplierProp, new GUIContent("Throw Multiplier"));
            EditorGUILayout.PropertyField(_enableAngularVelocityProp, new GUIContent("Enable Angular Velocity"));
            
            if (_enableAngularVelocityProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_angularVelocityMultiplierProp, new GUIContent("Angular Velocity Multiplier"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Events
            _showEvents = EditorGUILayout.BeginFoldoutHeaderGroup(_showEvents, "Events");
            if (_showEvents)
            {
                EditorGUILayout.PropertyField(_onThrowEndProp, new GUIContent("On Throw End"));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Debug Information
            _showDebug = EditorGUILayout.BeginFoldoutHeaderGroup(_showDebug, "Debug Information");
            if (_showDebug)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_isBeingThrownProp, new GUIContent("Is Being Thrown"));
                EditorGUILayout.PropertyField(_currentVelocityProp, new GUIContent("Current Velocity"));
                EditorGUILayout.PropertyField(_lastThrowVelocityProp, new GUIContent("Last Throw Velocity"));
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
} 
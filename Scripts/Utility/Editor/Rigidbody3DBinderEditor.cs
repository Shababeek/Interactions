using UnityEditor;
using UnityEngine;

namespace Shababeek.Utilities.Editors
{
    [CustomEditor(typeof(Rigidbody3DBinder))]
    public class Rigidbody3DBinderEditor : Editor
    {
        private SerializedProperty _rb;
        private SerializedProperty _inputMode;
        private SerializedProperty _vector3Input;
        private SerializedProperty _vector2Input;
        private SerializedProperty _floatInput;
        private SerializedProperty _direction;
        private SerializedProperty _applicationMode;
        private SerializedProperty _useLocalSpace;
        private SerializedProperty _multiplier;
        private SerializedProperty _continuous;

        private void OnEnable()
        {
            _rb = serializedObject.FindProperty("rb");
            _inputMode = serializedObject.FindProperty("inputMode");
            _vector3Input = serializedObject.FindProperty("vector3Input");
            _vector2Input = serializedObject.FindProperty("vector2Input");
            _floatInput = serializedObject.FindProperty("floatInput");
            _direction = serializedObject.FindProperty("direction");
            _applicationMode = serializedObject.FindProperty("applicationMode");
            _useLocalSpace = serializedObject.FindProperty("useLocalSpace");
            _multiplier = serializedObject.FindProperty("multiplier");
            _continuous = serializedObject.FindProperty("continuous");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_rb);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_inputMode);
            EditorGUILayout.Space();

            var mode = (VelocityInputMode)_inputMode.enumValueIndex;

            // Show relevant inputs based on mode
            switch (mode)
            {
                case VelocityInputMode.Vector3:
                    EditorGUILayout.PropertyField(_vector3Input, new GUIContent("Vector3 Variable"));
                    break;

                case VelocityInputMode.Vector2XY:
                case VelocityInputMode.Vector2XZ:
                case VelocityInputMode.Vector2YZ:
                    EditorGUILayout.PropertyField(_vector2Input, new GUIContent("Vector2 Variable"));
                    EditorGUILayout.HelpBox(GetPlaneDescription(mode), MessageType.Info);
                    break;

                case VelocityInputMode.Vector2PlusFloat:
                    EditorGUILayout.PropertyField(_vector2Input, new GUIContent("Vector2 Variable (XZ)"));
                    EditorGUILayout.PropertyField(_floatInput, new GUIContent("Float Variable (Y)"));
                    break;

                case VelocityInputMode.FloatDirection:
                    EditorGUILayout.PropertyField(_floatInput, new GUIContent("Float Variable"));
                    EditorGUILayout.PropertyField(_direction);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Application", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_applicationMode);
            EditorGUILayout.PropertyField(_useLocalSpace);
            EditorGUILayout.PropertyField(_multiplier);
            EditorGUILayout.PropertyField(_continuous);

            serializedObject.ApplyModifiedProperties();
        }

        private string GetPlaneDescription(VelocityInputMode mode)
        {
            return mode switch
            {
                VelocityInputMode.Vector2XY => "X maps to X, Y maps to Y (Z = 0)",
                VelocityInputMode.Vector2XZ => "X maps to X, Y maps to Z (Y = 0)",
                VelocityInputMode.Vector2YZ => "X maps to Y, Y maps to Z (X = 0)",
                _ => ""
            };
        }
    }
}

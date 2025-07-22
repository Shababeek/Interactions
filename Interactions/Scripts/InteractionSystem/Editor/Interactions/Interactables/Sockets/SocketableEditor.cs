using Shababeek.Interactions;
using UnityEngine;
using UnityEditor;
using Shababeek.InteractionSystem.Interactions;

namespace Shababeek.InteractionSystem.Interactions.Editors
{
    [CustomEditor(typeof(Socketable))]
    [CanEditMultipleObjects]
    public class SocketableEditor : Editor
    {
        private SerializedProperty bondsRendererProp;
        private SerializedProperty boundsProp;
        private Socketable socketable;

        private void OnEnable()
        {
            socketable = (Socketable)target;
            bondsRendererProp = serializedObject.FindProperty("bondsRenderer");
            boundsProp = serializedObject.FindProperty("bounds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(bondsRendererProp);
            EditorGUILayout.PropertyField(boundsProp, true);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();
            var bounds = socketable.GetType().GetField("bounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(socketable) as Bounds? ?? new Bounds();
            var center = bounds.center;
            var size = bounds.size;

            // Draw and edit bounds handle (placeholder, as original code may not have had a working handle)
            // You may want to implement a handle here if needed
        }
    }
} 
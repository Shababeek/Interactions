using Shababeek.Interactions;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Draws HapticPatternData flat (curve, duration, strength) instead of as a collapsed
    /// struct foldout, wherever the type appears in an inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(HapticPatternData))]
    public class HapticPatternDataDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var line = position;
            line.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(line, property.FindPropertyRelative("amplitude"));
            line.y += line.height + Spacing;
            EditorGUI.PropertyField(line, property.FindPropertyRelative("duration"));
            line.y += line.height + Spacing;
            EditorGUI.PropertyField(line, property.FindPropertyRelative("strength"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + Spacing * 2;
        }
    }
}

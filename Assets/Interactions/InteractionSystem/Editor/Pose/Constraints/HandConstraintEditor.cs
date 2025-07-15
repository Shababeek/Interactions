using UnityEngine;
using UnityEditor;

namespace Kandooz.InteractionSystem.Core.Editors
{
    [CustomPropertyDrawer(typeof(PoseConstrains))]
    public class HandConstraintPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel++;
            position.y += EditorGUIUtility.singleLineHeight;
            position = DrawPropertyAndIncrementHeight(property,  position, "indexFingerLimits");
            position = DrawPropertyAndIncrementHeight(property,  position, "middleFingerLimits");
            position = DrawPropertyAndIncrementHeight(property,  position, "ringFingerLimits");
            position = DrawPropertyAndIncrementHeight(property,  position, "pinkyFingerLimits");
            position = DrawPropertyAndIncrementHeight(property,  position, "thumbFingerLimits");
            EditorGUI.EndProperty();
            EditorGUI.indentLevel--;
        }

        private static Rect DrawPropertyAndIncrementHeight(SerializedProperty property, Rect position, string proptertyName)
        {
            var height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative(proptertyName));
            position.height = height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(proptertyName));
            position.y += height;
            return position;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("thumbFingerLimits")) * 5;
            height += EditorGUIUtility.singleLineHeight;
            return height;
        }

    }
}
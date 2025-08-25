using Shababeek.Interactions.Animations;
using UnityEngine;
using UnityEditor;

namespace Shababeek.Interactions.Editors
{
    [CustomPropertyDrawer(typeof(PoseData))]
    public class PosePropertyDrawer : PropertyDrawer
    {
        private const string Type = "type";
        private const string Name = "name";
        private const string Open = "open";
        private const string Closed = "closed";


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var line = new Rect(position);
            line.height = EditorGUIUtility.singleLineHeight;
            line = DrawPropertyAndIncreaseHeight(property, line, Type);
            line = DrawPropertyAndIncreaseHeight(property, line, Name);

            EditorGUI.indentLevel++;
            if (IsStatic(property))
            {
                line = DrawPropertyAndIncreaseHeight(property, line, Open);

            }
            else
            {
                line = DrawPropertyAndIncreaseHeight(property, line, Open);
                line = DrawPropertyAndIncreaseHeight(property, line, Closed);

            }
            EditorGUI.indentLevel--;

        }

        private static Rect DrawPropertyAndIncreaseHeight(SerializedProperty property, Rect line,string name,string label="")
        {
            if (string.IsNullOrEmpty(label))
            {
                EditorGUI.PropertyField(line, property.FindPropertyRelative(name));
            }
            else
            {
                EditorGUI.PropertyField(line, property.FindPropertyRelative(name),new GUIContent("animation clip"));
                
            }
            line.y += line.height;
            return line;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsStatic(property))
            {
                return EditorGUIUtility.singleLineHeight * 5;
            }
            return EditorGUIUtility.singleLineHeight * 6; 
        }

        private bool IsStatic(SerializedProperty property)
        {
            return property.FindPropertyRelative("type").enumValueIndex == (int)PoseData.PoseType.Static;
        }
    }

}

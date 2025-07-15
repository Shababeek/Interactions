// NOTE put in a Editor folder

using UnityEngine;
using UnityEditor;

namespace Kandooz.InteractionSystem.Core.Editors
{
    [CustomPropertyDrawer(typeof(FingerConstraints))]
    //TODO : refactor
    public class FingerConstraintDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pos = position;
            pos.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(pos, label);
            EditorGUI.indentLevel++;
            pos.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(pos, property.FindPropertyRelative("locked"));
            pos.y += EditorGUIUtility.singleLineHeight;
            pos.x = position.x;
            pos.width = position.width;
            if (property.FindPropertyRelative("locked").boolValue)
            {
                DrawNormalSlider(pos, property);
            }
            else
            {
                DrawMinMaxSlider(pos, property);
            }
            EditorGUI.indentLevel--;

        }

        private void DrawMinMaxSlider(Rect position, SerializedProperty property)
        {
            var labelPosition = position;
            labelPosition.x += position.width / 4 * 3 + 10;
            labelPosition.width /= 4;
            labelPosition.width -= 5;

            var sliderPosition = position;
            sliderPosition.width *= 3f / 4;
            sliderPosition.width -= 5;
            float minValue = property.FindPropertyRelative("min").floatValue;
            float maxValue = property.FindPropertyRelative("max").floatValue;
            float minLimit = 0;
            float maxLimit = 1;
            EditorGUI.MinMaxSlider(sliderPosition, "limits", ref minValue, ref maxValue, minLimit, maxLimit);
            EditorGUI.LabelField(labelPosition, minValue.ToString("0.0") + " : " + maxValue.ToString("0.0")); ;

            property.FindPropertyRelative("min").floatValue = minValue;
            property.FindPropertyRelative("max").floatValue = maxValue;
        }
        private void DrawNormalSlider(Rect position, SerializedProperty property)
        {
            position.width *= .8f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("min"),new GUIContent("value"));
            property.FindPropertyRelative("max").floatValue = property.FindPropertyRelative("min").floatValue;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float size = EditorGUIUtility.singleLineHeight*3;
            return size;
        }
    }
}
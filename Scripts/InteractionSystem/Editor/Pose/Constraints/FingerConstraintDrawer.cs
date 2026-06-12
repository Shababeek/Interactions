using Shababeek.Interactions.Core;
using UnityEngine;
using UnityEditor;

namespace Shababeek.Interactions.Editors
{
    [CustomPropertyDrawer(typeof(FingerConstraints))]
    public class FingerConstraintDrawer : PropertyDrawer
    {
        private static readonly string[] ModeNames = { "Free", "Range", "Fixed" };

        /// <summary>
        /// Resolves the effective mode, migrating legacy locked/min/max data the same way
        /// FingerConstraints.Mode does at runtime.
        /// </summary>
        internal static FingerConstraintMode ResolveMode(SerializedProperty property)
        {
            var mode = (FingerConstraintMode)property.FindPropertyRelative("mode").intValue;
            if (mode != FingerConstraintMode.Unset) return mode;
            if (property.FindPropertyRelative("locked").boolValue) return FingerConstraintMode.Fixed;
            float min = property.FindPropertyRelative("min").floatValue;
            float max = property.FindPropertyRelative("max").floatValue;
            return min <= 0f && max >= 1f ? FingerConstraintMode.Free : FingerConstraintMode.Range;
        }

        /// <summary>Writes the mode and keeps the legacy locked flag in sync.</summary>
        internal static void WriteMode(SerializedProperty property, FingerConstraintMode mode)
        {
            property.FindPropertyRelative("mode").intValue = (int)mode;
            property.FindPropertyRelative("locked").boolValue = mode == FingerConstraintMode.Fixed;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pos = position;
            pos.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(pos, label);
            EditorGUI.indentLevel++;

            pos.y += EditorGUIUtility.singleLineHeight;
            var mode = ResolveMode(property);
            int modeIndex = Mathf.Clamp((int)mode - 1, 0, 2);
            int newModeIndex = EditorGUI.Popup(pos, "Mode", modeIndex, ModeNames);
            if (newModeIndex != modeIndex || property.FindPropertyRelative("mode").intValue == (int)FingerConstraintMode.Unset)
            {
                mode = (FingerConstraintMode)(newModeIndex + 1);
                WriteMode(property, mode);
            }

            pos.y += EditorGUIUtility.singleLineHeight;
            pos.x = position.x;
            pos.width = position.width;
            switch (mode)
            {
                case FingerConstraintMode.Range:
                    DrawMinMaxSlider(pos, property);
                    break;
                case FingerConstraintMode.Fixed:
                    DrawFixedValueSlider(pos, property);
                    break;
                default:
                    EditorGUI.LabelField(pos, " ", "Full 0–1 input passes through.", EditorStyles.miniLabel);
                    break;
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawMinMaxSlider(Rect position, SerializedProperty property)
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
            EditorGUI.MinMaxSlider(sliderPosition, "limits", ref minValue, ref maxValue, 0f, 1f);
            EditorGUI.LabelField(labelPosition, minValue.ToString("0.00") + " : " + maxValue.ToString("0.00"));

            property.FindPropertyRelative("min").floatValue = minValue;
            property.FindPropertyRelative("max").floatValue = maxValue;
        }

        private static void DrawFixedValueSlider(Rect position, SerializedProperty property)
        {
            // Fixed mode stores its held value in min; max is left untouched so switching
            // back to Range restores the authored range.
            position.width *= .8f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("min"), new GUIContent("Value"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }
    }
}

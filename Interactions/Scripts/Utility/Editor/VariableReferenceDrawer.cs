using UnityEngine;
using UnityEditor;
using System;

namespace Shababeek.Core
{
    /// <summary>
    /// Custom property drawer for VariableReference<T> that provides a clean inspector interface.
    /// Shows a toggle to switch between variable reference and constant value modes.
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableReference<>), true)]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rects
            var useConstantRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, 
                                     position.width, EditorGUIUtility.singleLineHeight);

            // Get the UseConstant property
            var useConstantProperty = property.FindPropertyRelative("useConstant");
            
            // Draw the UseConstant toggle
            EditorGUI.PropertyField(useConstantRect, useConstantProperty, new GUIContent("Use Constant"));
            
            // Draw the appropriate field based on UseConstant
            if (useConstantProperty.boolValue)
            {
                // Draw constant value field
                var constantValueProperty = property.FindPropertyRelative("constantValue");
                if (constantValueProperty != null)
                {
                    EditorGUI.PropertyField(contentRect, constantValueProperty, new GUIContent("Constant Value"));
                }
                else
                {
                    EditorGUI.LabelField(contentRect, "Constant Value", "No constant value field found");
                }
            }
            else
            {
                var variableProperty = property.FindPropertyRelative("variable");
                
                if (variableProperty != null)
                {
                    EditorGUI.PropertyField(contentRect, variableProperty, new GUIContent("Variable"));
                }
                else
                {
                    // Fallback if variable property is not found
                    EditorGUI.LabelField(contentRect, "Variable", "Invalid variable reference");
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Height for the toggle + spacing + height for the content field
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }
} 
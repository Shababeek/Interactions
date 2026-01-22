using UnityEditor;
using UnityEngine;

namespace Shababeek.Utilities.Editors
{
    /// <summary>
    /// Custom property drawer for NumericalReference that provides a clean two-line interface.
    /// </summary>
    /// <remarks>
    /// Features:
    /// - Toggle between constant and variable mode
    /// - Shows float field for constant, object field for variable
    /// - Validates that the assigned variable is a numerical type
    /// </remarks>
    [CustomPropertyDrawer(typeof(NumericalReference))]
    public class NumericalReferenceDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get properties
            var useConstantProp = property.FindPropertyRelative("useConstant");
            var constantValueProp = property.FindPropertyRelative("constantValue");
            var variableProp = property.FindPropertyRelative("variable");

            // Calculate rects
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float labelWidth = EditorGUIUtility.labelWidth;

            // First line: Label + Toggle + Value
            Rect labelRect = new Rect(position.x, position.y, labelWidth, lineHeight);
            Rect toggleRect = new Rect(position.x + labelWidth, position.y, 20f, lineHeight);
            Rect valueRect = new Rect(position.x + labelWidth + 24f, position.y, position.width - labelWidth - 24f, lineHeight);

            // Draw label
            EditorGUI.LabelField(labelRect, label);

            // Draw toggle (use constant vs use variable)
            EditorGUI.BeginChangeCheck();
            bool useConstant = EditorGUI.Toggle(toggleRect, useConstantProp.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                useConstantProp.boolValue = useConstant;
            }

            // Draw tooltip for toggle
            EditorGUI.LabelField(toggleRect, new GUIContent("", useConstant ? "Using constant value" : "Using variable reference"));

            // Draw value field based on mode
            if (useConstant)
            {
                EditorGUI.PropertyField(valueRect, constantValueProp, GUIContent.none);
            }
            else
            {
                // Draw object field for variable
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(valueRect, variableProp, GUIContent.none);

                if (EditorGUI.EndChangeCheck())
                {
                    // Validate that it's a numerical variable
                    var assignedVar = variableProp.objectReferenceValue;
                    if (assignedVar != null && !(assignedVar is INumericalVariable))
                    {
                        Debug.LogWarning($"NumericalReference requires a numerical variable (IntVariable or FloatVariable). '{assignedVar.name}' is not a numerical type.");
                        variableProp.objectReferenceValue = null;
                    }
                }
            }

            // Second line: Mode indicator
            Rect indicatorRect = new Rect(position.x + labelWidth + 24f, position.y + lineHeight + Spacing, position.width - labelWidth - 24f, lineHeight);

            // Draw a subtle indicator of the mode
            var originalColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);

            string modeText = useConstant ? "Constant" : "Variable";
            if (!useConstant && variableProp.objectReferenceValue != null)
            {
                var variable = variableProp.objectReferenceValue as ScriptableVariable;
                if (variable != null)
                {
                    // Show the variable's current value
                    var numVar = variable as INumericalVariable;
                    if (numVar != null)
                    {
                        modeText = $"{variable.GetType().Name.Replace("Variable", "")} = {numVar.AsFloat:F2}";
                    }
                }
            }

            EditorGUI.LabelField(indicatorRect, modeText, EditorStyles.miniLabel);
            GUI.color = originalColor;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Two lines: main control + mode indicator
            return EditorGUIUtility.singleLineHeight * 2 + Spacing;
        }
    }
}

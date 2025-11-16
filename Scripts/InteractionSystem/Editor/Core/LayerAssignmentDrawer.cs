using UnityEditor;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions.Editors
{
    [CustomPropertyDrawer(typeof(LayerAssignment))]
    public class LayerAssignmentDrawer : PropertyDrawer
    {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Get the target and layer properties
            SerializedProperty targetProp = property.FindPropertyRelative("target");
            SerializedProperty layerProp = property.FindPropertyRelative("layer");
            
            // Calculate positions for vertical layout
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            Rect labelRect = new Rect(position.x, position.y, position.width, lineHeight);
            Rect targetRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
            Rect layerRect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width, lineHeight);
            
            // Draw label
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
            
            // Draw target field
            EditorGUI.PropertyField(targetRect, targetProp, new GUIContent("Target", "The transform to assign to the layer"));
            
            // Draw layer dropdown using Utilities
            int currentLayer = layerProp.intValue;
            int newLayer = EditorUtilities.LayerDropdown(layerRect, new GUIContent("Layer", "The layer to assign the target to"), currentLayer);
            
            if (newLayer != currentLayer)
            {
                layerProp.intValue = newLayer;
            }
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            return lineHeight * 3 + spacing * 2; // Label + Target + Layer + spacing
        }
        

    }
} 
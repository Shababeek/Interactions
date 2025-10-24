using System;
using Shababeek.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Extension methods for type information.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the declared type of an object.
        /// </summary>
        public static Type GetDeclaredType<T>(this T obj )
        {
            return typeof( T );
        }
    }
    
    /// <summary>
    /// Custom property drawer for ScriptableVariable that adds a "Find Asset" button.
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableVariable<>), true)]
    public class VariableDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 80f;
        private const float Spacing = 5f;
        
        /// <summary>
        /// Renders the property field with an optional Find Asset button.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Calculate rects
            var propertyRect = new Rect(position.x, position.y, position.width - ButtonWidth - Spacing, EditorGUIUtility.singleLineHeight);
            var findButtonRect = new Rect(position.x + position.width - ButtonWidth, position.y, ButtonWidth, EditorGUIUtility.singleLineHeight);
            
            EditorGUI.PropertyField(propertyRect, property, label);
            
            // Draw Find Asset button if no object is assigned
            if (property.objectReferenceValue == null)
            {
                if (GUI.Button(findButtonRect, "Find Asset"))
                {
                    // Extract type from property type (e.g., "PPtr<$FloatVariable>" -> "FloatVariable")
                    var type = property.type.Substring(6);
                    type = type.Substring(0, type.Length - 1);
                    
                    var assets = AssetDatabase.FindAssets($"t:{type} {property.name}");
                    if (assets.Length > 0)
                    {
                        property.objectReferenceValue = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]));
                        Debug.Log($"Found asset: {property.objectReferenceValue.name}");
                    }
                }
            }
            
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Gets the height of the property field.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        /// <summary>
        /// Creates the UI Toolkit property GUI.
        /// </summary>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Debug.Log(property.objectReferenceValue);
            var container = new VisualElement();
            var isShared = new Toggle("IsShared");
            container.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            container.Add(new PropertyField(property));
            if (property.objectReferenceValue == null)
            {
                var findButton = new Button(() =>
                {
                    //PPtr<$FloatVariable>
                    var type = property.type.Substring(6);
                    type = type.Substring(0, type.Length - 1);
                    var assets = AssetDatabase.FindAssets($"t:{type} {property.name}");
                    if (assets.Length == 0) return;
                    property.objectReferenceValue = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[0]));
                    Debug.Log(property.objectReferenceValue.name);
                })
                {
                    text = "Find Asset"
                };
                container.Add(findButton);
            }

            return container;
        }
    }
}
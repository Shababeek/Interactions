using System;
using Shababeek.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Interactions.Editors
{    public static class Extensions
    {
        public static Type GetDeclaredType<T>(this T obj )
        {
            return typeof( T );
        }
    }
    [CustomPropertyDrawer(typeof(ScriptableVariable<>), true)]
    public class VariableDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 80f;
        private const float Spacing = 5f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Calculate rects
            var propertyRect = new Rect(position.x, position.y, position.width - ButtonWidth * 2 - Spacing * 2, EditorGUIUtility.singleLineHeight);
            var findButtonRect = new Rect(position.x + position.width - ButtonWidth * 2 - Spacing, position.y, ButtonWidth, EditorGUIUtility.singleLineHeight);
            var createButtonRect = new Rect(position.x + position.width - ButtonWidth, position.y, ButtonWidth, EditorGUIUtility.singleLineHeight);
            
            // Draw the property field
            EditorGUI.PropertyField(propertyRect, property, label);
            
            // Draw buttons if no object is assigned
            if (property.objectReferenceValue == null)
            {
                // Find Asset button
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
                
                // Create Asset button
                if (GUI.Button(createButtonRect, "Create Asset"))
                {
                    // TODO: Implement asset creation logic
                    Debug.Log("Create Asset clicked - implement creation logic");
                }
            }
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
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
                    text = "FindAsset"
                };
                var createButton = new Button(() => { });
                container.Add(findButton);
                findButton.text = "Create Asset";
                container.Add(createButton);
            }

            return container;
        }
    }
}
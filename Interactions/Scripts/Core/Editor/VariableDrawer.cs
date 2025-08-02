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
    [CustomPropertyDrawer(typeof(ScriptableVariable), true)]
    public class VariableDrawer : PropertyDrawer
    {
        
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
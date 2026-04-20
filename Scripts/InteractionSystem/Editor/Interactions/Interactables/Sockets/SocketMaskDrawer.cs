using System.IO;
using Shababeek.Interactions;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomPropertyDrawer(typeof(SocketMask))]
    public class SocketMaskDrawer : PropertyDrawer
    {
        private const string DefaultDir = "Assets/_Shababeek/Hands/InteractionSystem";
        private const string DefaultAssetName = "SocketMaskRegistry.asset";

        private static SocketMaskRegistry _cached;
        private static double _lastLookup;

        private static SocketMaskRegistry GetRegistry()
        {
            if (_cached != null) return _cached;
            if (EditorApplication.timeSinceStartup - _lastLookup < 2.0) return _cached;
            _lastLookup = EditorApplication.timeSinceStartup;

            var guids = AssetDatabase.FindAssets("t:SocketMaskRegistry");
            if (guids != null && guids.Length > 0)
            {
                _cached = AssetDatabase.LoadAssetAtPath<SocketMaskRegistry>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            return _cached;
        }

        private static SocketMaskRegistry CreateRegistry()
        {
            var dir = DefaultDir;
            if (!AssetDatabase.IsValidFolder(dir))
            {
                dir = "Assets";
            }
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(dir, DefaultAssetName).Replace('\\', '/'));
            var asset = ScriptableObject.CreateInstance<SocketMaskRegistry>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _cached = asset;
            return asset;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("value");
            if (valueProp == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Invalid SocketMask"));
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var registry = GetRegistry();
            if (registry == null)
            {
                var labelRect = new Rect(position.x, position.y, position.width - 70, position.height);
                var btnRect = new Rect(position.xMax - 68, position.y, 68, position.height);
                EditorGUI.LabelField(labelRect, label, new GUIContent("No Registry"));
                if (GUI.Button(btnRect, "Create"))
                {
                    registry = CreateRegistry();
                    Selection.activeObject = registry;
                    EditorGUIUtility.PingObject(registry);
                }
            }
            else
            {
                var names = registry.GetDisplayNames();
                EditorGUI.BeginChangeCheck();
                int newValue = EditorGUI.MaskField(position, label, valueProp.intValue, names);
                if (EditorGUI.EndChangeCheck())
                {
                    valueProp.intValue = newValue;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomPropertyDrawer(typeof(SingleSocketCategoryAttribute))]
    public class SingleSocketCategoryDrawer : PropertyDrawer
    {
        private const string DefaultDir = "Assets/_Shababeek/Hands/InteractionSystem";
        private const string DefaultAssetName = "SocketMaskRegistry.asset";
        private const int AnyIndex = -1;

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
                // Build popup options: "Any" + each named slot.
                var options = new GUIContent[names.Length + 1];
                options[0] = new GUIContent("Any");
                for (int i = 0; i < names.Length; i++) options[i + 1] = new GUIContent(names[i]);

                int currentIndex = BitIndex(valueProp.intValue);
                int popupIndex = currentIndex + 1; // shift for "Any" at slot 0

                EditorGUI.BeginChangeCheck();
                int newPopup = EditorGUI.Popup(position, label, popupIndex, options);
                if (EditorGUI.EndChangeCheck())
                {
                    int newBitIndex = newPopup - 1;
                    valueProp.intValue = newBitIndex < 0 ? 0 : (1 << newBitIndex);
                }
            }

            EditorGUI.EndProperty();
        }

        // Returns the lowest set bit index, or -1 if no bit set / invalid.
        private static int BitIndex(int mask)
        {
            if (mask == 0) return AnyIndex;
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0) return i;
            }
            return AnyIndex;
        }
    }
}

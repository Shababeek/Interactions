using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Shababeek.Utilities.Editors
{
    [CustomEditor(typeof(VariableContainer))]
    public class VariableContainerEditor : Editor
    {
        private VariableContainer _container;
        private ReorderableList _variableList;
        private ReorderableList _eventList;

        // Foldout states
        private bool _showVariables = true;
        private bool _showEvents = true;

        // Cached serialized objects for sub-assets
        private readonly Dictionary<UnityEngine.Object, SerializedObject> _serializedCache = new();

        // Built-in variable types with known categories
        private static readonly Dictionary<Type, (string displayName, string category)> BuiltInTypes = new()
        {
            // Primitives
            { typeof(IntVariable), ("Int", "Primitives") },
            { typeof(FloatVariable), ("Float", "Primitives") },
            { typeof(BoolVariable), ("Bool", "Primitives") },
            { typeof(TextVariable), ("Text", "Primitives") },

            // Vectors
            { typeof(Vector2Variable), ("Vector2", "Vectors") },
            { typeof(Vector2IntVariable), ("Vector2Int", "Vectors") },
            { typeof(Vector3Variable), ("Vector3", "Vectors") },
            { typeof(QuaternionVariable), ("Quaternion", "Vectors") },

            // Graphics
            { typeof(ColorVariable), ("Color", "Graphics") },
            { typeof(GradientVariable), ("Gradient", "Graphics") },
            { typeof(AnimationCurveVariable), ("AnimationCurve", "Graphics") },

            // References
            { typeof(GameObjectVariable), ("GameObject", "References") },
            { typeof(TransformVariable), ("Transform", "References") },
            { typeof(AudioClipVariable), ("AudioClip", "References") },

            // Other
            { typeof(LayerMaskVariable), ("LayerMask", "Other") },
        };

        // Cached list of all discovered variable types (built-in + external)
        private static List<(Type type, string displayName, string category)> _allVariableTypes;

        private static List<(Type type, string displayName, string category)> AllVariableTypes
        {
            get
            {
                if (_allVariableTypes == null)
                    RefreshVariableTypes();
                return _allVariableTypes;
            }
        }

        private static void RefreshVariableTypes()
        {
            _allVariableTypes = new List<(Type, string, string)>();

            // Add built-in types first in their known categories
            foreach (var kvp in BuiltInTypes)
            {
                _allVariableTypes.Add((kvp.Key, kvp.Value.displayName, kvp.Value.category));
            }

            // Discover all concrete ScriptableVariable subclasses via TypeCache
            var discoveredTypes = TypeCache.GetTypesDerivedFrom<ScriptableVariable>();
            foreach (var type in discoveredTypes)
            {
                if (type.IsAbstract || type.IsGenericType) continue;
                if (BuiltInTypes.ContainsKey(type)) continue;

                string displayName = type.Name;
                if (displayName.EndsWith("Variable"))
                    displayName = displayName.Substring(0, displayName.Length - 8);

                _allVariableTypes.Add((type, displayName, "Custom"));
            }
        }

        private void OnEnable()
        {
            _container = (VariableContainer)target;
            SetupVariableList();
            SetupEventList();
        }

        private void OnDisable()
        {
            _serializedCache.Clear();
        }

        // ==================== LIST SETUP ====================

        private void SetupVariableList()
        {
            var prop = serializedObject.FindProperty("variables");
            _variableList = new ReorderableList(serializedObject, prop, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Variables", EditorStyles.boldLabel),
                drawElementCallback = DrawVariableElement,
                elementHeightCallback = GetVariableHeight,
                onAddDropdownCallback = OnAddVariableDropdown,
                onRemoveCallback = OnRemoveVariable
            };
        }

        private void SetupEventList()
        {
            var prop = serializedObject.FindProperty("events");
            _eventList = new ReorderableList(serializedObject, prop, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Events", EditorStyles.boldLabel),
                drawElementCallback = DrawEventElement,
                elementHeightCallback = _ => EditorGUIUtility.singleLineHeight + 4f,
                onAddCallback = OnAddEvent,
                onRemoveCallback = OnRemoveEvent
            };
        }

        // ==================== VARIABLE DRAWING ====================

        private float GetVariableHeight(int index)
        {
            if (index < 0 || index >= _container.VariableCount) return EditorGUIUtility.singleLineHeight;

            var variable = _container.GetVariable(index);
            if (variable == null) return EditorGUIUtility.singleLineHeight + 4f;

            var so = GetSerializedObject(variable);
            var valueProp = so.FindProperty("value");

            float height = EditorGUIUtility.singleLineHeight;
            if (valueProp != null)
            {
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(valueProp, true));
            }
            return height + 6f;
        }

        private void DrawVariableElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _container.VariableCount) return;

            var variable = _container.GetVariable(index);
            if (variable == null)
            {
                EditorGUI.LabelField(rect, "(Missing Reference)", EditorStyles.miniLabel);
                return;
            }

            rect.y += 2f;
            rect.height -= 4f;

            // Layout: [Name 30%] [Type 15%] [Value 55%]
            float nameWidth = rect.width * 0.30f - 4f;
            float typeWidth = rect.width * 0.15f - 4f;
            float valueWidth = rect.width * 0.55f - 4f;

            var nameRect = new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight);
            var typeRect = new Rect(rect.x + nameWidth + 4f, rect.y, typeWidth, EditorGUIUtility.singleLineHeight);
            var valueRect = new Rect(rect.x + nameWidth + typeWidth + 8f, rect.y, valueWidth, rect.height);

            // Name field (editable)
            string displayName = GetDisplayName(variable.name);
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.TextField(nameRect, displayName);
            if (EditorGUI.EndChangeCheck() && newName != displayName && !string.IsNullOrWhiteSpace(newName))
            {
                RenameSubAsset(variable, newName);
            }

            // Type label (read-only)
            string typeName = GetTypeName(variable.GetType());
            GUI.enabled = false;
            EditorGUI.TextField(typeRect, typeName);
            GUI.enabled = true;

            // Value field
            DrawValueField(valueRect, variable);
        }

        private void DrawValueField(Rect rect, ScriptableVariable variable)
        {
            var so = GetSerializedObject(variable);
            so.Update();

            var valueProp = so.FindProperty("value");
            if (valueProp != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, valueProp, GUIContent.none, true);
                if (EditorGUI.EndChangeCheck())
                {
                    so.ApplyModifiedProperties();
                    variable.Raise(); // Notify listeners of change
                }
            }
            else
            {
                EditorGUI.LabelField(rect, variable.ToString(), EditorStyles.miniLabel);
            }
        }

        // ==================== EVENT DRAWING ====================

        private void DrawEventElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _container.EventCount) return;

            var evt = _container.GetEvent(index);
            if (evt == null)
            {
                EditorGUI.LabelField(rect, "(Missing Reference)", EditorStyles.miniLabel);
                return;
            }

            rect.y += 2f;
            rect.height -= 4f;

            // Layout: [Name 60%] [Raise Button 40%]
            float nameWidth = rect.width * 0.60f - 4f;
            float buttonWidth = rect.width * 0.40f - 4f;

            var nameRect = new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight);
            var buttonRect = new Rect(rect.x + nameWidth + 4f, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            // Name field (editable)
            string displayName = GetDisplayName(evt.name);
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.TextField(nameRect, displayName);
            if (EditorGUI.EndChangeCheck() && newName != displayName && !string.IsNullOrWhiteSpace(newName))
            {
                RenameSubAsset(evt, newName);
            }

            // Raise button
            if (GUI.Button(buttonRect, "Raise"))
            {
                evt.Raise();
            }
        }

        // ==================== ADD/REMOVE CALLBACKS ====================

        private void OnAddVariableDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();

            foreach (var (type, displayName, category) in AllVariableTypes)
            {
                Type capturedType = type;
                string capturedName = displayName;

                menu.AddItem(
                    new GUIContent($"{category}/{displayName}"),
                    false,
                    () => AddVariable(capturedType, capturedName)
                );
            }

            menu.ShowAsContext();
        }

        private void AddVariable(Type type, string baseName)
        {
            string assetPath = AssetDatabase.GetAssetPath(_container);

            // Create the variable instance
            var variable = (ScriptableVariable)CreateInstance(type);
            variable.name = GenerateUniqueName(baseName, isVariable: true);

            // Add as sub-asset
            Undo.RecordObject(_container, "Add Variable");
            AssetDatabase.AddObjectToAsset(variable, assetPath);
            _container.EditorAddVariable(variable);

            // Refresh
            serializedObject.Update();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
        }

        private void OnRemoveVariable(ReorderableList list)
        {
            int index = list.index;
            if (index < 0 || index >= _container.VariableCount) return;

            var variable = _container.GetVariable(index);

            Undo.RecordObject(_container, "Remove Variable");
            _container.EditorRemoveVariableAt(index);

            if (variable != null)
            {
                _serializedCache.Remove(variable);
                AssetDatabase.RemoveObjectFromAsset(variable);
                DestroyImmediate(variable, true);
            }

            serializedObject.Update();
            AssetDatabase.SaveAssets();
        }

        private void OnAddEvent(ReorderableList list)
        {
            string assetPath = AssetDatabase.GetAssetPath(_container);

            // Create GameEvent instance
            var evt = CreateInstance<GameEvent>();
            evt.name = GenerateUniqueName("Event", isVariable: false);

            // Add as sub-asset
            Undo.RecordObject(_container, "Add Event");
            AssetDatabase.AddObjectToAsset(evt, assetPath);
            _container.EditorAddEvent(evt);

            // Refresh
            serializedObject.Update();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
        }

        private void OnRemoveEvent(ReorderableList list)
        {
            int index = list.index;
            if (index < 0 || index >= _container.EventCount) return;

            var evt = _container.GetEvent(index);

            Undo.RecordObject(_container, "Remove Event");
            _container.EditorRemoveEventAt(index);

            if (evt != null)
            {
                _serializedCache.Remove(evt);
                AssetDatabase.RemoveObjectFromAsset(evt);
                DestroyImmediate(evt, true);
            }

            serializedObject.Update();
            AssetDatabase.SaveAssets();
        }

        // ==================== UTILITY METHODS ====================

        private SerializedObject GetSerializedObject(UnityEngine.Object obj)
        {
            if (!_serializedCache.TryGetValue(obj, out var so))
            {
                so = new SerializedObject(obj);
                _serializedCache[obj] = so;
            }
            return so;
        }

        private string GetDisplayName(string fullName)
        {
            // Remove "ContainerName_" prefix if present
            string prefix = _container.name + "_";
            return fullName.StartsWith(prefix) ? fullName.Substring(prefix.Length) : fullName;
        }

        private string GetTypeName(Type type)
        {
            if (BuiltInTypes.TryGetValue(type, out var info))
                return info.displayName;

            string name = type.Name;
            return name.EndsWith("Variable") ? name.Substring(0, name.Length - 8) : name;
        }

        private void RenameSubAsset(ScriptableObject asset, string newDisplayName)
        {
            string newFullName = $"{_container.name}_{newDisplayName}";
            Undo.RecordObject(asset, "Rename Asset");
            asset.name = newFullName;
            EditorUtility.SetDirty(asset);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_container));
        }

        private string GenerateUniqueName(string baseName, bool isVariable)
        {
            string prefix = _container.name + "_";
            string fullName = prefix + baseName;
            int counter = 1;

            // Check for existing names
            while (isVariable ? _container.HasVariable(fullName) : _container.HasEvent(fullName))
            {
                fullName = $"{prefix}{baseName}_{counter}";
                counter++;
            }
            return fullName;
        }

        private void ClearOrphanedSubAssets()
        {
            string path = AssetDatabase.GetAssetPath(_container);
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            // Build set of referenced assets
            var referenced = new HashSet<UnityEngine.Object> { _container };
            foreach (var v in _container.Variables) if (v != null) referenced.Add(v);
            foreach (var e in _container.Events) if (e != null) referenced.Add(e);

            // Find orphans
            var orphans = allAssets
                .Where(a => a != null && !referenced.Contains(a))
                .Where(a => a is ScriptableVariable || a is GameEvent)
                .ToList();

            if (orphans.Count == 0)
            {
                EditorUtility.DisplayDialog("No Orphans", "No orphaned sub-assets found.", "OK");
                return;
            }

            // Confirm
            string names = string.Join("\n  - ", orphans.Select(o => o.name));
            if (!EditorUtility.DisplayDialog("Clear Orphaned Sub-Assets",
                $"Found {orphans.Count} orphaned sub-assets:\n  - {names}\n\nDelete them?",
                "Delete", "Cancel"))
            {
                return;
            }

            // Delete
            foreach (var orphan in orphans)
            {
                _serializedCache.Remove(orphan);
                AssetDatabase.RemoveObjectFromAsset(orphan);
                DestroyImmediate(orphan, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            Debug.Log($"Deleted {orphans.Count} orphaned sub-assets from {_container.name}");
        }

        // ==================== INSPECTOR GUI ====================

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Container for Variables and Events stored as sub-assets.\n" +
                "Names are prefixed with the container name.",
                MessageType.Info);
            EditorGUILayout.Space(5);

            // Variables section
            _showVariables = EditorGUILayout.Foldout(_showVariables, $"Variables ({_container.VariableCount})", true);
            if (_showVariables)
            {
                _variableList.DoLayoutList();
            }

            EditorGUILayout.Space(10);

            // Events section
            _showEvents = EditorGUILayout.Foldout(_showEvents, $"Events ({_container.EventCount})", true);
            if (_showEvents)
            {
                _eventList.DoLayoutList();
            }

            EditorGUILayout.Space(10);

            // Utility buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cleanup Nulls", GUILayout.Height(24)))
            {
                _container.EditorCleanupNulls();
                serializedObject.Update();
            }

            if (GUILayout.Button("Clear Orphans", GUILayout.Height(24)))
            {
                ClearOrphanedSubAssets();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Raise All Variables", GUILayout.Height(24)))
            {
                _container.RaiseAllVariables();
            }

            if (GUILayout.Button("Raise All Events", GUILayout.Height(24)))
            {
                _container.RaiseAllEvents();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

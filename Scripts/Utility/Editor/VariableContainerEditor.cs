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
        private ReorderableList _variableList;
        private ReorderableList _eventList;
        private VariableContainer _container;
        private List<Type> _variableTypes;
        private List<Type> _eventTypes;
        private Dictionary<Type, string> _typeDisplayNames;
        private Dictionary<UnityEngine.Object, SerializedObject> _cachedSerializedObjects = new();
        private bool _showVariables = true;
        private bool _showEvents = true;

        private void OnEnable()
        {
            _container = (VariableContainer)target;
            CacheTypes();
            SetupVariableList();
            SetupEventList();
        }

        private void OnDisable()
        {
            _cachedSerializedObjects.Clear();
        }

        private void CacheTypes()
        {
            _variableTypes = new List<Type>();
            _eventTypes = new List<Type>();
            _typeDisplayNames = new Dictionary<Type, string>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Cache variable types
                    var varTypes = assembly.GetTypes()
                        .Where(t => typeof(ScriptableVariable).IsAssignableFrom(t)
                                    && !t.IsAbstract && t != typeof(ScriptableVariable)
                                    && !t.IsGenericType);

                    foreach (var type in varTypes)
                    {
                        _variableTypes.Add(type);
                        string name = type.Name;
                        if (name.EndsWith("Variable")) name = name.Substring(0, name.Length - 8);
                        _typeDisplayNames[type] = name;
                    }

                    // Cache event types
                    var eventTypes = assembly.GetTypes()
                        .Where(t => typeof(GameEvent).IsAssignableFrom(t)
                                    && !t.IsAbstract && !t.IsGenericType);

                    foreach (var type in eventTypes)
                    {
                        _eventTypes.Add(type);
                        string name = type.Name;
                        if (name.EndsWith("Event")) name = name.Substring(0, name.Length - 5);
                        _typeDisplayNames[type] = name;
                    }
                }
                catch { }
            }

            _variableTypes = _variableTypes.OrderBy(t => _typeDisplayNames[t]).ToList();
            _eventTypes = _eventTypes.OrderBy(t => _typeDisplayNames[t]).ToList();
        }

        private void SetupVariableList()
        {
            var prop = serializedObject.FindProperty("variables");
            _variableList = new ReorderableList(serializedObject, prop, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Variables", EditorStyles.boldLabel),
                drawElementCallback = DrawVariableElement,
                elementHeightCallback = GetVariableElementHeight,
                onAddDropdownCallback = ShowVariableDropdown,
                onRemoveCallback = RemoveVariable
            };
        }

        private void SetupEventList()
        {
            var prop = serializedObject.FindProperty("events");
            _eventList = new ReorderableList(serializedObject, prop, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Events", EditorStyles.boldLabel),
                drawElementCallback = DrawEventElement,
                elementHeightCallback = GetEventElementHeight,
                onAddDropdownCallback = ShowEventDropdown,
                onRemoveCallback = RemoveEvent
            };
        }

        private float GetVariableElementHeight(int index)
        {
            var prop = _variableList.serializedProperty.GetArrayElementAtIndex(index);
            var variable = prop.objectReferenceValue as ScriptableVariable;
            if (variable == null) return EditorGUIUtility.singleLineHeight + 4f;

            if (!_cachedSerializedObjects.TryGetValue(variable, out var so))
            {
                so = new SerializedObject(variable);
                _cachedSerializedObjects[variable] = so;
            }

            var valueProp = so.FindProperty("value");
            if (valueProp != null)
            {
                return Mathf.Max(EditorGUI.GetPropertyHeight(valueProp, true), EditorGUIUtility.singleLineHeight) + 6f;
            }
            return EditorGUIUtility.singleLineHeight + 4f;
        }

        private float GetEventElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }

        private void DrawVariableElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var prop = _variableList.serializedProperty.GetArrayElementAtIndex(index);
            var variable = prop.objectReferenceValue as ScriptableVariable;

            if (variable == null)
            {
                EditorGUI.LabelField(rect, "(Missing)", EditorStyles.miniLabel);
                return;
            }

            rect.y += 2f;
            rect.height -= 4f;

            float nameWidth = rect.width * 0.30f - 4f;
            float typeWidth = rect.width * 0.15f - 4f;
            float valueWidth = rect.width * 0.55f - 4f;

            var nameRect = new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight);
            var typeRect = new Rect(rect.x + nameWidth + 4f, rect.y, typeWidth, EditorGUIUtility.singleLineHeight);
            var valueRect = new Rect(rect.x + nameWidth + typeWidth + 8f, rect.y, valueWidth, rect.height);

            // Name field - extract the variable name without container prefix
            string displayName = GetDisplayName(variable.name);
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.TextField(nameRect, displayName);
            if (EditorGUI.EndChangeCheck() && newName != displayName)
            {
                RenameAsset(variable, newName);
            }

            // Type label
            string typeName = _typeDisplayNames.TryGetValue(variable.GetType(), out var dn) ? dn : variable.GetType().Name;
            GUI.enabled = false;
            EditorGUI.TextField(typeRect, typeName);
            GUI.enabled = true;

            // Value field
            DrawValueField(valueRect, variable);
        }

        private void DrawEventElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var prop = _eventList.serializedProperty.GetArrayElementAtIndex(index);
            var evt = prop.objectReferenceValue as GameEvent;

            if (evt == null)
            {
                EditorGUI.LabelField(rect, "(Missing)", EditorStyles.miniLabel);
                return;
            }

            rect.y += 2f;
            rect.height -= 4f;

            float nameWidth = rect.width * 0.50f - 4f;
            float typeWidth = rect.width * 0.30f - 4f;
            float buttonWidth = rect.width * 0.20f - 4f;

            var nameRect = new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight);
            var typeRect = new Rect(rect.x + nameWidth + 4f, rect.y, typeWidth, EditorGUIUtility.singleLineHeight);
            var buttonRect = new Rect(rect.x + nameWidth + typeWidth + 8f, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            // Name field
            string displayName = GetDisplayName(evt.name);
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.TextField(nameRect, displayName);
            if (EditorGUI.EndChangeCheck() && newName != displayName)
            {
                RenameAsset(evt, newName);
            }

            // Type label
            string typeName = _typeDisplayNames.TryGetValue(evt.GetType(), out var dn) ? dn : evt.GetType().Name;
            GUI.enabled = false;
            EditorGUI.TextField(typeRect, typeName);
            GUI.enabled = true;

            // Raise button
            if (GUI.Button(buttonRect, "Raise"))
            {
                evt.Raise();
            }
        }

        private string GetDisplayName(string fullName)
        {
            // Remove "ContainerName_" prefix if present
            string prefix = _container.name + "_";
            if (fullName.StartsWith(prefix))
            {
                return fullName.Substring(prefix.Length);
            }
            return fullName;
        }

        private void RenameAsset(ScriptableObject asset, string newDisplayName)
        {
            string newFullName = $"{_container.name}_{newDisplayName}";
            Undo.RecordObject(asset, "Rename Asset");
            asset.name = newFullName;
            EditorUtility.SetDirty(asset);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_container));
        }

        private void DrawValueField(Rect rect, ScriptableVariable variable)
        {
            if (!_cachedSerializedObjects.TryGetValue(variable, out var so))
            {
                so = new SerializedObject(variable);
                _cachedSerializedObjects[variable] = so;
            }

            so.Update();
            var valueProp = so.FindProperty("value");
            if (valueProp != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, valueProp, GUIContent.none, true);
                if (EditorGUI.EndChangeCheck())
                {
                    so.ApplyModifiedProperties();
                    variable.Raise();
                }
            }
            else
            {
                EditorGUI.LabelField(rect, variable.ToString(), EditorStyles.miniLabel);
            }
        }

        private void ShowVariableDropdown(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();
            var primitives = new[] { "Int", "Float", "Bool", "Text" };
            var vectors = new[] { "Vector2", "Vector2Int", "Vector3", "Quaternion" };
            var graphics = new[] { "Color", "Gradient", "AnimationCurve" };

            foreach (var type in _variableTypes)
            {
                string name = _typeDisplayNames[type];
                string category = primitives.Any(p => name.StartsWith(p)) ? "Primitives/" :
                                  vectors.Any(v => name.StartsWith(v)) ? "Vectors/" :
                                  graphics.Any(g => name.StartsWith(g)) ? "Graphics/" : "Other/";

                menu.AddItem(new GUIContent(category + name), false, () => AddVariable(type));
            }
            menu.ShowAsContext();
        }

        private void ShowEventDropdown(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();
            foreach (var type in _eventTypes)
            {
                string name = _typeDisplayNames[type];
                menu.AddItem(new GUIContent(name), false, () => AddEvent(type));
            }
            menu.ShowAsContext();
        }

        private void AddVariable(Type type)
        {
            var path = AssetDatabase.GetAssetPath(_container);
            var variable = (ScriptableVariable)CreateInstance(type);

            string baseName = _typeDisplayNames.TryGetValue(type, out var dn) ? dn : type.Name;
            variable.name = GenerateUniqueName(baseName, true);

            Undo.RecordObject(_container, "Add Variable");
            AssetDatabase.AddObjectToAsset(variable, path);
            _container.EditorAddVariable(variable);

            serializedObject.Update();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
        }

        private void AddEvent(Type type)
        {
            var path = AssetDatabase.GetAssetPath(_container);
            var evt = (GameEvent)CreateInstance(type);

            string baseName = _typeDisplayNames.TryGetValue(type, out var dn) ? dn : type.Name;
            evt.name = GenerateUniqueName(baseName, false);

            Undo.RecordObject(_container, "Add Event");
            AssetDatabase.AddObjectToAsset(evt, path);
            _container.EditorAddEvent(evt);

            serializedObject.Update();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
        }

        private string GenerateUniqueName(string baseName, bool isVariable)
        {
            string prefix = _container.name + "_";
            string fullName = prefix + baseName;
            int counter = 1;

            while ((isVariable && _container.HasVariable(fullName)) || (!isVariable && _container.HasEvent(fullName)))
            {
                fullName = $"{prefix}{baseName}_{counter}";
                counter++;
            }
            return fullName;
        }

        private void RemoveVariable(ReorderableList list)
        {
            int index = list.index;
            if (index < 0 || index >= _container.VariableCount) return;

            var variable = _container.GetVariable(index);
            Undo.RecordObject(_container, "Remove Variable");
            _container.EditorRemoveVariableAt(index);

            if (variable != null)
            {
                _cachedSerializedObjects.Remove(variable);
                AssetDatabase.RemoveObjectFromAsset(variable);
                DestroyImmediate(variable, true);
            }

            serializedObject.Update();
            AssetDatabase.SaveAssets();
        }

        private void RemoveEvent(ReorderableList list)
        {
            int index = list.index;
            if (index < 0 || index >= _container.EventCount) return;

            var evt = _container.GetEvent(index);
            Undo.RecordObject(_container, "Remove Event");
            _container.EditorRemoveEventAt(index);

            if (evt != null)
            {
                _cachedSerializedObjects.Remove(evt);
                AssetDatabase.RemoveObjectFromAsset(evt);
                DestroyImmediate(evt, true);
            }

            serializedObject.Update();
            AssetDatabase.SaveAssets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Container holds Variables and Events as sub-assets.\n" +
                "Names are automatically prefixed with container name.",
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
            if (GUILayout.Button("Cleanup Nulls", GUILayout.Height(25)))
            {
                _container.EditorCleanupNulls();
                serializedObject.Update();
            }
            if (GUILayout.Button("Raise All Vars", GUILayout.Height(25)))
            {
                _container.RaiseAllVariables();
            }
            if (GUILayout.Button("Raise All Events", GUILayout.Height(25)))
            {
                _container.RaiseAllEvents();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

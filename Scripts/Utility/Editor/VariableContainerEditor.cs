using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Shababeek.Utilities.Editors
{
    /// <summary>
    /// Custom editor for VariableContainer that provides a dynamic interface for managing variables.
    /// </summary>
    /// <remarks>
    /// Features:
    /// - Reorderable list of variables with inline editing
    /// - Dynamic type dropdown populated via reflection
    /// - Sub-asset management (create/delete within the same asset file)
    /// - Inline name and value editing
    /// </remarks>
    [CustomEditor(typeof(VariableContainer))]
    public class VariableContainerEditor : Editor
    {
        private ReorderableList _variableList;
        private VariableContainer _container;
        private List<Type> _variableTypes;
        private Dictionary<Type, string> _typeDisplayNames;

        // Cached SerializedObjects for each variable to enable inline editing
        private Dictionary<UnityEngine.Object, SerializedObject> _cachedSerializedObjects = new();

        private void OnEnable()
        {
            _container = (VariableContainer)target;
            CacheVariableTypes();
            SetupReorderableList();
        }

        private void OnDisable()
        {
            _cachedSerializedObjects.Clear();
        }

        /// <summary>
        /// Discovers all non-abstract ScriptableVariable types via reflection.
        /// </summary>
        private void CacheVariableTypes()
        {
            _variableTypes = new List<Type>();
            _typeDisplayNames = new Dictionary<Type, string>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => typeof(ScriptableVariable).IsAssignableFrom(t)
                                    && !t.IsAbstract
                                    && t != typeof(ScriptableVariable)
                                    && !t.IsGenericType
                                    && t.GetConstructor(Type.EmptyTypes) != null);

                    foreach (var type in types)
                    {
                        _variableTypes.Add(type);

                        // Create display name (remove "Variable" suffix for cleaner display)
                        string displayName = type.Name;
                        if (displayName.EndsWith("Variable"))
                        {
                            displayName = displayName.Substring(0, displayName.Length - 8);
                        }
                        _typeDisplayNames[type] = displayName;
                    }
                }
                catch (Exception)
                {
                    // Skip assemblies that can't be loaded
                }
            }

            // Sort by display name
            _variableTypes = _variableTypes.OrderBy(t => _typeDisplayNames[t]).ToList();
        }

        private void SetupReorderableList()
        {
            var variablesProp = serializedObject.FindProperty("variables");

            _variableList = new ReorderableList(serializedObject, variablesProp, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeightCallback = GetElementHeight,
                onAddDropdownCallback = ShowAddDropdown,
                onRemoveCallback = RemoveVariable,
                onReorderCallback = OnReorder
            };
        }

        private void DrawHeader(Rect rect)
        {
            // Split header into columns
            float nameWidth = rect.width * 0.35f;
            float typeWidth = rect.width * 0.2f;
            float valueWidth = rect.width * 0.45f;

            var nameRect = new Rect(rect.x, rect.y, nameWidth, rect.height);
            var typeRect = new Rect(rect.x + nameWidth, rect.y, typeWidth, rect.height);
            var valueRect = new Rect(rect.x + nameWidth + typeWidth, rect.y, valueWidth, rect.height);

            EditorGUI.LabelField(nameRect, "Name", EditorStyles.boldLabel);
            EditorGUI.LabelField(typeRect, "Type", EditorStyles.boldLabel);
            EditorGUI.LabelField(valueRect, "Value", EditorStyles.boldLabel);
        }

        private float GetElementHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var elementProp = _variableList.serializedProperty.GetArrayElementAtIndex(index);
            var variable = elementProp.objectReferenceValue as ScriptableVariable;

            if (variable == null)
            {
                EditorGUI.LabelField(rect, "(Missing Reference)", EditorStyles.miniLabel);
                return;
            }

            rect.y += 2f;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            // Calculate column widths
            float nameWidth = rect.width * 0.35f - 4f;
            float typeWidth = rect.width * 0.2f - 4f;
            float valueWidth = rect.width * 0.45f - 4f;

            var nameRect = new Rect(rect.x, rect.y, nameWidth, lineHeight);
            var typeRect = new Rect(rect.x + nameWidth + 4f, rect.y, typeWidth, lineHeight);
            var valueRect = new Rect(rect.x + nameWidth + typeWidth + 8f, rect.y, valueWidth, lineHeight);

            // Name field (editable)
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.TextField(nameRect, variable.name);
            if (EditorGUI.EndChangeCheck() && newName != variable.name)
            {
                Undo.RecordObject(variable, "Rename Variable");
                variable.name = newName;
                EditorUtility.SetDirty(variable);

                // Re-import to update the asset database
                var path = AssetDatabase.GetAssetPath(_container);
                AssetDatabase.ImportAsset(path);
            }

            // Type label (read-only, styled)
            string typeName = _typeDisplayNames.TryGetValue(variable.GetType(), out var display)
                ? display
                : variable.GetType().Name;
            GUI.enabled = false;
            EditorGUI.TextField(typeRect, typeName);
            GUI.enabled = true;

            // Value field (inline editing)
            DrawValueField(valueRect, variable);
        }

        private void DrawValueField(Rect rect, ScriptableVariable variable)
        {
            // Get or create cached SerializedObject
            if (!_cachedSerializedObjects.TryGetValue(variable, out var varSO))
            {
                varSO = new SerializedObject(variable);
                _cachedSerializedObjects[variable] = varSO;
            }

            varSO.Update();

            var valueProp = varSO.FindProperty("value");
            if (valueProp != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, valueProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    varSO.ApplyModifiedProperties();
                    // Trigger the variable's event to update any listeners
                    variable.Raise();
                }
            }
            else
            {
                // Fallback: show as string
                EditorGUI.LabelField(rect, variable.ToString(), EditorStyles.miniLabel);
            }
        }

        private void ShowAddDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();

            // Group types by category
            var primitiveTypes = new[] { "Int", "Float", "Bool", "Text" };
            var vectorTypes = new[] { "Vector2", "Vector2Int", "Vector3", "Quaternion" };
            var graphicsTypes = new[] { "Color", "Gradient", "AnimationCurve" };

            foreach (var type in _variableTypes)
            {
                string displayName = _typeDisplayNames[type];

                // Determine category
                string category = "";
                if (primitiveTypes.Any(p => displayName.StartsWith(p)))
                    category = "Primitives/";
                else if (vectorTypes.Any(v => displayName.StartsWith(v)))
                    category = "Vectors/";
                else if (graphicsTypes.Any(g => displayName.StartsWith(g)))
                    category = "Graphics/";
                else
                    category = "Other/";

                menu.AddItem(new GUIContent(category + displayName), false, () => AddVariable(type));
            }

            menu.ShowAsContext();
        }

        private void AddVariable(Type type)
        {
            var path = AssetDatabase.GetAssetPath(_container);

            // Create the variable instance
            var variable = (ScriptableVariable)CreateInstance(type);

            // Generate a unique name
            string baseName = _typeDisplayNames.TryGetValue(type, out var display) ? display : type.Name;
            variable.name = GenerateUniqueName(baseName);

            // Record undo
            Undo.RecordObject(_container, "Add Variable");

            // Add to container
            _container.EditorAddVariable(variable);

            // Add as sub-asset
            AssetDatabase.AddObjectToAsset(variable, path);
            AssetDatabase.ImportAsset(path);

            serializedObject.ApplyModifiedProperties();

            // Select the new variable in the list
            _variableList.index = _container.Count - 1;
        }

        private string GenerateUniqueName(string baseName)
        {
            string name = baseName;
            int counter = 1;

            while (_container.Has(name))
            {
                name = $"{baseName}_{counter}";
                counter++;
            }

            return name;
        }

        private void RemoveVariable(ReorderableList list)
        {
            int index = list.index;
            if (index < 0 || index >= _container.Count) return;

            var variable = _container[index];

            // Record undo
            Undo.RecordObject(_container, "Remove Variable");

            // Remove from container
            _container.EditorRemoveVariableAt(index);

            // Remove the sub-asset
            if (variable != null)
            {
                // Clear from cache
                _cachedSerializedObjects.Remove(variable);

                AssetDatabase.RemoveObjectFromAsset(variable);
                DestroyImmediate(variable, true);
            }

            AssetDatabase.SaveAssets();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnReorder(ReorderableList list)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_container);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);

            // Draw info box
            EditorGUILayout.HelpBox(
                "Variables added here are stored as sub-assets within this container. " +
                "Use the + button to add new variables of any type.",
                MessageType.Info);

            EditorGUILayout.Space(5);

            // Draw the reorderable list
            _variableList.DoLayoutList();

            EditorGUILayout.Space(5);

            // Utility buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cleanup Nulls", GUILayout.Height(25)))
            {
                _container.EditorCleanupNulls();
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Raise All", GUILayout.Height(25)))
            {
                _container.RaiseAll();
            }

            if (GUILayout.Button("Reset All", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Reset All Variables",
                    "Are you sure you want to reset all variables to their default values?",
                    "Reset", "Cancel"))
                {
                    _container.ResetAll();
                }
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

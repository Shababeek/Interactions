using System;
using System.Collections.Generic;
using System.Linq;
using Shababeek.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Editor window for viewing and managing ScriptableVariables and GameEvents in the project.
    /// </summary>
    public class ScriptableSystemEditorWindow : EditorWindow
    {
        private enum Tab
        {
            Variables,
            Events
        }

        private enum FilterType
        {
            All,
            Int,
            Float,
            Bool,
            Text,
            Vector2,
            Vector3,
            Color,
            Other
        }

        private Tab _currentTab = Tab.Variables;
        private Vector2 _variablesScrollPos;
        private Vector2 _eventsScrollPos;
        private string _searchQuery = "";
        private FilterType _filterType = FilterType.All;
        private bool _showOnlyReferenced = true;

        private Dictionary<Object, List<ScriptableVariable>> _variablesByAsset = new();
        private Dictionary<Object, List<GameEvent>> _eventsByAsset = new();
        private Dictionary<Object, bool> _assetFoldouts = new();
        private HashSet<ScriptableVariable> _referencedVariables = new();
        private HashSet<GameEvent> _referencedEvents = new();

        private List<ScriptableVariable> _allVariables = new();
        private List<GameEvent> _allEvents = new();

        private bool _needsRefresh = true;
        private double _lastRefreshTime;
        private const double RefreshCooldown = 0.5;

        [MenuItem("Shababeek/Scriptable System Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<ScriptableSystemEditorWindow>("Scriptable System");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            _needsRefresh = true;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            _needsRefresh = true;
            Repaint();
        }

        private void OnHierarchyChanged()
        {
            _needsRefresh = true;
        }

        private void OnGUI()
        {
            if (_needsRefresh && EditorApplication.timeSinceStartup - _lastRefreshTime > RefreshCooldown)
            {
                RefreshData();
                _needsRefresh = false;
                _lastRefreshTime = EditorApplication.timeSinceStartup;
            }

            DrawToolbar();
            DrawTabContent();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Tab buttons
            if (GUILayout.Toggle(_currentTab == Tab.Variables, "Variables", EditorStyles.toolbarButton))
            {
                _currentTab = Tab.Variables;
            }
            if (GUILayout.Toggle(_currentTab == Tab.Events, "Events", EditorStyles.toolbarButton))
            {
                _currentTab = Tab.Events;
            }

            GUILayout.FlexibleSpace();

            // Search field
            EditorGUILayout.LabelField("Search:", GUILayout.Width(45));
            var newSearch = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(150));
            if (newSearch != _searchQuery)
            {
                _searchQuery = newSearch;
            }

            // Clear search button
            if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _searchQuery = "";
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            // Second row with filters
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Type filter (only for variables tab)
            if (_currentTab == Tab.Variables)
            {
                EditorGUILayout.LabelField("Type:", GUILayout.Width(35));
                _filterType = (FilterType)EditorGUILayout.EnumPopup(_filterType, EditorStyles.toolbarDropDown, GUILayout.Width(80));
            }

            GUILayout.FlexibleSpace();

            // Referenced toggle
            var newShowOnlyReferenced = GUILayout.Toggle(_showOnlyReferenced, "Scene Refs Only", EditorStyles.toolbarButton);
            if (newShowOnlyReferenced != _showOnlyReferenced)
            {
                _showOnlyReferenced = newShowOnlyReferenced;
            }

            // Refresh button
            if (GUILayout.Button("↻ Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                RefreshData();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabContent()
        {
            switch (_currentTab)
            {
                case Tab.Variables:
                    DrawVariablesTab();
                    break;
                case Tab.Events:
                    DrawEventsTab();
                    break;
            }
        }

        private void DrawVariablesTab()
        {
            var variablesToShow = GetFilteredVariables();
            var groupedVariables = GroupByAsset(variablesToShow);

            // Stats bar
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            var totalCount = _showOnlyReferenced ? _referencedVariables.Count : _allVariables.Count;
            var shownCount = variablesToShow.Count;
            EditorGUILayout.LabelField($"Showing {shownCount} of {totalCount} variables", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            _variablesScrollPos = EditorGUILayout.BeginScrollView(_variablesScrollPos);

            if (groupedVariables.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    _showOnlyReferenced
                        ? "No variables referenced in the current scene.\nTry unchecking 'Scene Refs Only' to see all variables."
                        : "No variables found matching the current filters.",
                    MessageType.Info);
            }
            else
            {
                foreach (var kvp in groupedVariables.OrderBy(x => GetAssetDisplayName(x.Key)))
                {
                    DrawAssetGroup(kvp.Key, kvp.Value);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawEventsTab()
        {
            var eventsToShow = GetFilteredEvents();
            var groupedEvents = GroupEventsByAsset(eventsToShow);

            // Stats bar
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            var totalCount = _showOnlyReferenced ? _referencedEvents.Count : _allEvents.Count;
            var shownCount = eventsToShow.Count;
            EditorGUILayout.LabelField($"Showing {shownCount} of {totalCount} events", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            _eventsScrollPos = EditorGUILayout.BeginScrollView(_eventsScrollPos);

            if (groupedEvents.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    _showOnlyReferenced
                        ? "No events referenced in the current scene.\nTry unchecking 'Scene Refs Only' to see all events."
                        : "No events found matching the current filters.",
                    MessageType.Info);
            }
            else
            {
                foreach (var kvp in groupedEvents.OrderBy(x => GetAssetDisplayName(x.Key)))
                {
                    DrawEventGroup(kvp.Key, kvp.Value);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAssetGroup(Object asset, List<ScriptableVariable> variables)
        {
            string assetName = GetAssetDisplayName(asset);
            string assetKey = asset != null ? AssetDatabase.GetAssetPath(asset) : "Standalone";

            if (!_assetFoldouts.ContainsKey(asset ?? (Object)this))
            {
                _assetFoldouts[asset ?? (Object)this] = true;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Header with foldout
            EditorGUILayout.BeginHorizontal();

            var foldoutKey = asset ?? (Object)this;
            _assetFoldouts[foldoutKey] = EditorGUILayout.Foldout(_assetFoldouts[foldoutKey], $"{assetName} ({variables.Count})", true, EditorStyles.foldoutHeader);

            // Select asset button
            if (asset != null)
            {
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            EditorGUILayout.EndHorizontal();

            // Draw variables if expanded
            if (_assetFoldouts[foldoutKey])
            {
                EditorGUI.indentLevel++;
                foreach (var variable in variables.OrderBy(v => v.name))
                {
                    DrawVariableRow(variable);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEventGroup(Object asset, List<GameEvent> events)
        {
            string assetName = GetAssetDisplayName(asset);

            if (!_assetFoldouts.ContainsKey(asset ?? (Object)this))
            {
                _assetFoldouts[asset ?? (Object)this] = true;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Header with foldout
            EditorGUILayout.BeginHorizontal();

            var foldoutKey = asset ?? (Object)this;
            _assetFoldouts[foldoutKey] = EditorGUILayout.Foldout(_assetFoldouts[foldoutKey], $"{assetName} ({events.Count})", true, EditorStyles.foldoutHeader);

            // Select asset button
            if (asset != null)
            {
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            EditorGUILayout.EndHorizontal();

            // Draw events if expanded
            if (_assetFoldouts[foldoutKey])
            {
                EditorGUI.indentLevel++;
                foreach (var evt in events.OrderBy(e => e.name))
                {
                    DrawEventRow(evt);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawVariableRow(ScriptableVariable variable)
        {
            EditorGUILayout.BeginHorizontal();

            // Select button
            if (GUILayout.Button("→", GUILayout.Width(25)))
            {
                Selection.activeObject = variable;
                EditorGUIUtility.PingObject(variable);
            }

            // Variable name with type
            string typeName = GetVariableTypeName(variable);
            EditorGUILayout.LabelField($"{variable.name}", GUILayout.MinWidth(100));
            EditorGUILayout.LabelField($"[{typeName}]", EditorStyles.miniLabel, GUILayout.Width(80));

            // Current value (runtime only)
            GUI.enabled = Application.isPlaying;
            DrawVariableValue(variable);
            GUI.enabled = true;

            // Reference indicator
            if (_referencedVariables.Contains(variable))
            {
                EditorGUILayout.LabelField("●", EditorStyles.miniLabel, GUILayout.Width(15));
            }
            else
            {
                EditorGUILayout.LabelField(" ", GUILayout.Width(15));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEventRow(GameEvent evt)
        {
            // Skip if this is actually a ScriptableVariable (they inherit from GameEvent)
            if (evt is ScriptableVariable)
                return;

            EditorGUILayout.BeginHorizontal();

            // Select button
            if (GUILayout.Button("→", GUILayout.Width(25)))
            {
                Selection.activeObject = evt;
                EditorGUIUtility.PingObject(evt);
            }

            // Event name
            EditorGUILayout.LabelField($"{evt.name}", GUILayout.MinWidth(150));

            // Fire button (runtime only)
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Fire", GUILayout.Width(50)))
            {
                evt.Raise();
            }
            GUI.enabled = true;

            // Reference indicator
            if (_referencedEvents.Contains(evt))
            {
                EditorGUILayout.LabelField("●", EditorStyles.miniLabel, GUILayout.Width(15));
            }
            else
            {
                EditorGUILayout.LabelField(" ", GUILayout.Width(15));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawVariableValue(ScriptableVariable variable)
        {
            const float valueWidth = 100;

            switch (variable)
            {
                case ScriptableVariable<int> intVar:
                    var newIntVal = EditorGUILayout.IntField(intVar.Value, GUILayout.Width(valueWidth));
                    if (newIntVal != intVar.Value && Application.isPlaying)
                        intVar.Value = newIntVal;
                    break;

                case ScriptableVariable<float> floatVar:
                    var newFloatVal = EditorGUILayout.FloatField(floatVar.Value, GUILayout.Width(valueWidth));
                    if (!Mathf.Approximately(newFloatVal, floatVar.Value) && Application.isPlaying)
                        floatVar.Value = newFloatVal;
                    break;

                case ScriptableVariable<bool> boolVar:
                    var newBoolVal = EditorGUILayout.Toggle(boolVar.Value, GUILayout.Width(valueWidth));
                    if (newBoolVal != boolVar.Value && Application.isPlaying)
                        boolVar.Value = newBoolVal;
                    break;

                case ScriptableVariable<string> stringVar:
                    var newStringVal = EditorGUILayout.TextField(stringVar.Value ?? "", GUILayout.Width(valueWidth));
                    if (newStringVal != stringVar.Value && Application.isPlaying)
                        stringVar.Value = newStringVal;
                    break;

                case ScriptableVariable<Vector2> vec2Var:
                    EditorGUILayout.LabelField($"({vec2Var.Value.x:F2}, {vec2Var.Value.y:F2})", GUILayout.Width(valueWidth));
                    break;

                case ScriptableVariable<Vector3> vec3Var:
                    EditorGUILayout.LabelField($"({vec3Var.Value.x:F2}, {vec3Var.Value.y:F2}, {vec3Var.Value.z:F2})", GUILayout.Width(valueWidth));
                    break;

                case ScriptableVariable<Color> colorVar:
                    EditorGUILayout.ColorField(GUIContent.none, colorVar.Value, false, true, false, GUILayout.Width(valueWidth));
                    break;

                default:
                    EditorGUILayout.LabelField(variable.ToString(), GUILayout.Width(valueWidth));
                    break;
            }
        }

        private void RefreshData()
        {
            _allVariables.Clear();
            _allEvents.Clear();
            _referencedVariables.Clear();
            _referencedEvents.Clear();
            _variablesByAsset.Clear();
            _eventsByAsset.Clear();

            // Find all ScriptableVariables in project
            var variableGuids = AssetDatabase.FindAssets("t:ScriptableVariable");
            foreach (var guid in variableGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var asset in assets)
                {
                    if (asset is ScriptableVariable variable && variable != null)
                    {
                        _allVariables.Add(variable);
                    }
                }
            }

            // Find all GameEvents in project (excluding ScriptableVariables which inherit from GameEvent)
            var eventGuids = AssetDatabase.FindAssets("t:GameEvent");
            foreach (var guid in eventGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var asset in assets)
                {
                    if (asset is GameEvent evt && evt != null && !(evt is ScriptableVariable))
                    {
                        _allEvents.Add(evt);
                    }
                }
            }

            // Find scene references
            FindSceneReferences();

            Repaint();
        }

        private void FindSceneReferences()
        {
            // Get all MonoBehaviours in loaded scenes
            var allComponents = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(c => c != null && c.gameObject.scene.isLoaded);

            foreach (var component in allComponents)
            {
                var so = new SerializedObject(component);
                var prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null)
                    {
                        if (prop.objectReferenceValue is ScriptableVariable variable)
                        {
                            _referencedVariables.Add(variable);
                        }
                        else if (prop.objectReferenceValue is GameEvent evt && !(evt is ScriptableVariable))
                        {
                            _referencedEvents.Add(evt);
                        }
                    }
                }
            }
        }

        private List<ScriptableVariable> GetFilteredVariables()
        {
            var source = _showOnlyReferenced ? _referencedVariables.ToList() : _allVariables;

            return source
                .Where(v => v != null)
                .Where(v => MatchesSearch(v.name))
                .Where(MatchesTypeFilter)
                .ToList();
        }

        private List<GameEvent> GetFilteredEvents()
        {
            var source = _showOnlyReferenced ? _referencedEvents.ToList() : _allEvents;

            return source
                .Where(e => e != null && !(e is ScriptableVariable))
                .Where(e => MatchesSearch(e.name))
                .ToList();
        }

        private bool MatchesSearch(string name)
        {
            if (string.IsNullOrEmpty(_searchQuery))
                return true;

            return name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool MatchesTypeFilter(ScriptableVariable variable)
        {
            if (_filterType == FilterType.All)
                return true;

            return _filterType switch
            {
                FilterType.Int => variable is ScriptableVariable<int>,
                FilterType.Float => variable is ScriptableVariable<float>,
                FilterType.Bool => variable is ScriptableVariable<bool>,
                FilterType.Text => variable is ScriptableVariable<string>,
                FilterType.Vector2 => variable is ScriptableVariable<Vector2> || variable is ScriptableVariable<Vector2Int>,
                FilterType.Vector3 => variable is ScriptableVariable<Vector3>,
                FilterType.Color => variable is ScriptableVariable<Color>,
                FilterType.Other => !(variable is ScriptableVariable<int> ||
                                     variable is ScriptableVariable<float> ||
                                     variable is ScriptableVariable<bool> ||
                                     variable is ScriptableVariable<string> ||
                                     variable is ScriptableVariable<Vector2> ||
                                     variable is ScriptableVariable<Vector2Int> ||
                                     variable is ScriptableVariable<Vector3> ||
                                     variable is ScriptableVariable<Color>),
                _ => true
            };
        }

        private Dictionary<Object, List<ScriptableVariable>> GroupByAsset(List<ScriptableVariable> variables)
        {
            var result = new Dictionary<Object, List<ScriptableVariable>>();

            foreach (var variable in variables)
            {
                var path = AssetDatabase.GetAssetPath(variable);
                var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);

                // Check if the variable is a sub-asset of a VariableContainer
                Object groupKey;
                if (mainAsset is VariableContainer)
                {
                    groupKey = mainAsset;
                }
                else if (mainAsset == variable)
                {
                    groupKey = null; // Will use "Standalone" as name
                }
                else
                {
                    groupKey = mainAsset;
                }

                if (groupKey != null && !result.ContainsKey(groupKey))
                {
                    result[groupKey] = new List<ScriptableVariable>();
                }

                if (groupKey != null) result[groupKey].Add(variable);
            }

            return result;
        }

        private Dictionary<Object, List<GameEvent>> GroupEventsByAsset(List<GameEvent> events)
        {
            var result = new Dictionary<Object, List<GameEvent>>();

            foreach (var evt in events)
            {
                var path = AssetDatabase.GetAssetPath(evt);
                var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);

                Object groupKey;
                if (mainAsset is VariableContainer)
                {
                    groupKey = mainAsset;
                }
                else if (mainAsset == evt)
                {
                    groupKey = null;
                }
                else
                {
                    groupKey = mainAsset;
                }

                if (groupKey != null && !result.ContainsKey(groupKey))
                {
                    result[groupKey] = new List<GameEvent>();
                }

                if (groupKey != null) result[groupKey].Add(evt);
            }

            return result;
        }

        private string GetAssetDisplayName(Object asset)
        {
            if (asset == null)
                return "Standalone Assets";

            if (asset is VariableContainer container)
                return $"📦 {container.name}";

            return asset.name;
        }

        private string GetVariableTypeName(ScriptableVariable variable)
        {
            var typeName = variable.GetType().Name;

            // Simplify common type names
            if (typeName.EndsWith("Variable"))
                typeName = typeName.Substring(0, typeName.Length - 8);

            return typeName;
        }
    }
}

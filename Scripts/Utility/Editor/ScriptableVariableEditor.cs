using System.Collections.Generic;
using System.Linq;
using Shababeek.Utilities;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for ScriptableVariable that shows scene references.
    /// </summary>
    [CustomEditor(typeof(ScriptableVariable), true)]
    public class ScriptableVariableEditor : Editor
    {
        private List<Component> _sceneReferences = new();
        private bool _showReferences = true;
        private Vector2 _scrollPos;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            DrawSceneReferences();
        }

        private void DrawSceneReferences()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            _showReferences = EditorGUILayout.Foldout(_showReferences, $"Scene References ({_sceneReferences.Count})", true);

            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                FindSceneReferences();
            }
            EditorGUILayout.EndHorizontal();

            if (_showReferences)
            {
                if (_sceneReferences.Count == 0)
                {
                    EditorGUILayout.LabelField("No references found in open scenes.", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("Click Refresh to scan.", EditorStyles.miniLabel);
                }
                else
                {
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(200));

                    foreach (var component in _sceneReferences)
                    {
                        if (component == null) continue;

                        EditorGUILayout.BeginHorizontal();

                        // Ping button
                        if (GUILayout.Button("→", GUILayout.Width(25)))
                        {
                            Selection.activeObject = component.gameObject;
                            EditorGUIUtility.PingObject(component.gameObject);
                        }

                        // Component info
                        string path = GetGameObjectPath(component.gameObject);
                        EditorGUILayout.LabelField($"{path}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"[{component.GetType().Name}]", EditorStyles.miniBoldLabel, GUILayout.Width(150));

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void FindSceneReferences()
        {
            _sceneReferences.Clear();
            var targetObject = target;

            // Find all MonoBehaviours in all loaded scenes
            var allComponents = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(c => c != null && c.gameObject.scene.isLoaded);

            foreach (var component in allComponents)
            {
                if (ReferencesObject(component, targetObject))
                {
                    _sceneReferences.Add(component);
                }
            }

            // Sort by path
            _sceneReferences = _sceneReferences
                .OrderBy(c => GetGameObjectPath(c.gameObject))
                .ToList();
        }

        private bool ReferencesObject(Component component, Object target)
        {
            var so = new SerializedObject(component);
            var prop = so.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (prop.objectReferenceValue == target)
                        return true;
                }
            }
            return false;
        }

        private string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private void OnEnable()
        {
            FindSceneReferences();
        }
    }
}

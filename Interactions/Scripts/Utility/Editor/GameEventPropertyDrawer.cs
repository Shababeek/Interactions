using UnityEditor;
using UnityEngine;
using Shababeek.Core;

namespace Shababeek.Core.Editors
{
    [CustomPropertyDrawer(typeof(GameEvent))]
    public class GameEventPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Check if the GameEvent is null
            if (property.objectReferenceValue == null)
            {
                // Calculate rects for split layout (property field + button)
                var labelRect = new Rect(position.x, position.y, position.width * 0.6f, EditorGUIUtility.singleLineHeight);
                var buttonRect = new Rect(position.x + position.width * 0.6f + 5, position.y, position.width * 0.4f - 5, EditorGUIUtility.singleLineHeight);
                
                // Draw the property field
                EditorGUI.PropertyField(labelRect, property, label);
                
                // Draw "Create Event" button
                if (GUI.Button(buttonRect, "Create Event"))
                {
                    CreateGameEvent(property);
                }
            }
            else
            {
                // Draw the property field at full width when GameEvent is assigned
                EditorGUI.PropertyField(position, property, label);
            }
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        private void CreateGameEvent(SerializedProperty property)
        {
            // Create a new GameEvent asset
            var gameEvent = ScriptableObject.CreateInstance<GameEvent>();
            
            // Generate a unique name
            string assetName = "New Game Event";
            string defaultPath = "Assets/";
            
            // Try to find a good default path - prioritize selected folder, then current object's folder
            string selectedFolder = GetSelectedFolderPath();
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                defaultPath = selectedFolder;
            }
            else if (property.serializedObject.targetObject != null)
            {
                string currentPath = AssetDatabase.GetAssetPath(property.serializedObject.targetObject);
                if (!string.IsNullOrEmpty(currentPath))
                {
                    defaultPath = System.IO.Path.GetDirectoryName(currentPath) + "/";
                }
            }
            
            // Open save dialog to let user choose location
            string fullPath = EditorUtility.SaveFilePanel(
                "Create Game Event",
                defaultPath,
                assetName,
                "asset"
            );
            
            // If user cancelled, destroy the created instance and return
            if (string.IsNullOrEmpty(fullPath))
            {
                Object.DestroyImmediate(gameEvent);
                return;
            }
            
            // Convert absolute path to project-relative path
            string projectPath = fullPath.Replace(Application.dataPath, "Assets");
            if (!projectPath.StartsWith("Assets/"))
            {
                projectPath = "Assets/" + projectPath;
            }
            
            // Ensure unique path
            projectPath = AssetDatabase.GenerateUniqueAssetPath(projectPath);
            
            // Create the asset
            AssetDatabase.CreateAsset(gameEvent, projectPath);
            AssetDatabase.SaveAssets();
            
            // Assign the created asset to the property
            property.objectReferenceValue = gameEvent;
            property.serializedObject.ApplyModifiedProperties();
            
            // Ping the created asset in the Project window
            EditorGUIUtility.PingObject(gameEvent);
            
            Debug.Log($"Created new GameEvent: {projectPath}");
        }
        
        private string GetSelectedFolderPath()
        {
            // Get the currently selected object in the Project window
            Object[] selection = Selection.GetFiltered<Object>(SelectionMode.Assets);
            
            if (selection.Length > 0)
            {
                string path = AssetDatabase.GetAssetPath(selection[0]);
                
                // If it's a folder, return it
                if (AssetDatabase.IsValidFolder(path))
                {
                    return path + "/";
                }
                
                // If it's a file, return its directory
                if (!string.IsNullOrEmpty(path))
                {
                    return System.IO.Path.GetDirectoryName(path) + "/";
                }
            }
            
            return null;
        }
    }
} 
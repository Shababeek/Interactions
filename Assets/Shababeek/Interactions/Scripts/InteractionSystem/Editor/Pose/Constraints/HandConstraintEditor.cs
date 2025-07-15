using Shababeek.Interactions.Animations;
using Shababeek.Interactions.Core;
using UnityEngine;
using UnityEditor;
using Shababeek.InteractionSystem.Animations;

namespace Shababeek.InteractionSystem.Core.Editors
{
    [CustomPropertyDrawer(typeof(PoseConstrains))]
    public class PoseConstraintPropertyDrawer : PropertyDrawer
    {
        private bool hasPose = false;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel++;
            position.y += EditorGUIUtility.singleLineHeight;
            
            // Add pose selection dropdown
            position = DrawPoseSelectionDropdown(property, position);
            
            position = DrawPropertyAndIncrementHeight(property,  position, "indexFingerLimits");
            position = DrawPropertyAndIncrementHeight(property,  position, "middleFingerLimits");
            position = DrawPropertyAndIncrementHeight(property,  position, "ringFingerLimits");
            position = DrawPropertyAndIncrementHeight(property,  position, "pinkyFingerLimits");
            position = DrawPropertyAndIncrementHeight(property,  position, "thumbFingerLimits");
            EditorGUI.EndProperty();
            EditorGUI.indentLevel--;
        }

        private  Rect DrawPoseSelectionDropdown(SerializedProperty property, Rect position)
        {
            var targetPoseIndexProperty = property.FindPropertyRelative("targetPoseIndex");
            var currentPoseIndex = targetPoseIndexProperty.intValue;
            
            // Get available poses from HandData
            var poseNames = GetPoseNames();
            if(poseNames.Length <=1){
                return position;
            }
            hasPose = true;
            var currentPoseName = currentPoseIndex >= 0 && currentPoseIndex < poseNames.Length 
                ? poseNames[currentPoseIndex] 
                : "Default";
            
            // Create dropdown
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "Target Pose", currentPoseName);
            position.y += EditorGUIUtility.singleLineHeight;
            
            // Add a button to open pose selection
            var buttonRect = position;
            buttonRect.height = EditorGUIUtility.singleLineHeight;
            if (GUI.Button(buttonRect, "Select Pose"))
            {
                ShowPoseSelectionMenu(property, poseNames, currentPoseIndex);
            }
            position.y += EditorGUIUtility.singleLineHeight;
            
            return position;
        }
        
        private static void ShowPoseSelectionMenu(SerializedProperty property, string[] poseNames, int currentIndex)
        {
            var menu = new GenericMenu();
            
            for (int i = 0; i < poseNames.Length; i++)
            {
                var poseIndex = i;
                var poseName = poseNames[i];
                var isSelected = poseIndex == currentIndex;
                
                menu.AddItem(new GUIContent(poseName), isSelected, () => {
                    var targetPoseIndexProperty = property.FindPropertyRelative("targetPoseIndex");
                    targetPoseIndexProperty.intValue = poseIndex;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            
            menu.ShowAsContext();
        }
        
        private static string[] GetPoseNames()
        {
            // Get HandData from Config system
            var handData = GetHandDataFromConfig();
            if (handData != null && handData.Poses != null)
            {
                var poseNames = new string[handData.Poses.Length];
                for (int i = 0; i < handData.Poses.Length; i++)
                {
                    poseNames[i] = handData.Poses[i].Name ?? $"Pose {i}";
                }
                return poseNames;
            }
            
            // Fallback names if HandData not found
            return new string[] { "Default", "Pose 1", "Pose 2", "Pose 3" };
        }
        
        private static HandData GetHandDataFromConfig()
        {
            // Try to find Config in the scene first
            var cameraRig = GameObject.FindObjectOfType<CameraRig>();
            if (cameraRig != null && cameraRig.Config != null)
            {
                return cameraRig.Config.HandData;
            }
            
            // Try to find Config in the project
            var configAssets = AssetDatabase.FindAssets("t:Config");
            if (configAssets.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(configAssets[0]);
                var config = AssetDatabase.LoadAssetAtPath<Config>(path);
                if (config != null)
                {
                    return config.HandData;
                }
            }
            
            // Try to find HandData directly in the project as last resort
            var handDataAssets = AssetDatabase.FindAssets("t:HandData");
            if (handDataAssets.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(handDataAssets[0]);
                return AssetDatabase.LoadAssetAtPath<HandData>(path);
            }
            
            return null;
        }

        private static Rect DrawPropertyAndIncrementHeight(SerializedProperty property, Rect position, string proptertyName)
        {
            var height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative(proptertyName));
            position.height = height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(proptertyName));
            position.y += height;
            return position;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("thumbFingerLimits")) * 5;
            height += EditorGUIUtility.singleLineHeight*3;
            return height;
        }

    }
}
using UnityEditor;
using UnityEngine;
using Shababeek.Interactions.Core;
using System.Collections.Generic;
using System.Linq;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Config))]
    public class ConfigEditor : Editor
    {
        private Config config;
        
        // Serialized Properties
        private SerializedProperty handDataProp;
        private SerializedProperty leftHandLayerProp;
        private SerializedProperty rightHandLayerProp;
        private SerializedProperty interactableLayerProp;
        private SerializedProperty playerLayerProp;
        private SerializedProperty inputTypeProp;
        private SerializedProperty leftHandActionsProp;
        private SerializedProperty rightHandActionsProp;
        private SerializedProperty oldInputSettingsProp;
        private SerializedProperty customLayerAssignmentsProp;
        private SerializedProperty feedbackSystemStyleSheetProp;
        private SerializedProperty handMassProp;
        private SerializedProperty linearDampingProp;
        private SerializedProperty angularDampingProp;

        private void OnEnable()
        {
            config = (Config)target;
            
            // Find all serialized properties
            handDataProp = serializedObject.FindProperty("handData");
            leftHandLayerProp = serializedObject.FindProperty("leftHandLayer");
            rightHandLayerProp = serializedObject.FindProperty("rightHandLayer");
            interactableLayerProp = serializedObject.FindProperty("interactableLayer");
            playerLayerProp = serializedObject.FindProperty("playerLayer");
            inputTypeProp = serializedObject.FindProperty("inputType");
            leftHandActionsProp = serializedObject.FindProperty("leftHandActions");
            rightHandActionsProp = serializedObject.FindProperty("rightHandActions");
            oldInputSettingsProp = serializedObject.FindProperty("oldInputSettings");
            customLayerAssignmentsProp = serializedObject.FindProperty("customLayerAssignments");
            feedbackSystemStyleSheetProp = serializedObject.FindProperty("feedbackSystemStyleSheet");
            handMassProp = serializedObject.FindProperty("handMass");
            linearDampingProp = serializedObject.FindProperty("linearDamping");
            angularDampingProp = serializedObject.FindProperty("angularDamping");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Interaction System Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Validation Section
            DrawValidationSection();
            
            EditorGUILayout.Space();
            
            // Hand Data Section
            EditorGUILayout.LabelField("Hand Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(handDataProp, new GUIContent("Hand Data", "Configuration for hand prefabs and settings"));
            
            EditorGUILayout.Space();
            
            // Layer Configuration Section
            EditorGUILayout.LabelField("Layer Configuration", EditorStyles.boldLabel);
            
            // Use Utilities for layer dropdowns
            int newLeftHandLayer = EditorUtilities.LayerDropdown(
                new GUIContent("Left Hand Layer", "Layer for the left hand, used for physics interactions"), 
                leftHandLayerProp.intValue);
            if (newLeftHandLayer != leftHandLayerProp.intValue)
            {
                leftHandLayerProp.intValue = newLeftHandLayer;
            }
            
            int newRightHandLayer = EditorUtilities.LayerDropdown(
                new GUIContent("Right Hand Layer", "Layer for the right hand, used for physics interactions"), 
                rightHandLayerProp.intValue);
            if (newRightHandLayer != rightHandLayerProp.intValue)
            {
                rightHandLayerProp.intValue = newRightHandLayer;
            }
            
            int newInteractableLayer = EditorUtilities.LayerDropdown(
                new GUIContent("Interactable Layer", "Layer for interactable objects, used for physics interactions"), 
                interactableLayerProp.intValue);
            if (newInteractableLayer != interactableLayerProp.intValue)
            {
                interactableLayerProp.intValue = newInteractableLayer;
            }
            
            int newPlayerLayer = EditorUtilities.LayerDropdown(
                new GUIContent("Player Layer", "Layer for the player"), 
                playerLayerProp.intValue);
            if (newPlayerLayer != playerLayerProp.intValue)
            {
                playerLayerProp.intValue = newPlayerLayer;
            }
            
            EditorGUILayout.Space();
            
     
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Create Layers", GUILayout.Height(25)))
            {
                CreateLayers();
            }
            EditorGUILayout.HelpBox("Click 'Create Layers' to automatically create the required layers in Unity's Layer Manager.", MessageType.Info);
            
            EditorGUILayout.Space();
            
            // Input Manager Section
            EditorGUILayout.LabelField("Input Manager Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(inputTypeProp, new GUIContent("Input Type", "Type of input manager to use for the interaction system"));
            
            if (inputTypeProp.enumValueIndex == 1) // InputSystem (enum value = 1)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(leftHandActionsProp, new GUIContent("Left Hand Actions", "Input actions for the left hand"));
                EditorGUILayout.PropertyField(rightHandActionsProp, new GUIContent("Right Hand Actions", "Input actions for the right hand"));
                EditorGUI.indentLevel--;
            }
            else if (inputTypeProp.enumValueIndex == 0) // InputManager (enum value = 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(oldInputSettingsProp, new GUIContent("Input Settings", "Axis names and button IDs for the old input manager"));
                
                EditorGUILayout.Space();
                if (GUILayout.Button("Create Input Axes", GUILayout.Height(25)))
                {
                    CreateInputAxes();
                }
                EditorGUILayout.HelpBox("Click 'Create Input Axes' to automatically add the required input axes to Unity's Input Manager.", MessageType.Info);
                EditorGUI.indentLevel--;
            }
            else if (inputTypeProp.enumValueIndex == 2) // KeyboardMock (enum value = -1, but displayed as 2 in inspector)
            {
                EditorGUILayout.HelpBox("Keyboard Mock input system selected. No additional configuration needed.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Editor UI Settings Section
            EditorGUILayout.LabelField("Editor UI Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(feedbackSystemStyleSheetProp, new GUIContent("Feedback System Style Sheet", "Style sheet for the feedback system UI"));
            
            EditorGUILayout.Space();
            
            // Hand Physics Section
            EditorGUILayout.LabelField("Hand Physics Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(handMassProp, new GUIContent("Hand Mass", "Mass of the hand for physics calculations"));
            EditorGUILayout.PropertyField(linearDampingProp, new GUIContent("Linear Damping", "Linear damping for hand physics"));
            EditorGUILayout.PropertyField(angularDampingProp, new GUIContent("Angular Damping", "Angular damping for hand physics"));
            
            EditorGUILayout.Space();
            
            // Debug Section
            DrawDebugSection();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawValidationSection()
        {
            bool hasWarnings = false;
            string warningMessage = "";
            
            if (handDataProp.objectReferenceValue == null)
            {
                hasWarnings = true;
                warningMessage += "• Hand Data is not assigned\n";
            }
            
            if (leftHandLayerProp.intValue == rightHandLayerProp.intValue && leftHandLayerProp.intValue != 0)
            {
                hasWarnings = true;
                warningMessage += "• Left and Right Hand layers should be different\n";
            }
            
            if (interactableLayerProp.intValue == leftHandLayerProp.intValue || 
                interactableLayerProp.intValue == rightHandLayerProp.intValue)
            {
                hasWarnings = true;
                warningMessage += "• Interactable layer should be different from hand layers\n";
            }
            
            if (hasWarnings)
            {
                EditorGUILayout.HelpBox("Configuration Issues:\n" + warningMessage, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("Configuration is properly set up!", MessageType.Info);
            }
        }
        
        private void DrawDebugSection()
        {
            EditorGUILayout.LabelField("Current Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Layer Information:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Left Hand: {EditorUtilities.GetLayerName(leftHandLayerProp.intValue)}");
            EditorGUILayout.LabelField($"Right Hand: {EditorUtilities.GetLayerName(rightHandLayerProp.intValue)}");
            EditorGUILayout.LabelField($"Interactable: {EditorUtilities.GetLayerName(interactableLayerProp.intValue)}");
            EditorGUILayout.LabelField($"Player: {EditorUtilities.GetLayerName(playerLayerProp.intValue)}");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Physics Settings:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Hand Mass: {handMassProp.floatValue}");
            EditorGUILayout.LabelField($"Linear Damping: {linearDampingProp.floatValue}");
            EditorGUILayout.LabelField($"Angular Damping: {angularDampingProp.floatValue}");
            EditorGUILayout.EndVertical();
        }
        
        private void CreateInputAxes()
        {
            var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            var serializedObject = new SerializedObject(inputManagerAsset);
            var axis = serializedObject.FindProperty("m_Axes");
            
            if (axis is not { isArray: true })
            {
                Debug.LogError("Failed to access Input Manager axes");
                return;
            }
            
            // Get current settings from config
            var settings = config.OldInputSettings;
            var defaultSettings = OldInputManagerSettings.Default;
            
            // Helper function to get name with fallback to default
            string GetName(string customName, string defaultName) => 
                string.IsNullOrEmpty(customName) ? defaultName : customName;
            
            // Update settings with defaults if empty
            bool settingsUpdated = false;
            var updatedSettings = new OldInputManagerSettings
            {
                leftTriggerAxis = GetName(settings.leftTriggerAxis, defaultSettings.leftTriggerAxis),
                leftGripAxis = GetName(settings.leftGripAxis, defaultSettings.leftGripAxis),
                leftPrimaryButton = GetName(settings.leftPrimaryButton, defaultSettings.leftPrimaryButton),
                leftSecondaryButton = GetName(settings.leftSecondaryButton, defaultSettings.leftSecondaryButton),
                leftGripDebugKey = GetName(settings.leftGripDebugKey, defaultSettings.leftGripDebugKey),
                leftTriggerDebugKey = GetName(settings.leftTriggerDebugKey, defaultSettings.leftTriggerDebugKey),
                leftThumbDebugKey = GetName(settings.leftThumbDebugKey, defaultSettings.leftThumbDebugKey),
                
                rightTriggerAxis = GetName(settings.rightTriggerAxis, defaultSettings.rightTriggerAxis),
                rightGripAxis = GetName(settings.rightGripAxis, defaultSettings.rightGripAxis),
                rightPrimaryButton = GetName(settings.rightPrimaryButton, defaultSettings.rightPrimaryButton),
                rightSecondaryButton = GetName(settings.rightSecondaryButton, defaultSettings.rightSecondaryButton),
                rightGripDebugKey = GetName(settings.rightGripDebugKey, defaultSettings.rightGripDebugKey),
                rightTriggerDebugKey = GetName(settings.rightTriggerDebugKey, defaultSettings.rightTriggerDebugKey),
                rightThumbDebugKey = GetName(settings.rightThumbDebugKey, defaultSettings.rightThumbDebugKey)
            };
            ((Config)target).OldInputSettings = updatedSettings;
            
            // Define the axes to create using updated settings
            var axesToCreate = new List<(string name, string descriptiveName, float dead, int axis, int type, string positiveButton, float gravity, float sensitivity, string altPositiveButton)>
            {
                // Left Hand
                (updatedSettings.leftTriggerAxis, "Left Hand Trigger Axis", 0f, 9, 2, "", 0f, 1f, ""),
                (updatedSettings.leftGripAxis, "Left Hand Grip Axis", 0f, 11, 2, "", 0f, 1f, ""),
                (updatedSettings.leftPrimaryButton, "Left Hand Primary Button", 0f, 0, 0, "joystick button 2", 1000f, 1000f, ""),
                (updatedSettings.leftSecondaryButton, "Left Hand Secondary Button", 0f, 0, 0, "joystick button 3", 1000f, 1000f, ""),
                (updatedSettings.leftGripDebugKey, "Left Hand Grip Debug Key", 0f, 0, 0, "z", 1000f, 1000f, ""),
                (updatedSettings.leftTriggerDebugKey, "Left Hand Trigger Debug Key", 0f, 0, 0, "x", 1000f, 1000f, ""),
                (updatedSettings.leftThumbDebugKey, "Left Hand Thumb Debug Key", 0f, 0, 0, "c", 1000f, 1000f, ""),
                
                // Right Hand
                (updatedSettings.rightTriggerAxis, "Right Hand Trigger Axis", 0f, 10, 2, "", 0f, 1f, ""),
                (updatedSettings.rightGripAxis, "Right Hand Grip Axis", 0f, 12, 2, "", 0f, 1f, ""),
                (updatedSettings.rightPrimaryButton, "Right Hand Primary Button", 0f, 0, 0, "joystick button 0", 1000f, 1000f, ""),
                (updatedSettings.rightSecondaryButton, "Right Hand Secondary Button", 0f, 0, 0, "joystick button 1", 1000f, 1000f, ""),
                (updatedSettings.rightGripDebugKey, "Right Hand Grip Debug Key", 0f, 0, 0, "m", 1000f, 1000f, ""),
                (updatedSettings.rightTriggerDebugKey, "Right Hand Trigger Debug Key", 0f, 0, 0, "n", 1000f, 1000f, ""),
                (updatedSettings.rightThumbDebugKey, "Right Hand Thumb Debug Key", 0f, 0, 0, "b", 1000f, 1000f, "")
            };
            
            // Remove existing axes with the same names
            for (int i = axis.arraySize - 1; i >= 0; i--)
            {
                var axisElement = axis.GetArrayElementAtIndex(i);
                var axisName = axisElement.FindPropertyRelative("m_Name").stringValue;
                
                if (axesToCreate.Any(axe => axe.name == axisName))
                {
                    axis.DeleteArrayElementAtIndex(i);
                }
            }
            
            // Add new axes
            int startIndex = axis.arraySize;
            axis.arraySize = startIndex + axesToCreate.Count;
            
            for (int i = 0; i < axesToCreate.Count; i++)
            {
                var axe = axesToCreate[i];
                var property = axis.GetArrayElementAtIndex(startIndex + i);
                SetAxis(property, axe);
            }
            
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            
            // Update the config file with the new settings if any were updated
            if (settingsUpdated)
            {
                var configSerializedObject = new SerializedObject(config);
                var oldInputSettingsProp = configSerializedObject.FindProperty("oldInputSettings");
                
                oldInputSettingsProp.FindPropertyRelative("leftTriggerAxis").stringValue = updatedSettings.leftTriggerAxis;
                oldInputSettingsProp.FindPropertyRelative("leftGripAxis").stringValue = updatedSettings.leftGripAxis;
                oldInputSettingsProp.FindPropertyRelative("leftPrimaryButton").stringValue = updatedSettings.leftPrimaryButton;
                oldInputSettingsProp.FindPropertyRelative("leftSecondaryButton").stringValue = updatedSettings.leftSecondaryButton;
                oldInputSettingsProp.FindPropertyRelative("leftGripDebugKey").stringValue = updatedSettings.leftGripDebugKey;
                oldInputSettingsProp.FindPropertyRelative("leftTriggerDebugKey").stringValue = updatedSettings.leftTriggerDebugKey;
                oldInputSettingsProp.FindPropertyRelative("leftThumbDebugKey").stringValue = updatedSettings.leftThumbDebugKey;
                
                oldInputSettingsProp.FindPropertyRelative("rightTriggerAxis").stringValue = updatedSettings.rightTriggerAxis;
                oldInputSettingsProp.FindPropertyRelative("rightGripAxis").stringValue = updatedSettings.rightGripAxis;
                oldInputSettingsProp.FindPropertyRelative("rightPrimaryButton").stringValue = updatedSettings.rightPrimaryButton;
                oldInputSettingsProp.FindPropertyRelative("rightSecondaryButton").stringValue = updatedSettings.rightSecondaryButton;
                oldInputSettingsProp.FindPropertyRelative("rightGripDebugKey").stringValue = updatedSettings.rightGripDebugKey;
                oldInputSettingsProp.FindPropertyRelative("rightTriggerDebugKey").stringValue = updatedSettings.rightTriggerDebugKey;
                oldInputSettingsProp.FindPropertyRelative("rightThumbDebugKey").stringValue = updatedSettings.rightThumbDebugKey;
                
                configSerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                
                Debug.Log("Updated Config file with default input names for empty fields");
            }
            
            Debug.Log($"Successfully created {axesToCreate.Count} input axes for Shababeek Interaction System");
        }
        
        private static void SetAxis(SerializedProperty property, (string name, string descriptiveName, float dead, int axis, int type, string positiveButton, float gravity, float sensitivity, string altPositiveButton) axe)
        {
            property.FindPropertyRelative("m_Name").stringValue = axe.name;
            property.FindPropertyRelative("descriptiveName").stringValue = axe.descriptiveName;
            property.FindPropertyRelative("positiveButton").stringValue = axe.positiveButton;
            property.FindPropertyRelative("gravity").floatValue = axe.gravity;
            property.FindPropertyRelative("dead").floatValue = axe.dead;
            property.FindPropertyRelative("sensitivity").floatValue = axe.sensitivity;
            property.FindPropertyRelative("type").intValue = axe.type;
            property.FindPropertyRelative("axis").intValue = axe.axis - 1;
            property.FindPropertyRelative("altPositiveButton").stringValue = axe.altPositiveButton;
        }
        
        private void CreateLayers()
        {
            // Define the layers to create with Shababeek_ prefix
            string[] layersToCreate = new[]
            {
                "Shababeek_LeftInteractor",
                "Shababeek_RightInteractor", 
                "Shababeek_Interactable",
                "Shababeek_PlayerLayer"
            };
            
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            
            int index = 6; // Start from layer 7 (index 6)
            int count = 0;
            
            // Find available slots and create layers
            while (index < 32 && count < layersToCreate.Length)
            {
                index++;
                if (layers.GetArrayElementAtIndex(index).stringValue == layersToCreate[count])
                {
                    count++;
                    continue;
                }
                if (layers.GetArrayElementAtIndex(index).stringValue?.Length > 0) continue;
             
                layers.GetArrayElementAtIndex(index).stringValue = layersToCreate[count];
                count++;
            }
            
            tagManager.ApplyModifiedProperties();
            
            // Update the config file with the new layer indices
            var configSerializedObject = new SerializedObject(config);
            var leftHandLayerIndex = LayerMask.NameToLayer("Shababeek_LeftInteractor");
            var rightHandLayerIndex = LayerMask.NameToLayer("Shababeek_RightInteractor");
            var interactableLayerIndex = LayerMask.NameToLayer("Shababeek_Interactable");
            var playerLayerIndex = LayerMask.NameToLayer("Shababeek_PlayerLayer");
            
            if (leftHandLayerIndex >= 0)
                configSerializedObject.FindProperty("leftHandLayer").intValue = leftHandLayerIndex;
            if (rightHandLayerIndex >= 0)
                configSerializedObject.FindProperty("rightHandLayer").intValue = rightHandLayerIndex;
            if (interactableLayerIndex >= 0)
                configSerializedObject.FindProperty("interactableLayer").intValue = interactableLayerIndex;
            if (playerLayerIndex >= 0)
                configSerializedObject.FindProperty("playerLayer").intValue = playerLayerIndex;
                
            configSerializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Updated Config with layer indices: Left={leftHandLayerIndex}, Right={rightHandLayerIndex}, Interactable={interactableLayerIndex}, Player={playerLayerIndex}");
            
            // Set up physics layer collisions
            SetupPhysicsLayerCollisions();
            
            Debug.Log($"Successfully created {count} layers for Shababeek Interaction System");
        }
        
        private void SetupPhysicsLayerCollisions()
        {
            var leftLayer = LayerMask.NameToLayer("Shababeek_LeftInteractor");
            var rightLayer = LayerMask.NameToLayer("Shababeek_RightInteractor");
            var playerLayer = LayerMask.NameToLayer("Shababeek_PlayerLayer");
            
            // Ignore collisions between hands and themselves
            Physics.IgnoreLayerCollision(leftLayer, leftLayer);
            Physics.IgnoreLayerCollision(rightLayer, rightLayer);
            Physics.IgnoreLayerCollision(rightLayer, leftLayer);
            
            // Ignore collisions between player and hands
            Physics.IgnoreLayerCollision(playerLayer, leftLayer);
            Physics.IgnoreLayerCollision(playerLayer, rightLayer);
            Physics.IgnoreLayerCollision(playerLayer, playerLayer);
            
            Debug.Log("Physics layer collision settings configured");
        }
    }
} 
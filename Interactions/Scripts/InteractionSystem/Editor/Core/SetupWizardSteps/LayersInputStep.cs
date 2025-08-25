using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Layers & Input step for the Shababeek Setup Wizard
    /// </summary>
    public class LayersInputStep : ISetupWizardStep
    {
        public string StepName => "Layers & Input";

        // Step-specific variables
        private string leftInteractorLayerName = "Shababeek_LeftInteractor";
        private string rightInteractorLayerName = "Shababeek_RightInteractor";
        private string interactableLayerName = "Shababeek_Interactable";
        private string playerLayerName = "Shababeek_PlayerLayer";
        
        private int leftHandLayer = -1;
        private int rightHandLayer = -1;
        private int interactableLayer = -1;
        private int playerLayer = -1;

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 2: Layer Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure project layers for interactions.", MessageType.Info);
            EditorGUILayout.Space();
            
            DrawCustomSettingsInput(wizard);
        }



        private void DrawCustomSettingsInput(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Custom Layer Configuration:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Enter the layer names you want to use. These layers will be created if they don't exist.", MessageType.Info);
            EditorGUILayout.Space();
            
            // Custom layer name input fields
            EditorGUILayout.LabelField("Layer Names:", EditorStyles.boldLabel);
            leftInteractorLayerName = EditorGUILayout.TextField("Left Interactor Layer Name", leftInteractorLayerName);
            rightInteractorLayerName = EditorGUILayout.TextField("Right Interactor Layer Name", rightInteractorLayerName);
            interactableLayerName = EditorGUILayout.TextField("Interactable Layer Name", interactableLayerName);
            playerLayerName = EditorGUILayout.TextField("Player Layer Name", playerLayerName);
            
            EditorGUILayout.Space();
            
            // Show current layer indices if they're set
            if (leftHandLayer >= 0 || rightHandLayer >= 0 || interactableLayer >= 0 || playerLayer >= 0)
            {
                EditorGUILayout.LabelField("Current layer indices:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Left: {leftHandLayer}, Right: {rightHandLayer}, Interactable: {interactableLayer}, Player: {playerLayer}");
                EditorGUILayout.Space();
            }
            
            // Show what will happen when proceeding
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("What will happen when you press Next:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Custom layers will be created in Project Settings", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Physics collision settings will be configured", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Layer settings will be saved to the Config asset", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Press Next to create these layers and apply the configuration.", MessageType.Info);
        }



        private void CreateCustomLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            
            string[] customLayerNames = new[]
            {
                leftInteractorLayerName,
                rightInteractorLayerName,
                interactableLayerName,
                playerLayerName
            };
            
            int index = 6;
            int count = 0;
            Dictionary<string, int> createdLayerIndices = new Dictionary<string, int>();
            
            while (index < 32 && count < customLayerNames.Length)
            {
                index++;
                if (layers.GetArrayElementAtIndex(index).stringValue == customLayerNames[count])
                {
                    // Layer already exists, record its index
                    createdLayerIndices[customLayerNames[count]] = index;
                    count++;
                    continue;
                }
                if (layers.GetArrayElementAtIndex(index).stringValue?.Length > 0) continue;
             
                // Create new layer and record its index
                layers.GetArrayElementAtIndex(index).stringValue = customLayerNames[count];
                createdLayerIndices[customLayerNames[count]] = index;
                count++;
            }
            
            tagManager.ApplyModifiedProperties();
            
            // Update the layer variables with the actual indices
            if (createdLayerIndices.TryGetValue(leftInteractorLayerName, out int leftIndex))
                leftHandLayer = leftIndex;
            if (createdLayerIndices.TryGetValue(rightInteractorLayerName, out int rightIndex))
                rightHandLayer = rightIndex;
            if (createdLayerIndices.TryGetValue(interactableLayerName, out int interactableIndex))
                interactableLayer = interactableIndex;
            if (createdLayerIndices.TryGetValue(playerLayerName, out int playerIndex))
                playerLayer = playerIndex;
            
            Debug.Log($"Created custom layers in project settings. Layer indices: Left={leftHandLayer}, Right={rightHandLayer}, Interactable={interactableLayer}, Player={playerLayer}");
        }

        private void ApplyPhysicsSettings()
        {
            var leftLayer = LayerMask.NameToLayer(leftInteractorLayerName);
            var rightLayer = LayerMask.NameToLayer(rightInteractorLayerName);
            var playerLayerIndex = LayerMask.NameToLayer(playerLayerName);

            if (leftLayer != -1 && rightLayer != -1 && playerLayerIndex != -1)
            {
                Physics.IgnoreLayerCollision(leftLayer, leftLayer);
                Physics.IgnoreLayerCollision(rightLayer, rightLayer);
                Physics.IgnoreLayerCollision(rightLayer, leftLayer);
                Physics.IgnoreLayerCollision(playerLayerIndex, leftLayer);
                Physics.IgnoreLayerCollision(playerLayerIndex, rightLayer);
                Physics.IgnoreLayerCollision(playerLayerIndex, playerLayerIndex);

                Debug.Log($"Applied physics layer collision settings for layers: {leftInteractorLayerName}, {rightInteractorLayerName}, {playerLayerName}");
            }
            else
            {
                Debug.LogWarning("Could not apply physics settings: Some layers were not found");
            }
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Initialize with default values when entering the step
            FindExistingLayers();
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            try
            {
                // Create the custom layers and apply physics settings when exiting the step
                CreateCustomLayers();
                ApplyPhysicsSettings();
                
                // Validate that layers were created successfully
                if (!ValidateLayerCreation())
                {
                    throw new System.InvalidOperationException("Failed to create one or more layers. Please check the console for details.");
                }
                
                // Update the config asset with the new layer settings
                if (wizard.ConfigAsset != null)
                {
                    var serializedConfig = new SerializedObject(wizard.ConfigAsset);
                    
                    if (leftHandLayer >= 0)
                    {
                        var leftLayerProperty = serializedConfig.FindProperty("leftHandLayer");
                        if (leftLayerProperty != null)
                            leftLayerProperty.intValue = 1 << leftHandLayer;
                    }
                    
                    if (rightHandLayer >= 0)
                    {
                        var rightLayerProperty = serializedConfig.FindProperty("rightHandLayer");
                        if (rightLayerProperty != null)
                            rightLayerProperty.intValue = 1 << rightHandLayer;
                    }
                    
                    if (interactableLayer >= 0)
                    {
                        var interactableLayerProperty = serializedConfig.FindProperty("interactableLayer");
                        if (interactableLayerProperty != null)
                            interactableLayerProperty.intValue = 1 << interactableLayer;
                    }
                    
                    if (playerLayer >= 0)
                    {
                        var playerLayerProperty = serializedConfig.FindProperty("playerLayer");
                        if (playerLayerProperty != null)
                            playerLayerProperty.intValue = 1 << playerLayer;
                    }
                    
                    serializedConfig.ApplyModifiedProperties();
                    EditorUtility.SetDirty(wizard.ConfigAsset);
                    AssetDatabase.SaveAssets();
                    
                    Debug.Log("Applied custom layer settings to config asset");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create layers: {e.Message}");
                
                // Show error dialog with recovery options
                int choice = EditorUtility.DisplayDialogComplex("Layer Creation Failed", 
                    $"Failed to create layers:\n{e.Message}\n\nWhat would you like to do?", 
                    "Try Again", "Go Back", "Continue Anyway");
                
                switch (choice)
                {
                    case 0: // Try Again
                        // Stay on this step by not advancing
                        wizard.NextStep(); // This will be called again
                        break;
                    case 1: // Go Back
                        // The wizard will handle going back
                        break;
                    case 2: // Continue Anyway
                        Debug.LogWarning("Continuing with layer creation failure - some features may not work properly");
                        break;
                }
            }
        }

        private bool ValidateLayerCreation()
        {
            try
            {
                // Check if all required layers exist
                var leftLayer = LayerMask.NameToLayer(leftInteractorLayerName);
                var rightLayer = LayerMask.NameToLayer(rightInteractorLayerName);
                var interactableLayerIndex = LayerMask.NameToLayer(interactableLayerName);
                var playerLayerIndex = LayerMask.NameToLayer(playerLayerName);
                
                if (leftLayer == -1 || rightLayer == -1 || interactableLayerIndex == -1 || playerLayerIndex == -1)
                {
                    Debug.LogError($"Layer validation failed: Left={leftLayer}, Right={rightLayer}, Interactable={interactableLayerIndex}, Player={playerLayerIndex}");
                    return false;
                }
                
                // Update the layer variables with the actual indices
                leftHandLayer = leftLayer;
                rightHandLayer = rightLayer;
                interactableLayer = interactableLayerIndex;
                playerLayer = playerLayerIndex;
                
                Debug.Log($"Layer validation passed: Left={leftHandLayer}, Right={rightHandLayer}, Interactable={interactableLayer}, Player={playerLayer}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Layer validation error: {e.Message}");
                return false;
            }
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Can always proceed - layers will be created when the step exits
            // The user just needs to enter the layer names they want to use
            return true;
        }

        private void FindExistingLayers()
        {
            leftHandLayer = LayerMask.NameToLayer(leftInteractorLayerName);
            rightHandLayer = LayerMask.NameToLayer(rightInteractorLayerName);
            interactableLayer = LayerMask.NameToLayer(interactableLayerName);
            playerLayer = LayerMask.NameToLayer(playerLayerName);
            
            // Log which layers were found
            if (leftHandLayer >= 0 || rightHandLayer >= 0 || interactableLayer >= 0 || playerLayer >= 0)
            {
                Debug.Log($"Found existing layers: Left={leftHandLayer}, Right={rightHandLayer}, Interactable={interactableLayer}, Player={playerLayer}");
            }
        }
    }
}

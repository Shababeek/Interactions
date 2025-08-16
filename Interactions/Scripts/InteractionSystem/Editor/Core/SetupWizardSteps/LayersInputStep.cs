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
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Press Next to apply these layer settings.", MessageType.Info);
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
            // Nothing special needed when exiting this step
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Check if custom layers are properly configured
            return leftHandLayer >= 0 && rightHandLayer >= 0 && 
                   interactableLayer >= 0 && playerLayer >= 0;
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

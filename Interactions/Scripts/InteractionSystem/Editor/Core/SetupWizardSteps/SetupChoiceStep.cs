using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Setup Choice step for the Shababeek Setup Wizard
    /// </summary>
    public class SetupChoiceStep : ISetupWizardStep
    {
        public string StepName => "Setup Choice";

        // Step-specific variables for default settings
        private const string LEFT_INTERACTOR_LAYER_NAME = "Shababeek_LeftInteractor";
        private const string RIGHT_INTERACTOR_LAYER_NAME = "Shababeek_RightInteractor";
        private const string INTERACTABLE_LAYER_NAME = "Shababeek_Interactable";
        private const string PLAYER_LAYER_NAME = "Shababeek_PlayerLayer";

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Choose Your Setup Method", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("Select how you want to configure the Shababeek Interaction System:", MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("Use Default Settings", GUILayout.Height(40)))
            {
                ApplyDefaultSettings(wizard);
                
                // Show completion message and close wizard
                EditorUtility.DisplayDialog("Setup Complete", "Default settings have been applied successfully!\n\n" +
                    "• Layers created and configured\n" +
                    "• Physics settings applied\n" +
                    "• Input System selected\n" +
                    "• Basic configuration complete\n\n" +
                    "The wizard will now close.", "OK");
                
                wizard.CloseWizard();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Customize Setup", GUILayout.Height(40)))
            {
                wizard.NextStep();
            }
        }

        private void ApplyDefaultSettings(ShababeekSetupWizard wizard)
        {
            if (wizard.ConfigAsset == null) return;
            wizard.useDefaultSettings = true;
            CreateLayersIfNeeded();
            
            if (LeftHandLayer >= 0)
                wizard.ConfigAsset.LeftHandLayer = 1 << LeftHandLayer;
            if (RightHandLayer >= 0)
                wizard.ConfigAsset.RightHandLayer = 1 << RightHandLayer;
            if (InteractableLayer >= 0)
                wizard.ConfigAsset.InteractableLayer = 1 << InteractableLayer;
            if (PlayerLayer >= 0)
                wizard.ConfigAsset.PlayerLayer = 1 << PlayerLayer;
            
            var serializedConfig = new SerializedObject(wizard.ConfigAsset);
            var inputTypeProperty = serializedConfig.FindProperty("inputType");
            if (inputTypeProperty != null)
                inputTypeProperty.enumValueIndex = (int)Config.InputManagerType.InputSystem;
            serializedConfig.ApplyModifiedProperties();
            
            ApplyPhysicsSettings();
            
            CreateDefaultHandDataIfNeeded(wizard);
            
            EditorUtility.SetDirty(wizard.ConfigAsset);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Applied default settings to config asset - setup complete");
        }

        private void CreateLayersIfNeeded()
        {
            FindExistingLayers();
            
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            
            string[] layersName = new[]
            {
                LEFT_INTERACTOR_LAYER_NAME,
                RIGHT_INTERACTOR_LAYER_NAME,
                INTERACTABLE_LAYER_NAME,
                PLAYER_LAYER_NAME
            };
            
            int index = 6;
            int count = 0;
            Dictionary<string, int> createdLayerIndices = new Dictionary<string, int>();
            
            while (index < 32 && count < layersName.Length)
            {
                index++;
                if (layers.GetArrayElementAtIndex(index).stringValue == layersName[count])
                {
                    // Layer already exists, record its index
                    createdLayerIndices[layersName[count]] = index;
                    count++;
                    continue;
                }
                if (layers.GetArrayElementAtIndex(index).stringValue?.Length > 0) continue;
             
                // Create new layer and record its index
                layers.GetArrayElementAtIndex(index).stringValue = layersName[count];
                createdLayerIndices[layersName[count]] = index;
                count++;
            }
            
            tagManager.ApplyModifiedProperties();
            
            // Update the layer variables with the actual indices
            if (createdLayerIndices.TryGetValue(LEFT_INTERACTOR_LAYER_NAME, out int leftIndex))
                LeftHandLayer = leftIndex;
            if (createdLayerIndices.TryGetValue(RIGHT_INTERACTOR_LAYER_NAME, out int rightIndex))
                RightHandLayer = rightIndex;
            if (createdLayerIndices.TryGetValue(INTERACTABLE_LAYER_NAME, out int interactableIndex))
                InteractableLayer = interactableIndex;
            if (createdLayerIndices.TryGetValue(PLAYER_LAYER_NAME, out int playerIndex))
                PlayerLayer = playerIndex;
            
            Debug.Log($"Created Shababeek layers in project settings. Layer indices: Left={LeftHandLayer}, Right={RightHandLayer}, Interactable={InteractableLayer}, Player={PlayerLayer}");
        }

        private void FindExistingLayers()
        {
            LeftHandLayer = LayerMask.NameToLayer(LEFT_INTERACTOR_LAYER_NAME);
            RightHandLayer = LayerMask.NameToLayer(RIGHT_INTERACTOR_LAYER_NAME);
            InteractableLayer = LayerMask.NameToLayer(INTERACTABLE_LAYER_NAME);
            PlayerLayer = LayerMask.NameToLayer(PLAYER_LAYER_NAME);
            
            // Log which layers were found
            if (LeftHandLayer >= 0 || RightHandLayer >= 0 || InteractableLayer >= 0 || PlayerLayer >= 0)
            {
                Debug.Log($"Found existing Shababeek layers: Left={LeftHandLayer}, Right={RightHandLayer}, Interactable={InteractableLayer}, Player={PlayerLayer}");
            }
        }

        private void ApplyPhysicsSettings()
        {
            var leftLayer = LayerMask.NameToLayer(LEFT_INTERACTOR_LAYER_NAME);
            var rightLayer = LayerMask.NameToLayer(RIGHT_INTERACTOR_LAYER_NAME);
            var playerLayerIndex = LayerMask.NameToLayer(PLAYER_LAYER_NAME);
            
            if (leftLayer != -1 && rightLayer != -1 && playerLayerIndex != -1)
            {
                Physics.IgnoreLayerCollision(leftLayer, leftLayer);
                Physics.IgnoreLayerCollision(rightLayer, rightLayer);
                Physics.IgnoreLayerCollision(rightLayer, leftLayer);
                Physics.IgnoreLayerCollision(playerLayerIndex, leftLayer);
                Physics.IgnoreLayerCollision(playerLayerIndex, rightLayer);
                Physics.IgnoreLayerCollision(playerLayerIndex, playerLayerIndex);
                
                Debug.Log($"Applied physics layer collision settings for layers: {LEFT_INTERACTOR_LAYER_NAME}, {RIGHT_INTERACTOR_LAYER_NAME}, {PLAYER_LAYER_NAME}");
            }
            else
            {
                Debug.LogWarning("Could not apply physics settings: Some layers were not found");
            }
        }

        private void CreateDefaultHandDataIfNeeded(ShababeekSetupWizard wizard)
        {
            // Check if any HandData assets exist
            string[] guids = AssetDatabase.FindAssets("t:HandData");
            if (guids.Length == 0)
            {
                // Create default HandData
                wizard.CreateDefaultHandDataAsset();
                Debug.Log("Created default HandData asset");
            }
        }

        // Properties for layer indices
        private int LeftHandLayer { get; set; } = -1;
        private int RightHandLayer { get; set; } = -1;
        private int InteractableLayer { get; set; } = -1;
        private int PlayerLayer { get; set; } = -1;

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Reset choices when entering this step
            wizard.ResetChoices();
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            // Nothing special needed when exiting this step
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Can proceed once a choice is made
            return true;
        }
    }
}

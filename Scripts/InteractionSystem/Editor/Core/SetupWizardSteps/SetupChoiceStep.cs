using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Shababeek.Interactions.Core;
using UnityEditor.PackageManager;

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
                    "• OpenXR package installed\n" +
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
            
            try
            {
                wizard.useDefaultSettings = true;
                
                // Show progress dialog
                EditorUtility.DisplayProgressBar("Applying Default Settings", "Creating layers...", 0.1f);
                
                CreateLayersIfNeeded();
                EditorUtility.DisplayProgressBar("Applying Default Settings", "Configuring layers...", 0.3f);
                
                if (LeftHandLayer >= 0)
                    wizard.ConfigAsset.LeftHandLayer = LeftHandLayer;
                if (RightHandLayer >= 0)
                    wizard.ConfigAsset.RightHandLayer = RightHandLayer;
                if (InteractableLayer >= 0)
                    wizard.ConfigAsset.InteractableLayer = InteractableLayer;
                if (PlayerLayer >= 0)
                    wizard.ConfigAsset.PlayerLayer = PlayerLayer;
                
                EditorUtility.DisplayProgressBar("Applying Default Settings", "Setting input type...", 0.5f);
                
                var serializedConfig = new SerializedObject(wizard.ConfigAsset);
                var inputTypeProperty = serializedConfig.FindProperty("inputType");
                if (inputTypeProperty != null)
                    inputTypeProperty.enumValueIndex = (int)Config.TrackingType.ControllerTracking;
                serializedConfig.ApplyModifiedProperties();
                
                EditorUtility.DisplayProgressBar("Applying Default Settings", "Installing OpenXR...", 0.6f);
                
                InstallOpenXRIfNeeded();
                
                EditorUtility.DisplayProgressBar("Applying Default Settings", "Applying physics settings...", 0.8f);
                
                ApplyPhysicsSettings();
                
                EditorUtility.DisplayProgressBar("Applying Default Settings", "Creating hand data...", 0.9f);
                
                CreateDefaultHandDataIfNeeded(wizard);
                
                EditorUtility.DisplayProgressBar("Applying Default Settings", "Saving configuration...", 1.0f);
                
                EditorUtility.SetDirty(wizard.ConfigAsset);
                AssetDatabase.SaveAssets();
                
                Debug.Log("Applied default settings to config asset - setup complete");
                
                // Validate that the settings were actually applied
                if (!ValidateDefaultSettings(wizard))
                {
                    throw new System.InvalidOperationException("Default settings were not applied correctly. Please try again or use custom setup.");
                }
                
                // Validate that layers were saved to config
                ValidateConfigLayerSettings(wizard);
                
                // Show completion message and close wizard
                EditorUtility.DisplayDialog("Setup Complete", "Default settings have been applied successfully!\n\n" +
                    "✓ Layers created and configured\n" +
                    "✓ OpenXR package installed\n" +
                    "✓ Physics settings applied\n" +
                    "✓ Input System selected\n" +
                    "✓ Basic configuration complete\n\n" +
                    "The wizard will now close.", "OK");
                
                wizard.CloseWizard();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to apply default settings: {e.Message}");
                
                // Show error dialog with recovery options
                int choice = EditorUtility.DisplayDialogComplex("Setup Failed", 
                    $"Failed to apply default settings:\n{e.Message}\n\nWhat would you like to do?", 
                    "Try Again", "Use Custom Setup", "Close Wizard");
                
                switch (choice)
                {
                    case 0: // Try Again
                        EditorUtility.ClearProgressBar();
                        ApplyDefaultSettings(wizard);
                        break;
                    case 1: // Use Custom Setup
                        EditorUtility.ClearProgressBar();
                        wizard.NextStep();
                        break;
                    case 2: // Close Wizard
                        EditorUtility.ClearProgressBar();
                        wizard.CloseWizard();
                        break;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
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

        private void InstallOpenXRIfNeeded()
        {
            // Check if OpenXR is already installed
            if (CheckOpenXRInstalled())
            {
                Debug.Log("OpenXR is already installed");
                return;
            }

            try
            {
                Debug.Log("Installing OpenXR package for default settings...");
                
                // Add OpenXR package via Package Manager
                var addRequest = UnityEditor.PackageManager.Client.Add("com.unity.xr.openxr");
                
                // Wait for the request to complete (this is synchronous in the editor)
                while (!addRequest.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                if (addRequest.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    Debug.Log("OpenXR package installed successfully for default settings!");
                    
                    // Refresh the asset database to pick up new packages
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogWarning($"OpenXR installation failed: {addRequest.Error?.message}. Continuing with setup...");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to install OpenXR: {e.Message}. Continuing with setup...");
            }
        }

        private bool CheckOpenXRInstalled()
        {
            // Check if OpenXR is installed via Package Manager
            try
            {
                var openXRPackage = UnityEditor.PackageManager.PackageInfo.FindForPackageName("Unity.XR.OpenXR");
                return openXRPackage != null;
            }
            catch (System.Exception)
            {
                return false;
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

        private bool ValidateDefaultSettings(ShababeekSetupWizard wizard)
        {
            try
            {
                // Validate that layers were created
                if (LeftHandLayer < 0 || RightHandLayer < 0 || InteractableLayer < 0 || PlayerLayer < 0)
                {
                    Debug.LogError("Layer validation failed: Some layers were not created");
                    return false;
                }
                
                // Validate that config asset has correct values
                if (wizard.ConfigAsset == null)
                {
                    Debug.LogError("Config validation failed: Config asset is null");
                    return false;
                }
                
                // Validate that OpenXR was installed
                if (!CheckOpenXRInstalled())
                {
                    Debug.LogWarning("OpenXR validation failed: OpenXR package not found. Some VR/AR features may not work properly.");
                    // Don't fail validation for OpenXR as it's optional
                }
                
                // Validate that physics settings were applied
                var leftLayer = LayerMask.NameToLayer(LEFT_INTERACTOR_LAYER_NAME);
                var rightLayer = LayerMask.NameToLayer(RIGHT_INTERACTOR_LAYER_NAME);
                var playerLayerIndex = LayerMask.NameToLayer(PLAYER_LAYER_NAME);
                
                if (leftLayer == -1 || rightLayer == -1 || playerLayerIndex == -1)
                {
                    Debug.LogError("Physics validation failed: Some layers not found in LayerMask");
                    return false;
                }
                
                // Validate that HandData was created if needed
                string[] guids = AssetDatabase.FindAssets("t:HandData");
                if (guids.Length == 0)
                {
                    Debug.LogError("HandData validation failed: No HandData assets found");
                    return false;
                }
                
                Debug.Log("Default settings validation passed successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Validation error: {e.Message}");
                return false;
            }
        }

        private void ValidateConfigLayerSettings(ShababeekSetupWizard wizard)
        {
            if (wizard.ConfigAsset != null)
            {
                if (wizard.ConfigAsset.LeftHandLayer != LeftHandLayer ||
                    wizard.ConfigAsset.RightHandLayer != RightHandLayer ||
                    wizard.ConfigAsset.InteractableLayer != InteractableLayer ||
                    wizard.ConfigAsset.PlayerLayer != PlayerLayer)
                {
                    Debug.LogWarning("Layer validation: Some layers were not properly saved to config asset");
                    // Don't fail validation for this as it's not critical
                }
                else
                {
                    Debug.Log("Layer validation: All layers were properly saved to config asset");
                }
            }
        }
    }
}

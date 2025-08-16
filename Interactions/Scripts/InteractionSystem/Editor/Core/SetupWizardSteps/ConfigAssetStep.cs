using UnityEditor;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Config Asset step for the Shababeek Setup Wizard
    /// </summary>
    public class ConfigAssetStep : ISetupWizardStep
    {
        public string StepName => "Config Asset";

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 1: Config Asset", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Config asset configuration and validation.", MessageType.Info);
            EditorGUILayout.Space();
            
            if (wizard.ConfigAsset != null)
            {
                EditorGUILayout.LabelField("Current Config:", EditorStyles.boldLabel);
                EditorGUILayout.ObjectField("Config Asset", wizard.ConfigAsset, typeof(Config), false);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Layer Settings:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Left Hand Layer: {LayerMask.LayerToName(wizard.ConfigAsset.LeftHandLayer)} ({wizard.ConfigAsset.LeftHandLayer})");
                EditorGUILayout.LabelField($"Right Hand Layer: {LayerMask.LayerToName(wizard.ConfigAsset.RightHandLayer)} ({wizard.ConfigAsset.RightHandLayer})");
                EditorGUILayout.LabelField($"Interactable Layer: {LayerMask.LayerToName(wizard.ConfigAsset.InteractableLayer)} ({wizard.ConfigAsset.InteractableLayer})");
                EditorGUILayout.LabelField($"Player Layer: {LayerMask.LayerToName(wizard.ConfigAsset.PlayerLayer)} ({wizard.ConfigAsset.PlayerLayer})");
                
                EditorGUILayout.Space();
                if (GUILayout.Button("Validate Config"))
                {
                    ValidateConfig(wizard);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No config asset found. Creating one...", MessageType.Warning);
                if (GUILayout.Button("Create Config Asset"))
                {
                    wizard.CreateDefaultConfigAsset();
                }
            }
        }

        private void ValidateConfig(ShababeekSetupWizard wizard)
        {
            if (wizard.ConfigAsset == null) return;
            
            bool isValid = true;
            string errors = "";
            
            if (wizard.ConfigAsset.LeftHandLayer == 0) { isValid = false; errors += "Left hand layer not set\n"; }
            if (wizard.ConfigAsset.RightHandLayer == 0) { isValid = false; errors += "Right hand layer not set\n"; }
            if (wizard.ConfigAsset.InteractableLayer == 0) { isValid = false; errors += "Interactable layer not set\n"; }
            if (wizard.ConfigAsset.PlayerLayer == 0) { isValid = false; errors += "Player layer not set\n"; }
            
            if (isValid)
            {
                EditorUtility.DisplayDialog("Config Validation", "Config asset is valid!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Config Validation", $"Config has issues:\n{errors}", "OK");
            }
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Nothing special needed when entering this step
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            // Nothing special needed when exiting this step
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Can proceed if config asset exists
            return wizard.ConfigAsset != null;
        }
    }
}

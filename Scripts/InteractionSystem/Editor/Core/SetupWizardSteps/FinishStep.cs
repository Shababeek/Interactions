using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Finish step for the Shababeek Setup Wizard
    /// </summary>
    public class FinishStep : ISetupWizardStep
    {
        public string StepName => "Finish";

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Validate the complete setup when entering the finish step
            ValidateCompleteSetup(wizard);
        }

        private void ValidateCompleteSetup(ShababeekSetupWizard wizard)
        {
            try
            {
                bool setupValid = true;
                string validationMessage = "Setup validation results:\n\n";
                
                // Validate config asset
                if (wizard.ConfigAsset == null)
                {
                    setupValid = false;
                    validationMessage += "✗ Config asset not found\n";
                }
                else
                {
                    validationMessage += "✓ Config asset found\n";
                }
                
                // Validate layers
                if (wizard.ConfigAsset != null)
                {
                    if (wizard.ConfigAsset.LeftHandLayer == 0)
                    {
                        setupValid = false;
                        validationMessage += "✗ Left hand layer not configured\n";
                    }
                    else
                    {
                        validationMessage += "✓ Left hand layer configured\n";
                    }
                    
                    if (wizard.ConfigAsset.RightHandLayer == 0)
                    {
                        setupValid = false;
                        validationMessage += "✗ Right hand layer not configured\n";
                    }
                    else
                    {
                        validationMessage += "✓ Right hand layer configured\n";
                    }
                    
                    if (wizard.ConfigAsset.InteractableLayer == 0)
                    {
                        setupValid = false;
                        validationMessage += "✗ Interactable layer not configured\n";
                    }
                    else
                    {
                        validationMessage += "✓ Interactable layer configured\n";
                    }
                    
                    if (wizard.ConfigAsset.PlayerLayer == 0)
                    {
                        setupValid = false;
                        validationMessage += "✗ Player layer not configured\n";
                    }
                    else
                    {
                        validationMessage += "✓ Player layer configured\n";
                    }
                }

                // Validate hand data
                if (wizard.SelectedHandData != null)
                {
                    validationMessage += $"✓ Hand data: {wizard.SelectedHandData.name}\n";
                    
                    if (wizard.SelectedHandData.Poses != null && wizard.SelectedHandData.Poses.Length > 0)
                    {
                        validationMessage += $"✓ Hand poses: {wizard.SelectedHandData.Poses.Length} found\n";
                    }
                    else
                    {
                        setupValid = false;
                        validationMessage += "✗ No hand poses found\n";
                    }
                }
                else
                {
                    setupValid = false;
                    validationMessage += "✗ No hand data selected\n";
                }
                
                // Log validation results
                if (setupValid)
                {
                    Debug.Log("Setup validation passed successfully!");
                }
                else
                {
                    Debug.LogWarning("Setup validation found issues:\n" + validationMessage);
                }
                
                // Store validation results for display
                _setupValidationMessage = validationMessage;
                _setupIsValid = setupValid;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Setup validation error: {e.Message}");
                _setupValidationMessage = $"Setup validation error: {e.Message}";
                _setupIsValid = false;
            }
        }

        private bool CheckInputAxesExist()
        {
            try
            {
                var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
                var serializedObject = new SerializedObject(inputManagerAsset);
                var axis = serializedObject.FindProperty("m_Axes");
                
                if (axis == null || !axis.isArray) return false;
                
                for (int i = 0; i < axis.arraySize; i++)
                {
                    var element = axis.GetArrayElementAtIndex(i);
                    var name = element.FindPropertyRelative("m_Name")?.stringValue;
                    
                    if (!string.IsNullOrEmpty(name) && name.StartsWith("Shababeek_"))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
            
            // Show validation results
            if (_setupIsValid)
            {
                EditorGUILayout.HelpBox("✓ Setup validation passed! Your Shababeek Interaction System is ready to use.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠ Setup validation found some issues. Some features may not work properly.", MessageType.Warning);
            }
            
            EditorGUILayout.Space();
            
            if (wizard.useDefaultSettings && wizard.SelectedHandData != null)
            {
                EditorGUILayout.HelpBox($"You have completed the setup using default settings with the '{wizard.SelectedHandData.name}' hand configuration.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("You have completed the initial setup. Refer to the documentation for advanced configuration and usage.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("What was configured:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("✓ Config asset created/configured");
            EditorGUILayout.LabelField("✓ Layer collision physics applied");
            EditorGUILayout.LabelField("✓ Input manager type set");
            
            if (wizard.SelectedHandData != null)
            {
                EditorGUILayout.LabelField("✓ HandData linked to config");
            }
            
            // Show detailed validation results
            if (!string.IsNullOrEmpty(_setupValidationMessage))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Detailed Validation Results:", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(_setupValidationMessage, _setupIsValid ? MessageType.Info : MessageType.Warning);
            }
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Documentation"))
                Application.OpenURL("https://github.com/Shababeek/Interactions/tree/master/Assets/Shababeek/Documentation");
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            // Nothing special needed when exiting this step
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Finish step can always proceed (it's the last step)
            return true;
        }

        // Private fields for validation results
        private string _setupValidationMessage = "";
        private bool _setupIsValid = false;
    }
}

using UnityEditor;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Input Method step for the Shababeek Setup Wizard
    /// </summary>
    public class InputMethodStep : ISetupWizardStep
    {
        public string StepName => "Input Method";

        private Config.InputManagerType selectedInputType = Config.InputManagerType.InputSystem;

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 3: Input Method Selection", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose the input system you want to use for interactions.", MessageType.Info);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Input Type Selection:", EditorStyles.boldLabel);
            selectedInputType = (Config.InputManagerType)EditorGUILayout.EnumPopup("Input Manager Type", selectedInputType);
            
            EditorGUILayout.Space();
            
            if (selectedInputType == Config.InputManagerType.InputManager)
            {
                EditorGUILayout.HelpBox("Input Manager (Legacy) selected. This will create the necessary axis mappings for hand interactions.", MessageType.Info);
                EditorGUILayout.LabelField("The following axes will be created:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• Left Hand Grip");
                EditorGUILayout.LabelField("• Right Hand Grip");
                EditorGUILayout.LabelField("• Left Hand Trigger");
                EditorGUILayout.LabelField("• Right Hand Trigger");
                EditorGUILayout.LabelField("• Left Hand Thumb");
                EditorGUILayout.LabelField("• Right Hand Thumb");
            }
            else
            {
                EditorGUILayout.HelpBox("Input System selected. This is the modern input system and will be configured for future functionality.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Press Next to apply the input method selection.", MessageType.Info);
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Initialize with the current config setting if available
            if (wizard.ConfigAsset != null)
            {
                selectedInputType = wizard.ConfigAsset.InputType;
            }
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            // Apply the input method selection
            if (wizard.ConfigAsset != null)
            {
                var serializedConfig = new SerializedObject(wizard.ConfigAsset);
                var inputTypeProperty = serializedConfig.FindProperty("inputType");
                if (inputTypeProperty != null)
                {
                    inputTypeProperty.enumValueIndex = (int)selectedInputType;
                    serializedConfig.ApplyModifiedProperties();
                    
                    // If Input Manager is selected, create the axis mappings
                    if (selectedInputType == Config.InputManagerType.InputManager)
                    {
                        CreateInputManagerAxes();
                    }
                    
                    EditorUtility.SetDirty(wizard.ConfigAsset);
                    AssetDatabase.SaveAssets();
                    
                    Debug.Log($"Applied input method: {selectedInputType}");
                }
            }
        }

        private void CreateInputManagerAxes()
        {
            // TODO: Call the InteractionSystemLoader functions to create axis mappings
            // This will be implemented when you define the specific functionality
            
            Debug.Log("Input Manager selected - axis mappings will be created by InteractionSystemLoader");
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Can always proceed - validation and application happens in OnStepExit
            return true;
        }
    }
}

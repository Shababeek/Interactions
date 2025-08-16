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

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
            
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
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Documentation"))
                Application.OpenURL("https://github.com/Shababeek/Interactions/tree/master/Assets/Shababeek/Documentation");
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
            // Finish step can always proceed (it's the last step)
            return true;
        }
    }
}

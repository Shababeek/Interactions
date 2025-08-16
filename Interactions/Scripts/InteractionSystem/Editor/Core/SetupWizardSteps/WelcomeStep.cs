using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Welcome step for the Shababeek Setup Wizard
    /// </summary>
    public class WelcomeStep : ISetupWizardStep
    {
        public string StepName => "Welcome";

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Welcome to the Shababeek Interaction System Setup Wizard!", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            
            // Check if this is the first time setup
            bool isFirstTimeSetup = !EditorPrefs.GetBool(ShababeekSetupWizard.SetupWizardShownKey, false);
            if (isFirstTimeSetup)
            {
                EditorGUILayout.HelpBox("This appears to be your first time setting up Shababeek. Let's get you started!", MessageType.Info);
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.LabelField("This wizard will guide you through the initial setup:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Config asset creation", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- HandData and hand prefab setup", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Layer and input configuration", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            
            // Show current config status
            if (wizard.ConfigAsset != null)
            {
                EditorGUILayout.HelpBox($"Found existing config: {wizard.ConfigAsset.name}", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Option to prevent showing again
            bool preventShowingAgain = EditorGUILayout.Toggle("Don't show this wizard again automatically", false);
            if (preventShowingAgain)
            {
                EditorPrefs.SetBool(ShababeekSetupWizard.SetupWizardShownKey, true);
            }
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Documentation"))
                Application.OpenURL("https://github.com/Shababeek/Interactions/tree/master/Assets/Shababeek/Documentation");
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Nothing special needed when entering welcome step
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            // Nothing special needed when exiting welcome step
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Welcome step can always proceed
            return true;
        }
    }
}

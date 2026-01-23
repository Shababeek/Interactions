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
            // Header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Welcome to Shababeek Interactions!", headerStyle);
            EditorGUILayout.Space();

            // Check if this is the first time setup
            bool isFirstTimeSetup = !EditorPrefs.GetBool(ShababeekSetupWizard.SetupWizardShownKey, false);
            if (isFirstTimeSetup)
            {
                EditorGUILayout.HelpBox("This appears to be your first time setting up the system. Let's get you started!", MessageType.Info);
                EditorGUILayout.Space();
            }

            // System description
            EditorGUILayout.LabelField("A physics-based VR interaction system for Unity featuring:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Physics Hand Interactions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Grab, push, and manipulate objects with realistic physics.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Controller & Hand Tracking", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Support for VR controllers and optical hand tracking (XR Hands).", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Constrained Interactables", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Levers, wheels, drawers, joysticks, buttons, and switches.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Scriptable System", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Variables, events, and binders for no-code reactive data flow.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // This wizard will configure
            EditorGUILayout.LabelField("This wizard will configure:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Config asset with system settings", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Physics layers for hand/object interactions", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• Tracking type (controller or hand tracking)", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("• HandData with poses and prefabs", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();

            // Show current config status
            if (wizard.ConfigAsset != null)
            {
                EditorGUILayout.HelpBox($"Found existing config: {wizard.ConfigAsset.name}", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Option to prevent showing again
            bool preventShowingAgain = EditorGUILayout.Toggle("Don't show this wizard again automatically",
                EditorPrefs.GetBool(ShababeekSetupWizard.SetupWizardShownKey, false));

            if (preventShowingAgain != EditorPrefs.GetBool(ShababeekSetupWizard.SetupWizardShownKey, false))
            {
                EditorPrefs.SetBool(ShababeekSetupWizard.SetupWizardShownKey, preventShowingAgain);
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

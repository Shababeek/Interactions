using UnityEditor;
using UnityEngine;

namespace Shababeek.Core.Editors
{
    public class ShababeekSetupWizard : EditorWindow
    {
        private int step = 0;
        private readonly string[] steps =
        {
            "Welcome",
            "Config Asset",
            "HandData",
            "Hand Prefabs",
            "Layers & Input",
            "Finish"
        };

        [MenuItem("Tools/Shababeek/Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShababeekSetupWizard>(true, "Shababeek Setup Wizard");
            window.minSize = new Vector2(500, 350);
        }

        private void OnGUI()
        {
            GUILayout.Label($"Step {step + 1} of {steps.Length}: {steps[step]}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            switch (step)
            {
                case 0:
                    DrawWelcome();
                    break;
                case 1:
                    DrawConfigStep();
                    break;
                case 2:
                    DrawHandDataStep();
                    break;
                case 3:
                    DrawHandPrefabsStep();
                    break;
                case 4:
                    DrawLayersInputStep();
                    break;
                case 5:
                    DrawFinishStep();
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (step > 0 && GUILayout.Button("Back")) step--;
            GUILayout.FlexibleSpace();
            if (step < steps.Length - 1 && GUILayout.Button("Next")) step++;
            if (step == steps.Length - 1 && GUILayout.Button("Close")) Close();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawWelcome()
        {
            EditorGUILayout.LabelField("Welcome to the Shababeek Interaction System Setup Wizard!", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("This wizard will guide you through the initial setup:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Config asset creation", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- HandData and hand prefab setup", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("- Layer and input configuration", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Documentation"))
                Application.OpenURL("https://github.com/Shababeek/Interactions/tree/master/Assets/Shababeek/Documentation");
        }

        private void DrawConfigStep()
        {
            EditorGUILayout.LabelField("Step 1: Config Asset", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create or select a Config asset. This asset holds all system-wide settings.", MessageType.Info);
            // TODO: Add logic to create/select Config asset
        }

        private void DrawHandDataStep()
        {
            EditorGUILayout.LabelField("Step 2: HandData", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create or select a HandData asset, and assign required poses and masks.", MessageType.Info);
            // TODO: Add logic to create/select HandData asset and assign poses/masks
        }

        private void DrawHandPrefabsStep()
        {
            EditorGUILayout.LabelField("Step 3: Hand Prefabs", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create or select hand prefabs, add PoseConstrainer, and link to HandData.", MessageType.Info);
            // TODO: Add logic to create/select hand prefabs and link PoseConstrainer/HandData
        }

        private void DrawLayersInputStep()
        {
            EditorGUILayout.LabelField("Step 4: Layers & Input", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Validate and configure project layers and input settings.", MessageType.Info);
            // TODO: Add logic to validate and guide layer/input setup
        }

        private void DrawFinishStep()
        {
            EditorGUILayout.LabelField("Setup Complete!", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("You have completed the initial setup. Refer to the documentation for advanced configuration and usage.", MessageType.Info);
            if (GUILayout.Button("Open Documentation"))
                Application.OpenURL("https://github.com/Shababeek/Interactions/tree/master/Assets/Shababeek/Documentation");
        }
    }
} 
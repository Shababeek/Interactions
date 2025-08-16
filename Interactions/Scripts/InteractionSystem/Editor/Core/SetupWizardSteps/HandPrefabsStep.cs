using UnityEditor;
using UnityEngine;
using Shababeek.Interactions.Animations;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Hand Prefabs step for the Shababeek Setup Wizard
    /// </summary>
    public class HandPrefabsStep : ISetupWizardStep
    {
        public string StepName => "Hand Prefabs";

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 3: Hand Prefabs", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create or select hand prefabs, add PoseConstrainer, and link to HandData.", MessageType.Info);
            EditorGUILayout.Space();
            
            if (wizard.SelectedHandData != null)
            {
                EditorGUILayout.LabelField("Selected HandData:", EditorStyles.boldLabel);
                EditorGUILayout.ObjectField(wizard.SelectedHandData, typeof(HandData), false);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Hand Prefabs:", EditorStyles.boldLabel);
                EditorGUILayout.ObjectField("Left Hand Prefab", wizard.SelectedHandData.LeftHandPrefab, typeof(HandPoseController), false);
                EditorGUILayout.ObjectField("Right Hand Prefab", wizard.SelectedHandData.RightHandPrefab, typeof(HandPoseController), false);
                
                if (GUILayout.Button("Validate Hand Prefabs"))
                {
                    ValidateHandPrefabs(wizard);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a HandData asset first.", MessageType.Warning);
            }
        }

        private void ValidateHandPrefabs(ShababeekSetupWizard wizard)
        {
            if (wizard.SelectedHandData == null) return;
            
            bool isValid = true;
            string errors = "";
            
            if (wizard.SelectedHandData.LeftHandPrefab == null) { isValid = false; errors += "Left hand prefab not assigned\n"; }
            if (wizard.SelectedHandData.RightHandPrefab == null) { isValid = false; errors += "Right hand prefab not assigned\n"; }
            
            if (wizard.SelectedHandData.Poses.Length == 0) { isValid = false; errors += "No poses defined\n"; }
            
            if (isValid)
            {
                EditorUtility.DisplayDialog("Hand Prefabs Validation", "Hand prefabs are valid!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Hand Prefabs Validation", $"Hand prefabs have issues:\n{errors}", "OK");
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
            // Can proceed if HandData is selected
            return wizard.SelectedHandData != null;
        }
    }
}

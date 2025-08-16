using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Hand Choice step for the Shababeek Setup Wizard
    /// </summary>
    public class HandChoiceStep : ISetupWizardStep
    {
        public string StepName => "Hand Choice";

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Choose Your Hand Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("Would you like to use one of the provided hands or create your own?", MessageType.Info);
            EditorGUILayout.Space();

            if (GUILayout.Button("Use Provided Hands", GUILayout.Height(40)))
            {
                wizard.UseProvidedHands = true;
                wizard.NextStep();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Make Your Own", GUILayout.Height(40)))
            {
                wizard.UseProvidedHands = false;
                wizard.NextStep();
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
            // Can proceed once a choice is made
            return true;
        }
    }
}

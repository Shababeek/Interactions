using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Interface for setup wizard steps to ensure consistency and modularity
    /// </summary>
    public interface ISetupWizardStep
    {
        /// <summary>
        /// The name/identifier for this step
        /// </summary>
        string StepName { get; }
        
        /// <summary>
        /// Draws the step's UI
        /// </summary>
        /// <param name="wizard">Reference to the main setup wizard</param>
        void DrawStep(ShababeekSetupWizard wizard);
        
        /// <summary>
        /// Called when the step is entered
        /// </summary>
        /// <param name="wizard">Reference to the main setup wizard</param>
        void OnStepEnter(ShababeekSetupWizard wizard);
        
        /// <summary>
        /// Called when the step is exited
        /// </summary>
        /// <param name="wizard">Reference to the main setup wizard</param>
        void OnStepExit(ShababeekSetupWizard wizard);
        
        /// <summary>
        /// Validates if the step can proceed to the next step
        /// </summary>
        /// <param name="wizard">Reference to the main setup wizard</param>
        /// <returns>True if the step is valid and can proceed</returns>
        bool CanProceed(ShababeekSetupWizard wizard);
    }
}

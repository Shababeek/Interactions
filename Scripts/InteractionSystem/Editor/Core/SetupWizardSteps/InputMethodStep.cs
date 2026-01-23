using UnityEditor;
using UnityEngine;
using Shababeek.Interactions.Core;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Tracking Type step for the Shababeek Setup Wizard.
    /// Allows selection between Controller Tracking and Hand Tracking (if available).
    /// </summary>
    public class InputMethodStep : ISetupWizardStep
    {
        public string StepName => "Tracking Type";

        private Config.TrackingType selectedTrackingType = Config.TrackingType.ControllerTracking;
        private bool _xrHandsAvailable = false;

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 3: Tracking Type Selection", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose the tracking method for hand interactions.", MessageType.Info);
            EditorGUILayout.Space();

            // Check for XR Hands availability
            CheckXRHandsAvailability();

            EditorGUILayout.LabelField("Tracking Type:", EditorStyles.boldLabel);

            // Controller Tracking option
            EditorGUILayout.BeginVertical("box");
            bool isControllerSelected = selectedTrackingType == Config.TrackingType.ControllerTracking;
            if (GUILayout.Toggle(isControllerSelected, " Controller Tracking", EditorStyles.radioButton))
            {
                selectedTrackingType = Config.TrackingType.ControllerTracking;
            }
            EditorGUILayout.LabelField("Uses VR controller input for finger animations.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Trigger, grip, and thumbstick inputs animate the hand.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

#if XR_HANDS_AVAILABLE
            // Hand Tracking option (only if XR Hands is available)
            EditorGUILayout.BeginVertical("box");
            bool isHandTrackingSelected = selectedTrackingType == Config.TrackingType.HandTracking;
            if (GUILayout.Toggle(isHandTrackingSelected, " Hand Tracking", EditorStyles.radioButton))
            {
                selectedTrackingType = Config.TrackingType.HandTracking;
            }
            EditorGUILayout.LabelField("Uses optical hand tracking for natural hand poses.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Requires XR Hands package and compatible hardware.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Both option
            EditorGUILayout.BeginVertical("box");
            bool isBothSelected = selectedTrackingType == Config.TrackingType.Both;
            if (GUILayout.Toggle(isBothSelected, " Both (Automatic Switching)", EditorStyles.radioButton))
            {
                selectedTrackingType = Config.TrackingType.Both;
            }
            EditorGUILayout.LabelField("Automatically switches between controller and hand tracking.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Uses hand tracking when available, falls back to controllers.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
#else
            // Show info about XR Hands if not available
            if (!_xrHandsAvailable)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "Hand Tracking requires the XR Hands package.\n\n" +
                    "To enable hand tracking:\n" +
                    "1. Install 'com.unity.xr.hands' via Package Manager\n" +
                    "2. The Hand Tracking option will appear here",
                    MessageType.Info);
            }
#endif

            EditorGUILayout.Space();

            // Input Actions info
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Input Configuration", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The system uses Unity's Input System package.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("You can configure input action references in the Config asset:", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("• Thumb, Index, Middle, Ring, Pinky actions per hand", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Each action controls finger curl animation", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Show current selection
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Current Selection:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(GetTrackingTypeDisplayName(selectedTrackingType), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Press Next to apply the tracking type and continue.", MessageType.Info);
        }

        private string GetTrackingTypeDisplayName(Config.TrackingType trackingType)
        {
            return trackingType switch
            {
                Config.TrackingType.ControllerTracking => "Controller Tracking",
#if XR_HANDS_AVAILABLE
                Config.TrackingType.HandTracking => "Hand Tracking",
                Config.TrackingType.Both => "Both (Auto-Switch)",
#endif
                _ => trackingType.ToString()
            };
        }

        private void CheckXRHandsAvailability()
        {
#if XR_HANDS_AVAILABLE
            _xrHandsAvailable = true;
#else
            _xrHandsAvailable = false;
#endif
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Initialize with the current config setting if available
            if (wizard.ConfigAsset != null)
            {
                selectedTrackingType = wizard.ConfigAsset.InputType;
                Debug.Log($"InputMethodStep: Initialized with tracking type: {selectedTrackingType}");
            }

            CheckXRHandsAvailability();
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            try
            {
                // Apply the tracking type selection
                if (wizard.ConfigAsset != null)
                {
                    var serializedConfig = new SerializedObject(wizard.ConfigAsset);
                    var inputTypeProperty = serializedConfig.FindProperty("inputType");
                    if (inputTypeProperty != null)
                    {
                        inputTypeProperty.enumValueIndex = (int)selectedTrackingType;
                        serializedConfig.ApplyModifiedProperties();

                        EditorUtility.SetDirty(wizard.ConfigAsset);
                        AssetDatabase.SaveAssets();

                        Debug.Log($"Applied tracking type: {selectedTrackingType}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to apply tracking type: {e.Message}");

                int choice = EditorUtility.DisplayDialogComplex("Tracking Setup Failed",
                    $"Failed to apply tracking type:\n{e.Message}\n\nWhat would you like to do?",
                    "Try Again", "Go Back", "Continue Anyway");

                switch (choice)
                {
                    case 0: // Try Again
                        wizard.NextStep();
                        break;
                    case 1: // Go Back
                        break;
                    case 2: // Continue Anyway
                        Debug.LogWarning("Continuing with tracking setup failure - some features may not work properly");
                        break;
                }
            }
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            return true;
        }
    }
}

using UnityEditor;
using UnityEngine;
using Shababeek.Interactions.Core;
using System.Collections.Generic;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Input Method step for the Shababeek Setup Wizard
    /// </summary>
    public class InputMethodStep : ISetupWizardStep
    {
        public string StepName => "Input Method";

        private Config.TrackingType selectedInputType = Config.TrackingType.ControllerTracking;
        private bool axesCreated = false;
        private bool isCreatingAxes = false;

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            //todo remake this step
            EditorGUILayout.LabelField("Step 3: Input Method Selection", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose the input system you want to use for interactions.", MessageType.Info);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Input Type Selection:", EditorStyles.boldLabel);
            selectedInputType = (Config.TrackingType)EditorGUILayout.EnumPopup("Input Manager Type", selectedInputType);
            
            EditorGUILayout.Space();
            
            if (selectedInputType == Config.TrackingType.ControllerTracking)
            {
                EditorGUILayout.HelpBox("Input Manager (Legacy) selected. This will create the necessary axis mappings for hand interactions.", MessageType.Info);
                EditorGUILayout.LabelField("The following axes will be created:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• Left Hand Grip");
                EditorGUILayout.LabelField("• Right Hand Grip");
                EditorGUILayout.LabelField("• Left Hand Trigger");
                EditorGUILayout.LabelField("• Right Hand Trigger");
                EditorGUILayout.LabelField("• Left Hand Thumb");
                EditorGUILayout.LabelField("• Right Hand Thumb");
                EditorGUILayout.LabelField("• And many more...");
                
                EditorGUILayout.Space();
                
                if (axesCreated)
                {
                    EditorGUILayout.HelpBox("✓ Input Manager axes have been created successfully!", MessageType.Info);
                }
                else if (isCreatingAxes)
                {
                    EditorGUILayout.LabelField("Creating Input Manager axes...", EditorStyles.boldLabel);
                    EditorGUI.ProgressBar(GUILayoutUtility.GetRect(0, 20), 0.5f, "Creating Input Axes...");
                }
                else
                {
                    if (GUILayout.Button("Create Input Manager Axes"))
                    {
                        CreateInputManagerAxes();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Input System selected. This is the modern input system and will be configured for future functionality.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Show current selection status
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Current Selection:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(selectedInputType.ToString(), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Press Next to apply the input method selection and continue.", MessageType.Info);
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Initialize with the current config setting if available
            if (wizard.ConfigAsset != null)
            {
                selectedInputType = wizard.ConfigAsset.InputType;
                Debug.Log($"InputMethodStep: Initialized with input type: {selectedInputType}");
            }
            else
            {
                Debug.LogWarning("InputMethodStep: No config asset found, using default InputSystem");
            }
            
            // Check if axes already exist
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            try
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
                       
                        EditorUtility.SetDirty(wizard.ConfigAsset);
                        AssetDatabase.SaveAssets();
                        
                        Debug.Log($"Applied input method: {selectedInputType}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to apply input method: {e.Message}");
                
                // Show error dialog with recovery options
                int choice = EditorUtility.DisplayDialogComplex("Input Setup Failed", 
                    $"Failed to apply input method:\n{e.Message}\n\nWhat would you like to do?", 
                    "Try Again", "Go Back", "Continue Anyway");
                
                switch (choice)
                {
                    case 0: // Try Again
                        // Stay on this step by not advancing
                        wizard.NextStep(); // This will be called again
                        break;
                    case 1: // Go Back
                        // The wizard will handle going back
                        break;
                    case 2: // Continue Anyway
                        Debug.LogWarning("Continuing with input setup failure - some input features may not work properly");
                        break;
                }
            }
        }

        private void CreateInputManagerAxes()
        {
            if (isCreatingAxes) return; // Prevent multiple calls
            
            isCreatingAxes = true;
            axesCreated = false;
            
            try
            {
                // Create the axes using the same logic as InteractionSystemLoader
                CreateInputManagerAxesInternal();
                
                axesCreated = true;
                isCreatingAxes = false;
                
                Debug.Log("Input Manager axes created successfully!");
                
                // Force a repaint to show the success message
                if (EditorWindow.HasOpenInstances<ShababeekSetupWizard>())
                {
                    EditorWindow.GetWindow<ShababeekSetupWizard>().Repaint();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create Input Manager axes: {e.Message}");
                isCreatingAxes = false;
                
                EditorUtility.DisplayDialog("Axis Creation Failed", 
                    $"Failed to create Input Manager axes:\n{e.Message}\n\nYou may need to manually configure the input axes.", 
                    "OK");
            }
        }

        private void CreateInputManagerAxesInternal()
        {
            // Define the axis list (copied from InteractionSystemLoader)
            var axisList = new List<AxeData>();
            axisList.Add (new AxeData("Shababeek_Left_Trigger", "Device trigger axis", 0f, 9, 2, "", 0f, 1f, ""));
            axisList.Add(new AxeData("Shababeek_Left_Grip", "Device grip axis", 0, 11, 2, "", 0, 1, ""));
            axisList.Add(new AxeData("Shababeek_Left_PrimaryButton", "Device primary button", 0, 0, 0, "joystick button 2", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Left_SecondaryButton", "Device secondary button", 0, 0, 0, "joystick button 3", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Left_GripButton", "Device grip button", 0, 0, 0, "joystick button 4", 0, 0.1f, ""));
            axisList.Add(new AxeData("Shababeek_Left_TriggerButton", "Device trigger button", 0, 0, 0, "joystick button 14", 0, 0.1f, ""));
            axisList.Add(new AxeData("Shababeek_Left_Grip_DebugKey", "Left Grip Debug Key (Z)", 0, 0, 0, "z", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Left_Index_DebugKey", "Left Index Debug Key (X)", 0, 0, 0, "x", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Left_Primary_DebugKey", "Left Primary Debug Key (C)", 0, 0, 0, "c", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Right_Trigger", "Device trigger axis", 0f, 10, 2, "", 0f, 1f, ""));
            axisList.Add(new AxeData("Shababeek_Right_Grip", "Device grip axis", 0, 12, 2, "", 0, 1, ""));
            axisList.Add(new AxeData("Shababeek_Right_PrimaryButton", "Device primary button", 0, 0, 0, "joystick button 0", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Right_SecondaryButton", "Device secondary button", 0, 0, 0, "joystick button 1", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Right_GripButton", "Device grip button", 0, 0, 0, "joystick button 5", 0, 0.1f, ""));
            axisList.Add(new AxeData("Shababeek_Right_TriggerButton", "Device trigger button", 0, 0, 0, "joystick button 15", 0, 0.1f, ""));
            axisList.Add(new AxeData("Shababeek_Right_Grip_DebugKey", "Right Grip Debug Key (m)", 0, 0, 0, "m", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Right_Index_DebugKey", "Right Index Debug Key (n)", 0, 0, 0, "n", 1000.0f, 1000.0f, ""));
            axisList.Add(new AxeData("Shababeek_Right_Primary_DebugKey", "Right Primary Debug Key (b)", 0, 0, 0, "b", 1000.0f, 1000.0f, ""));

            // Load the InputManager asset
            var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            var serializedObject = new SerializedObject(inputManagerAsset);
            var axis = serializedObject.FindProperty("m_Axes");
            
            if (axis == null || !axis.isArray)
            {
                throw new System.InvalidOperationException("Could not access InputManager axes property");
            }

            // Remove any existing Shababeek axes to avoid duplicates
            RemoveExistingShababeekAxes(axis);

            // Add new axes
            int count = axis.arraySize;
            for (var i = 0; i < axisList.Count; i++)
            {
                var axe = axisList[i];
                axis.InsertArrayElementAtIndex(count + i);
                var property = axis.GetArrayElementAtIndex(count + i);
                SetAxis(property, axe);
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void RemoveExistingShababeekAxes(SerializedProperty axis)
        {
            // Remove existing Shababeek axes to avoid duplicates
            for (int i = axis.arraySize - 1; i >= 0; i--)
            {
                var element = axis.GetArrayElementAtIndex(i);
                var name = element.FindPropertyRelative("m_Name")?.stringValue;
                
                if (!string.IsNullOrEmpty(name) && name.StartsWith("Shababeek_"))
                {
                    axis.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private static void SetAxis(SerializedProperty property, AxeData axe)
        {
            property.FindPropertyRelative("m_Name").stringValue = axe.Name;
            property.FindPropertyRelative("descriptiveName").stringValue = axe.DescriptiveName;
            property.FindPropertyRelative("positiveButton").stringValue = axe.PositiveButton;
            property.FindPropertyRelative("gravity").floatValue = axe.Gravity;
            property.FindPropertyRelative("dead").floatValue = axe.Dead;
            property.FindPropertyRelative("sensitivity").floatValue = axe.Sensitivity;
            property.FindPropertyRelative("type").intValue = axe.Type;
            property.FindPropertyRelative("axis").intValue = axe.Axis - 1;
            property.FindPropertyRelative("altPositiveButton").stringValue = axe.AltPositiveButton;
        }

        private bool CheckIfAxesExist()
        {
            try
            {
                var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
                var serializedObject = new SerializedObject(inputManagerAsset);
                var axis = serializedObject.FindProperty("m_Axes");
                
                if (axis == null || !axis.isArray) return false;
                
                // Check if any Shababeek axes exist
                for (int i = 0; i < axis.arraySize; i++)
                {
                    var element = axis.GetArrayElementAtIndex(i);
                    var name = element.FindPropertyRelative("m_Name")?.stringValue;
                    
                    if (!string.IsNullOrEmpty(name) && name.StartsWith("Shababeek_"))
                    {
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not check for existing axes: {e.Message}");
            }
            
            return false;
        }

        private bool ValidateInputAxesCreation()
        {
            try
            {
                // Check if any Shababeek axes exist
                var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
                var serializedObject = new SerializedObject(inputManagerAsset);
                var axis = serializedObject.FindProperty("m_Axes");
                
                if (axis == null || !axis.isArray) return false;
                
                bool foundShababeekAxes = false;
                for (int i = 0; i < axis.arraySize; i++)
                {
                    var element = axis.GetArrayElementAtIndex(i);
                    var name = element.FindPropertyRelative("m_Name")?.stringValue;
                    
                    if (!string.IsNullOrEmpty(name) && name.StartsWith("Shababeek_"))
                    {
                        foundShababeekAxes = true;
                        break;
                    }
                }
                
                if (!foundShababeekAxes)
                {
                    Debug.LogError("Input axes validation failed: No Shababeek axes found in InputManager");
                    return false;
                }
                
                Debug.Log("Input axes validation passed successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Input axes validation error: {e.Message}");
                return false;
            }
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Can always proceed - the input method selection is automatically applied in OnStepExit
            // The user just needs to select their preferred input method
            return true;
        }

        private struct AxeData
        {
            public string Name;
            public string DescriptiveName;
            public float Dead;
            public int Axis;
            public int Type;
            public string PositiveButton;
            public float Gravity;
            public float Sensitivity;
            public string AltPositiveButton;

            public AxeData(string name, string descriptiveName, float dead, int axis, int type, string positiveButton, float gravity, float sensitivity, string altPositiveButton)
            {
                this.Name = name;
                this.DescriptiveName = descriptiveName;
                this.Dead = dead;
                this.Axis = axis;
                this.Type = type;
                this.PositiveButton = positiveButton;
                this.Gravity = gravity;
                this.Sensitivity = sensitivity;
                this.AltPositiveButton = altPositiveButton;
            }
           
        }
    }
}

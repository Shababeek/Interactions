using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Shababeek.Interactions.Animations;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Hand Setup step for the Shababeek Setup Wizard
    /// </summary>
    public class HandSetupStep : ISetupWizardStep
    {
        public string StepName => "Hand Setup";

        private bool _useExistingHands = true;
        private HandData _selectedHandData;
        private Vector2 _scrollPosition = Vector2.zero;
        private bool _isCreatingHandData = false;

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 5: Hand Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose how you want to set up your hands for interactions.", MessageType.Info);
            EditorGUILayout.Space();
            
            // Toggle for existing vs new hands
            _useExistingHands = EditorGUILayout.Toggle("Use Existing Hands", _useExistingHands);
            EditorGUILayout.Space();
            
            if (_useExistingHands)
            {
                DrawExistingHandsSelection(wizard);
            }
            else
            {
                DrawNewHandsCreation(wizard);
            }
        }

        private void DrawExistingHandsSelection(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Select from Existing Hands", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose from the available hand configurations in your project.", MessageType.Info);
            EditorGUILayout.Space();

            // Find all HandData assets
            string[] guids = AssetDatabase.FindAssets("t:HandData");
            List<HandData> handDataAssets = new List<HandData>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                HandData handData = AssetDatabase.LoadAssetAtPath<HandData>(path);
                if (handData != null)
                    handDataAssets.Add(handData);
            }

            if (handDataAssets.Count == 0)
            {
                EditorGUILayout.HelpBox("No HandData assets found in the project. Please create one first.", MessageType.Warning);
                if (GUILayout.Button("Create HandData Asset"))
                {
                    CreateHandDataAsset(wizard);
                }
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            foreach (HandData handData in handDataAssets)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                // Draw hand preview
                DrawHandPreview(handData);
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(handData.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Number of Poses: {handData.Poses.Length}", EditorStyles.miniLabel);
                
                if (!string.IsNullOrEmpty(handData.Description))
                {
                    EditorGUILayout.LabelField($"Description: {handData.Description}", EditorStyles.miniLabel);
                }
                
                // Show selection status
                if (_selectedHandData == handData)
                {
                    EditorGUILayout.LabelField("✓ Selected", EditorStyles.boldLabel);
                }
                
                if (GUILayout.Button(_selectedHandData == handData ? "Currently Selected" : "Select This Hand"))
                {
                    _selectedHandData = handData;
                    if (wizard.ConfigAsset != null)
                    {
                        var serializedConfig = new SerializedObject(wizard.ConfigAsset);
                        var handDataProperty = serializedConfig.FindProperty("handData");
                        if (handDataProperty != null)
                        {
                            handDataProperty.objectReferenceValue = handData;
                            serializedConfig.ApplyModifiedProperties();
                            EditorUtility.SetDirty(wizard.ConfigAsset);
                            AssetDatabase.SaveAssets();
                            
                            Debug.Log($"Selected hand data: {handData.name}");
                        }
                    }
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndScrollView();
            
            // Show current selection info
            if (_selectedHandData != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox($"Selected Hand: {_selectedHandData.name}\nPoses: {_selectedHandData.Poses.Length}\nDescription: {_selectedHandData.Description}", MessageType.Info);
            }
        }

        private void DrawHandPreview(HandData handData)
        {
            if (handData == null) return;
            
            if (handData.previewSprite != null)
            {
                // Get the sprite's texture
                Texture2D previewTexture = handData.previewSprite;
                if (previewTexture != null)
                {
                    float aspectRatio = (float)previewTexture.width / previewTexture.height;
                    float previewHeight = 80f;
                    float previewWidth = previewHeight * aspectRatio;
                    
                    // Clamp width to reasonable bounds
                    previewWidth = Mathf.Clamp(previewWidth, 60f, 120f);
                    
                    Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
                    
                    // Draw border
                    EditorGUI.DrawRect(new Rect(previewRect.x - 1, previewRect.y - 1, previewRect.width + 2, previewRect.height + 2), Color.black);
                    
                    // Draw the texture
                    if (previewTexture != null)
                    {
                        // Use DrawPreviewTexture for better quality
                        EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
                    }
                }
                else
                {
                    DrawHandPreviewPlaceholder();
                }
            }
            else
            {
                DrawHandPreviewPlaceholder();
            }
        }

        private void DrawNewHandsCreation(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Create New Hands", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("You chose to create new hands. This will start a multi-step process to create custom hand configurations.", MessageType.Info);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("The following steps will be available:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Hand Model Selection");
            EditorGUILayout.LabelField("• Pose Creation");
            EditorGUILayout.LabelField("• Avatar Mask Setup");
            EditorGUILayout.LabelField("• Prefab Configuration");
            EditorGUILayout.Space();
            
            if (_isCreatingHandData)
            {
                EditorGUILayout.LabelField("Creating HandData asset...", EditorStyles.boldLabel);
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(0, 20), 0.5f, "Creating HandData...");
            }
            else
            {
                if (GUILayout.Button("Create New HandData Asset"))
                {
                    CreateHandDataAsset(wizard);
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Press Next to begin the hand creation process.", MessageType.Info);
        }

        private void CreateHandDataAsset(ShababeekSetupWizard wizard)
        {
            if (_isCreatingHandData) return;
            
            _isCreatingHandData = true;
            
            try
            {
                // Create the HandData asset
                wizard.CreateDefaultHandDataAsset();
                
                // Refresh the asset database
                AssetDatabase.Refresh();
                
                Debug.Log("HandData asset created successfully!");
                
                // Force a repaint to show the new asset
                if (EditorWindow.HasOpenInstances<ShababeekSetupWizard>())
                {
                    EditorWindow.GetWindow<ShababeekSetupWizard>().Repaint();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create HandData asset: {e.Message}");
                EditorUtility.DisplayDialog("Creation Failed", 
                    $"Failed to create HandData asset:\n{e.Message}\n\nPlease try again or create the asset manually.", 
                    "OK");
            }
            finally
            {
                _isCreatingHandData = false;
            }
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Initialize with existing selection if available
            if (wizard.ConfigAsset != null && wizard.ConfigAsset.HandData != null)
            {
                _selectedHandData = wizard.ConfigAsset.HandData;
                _useExistingHands = true;
            }
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            try
            {
                wizard.SelectedHandData = _selectedHandData;
                wizard.UseProvidedHands = _useExistingHands;
                
                // Validate hand setup if using existing hands
                if (_useExistingHands && _selectedHandData != null)
                {
                    if (!ValidateHandData(_selectedHandData))
                    {
                        throw new System.InvalidOperationException("Selected HandData asset is not properly configured. Please check the console for details.");
                    }
                }
                
                // TODO: Add logic for creating a new hand step
                // This would involve adding additional steps to the wizard for hand creation workflow
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to validate hand setup: {e.Message}");
                
                // Show error dialog with recovery options
                int choice = EditorUtility.DisplayDialogComplex("Hand Setup Failed", 
                    $"Failed to validate hand setup:\n{e.Message}\n\nWhat would you like to do?", 
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
                        Debug.LogWarning("Continuing with hand setup failure - some hand features may not work properly");
                        break;
                }
            }
        }

        private bool ValidateHandData(HandData handData)
        {
            try
            {
                if (handData == null)
                {
                    Debug.LogError("HandData validation failed: HandData is null");
                    return false;
                }
                
                // Check if HandData has poses
                if (handData.Poses == null || handData.Poses.Length == 0)
                {
                    Debug.LogError("HandData validation failed: No poses found");
                    return false;
                }
                
                
                // Check if HandData has avatar masks
                bool hasAvatarMasks = false;
                for (int i = 0; i < 5; i++) // Check first 5 fingers
                {
                    if (handData[i] != null)
                    {
                        hasAvatarMasks = true;
                        break;
                    }
                }
                
                if (!hasAvatarMasks)
                {
                    Debug.LogWarning("HandData validation warning: No avatar masks found - finger animations may not work properly");
                }
                
                Debug.Log($"HandData validation passed: {handData.name} with {handData.Poses.Length} poses");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"HandData validation error: {e.Message}");
                return false;
            }
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            if (_useExistingHands)
            {
                return _selectedHandData != null;
            }
            return true;
        }
        
        private void DrawHandPreviewPlaceholder()
        {
            Rect placeholderRect = GUILayoutUtility.GetRect(64, 64);
            EditorGUI.DrawRect(placeholderRect, new Color(0.8f, 0.8f, 0.8f, 0.3f));
            EditorGUI.DrawRect(new Rect(placeholderRect.x - 1, placeholderRect.y - 1, placeholderRect.width + 2, placeholderRect.height + 2), Color.black);
            
            GUIStyle handIconStyle = new GUIStyle(EditorStyles.label);
            handIconStyle.alignment = TextAnchor.MiddleCenter;
            handIconStyle.fontSize = 50;
            handIconStyle.normal.textColor = Color.black;
            
            GUI.Label(placeholderRect, "✋", handIconStyle);
        }
    }
}

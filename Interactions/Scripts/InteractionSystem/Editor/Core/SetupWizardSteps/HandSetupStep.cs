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

        private bool useExistingHands = true;
        private HandData selectedHandData;
        private Vector2 scrollPosition = Vector2.zero;

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 5: Hand Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Choose how you want to set up your hands for interactions.", MessageType.Info);
            EditorGUILayout.Space();
            
            // Toggle for existing vs new hands
            useExistingHands = EditorGUILayout.Toggle("Use Existing Hands", useExistingHands);
            EditorGUILayout.Space();
            
            if (useExistingHands)
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
                    wizard.CreateDefaultHandDataAsset();
                }
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (HandData handData in handDataAssets)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                // Preview image - display the actual sprite for visualization
                if (handData.previewSprite != null)
                {
                    // Create a preview texture from the sprite
                    Texture2D previewTexture = handData.previewSprite.texture;
                    if (previewTexture != null)
                    {
                        // Calculate aspect ratio to maintain proportions
                        float aspectRatio = (float)previewTexture.width / previewTexture.height;
                        float previewHeight = 80f;
                        float previewWidth = previewHeight * aspectRatio;
                        
                        // Clamp width to reasonable bounds
                        previewWidth = Mathf.Clamp(previewWidth, 60f, 120f);
                        
                        // Draw the sprite preview
                        Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
                        GUI.DrawTextureWithTexCoords(previewRect, previewTexture, handData.previewSprite.rect);
                        
                        // Add a subtle border
                        EditorGUI.DrawRect(new Rect(previewRect.x - 1, previewRect.y - 1, previewRect.width + 2, previewRect.height + 2), Color.gray);
                    }
                    else
                    {
                        DrawHandPreviewPlaceholder();
                    }
                }
                else
                {
                    // Try to get preview from hand prefabs if no sprite is set
                    Texture2D prefabPreview = GetHandPrefabPreview(handData);
                    if (prefabPreview != null)
                    {
                        // Draw the prefab preview
                        Rect previewRect = GUILayoutUtility.GetRect(64, 64);
                        GUI.DrawTexture(previewRect, prefabPreview);
                        
                        // Add a subtle border
                        EditorGUI.DrawRect(new Rect(previewRect.x - 1, previewRect.y - 1, previewRect.width + 2, previewRect.height + 2), Color.gray);
                    }
                    else
                    {
                        DrawHandPreviewPlaceholder();
                    }
                }
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(handData.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Poses: {handData.Poses.Length}", EditorStyles.miniLabel);
                
                // Show additional hand info if available
                if (handData.LeftHandPrefab != null)
                {
                    EditorGUILayout.LabelField($"Left Hand: {handData.LeftHandPrefab.name}", EditorStyles.miniLabel);
                }
                if (handData.RightHandPrefab != null)
                {
                    EditorGUILayout.LabelField($"Right Hand: {handData.RightHandPrefab.name}", EditorStyles.miniLabel);
                }
                
                if (GUILayout.Button("Select This Hand"))
                {
                    selectedHandData = handData;
                    // Link this hand data to the config
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
                        }
                    }
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndScrollView();
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
            
            EditorGUILayout.HelpBox("Press Next to begin the hand creation process.", MessageType.Info);
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Initialize with existing selection if available
            if (wizard.ConfigAsset != null && wizard.ConfigAsset.HandData != null)
            {
                selectedHandData = wizard.ConfigAsset.HandData;
                useExistingHands = true;
            }
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            // Update the wizard's hand selection
            wizard.SelectedHandData = selectedHandData;
            wizard.UseProvidedHands = useExistingHands;
            
            // If creating new hands, we'll need to add additional steps
            // This will be handled by the main wizard flow
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Can proceed if using existing hands and one is selected, or if creating new hands
            if (useExistingHands)
            {
                return selectedHandData != null;
            }
            return true;
        }
        
        /// <summary>
        /// Draws a placeholder preview when no sprite or prefab preview is available
        /// </summary>
        private void DrawHandPreviewPlaceholder()
        {
            Rect placeholderRect = GUILayoutUtility.GetRect(64, 64);
            EditorGUI.DrawRect(placeholderRect, new Color(0.8f, 0.8f, 0.8f, 0.3f));
            
            // Draw a simple hand icon
            GUIStyle handIconStyle = new GUIStyle(EditorStyles.label);
            handIconStyle.alignment = TextAnchor.MiddleCenter;
            handIconStyle.fontSize = 24;
            handIconStyle.normal.textColor = Color.gray;
            
            GUI.Label(placeholderRect, "✋", handIconStyle);
            
            // Add border
            EditorGUI.DrawRect(new Rect(placeholderRect.x - 1, placeholderRect.y - 1, placeholderRect.width + 2, placeholderRect.height + 2), Color.gray);
        }
        
        /// <summary>
        /// Attempts to get a preview texture from the hand prefabs
        /// </summary>
        private Texture2D GetHandPrefabPreview(HandData handData)
        {
            // Try to get preview from left hand prefab first
            if (handData.LeftHandPrefab != null)
            {
                var preview = AssetPreview.GetAssetPreview(handData.LeftHandPrefab);
                if (preview != null) return preview;
            }
            
            // Try right hand prefab if left doesn't have preview
            if (handData.RightHandPrefab != null)
            {
                var preview = AssetPreview.GetAssetPreview(handData.RightHandPrefab);
                if (preview != null) return preview;
            }
            
            // Try to get a preview from the prefab's renderer if available
            if (handData.LeftHandPrefab != null)
            {
                var renderer = handData.LeftHandPrefab.GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    var material = renderer.sharedMaterial;
                    if (material.mainTexture != null)
                    {
                        return material.mainTexture as Texture2D;
                    }
                }
            }
            
            return null;
        }
    }
}

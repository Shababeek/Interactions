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
                    wizard.CreateDefaultHandDataAsset();
                }
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            foreach (HandData handData in handDataAssets)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                if ( handData && handData.previewSprite)
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
                        EditorGUI.DrawRect(new Rect(previewRect.x - 1, previewRect.y - 1, previewRect.width + 2, previewRect.height + 2), Color.black);
                        if (previewTexture != null)
                        {
                            GUI.DrawTexture(previewRect, previewTexture);
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
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(handData.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"number of Poses: {handData.Poses.Length}", EditorStyles.miniLabel);
                
                {
                    EditorGUILayout.LabelField($"Description: {handData.Description}", EditorStyles.miniLabel);
                }
                
                if (GUILayout.Button("Select This Hand"))
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
                _selectedHandData = wizard.ConfigAsset.HandData;
                _useExistingHands = true;
            }
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            wizard.SelectedHandData = _selectedHandData;
            wizard.UseProvidedHands = _useExistingHands;
            //TODO: add logic for creating a new hand step
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

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Shababeek.Interactions.Animations;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// HandData step for the Shababeek Setup Wizard
    /// </summary>
    public class HandDataStep : ISetupWizardStep
    {
        public string StepName => "HandData";

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 2: HandData", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create or select a HandData asset, and assign required poses and masks.", MessageType.Info);
            EditorGUILayout.Space();
            
            if (wizard.UseProvidedHands)
            {
                DrawProvidedHandsSelection(wizard);
            }
            else
            {
                EditorGUILayout.HelpBox("You chose to create your own hands. Please create HandData assets manually.", MessageType.Info);
                if (GUILayout.Button("Create New HandData Asset"))
                {
                    wizard.CreateDefaultHandDataAsset();
                }
            }
        }

        private void DrawProvidedHandsSelection(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Select a Hand", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("Choose from the available hand configurations:", MessageType.Info);
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

            var scrollPosition = wizard.ScrollPosition;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (HandData handData in handDataAssets)
            {
                EditorGUILayout.BeginHorizontal("box");
                
                // Preview image
                if (handData.previewSprite != null)
                {
                    EditorGUILayout.ObjectField(handData.previewSprite, typeof(Sprite), false);
                }
                else
                {
                    GUILayout.Label("No Preview", GUILayout.Width(64), GUILayout.Height(64));
                }
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(handData.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Poses: {handData.Poses.Length}", EditorStyles.miniLabel);
                
                if (GUILayout.Button("Select This Hand"))
                {
                    wizard.SelectedHandData = handData;
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
            wizard.ScrollPosition = scrollPosition;
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
            // Can proceed if using provided hands and one is selected, or if making custom hands
            if (wizard.UseProvidedHands)
            {
                return wizard.SelectedHandData != null;
            }
            return true;
        }
    }
}

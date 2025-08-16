using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Shababeek.Interactions.Animations;
using Shababeek.Interactions.Core;
using System.IO;
using UnityEditor.Callbacks;
using UnityEngine.Serialization;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Asset postprocessor to detect when Shababeek assets are imported
    /// </summary>
    public class ShababeekAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // Check if any Shababeek assets were imported
            bool hasShababeekAssets = false;
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.StartsWith("Assets/Shababeek/"))
                {
                    hasShababeekAssets = true;
                    break;
                }
            }

            if (hasShababeekAssets && !EditorPrefs.GetBool(ShababeekSetupWizard.SetupWizardShownKey, false))
            {
                // Check if this is a fresh import by looking for existing config
                bool hasExistingConfig = false;
                string[] configGuids = AssetDatabase.FindAssets("t:Shababeek.Interactions.Core.Config");
                if (configGuids.Length > 0)
                {
                    hasExistingConfig = true;
                }

                // Only show for fresh imports, not when adding to existing projects
                if (!hasExistingConfig)
                {
                    // Delay the call to ensure Unity is fully loaded
                    EditorApplication.delayCall += () =>
                    {
                        if (!Application.isPlaying && !ShababeekSetupWizard.HasOpenInstances())
                        {
                            ShababeekSetupWizard.ShowWindow();
                            EditorPrefs.SetBool(ShababeekSetupWizard.SetupWizardShownKey, true);
                        }
                    };
                }
            }
        }
    }

    public class ShababeekSetupWizard : EditorWindow
    {
        private int _step = 0;
        private bool _useProvidedHands = false;
        private HandData _selectedHandData;

        // Configuration variables
        private Config _configAsset;

        public bool useDefaultSettings;
        // Editor prefs key to track if setup has been shown
        public const string SetupWizardShownKey = "Shababeek_SetupWizard_Shown";
        
        // Step management
        private List<ISetupWizardStep> _steps;
        private ISetupWizardStep CurrentStep => _steps != null && _step < _steps.Count ? _steps[_step] : null;

        [MenuItem("Tools/Shababeek/Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShababeekSetupWizard>(true, "Shababeek Setup Wizard");
            window.minSize = new Vector2(500, 400);
        }

        [MenuItem("Tools/Shababeek/Reset Setup Wizard")]
        public static void ResetSetupWizard()
        {
            EditorPrefs.DeleteKey(SetupWizardShownKey);
            Debug.Log("Shababeek Setup Wizard reset. It will show again on next import.");
        }

        // Properties for step access
        public Config ConfigAsset => _configAsset;
     

        public bool UseProvidedHands
        {
            get => _useProvidedHands;
            set => _useProvidedHands = value;
        }

        public HandData SelectedHandData
        {
            get => _selectedHandData;
            set => _selectedHandData = value;
        }

        public Vector2 ScrollPosition { get; set; } = Vector2.zero;

        // Methods for step control
        public void NextStep()
        {
            if (_step < _steps.Count - 1)
            {
                _step++;
            }
        }

        public void CloseWizard()
        {
            Close();
        }

        /// <summary>
        /// Checks if there are any open instances of the setup wizard
        /// </summary>
        public static bool HasOpenInstances()
        {
            return HasOpenInstances<ShababeekSetupWizard>();
        }

        /// <summary>
        /// Automatically shows the setup wizard when Shababeek assets are first imported
        /// </summary>
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            // Check if this is the first time the setup wizard should be shown
            if (!EditorPrefs.GetBool(SetupWizardShownKey, false))
            {
                // Delay the call to ensure Unity is fully loaded
                EditorApplication.delayCall += () =>
                {
                    // Only show if we're not in play mode and the window isn't already open
                    if (!Application.isPlaying && !HasOpenInstances<ShababeekSetupWizard>())
                    {
                        ShowWindow();
                        EditorPrefs.SetBool(SetupWizardShownKey, true);
                    }
                };
            }
        }
        

        private void OnEnable()
        {
            // Try to find existing config asset
            FindOrCreateConfigAsset();
            
            // Initialize steps
            InitializeSteps();
        }

        private void InitializeSteps()
        {
            _steps = new List<ISetupWizardStep>
            {
                new WelcomeStep(),
                new SetupChoiceStep()
            };
            
            // Always add the detailed setup steps since defaults will exit early
            _steps.AddRange(new ISetupWizardStep[]
            {
                new LayersInputStep(),
                new InputMethodStep(),
                new DependenciesCheckStep(),
                new HandSetupStep()
            });
            
            _steps.Add(new FinishStep());
        }

        private void FindOrCreateConfigAsset()
        {
            // First try to find existing config in the project
            string[] guids = AssetDatabase.FindAssets("t:Shababeek.Interactions.Core.Config");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _configAsset = AssetDatabase.LoadAssetAtPath<Config>(path);
                return;
            }

            // If no config found, create one in the Shababeek folder
            CreateDefaultConfigAsset();
        }

        public void CreateDefaultConfigAsset()
        {
            // Create the config asset
            _configAsset = ScriptableObject.CreateInstance<Config>();
            
            // Set default input type
            var serializedConfig = new SerializedObject(_configAsset);
            var inputTypeProperty = serializedConfig.FindProperty("inputType");
            if (inputTypeProperty != null)
                inputTypeProperty.enumValueIndex = (int)Config.InputManagerType.InputSystem;
            serializedConfig.ApplyModifiedProperties();
            
            // Save the asset first (layers will be set later when ApplyDefaultSettings is called)
            string folderPath = "Assets/Shababeek/Interactions/Data";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
                
            string assetPath = $"{folderPath}/config.asset";
            AssetDatabase.CreateAsset(_configAsset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created default config asset at: {assetPath}");
        }

        public void CreateDefaultHandDataAsset()
        {
            HandData handData = ScriptableObject.CreateInstance<HandData>();
            
            string folderPath = "Assets/Shababeek/Interactions/Data";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
                
            string assetPath = $"{folderPath}/DefaultHandData.asset";
            AssetDatabase.CreateAsset(handData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created default HandData asset at: {assetPath}");
        }

        private void OnGUI()
        {
            if (CurrentStep == null)
            {
                EditorGUILayout.HelpBox("Steps not initialized properly.", MessageType.Error);
                return;
            }

            GUILayout.Label($"Step {_step + 1} of {_steps.Count}: {CurrentStep.StepName}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Draw the current step
            CurrentStep.DrawStep(this);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (_step > 0 && GUILayout.Button("Back")) 
            {
                CurrentStep.OnStepExit(this);
                _step--;
                CurrentStep.OnStepEnter(this);
                if (_step == 0) ResetChoices();
            }
            GUILayout.FlexibleSpace();
            if (_step < _steps.Count - 1 && GUILayout.Button("Next"))
            {
                if (CurrentStep.CanProceed(this))
                {
                    CurrentStep.OnStepExit(this);
                    _step++;
                    CurrentStep.OnStepEnter(this);
                }
                else
                {
                    EditorUtility.DisplayDialog("Cannot Proceed", "Please complete the current step before proceeding.", "OK");
                }
            }
            if (_step == _steps.Count - 1 && GUILayout.Button("Close")) CloseWizard();
            EditorGUILayout.EndHorizontal();
        }

        public void ResetChoices()
        {
            _useProvidedHands = false;
            _selectedHandData = null;
        }

    }
}

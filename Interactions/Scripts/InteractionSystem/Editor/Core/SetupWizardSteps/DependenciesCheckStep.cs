using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Dependencies Check step for the Shababeek Setup Wizard
    /// </summary>
    public class DependenciesCheckStep : ISetupWizardStep
    {
        public string StepName => "Dependencies Check";

        private bool openXRInstalled = false;
        private bool openXRInstalling = false;

        public void DrawStep(ShababeekSetupWizard wizard)
        {
            EditorGUILayout.LabelField("Step 4: Dependencies Check", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Checking for required dependencies and offering installation options.", MessageType.Info);
            EditorGUILayout.Space();
            
            CheckDependencies();
            
            EditorGUILayout.LabelField("Required Dependencies:", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // OpenXR Check
            EditorGUILayout.BeginHorizontal("box");
            if (openXRInstalled)
            {
                EditorGUILayout.LabelField("✓ OpenXR", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Installed", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("✗ OpenXR", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Not Installed", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            if (!openXRInstalled)
            {
                EditorGUILayout.HelpBox("OpenXR is recommended for VR/AR interactions but not required for basic functionality.", MessageType.Warning);
                
                if (openXRInstalling)
                {
                    EditorGUILayout.LabelField("Installing OpenXR...", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Please wait for the installation to complete.", EditorStyles.miniLabel);
                }
                else
                {
                    if (GUILayout.Button("Install OpenXR"))
                    {
                        InstallOpenXR();
                    }
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("You can continue without OpenXR, but some VR/AR features may not work properly.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("All required dependencies are installed!", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Press Next to continue with the setup.", MessageType.Info);
        }

        private void CheckDependencies()
        {
            // Check if OpenXR is installed
            openXRInstalled = CheckOpenXRInstalled();
        }

        private bool CheckOpenXRInstalled()
        {
            // Method 1: Check via Package Manager (safest)
            try
            {
                var openXRPackage = UnityEditor.PackageManager.PackageInfo.FindForPackageName("Unity.XR.OpenXR");
                if (openXRPackage != null)
                {
                    return true;
                }
            }
            catch (System.Exception)
            {
                // Package not found, continue to other methods
            }

            // Method 2: Check via reflection (safe)
            try
            {
                var openXRType = System.Type.GetType("UnityEngine.XR.OpenXR.OpenXRLoader, Unity.XR.OpenXR");
                if (openXRType != null)
                {
                    return true;
                }
            }
            catch (System.Exception)
            {
                // Type not found, continue to other methods
            }

            // Method 3: Check for OpenXR files in the project
            try
            {
                string[] openXRGuids = AssetDatabase.FindAssets("OpenXR");
                if (openXRGuids.Length > 0)
                {
                    // Check if any of these are actually OpenXR related
                    foreach (string guid in openXRGuids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (path.Contains("OpenXR") || path.Contains("openxr"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                // Asset search failed, continue
            }

            // Method 4: Check Package Manager manifest
            try
            {
                string manifestPath = "Packages/manifest.json";
                if (System.IO.File.Exists(manifestPath))
                {
                    string manifestContent = System.IO.File.ReadAllText(manifestPath);
                    if (manifestContent.Contains("com.unity.xr.openxr"))
                    {
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                // File read failed, continue
            }

            return false;
        }

        private void InstallOpenXR()
        {
            openXRInstalling = true;
            
            // TODO: Implement OpenXR installation via Package Manager
            
            Debug.Log("OpenXR installation requested - implementation pending");
            
            // For now, simulate installation
            EditorApplication.delayCall += () =>
            {
                openXRInstalling = false;
                openXRInstalled = true;
                Debug.Log("OpenXR installation completed (simulated)");
            };
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Check dependencies when entering the step
            CheckDependencies();
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            // Nothing special needed when exiting this step
            // Dependencies are checked but not automatically installed
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Can always proceed - dependencies are optional
            return true;
        }
    }
}

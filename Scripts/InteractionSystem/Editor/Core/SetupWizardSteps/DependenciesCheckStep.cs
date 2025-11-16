using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

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
        private AddRequest addRequest;
        private bool hasAddRequest = false;

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
                    
                    // Show progress bar
                    EditorGUI.ProgressBar(GUILayoutUtility.GetRect(0, 20), 0.5f, "Installing OpenXR Package...");
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
            if (hasAddRequest) return; // Prevent multiple requests
            
            openXRInstalling = true;
            hasAddRequest = true;
            
            // Add OpenXR package via Package Manager
            addRequest = Client.Add("com.unity.xr.openxr");
            
            Debug.Log("Starting OpenXR installation via Package Manager...");
            
            // Start monitoring the request
            EditorApplication.update += MonitorAddRequest;
        }

        private void MonitorAddRequest()
        {
            if (addRequest == null) return;
            
            if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("OpenXR package installed successfully!");
                    openXRInstalled = true;
                    openXRInstalling = false;
                    hasAddRequest = false;
                    
                    // Refresh the asset database to pick up new packages
                    AssetDatabase.Refresh();
                    
                    // Force a repaint of the window
                    if (EditorWindow.HasOpenInstances<ShababeekSetupWizard>())
                    {
                        EditorWindow.GetWindow<ShababeekSetupWizard>().Repaint();
                    }
                }
                else if (addRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"Failed to install OpenXR package: {addRequest.Error?.message}");
                    openXRInstalling = false;
                    hasAddRequest = false;
                    
                    // Show error dialog
                    EditorUtility.DisplayDialog("Installation Failed", 
                        $"Failed to install OpenXR package:\n{addRequest.Error?.message}\n\nYou can continue without OpenXR, but some VR/AR features may not work properly.", 
                        "OK");
                }
                
                // Clean up
                addRequest = null;
                EditorApplication.update -= MonitorAddRequest;
            }
        }

        public void OnStepEnter(ShababeekSetupWizard wizard)
        {
            // Check dependencies when entering the step
            CheckDependencies();
        }

        public void OnStepExit(ShababeekSetupWizard wizard)
        {
            // Clean up any ongoing requests
            if (hasAddRequest && addRequest != null)
            {
                EditorApplication.update -= MonitorAddRequest;
                hasAddRequest = false;
                addRequest = null;
            }
            
            // Validate dependencies if OpenXR was installed
            if (openXRInstalled)
            {
                if (!ValidateOpenXRInstallation())
                {
                    Debug.LogWarning("OpenXR installation validation failed - some VR/AR features may not work properly");
                }
            }
        }

        private bool ValidateOpenXRInstallation()
        {
            try
            {
                // Try to access OpenXR types to verify installation
                var openXRType = System.Type.GetType("UnityEngine.XR.OpenXR.OpenXRLoader, Unity.XR.OpenXR");
                if (openXRType == null)
                {
                    Debug.LogError("OpenXR validation failed: OpenXRLoader type not found");
                    return false;
                }
                
                // Check if the package is properly installed
                var openXRPackage = UnityEditor.PackageManager.PackageInfo.FindForPackageName("Unity.XR.OpenXR");
                if (openXRPackage == null)
                {
                    Debug.LogError("OpenXR validation failed: Package not found in Package Manager");
                    return false;
                }
                
                Debug.Log("OpenXR installation validation passed successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"OpenXR validation error: {e.Message}");
                return false;
            }
        }

        public bool CanProceed(ShababeekSetupWizard wizard)
        {
            // Can always proceed - dependencies are optional
            return true;
        }
    }
}

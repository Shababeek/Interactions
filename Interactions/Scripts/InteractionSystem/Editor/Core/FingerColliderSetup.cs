using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Shababeek.Editors
{
    public class FingerColliderSetup : EditorWindow
    {

        
        [Header("Capsule Settings")]
        [SerializeField] private float capsuleRadius = 0.008f;
        [SerializeField] private bool addColliders = true;
        [SerializeField] private bool addRenderers = true;
        

        
        private List<GameObject> createdCapsules = new List<GameObject>();
        private float lastCapsuleRadius = 0.008f;
        
        [MenuItem("Shababeek/Finger Collider Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<FingerColliderSetup>("Finger Collider Setup");
            window.minSize = new Vector2(300, 400);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Finger Collider Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select a finger root and press 'Create Capsules' to generate capsules at each bone in the hierarchy.", MessageType.Info);
            EditorGUILayout.Space();
            
            // Target Selection
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length > 0)
            {
                if (selectedObjects.Length == 1)
                {
                    EditorGUILayout.LabelField($"Selected: {selectedObjects[0].name}", EditorStyles.boldLabel);
                }
                else
                {
                    EditorGUILayout.LabelField($"Selected: {selectedObjects.Length} objects", EditorStyles.boldLabel);
                    for (int i = 0; i < Mathf.Min(selectedObjects.Length, 5); i++)
                    {
                        EditorGUILayout.LabelField($"  • {selectedObjects[i].name}");
                    }
                    if (selectedObjects.Length > 5)
                    {
                        EditorGUILayout.LabelField($"  ... and {selectedObjects.Length - 5} more");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select one or more finger roots in the hierarchy.", MessageType.Warning);
            }
            
            EditorGUILayout.Space();
            
            // Capsule Settings
            EditorGUILayout.LabelField("Capsule Settings", EditorStyles.boldLabel);
            capsuleRadius = EditorGUILayout.Slider("Capsule Radius", capsuleRadius, 0.001f, 0.05f);
            addColliders = EditorGUILayout.Toggle("Add Colliders", addColliders);
            addRenderers = EditorGUILayout.Toggle("Add Renderers", addRenderers);
            

            
            EditorGUILayout.Space();
            
            // Action Buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Capsules", GUILayout.Height(30)))
            {
                CreateCapsules();
            }
            
            if (GUILayout.Button("Clear All", GUILayout.Height(30)))
            {
                ClearAllCapsules();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Status
            if (createdCapsules.Count > 0)
            {
                EditorGUILayout.HelpBox($"Created {createdCapsules.Count} capsule(s)", MessageType.Info);
            }
            
            // Check for radius changes and update capsules in real-time
            if (lastCapsuleRadius != capsuleRadius && createdCapsules.Count > 0)
            {
                UpdateCapsuleRadius();
                lastCapsuleRadius = capsuleRadius;
            }
        }
        
        private void OnInspectorUpdate()
        {
            // Force repaint to update the UI
            Repaint();
        }
        private void CreateCapsules()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please select one or more finger roots first.", "OK");
                return;
            }
            
            ClearAllCapsules();
            
            int totalCapsulesCreated = 0;
            
            // Process each selected object
            foreach (var selectedObject in selectedObjects)
            {
                if (selectedObject == null) continue;
                
                // Get all transforms in the finger hierarchy
                var allTransforms = selectedObject.GetComponentsInChildren<Transform>();
                
                foreach (var transform in allTransforms)
                {
                    // Create capsule as a child of this bone (including root)
                    var capsule = CreateCapsuleAtBone(transform);
                    
                    if (capsule != null)
                    {
                        // Parent the capsule directly to the bone
                        capsule.transform.SetParent(transform);
                        
                        createdCapsules.Add(capsule);
                        totalCapsulesCreated++;
                    }
                }
            }
            
            Debug.Log($"Created {totalCapsulesCreated} capsule(s) for {selectedObjects.Length} finger(s)");
            
            // Select the first selected object
            if (selectedObjects.Length > 0)
            {
                Selection.activeGameObject = selectedObjects[0];
            }
        }
        
        private GameObject CreateCapsuleAtBone(Transform bone)
        {
            // Check if this bone has a child
            if (bone.childCount == 0)
            {
                Debug.LogWarning($"Bone {bone.name} has no children, skipping capsule creation.");
                return null;
            }
            
            // Get the child bone
            Transform childBone = bone.GetChild(0);
            
            // Create primitive capsule
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = $"{bone.name}_Capsule";
            
            // Calculate points and direction
            Vector3 p1 = bone.position;
            Vector3 p2 = childBone.position;
            Vector3 direction = (p2 - p1).normalized;
            float distance = Vector3.Distance(p1, p2);
            
            // Position at midpoint between p1 and p2
            capsule.transform.position = (p1 + p2) * 0.5f;
            
            // Set local rotation to zero
            capsule.transform.rotation = bone.rotation;
            
            // Scale: distance divided by 2 since capsule height is 2 units
            capsule.transform.localScale = new Vector3(
                capsuleRadius * 2, // Width (radius)
                distance * 0.5f,    // Height (distance / 2)
                capsuleRadius * 2  // Depth (radius)
            );
            
            // Configure components based on settings
            if (!addColliders)
            {
                DestroyImmediate(capsule.GetComponent<Collider>());
            }
            
            if (!addRenderers)
            {
                DestroyImmediate(capsule.GetComponent<Renderer>());
            }
            
            return capsule;
        }
        
        private void ClearAllCapsules()
        {
            foreach (var capsule in createdCapsules)
            {
                if (capsule != null)
                {
                    DestroyImmediate(capsule);
                }
            }
            
            createdCapsules.Clear();
            
            Debug.Log("Cleared all finger capsules.");
        }
        
        private void UpdateCapsuleRadius()
        {
            foreach (var capsule in createdCapsules)
            {
                if (capsule != null)
                {
                    // Update the width and depth (radius) while keeping the height the same
                    Vector3 currentScale = capsule.transform.localScale;
                    capsule.transform.localScale = new Vector3(
                        capsuleRadius * 2, // Width (radius)
                        currentScale.y,     // Keep current height
                        capsuleRadius * 2  // Depth (radius)
                    );
                }
            }
            
            Debug.Log($"Updated radius to {capsuleRadius} for {createdCapsules.Count} capsule(s)");
        }
    }
}

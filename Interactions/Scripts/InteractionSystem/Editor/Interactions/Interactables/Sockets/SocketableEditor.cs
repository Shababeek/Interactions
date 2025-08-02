using Shababeek.Interactions;
using UnityEngine;
using UnityEditor;
using Shababeek.Interactions;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Socketable))]
    [CanEditMultipleObjects]
    public class SocketableEditor : Editor
    {
        // Basic Settings
        private SerializedProperty shouldReturnToParentProp;
        private SerializedProperty useSmoothReturnProp;
        private SerializedProperty returnDurationProp;
        private SerializedProperty debugKeyProp;

        // Detection Settings
        private SerializedProperty maskProp;
        private SerializedProperty findBoundsAutomaticallyProp;
        private SerializedProperty boundsRendererProp;
        private SerializedProperty boundsProp;

        // Socket Settings
        private SerializedProperty rotationWhenSocketedProp;
        private SerializedProperty indicatorProp;

        // Events
        private SerializedProperty onSocketedProp;

        // Read-only fields (for display only)
        private SerializedProperty socketProp;
        private SerializedProperty isSocketedProp;

        private Socketable socketable;

        private void OnEnable()
        {
            socketable = (Socketable)target;

            // Basic Settings
            shouldReturnToParentProp = serializedObject.FindProperty("shouldReturnToParent");
            useSmoothReturnProp = serializedObject.FindProperty("useSmoothReturn");
            returnDurationProp = serializedObject.FindProperty("returnDuration");
            debugKeyProp = serializedObject.FindProperty("debugKey");

            // Detection Settings
            maskProp = serializedObject.FindProperty("mask");
            findBoundsAutomaticallyProp = serializedObject.FindProperty("findBoundsAutomatically");
            boundsRendererProp = serializedObject.FindProperty("boundsRenderer");
            boundsProp = serializedObject.FindProperty("bounds");

            // Socket Settings
            rotationWhenSocketedProp = serializedObject.FindProperty("rotationWhenSocketed");
            indicatorProp = serializedObject.FindProperty("indicator");

            // Events
            onSocketedProp = serializedObject.FindProperty("onSocketed");

            // Read-only fields
            socketProp = serializedObject.FindProperty("socket");
            isSocketedProp = serializedObject.FindProperty("isSocketed");
        }

                public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Socketable Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Validation and Help (moved to beginning)
            DrawValidationSection();
            
            EditorGUILayout.Space();
            
            // Basic Settings Section
            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(shouldReturnToParentProp, new GUIContent("Return to Parent", "If true, the object will return to its original position when unsocketed"));
            
            if (shouldReturnToParentProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(useSmoothReturnProp, new GUIContent("Use Smooth Return", "If true, the object will smoothly animate back to its original position"));
                
                if (useSmoothReturnProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(returnDurationProp, new GUIContent("Return Duration", "Duration of the smooth return animation in seconds"));
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Detection Settings Section
            EditorGUILayout.LabelField("Detection Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(maskProp, new GUIContent("Socket Layer Mask", "Layers to detect sockets on"));
            EditorGUILayout.PropertyField(findBoundsAutomaticallyProp, new GUIContent("Auto Find Bounds", "Automatically find bounds from renderer"));

            if (!findBoundsAutomaticallyProp.boolValue)
            {
                EditorGUILayout.PropertyField(boundsRendererProp, new GUIContent("Bounds Renderer", "Renderer to use for bounds calculation"));
                EditorGUILayout.PropertyField(boundsProp, new GUIContent("Custom Bounds", "Custom bounds for socket detection"));
            }
            else
            {
                EditorGUILayout.PropertyField(boundsRendererProp, new GUIContent("Bounds Renderer", "Renderer to use for bounds calculation (optional override)"));
            }

            EditorGUILayout.Space();

            // Socket Settings Section
            EditorGUILayout.LabelField("Socket Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rotationWhenSocketedProp, new GUIContent("Rotation When Socketed", "Additional rotation to apply when socketed"));
            EditorGUILayout.PropertyField(indicatorProp, new GUIContent("Indicator", "Visual indicator to show socket position"));

            EditorGUILayout.Space();

            // Events Section
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onSocketedProp, new GUIContent("On Socketed", "Event triggered when object is socketed"));

            EditorGUILayout.Space();

            // Read-only Status Section
            EditorGUILayout.LabelField("Status (Read-only)", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(socketProp, new GUIContent("Current Socket", "Currently detected socket"));
            EditorGUILayout.PropertyField(isSocketedProp, new GUIContent("Is Socketed", "Whether the object is currently socketed"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

                        // Debug and Test Section
            DrawDebugSection();
            
            EditorGUILayout.Space();
            
            // Debug Key Section (at the end)
            EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(debugKeyProp, new GUIContent("Debug Key", "Keyboard key to manually trigger socket/unsocket for debugging"));
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidationSection()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            // Check for required components
            var interactableBase = socketable.GetComponent<InteractableBase>();
            if (interactableBase == null)
            {
                EditorGUILayout.HelpBox("Socketable requires an InteractableBase component!", MessageType.Error);
            }

            // Check for indicator
            if (indicatorProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Indicator is not assigned. Socket position feedback will not work.", MessageType.Warning);
            }

            // Check for bounds renderer if auto-find is enabled
            if (findBoundsAutomaticallyProp.boolValue)
            {
                var renderer = socketable.GetComponentInChildren<Renderer>();
                if (renderer == null)
                {
                    EditorGUILayout.HelpBox("No renderer found in children. Bounds detection may not work properly.", MessageType.Warning);
                }
            }

            // Check layer mask
            if (maskProp.intValue == 0)
            {
                EditorGUILayout.HelpBox("Layer mask is set to 'Nothing'. No sockets will be detected.", MessageType.Warning);
            }
        }

        private void DrawDebugSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug & Testing", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test Socket Detection", GUILayout.Height(25)))
            {
                // This would trigger a manual socket detection test
                Debug.Log($"Testing socket detection for {socketable.name}");
                // You could add actual test logic here
            }

            if (GUILayout.Button("Force Return", GUILayout.Height(25)))
            {
                // Force the object to return to its original position
                if (Application.isPlaying)
                {
                    socketable.ForceReturn();
                    Debug.Log($"Forced immediate return for {socketable.name}");
                }
                else
                {
                    Debug.Log("Force Return can only be used in Play mode");
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Smooth Return", GUILayout.Height(25)))
            {
                // Force smooth return to original position
                if (Application.isPlaying)
                {
                    socketable.ForceReturnWithTween();
                    Debug.Log($"Forced smooth return for {socketable.name}");
                }
                else
                {
                    Debug.Log("Smooth Return can only be used in Play mode");
                }
            }

            if (GUILayout.Button("Reset Bounds", GUILayout.Height(25)))
            {
                // Reset bounds to automatic calculation
                if (socketable.BondsRenderer != null)
                {
                    var bounds = socketable.BondsRenderer.bounds;
                    bounds.size *= 2;
                    socketable.Bounds = bounds;
                    EditorUtility.SetDirty(socketable);
                }
            }

            EditorGUILayout.EndHorizontal();

            // Show current socket info
            if (socketable.IsSocketed)
            {
                EditorGUILayout.HelpBox($"Currently socketed to: {(socketable.CurrentSocket != null ? socketable.CurrentSocket.name : "Unknown")}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Not currently socketed", MessageType.Info);
            }

            // Show return status
            if (Application.isPlaying)
            {
                var socketableComponent = socketable.GetComponent<Socketable>();
                if (socketableComponent != null && socketableComponent.IsReturning)
                {
                    EditorGUILayout.HelpBox("Currently returning to original position...", MessageType.Warning);
                }
            }
        }

        private void OnSceneGUI()
        {
            var bounds = socketable.Bounds;
            var center = bounds.center;
            var size = bounds.size;

            // Draw bounds in scene view
            Handles.color = Color.green;
            Handles.DrawWireCube(center, size);

            // Draw socket detection range
            if (socketable.IsSocketed)
            {
                Handles.color = Color.red;
                Handles.DrawWireCube(center, size * 1.2f);
            }

            // Draw rotation indicator when socketed
            if (socketable.IsSocketed)
            {
                Handles.color = Color.blue;
                var rotation = socketable.RotationWhenSocketed;
                var forward = Quaternion.Euler(rotation) * Vector3.forward;
                Handles.ArrowHandleCap(0, center, Quaternion.LookRotation(forward), 0.5f, EventType.Repaint);
            }

            // Draw socket key info
            if (Event.current.type == EventType.Repaint)
            {
                var style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.white;
                style.fontSize = 10;

                var worldPos = socketable.transform.position;
                var screenPos = Camera.current.WorldToScreenPoint(worldPos);

                if (screenPos.z > 0)
                {
                    var rect = new Rect(screenPos.x - 50, Screen.height - screenPos.y - 20, 100, 20);
                    Handles.BeginGUI();
                    GUI.Label(rect, $"Debug Key: {socketable.DebugKey}", style);
                    Handles.EndGUI();
                }
            }
        }
    }
}
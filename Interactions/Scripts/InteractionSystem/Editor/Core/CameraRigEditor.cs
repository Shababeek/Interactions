using System;
using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(CameraRig))]
    public class CameraRigEditor : Editor
    {
        private CameraRig rig;
        
        // Serialized Properties
        private SerializedProperty configProp;
        private SerializedProperty initializeHandsProp;
        private SerializedProperty trackingMethodProp;
        private SerializedProperty leftHandPivotProp;
        private SerializedProperty rightHandPivotProp;
        private SerializedProperty leftHandInteractorTypeProp;
        private SerializedProperty rightHandInteractorTypeProp;
        private SerializedProperty offsetObjectProp;
        private SerializedProperty xrCameraProp;
        private SerializedProperty cameraHeightProp;
        private SerializedProperty alignRigForwardOnTrackingProp;
        private SerializedProperty initializeLayersProp;
        private SerializedProperty customLayerAssignmentsProp;

        private void OnEnable()
        {
            rig = (CameraRig)target;
            
            // Find all serialized properties
            configProp = serializedObject.FindProperty("config");
            initializeHandsProp = serializedObject.FindProperty("initializeHands");
            trackingMethodProp = serializedObject.FindProperty("trackingMethod");
            leftHandPivotProp = serializedObject.FindProperty("leftHandPivot");
            rightHandPivotProp = serializedObject.FindProperty("rightHandPivot");
            leftHandInteractorTypeProp = serializedObject.FindProperty("leftHandInteractorType");
            rightHandInteractorTypeProp = serializedObject.FindProperty("rightHandInteractorType");
            offsetObjectProp = serializedObject.FindProperty("offsetObject");
            xrCameraProp = serializedObject.FindProperty("xrCamera");
            cameraHeightProp = serializedObject.FindProperty("cameraHeight");
            alignRigForwardOnTrackingProp = serializedObject.FindProperty("alignRigForwardOnTracking");
            initializeLayersProp = serializedObject.FindProperty("initializeLayers");
            customLayerAssignmentsProp = serializedObject.FindProperty("customLayerAssignments");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Camera Rig Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Validation Section
            DrawValidationSection();
            
            EditorGUILayout.Space();
            
            // Configuration Section
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(configProp, new GUIContent("Config", "Configuration asset containing hand prefabs and settings"));
            
            if (configProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Please assign a Config asset to continue setup.", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            
            EditorGUILayout.Space();
            
            // Hand Initialization Section
            EditorGUILayout.LabelField("Hand Initialization", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initializeHandsProp, new GUIContent("Initialize Hands", "If true, hands will be automatically created and initialized"));
            
            if (initializeHandsProp.boolValue)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(trackingMethodProp, new GUIContent("Tracking Method", "Method used for hand tracking - Transform or Physics based"));
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Hand Pivots", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(leftHandPivotProp, new GUIContent("Left Hand Pivot", "Transform that defines the left hand's position and rotation"));
                EditorGUILayout.PropertyField(rightHandPivotProp, new GUIContent("Right Hand Pivot", "Transform that defines the right hand's position and rotation"));
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Interactor Types", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(leftHandInteractorTypeProp, new GUIContent("Left Hand Interactor", "Type of interaction system for the left hand"));
                EditorGUILayout.PropertyField(rightHandInteractorTypeProp, new GUIContent("Right Hand Interactor", "Type of interaction system for the right hand"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // XR Camera Section
            EditorGUILayout.LabelField("XR Camera Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(offsetObjectProp, new GUIContent("Offset Object", "Transform used to offset the camera position"));
            EditorGUILayout.PropertyField(xrCameraProp, new GUIContent("XR Camera", "The main camera used for XR rendering"));
            EditorGUILayout.PropertyField(cameraHeightProp, new GUIContent("Camera Height", "Default standing height for the camera (in meters)"));
            EditorGUILayout.PropertyField(alignRigForwardOnTrackingProp, new GUIContent("Align Rig Forward", "If true, aligns the rig's forward direction with tracking"));
            
            EditorGUILayout.Space();
            
            // Layer Management Section
            EditorGUILayout.LabelField("Layer Management", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initializeLayersProp, new GUIContent("Initialize Layers", "If true, automatically assigns layers to camera rig and hands"));
            
            if (initializeLayersProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customLayerAssignmentsProp, new GUIContent("Custom Layer Assignments", "Additional layer assignments for specific objects. Each assignment allows you to set a target transform and assign it to a specific layer."));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Debug Section
            DrawDebugSection();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawValidationSection()
        {
            bool hasWarnings = false;
            string warningMessage = "";
            
            if (configProp.objectReferenceValue == null)
            {
                hasWarnings = true;
                warningMessage += "• Config asset is not assigned\n";
            }
            
            if (initializeHandsProp.boolValue)
            {
                if (leftHandPivotProp.objectReferenceValue == null)
                {
                    hasWarnings = true;
                    warningMessage += "• Left Hand Pivot is not assigned\n";
                }
                
                if (rightHandPivotProp.objectReferenceValue == null)
                {
                    hasWarnings = true;
                    warningMessage += "• Right Hand Pivot is not assigned\n";
                }
            }
            
            if (offsetObjectProp.objectReferenceValue == null)
            {
                hasWarnings = true;
                warningMessage += "• Offset Object is not assigned\n";
            }
            
            if (xrCameraProp.objectReferenceValue == null)
            {
                hasWarnings = true;
                warningMessage += "• XR Camera is not assigned\n";
            }
            
            if (hasWarnings)
            {
                EditorGUILayout.HelpBox("Setup Issues:\n" + warningMessage, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("Camera Rig is properly configured!", MessageType.Info);
            }
        }
        
        private void DrawDebugSection()
        {
            EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
            
            if (rig.Config != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Config Information:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Left Hand Prefab: {(rig.LeftHandPrefab != null ? rig.LeftHandPrefab.name : "None")}");
                EditorGUILayout.LabelField($"Right Hand Prefab: {(rig.RightHandPrefab != null ? rig.RightHandPrefab.name : "None")}");
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Current Settings:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Tracking Method: {trackingMethodProp.enumDisplayNames[trackingMethodProp.enumValueIndex]}");
            EditorGUILayout.LabelField($"Left Interactor: {leftHandInteractorTypeProp.enumDisplayNames[leftHandInteractorTypeProp.enumValueIndex]}");
            EditorGUILayout.LabelField($"Right Interactor: {rightHandInteractorTypeProp.enumDisplayNames[rightHandInteractorTypeProp.enumValueIndex]}");
            EditorGUILayout.LabelField($"Camera Height: {cameraHeightProp.floatValue}m");
            EditorGUILayout.LabelField($"Align Forward: {alignRigForwardOnTrackingProp.boolValue}");
            EditorGUILayout.EndVertical();
        }
        
        private void OnSceneGUI()
        {
            if (rig == null) return;
            
            // Draw camera height indicator
            if (offsetObjectProp.objectReferenceValue != null)
            {
                Vector3 worldPos = rig.Offset.position;
                
                Handles.color = Color.cyan;
                Handles.DrawWireCube(worldPos, Vector3.one * 0.1f);
                
                // Draw height line
                Vector3 groundPos = rig.transform.position;
                Handles.color = Color.yellow;
                Handles.DrawLine(groundPos, worldPos);
                
                // Draw label
                Handles.BeginGUI();
                Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);
                GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 20, 100, 20), $"Height: {cameraHeightProp.floatValue}m");
                Handles.EndGUI();
            }
            
            // Draw hand pivot indicators
            if (leftHandPivotProp.objectReferenceValue != null)
            {
                Transform leftPivot = (Transform)leftHandPivotProp.objectReferenceValue;
                Handles.color = Color.blue;
                Handles.DrawWireCube(leftPivot.position, Vector3.one * 0.05f);
                
                Handles.BeginGUI();
                Vector2 screenPos = HandleUtility.WorldToGUIPoint(leftPivot.position);
                GUI.Label(new Rect(screenPos.x - 30, screenPos.y - 15, 60, 15), "Left Hand");
                Handles.EndGUI();
            }
            
            if (rightHandPivotProp.objectReferenceValue != null)
            {
                Transform rightPivot = (Transform)rightHandPivotProp.objectReferenceValue;
                Handles.color = Color.red;
                Handles.DrawWireCube(rightPivot.position, Vector3.one * 0.05f);
                
                Handles.BeginGUI();
                Vector2 screenPos = HandleUtility.WorldToGUIPoint(rightPivot.position);
                GUI.Label(new Rect(screenPos.x - 30, screenPos.y - 15, 60, 15), "Right Hand");
                Handles.EndGUI();
            }
        }
    }
}
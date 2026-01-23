using UnityEditor;
using UnityEngine;
using Shababeek.Interactions.Core;
using System.Collections.Generic;
using System.Linq;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(Config))]
    public class ConfigEditor : Editor
    {
        // Serialized Properties
        private SerializedProperty _handDataProp;
        private SerializedProperty _leftHandLayerProp;
        private SerializedProperty _rightHandLayerProp;
        private SerializedProperty _interactableLayerProp;
        private SerializedProperty _playerLayerProp;
        private SerializedProperty _inputTypeProp;
        private SerializedProperty _leftHandActionsProp;
        private SerializedProperty _rightHandActionsProp;
        private SerializedProperty _oldInputSettingsProp;
        private SerializedProperty _feedbackSystemStyleSheetProp;
        private SerializedProperty _handMassProp;
        private SerializedProperty _linearDampingProp;
        private SerializedProperty _angularDampingProp;
        private SerializedProperty _followerPresetProp;
        private SerializedProperty _customFollowerSettingsProp;

        // Physics validation
        private bool _physicsLayersValid = true;
        private string _physicsValidationMessage = "";

        private void OnEnable()
        {

            // Find all serialized properties
            _handDataProp = serializedObject.FindProperty("handData");
            _leftHandLayerProp = serializedObject.FindProperty("leftHandLayer");
            _rightHandLayerProp = serializedObject.FindProperty("rightHandLayer");
            _interactableLayerProp = serializedObject.FindProperty("interactableLayer");
            _playerLayerProp = serializedObject.FindProperty("playerLayer");
            _inputTypeProp = serializedObject.FindProperty("inputType");
            _leftHandActionsProp = serializedObject.FindProperty("leftHandActions");
            _rightHandActionsProp = serializedObject.FindProperty("rightHandActions");
            _feedbackSystemStyleSheetProp = serializedObject.FindProperty("feedbackSystemStyleSheet");
            _handMassProp = serializedObject.FindProperty("handMass");
            _linearDampingProp = serializedObject.FindProperty("linearDamping");
            _angularDampingProp = serializedObject.FindProperty("angularDamping");
            _followerPresetProp = serializedObject.FindProperty("followerPreset");
            _customFollowerSettingsProp = serializedObject.FindProperty("customFollowerSettings");

            // Validate physics layer matrix
            ValidatePhysicsLayerMatrix();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Interaction System Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawPhysicsLayerValidationSection();
            EditorGUILayout.Space();
            
            DrawValidationSection();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Hand Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_handDataProp, new GUIContent("Hand Data"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Layer Configuration", EditorStyles.boldLabel);

            var newLeftHandLayer = EditorUtilities.LayerDropdown(
                new GUIContent("Left Hand Layer", "used for physics interactions"), _leftHandLayerProp.intValue);
            if (newLeftHandLayer != _leftHandLayerProp.intValue)
            {
                _leftHandLayerProp.intValue = newLeftHandLayer;
                ValidatePhysicsLayerMatrix(); 
            }

            var newRightHandLayer = EditorUtilities.LayerDropdown(
                new GUIContent("Right Hand Layer", "used for physics interactions"), _rightHandLayerProp.intValue);
            if (newRightHandLayer != _rightHandLayerProp.intValue)
            {
                _rightHandLayerProp.intValue = newRightHandLayer;
                ValidatePhysicsLayerMatrix(); // Re-validate when layers change
            }

            var newInteractableLayer = EditorUtilities.LayerDropdown(
                new GUIContent("Interactable Layer", "used for physics interactions"), _interactableLayerProp.intValue);
            if (newInteractableLayer != _interactableLayerProp.intValue)
            {
                _interactableLayerProp.intValue = newInteractableLayer;
                ValidatePhysicsLayerMatrix(); // Re-validate when layers change
            }

            int newPlayerLayer = EditorUtilities.LayerDropdown(
                new GUIContent("Player Layer", "Layer for the player"),
                _playerLayerProp.intValue);
            if (newPlayerLayer != _playerLayerProp.intValue)
            {
                _playerLayerProp.intValue = newPlayerLayer;
                ValidatePhysicsLayerMatrix(); // Re-validate when layers change
            }

            EditorGUILayout.Space();

            // Input Manager Section
            EditorGUILayout.LabelField("Input Manager Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_inputTypeProp,
                new GUIContent("Input Type", "Type of input manager to use for the interaction system"));

#if !XR_HANDS_AVAILABLE
            EditorGUILayout.HelpBox(
                "To enable Hand Tracking, install the Unity XR Hands package and add 'XR_HANDS_AVAILABLE' to your Scripting Define Symbols (Edit > Project Settings > Player > Other Settings > Scripting Define Symbols).",
                MessageType.Info);
#endif

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_leftHandActionsProp,
                new GUIContent("Left Hand Actions", "Input actions for the left hand"));
            EditorGUILayout.PropertyField(_rightHandActionsProp,
                new GUIContent("Right Hand Actions", "Input actions for the right hand"));
            EditorGUI.indentLevel--;


            EditorGUILayout.Space();

            // Editor UI Settings Section
            EditorGUILayout.LabelField("Editor UI Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_feedbackSystemStyleSheetProp,
                new GUIContent("Feedback System Style Sheet", "Style sheet for the feedback system UI"));

            EditorGUILayout.Space();

            // Hand Physics Section
            EditorGUILayout.LabelField("Hand Physics Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_handMassProp,
                new GUIContent("Hand Mass", "Mass of the hand for physics calculations"));
            EditorGUILayout.PropertyField(_linearDampingProp,
                new GUIContent("Linear Damping", "Linear damping for hand physics"));
            EditorGUILayout.PropertyField(_angularDampingProp,
                new GUIContent("Angular Damping", "Angular damping for hand physics"));

            EditorGUILayout.Space();

            // Hand Following Settings Section
            DrawHandFollowingSection();

            EditorGUILayout.Space();

            // Debug Section
            DrawDebugSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPhysicsLayerValidationSection()
        {
            if (!_physicsLayersValid)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("⚠️ Physics Layer Matrix Issues", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(_physicsValidationMessage, MessageType.Warning);

                if (GUILayout.Button("Apply Physics Settings", GUILayout.Height(25)))
                {
                    ApplyPhysicsSettings();
                    ValidatePhysicsLayerMatrix(); // Re-validate after applying
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        private string GetPropertyPath(string label)
        {
            // Map display labels to property paths
            switch (label)
            {
                case "Left Trigger Axis": return "leftTriggerAxis";
                case "Left Grip Axis": return "leftGripAxis";
                case "Left Primary Button": return "leftPrimaryButton";
                case "Left Secondary Button": return "leftSecondaryButton";
                case "Left Grip Debug Key": return "leftGripDebugKey";
                case "Left Trigger Debug Key": return "leftTriggerDebugKey";
                case "Left Thumb Debug Key": return "leftThumbDebugKey";
                case "Right Trigger Axis": return "rightTriggerAxis";
                case "Right Grip Axis": return "rightGripAxis";
                case "Right Primary Button": return "rightPrimaryButton";
                case "Right Secondary Button": return "rightSecondaryButton";
                case "Right Grip Debug Key": return "rightGripDebugKey";
                case "Right Trigger Debug Key": return "rightTriggerDebugKey";
                case "Right Thumb Debug Key": return "rightThumbDebugKey";
                default: return "";
            }
        }


        private void ValidatePhysicsLayerMatrix()
        {
            _physicsLayersValid = true;
            _physicsValidationMessage = "";

            var leftLayer = _leftHandLayerProp.intValue;
            var rightLayer = _rightHandLayerProp.intValue;
            var playerLayer = _playerLayerProp.intValue;

            // Check if layers are valid
            if (leftLayer < 0 || rightLayer < 0 || playerLayer < 0)
            {
                _physicsLayersValid = false;
                _physicsValidationMessage = "One or more layers are not set. Please assign valid layers.";
                return;
            }

            // Check for invalid collisions
            var issues = new List<string>();

            // Check if left hand can collide with itself
            if (Physics.GetIgnoreLayerCollision(leftLayer, leftLayer) == false)
            {
                issues.Add("Left hand layer can collide with itself");
            }

            // Check if right hand can collide with itself
            if (Physics.GetIgnoreLayerCollision(rightLayer, rightLayer) == false)
            {
                issues.Add("Right hand layer can collide with itself");
            }

            // Check if hands can collide with each other
            if (Physics.GetIgnoreLayerCollision(leftLayer, rightLayer) == false)
            {
                issues.Add("Left and right hand layers can collide with each other");
            }

            // Check if player can collide with hands
            if (Physics.GetIgnoreLayerCollision(playerLayer, leftLayer) == false)
            {
                issues.Add("Player layer can collide with left hand layer");
            }

            if (Physics.GetIgnoreLayerCollision(playerLayer, rightLayer) == false)
            {
                issues.Add("Player layer can collide with right hand layer");
            }

            // Check if player can collide with itself
            if (Physics.GetIgnoreLayerCollision(playerLayer, playerLayer) == false)
            {
                issues.Add("Player layer can collide with itself");
            }

            if (issues.Count > 0)
            {
                _physicsLayersValid = false;
                _physicsValidationMessage = "Physics layer collision issues detected:\n" + string.Join("\n", issues);
            }
        }

        private void ApplyPhysicsSettings()
        {
            var leftLayer = _leftHandLayerProp.intValue;
            var rightLayer = _rightHandLayerProp.intValue;
            var playerLayer = _playerLayerProp.intValue;

            if (leftLayer < 0 || rightLayer < 0 || playerLayer < 0)
            {
                Debug.LogError("Cannot apply physics settings: One or more layers are not set");
                return;
            }

            // Apply the same physics settings as in InteractionSystemLoader
            Physics.IgnoreLayerCollision(leftLayer, leftLayer);
            Physics.IgnoreLayerCollision(rightLayer, rightLayer);
            Physics.IgnoreLayerCollision(rightLayer, leftLayer);
            Physics.IgnoreLayerCollision(playerLayer, leftLayer);
            Physics.IgnoreLayerCollision(playerLayer, rightLayer);
            Physics.IgnoreLayerCollision(playerLayer, playerLayer);

            Debug.Log("Physics layer collision settings applied successfully");
        }

        private void DrawValidationSection()
        {
            bool hasWarnings = false;
            string warningMessage = "";

            if (_handDataProp.objectReferenceValue == null)
            {
                hasWarnings = true;
                warningMessage += "• Hand Data is not assigned\n";
            }

            if (_leftHandLayerProp.intValue == _rightHandLayerProp.intValue && _leftHandLayerProp.intValue != 0)
            {
                hasWarnings = true;
                warningMessage += "• Left and Right Hand layers should be different\n";
            }

            if (_interactableLayerProp.intValue == _leftHandLayerProp.intValue ||
                _interactableLayerProp.intValue == _rightHandLayerProp.intValue)
            {
                hasWarnings = true;
                warningMessage += "• Interactable layer should be different from hand layers\n";
            }

            if (hasWarnings)
            {
                EditorGUILayout.HelpBox("Configuration Issues:\n" + warningMessage, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("Configuration is properly set up!", MessageType.Info);
            }
        }

        private void DrawDebugSection()
        {
            EditorGUILayout.LabelField("Current Configuration", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Layer Information:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Left Hand: {EditorUtilities.GetLayerName(_leftHandLayerProp.intValue)}");
            EditorGUILayout.LabelField($"Right Hand: {EditorUtilities.GetLayerName(_rightHandLayerProp.intValue)}");
            EditorGUILayout.LabelField($"Interactable: {EditorUtilities.GetLayerName(_interactableLayerProp.intValue)}");
            EditorGUILayout.LabelField($"Player: {EditorUtilities.GetLayerName(_playerLayerProp.intValue)}");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Physics Settings:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Hand Mass: {_handMassProp.floatValue}");
            EditorGUILayout.LabelField($"Linear Damping: {_linearDampingProp.floatValue}");
            EditorGUILayout.LabelField($"Angular Damping: {_angularDampingProp.floatValue}");
            EditorGUILayout.EndVertical();
        }


        private void DrawHandFollowingSection()
        {
            EditorGUILayout.LabelField("Hand Following Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_followerPresetProp,
                new GUIContent("Follower Preset", "Preset configuration for physics hand following behavior"));
        }
        
    }
}
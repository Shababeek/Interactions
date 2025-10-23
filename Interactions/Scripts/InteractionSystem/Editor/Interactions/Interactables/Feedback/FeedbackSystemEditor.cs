using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Shababeek.Interactions.Feedback;
using System.Collections.Generic;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Custom editor for the FeedbackSystem component.
    /// Provides an improved UI for managing multiple feedback types.
    /// </summary>
    [CustomEditor(typeof(FeedbackSystem))]
    [CanEditMultipleObjects]
    public class FeedbackSystemEditor : Editor
    {
        private SerializedProperty feedbacksProperty;
        private List<bool> foldoutStates = new List<bool>();

        private void OnEnable()
        {
            feedbacksProperty = serializedObject.FindProperty("feedbacks");
            RefreshFoldoutStates();
        }
        
        // Override to prevent UI Toolkit from being used
        public override VisualElement CreateInspectorGUI()
        {
            return null; // Force use of IMGUI
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Add margin manually since we disabled default margins
            EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(4, 4, 4, 4) });

            EditorGUILayout.Space(3);
            DrawHeader();
            EditorGUILayout.Space(5);

            DrawFeedbackList();
            
            EditorGUILayout.Space(3);
            DrawAddButton();
            
            // Only show help box when there are feedbacks
            if (feedbacksProperty.arraySize > 0)
            {
                EditorGUILayout.Space(5);
                DrawHelpBox();
            }
            
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Feedback System", headerStyle);
            
            // Status info
            var feedbackSystem = target as FeedbackSystem;
            var feedbackCount = feedbackSystem?.GetFeedbacks()?.Count ?? 0;
            var validCount = 0;
            if (feedbackSystem != null)
            {
                foreach (var feedback in feedbackSystem.GetFeedbacks())
                {
                    if (feedback != null && feedback.Enabled && feedback.IsValid())
                        validCount++;
                }
            }
            
            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = validCount > 0 ? new Color(0.3f, 0.8f, 0.3f) : Color.gray }
            };
            
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{validCount}/{feedbackCount} Active", statusStyle);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFeedbackList()
        {
            if (feedbacksProperty.arraySize == 0)
            {
                // Compact empty state
                var emptyStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 8, 8)
                };
                EditorGUILayout.BeginVertical(emptyStyle);
                EditorGUILayout.LabelField("No feedbacks configured.", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            RefreshFoldoutStates();

            for (int i = 0; i < feedbacksProperty.arraySize; i++)
            {
                DrawFeedbackItem(i);
            }
        }

        private void DrawFeedbackItem(int index)
        {
            var feedbackProp = feedbacksProperty.GetArrayElementAtIndex(index);
            var feedback = feedbackProp.managedReferenceValue as FeedbackData;
            
            if (feedback == null)
            {
                EditorGUILayout.HelpBox($"Feedback at index {index} is null or invalid.", MessageType.Error);
                return;
            }

            // Background box with reduced padding
            var boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 4, 4),
                margin = new RectOffset(0, 0, 0, 2)
            };
            
            EditorGUILayout.BeginVertical(boxStyle);
            
            // Header row with foldout, enabled toggle, name, and delete button
            EditorGUILayout.BeginHorizontal();
            
            // Foldout
            foldoutStates[index] = EditorGUILayout.Foldout(foldoutStates[index], "", true);
            
            // Enabled toggle
            var enabledProp = feedbackProp.FindPropertyRelative("enabled");
            var wasEnabled = enabledProp.boolValue;
            var isEnabled = EditorGUILayout.Toggle(GUIContent.none, wasEnabled, GUILayout.Width(16));
            if (isEnabled != wasEnabled)
            {
                enabledProp.boolValue = isEnabled;
            }
            
            // Icon
            var iconContent = EditorGUIUtility.IconContent(GetIconForFeedbackType(feedback));
            if (iconContent != null && iconContent.image != null)
            {
                GUILayout.Label(iconContent, GUILayout.Width(20), GUILayout.Height(20));
            }
            
            // Name field
            var nameProp = feedbackProp.FindPropertyRelative("feedbackName");
            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = isEnabled ? GUI.skin.label.normal.textColor : Color.gray }
            };
            
            EditorGUILayout.LabelField(nameProp.stringValue, labelStyle);
            
            // Type label
            var typeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
            GUILayout.Label($"[{GetFeedbackTypeName(feedback)}]", typeStyle, GUILayout.Width(120));
            
            // Validation status
            if (isEnabled)
            {
                var isValid = feedback.IsValid();
                var statusColor = isValid ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.8f, 0.3f, 0.3f);
                var prevColor = GUI.color;
                GUI.color = statusColor;
                GUILayout.Label(isValid ? "●" : "⚠", GUILayout.Width(16));
                GUI.color = prevColor;
            }
            
            // Delete button
            if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(18)))
            {
                if (EditorUtility.DisplayDialog("Delete Feedback", 
                    $"Are you sure you want to delete '{nameProp.stringValue}'?", 
                    "Delete", "Cancel"))
                {
                    DeleteFeedback(index);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Expanded content
            if (foldoutStates[index])
            {
                EditorGUILayout.Space(2);
                
                using (new EditorGUI.DisabledGroupScope(!isEnabled))
                {
                    EditorGUI.indentLevel++;
                    
                    // Name field
                    nameProp.stringValue = EditorGUILayout.TextField("Name", nameProp.stringValue);
                    
                    EditorGUILayout.Space(1);
                    
                    // Draw properties based on feedback type
                    DrawFeedbackTypeProperties(feedback, feedbackProp);
                    
                    EditorGUI.indentLevel--;
                }
                
                // Validation message
                if (isEnabled && !feedback.IsValid())
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.HelpBox(GetValidationMessage(feedback), MessageType.Warning);
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawFeedbackTypeProperties(FeedbackData feedback, SerializedProperty feedbackProp)
        {
            switch (feedback)
            {
                case MaterialFeedback:
                    DrawMaterialFeedbackProperties(feedbackProp);
                    break;
                case AnimationFeedback:
                    DrawAnimationFeedbackProperties(feedbackProp);
                    break;
                case HapticFeedback:
                    DrawHapticFeedbackProperties(feedbackProp);
                    break;
                case AudioFeedback:
                    DrawAudioFeedbackProperties(feedbackProp);
                    break;
            }
        }

        private void DrawMaterialFeedbackProperties(SerializedProperty prop)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("renderers"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("colorPropertyName"));
            
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("hoverColor"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("selectColor"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("activateColor"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("colorMultiplier"));
        }

        private void DrawAnimationFeedbackProperties(SerializedProperty prop)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("animator"));
            
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Parameter Names", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("hoverBoolName"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("selectTriggerName"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("deselectTriggerName"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("activatedTriggerName"));
        }

        private void DrawHapticFeedbackProperties(SerializedProperty prop)
        {
            EditorGUILayout.LabelField("Hover", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("hoverAmplitude"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("hoverDuration"));
            
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("selectAmplitude"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("selectDuration"));
            
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Activation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("activateAmplitude"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("activateDuration"));
        }

        private void DrawAudioFeedbackProperties(SerializedProperty prop)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("audioSource"));
            
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("hoverClip"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("hoverExitClip"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("selectClip"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("deselectClip"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("activateClip"));
            
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Volume Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("hoverVolume"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("selectVolume"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("activateVolume"));
            
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("useSpatialAudio"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("randomizePitch"));
            
            var randomizePitch = prop.FindPropertyRelative("randomizePitch");
            if (randomizePitch.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(prop.FindPropertyRelative("pitchRandomization"));
                EditorGUI.indentLevel--;
            }
        }

        private void DrawAddButton()
        {
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fixedHeight = 28
            };

            if (GUILayout.Button("+ Add Feedback", buttonStyle))
            {
                ShowAddMenu();
            }
        }

        private void ShowAddMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Material Feedback"), false, () => AddFeedback(new MaterialFeedback()));
            menu.AddItem(new GUIContent("Animation Feedback"), false, () => AddFeedback(new AnimationFeedback()));
            menu.AddItem(new GUIContent("Haptic Feedback"), false, () => AddFeedback(new HapticFeedback()));
            menu.AddItem(new GUIContent("Audio Feedback"), false, () => AddFeedback(new AudioFeedback()));
            menu.ShowAsContext();
        }

        private void AddFeedback(FeedbackData newFeedback)
        {
            var feedbackSystem = target as FeedbackSystem;
            if (feedbackSystem == null) return;

            Undo.RecordObject(feedbackSystem, "Add Feedback");
            feedbackSystem.AddFeedback(newFeedback);
            EditorUtility.SetDirty(feedbackSystem);
            serializedObject.Update();
            RefreshFoldoutStates();
            
            // Auto-expand the newly added feedback
            if (foldoutStates.Count > 0)
            {
                foldoutStates[foldoutStates.Count - 1] = true;
            }
        }

        private void DeleteFeedback(int index)
        {
            var feedbackSystem = target as FeedbackSystem;
            if (feedbackSystem == null) return;

            Undo.RecordObject(feedbackSystem, "Remove Feedback");
            
            // Get the feedback list and remove the item
            var feedbacks = feedbackSystem.GetFeedbacks();
            if (index >= 0 && index < feedbacks.Count)
            {
                feedbacks.RemoveAt(index);
            }
            
            EditorUtility.SetDirty(feedbackSystem);
            serializedObject.Update();
            RefreshFoldoutStates();
            
            // Force repaint
            Repaint();
        }

        private void DrawHelpBox()
        {
            EditorGUILayout.HelpBox(
                "Feedbacks respond to interaction events (hover, select, activate). " +
                "Configure multiple feedback types for rich multi-sensory experiences.",
                MessageType.Info);
        }

        private void RefreshFoldoutStates()
        {
            while (foldoutStates.Count < feedbacksProperty.arraySize)
            {
                foldoutStates.Add(false);
            }
            while (foldoutStates.Count > feedbacksProperty.arraySize)
            {
                foldoutStates.RemoveAt(foldoutStates.Count - 1);
            }
        }

        private string GetFeedbackTypeName(FeedbackData feedback)
        {
            return feedback switch
            {
                MaterialFeedback => "Material",
                AnimationFeedback => "Animation",
                HapticFeedback => "Haptic",
                AudioFeedback => "Audio",
                _ => "Unknown"
            };
        }

        private string GetIconForFeedbackType(FeedbackData feedback)
        {
            return feedback switch
            {
                MaterialFeedback => "d_Material Icon",
                AnimationFeedback => "d_AnimationClip Icon",
                HapticFeedback => "d_Profiler.Audio",
                AudioFeedback => "d_AudioSource Icon",
                _ => "d_Settings Icon"
            };
        }

        private string GetValidationMessage(FeedbackData feedback)
        {
            return feedback switch
            {
                MaterialFeedback => "Please assign at least one renderer.",
                AnimationFeedback => "Please assign an Animator component.",
                AudioFeedback => "Please assign an Audio Source component.",
                _ => "This feedback is not properly configured."
            };
        }
    }
}

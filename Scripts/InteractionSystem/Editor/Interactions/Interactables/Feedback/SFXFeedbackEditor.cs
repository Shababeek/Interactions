using Shababeek.Interactions.Feedback;
using UnityEngine;
using UnityEditor;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(SFXFeedback))]
    public class SFXFeedbackEditor : UnityEditor.Editor
    {
        private SFXFeedback _sfxFeedback;
        private AudioSource _audioSource;
        
        // Foldout states
        private bool _hoverFoldout = false;
        private bool _selectionFoldout = true;
        private bool _activationFoldout = true;
        private bool _audioSettingsFoldout = true;
        
        // Preview audio clips
        private AudioClip _previewClip;
        private float _previewVolume = 1f;
        
        private void OnEnable()
        {
            _sfxFeedback = (SFXFeedback)target;
            _audioSource = _sfxFeedback.GetComponent<AudioSource>();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            DrawHeader();
            EditorGUILayout.Space();
            
            // Hover Sound Effects Section
            _hoverFoldout = EditorGUILayout.Foldout(_hoverFoldout, "Hover Sound Effects", true);
            
            if (_hoverFoldout)
            {
                EditorGUI.indentLevel++;
                DrawHoverSection();
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Selection Sound Effects Section
            _selectionFoldout = EditorGUILayout.Foldout(_selectionFoldout, "Selection Sound Effects", true);
            if (_selectionFoldout)
            {
                EditorGUI.indentLevel++;
                DrawSelectionSection();
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Activation Sound Effects Section
            _activationFoldout = EditorGUILayout.Foldout(_activationFoldout, "Activation Sound Effects", true);
            if (_activationFoldout)
            {
                EditorGUI.indentLevel++;
                DrawActivationSection();
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Audio Settings Section
            _audioSettingsFoldout = EditorGUILayout.Foldout(_audioSettingsFoldout, "Audio Settings", true);
            if (_audioSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                DrawAudioSettingsSection();
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Audio Preview Section
            DrawAudioPreviewSection();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SFX Feedback System", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox("Configure sound effects for different interaction states. Each section can be enabled/disabled independently.", MessageType.Info);
        }
        
        private void DrawHoverSection()
        {
            var playHoverSFX = serializedObject.FindProperty("playHoverSFX");
            var hoverEnterClip = serializedObject.FindProperty("hoverEnterClip");
            var hoverExitClip = serializedObject.FindProperty("hoverExitClip");
            var hoverVolume = serializedObject.FindProperty("hoverVolume");
            
            EditorGUILayout.PropertyField(playHoverSFX);
            
            if (playHoverSFX.boolValue)
            {
                EditorGUILayout.Space(5);
                
                // Hover Enter
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(hoverEnterClip, new GUIContent("Hover Enter Clip"));
                if (hoverEnterClip.objectReferenceValue != null)
                {
                    if (GUILayout.Button("▶", GUILayout.Width(25)))
                    {
                        PlayPreviewClip((AudioClip)hoverEnterClip.objectReferenceValue, hoverVolume.floatValue);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // Hover Exit
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(hoverExitClip, new GUIContent("Hover Exit Clip"));
                if (hoverExitClip.objectReferenceValue != null)
                {
                    if (GUILayout.Button("▶", GUILayout.Width(25)))
                    {
                        PlayPreviewClip((AudioClip)hoverExitClip.objectReferenceValue, hoverVolume.floatValue);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.PropertyField(hoverVolume);
                
                // Visual feedback
                DrawClipStatus(hoverEnterClip, hoverExitClip, "Hover");
            }
        }
        
        private void DrawSelectionSection()
        {
            var playSelectionSFX = serializedObject.FindProperty("playSelectionSFX");
            var selectClip = serializedObject.FindProperty("selectClip");
            var deselectClip = serializedObject.FindProperty("deselectClip");
            var selectionVolume = serializedObject.FindProperty("selectionVolume");
            
            EditorGUILayout.PropertyField(playSelectionSFX);
            
            if (playSelectionSFX.boolValue)
            {
                EditorGUILayout.Space(5);
                
                // Select
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(selectClip, new GUIContent("Select Clip"));
                if (selectClip.objectReferenceValue != null)
                {
                    if (GUILayout.Button("▶", GUILayout.Width(25)))
                    {
                        PlayPreviewClip((AudioClip)selectClip.objectReferenceValue, selectionVolume.floatValue);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // Deselect
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(deselectClip, new GUIContent("Deselect Clip"));
                if (deselectClip.objectReferenceValue != null)
                {
                    if (GUILayout.Button("▶", GUILayout.Width(25)))
                    {
                        PlayPreviewClip((AudioClip)deselectClip.objectReferenceValue, selectionVolume.floatValue);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.PropertyField(selectionVolume);
                
                // Visual feedback
                DrawClipStatus(selectClip, deselectClip, "Selection");
            }
        }
        
        private void DrawActivationSection()
        {
            var playActivationSFX = serializedObject.FindProperty("playActivationSFX");
            var activateClip = serializedObject.FindProperty("activateClip");
            var activationVolume = serializedObject.FindProperty("activationVolume");
            
            EditorGUILayout.PropertyField(playActivationSFX);
            
            if (playActivationSFX.boolValue)
            {
                EditorGUILayout.Space(5);
                
                // Activate
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(activateClip, new GUIContent("Activate Clip"));
                if (activateClip.objectReferenceValue != null)
                {
                    if (GUILayout.Button("▶", GUILayout.Width(25)))
                    {
                        PlayPreviewClip((AudioClip)activateClip.objectReferenceValue, activationVolume.floatValue);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.PropertyField(activationVolume);
                
                // Visual feedback
                DrawClipStatus(activateClip, null, "Activation");
            }
        }
        
        private void DrawAudioSettingsSection()
        {
            var useSpatialAudio = serializedObject.FindProperty("useSpatialAudio");
            var minDistance = serializedObject.FindProperty("minDistance");
            var maxDistance = serializedObject.FindProperty("maxDistance");
            var randomizePitch = serializedObject.FindProperty("randomizePitch");
            var pitchVariation = serializedObject.FindProperty("pitchVariation");
            
            EditorGUILayout.PropertyField(useSpatialAudio);
            
            if (useSpatialAudio.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(minDistance);
                EditorGUILayout.PropertyField(maxDistance);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(randomizePitch);
            
            if (randomizePitch.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(pitchVariation);
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawAudioPreviewSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Preview", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            _previewClip = (AudioClip)EditorGUILayout.ObjectField("Preview Clip", _previewClip, typeof(AudioClip), false);
            _previewVolume = EditorGUILayout.Slider(_previewVolume, 0f, 1f, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Preview") && _previewClip != null)
            {
                PlayPreviewClip(_previewClip, _previewVolume);
            }
            if (GUILayout.Button("Stop Preview"))
            {
                StopPreview();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawClipStatus(SerializedProperty clip1, SerializedProperty clip2, string sectionName)
        {
            bool hasClip1 = clip1.objectReferenceValue != null;
            bool hasClip2 = clip2 != null && clip2.objectReferenceValue != null;
            
            string status = "";
            Color statusColor = Color.green;
            
            if (clip2 != null)
            {
                // Two clips section
                if (hasClip1 && hasClip2)
                {
                    status = $"✓ {sectionName} clips assigned";
                }
                else if (hasClip1 || hasClip2)
                {
                    status = $"⚠ {sectionName} partially configured";
                    statusColor = Color.yellow;
                }
                else
                {
                    status = $"✗ {sectionName} clips missing";
                    statusColor = Color.red;
                }
            }
            else
            {
                // Single clip section
                if (hasClip1)
                {
                    status = $"✓ {sectionName} clip assigned";
                }
                else
                {
                    status = $"✗ {sectionName} clip missing";
                    statusColor = Color.red;
                }
            }
            
            var originalColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField(status, EditorStyles.miniLabel);
            GUI.color = originalColor;
        }
        
        private void PlayPreviewClip(AudioClip clip, float volume)
        {
            if (clip == null || _audioSource == null) return;
            
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.Play();
        }
        
        private void StopPreview()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }
    }
} 
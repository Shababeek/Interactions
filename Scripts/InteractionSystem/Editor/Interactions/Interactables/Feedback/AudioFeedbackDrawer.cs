using Shababeek.Interactions.Feedback;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Property drawer for AudioFeedback.
    /// Note: The FeedbackSystemEditor handles drawing inline, this is a fallback.
    /// </summary>
    [CustomPropertyDrawer(typeof(AudioFeedback))]
    public class AudioFeedbackDrawer : FeedbackDrawerBase
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var styleSheet = GetFeedbackStyleSheet();
            if (styleSheet != null)
            {
                container.styleSheets.Add(styleSheet);
            }

            container.AddToClassList("feedback-section");

            // Header
            var nameProp = property.FindPropertyRelative("feedbackName");
            var header = new Label(string.IsNullOrEmpty(nameProp.stringValue) ? "Audio Feedback" : nameProp.stringValue);
            header.AddToClassList("feedback-header");
            container.Add(header);

            // Properties
            container.Add(new PropertyField(property.FindPropertyRelative("enabled")));
            container.Add(new PropertyField(property.FindPropertyRelative("feedbackName")));
            container.Add(new PropertyField(property.FindPropertyRelative("audioSource")));
            
            var clipsLabel = new Label("Audio Clips") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            container.Add(clipsLabel);
            container.Add(new PropertyField(property.FindPropertyRelative("hoverClip")));
            container.Add(new PropertyField(property.FindPropertyRelative("hoverExitClip")));
            container.Add(new PropertyField(property.FindPropertyRelative("selectClip")));
            container.Add(new PropertyField(property.FindPropertyRelative("deselectClip")));
            container.Add(new PropertyField(property.FindPropertyRelative("activateClip")));
            
            var volumeLabel = new Label("Volume Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            container.Add(volumeLabel);
            container.Add(new PropertyField(property.FindPropertyRelative("hoverVolume")));
            container.Add(new PropertyField(property.FindPropertyRelative("selectVolume")));
            container.Add(new PropertyField(property.FindPropertyRelative("activateVolume")));
            
            var advancedLabel = new Label("Advanced") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            container.Add(advancedLabel);
            container.Add(new PropertyField(property.FindPropertyRelative("useSpatialAudio")));
            container.Add(new PropertyField(property.FindPropertyRelative("randomizePitch")));
            container.Add(new PropertyField(property.FindPropertyRelative("pitchRandomization")));

            return container;
        }
    }
}

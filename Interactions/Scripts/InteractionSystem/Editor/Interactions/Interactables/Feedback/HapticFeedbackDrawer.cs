using Shababeek.Interactions.Feedback;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Property drawer for HapticFeedback.
    /// Note: The FeedbackSystemEditor handles drawing inline, this is a fallback.
    /// </summary>
    [CustomPropertyDrawer(typeof(HapticFeedback))]
    public class HapticFeedbackDrawer : FeedbackDrawerBase
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
            var header = new Label(string.IsNullOrEmpty(nameProp.stringValue) ? "Haptic Feedback" : nameProp.stringValue);
            header.AddToClassList("feedback-header");
            container.Add(header);

            // Properties
            container.Add(new PropertyField(property.FindPropertyRelative("enabled")));
            container.Add(new PropertyField(property.FindPropertyRelative("feedbackName")));
            
            var hoverLabel = new Label("Hover Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            container.Add(hoverLabel);
            container.Add(new PropertyField(property.FindPropertyRelative("hoverAmplitude")));
            container.Add(new PropertyField(property.FindPropertyRelative("hoverDuration")));
            
            var selectLabel = new Label("Selection Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            container.Add(selectLabel);
            container.Add(new PropertyField(property.FindPropertyRelative("selectAmplitude")));
            container.Add(new PropertyField(property.FindPropertyRelative("selectDuration")));
            
            var activateLabel = new Label("Activation Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            container.Add(activateLabel);
            container.Add(new PropertyField(property.FindPropertyRelative("activateAmplitude")));
            container.Add(new PropertyField(property.FindPropertyRelative("activateDuration")));

            return container;
        }
    }
}

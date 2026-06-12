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
            
            var patternsLabel = new Label("Haptic Patterns") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            container.Add(patternsLabel);
            container.Add(new PropertyField(property.FindPropertyRelative("hoverPattern")));
            container.Add(new PropertyField(property.FindPropertyRelative("selectPattern")));
            container.Add(new PropertyField(property.FindPropertyRelative("activatePattern")));

            return container;
        }
    }
}

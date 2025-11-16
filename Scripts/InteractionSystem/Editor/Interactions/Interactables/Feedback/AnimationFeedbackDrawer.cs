using Shababeek.Interactions.Feedback;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Property drawer for AnimationFeedback.
    /// Note: The FeedbackSystemEditor handles drawing inline, this is a fallback.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimationFeedback))]
    public class AnimationFeedbackDrawer : FeedbackDrawerBase
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
            var header = new Label(string.IsNullOrEmpty(nameProp.stringValue) ? "Animation Feedback" : nameProp.stringValue);
            header.AddToClassList("feedback-header");
            container.Add(header);

            // Properties
            container.Add(new PropertyField(property.FindPropertyRelative("enabled")));
            container.Add(new PropertyField(property.FindPropertyRelative("feedbackName")));
            container.Add(new PropertyField(property.FindPropertyRelative("animator")));
            container.Add(new PropertyField(property.FindPropertyRelative("hoverBoolName")));
            container.Add(new PropertyField(property.FindPropertyRelative("selectTriggerName")));
            container.Add(new PropertyField(property.FindPropertyRelative("deselectTriggerName")));
            container.Add(new PropertyField(property.FindPropertyRelative("activatedTriggerName")));

            return container;
        }
    }
}

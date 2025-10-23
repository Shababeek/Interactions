using Shababeek.Interactions.Feedback;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// Property drawer for MaterialFeedback.
    /// Note: The FeedbackSystemEditor handles drawing inline, this is a fallback.
    /// </summary>
    [CustomPropertyDrawer(typeof(MaterialFeedback))]
    public class MaterialFeedbackDrawer : FeedbackDrawerBase
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
            var header = new Label(string.IsNullOrEmpty(nameProp.stringValue) ? "Material Feedback" : nameProp.stringValue);
            header.AddToClassList("feedback-header");
            container.Add(header);

            // Properties
            container.Add(new PropertyField(property.FindPropertyRelative("enabled")));
            container.Add(new PropertyField(property.FindPropertyRelative("feedbackName")));
            container.Add(new PropertyField(property.FindPropertyRelative("renderers")));
            container.Add(new PropertyField(property.FindPropertyRelative("colorPropertyName")));
            container.Add(new PropertyField(property.FindPropertyRelative("hoverColor")));
            container.Add(new PropertyField(property.FindPropertyRelative("selectColor")));
            container.Add(new PropertyField(property.FindPropertyRelative("activateColor")));
            container.Add(new PropertyField(property.FindPropertyRelative("colorMultiplier")));

            return container;
        }
    }
}

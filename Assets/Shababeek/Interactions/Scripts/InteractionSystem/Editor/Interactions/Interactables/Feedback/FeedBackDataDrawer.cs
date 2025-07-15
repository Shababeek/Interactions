using Shababeek.Interactions.Feedback;
using UnityEditor;
using UnityEngine.UIElements;

namespace Shababeek.InteractionSystem.Interactions.Feedback.Editors
{
    [CustomPropertyDrawer(typeof(FeedbackData))]

    public class FeedBackDataDrawer: PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var name = property.FindPropertyRelative("feedbackName").stringValue;
            var header =  new Label($"Animation {name} Settings");
            header.AddToClassList("feedback-header");
            container.Add(header);  
            return container;
        }

    }
}
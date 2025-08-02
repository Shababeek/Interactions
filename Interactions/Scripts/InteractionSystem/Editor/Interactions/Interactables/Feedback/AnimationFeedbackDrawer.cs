using Shababeek.Interactions.Feedback;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Shababeek.Interactions.Editors
{
    [CustomPropertyDrawer(typeof(AnimationFeedback))]
    public class AnimationFeedbackDrawer : PropertyDrawer
    {
        static string cssPath = "Assets/Kandooz/Kinteractions-VR/InteractionSystem/Editor/Interactions/Interactables/Feedback/FeedbackDrawers.uss";

        public static VisualElement GetVisualElement(SerializedProperty property)
        {
            var container = new VisualElement();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(cssPath);
            container.styleSheets.Add(styleSheet);
            var name = property.FindPropertyRelative("feedbackName").stringValue;
            var header = new Label($"Animation {name} Settings");
            header.AddToClassList("feedback-header");
            container.Add(header);
            var dataContainer = new VisualElement();
            dataContainer.AddToClassList("feedback-section");

            dataContainer.AddToClassList("data");

            var animatorField = new PropertyField(property.FindPropertyRelative("animator"), "Animator");
            dataContainer.Add(animatorField);

            // Event Triggers Section
            var triggersSection = new VisualElement();
            var hoverBoolField = new PropertyField(property.FindPropertyRelative("hoverBoolName"), "Hover Bool");
            var selectTriggerField =
                new PropertyField(property.FindPropertyRelative("selectTriggerName"), "Select Trigger");
            var deselectTriggerField =
                new PropertyField(property.FindPropertyRelative("deselectTriggerName"), "Deselect Trigger");
            var activateTriggerField =
                new PropertyField(property.FindPropertyRelative("activateTriggerName"), "Activate Trigger");

            triggersSection.Add(hoverBoolField);
            triggersSection.Add(selectTriggerField);
            triggersSection.Add(deselectTriggerField);
            triggersSection.Add(activateTriggerField);
            dataContainer.Add(triggersSection);
            container.Add(dataContainer);
            return container;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return GetVisualElement(property);
        }
    }
}
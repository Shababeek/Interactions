using Shababeek.Interactions.Feedback;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Shababeek.Interactions.Editors
{
    [CustomPropertyDrawer(typeof(HapticFeedback))]
    public class HapticFeedbackDrawer : PropertyDrawer
    {
        string cssPath = "Assets/Kandooz/Kinteractions-VR/InteractionSystem/Editor/Interactions/Interactables/Feedback/FeedbackDrawers.uss";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return GetVisualElement(property);
        }

        public static VisualElement GetVisualElement(SerializedProperty property)
        {
            var container = new VisualElement();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Kandooz/Kinteractions-VR/InteractionSystem/Editor/Interactions/Interactables/Feedback/FeedbackDrawers.uss");
            container.styleSheets.Add(styleSheet);
            var name = property.FindPropertyRelative("feedbackName").stringValue;
            var header = new Label($"Haptic {name} Settings");
            header.AddToClassList("feedback-header");
            container.Add(header);
            var dataContainer = new VisualElement();
            dataContainer.AddToClassList("feedback-section");
            dataContainer.AddToClassList("data");

            var hapticDriverField = new PropertyField(property.FindPropertyRelative("hapticDriver"), "Haptic Driver");
            var hoverIntensityField = new PropertyField(property.FindPropertyRelative("hoverIntensity"), "Hover Intensity");
            var selectIntensityField = new PropertyField(property.FindPropertyRelative("selectIntensity"), "Select Intensity");
            var activateIntensityField = new PropertyField(property.FindPropertyRelative("activateIntensity"), "Activate Intensity");
            var durationField = new PropertyField(property.FindPropertyRelative("duration"), "Duration");

            dataContainer.Add(hapticDriverField);
            dataContainer.Add(hoverIntensityField);
            dataContainer.Add(selectIntensityField);
            dataContainer.Add(activateIntensityField);
            dataContainer.Add(durationField);
            container.Add(dataContainer);
            return container;
        }
    }
} 
using Shababeek.Interactions.Feedback;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Shababeek.InteractionSystem.Interactions.Feedback.Editors
{
    [CustomPropertyDrawer(typeof(MaterialFeedback))]
    public class MaterialFeedbackDrawer : PropertyDrawer
    {
        static string cssPath = "Assets/Kandooz/Kinteractions-VR/InteractionSystem/Editor/Interactions/Interactables/Feedback/FeedbackDrawers.uss";

        public static VisualElement GetVisualElement(SerializedProperty property)
        {
            var container = new VisualElement();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(cssPath);
            container.styleSheets.Add(styleSheet);
            var name = property.FindPropertyRelative("feedbackName").stringValue;
            var header = new Label($"Material {name} Settings");
            header.AddToClassList("feedback-header");
            container.Add(header);
            var dataContainer = new VisualElement();
            dataContainer.AddToClassList("feedback-section");
            dataContainer.AddToClassList("data");

            var renderersField = new PropertyField(property.FindPropertyRelative("renderers"), "Renderers");
            var colorPropertyField = new PropertyField(property.FindPropertyRelative("colorPropertyName"), "Color Property");
            var hoverColorField = new PropertyField(property.FindPropertyRelative("hoverColor"), "Hover Color");
            var selectColorField = new PropertyField(property.FindPropertyRelative("selectColor"), "Select Color");
            var activateColorField = new PropertyField(property.FindPropertyRelative("activateColor"), "Activate Color");
            var multiplierField = new PropertyField(property.FindPropertyRelative("colorMultiplier"), "Color Multiplier");

            dataContainer.Add(renderersField);
            dataContainer.Add(colorPropertyField);
            dataContainer.Add(hoverColorField);
            dataContainer.Add(selectColorField);
            dataContainer.Add(activateColorField);
            dataContainer.Add(multiplierField);
            container.Add(dataContainer);
            return container;
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return GetVisualElement(property);
        }
    }
} 
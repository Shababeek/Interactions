using Shababeek.Interactions.Feedback;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Shababeek.InteractionSystem.Interactions.Feedback.Editors
{
    [CustomPropertyDrawer(typeof(AudioFeedback))]
    public class AudioFeedbackDrawer : PropertyDrawer
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
            var header = new Label($"Audio {name} Settings");
            header.AddToClassList("feedback-header");
            container.Add(header);
            var dataContainer = new VisualElement();
            dataContainer.AddToClassList("feedback-section");
            dataContainer.AddToClassList("data");

            var audioSourceField = new PropertyField(property.FindPropertyRelative("audioSource"), "Audio Source");
            var hoverClipField = new PropertyField(property.FindPropertyRelative("hoverClip"), "Hover Clip");
            var selectClipField = new PropertyField(property.FindPropertyRelative("selectClip"), "Select Clip");
            var activateClipField = new PropertyField(property.FindPropertyRelative("activateClip"), "Activate Clip");
            var volumeField = new PropertyField(property.FindPropertyRelative("volume"), "Volume");
            var spatialAudioField = new PropertyField(property.FindPropertyRelative("spatialAudio"), "Spatial Audio");
            var pitchRandomizationField = new PropertyField(property.FindPropertyRelative("pitchRandomization"), "Pitch Randomization");

            dataContainer.Add(audioSourceField);
            dataContainer.Add(hoverClipField);
            dataContainer.Add(selectClipField);
            dataContainer.Add(activateClipField);
            dataContainer.Add(volumeField);
            dataContainer.Add(spatialAudioField);
            dataContainer.Add(pitchRandomizationField);
            container.Add(dataContainer);
            return container;
        }
    }
} 
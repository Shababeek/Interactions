using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Shababeek.InteractionSystem.Interactions.Feedback;
using Shababeek.Interactions.Core;
using System;
using Shababeek.Interactions.Feedback;

namespace Shababeek.InteractionSystem.Interactions.Feedback.Editors
{
    [CustomEditor(typeof(FeedbackSystem))]
    public class FeedbackSystemEditor : Editor
    {
        private VisualElement root;
        private ListView feedbackListView;
        private VisualElement feedbackDetailContainer;
        private SerializedProperty feedbacksProperty;
        private int selectedIndex = -1;
        private string[] feedbackTypes = { "Material", "Animation", "Haptic", "Audio" };

        public override VisualElement CreateInspectorGUI()
        {
            Debug.Log(selectedIndex);
            root = new VisualElement();

            // Load USS stylesheet
            var styleSheet = GetFeedbackStyleSheet();
            if (styleSheet != null)
                root.styleSheets.Add(styleSheet);

            // Header
            var header = new Label("Feedback System");
            header.AddToClassList("feedback-header");
            root.Add(header);

            // Description
            var description = new Label("Configure multiple feedback types for this interactable. Each feedback can respond to hover, select, and activate events.");
            description.AddToClassList("description-text");
            root.Add(description);

            feedbacksProperty = serializedObject.FindProperty("feedbacks");

            // ListView for feedbacks
            feedbackListView = new ListView();
            feedbackListView.itemsSource = new System.Collections.Generic.List<SerializedProperty>();
            feedbackListView.selectionType = SelectionType.Single;
            feedbackListView.style.height = 200;
            feedbackListView.makeItem = CreateListItem;
            feedbackListView.bindItem = BindListItem;
            feedbackListView.selectionChanged += OnFeedbackSelected;
            feedbackListView.reorderable = true;
            feedbackListView.showAddRemoveFooter = true;
            feedbackListView.showBorder = true;
            feedbackListView.style.marginBottom = 10;
            feedbackListView.onAdd += _=> ShowAddMenu();
            root.Add(feedbackListView);

            // Add/Remove buttons
            var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 10 } };
            var addButton = new Button(ShowAddMenu) { text = "Add Feedback" };
            var removeButton = new Button(RemoveSelectedFeedback) { text = "Remove Selected" };
            buttonContainer.Add(addButton);
            buttonContainer.Add(removeButton);
            root.Add(buttonContainer);

            // Detail container
            feedbackDetailContainer = new VisualElement();
            root.Add(feedbackDetailContainer);
            RefreshList();

            return root;
        }
        

        private void ShowAddMenu()
        {
            var menu = new GenericMenu();
            for (int i = 0; i < feedbackTypes.Length; i++)
            {
                int typeIndex = i;
                menu.AddItem(new GUIContent(feedbackTypes[i]), false, () => AddFeedbackOfType(typeIndex));
            }
            menu.ShowAsContext();
        }

        private void AddFeedbackOfType(int typeIndex)
        {
            var feedbackSystem = target as FeedbackSystem;
            if (feedbackSystem == null) return;
            Undo.RecordObject(feedbackSystem, "Add Feedback");
            FeedbackData newFeedback = typeIndex switch
            {
                0 => new MaterialFeedback(),
                1 => new AnimationFeedback(),
                2 => new HapticFeedback(),
                3 => new AudioFeedback(),
                _ => null
            };
            if (newFeedback != null)
            {
                feedbackSystem.AddFeedback(newFeedback);
                EditorUtility.SetDirty(feedbackSystem);
                serializedObject.Update();
                RefreshList();
            }
        }

        private VisualElement CreateListItem()
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 2 } };
            var nameField = new TextField { style = { flexGrow = 1, marginRight = 8 } };
            var enabledToggle = new Toggle { style = { marginRight = 8 } };
            row.Add(nameField);
            row.Add(enabledToggle);
            return row;
        }

        private void BindListItem(VisualElement element, int index)
        {
            if (index < 0 || index >= feedbacksProperty.arraySize) return;
            var feedbackProp = feedbacksProperty.GetArrayElementAtIndex(index);
            var nameProp = feedbackProp.FindPropertyRelative("feedbackName");
            var enabledProp = feedbackProp.FindPropertyRelative("enabled");

            var nameField = element.ElementAt(0) as TextField;
            var enabledToggle = element.ElementAt(1) as Toggle;

            nameField.value = nameProp.stringValue;
            nameField.RegisterValueChangedCallback(evt => {
                nameProp.stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            enabledToggle.value = enabledProp.boolValue;
            enabledToggle.RegisterValueChangedCallback(evt => {
                enabledProp.boolValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });
        }

        private void OnFeedbackSelected(System.Collections.Generic.IEnumerable<object> selected)
        {
            feedbackDetailContainer.Clear();
            selectedIndex = -1;
            foreach (var obj in selected)
            {
                selectedIndex = feedbackListView.selectedIndex;
                break;
            }

            if (selectedIndex < 0 || selectedIndex >= feedbacksProperty.arraySize) return;
            var feedbackProp = feedbacksProperty.GetArrayElementAtIndex(selectedIndex);
            if (feedbackProp.managedReferenceValue is FeedbackData feedbackObj)
            {
                var feedbackSystem = target as FeedbackSystem;
                var detail = feedbackObj.CreateVisualElement(() =>
                {
                    if (feedbackSystem == null) return;
                    Undo.RecordObject(feedbackSystem, "Change Feedback Property");
                    EditorUtility.SetDirty(feedbackSystem);
                });
                feedbackDetailContainer.Add(detail);
            }
            else
            {
                feedbackDetailContainer.Add(new UnityEngine.UIElements.Label("Invalid feedback reference."));
            }
        }

        private PropertyDrawer PropertyDrawerFor(System.Type type)
        {
            if (type == typeof(MaterialFeedback)) return new MaterialFeedbackDrawer();
            if (type == typeof(AnimationFeedback)) return new AnimationFeedbackDrawer();
            if (type == typeof(HapticFeedback)) return new HapticFeedbackDrawer();
            if (type == typeof(AudioFeedback)) return new AudioFeedbackDrawer();
            return null;
        }
        

        private void RemoveSelectedFeedback()
        {
            if (selectedIndex >= 0 && selectedIndex < feedbacksProperty.arraySize)
            {
                feedbacksProperty.DeleteArrayElementAtIndex(selectedIndex);
                serializedObject.ApplyModifiedProperties();
                RefreshList();
                feedbackDetailContainer.Clear();
            }
        }

        private void RefreshList()
        {
            var list = new System.Collections.Generic.List<SerializedProperty>();
            for (int i = 0; i < feedbacksProperty.arraySize; i++)
            {
                list.Add(feedbacksProperty.GetArrayElementAtIndex(i));
            }
            feedbackListView.itemsSource = list;
            feedbackListView.Rebuild();
        }

        private static StyleSheet GetFeedbackStyleSheet()
        {
            // Try to get from config first
            var configs = AssetDatabase.FindAssets("t:Shababeek.Interactions.Core.Config");
            if (configs.Length > 0)
            {
                var configPath = AssetDatabase.GUIDToAssetPath(configs[0]);
                var config = AssetDatabase.LoadAssetAtPath<Config>(configPath);
                if (config?.FeedbackSystemStyleSheet != null)
                {
                    return config.FeedbackSystemStyleSheet;
                }
            }

            // Fallback to hardcoded path
            var fallbackPath = "Assets/Shababeek/Interactions/Scripts/InteractionSystem/Editor/Interactions/Interactables/Feedback/FeedbackDrawers.uss";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(fallbackPath);
            if (styleSheet == null)
            {
                Debug.LogWarning($"Feedback style sheet not found at {fallbackPath}. Please assign it in the Shababeek Config asset.");
            }
            return styleSheet;
        }
    }
} 
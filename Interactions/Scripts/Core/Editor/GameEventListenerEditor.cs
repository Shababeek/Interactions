using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Shababeek.Core;

namespace Shababeek.Core.Editors
{
    [CustomEditor(typeof(GameEventListener))]
    public class GameEventListenerEditor : Editor
    {
        private GameEventListener gameEventListener;
        private ReorderableList gameEventsList;
        private int selectedGameEventIndex = -1;
        private Editor selectedGameEventEditor;

        private void OnEnable()
        {
            gameEventListener = (GameEventListener)target;
            
            // Create ReorderableList for game events
            var gameEventListProp = serializedObject.FindProperty("gameEventList");
            gameEventsList = new ReorderableList(serializedObject, gameEventListProp, true, true, true, true);
            gameEventsList.drawHeaderCallback = DrawHeaderCallback;
            gameEventsList.drawElementCallback = DrawElementCallback;
            gameEventsList.onAddCallback = OnAddCallback;
            gameEventsList.onRemoveCallback = OnRemoveCallback;
            gameEventsList.onSelectCallback = OnSelectCallback;
        }
        


        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Game Events");
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            var element = gameEventsList.serializedProperty.GetArrayElementAtIndex(index);
            var gameEventProp = element.FindPropertyRelative("gameEvent");
            
            // Only show the game event reference field
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight), 
                gameEventProp, new GUIContent($"Game Event {index + 1}"));
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            var newElement = list.serializedProperty.GetArrayElementAtIndex(index);
            newElement.FindPropertyRelative("gameEvent").objectReferenceValue = null;
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            if (selectedGameEventIndex == list.index)
            {
                selectedGameEventIndex = -1;
                selectedGameEventEditor = null;
            }
            else if (selectedGameEventIndex > list.index)
            {
                selectedGameEventIndex--;
            }
        }

        private void OnSelectCallback(ReorderableList list)
        {
            selectedGameEventIndex = list.index;
            var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
            var gameEventProp = element.FindPropertyRelative("gameEvent");
            if (gameEventProp.objectReferenceValue != null)
            {
                selectedGameEventEditor = Editor.CreateEditor(gameEventProp.objectReferenceValue);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "GameEventListener manages events for multiple game events. Select a game event to configure its Unity events.",
                MessageType.Info
            );

            serializedObject.Update();

            // Game Events List
            gameEventsList.DoLayoutList();

            // Show selected game event configuration
            if (selectedGameEventIndex >= 0 && selectedGameEventIndex < gameEventListener.GameEventList.Count)
            {
                var selectedGameEventWithEvents = gameEventListener.GameEventList[selectedGameEventIndex];
                if (selectedGameEventWithEvents.gameEvent != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"Game Event {selectedGameEventIndex + 1} Configuration", EditorStyles.boldLabel);
                    
                    // Game Event Events
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Unity Events", EditorStyles.boldLabel);
                    
                    SerializedProperty gameEventListProp = serializedObject.FindProperty("gameEventList");
                    var selectedGameEventWithEventsProp = gameEventListProp.GetArrayElementAtIndex(selectedGameEventIndex);
                    
                    var onGameEventRaisedProp = selectedGameEventWithEventsProp.FindPropertyRelative("onGameEventRaised");
                    
                    EditorGUILayout.PropertyField(onGameEventRaisedProp, new GUIContent("On Game Event Raised"));
                    EditorGUILayout.EndVertical();
                    
                    // Game Event Properties (Scriptable Object Editor)
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Game Event Properties", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box");
                    if (selectedGameEventEditor != null)
                    {
                        selectedGameEventEditor.OnInspectorGUI();
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(
                        $"Game Event {selectedGameEventIndex + 1} has no GameEvent assigned. Please assign a GameEvent to configure its Unity events.",
                        MessageType.Warning
                    );
                    
                    // Property field to assign the GameEvent
                    SerializedProperty gameEventListProp = serializedObject.FindProperty("gameEventList");
                    var selectedGameEventWithEventsProp = gameEventListProp.GetArrayElementAtIndex(selectedGameEventIndex);
                    var gameEventProp = selectedGameEventWithEventsProp.FindPropertyRelative("gameEvent");
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Assign Game Event", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(gameEventProp, new GUIContent("Game Event"));
                }
            }
            else if (selectedGameEventIndex >= 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "Selected index is out of range. Please select a valid game event from the list above.",
                    MessageType.Warning
                );
            }

            // Add Game Event Button
            EditorGUILayout.Space();
            if (GUILayout.Button("Add New Game Event"))
            {
                gameEventListener.AddGameEvent(null);
                EditorUtility.SetDirty(gameEventListener);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
} 
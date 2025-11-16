using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Shababeek.Sequencing;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Custom editor for StepEventListener with reorderable step list and event configuration.
    /// </summary>
    [CustomEditor(typeof(StepEventListener))]
    public class StepEventListenerEditor : Editor
    {
        private StepEventListener stepEventListener;
        private ReorderableList stepsList;
        private int selectedStepIndex = -1;
        private Editor selectedStepEditor;

        private void OnEnable()
        {
            stepEventListener = (StepEventListener)target;
            
            // Create ReorderableList for steps
            var stepListProp = serializedObject.FindProperty("stepList");
            stepsList = new ReorderableList(serializedObject, stepListProp, true, true, true, true);
            stepsList.drawHeaderCallback = DrawHeaderCallback;
            stepsList.drawElementCallback = DrawElementCallback;
            stepsList.onAddCallback = OnAddCallback;
            stepsList.onRemoveCallback = OnRemoveCallback;
            stepsList.onSelectCallback = OnSelectCallback;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Steps");
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            var element = stepsList.serializedProperty.GetArrayElementAtIndex(index);
            var stepProp = element.FindPropertyRelative("step");
            
            // Only show the step reference field
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight), 
                stepProp, new GUIContent($"Step {index + 1}"));
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            var newElement = list.serializedProperty.GetArrayElementAtIndex(index);
            newElement.FindPropertyRelative("step").objectReferenceValue = null;
            newElement.FindPropertyRelative("onStepStarted").objectReferenceValue = null;
            newElement.FindPropertyRelative("onStepCompleted").objectReferenceValue = null;
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            if (selectedStepIndex == list.index)
            {
                selectedStepIndex = -1;
                selectedStepEditor = null;
            }
            else if (selectedStepIndex > list.index)
            {
                selectedStepIndex--;
            }
        }

        private void OnSelectCallback(ReorderableList list)
        {
            selectedStepIndex = list.index;
            var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
            var stepProp = element.FindPropertyRelative("step");
            if (stepProp.objectReferenceValue != null)
            {
                selectedStepEditor = Editor.CreateEditor(stepProp.objectReferenceValue);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "StepEventListener manages events for multiple steps. Select a step to configure its events.",
                MessageType.Info
            );

            serializedObject.Update();

            // Steps List
            stepsList.DoLayoutList();

            // Show selected step configuration
            if (selectedStepIndex >= 0 && selectedStepIndex < stepEventListener.StepList.Count)
            {
                var selectedStepWithEvents = stepEventListener.StepList[selectedStepIndex];
                if (selectedStepWithEvents.step != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"Step {selectedStepIndex + 1} Configuration", EditorStyles.boldLabel);
                    
                    // Step Events
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Step Events", EditorStyles.boldLabel);
                    
                    SerializedProperty stepListProp = serializedObject.FindProperty("stepList");
                    var selectedStepWithEventsProp = stepListProp.GetArrayElementAtIndex(selectedStepIndex);
                    
                    var onStepStartedProp = selectedStepWithEventsProp.FindPropertyRelative("onStepStarted");
                    var onStepCompletedProp = selectedStepWithEventsProp.FindPropertyRelative("onStepCompleted");
                    
                    EditorGUILayout.PropertyField(onStepStartedProp, new GUIContent("On Step Started"));
                    EditorGUILayout.PropertyField(onStepCompletedProp, new GUIContent("On Step Completed"));
                    EditorGUILayout.EndVertical();
                    
                    // Step Properties (Scriptable Object Editor)
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Step Properties", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box");
                    if (selectedStepEditor != null)
                    {
                        selectedStepEditor.OnInspectorGUI();
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
} 
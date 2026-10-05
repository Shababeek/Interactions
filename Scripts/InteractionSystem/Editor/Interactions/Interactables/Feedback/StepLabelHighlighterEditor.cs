using System.Collections.Generic;
using Shababeek.Interactions.Feedback;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(StepLabelHighlighter))]
    public class StepLabelHighlighterEditor : Editor
    {
        private const string UndoName = "Sync Labels To Steps";

        private StepLabelHighlighter _highlighter;
        private SerializedProperty _labelsProp;
        private SerializedProperty _positionModeProp;
        private SerializedProperty _writeNumbersProp;
        private SerializedProperty _firstLabelNumberProp;
        private SerializedProperty _labelRadiusProp;
        private SerializedProperty _labelOffsetProp;

        private void OnEnable()
        {
            _highlighter = (StepLabelHighlighter)target;
            _labelsProp = serializedObject.FindProperty("labels");
            _positionModeProp = serializedObject.FindProperty("positionMode");
            _writeNumbersProp = serializedObject.FindProperty("writeNumbers");
            _firstLabelNumberProp = serializedObject.FindProperty("firstLabelNumber");
            _labelRadiusProp = serializedObject.FindProperty("labelRadius");
            _labelOffsetProp = serializedObject.FindProperty("labelOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script", "positionMode", "writeNumbers",
                "firstLabelNumber", "labelRadius", "labelOffset");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sync Labels To Steps", EditorStyles.boldLabel);

            var stepper = _highlighter.Stepper;
            if (stepper == null)
            {
                EditorGUILayout.HelpBox("No Dial or Slider found. Assign one as Source, or put this on (or under) one.", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            int steps = stepper.NumberOfSteps;
            int labelCount = _labelsProp.arraySize;
            int missing = CountMissing();
            if (labelCount == steps && missing == 0)
                EditorGUILayout.HelpBox($"{labelCount} labels for {steps} steps — matched.", MessageType.Info);
            else
                EditorGUILayout.HelpBox($"{labelCount} labels ({missing} empty) for {steps} steps. Press Sync to fix.", MessageType.Warning);

            EditorGUILayout.PropertyField(_positionModeProp, new GUIContent("Label Positions",
                "Keep Current: existing labels stay where they are, only new labels are placed on their step.\n" +
                "Move To Steps: every label is moved onto its step position."));

            EditorGUILayout.PropertyField(_writeNumbersProp, new GUIContent("Write Step Numbers",
                "Overwrite each label's text with its step number. Turn off to keep custom text (e.g. Low / High)."));
            if (_writeNumbersProp.boolValue)
                EditorGUILayout.PropertyField(_firstLabelNumberProp, new GUIContent("First Number",
                    "Number on the first step's label: 1 → 1..N, 0 → 0..N-1."));

            if (stepper is DialInteractable)
                EditorGUILayout.PropertyField(_labelRadiusProp, new GUIContent("Radius", "Distance of the labels from the dial center."));
            EditorGUILayout.PropertyField(_labelOffsetProp, new GUIContent("Offset",
                "Added to every step position, in the source's local space."));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(4);
            using (new EditorGUI.DisabledScope(FirstLabel() == null))
            {
                if (GUILayout.Button(new GUIContent("Sync Labels To Steps",
                        "One label per step (copies the first label if more are needed), in step order."), GUILayout.Height(26)))
                    Sync(stepper);
            }

            if (FirstLabel() == null)
                EditorGUILayout.HelpBox("Assign at least one label — it is used as the template for the others.", MessageType.None);
        }

        private void OnSceneGUI()
        {
            var stepper = _highlighter.Stepper;
            if (stepper == null) return;

            bool moving = _positionModeProp.enumValueIndex == (int)StepLabelHighlighter.LabelPositionMode.MoveToSteps;
            float size = HandleUtility.GetHandleSize(_highlighter.transform.position) * 0.04f;

            for (int i = 0; i < stepper.NumberOfSteps; i++)
            {
                var stepPos = _highlighter.GetLabelWorldPosition(i);
                var label = i < _labelsProp.arraySize ? _labelsProp.GetArrayElementAtIndex(i).objectReferenceValue as TMP_Text : null;

                // Where the label will end up after sync: its step slot, unless it's kept in place.
                bool placedOnStep = moving || label == null;
                Handles.color = placedOnStep ? Color.cyan : new Color(0.5f, 0.5f, 0.5f, 0.6f);
                Handles.DrawWireDisc(stepPos, Camera.current.transform.forward, size);
                Handles.Label(stepPos, $" {i}");

                if (label != null && moving)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawDottedLine(label.transform.position, stepPos, 3f);
                }
            }
        }

        private void Sync(IStepInteractable stepper)
        {
            serializedObject.Update();

            var template = FirstLabel();
            bool moveAll = _positionModeProp.enumValueIndex == (int)StepLabelHighlighter.LabelPositionMode.MoveToSteps;
            bool writeNumbers = _writeNumbersProp.boolValue;
            int firstNumber = _firstLabelNumberProp.intValue;
            int steps = stepper.NumberOfSteps;

            var current = new List<TMP_Text>();
            for (int i = 0; i < _labelsProp.arraySize; i++)
                current.Add(_labelsProp.GetArrayElementAtIndex(i).objectReferenceValue as TMP_Text);

            Undo.SetCurrentGroupName(UndoName);
            int group = Undo.GetCurrentGroup();

            var synced = new TMP_Text[steps];
            for (int i = 0; i < steps; i++)
            {
                var label = i < current.Count ? current[i] : null;
                bool isNew = label == null;
                if (isNew)
                {
                    label = Instantiate(template, template.transform.parent);
                    Undo.RegisterCreatedObjectUndo(label.gameObject, UndoName);
                }

                Undo.RecordObject(label, UndoName);
                Undo.RecordObject(label.transform, UndoName);
                Undo.RecordObject(label.gameObject, UndoName);

                if (writeNumbers)
                {
                    int number = firstNumber + i;
                    label.text = number.ToString();
                    label.gameObject.name = $"StepLabel_{number}";
                }
                else if (isNew)
                {
                    label.gameObject.name = $"StepLabel_{i}";
                }

                if (moveAll || isNew)
                    label.transform.position = _highlighter.GetLabelWorldPosition(i);

                PrefabUtility.RecordPrefabInstancePropertyModifications(label);
                PrefabUtility.RecordPrefabInstancePropertyModifications(label.transform);
                synced[i] = label;
            }

            for (int i = steps; i < current.Count; i++)
                if (current[i] != null)
                    Debug.LogWarning($"[{UndoName}] '{current[i].name}' is beyond step {steps - 1} and was unassigned (left in the scene).", current[i]);

            _labelsProp.arraySize = steps;
            for (int i = 0; i < steps; i++)
                _labelsProp.GetArrayElementAtIndex(i).objectReferenceValue = synced[i];
            serializedObject.ApplyModifiedProperties();

            Undo.CollapseUndoOperations(group);
        }

        private TMP_Text FirstLabel()
        {
            for (int i = 0; i < _labelsProp.arraySize; i++)
                if (_labelsProp.GetArrayElementAtIndex(i).objectReferenceValue is TMP_Text t) return t;
            return null;
        }

        private int CountMissing()
        {
            int missing = 0;
            for (int i = 0; i < _labelsProp.arraySize; i++)
                if (_labelsProp.GetArrayElementAtIndex(i).objectReferenceValue == null) missing++;
            return missing;
        }
    }
}
